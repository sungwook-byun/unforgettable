using UnityEngine;
using System.Collections;

// Day1 상호작용 전체를 관리하는 스크립트
public class Day1Controller : MonoBehaviour
{
    [Header("플레이어 참조")]
    [SerializeField] private Transform player;

    [Header("상호작용 오브젝트 (순서대로)")]
    [SerializeField] private GameObject[] interactObjects; // 0: 창문, 1: 책상, 2: 가족사진, 3: 문
    [SerializeField] private Vector3[] targetRotations; // 회전값
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private DialogueScriptable[] interactionDialogues; // 추가

    [Header("조명 / 페이드 / 연출 관련")]
    [SerializeField] private Light directionalLight; // 낮밤 전환용
    [SerializeField] private CanvasGroup fadePanel; // 흰색 페이드 아웃용

    private Quaternion[] initialRotations;
    private int currentStep = 0;
    private bool isActivated = false; // SaveNode에서 활성화된 후만 작동
    private bool hasWindowTransitioned = false; // 창문 트랜지션 중복 방지

    private Color defaultLightColor; // 낮 상태 복원용 색상
    private float defaultLightIntensity; // 낮 상태 복원용 밝기

    public bool readyInteraction = true; // 추가

    // SaveNode에서 호출될 때 꺼둔 오브젝트를 켜줌
    public void ActivateObjectsFromNode()
    {
        isActivated = true;
        for (int i = 0; i < interactObjects.Length; i++)
        {
            if (interactObjects[i] != null)
                interactObjects[i].SetActive(true);
        }
    }

    void Start()
    {
        // 오브젝트들의 초기 회전값 저장
        initialRotations = new Quaternion[interactObjects.Length];
        for (int i = 0; i < interactObjects.Length; i++)
        {
            if (interactObjects[i] != null)
                initialRotations[i] = interactObjects[i].transform.rotation; // 월드 기준 회전 저장
        }

        // 조명 초기 상태 저장
        if (directionalLight != null)
        {
            defaultLightColor = directionalLight.color;
            defaultLightIntensity = directionalLight.intensity;
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isActivated || !readyInteraction)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }

    // 플레이어 주변의 상호작용 가능한 오브젝트를 탐색하고 상호작용 처리
    private void TryInteract()
    {
        for (int i = 0; i < interactObjects.Length; i++)
        {
            if (!interactObjects[i] || !interactObjects[i].activeSelf)
                continue;

            float distance = Vector3.Distance(player.position, interactObjects[i].transform.position);
            if (distance <= interactDistance)
            {
                HandleInteraction(i);
                break;
            }
        }
    }

    private void HandleInteraction(int index)
    {
        var system = FindFirstObjectByType<DialogueSystem>();
        readyInteraction = false;

        if (index == 0 && currentStep == 0 && !hasWindowTransitioned)
        {
            hasWindowTransitioned = true;
            interactObjects[index].transform.rotation = Quaternion.Euler(targetRotations[index]);
            if (interactionDialogues.Length > index && interactionDialogues[index] != null)
                system.StartDialogue(interactionDialogues[index], OnDialogueEnd);
            StartCoroutine(WindowTransition());
            currentStep++;
            return;
        }

        if (index == currentStep && index < interactObjects.Length - 1)
        {
            interactObjects[index].transform.rotation = Quaternion.Euler(targetRotations[index]);
            if (interactionDialogues.Length > index && interactionDialogues[index] != null)
                system.StartDialogue(interactionDialogues[index], OnDialogueEnd);
            currentStep++;
            return;
        }

        if (index == interactObjects.Length - 1)
        {
            if (currentStep == interactObjects.Length - 1)
            {
                if (interactionDialogues.Length > index && interactionDialogues[index] != null)
                    system.StartDialogue(interactionDialogues[index], OnDialogueEnd);

                var data = SaveManager.Instance.GetCurrentData();

                if (player != null)
                {
                    Vector3 playerPos = player.position;
                    playerPos.y = 1f;
                    data.lastMemoryWorldPosition = playerPos;
                }

                data.sceneName = SceneNames.TestMemoryWorld;
                data.isDiaryMode = true;

                SaveManager.Instance.SetCurrentData(data);
                SaveManager.Instance.AutoSave(true);

                GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.TestGrandmaRoom);
            }
            else
            {
                StartCoroutine(ReturnToLastActivatedNode());
            }
            return;
        }

        ResetAllObjects();
        hasWindowTransitioned = false;
        readyInteraction = true;
    }

    private IEnumerator WindowTransition()
    {
        if (directionalLight != null)
        {
            Color startColor = directionalLight.color;
            Color endColor = new Color(0.1f, 0.1f, 0.25f); // 훨씬 어둡고 푸른 밤색
            float startIntensity = directionalLight.intensity;
            float endIntensity = 0.05f; // 거의 달빛 수준 밝기

            float duration = 2f; // 천천히 변하게 하면 더 자연스러움
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                directionalLight.color = Color.Lerp(startColor, endColor, t);
                directionalLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);

                yield return null;
            }
        }
    }

    private IEnumerator ReturnToLastActivatedNode()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0f, 1f, 0.6f));
        }

        var data = SaveManager.Instance.GetCurrentData();
        if (data != null)
        {
            Vector3 returnPos = data.lastMemoryWorldPosition;
            returnPos.y = 1f;
            player.position = returnPos;
        }

        yield return new WaitForSeconds(0.3f);

        if (fadePanel != null)
        {
            yield return StartCoroutine(Fade(1f, 0f, 0.6f));
            fadePanel.gameObject.SetActive(false);
        }

        ResetAllObjects();
        hasWindowTransitioned = false;
        readyInteraction = true;
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadePanel.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
    }

    private void ResetAllObjects()
    {
        for (int i = 0; i < interactObjects.Length; i++)
        {
            if (interactObjects[i] != null)
                interactObjects[i].transform.rotation = initialRotations[i]; // 월드 기준 회전 복원
        }

        if (directionalLight != null)
        {
            directionalLight.color = defaultLightColor;
            directionalLight.intensity = defaultLightIntensity;
        }

        currentStep = 0;

        // 플레이어 제어 복구
        var playerController = player.GetComponent<PlayerController_Dream>();
        if (playerController != null)
            playerController.enabled = true;
    }

    private void OnDialogueEnd()
    {
        StartCoroutine(WaitForDialogueEndThenEnablePlayer());
    }

    private IEnumerator WaitForDialogueEndThenEnablePlayer()
    {
        var system = FindFirstObjectByType<DialogueSystem>();
        if (system != null)
        {
            while (system.IsDialogueActive)
                yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        readyInteraction = true;
        var playerController = player.GetComponent<PlayerController_Dream>();
        if (playerController != null)
        {
            playerController.enabled = true;
            var anim = playerController.GetComponentInChildren<Animator>();
            if (anim != null)
                anim.SetBool("IsMoving", false);
        }
    }
}
