using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DiaryManager : MonoBehaviour
{
    // 꿈속 세계
    [SerializeField] private Light directionalLight;

    [Header("다이어리 활성화 설정")]
    [SerializeField] private GameObject diaryPanel;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform player;
    [SerializeField] private Transform diaryObject;

    [Header("읽기 설정")]
    [SerializeField] private GameObject readPanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private TMP_Text[] readTexts;
    private int currentPage = 0; // 현재 페이지 인덱스

    [Header("쓰기 설정")]
    [SerializeField] private GameObject writePanel;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private GameObject buttonPrefab;

    [SerializeField] private TMP_Text selectedText;

    private string[] emotions = new string[]
    {
        "따뜻함", "그리움", "사랑", "안도감", "슬픔", "감사", "외로움", "행복", "회상", "평온",
        "즐거움", "아쉬움", "감동", "포근함", "향수", "위로", "순수함", "고요함", "소중함", "애틋함",
        "자부심", "설렘", "눈물", "온기", "다정함", "따사로움", "미소", "애정", "추억", "평화",
        "정겨움", "진심", "담담함", "잔잔함", "여운", "그윽함", "소망", "자애로움", "감사함", "다감함"
    };
    private List<string> selectedEmotions = new List<string>();

    void Start()
    {
        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
        StartCoroutine(InitDiaryAfterDelay());
    }

    // 추가 - GrandmaEventManager2가 밤으로 설정을 완료한 뒤 다이어리 상태를 불러오기
    private IEnumerator InitDiaryAfterDelay()
    {
        yield return new WaitForSeconds(1.2f); // 밤 설정이 완료될 시간을 확보
        var data = SaveManager.Instance.GetCurrentData();
        if (data != null)
        {
            DiaryOpen();
        }
    }

    void Update()
    {
        if (player == null || diaryPanel == null)
            return;

        float distance = Vector3.Distance(player.position, diaryObject.position);

        if (distance <= interactionDistance && Input.GetKeyDown(KeyCode.E))
        {
            diaryPanel.SetActive(true);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject == diaryObject.gameObject && distance <= interactionDistance)
                {
                    diaryPanel.SetActive(true);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            diaryPanel.SetActive(false);
        }
    }

    private void DiaryOpen()
    {
        SaveData data = SaveManager.Instance.GetCurrentData();

        if (data.isDiaryMode)
        {
            // 밤이면 쓰기모드
            directionalLight.color = Color.gray;
            readPanel.SetActive(false);
            writePanel.SetActive(true);
            DiaryWrite();
            Debug.Log($"{data.currentDay}일차 밤 상태이므로 쓰기모드 시작");
        }
        else
        {
            // 낮이면 읽기모드 (전날 감정 보기)
            directionalLight.color = new Color32(255, 244, 214, 255);
            readPanel.SetActive(true);
            writePanel.SetActive(false);
            currentPage = 0;
            DiaryRead();
            Debug.Log($"낮 상태이므로 {data.currentDay - 1}일차 읽기모드 시작");
        }
    }

    public void DiaryRead()
    {
        SaveData data = SaveManager.Instance.GetCurrentData();
        if (data.diaryEntries == null || data.diaryEntries.Count == 0)
        {
            readTexts[0].text = "저장된 일기가 없습니다.";
            readTexts[1].text = "";
            return;
        }

        data.diaryEntries.Sort((a, b) => a.day.CompareTo(b.day));

        int startIndex = currentPage * 2;
        readTexts[0].text = "";
        readTexts[1].text = "";

        if (startIndex < data.diaryEntries.Count)
        {
            var left = data.diaryEntries[startIndex];
            readTexts[0].text = $"{left.day}일차\n{left.emotion}";
        }
        if (startIndex + 1 < data.diaryEntries.Count)
        {
            var right = data.diaryEntries[startIndex + 1];
            readTexts[1].text = $"{right.day}일차\n{right.emotion}";
        }

        prevButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive(startIndex + 2 < data.diaryEntries.Count);
    }

    public void NextPage()
    {
        SaveData data = SaveManager.Instance.GetCurrentData();
        int maxPage = Mathf.CeilToInt(data.diaryEntries.Count / 2f) - 1;
        if (currentPage < maxPage)
        {
            currentPage++;
            DiaryRead();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            DiaryRead();
        }
    }

    public void DiaryWrite()
    {
        foreach (Transform child in buttonParent)
            Destroy(child.gameObject);

        foreach (var emotion in emotions)
        {
            var btnObj = Instantiate(buttonPrefab, buttonParent);
            var text = btnObj.GetComponentInChildren<TMP_Text>();
            text.text = emotion;

            var button = btnObj.GetComponent<Button>();
            var colors = button.colors;
            button.colors = colors;

            button.onClick.AddListener(() => OnEmotionClicked(emotion, button));
        }
        UpdateSelectedText();
    }

    private void OnEmotionClicked(string emotion, Button button)
    {
        if (selectedEmotions.Contains(emotion))
        {
            selectedEmotions.Remove(emotion);
            button.image.color = Color.white;
        }
        else
        {
            if (selectedEmotions.Count >= 5) return;
            selectedEmotions.Add(emotion);
            button.image.color = Color.gray;
        }
        UpdateSelectedText();
    }

    private void UpdateSelectedText()
    {
        if (selectedEmotions.Count == 0)
            selectedText.text = "할머니의 기억을 보며 오늘 느낀 마음을 일기에 담아보세요";
        else
            selectedText.text = string.Join(", ", selectedEmotions);
    }

    public void OnSave()
    {
        var saveMgr = SaveManager.Instance;
        SaveData data = saveMgr.Load() ?? new SaveData();

        int day = data.currentDay;
        string emotionsToSave = string.Join(", ", selectedEmotions);

        var existing = data.diaryEntries.Find(e => e.day == day);
        if (existing != null)
            existing.emotion = emotionsToSave;
        else
            data.diaryEntries.Add(new DiaryEntry { day = day, emotion = emotionsToSave });

        data.diaryEntries.Sort((a, b) => a.day.CompareTo(b.day));

        saveMgr.SaveData(data);
        saveMgr.SetCurrentData(data);

        Debug.Log($"[{day}일차] 감정 저장 완료: {emotionsToSave}");

        writePanel.SetActive(false);
        readPanel.SetActive(true);
        DiaryRead();

        StartCoroutine(NextDay());
    }

    private IEnumerator NextDay()
    {
        SaveData data = SaveManager.Instance.GetCurrentData();

        // 밤 상태로 시작하되 5초 뒤 낮으로 전환 예정
        Debug.Log("낮 전환 예약됨 (5초 뒤 적용)");

        yield return new WaitForSeconds(5f);

        diaryPanel.SetActive(false);
        yield return new WaitForSeconds(1f);

        data.currentDay += 1;
        data.isDiaryMode = false;
        SaveManager.Instance.SaveData(data);

        directionalLight.color = new Color32(255, 244, 214, 255);
        Debug.Log("낮 전환 완료 후 미션 재개 시도");

        // GrandmaEventManager2 다시 찾아서 낮 미션 재개
        GrandmaEventManager2 grandmaEventMgr = FindFirstObjectByType<GrandmaEventManager2>(FindObjectsInactive.Include);
        if (grandmaEventMgr != null)
        {
            grandmaEventMgr.StartNextDayMission();
            Debug.Log("낮 전환 후 GrandmaEventManager2 미션 재개 호출 완료");
        }
        else
        {
            Debug.LogWarning("낮 전환 후 GrandmaEventManager2를 찾지 못했습니다 (씬 로드 시점 확인 필요)");
        }
    }
}
