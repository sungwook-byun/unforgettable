using UnityEngine;

public class DayFour : MonoBehaviour
{
    [SerializeField] private GameObject Object;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Object.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Object.SetActive(false);
        }
    }
}
