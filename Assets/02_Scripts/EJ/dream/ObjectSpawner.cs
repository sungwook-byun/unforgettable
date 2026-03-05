using System.Collections;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject targetObject;

    private float interval = 0.5f;

    private bool isSpawning = false;
    private Coroutine currentRoutine;

    private GameObject spawnedInstance;

    void Start()
    {
        isSpawning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSpawning) return;

        if (other.CompareTag("Player"))
        {
            //StartSequence(true);
            spawnedInstance = Instantiate(targetObject, transform);
            spawnedInstance.SetActive(true);
            isSpawning = true;
        }
    }

    /*private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartSequence(false);
        }
    }

    private void StartSequence(bool activate)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ActivateChildrenSequentially(activate));
    }

    private IEnumerator ActivateChildrenSequentially(bool activate)
    {
        isSpawning = true;

        if (spawnedInstance == null)
        {
            spawnedInstance = Instantiate(targetObject, transform);
            spawnedInstance.name = targetObject.name + "_Instance";
            spawnedInstance.transform.localPosition = Vector3.zero;
            spawnedInstance.transform.localRotation = Quaternion.identity;

            foreach (Transform child in spawnedInstance.transform)
                child.gameObject.SetActive(false);
        }

        int childCount = spawnedInstance.transform.childCount;

        if (activate)
        {
            // 순서대로 켜기
            for (int i = 0; i < childCount; i++)
            {
                Transform child = spawnedInstance.transform.GetChild(i);
                child.gameObject.SetActive(true);
                yield return new WaitForSeconds(interval);
            }
        }
        else
        {
            // 역순으로 끄기
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = spawnedInstance.transform.GetChild(i);
                child.gameObject.SetActive(false);
                yield return new WaitForSeconds(interval);
            }
        }

        isSpawning = false;
    }*/
}
