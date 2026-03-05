using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileConnect : MiniGameBase
{
    [Header("Board")]
    int width = 15;
    int height = 6;
    Vector2 tileSize;
    [SerializeField] RectTransform boardRoot;
    [SerializeField] Button tilePrefab;

    [Header("Tiles")]
    [SerializeField] Sprite[] tileSprites;

    [Header("Path FX")]
    float pathShowTime = 0.5f;
    float pathThickness = 8f;
    [SerializeField] Color pathColor = Color.white;
    [SerializeField] UIParticle[] matchParticles;

    Tile[,] tiles;
    Tile? firstTile = null;
    int remainingTiles;
    bool playing;

    int[] pairsPerLevel = { 18, 27, 36 };

    // 1x1 흰색 텍스처 (UI 선분용)
    static Texture2D tex1x1;

    struct Tile
    {
        public int id;
        public int x, y;          // 보드 좌표 (0..W-1,0..H-1)
        public Button button;
        public bool active => button != null && button.gameObject.activeSelf;
    }

    // ----- 패딩 좌표(외곽 허용) -----
    int PW => width + 2;
    int PH => height + 2;
    int PX(int x) => x + 1;
    int PY(int y) => y + 1;
    bool InPad(int px, int py) => px >= 0 && px < PW && py >= 0 && py < PH;
    bool OccupiedP(int px, int py)
    {
        if (px <= 0 || px >= PW - 1 || py <= 0 || py >= PH - 1) return false; // 외곽은 빈칸
        var t = tiles[px - 1, py - 1];
        return t.button != null && t.active;
    }

    private void Start()
    {
        OnFinished += HandleFinish;
    }

    private void OnDestroy()
    {
        OnFinished -= HandleFinish;
        DOTween.Kill(this);
    }

    void HandleFinish(int score, bool clear)
    {
        playing = false;
    }

    public override void Setup(Score s, TimerController t, InputReader i)
    {
        base.Setup(s, t, i);
        GenerateBoard();
    }

    public override void Begin() { playing = true; }

    void GenerateBoard()
    {
        tiles = new Tile[width, height];

        var all = new List<(int x, int y)>(width * height);
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) all.Add((x, y));
        Shuffle(all);

        int maxPairs = (width * height) / 2;
        int pairs = Mathf.Clamp(pairsPerLevel[MiniGameContext.gameLevel], 1, maxPairs);

        int made = 0, guard = 0;
        while (made < pairs && guard++ < 10000)
        {
            bool placedAny = false;
            for (int i = 0; i < all.Count && !placedAny; i++)
            {
                if (!IsEmpty(all[i].x, all[i].y)) continue;
                for (int j = i + 1; j < all.Count && !placedAny; j++)
                {
                    if (!IsEmpty(all[j].x, all[j].y)) continue;
                    if (TryGetPathP(PX(all[i].x), PY(all[i].y), PX(all[j].x), PY(all[j].y), out _))
                    {
                        int id = made % tileSprites.Length;
                        PlaceTile(all[i].x, all[i].y, id);
                        PlaceTile(all[j].x, all[j].y, id);
                        made++; placedAny = true;
                    }
                }
            }
            if (!placedAny) Shuffle(all);
        }
        remainingTiles = made * 2;

        ApplyRenderOrder();
    }

    void ApplyRenderOrder()
    {
        // 위(y=0)부터 순서대로 뒤에 쌓이게 하고
        // 아래(y=height-1)로 갈수록 마지막(=가장 앞)으로 오도록
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var t = tiles[x, y];
                if (t.button != null)
                    t.button.transform.SetAsLastSibling();
            }
        }

        matchParticles[0].GetComponent<RectTransform>().SetAsLastSibling();
        matchParticles[1].GetComponent<RectTransform>().SetAsLastSibling();
    }

    // 중앙 정렬로 버튼 생성
    void PlaceTile(int x, int y, int id)
    {
        Button btn = Instantiate(tilePrefab, boardRoot);
        tileSize = btn.GetComponent<RectTransform>().sizeDelta;
        tileSize.y -= 20;
        float startX = -(width - 1) * tileSize.x * 0.5f;
        float startY = +(height - 1) * tileSize.y * 0.5f;
        Vector2 pos = new Vector2(startX + x * tileSize.x, startY - y * tileSize.y);
        btn.GetComponent<RectTransform>().anchoredPosition = pos;
        btn.image.sprite = tileSprites[id];
        int cx = x, cy = y;
        btn.onClick.AddListener(() => OnTileClick(cx, cy));
        tiles[x, y] = new Tile { id = id, x = x, y = y, button = btn };
    }

    bool IsEmpty(int x, int y) => tiles[x, y].button == null;

    // ==== 클릭/매치 ====
    void OnTileClick(int x, int y)
    {
        if (!playing || !timer.Running || timer.Paused) return;
        //if (fxPlaying) return; // 연출 중 입력 차단

        Tile tile = tiles[x, y];
        if (!tile.active) return;
        if (firstTile != null && firstTile.Value.button == tile.button) return;

        RectTransform rt = tile.button.GetComponent<RectTransform>();
        rt.DOAnchorPosY(rt.anchoredPosition.y + 20f, 0.2f).SetEase(Ease.Linear)
            .SetTarget(this)
            .OnComplete(() =>
            {
                if (firstTile == null)
                {
                    firstTile = tile;
                    return;
                }

                Tile t1 = firstTile.Value;
                Tile t2 = tile;
                firstTile = null;

                // 같은 ID + 연결 가능하면 경로 그려서 보여준 뒤 제거
                if (t1.id == t2.id && t1.button != t2.button && TryGetAnchoredPath(t1, t2, out var pts))
                {
                    StartCoroutine(PlayPathAndRemove(t1, t2, pts));
                }
                else
                {
                    score.Miss();
                    rt.DOAnchorPosY(rt.anchoredPosition.y - 15f, 0.2f).SetEase(Ease.Linear).SetTarget(this);
                    rt = t1.button.GetComponent<RectTransform>();
                    rt.DOAnchorPosY(rt.anchoredPosition.y - 15f, 0.2f).SetEase(Ease.Linear).SetTarget(this);
                }
            });
    }

    IEnumerator PlayPathAndRemove(Tile a, Tile b, List<Vector2> pts)
    {
        playing = false;

        // 경로 세그먼트들 생성
        var segments = DrawPathSegments(pts);

        // 매치 파티클
        matchParticles[0].GetComponent<RectTransform>().anchoredPosition = a.button.GetComponent<RectTransform>().anchoredPosition;
        matchParticles[1].GetComponent<RectTransform>().anchoredPosition = b.button.GetComponent<RectTransform>().anchoredPosition;
        matchParticles[0].Play(); matchParticles[1].Play();
        score.Add(10, true, true);

        // 유지
        yield return new WaitForSeconds(pathShowTime);

        // 세그먼트 제거 
        // TODO: 최적화 풀링
        foreach (var go in segments) if (go) GameObject.Destroy(go);

        // 타일 제거
        a.button.gameObject.SetActive(false);
        b.button.gameObject.SetActive(false);
        remainingTiles -= 2;


        if (remainingTiles <= 0)
        {
            // 클리어
            playing = false;
            score.Add((int)timer.Remaining * 10);
            clear = true;
            Finish();
            yield break;
        }

        if (!HasAnyMove()) { Finish(); yield break; }

        playing = true;
    }

    // ==== 경로 계산 (패딩 좌표 → 앵커드 좌표) ====
    bool TryGetAnchoredPath(Tile a, Tile b, out List<Vector2> pts)
    {
        if (!TryGetPathP(PX(a.x), PY(a.y), PX(b.x), PY(b.y), out var padPts)) { pts = null; return false; }
        pts = new List<Vector2>(padPts.Count);
        foreach (var p in padPts) pts.Add(PadToAnchored(p.x, p.y));
        return true;
    }

    // 패딩좌표를 UI 앵커드 좌표로 변환(타일 센터/외곽 포함)
    Vector2 PadToAnchored(int px, int py)
    {
        // px=1..width => 타일 0..width-1 의 센터
        // px=0(왼 외곽), px=width+1(오 외곽) 은 타일 한 칸 바깥
        float startX = -(width - 1) * tileSize.x * 0.5f;
        float startY = +(height - 1) * tileSize.y * 0.5f;

        float x;
        if (px == 0) x = startX - tileSize.x;
        else if (px == width + 1) x = startX + (width - 1) * tileSize.x + tileSize.x;
        else x = startX + (px - 1) * tileSize.x;

        float y;
        if (py == 0) y = startY + tileSize.y;
        else if (py == height + 1) y = startY - (height - 1) * tileSize.y - tileSize.y;
        else y = startY - (py - 1) * tileSize.y;

        return new Vector2(x, y);
    }

    // 패딩 좌표에서 경로 점(시작, 코너들, 끝) 찾기
    bool TryGetPathP(int ax, int ay, int bx, int by, out List<(int x, int y)> path)
    {
        path = null;
        if (ax == bx && ay == by) return false;

        // 직선
        if (ClearPathP(ax, ay, bx, by))
        { path = new() { (ax, ay), (bx, by) }; return true; }

        // 한 번 굴절(L)
        if (InPad(ax, by) && !OccupiedP(ax, by) && ClearPathP(ax, ay, ax, by) && ClearPathP(ax, by, bx, by))
        { path = new() { (ax, ay), (ax, by), (bx, by) }; return true; }

        if (InPad(bx, ay) && !OccupiedP(bx, ay) && ClearPathP(ax, ay, bx, ay) && ClearPathP(bx, ay, bx, by))
        { path = new() { (ax, ay), (bx, ay), (bx, by) }; return true; }

        // 두 번 굴절: a에서 사방으로 뻗으며 피벗 P를 찾고, (P, b)가 한 번 굴절로 이어지는지 확인
        // 가로로
        for (int x = ax - 1; x >= 0 && !OccupiedP(x, ay); x--)
            if (TryGetOneTurnPath(x, ay, bx, by, out var p1)) { path = new() { (ax, ay) }; path.AddRange(p1); return true; }

        for (int x = ax + 1; x < PW && !OccupiedP(x, ay); x++)
            if (TryGetOneTurnPath(x, ay, bx, by, out var p2)) { path = new() { (ax, ay) }; path.AddRange(p2); return true; }

        // 세로로
        for (int y = ay - 1; y >= 0 && !OccupiedP(ax, y); y--)
            if (TryGetOneTurnPath(ax, y, bx, by, out var p3)) { path = new() { (ax, ay) }; path.AddRange(p3); return true; }

        for (int y = ay + 1; y < PH && !OccupiedP(ax, y); y++)
            if (TryGetOneTurnPath(ax, y, bx, by, out var p4)) { path = new() { (ax, ay) }; path.AddRange(p4); return true; }

        return false;
    }

    bool TryGetOneTurnPath(int ax, int ay, int bx, int by, out List<(int x, int y)> path)
    {
        path = null;
        // A -> (ax, by) -> B
        if (InPad(ax, by) && !OccupiedP(ax, by) && ClearPathP(ax, ay, ax, by) && ClearPathP(ax, by, bx, by))
        { path = new() { (ax, ay), (ax, by), (bx, by) }; return true; }

        // A -> (bx, ay) -> B
        if (InPad(bx, ay) && !OccupiedP(bx, ay) && ClearPathP(ax, ay, bx, ay) && ClearPathP(bx, ay, bx, by))
        { path = new() { (ax, ay), (bx, ay), (bx, by) }; return true; }

        return false;
    }

    bool ClearPathP(int x1, int y1, int x2, int y2)
    {
        if (x1 == x2)
        {
            int minY = Mathf.Min(y1, y2), maxY = Mathf.Max(y1, y2);
            for (int y = minY + 1; y < maxY; y++) if (OccupiedP(x1, y)) return false;
            return true;
        }
        if (y1 == y2)
        {
            int minX = Mathf.Min(x1, x2); 
            int maxX = Mathf.Max(x1, x2);
            for (int x = minX + 1; x < maxX; x++) if (OccupiedP(x, y1)) return false;
            return true;
        }
        return false;
    }

    // ==== 경로 UI 그리기 ====
    List<GameObject> DrawPathSegments(List<Vector2> pts)
    {
        if (tex1x1 == null)
        {
            tex1x1 = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex1x1.SetPixel(0, 0, Color.white); tex1x1.Apply();
        }

        var list = new List<GameObject>();
        for (int i = 0; i < pts.Count - 1; i++)
        {
            var a = pts[i];
            var b = pts[i + 1];

            // 세그먼트 오브젝트
            var go = new GameObject("PathSeg", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(boardRoot, false);
            var rt = go.GetComponent<RectTransform>();
            var img = go.GetComponent<RawImage>();
            img.texture = tex1x1;
            img.color = pathColor;

            // 길이/회전/위치
            Vector2 dir = b - a;
            float len = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rt.sizeDelta = new Vector2(len, pathThickness);
            rt.anchoredPosition = (a + b) * 0.5f;
            rt.localRotation = Quaternion.Euler(0, 0, angle);

            list.Add(go);
        }
        return list;
    }

    // ==== 무브 검사 & 유틸 ====
    bool HasAnyMove()
    {
        var list = new List<Tile>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (tiles[x, y].active) list.Add(tiles[x, y]);

        for (int i = 0; i < list.Count; i++)
            for (int j = i + 1; j < list.Count; j++)
                if (list[i].id == list[j].id && TryGetPathP(PX(list[i].x), PY(list[i].y), PX(list[j].x), PY(list[j].y), out _))
                    return true;
        return false;
    }

    void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--) { int r = Random.Range(0, i + 1); (list[i], list[r]) = (list[r], list[i]); }
    }
}
