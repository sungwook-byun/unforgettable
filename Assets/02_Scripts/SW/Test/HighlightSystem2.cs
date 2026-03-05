using System.Collections;
using UnityEngine;

public class HighlightSystem2 : MonoBehaviour
{
    [SerializeField] private GrandmaEventManager2 grandmaEventManager;
    [SerializeField] private float lostObjectTimeLimit = 5f;

    private MeshRenderer targetRenderer;
    private GameObject targetObject;

    private float lostTimer = 0f;

    private bool timeOver = false;
    private bool isHighlighting = false;

    void Update()
    {
        if (RequestCompleted2.getTarget || !grandmaEventManager.IsRequest()) return;

        lostTimer += Time.deltaTime;
        TimeCondition();
    }

    void TimeCondition()
    {
        if (lostTimer >= lostObjectTimeLimit)
        {
            if (timeOver) return;
            StartCoroutine(Highlight());
            Debug.Log("오브젝트를 너무 오래 찾지 못함");
            timeOver = true;
        }
    }

    public void ResetTimer()
    {
        lostTimer = 0f;
        timeOver = false;
    }

    // 추가 - load시 하이라이트 시스템에게 지금 깜빡여야 할 오브젝트를 다시 알려주는 역할
    public void PrepareTarget(string objectName)
    {
        targetObject = GameObject.Find(objectName);
        if (targetObject != null)
            Debug.Log("복원된 요청 대상 " + targetObject.name);
    }

    public void TargetObject()
    {
        targetObject = GameObject.Find(grandmaEventManager.GetObjectName());
        Debug.Log(targetObject);
    }

    private IEnumerator Highlight()
    {
        if (isHighlighting) yield break;
        if (targetObject == null)
        {
            Debug.LogWarning("Highlight 실패 대상 오브젝트가 존재하지 않습니다");
            yield break;
        }

        targetRenderer = targetObject.GetComponent<MeshRenderer>();
        if (targetRenderer == null)
        {
            Debug.LogWarning("Highlight 실패 MeshRenderer를 찾을 수 없습니다 " + targetObject.name);
            yield break;
        }

        isHighlighting = true;

        for (int i = 0; i < 2; i++)
        {
            targetRenderer.enabled = false;
            yield return new WaitForSeconds(0.5f);
            targetRenderer.enabled = true;
            yield return new WaitForSeconds(0.5f);
        }

        targetRenderer.enabled = true;
        isHighlighting = false;
    }
}