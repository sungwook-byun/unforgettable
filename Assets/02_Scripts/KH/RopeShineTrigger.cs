using UnityEngine;

public class RopeShineTrigger : MonoBehaviour
{
    [SerializeField] private GameObject rope;
    
    private void OnTriggerEnter(Collider other)
    {
        rope.GetComponent<RopeShineEffect>().ShineRope();
    }
}
