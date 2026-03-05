using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunnerGame : MiniGameBase
{
    [System.Serializable]
    public struct BackgroundLayer
    {
        public Renderer renderer;
        public float speedMultiplier;
        [HideInInspector] public float offsetX;
    }

    [Header("Background")]
    [SerializeField] BackgroundLayer[] bgLayers;
    [SerializeField] Sprite[] groundSprites;
    [SerializeField] Renderer groundRenderer;
    int groundIndex;
    [SerializeField] RunnerScroll[] cloudScrolls;

    [Header("Player")]
    [SerializeField] Rigidbody2D playerRb;
    [SerializeField] Animator playerAnim;
    [SerializeField] LayerMask groundMask;
    Vector2 groundCheckOffset = new Vector2(0f, -.5f);
    float groundCheckRadius = 0.1f;
    float jumpForce = 12f;
    [SerializeField] AudioClip jumpSfx;

    [Header("Scroll / World")]
    [SerializeField] float scrollSpeed = 5f;
    [SerializeField] float killX = -10f;     // 이 x보다 왼쪽이면 풀로 복귀
    float speedUpTimer = 0f;
    float speedUpInterval = 10f;
    float speedIncrement = 2f;

    [Header("Spawning")]
    [SerializeField] GameObject obstaclePrefab;
    Vector2[] spawnIntervalPerLevel = { new Vector2(3f, 4f), new Vector2(2f, 3f), new Vector2(1f, 2f) };
    Vector2 spawnInterval;
    float spawnTimer, spawnTime;

    float distanceScoreAcc = 0f;

    readonly List<RunnerScroll> obstaclePool = new List<RunnerScroll>();

    bool grounded;
    bool playing;

    [SerializeField] RectTransform mark;
    [SerializeField] Image distanceBar;
    [SerializeField] Gradient distanceGradient;
    float maxTime = 40f;
    float runTime = 0;

    public override void Begin()
    {
        cloudScrolls[0].Init(this);
        cloudScrolls[1].Init(this);
        playing = true;
        spawnTimer = 0f;
        spawnInterval = spawnIntervalPerLevel[MiniGameContext.gameLevel];
        playerAnim.SetTrigger("Walk");

        // 배경 offset 초기화
        for (int i = 0; i < bgLayers.Length; i++)
        {
            bgLayers[i].offsetX = 0f;
            ApplyBGOffset(i);
        }
    }

    void Update()
    {
        if (!playing) return;
        if (timer.Paused) return;

        float dt = Time.deltaTime;
        runTime += dt;
        distanceBar.fillAmount = runTime / maxTime;
        distanceBar.color = distanceGradient.Evaluate(distanceBar.fillAmount);
        mark.anchoredPosition = new Vector2(Mathf.Lerp(55f, 1010f, Mathf.Clamp01(runTime / maxTime)),mark.anchoredPosition.y);

        // 플레이어 점프 처리
        UpdatePlayer();

        // 배경 패럴럭스 스크롤
        ScrollBackground(dt);

        // 코인/장애물 스폰
        spawnTimer += dt;
        if (spawnTimer >= spawnTime)
        {
            spawnTimer = 0f;
            spawnTime = Random.Range(spawnInterval.x, spawnInterval.y);
            SpawnObstacle();
        }

        // 기본 점수 업데이트
        distanceScoreAcc += dt * 10f; // 초당 10점 기준

        int gain = Mathf.FloorToInt(distanceScoreAcc);
        if (gain > 0)
        {
            distanceScoreAcc -= gain;
            score.Add(gain, false);
        }

        if (runTime >= maxTime)
        {
            // clear
            clear = true;
            playing = false;
            playerAnim.SetTrigger("Idle");
            Finish();
        }

        speedUpTimer += dt;
        SpeedUp();
    }

    void SpeedUp()
    {
        // 스피드업

        if (speedUpTimer >= speedUpInterval)
        {
            speedUpTimer = 0f;
            scrollSpeed += speedIncrement;
            groundIndex++;
            var mpb = new MaterialPropertyBlock();
            groundRenderer.GetPropertyBlock(mpb);
            var tex = groundSprites[groundIndex].texture;
            mpb.SetTexture("_BaseMap", tex);
            groundRenderer.SetPropertyBlock(mpb);
        }
    }

    void UpdatePlayer()
    {
        // 바닥 체크
        Vector2 checkPos = (Vector2)playerRb.transform.position + groundCheckOffset;
        grounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundMask);

        // 점프 입력
        if (grounded && input.JumpDown)
        {
            playerAnim.SetTrigger("Jump");
            Vector2 v = playerRb.linearVelocity;
            v.y = jumpForce;
            playerRb.linearVelocity = v;
            AudioManager.Instance.PlaySFX(jumpSfx);
        }
    }

    void ScrollBackground(float dt)
    {
        for (int i = 0; i < bgLayers.Length; i++)
        {
            bgLayers[i].offsetX += scrollSpeed * bgLayers[i].speedMultiplier * dt;

            // float 너무 커지는 거 방지
            if (bgLayers[i].offsetX > 10000f)
                bgLayers[i].offsetX -= 10000f;

            ApplyBGOffset(i);
        }
    }

    void ApplyBGOffset(int idx)
    {
        var layer = bgLayers[idx];
        var mat = layer.renderer.material;
        var off = mat.mainTextureOffset;
        off.x = layer.offsetX;
        mat.mainTextureOffset = off;
    }

    void SpawnObstacle()
    {
        RunnerScroll sc = GetFromPool();
        sc.Init(this);
    }

    RunnerScroll GetFromPool()
    {
        List<RunnerScroll> pool = obstaclePool;
        RunnerScroll sc;

        int last = pool.Count - 1;
        if (last >= 0)
        {
            sc = pool[last];
            pool.RemoveAt(last);
        }
        else
        {
            GameObject prefab = obstaclePrefab;
            GameObject go = Object.Instantiate(prefab);
            sc = go.GetComponent<RunnerScroll>();
        }

        sc.gameObject.SetActive(true);
        return sc;
    }

    public void Despawn(RunnerScroll sc)
    {
        sc.gameObject.SetActive(false);

        obstaclePool.Add(sc);
    }

    public void OnPlayerHitObstacle()
    {
        if (!playing) return;
        playing = false;
        playerAnim.SetTrigger("Stun");

        // 게임 종료
        Finish();
    }

    public void AddScore()
    {
        score.Add(50, false, false);
    }

    public bool IsActive() => playing && !timer.Paused;
    public float GetScrollSpeed() => scrollSpeed;
    public float GetKillX() => killX;
    public float GetPlayerX() => playerRb.transform.position.x;
}
