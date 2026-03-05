using UnityEngine;
using System.Collections.Generic;

// 노드 경로 데이터 클래스
[System.Serializable]
public class NodePath
{
    public List<Transform> nodes = new List<Transform>(); // 여러 노드를 연결 할 수 있도록 리스트로 정의
    public float width = 3f; // 폭
    public float nearRange = 2f; // 근거리
    public float midRange = 4f; // 중거리
    public float farRange = 8f; // 원거리
}

public class PathSystem : MonoBehaviour
{
    public List<NodePath> paths = new List<NodePath>(); // 여러 노드Path를 관리할 수 있도록 리스트로 정의

    public Color nearColor = Color.blue;
    public Color midColor = Color.yellow;
    public Color farColor = Color.red;

    // 기즈모 관련 상수
    private const float GIZMO_STEP_FACTOR = 0.05f;
    private const float GIZMO_SPACING_FACTOR = 0.2f;

    // GetDistanceToPath함수에서 계산한 거리값을 바탕으로 조건부 색상 반환 함수
    public Color GetConditionalColor(Vector3 pos)
    {
        float nearestDist = float.MaxValue; // dist와 비교하기 위해 max값으로 초기화
        NodePath nearestPath = null; // 가장 가까운 경로를 저장할 변수를 우선은 null로 초기화

        // 현재 위치에서 가장 가까운 경로를 찾는 반복문
        foreach (var path in paths)
        {
            float dist = GetDistanceToPath(pos, path);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestPath = path;
            }
        }

        // 가장 가까운 경로가 없으면 디폴트 색상으로 투명 반환
        if (nearestPath == null)
            return Color.clear;

        float w = nearestPath.width;
        float nearR = nearestPath.nearRange * w;
        float midR = nearestPath.midRange * w;
        float farR = nearestPath.farRange * w;

        if (nearestDist > farR) return Color.clear;
        if (nearestDist <= nearR) return nearColor;
        if (nearestDist <= midR) return midColor;
        return farColor;
    }

    // 거리 계산용 함수
    private float GetDistanceToPath(Vector3 point, NodePath path)
    {
        float nearest = float.MaxValue;

        for (int i = 0; i < path.nodes.Count - 1; i++)
        {
            Vector3 a = path.nodes[i].position;
            Vector3 b = path.nodes[i + 1].position;

            Vector3 ap = point - a;
            Vector3 ab = b - a;
            float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / ab.sqrMagnitude);
            Vector3 nearestPoint = a + ab * t;

            float dist = Vector3.Distance(point, nearestPoint);
            if (dist < nearest)
                nearest = dist;
        }

        // 시작점, 끝점도 포함
        nearest = Mathf.Min(nearest,
            Vector3.Distance(point, path.nodes[0].position),
            Vector3.Distance(point, path.nodes[^1].position));
            
        return nearest;
    }

    private void OnDrawGizmos()
    {
        if (paths == null) return;

        foreach (var path in paths)
        {
            if (path.nodes.Count < 2) continue;

            float w = path.width;
            float nearR = path.nearRange * w;
            float midR = path.midRange * w;
            float farR = path.farRange * w;

            for (int i = 0; i < path.nodes.Count - 1; i++)
            {
                Vector3 a = path.nodes[i].position;
                Vector3 b = path.nodes[i + 1].position;
                Vector3 dir = (b - a).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, dir);
                float length = Vector3.Distance(a, b);

                float step = Mathf.Max(1f, length * GIZMO_STEP_FACTOR);
                float spacing = Mathf.Max(1f, farR * GIZMO_SPACING_FACTOR);

                for (float d = -farR; d <= length + farR; d += step)
                {
                    for (float x = -farR; x <= farR; x += spacing)
                    {
                        Vector3 pos = a + dir * d + right * x;
                        float dist = GetDistanceToPath(pos, path);
                        if (dist > farR) continue;

                        Color c = dist <= nearR ? nearColor :
                                  dist <= midR ? midColor : farColor;

                        Gizmos.color = c;
                        Gizmos.DrawSphere(pos, spacing * 0.15f);
                    }
                }

                Gizmos.color = Color.white * 0.6f;
                Gizmos.DrawLine(a, b);
            }
        }
    }
}
