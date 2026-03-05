using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class LocationGame : MiniGameBase
{
    [System.Serializable]
    public class RoomData
    {
        public GameObject room;
        public GameObject[] easyObjects, normalObjects, hardObjects;
    }

    class ObjectPosition : MonoBehaviour
    {
        public Vector3 correctPos, startPos;
        public Vector2 correctViewport; // 화면 기준 정답 좌표(0~1)
        public Vector2 correctScreen; // 픽셀 단위 비교용
        public Vector3 originalScale;
    }

    [SerializeField] Image timerImage;
    [SerializeField] RectTransform blind;
    [SerializeField] ParticleSystem correctParticle;
    float revealDuration = 10;

    [SerializeField] RoomData[] roomDatas;
    GameObject[] objects;
    [SerializeField] Transform[] startPositions;
    [SerializeField] Camera gameCam;
    [SerializeField] LayerMask pickMask; // 드래그할 오브젝트 레이어

    HashSet<GameObject> selectedObjects = new HashSet<GameObject>();
    List<GameObject> selectedList;
    int roomIndex;
    bool playing, showingHint;

    ObjectPosition curObject;
    Vector3 pickBaseScale; // 드래그 시작 순간의 스케일(표준화 반영된 현재 스케일)

    // 드래그 보정용
    Vector3 dragOffset; // 잡은 지점과 오브젝트 pivot간 오프셋
    float zFixed = -5f;    // 드래그 동안 유지할 Z
    float fixedDepth;                      // 카메라 기준 고정 depth
    float pxThreshold, pxThreshold2;

    AudioSource clockSource;

    private void Start()
    {
        fixedDepth = zFixed - gameCam.transform.position.z; // -10 -> 5면 15
        // 해상도에 무관한 상대 임계값(예: 화면 높이의 2%) 또는 절대 픽셀값 사용
        pxThreshold = Mathf.Max(Screen.width, Screen.height) * 0.02f; // 2%
        pxThreshold2 = pxThreshold * pxThreshold * 2;

        clockSource = GetComponent<AudioSource>();
    }

    public override void Begin()
    {
        StartCoroutine(SetRoom());
    }

    IEnumerator SetRoom()
    {
        timer.SetStop(true);
        playing = false;
        clockSource.Play();

        int it = 0;
        selectedObjects.Clear();
        switch (MiniGameContext.gameLevel)
        {
            case 0:
                objects = roomDatas[roomIndex].easyObjects;
                break;
            case 1:
                objects = roomDatas[roomIndex].normalObjects;
                break;
            case 2:
                objects = roomDatas[roomIndex].hardObjects;
                break;
        }
        while (selectedObjects.Count < startPositions.Length)
        {
            selectedObjects.Add(objects[Random.Range(0, objects.Length)]);
            it++;
            if (it > 100) break;
        }

        selectedList = selectedObjects.ToList();
        for (int i = 0; i < selectedList.Count; i++)
        {
            ObjectPosition objectPos = selectedList[i].GetComponent<ObjectPosition>();
            if (objectPos == null) objectPos = selectedList[i].AddComponent<ObjectPosition>();
            objectPos.correctPos = objectPos.transform.position;
            objectPos.startPos = startPositions[i].position;

            Vector3 vp = gameCam.WorldToViewportPoint(objectPos.correctPos);
            objectPos.correctViewport = new Vector2(vp.x, vp.y);

            objectPos.originalScale = objectPos.transform.localScale;

            Vector3 sp = gameCam.WorldToScreenPoint(objectPos.correctPos);
            objectPos.correctScreen = new Vector2(sp.x, sp.y);

            Collider col = objectPos.GetComponent<Collider>();
            col.enabled = true;
            //StandardizeScale(objectPos.gameObject, objectPos, 1.5f, 1f);
        }
        roomDatas[roomIndex].room.SetActive(true);

        //yield return timerImage.DOFillAmount(1f, revealDuration).WaitForCompletion();
        timerImage.DOFillAmount(1f, revealDuration);
        yield return new WaitForSeconds(revealDuration);
        //blind.SetActive(true);
        blind.DOAnchorPosY(0, 0.5f);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < selectedList.Count; i++)
        {
            ObjectPosition objectPos = selectedList[i].GetComponent<ObjectPosition>();
            objectPos.transform.position = objectPos.startPos;
            StandardizeScale(objectPos.gameObject, objectPos, 1.5f, 0.8f);
        }

        timerImage.fillAmount = 0;
        //blind.SetActive(false);
        blind.DOAnchorPosY(1080, 0.5f);
        playing = true;
        timer.SetStop(false);
        clockSource.Stop();
    }

    static bool TryGetWorldSize_BoxFirst(GameObject obj, out Vector3 worldSize)
    {
        const float EPS = 1e-6f;

        var boxes = obj.GetComponentsInChildren<BoxCollider>(true);
        if (boxes != null && boxes.Length > 0)
        {
            Bounds acc = new Bounds();
            bool has = false;

            foreach (var bc in boxes)
            {
                // BoxCollider는 local size를 가짐 → lossyScale로 월드 환산
                Vector3 szLocal = bc.size;
                Vector3 scl = bc.transform.lossyScale; // 부모 스케일까지 포함
                Vector3 szWorld = new Vector3(
                    Mathf.Abs(szLocal.x * scl.x),
                    Mathf.Abs(szLocal.y * scl.y),
                    Mathf.Abs(szLocal.z * scl.z)
                );

                // EPS 가드
                if (szWorld.x < EPS || szWorld.y < EPS || szWorld.z < EPS) continue;

                // 월드 AABB로 변환(회전 고려)
                var b = new Bounds(bc.transform.TransformPoint(bc.center), Vector3.zero);
                // box의 8개 꼭짓점을 월드로 변환해 Bounds에 캡슐레이션
                Vector3 half = szWorld * 0.5f;
                for (int xi = -1; xi <= 1; xi += 2)
                    for (int yi = -1; yi <= 1; yi += 2)
                        for (int zi = -1; zi <= 1; zi += 2)
                        {
                            Vector3 localCorner = Vector3.Scale(new Vector3(xi, yi, zi), half);
                            // localCorner는 box의 로컬축 기준이므로 회전 적용
                            Vector3 worldCorner = bc.transform.TransformPoint(bc.center) +
                                                  bc.transform.rotation * localCorner;
                            b.Encapsulate(worldCorner);
                        }

                if (!has) { acc = b; has = true; }
                else acc.Encapsulate(b);
            }

            if (has)
            {
                worldSize = acc.size;
                return true;
            }
        }

        // Renderer fallback (보이는 크기 기준)
        var rends = obj.GetComponentsInChildren<Renderer>(true);
        if (rends != null && rends.Length > 0)
        {
            Bounds acc = new Bounds();
            bool has = false;
            foreach (var r in rends)
            {
                if (!has) { acc = r.bounds; has = true; }
                else acc.Encapsulate(r.bounds);
            }
            if (has)
            {
                worldSize = acc.size;
                return true;
            }
        }

        worldSize = Vector3.zero;
        return false;
    }
    static void StandardizeScale(GameObject obj, ObjectPosition op, float maxTarget = 1f, float minTarget = 0.5f, float clampMin = 0.8f, float clampMax = 5f)
    {
        //if (op.standardized) return;                  // 이미 했다면 스킵
        if (op.originalScale == Vector3.zero)         // 최초 1회 저장
            op.originalScale = obj.transform.localScale;

        if (!TryGetWorldSize_BoxFirst(obj, out var size)) return;

        float maxSize = Mathf.Max(size.x, size.y, size.z);
        float minSize = Mathf.Min(size.x, size.y, size.z);

        float scaleFactor = 1f;
        if (maxSize > maxTarget) scaleFactor = maxTarget / maxSize;
        else if (minSize < minTarget) scaleFactor = minTarget / minSize;
        //else { op.standardized = true; return; }      // 범위 내면 패스

        if (float.IsNaN(scaleFactor) || float.IsInfinity(scaleFactor)) return;

        scaleFactor = Mathf.Clamp(scaleFactor, clampMin, clampMax);
        obj.transform.localScale *= scaleFactor;

        //op.standardized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing) return;
        if (!timer.Running || timer.Paused) return;

        // 마우스 왼쪽 버튼 누르고 있는 동안만 체크
        if (input.PointerDown)
        {
            DetectObjectUnderCursor();
        }
        else if (input.PointerHold)
        {
            if (curObject == null) return;

            Vector3 cursorWorld = gameCam.ScreenToWorldPoint(new Vector3(input.Pointer.x, input.Pointer.y, fixedDepth));
            Vector3 target = cursorWorld + dragOffset;

            target.z = zFixed; // z 고정
            curObject.transform.position = target;

            // 스크린 좌표 계산
            Vector3 curSp3 = gameCam.WorldToScreenPoint(curObject.transform.position);
            // 제곱거리
            float dx = curSp3.x - curObject.correctScreen.x;
            float dy = curSp3.y - curObject.correctScreen.y;
            float dist2 = dx * dx + dy * dy;
            if (!showingHint && dist2 <= pxThreshold2)
            {
                showingHint = true;
                Transform t = curObject.transform;
                t.DOKill();
                t.DOScale(pickBaseScale * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(t.gameObject);
            }
            else if (showingHint && dist2 > pxThreshold2)
            {
                showingHint = false;
                Transform t = curObject.transform;
                t.DOKill();
                t.DOScale(pickBaseScale, 0.2f);
            }
        }
        else if (input.PointerUp)
        {
            if (curObject == null) return;

            Transform t = curObject.transform;
            t.DOKill();
            t.DOScale(Vector3.one, 0.2f);

            // 현재 스크린 좌표
            Vector2 curScreen = (Vector2)gameCam.WorldToScreenPoint(curObject.transform.position);
            bool success = Vector2.Distance(curScreen, curObject.correctScreen) < pxThreshold;

            if (success)
            {
                curObject.transform.DOMove(curObject.correctPos, 0.5f);
                curObject.transform.DOScale(curObject.originalScale, 0.5f);
                score.Add(100, true, true);
                curObject.GetComponent<Collider>().enabled = false;
                selectedList.Remove(curObject.gameObject);
                correctParticle.transform.position = curObject.correctPos;
                correctParticle.transform.position = new Vector3(correctParticle.transform.position.x, correctParticle.transform.position.y, zFixed);
                correctParticle.Play();
            }
            else
            {
                curObject.transform.DOMove(curObject.startPos, 0.5f);
                curObject.transform.DOScale(pickBaseScale, 0.2f);
                score.Miss();
            }
            curObject = null;

            if(selectedList.Count <= 0)
            {
                StartCoroutine(NextStage());
            }
        }
    }

    IEnumerator NextStage()
    {
        yield return new WaitForSeconds(1);
        roomDatas[roomIndex].room.SetActive(false);
        roomIndex++;
        if (roomIndex >= roomDatas.Length)
        {
            playing = false;
            clear = true;
            score.Add((int)timer.Remaining * 50);
            Finish();
        }
        else
        {
            StartCoroutine(SetRoom());
        }
    }

    void DetectObjectUnderCursor()
    {
        Ray ray = gameCam.ScreenPointToRay(new Vector3(input.Pointer.x, input.Pointer.y, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, pickMask)) { curObject = null; return; }

        curObject = hit.transform.GetComponent<ObjectPosition>();
        if (curObject == null) return;

        // 포인터 위치를 "고정 depth"에서 월드로 변환
        Vector3 pickWorld = gameCam.ScreenToWorldPoint(new Vector3(input.Pointer.x, input.Pointer.y, fixedDepth));

        // 드래그 오프셋: 현재 오브젝트 위치 - 마우스로 찍은 월드 위치
        dragOffset = curObject.transform.position - pickWorld;

        // z 오프셋은 없도록 고정 (z=5만 쓰기 위해)
        dragOffset.z = 0f;

        // 드래그 시작 시 힌트 상태/스케일 초기화
        showingHint = false;
        pickBaseScale = curObject.transform.localScale;
    }
}
