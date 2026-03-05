using UnityEngine;

public class RequestCompleted2 : MonoBehaviour
{
    [SerializeField] private GrandmaEventManager2 grandmaEventManager;
    [SerializeField] private Transform requestTransform;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactDistance = 5f;

    public static bool getTarget = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject target = hit.collider.gameObject;
                DistanceCheck(target);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (getTarget)
            {
                GameObject grandma = GameObject.FindGameObjectWithTag("Grandma");
                DistanceCheck(grandma);
            }
            else
            {
                GameObject target = GameObject.Find(grandmaEventManager.GetObjectName());
                DistanceCheck(target);
            }
        }
    }

    void DistanceCheck(GameObject obj)
    {
        if (obj == null) return;

        float distance = Vector3.Distance(playerTransform.position, obj.transform.position);
        if (distance <= interactDistance)
            TryInteract(obj);
    }

    void TryInteract(GameObject target)
    {
        // 오브젝트 클릭 시 집기 처리
        if (!getTarget && !target.CompareTag("Grandma"))
        {
            Debug.Log(target.name + " 클릭");

            if (grandmaEventManager.IsRequest() && target.name == grandmaEventManager.GetObjectName())
                PickUpItem(target);
        }
        // 할머니와 상호작용 시 전달 처리
        else if (target.CompareTag("Grandma"))
        {
            DropItem();
        }
    }

    void PickUpItem(GameObject target)
    {
        if (grandmaEventManager.GetPickUp())
        {
            Debug.Log("물건을 들기 시작함");
            target.layer = LayerMask.NameToLayer("Ignore Raycast");
            target.transform.position = requestTransform.position;
            target.transform.SetParent(requestTransform);
        }

        getTarget = true;
    }

    void DropItem()
    {
        // 미션 완료는 GrandmaEventManager2에서 처리
        grandmaEventManager.CompleteRequest();
        getTarget = false;

        // 손에 들고 있던 물건 비활성화 및 저장 기록
        if (requestTransform.childCount > 0)
        {
            for (int i = 0; i < Mathf.Min(2, requestTransform.childCount); i++)
            {
                var child = requestTransform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    SaveManager.Instance.MarkObjectRemoved(child.gameObject.name);
                    child.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }
}