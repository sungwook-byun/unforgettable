using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class MazeGame : MiniGameBase
{
    [Header("Maze")]
    [SerializeField] Camera mazeCam;
    [SerializeField] MazeCell mazeCellPrefab;
    [SerializeField] int mazeWidth, mazeDepth;
    MazeCell[,] mazeGrid;
    List<Transform> mazeList = new List<Transform>();
    int[] mazeWidthPerLevel = { 10, 15, 25 };
    int[] mazeDepthPerLevel = { 5, 10, 15 };

    [Header("Camera")]
    [SerializeField] Transform centerTR;
    [SerializeField] CinemachineCamera finishCamera, gameCamera;
    Vector3[] targetPerLevel = { new Vector3(4.5f, 0, 2.5f), new Vector3(7, 0, 5.5f), new Vector3(12, 0, 8.5f) };
    float[] sizePerLevel = { 4.5f, 7, 10 };

    [Header("Player")]
    [SerializeField] Transform player;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float turnSpeed = 12f;
    Rigidbody rigid;                
    Vector3 inputDir;
    Animator anim;
    float autoWalkSpeed = 1f;  
    float postExitOffsetZ = .5f;   
    bool autoWalking = false;
    Vector3 autoTarget;

    [Header("Finish")]
    [SerializeField] ParticleSystem finishParticle;
    [SerializeField] Transform finish;
    [SerializeField] float finishReachRadius = 0.5f;  // 플레이어가 이 반경 안에 들어오면 클리어
    bool reachedFinish = false;
    bool playing;

    [Header("Item")]
    [SerializeField] GameObject[] items;
    int totalItemCount;     // 이번 라운드 필요한 아이템 수
    int collectedItemCount; // 플레이어가 먹은 아이템 수
    bool tweening;

    [SerializeField] ParticleSystem bubbleParticle;
    [SerializeField] AudioClip bubbleClip;

    public override void Setup(Score s, TimerController t, InputReader i)
    {
        base.Setup(s, t, i);

        // 미로 초기화
        mazeWidth = mazeWidthPerLevel[MiniGameContext.gameLevel];
        mazeDepth = mazeDepthPerLevel[MiniGameContext.gameLevel];
        mazeGrid = new MazeCell[mazeWidth, mazeDepth];
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int z = 0; z < mazeDepth; z++)
            {
                mazeGrid[x, z] = Instantiate(mazeCellPrefab, new Vector3(x, 0, z), Quaternion.identity, transform);
                if ((x == 0 && z == 0) || (x == mazeWidth - 1 && z == mazeDepth - 1)) continue;
                mazeList.Add(mazeGrid[x, z].transform);
            }
        }

        GenerateMazeIterative(mazeGrid[0, 0]);
        AddExtraConnections(loopsToAdd: (mazeWidth * mazeDepth) / 10);

        // 플레이어 초기화
        rigid = player.GetComponent<Rigidbody>();
        rigid.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionY;
        rigid.useGravity = false;
        anim = player.GetComponentInChildren<Animator>();
        player.position = mazeGrid[0, 0].transform.position;
        if (!mazeGrid[0,0].HasRightWall()) player.rotation = Quaternion.Euler(0, 90f, 0);
        player.gameObject.SetActive(true);

        // 피니시 초기화
        finish.position = mazeGrid[mazeWidth - 1, mazeDepth - 1].transform.position;
        finish.gameObject.SetActive(true);

        // 카메라 초기화
        //mazeCam.orthographicSize = cameraSizePerLevel[MiniGameContext.gameLevel];
        //mazeCam.transform.position = cameraPosPerLevel[MiniGameContext.gameLevel];
        Vector3 prev = centerTR.position;
        centerTR.position = targetPerLevel[MiniGameContext.gameLevel];
        gameCamera.Lens.OrthographicSize = sizePerLevel[MiniGameContext.gameLevel];
        gameCamera.OnTargetObjectWarped(centerTR, centerTR.position-prev);

        // 아이템 초기화
        SetItems();
    }

    #region Maze
    void GenerateMazeIterative(MazeCell startCell)
    {
        Stack<MazeCell> stack = new Stack<MazeCell>();
        stack.Push(startCell);

        while (stack.Count > 0)
        {
            MazeCell cur = stack.Peek();

            // 아직 방문 안 했으면 방문 처리
            if (!cur.IsVisited)
            {
                cur.Visit();
            }

            // 현재 셀의 미방문 이웃을 모두 모은다
            List<MazeCell> neighbors = GetUnvisitedNeighbors(cur);

            if (neighbors.Count > 0)
            {
                // 미방문 이웃 중 하나를 랜덤 선택
                int idx = Random.Range(0, neighbors.Count);
                MazeCell next = neighbors[idx];

                // cur와 next 사이 벽 제거
                ClearWalls(cur, next);

                // 더 깊이 들어가기 위해 push
                stack.Push(next);
            }
            else
            {
                // 더 갈 곳 없으면 백트래킹
                stack.Pop();
            }
        }
    }

    List<MazeCell> GetUnvisitedNeighbors(MazeCell curCell)
    {
        List<MazeCell> list = new List<MazeCell>(4);

        int x = (int)curCell.transform.position.x;
        int z = (int)curCell.transform.position.z;

        // 오른쪽 (x+1, z)
        if (x + 1 < mazeWidth)
        {
            MazeCell c = mazeGrid[x + 1, z];
            if (!c.IsVisited) list.Add(c);
        }

        // 왼쪽 (x-1, z)
        if (x - 1 >= 0)
        {
            MazeCell c = mazeGrid[x - 1, z];
            if (!c.IsVisited) list.Add(c);
        }

        // 앞 (x, z+1)
        if (z + 1 < mazeDepth)
        {
            MazeCell c = mazeGrid[x, z + 1];
            if (!c.IsVisited) list.Add(c);
        }

        // 뒤 (x, z-1)
        if (z - 1 >= 0)
        {
            MazeCell c = mazeGrid[x, z - 1];
            if (!c.IsVisited) list.Add(c);
        }

        return list;
    }

    void ClearWalls(MazeCell prevCell, MazeCell curCell)
    {
        if (prevCell == null) return;
        if (prevCell.transform.position.x < curCell.transform.position.x)
        {
            prevCell.ClearRightWall();
            curCell.ClearLeftWall();
        }
        else if (prevCell.transform.position.x > curCell.transform.position.x)
        {
            prevCell.ClearLeftWall();
            curCell.ClearRightWall();
        }
        else if (prevCell.transform.position.z < curCell.transform.position.z)
        {
            prevCell.ClearFrontWall();
            curCell.ClearBackWall();
        }
        else if (prevCell.transform.position.z > curCell.transform.position.z)
        {
            prevCell.ClearBackWall();
            curCell.ClearFrontWall();
        }
    }

    void AddExtraConnections(int loopsToAdd)
    {
        // 안전장치
        if (loopsToAdd <= 0) return;

        for (int i = 0; i < loopsToAdd; i++)
        {
            // 랜덤 셀 하나 고르기
            int x = Random.Range(0, mazeWidth);
            int z = Random.Range(0, mazeDepth);
            MazeCell a = mazeGrid[x, z];

            // 그 셀의 "이웃 후보들" 모으기 (방문 여부 상관없이 사방)
            List<MazeCell> neighborAll = new List<MazeCell>(4);

            if (x + 1 < mazeWidth) neighborAll.Add(mazeGrid[x + 1, z]);
            if (x - 1 >= 0) neighborAll.Add(mazeGrid[x - 1, z]);
            if (z + 1 < mazeDepth) neighborAll.Add(mazeGrid[x, z + 1]);
            if (z - 1 >= 0) neighborAll.Add(mazeGrid[x, z - 1]);

            if (neighborAll.Count == 0) continue;

            // 랜덤 이웃 한 명 선택
            MazeCell b = neighborAll[Random.Range(0, neighborAll.Count)];

            // a와 b 사이의 벽을 (이미 뚫렸든 말든) 한 번 더 제거 시도
            ClearWalls(a, b);
        }
    }

    void SetItems()
    {
        // 아이템 섞기
        for (int i = 0; i < items.Length; i++)
        {
            int r = Random.Range(i, items.Length);
            (items[i], items[r]) = (items[r], items[i]);
        }

        // 미로 셀 섞기
        for (int i = mazeList.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (mazeList[i], mazeList[r]) = (mazeList[r], mazeList[i]);
        }

        for(int i = 0; i <= MiniGameContext.gameLevel; i++)
        {
            items[i].transform.position = mazeList[i].transform.position;
            items[i].SetActive(true);
        }

        collectedItemCount = 0;
        totalItemCount = MiniGameContext.gameLevel + 1;
    }
    #endregion

    public override void Begin()
    {
        playing = true;
        Invoke(nameof(PlayBubble), Random.Range(2f, 5f));
    }

    private void Update()
    {
        if (!playing) return;
        if (!timer.Running || timer.Paused) return;

        Vector2 m = input.Move;

        inputDir = new Vector3(m.x, 0f, m.y).normalized;
    }

    void FixedUpdate()
    {
        if (autoWalking)
        {
            Vector3 to = autoTarget - rigid.position;
            to.y = 0f;

            float step = autoWalkSpeed * Time.fixedDeltaTime;
            Vector3 move = Vector3.ClampMagnitude(to, step);
            rigid.MovePosition(rigid.position + move);

            if (to.sqrMagnitude > 0.0004f)
            {
                // 회전 유지
                Quaternion targetRotAuto = Quaternion.LookRotation(to, Vector3.up);
                Quaternion smoothAuto = Quaternion.Slerp(rigid.rotation, targetRotAuto, turnSpeed * Time.fixedDeltaTime);
                rigid.MoveRotation(smoothAuto);
            }
            else
            {
                autoWalking = false;
                rigid.linearVelocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                if (anim) anim.SetBool("isMoving", false);
            }

            return;
        }


        if (!playing) return;
        if (!timer.Running || timer.Paused) return;

        if (inputDir.sqrMagnitude < 0.0001f)
        {
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
            // 애니도 정지
            if (anim) anim.SetBool("isMoving", false);
            return; // 이동/회전 로직 스킵
        }

        // 이동
        Vector3 target = rigid.position + inputDir * moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(target);

        // 회전
        Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
        Quaternion smooth = Quaternion.Slerp(rigid.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        rigid.MoveRotation(smooth);

        // 애니
        if (anim) anim.SetBool("isMoving", true);

        // 피니시 근접 판정
        if (!reachedFinish)
        {
            Vector3 dp = (player.position - finish.position);
            // y축 차이는 무시하고
            dp.y = 0f;

            if (dp.sqrMagnitude <= finishReachRadius * finishReachRadius)
            {
                if (collectedItemCount >= totalItemCount)
                {
                    reachedFinish = true;
                    OnReachFinish();
                }
                else
                {
                    if (!tweening)
                    {
                        tweening = true;
                        for (int i = 0; i < totalItemCount; i++)
                        {
                            if (!items[i].activeSelf) continue;
                            items[i].transform.DOKill();
                            items[i].transform.DOScale(1.5f, 1f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo)
                                .SetLink(gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy)
                                .OnComplete(() => { tweening = false; });
                        }
                    }
                }
            }
        }

        CheckItemCollect();
    }

    void CheckItemCollect()
    {
        for (int i = 0; i < totalItemCount; i++)
        {
            if (!items[i].activeSelf) continue;

            Vector3 dp = player.position - items[i].transform.position;
            dp.y = 0f;
            if (dp.sqrMagnitude <= 0.5f * 0.5f) 
            {
                items[i].SetActive(false);
                collectedItemCount++;

                score.Add(500, true, true);

                if(collectedItemCount >= totalItemCount)
                {
                    MazeCell exitCell = mazeGrid[mazeWidth - 1, mazeDepth - 1];
                    exitCell.ClearFrontWall();
                    finishParticle.Play();
                }
            }
        }
    }

    void OnReachFinish()
    {
        if (!playing) return;

        playing = false;
        clear = true;

        int bonus = Mathf.CeilToInt(timer.Remaining) * 10;
        if (bonus > 0) score.Add(bonus, false);

        // 자동 보행 설정
        autoTarget = finish.position + new Vector3(0f, 0f, postExitOffsetZ);
        autoWalking = true;
        anim.SetBool("isMoving", true);
        
        finish.GetComponentInChildren<Animator>().SetTrigger("doOpen");

        finishCamera.Priority = 10;

        Invoke(nameof(Finish), 2);
    }

    void PlayBubble()
    {
        bubbleParticle.transform.position = mazeList[Random.Range(0, mazeList.Count)].position;
        bubbleParticle.Play();
        AudioManager.Instance.PlaySFX(bubbleClip);
        Invoke(nameof(PlayBubble), Random.Range(2f, 5f));
    }

    void HandleFinish(int score, bool clear)
    {
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        if (anim) anim.SetBool("isMoving", false);
    }

    private void Start()
    {
        OnFinished += HandleFinish;
    }

    private void OnDestroy()
    {
        OnFinished-= HandleFinish;
    }
}
