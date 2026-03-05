using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.UI;

// Day2 상호작용 전체를 관리하는 스크립트
public class Day2Controller : MonoBehaviour
{
    [Header("플레이어 참조")]
    [SerializeField] private Transform player;

    [Header("상호작용 오브젝트 (순서대로)")]
    [SerializeField] private GameObject[] interactObjects; // 0: 창문, 1: 책상, 2: 가족사진, 3: 문
    [SerializeField] DialogueScriptable[] hintDialogues, interactionDialogues;
    [SerializeField] private int[] originalValues;
    [SerializeField] private float interactDistance = 3f;

    [Header("조명 / 페이드 / 연출 관련")]
    [SerializeField] private Light directionalLight; // 낮밤 전환용
    [SerializeField] private CanvasGroup fadePanel; // 흰색 페이드 아웃용
    [SerializeField] AudioSource soundNodeSource;
    [SerializeField] AudioSource playSource;
    [SerializeField] CinemachineCamera potCamera;
    [SerializeField] Material[] flowerMaterials;
    [SerializeField] Renderer flowerRenderer;
    [SerializeField] Image keyGauge;
    [SerializeField] Button leftArrowButton, rightArrowButton;

    private Quaternion[] initialRotations;
    private int currentStep = 0;
    private bool isActivated = false; // SaveNode에서 활성화된 후만 작동
    private bool hasWindowTransitioned = false; // 창문 트랜지션 중복 방지
    bool interacting;
    int targetValue = 5;
    float currentValue;
    float minValue, maxValue;

    private Color defaultLightColor; // 낮 상태 복원용 색상
    private float defaultLightIntensity; // 낮 상태 복원용 밝기

    public bool readyInteraction;

    // SaveNode에서 호출될 때 꺼둔 오브젝트를 켜줌
    public void ActivateObjectsFromNode()
    {
        isActivated = true;
        StartCoroutine(StartPuzzle());
    }

    IEnumerator StartPuzzle()
    {
        player.GetComponent<PlayerController_Dream>().enabled = false;
        for (int i = 0; i < interactObjects.Length; i++)
        {
            if (interactObjects[i] != null)
            {
                interactObjects[i].SetActive(true);
                Transform t = interactObjects[i].transform;
                t.DOMoveY(t.position.y, 0.5f).From(t.position.y + 0.5f);
                interactObjects[i].GetComponent<Day2DialogueTrigger>().dialogueData = i == 0 ? interactionDialogues[i] :hintDialogues[i];
                yield return new WaitForSeconds(0.1f);
            }
        }
        potCamera.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        potCamera.gameObject.SetActive(false);
        player.GetComponent<PlayerController_Dream>().enabled = true;
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

        soundNodeSource.transform.position = interactObjects[currentStep].transform.position;
        keyGauge.transform.parent.gameObject.SetActive(false);
        leftArrowButton.onClick.AddListener(() => OnClickArrowButton(-1));
        rightArrowButton.onClick.AddListener(() => OnClickArrowButton(1));
    }

    void Update()
    {
        if (!isActivated)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentStep == interactObjects.Length - 1) TryInteract();
            else if(readyInteraction) TryInteract();
            return;
        }

        if (interacting)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                // 피치 다운
                if (currentValue == minValue) return;
                OnClickArrowButton(-1);
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                // 피치 업
                if (currentValue == maxValue) return;
                OnClickArrowButton(1);
            }
        }
    }

    void OnClickArrowButton(int value)
    {
        Image img = value == -1 ? leftArrowButton.image : rightArrowButton.image;
        img.DOKill();
        img.DOFade(0, 0.5f).From(1);

        currentValue += value;
        playSource.pitch = currentValue * 0.2f;
        keyGauge.fillAmount = 1 - ((maxValue - currentValue) / 10f);
        if (currentValue == targetValue)
        {
            // clear
            keyGauge.transform.parent.gameObject.SetActive(false);
            interactObjects[currentStep].GetComponent<Collider>().enabled = false;
            currentStep++;
            StartCoroutine(FinishInteraction());
        }
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
        readyInteraction = false;
        if (index == currentStep)
        {
            if(index == interactObjects.Length - 1)
            {
                Debug.Log("puzzle day2 clear");
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
                Debug.Log("start interaction : " + interactObjects[index].name);
                interacting = true;
                player.GetComponent<PlayerController_Dream>().enabled = false;
                soundNodeSource.GetComponent<SoundNode>().enabled = false;
                soundNodeSource.Stop();
                currentValue = originalValues[currentStep];
                playSource.pitch = currentValue * 0.2f;
                playSource.Play();
                playSource.loop = true;
                minValue = originalValues[currentStep] - 5;
                maxValue = originalValues[currentStep] + 5;
                keyGauge.fillAmount = 1 - ((maxValue - currentValue) / 10f);
                keyGauge.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            currentStep = 0;
            for (int i = 0; i < interactObjects.Length - 1; i++) interactObjects[i].GetComponent<Collider>().enabled = true;
            StartCoroutine(FinishInteraction());
            Debug.Log("상호작용 턴 초기화");
        }
    }

    IEnumerator FinishInteraction()
    {
        flowerRenderer.material = flowerMaterials[currentStep];
        potCamera.gameObject.SetActive(true);
        soundNodeSource.transform.position = interactObjects[currentStep].transform.position;
        for(int i = 0; i < interactObjects.Length; i++)
        {
            interactObjects[i].GetComponent<Day2DialogueTrigger>().dialogueData = i == currentStep ? interactionDialogues[i] : hintDialogues[i];
        }
        yield return new WaitForSeconds(2f);
        potCamera.gameObject.SetActive(false);
        ResetInteraction();
    }

    void ResetInteraction()
    {
        interacting = false;
        player.GetComponent<PlayerController_Dream>().enabled = true;
        soundNodeSource.GetComponent<SoundNode>().enabled = true;
        playSource.Stop();
        Debug.Log("stop interaction");
    }
}
