using UnityEngine;

public class BasketDropItem : MonoBehaviour
{
    [SerializeField]
    GameObject[] models;
    BasketGame basketGame;
    Rigidbody2D rigid;
    string playerTag;

    private void OnEnable()
    {
        if(rigid == null) rigid = GetComponent<Rigidbody2D>();

        rigid.linearVelocity= Vector3.zero;

        int ran = Random.Range(0, 2);
        if (ran == 0) playerTag = "Player_L";
        else playerTag = "Player_R";
        models[0].SetActive(ran == 0);
        models[1].SetActive(ran == 1);
    }

    public void Init(BasketGame bg)
    {
        basketGame = bg;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == playerTag)
        {
            basketGame.AddScore(this);
        }
        else if(collision.tag == "Finish")
        {
            basketGame.EndGame();
        }
    }
}