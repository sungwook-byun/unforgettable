using UnityEngine;

public class GrandmaUp : MonoBehaviour
{
    private bool up;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        if (up == true)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
    }

    public void Up()
    {
        up = true;
    }
}
