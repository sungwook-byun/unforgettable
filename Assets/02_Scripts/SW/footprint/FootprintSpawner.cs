using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FootprintSpawner : MonoBehaviour
{
    [Header("참조")]
    public PathSystem pathSystem;

    [Header("빛")]
    public GameObject lightPrefab; // 빛기둥 또는 파티클 오브젝트

    [Header("설정")]
    public float stepDistance = 0.5f; // 빈도 간격
    public float lightLife = 2f; // 빛 지속 시간
    public int poolSize = 15; // 오브젝트 풀 크기

    private List<GameObject> pool = new List<GameObject>();
    private Vector3 lastPos;
    private int poolIndex;

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(lightPrefab);
            obj.SetActive(false);
            pool.Add(obj);
        }

        lastPos = transform.position;
    }

    // 거리를 계산해서 빛 생성
    void Update()
    {
        float dist = Vector3.Distance(transform.position, lastPos);
        if (dist >= stepDistance)
        {
            SpawnLight(transform.position);
            lastPos = transform.position;
        }
    }

    // 오브젝트 풀에서 빛을 꺼내서 배치
    private void SpawnLight(Vector3 position)
    {
        GameObject obj = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Count;

        // Y값 (높이)
        position.y = 0.1f;

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);

        // 빛 색상 설정
        Light light = obj.GetComponent<Light>();
        if (light != null)
        {
            Color color = pathSystem ? pathSystem.GetConditionalColor(position) : Color.white;
            light.color = (color == Color.clear) ? Color.white : color;
            light.range = 2f;
            light.intensity = 3f;
        }

        // 코루틴으로 2초 후 비활성화
        StartCoroutine(DisableAfterTime(obj));
    }


    // 코루틴 함수
    private IEnumerator DisableAfterTime(GameObject obj)
    {
        yield return new WaitForSeconds(lightLife);
        obj.SetActive(false);
    }
}
