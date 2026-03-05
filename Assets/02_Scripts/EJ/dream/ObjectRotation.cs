using UnityEngine;

public class ObjectRotation : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(90f, 0f, 0f);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}