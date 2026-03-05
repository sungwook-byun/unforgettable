using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FootprintSpawner_forSound : MonoBehaviour
{
    [Header("참조")]
    public PathSystem pathSystem;
    public AudioMixer gameMixer; 
    private PlayerController_Dream playerController;

    [Header("빛 (발자국)")]
    public GameObject lightPrefab; 
    public float stepDistance = 0.5f; 
    public float lightLife = 2f; 
    public int poolSize = 15; 

    private List<GameObject> pool = new List<GameObject>();
    private Vector3 lastPos;
    private int poolIndex;
    
    private Color currentColor = Color.clear;
    
    // --- 1. 기본값 설정 ---
    [Header("사운드 기본값")]
    private float defaultPitch = 1f;
    private float defaultLowpass = 22000f;
    private float defaultDistortion = 0f;    // Distortion 기본값 (꺼짐)
    private float defaultFlangerWet = 0f;    // Flanger 기본값 (꺼짐)

    // --- 2. 현재값 변수 ---
    private float currentPitch = 1f;
    private float currentLowpass = 22000f;
    private float currentDistortion = 0f;
    private float currentFlangerWet = 0f;
    
    [Header("사운드 설정")]
    [SerializeField] private float soundChangeSpeed = 2.0f;

    void Awake()
    {
        playerController = GetComponent<PlayerController_Dream>(); 
        
        for (int i = 0; i < poolSize; i++) { GameObject obj = Instantiate(lightPrefab); obj.SetActive(false); pool.Add(obj); }
        lastPos = transform.position;
        
        // 믹서 값 초기화
        InitializeMixer();
    }
    
    // 믹서 초기화 함수
    private void InitializeMixer()
    {
        currentPitch = defaultPitch; 
        currentLowpass = defaultLowpass;
        currentDistortion = defaultDistortion;
        currentFlangerWet = defaultFlangerWet;
        
        if (gameMixer != null) 
        { 
            gameMixer.SetFloat("BGM_Pitch", currentPitch); 
            gameMixer.SetFloat("BGM_Lowpass", currentLowpass);
            gameMixer.SetFloat("BGM_Distortion", currentDistortion);
            gameMixer.SetFloat("BGM_FlangerWet", currentFlangerWet);
        }
    }

    void Update()
    {
        if (pathSystem != null)
            currentColor = pathSystem.GetConditionalColor(transform.position);
        else
            currentColor = Color.clear;
        
        float dist = Vector3.Distance(transform.position, lastPos);
        if (dist >= stepDistance)
        {
            SpawnLight(transform.position, currentColor); 
            lastPos = transform.position;
        }

        UpdateSoundDistortion();
    }
    
    void FixedUpdate()
    {
        UpdateInputPenalties();
    }

    // 소리 왜곡 (Update에서 호출됨)
    private void UpdateSoundDistortion()
    {
        if (gameMixer == null || pathSystem == null) return;
        
        float targetPitch = defaultPitch;
        float targetLowpass = defaultLowpass;
        float targetDistortion = defaultDistortion;
        float targetFlangerWet = defaultFlangerWet;

        if (currentColor == pathSystem.nearColor) // 파란색 (안전)
        {
            // 모든 값을 기본값으로
            targetPitch = defaultPitch;
            targetLowpass = defaultLowpass;
            targetDistortion = defaultDistortion;
            targetFlangerWet = defaultFlangerWet;
        }
        else if (currentColor == pathSystem.midColor) // 노란색 (경고 1단계)
        {
            // 기존: 먹먹하게만
            targetPitch = defaultPitch;
            targetLowpass = 1500f; 
            targetDistortion = defaultDistortion;
            targetFlangerWet = defaultFlangerWet;
        }
        else // 빨간색 (위험 2단계) 또는 범위 밖
        {
            // --- 3. [!!!] 기괴한 왜곡 적용 [!!!] ---
            targetPitch = 0.7f;         // (더 낮춤) 악마의 목소리처럼
            targetLowpass = 1000f;      // (더 낮춤) 물속에 잠긴 것처럼
            targetDistortion = 0.6f;    // (신규) 스피커가 찢어진 듯한 노이즈
            targetFlangerWet = 80f;     // (신규) 금속성 울렁임 (0~100)
        }
        
        // 부드럽게 값 변경 (4개 모두)
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * soundChangeSpeed);
        currentLowpass = Mathf.Lerp(currentLowpass, targetLowpass, Time.deltaTime * soundChangeSpeed);
        currentDistortion = Mathf.Lerp(currentDistortion, targetDistortion, Time.deltaTime * soundChangeSpeed);
        currentFlangerWet = Mathf.Lerp(currentFlangerWet, targetFlangerWet, Time.deltaTime * soundChangeSpeed);

        // 믹서에 최종 값 적용 (4개 모두)
        gameMixer.SetFloat("BGM_Pitch", currentPitch);
        gameMixer.SetFloat("BGM_Lowpass", currentLowpass);
        gameMixer.SetFloat("BGM_Distortion", currentDistortion);
        gameMixer.SetFloat("BGM_FlangerWet", currentFlangerWet);
    }

    // 입력 패널티 (FixedUpdate에서 호출됨)
    private void UpdateInputPenalties()
    {
        if (pathSystem == null || playerController == null) return;
        
        bool shouldReverse = false;
        bool shouldWobble = false; 

        if (currentColor == pathSystem.nearColor) 
        {
            shouldReverse = false;
            shouldWobble = false;
        }
        else if (currentColor == pathSystem.midColor)
        {
            shouldWobble = true; 
            shouldReverse = false;
        }
        else 
        {
            shouldReverse = true; 
            shouldWobble = false;
        }

        playerController.SetInputReversal(shouldReverse);
        playerController.SetWobble(shouldWobble);
    }
    
    // (SpawnLight, DisableAfterTime 함수는 기존과 동일)
    private void SpawnLight(Vector3 position, Color pathColor) 
    {
        GameObject obj = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Count;
        position.y = 0.1f; 
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
        Light light = obj.GetComponent<Light>();
        if (light != null) 
        {
            light.color = (pathColor == Color.clear) ? Color.white : pathColor;
            light.range = 2f;
            light.intensity = 3f;
        }
        StartCoroutine(DisableAfterTime(obj));
    }

    private IEnumerator DisableAfterTime(GameObject obj) 
    {
        yield return new WaitForSeconds(lightLife);
        obj.SetActive(false);
    }

    // 씬이 꺼질 때 믹서 소리를 원상복구
    private void OnDisable() 
    {
        // 4. OnDisable에서도 모든 값을 초기화
        InitializeMixer(); 
    }
}