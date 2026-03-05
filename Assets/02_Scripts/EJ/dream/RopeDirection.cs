using UnityEngine;

public class RopeDirection : MonoBehaviour
{
    [SerializeField] private int dir = 0; // 0: 정면, 1: 오, 2: 왼, 3: 수평

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dir = RopeDuplicator.direction;
        }
    }
}
