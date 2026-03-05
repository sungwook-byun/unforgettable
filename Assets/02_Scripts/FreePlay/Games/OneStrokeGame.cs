using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class OneStrokeGame : MiniGameBase
{
    [System.Serializable]
    public class Pattern
    {
        public Vector2[] points;
        public Edge[] edges;
    }

    [System.Serializable]
    public class Edge
    {
        public int a; // points 인덱스
        public int b; // points 인덱스
    }

    class NodeTag : MonoBehaviour
    {
        public int nodeId;
    }

    [SerializeField] Pattern[] patterns;
    [SerializeField] Pattern[] easyPatterns, normalPatterns, hardPatterns;
    [SerializeField] GameObject pointPrefab;
    [SerializeField] LineRenderer guideLine, playerLine;
    [SerializeField] ParticleSystem dragParticle;
    [SerializeField] AudioClip touchClip;
    [SerializeField] TextMeshProUGUI stageText;


    // 내부 상태
    Pattern currentPattern;
    List<GameObject> spawnedPoints = new List<GameObject>();
    int patternIndex;

    // 간선 사용 여부 추적용
    HashSet<string> usedEdges = new HashSet<string>();

    // 마지막으로 방문한 노드 
    int lastNodeId = -1;

    Queue<GameObject> pointPool = new Queue<GameObject>();

    bool playing;
    bool dragStarted = false;

    private void Start()
    {
        stageText.text = $"<color=#FFFFFFFF>{patternIndex}</color> <color=#FFFFFF7F>/ 5</color>";
    }

    public override void Begin()
    {
        // 레벨 적용
        switch (MiniGameContext.gameLevel)
        {
            case 0: patterns = easyPatterns; break;
            case 1: patterns = normalPatterns; break;
            case 2: patterns = hardPatterns; break;
        }

        SpawnRandomPattern();
        playing = true;
    }

    void Update()
    {
        if (!playing) return;
        if (!timer.Running || timer.Paused) return;

        if (input.PointerDown)
        {
            TryBeginDrag();
        }
        else if (input.PointerHold)
        {
            DragUpdate();
            DetectNodeUnderCursor();
        }
        else if (input.PointerUp)
        {
            bool success = CheckSolveSuccess();

            if (success)
            {
                score.Add(100, true ,true);
                patternIndex++;
                stageText.text = $"<color=#FFFFFFFF>{patternIndex}</color> <color=#FFFFFF7F>/ 5</color>";
                if (patternIndex >= patterns.Length)
                {
                    clear = true;
                    score.Add((int)timer.Remaining*50,false);
                    Finish();
                }
                else SpawnRandomPattern();
            }
            else
            {
                score.Miss();
                ResetPlayerProgress();
            }

            dragStarted = false;
            dragParticle.Stop();
        }
    }

    void ResetPlayerProgress()
    {
        usedEdges.Clear();
        lastNodeId = -1;

        if (playerLine != null) playerLine.positionCount = 0;

        for (int i = 0; i < spawnedPoints.Count; i++)
        {
            Transform t = spawnedPoints[i].transform;
            if (t.localScale != Vector3.one)
            {
                t.DOKill();
                t.localScale = Vector3.one;

            }

        }
    }

    #region PointPool
    GameObject GetPooledPoint()
    {
        if (pointPool.Count > 0)
        {
            GameObject dot = pointPool.Dequeue();
            dot.SetActive(true);
            return dot;
        }
        else
        {
            GameObject dot = Instantiate(pointPrefab, transform);
            return dot;
        }
    }

    void ReturnAllPointsToPool()
    {
        for(int i = 0; i < spawnedPoints.Count; i++)
        {
            spawnedPoints[i].SetActive(false);
            pointPool.Enqueue(spawnedPoints[i]);
            spawnedPoints[i].transform.DOKill();
            spawnedPoints[i].transform.localScale = Vector3.one;

        }
        spawnedPoints.Clear();
    }
    #endregion

    #region GuidePattern
    void SpawnRandomPattern()
    {


        // 랜덤 패턴 선택
        //int randIndex = Random.Range(0, patterns.Length);
        currentPattern = patterns[patternIndex];

        // 패턴 초기화
        ReturnAllPointsToPool();
        usedEdges.Clear();
        lastNodeId = -1;

        // 라인 초기화
        playerLine.positionCount = 0;
        playerLine.useWorldSpace = true;
        guideLine.positionCount = 0;
        guideLine.useWorldSpace = true;

        // 포인트 생성
        for (int i = 0; i < currentPattern.points.Length; i++)
        {
            GameObject dot = GetPooledPoint();
            dot.transform.position = new Vector3(currentPattern.points[i].x, currentPattern.points[i].y, 0f);
            dot.transform.SetParent(transform);

            NodeTag tag = dot.GetComponent<NodeTag>();
            if (tag == null) tag = dot.AddComponent<NodeTag>();
            tag.nodeId = i;

            spawnedPoints.Add(dot);
        }

        BuildGuideLine();
    }

    void BuildGuideLine()
    {
        guideLine.positionCount = currentPattern.edges.Length + 1;
        int firstPointIndex = currentPattern.edges[0].a;
        guideLine.SetPosition(0, new Vector3(currentPattern.points[firstPointIndex].x, currentPattern.points[firstPointIndex].y));
        for (int i = 0; i < currentPattern.edges.Length; i++)
        {
            int pointIndex = currentPattern.edges[i].b;
            guideLine.SetPosition(i + 1, new Vector3(currentPattern.points[pointIndex].x, currentPattern.points[pointIndex].y));
        }
    }
    #endregion

    #region PlayerPattern
    void TryBeginDrag()
    {
        Collider2D hit = Physics2D.OverlapPoint(input.PointerWorldPos);
        if (hit == null) return;

        NodeTag node = hit.GetComponent<NodeTag>();
        if (node == null) return;

        int id = node.nodeId;

        dragStarted = true;
        lastNodeId = id;

        usedEdges.Clear();

        // 첫 점 + 첫 프리뷰 점 생성
        playerLine.positionCount = 2;
        Vector3 pos = spawnedPoints[id].transform.position;
        playerLine.SetPosition(0, pos);
        playerLine.SetPosition(1, pos);

        // 첫 노드 강조
        TweenPoint(id, -1);

        dragParticle.Play();
    }


    void DragUpdate()
    {
        if (!dragStarted) return;
        if (playerLine.positionCount < 2) return;

        int last = playerLine.positionCount - 1;
        playerLine.SetPosition(last, input.PointerWorldPos);
        dragParticle.transform.position = input.PointerWorldPos;
    }


    void DetectNodeUnderCursor()
    {
        // 이 지점에 있는 콜라이더 확인
        Collider2D hit = Physics2D.OverlapPoint(input.PointerWorldPos);
        if (hit == null) return;

        NodeTag node = hit.GetComponent<NodeTag>();
        if (node == null) return;

        int id = node.nodeId;

        // 방문 처리
        RegisterVisit(id);
    }

    void RegisterVisit(int nodeId)
    {
        if (!dragStarted) return;

        // 첫 노드 클릭은 TryBeginDrag()에서 처리하므로
        // 여기서는 2번째 노드부터 처리됨

        int from = lastNodeId;
        int to = nodeId;

        if (from == to) return;
        if (!EdgeExistsInPattern(from, to)) return;

        string edgeKey = MakeEdgeKey(from, to);
        if (usedEdges.Contains(edgeKey)) return;

        // 정답 간선 사용 처리
        usedEdges.Add(edgeKey);

        // 미리점 확정: 마지막 점을 노드 위치로 고정
        int lastIdx = playerLine.positionCount - 1;
        Vector3 nodePos = spawnedPoints[nodeId].transform.position;
        playerLine.SetPosition(lastIdx, nodePos);

        TweenPoint(nodeId, lastNodeId);

        // 새 프리뷰 점 추가 (지금 노드 위치에서 시작)
        playerLine.positionCount++;
        int previewIdx = playerLine.positionCount - 1;
        playerLine.SetPosition(previewIdx, nodePos); // 새 프리뷰 점은 노드 위치로 생성

        // 마지막 노드 갱신
        lastNodeId = nodeId;
    }


    // 패턴에 간선이 실제로 존재하는지 확인
    bool EdgeExistsInPattern(int a, int b)
    {
        if (currentPattern == null || currentPattern.edges == null)
            return false;

        // 무방향 간선이니까 a-b 또는 b-a 둘 다 허용
        for (int i = 0; i < currentPattern.edges.Length; i++)
        {
            var e = currentPattern.edges[i];
            if ((e.a == a && e.b == b) || (e.a == b && e.b == a))
            {
                return true;
            }
        }
        return false;
    }

    // 간선을 HashSet에 넣을 때 키로 쓸 문자열
    // 무방향 간선이므로 작은쪽-큰쪽 으로 정규화해서 저장
    string MakeEdgeKey(int a, int b)
    {
        if (a < b) return a + "-" + b;
        else return b + "-" + a;
    }

    void AddPointToLine(int nodeId)
    {
        Vector3 p = spawnedPoints[nodeId].transform.position;

        int idx = playerLine.positionCount;
        playerLine.positionCount = idx + 1;
        playerLine.SetPosition(idx, p);
    }

    void TweenPoint(int nodeId, int lastId = -1)
    {
        if (lastId > -1)
        {
            spawnedPoints[lastId].transform.DOKill();
            spawnedPoints[lastId].transform.DOScale(1f, 0.25f);
        }

        spawnedPoints[nodeId].transform.DOKill();
        spawnedPoints[nodeId].transform.DOScale(1.5f, 0.25f);

        AudioManager.Instance.PlaySFX(touchClip);
    }

    #endregion

    bool CheckSolveSuccess()
    {
        // 패턴에 정의된 전체 간선 수
        int totalEdges = (currentPattern.edges != null) ? currentPattern.edges.Length : 0;

        // 실제로 사용한 간선 수
        int usedEdgesCount = usedEdges.Count;

        // 모든 간선을 정확히 한 번씩 다 썼다면 성공
        return (usedEdgesCount == totalEdges);
    }
}
