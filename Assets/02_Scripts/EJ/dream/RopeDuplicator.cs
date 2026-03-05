using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RopeDuplicator : MonoBehaviour
{
    [SerializeField] private RopeMeshGenerator[] originalRope;
    [SerializeField] private RopeMeshGenerator[] originalRopeR;
    [SerializeField] private RopeMeshGenerator[] originalRopeL;
    [SerializeField] private Transform playerTransform;

    [SerializeField] private int maxRopes = 150;
    [SerializeField] private float offsetDistance = 0.2f;

    static public int direction = 0; // 0: 정면, 1: 오, 2: 왼
    
    private float despawnDistance = 15f; // 너무 멀면 삭제

    private List<GameObject> ropes = new List<GameObject>();

    private bool allowSpawning = true;

    void Start()
    {
        allowSpawning = true;
        SpawnRope(direction);
    }

    void Update()
    {
        if (ropes.Count == 0) return;

        for (int i = ropes.Count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(playerTransform.position, ropes[i].transform.position) > despawnDistance)
            {
                Destroy(ropes[i]);
                ropes.RemoveAt(i);
            }
        }
        
        if (allowSpawning)
        {
            while (ropes.Count < maxRopes)
            {
                SpawnRope(direction);
            }
        }
    }

    void SpawnRope(int rot)
    {
        Vector2 randomCircle = Random.insideUnitCircle;
        Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y) * Random.Range(0.05f, offsetDistance);

        Vector3 spawnPos = playerTransform.position + offset;
        spawnPos.y = 0f; // y 고정

        RopeMeshGenerator[] selectedArray;
        switch (rot)
        {
            case 0: selectedArray = originalRope; break;
            case 1: selectedArray = originalRopeR; break;
            case 2: selectedArray = originalRopeL; break;
            case 3: selectedArray = originalRope; break;
            default: selectedArray = originalRope; break;
        }
        
        if (selectedArray.Length == 0) return;
        int randomIndex = Random.Range(0, selectedArray.Length);
        RopeMeshGenerator selectedRope = selectedArray[randomIndex];

        Quaternion newRot = selectedRope.transform.rotation;
        if (rot == 3) newRot *= Quaternion.Euler(0f, 90f, 0f); // Y축으로 +90도 회전

        GameObject copy = Instantiate(selectedRope.gameObject, spawnPos, newRot, transform);
        copy.name = $"Rope_{ropes.Count}";

        ropes.Add(copy);
    }

    /// 로프 스폰 중단 (기존 로프는 거리 기준으로 계속 삭제됨)
    public void StopSpawning()
    {
        allowSpawning = false;
    }

    /// 모든 로프 제거
    public void ClearAllRopes()
    {
        foreach (var rope in ropes)
        {
            if (rope != null) Destroy(rope);
        }
        ropes.Clear();
    }
}