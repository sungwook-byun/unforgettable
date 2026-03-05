using UnityEngine;

public class ArriveSoon : MonoBehaviour
{
    [SerializeField] private RopeDuplicator ropeDuplicator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ropeDuplicator.StopSpawning();
        }
    }
}
