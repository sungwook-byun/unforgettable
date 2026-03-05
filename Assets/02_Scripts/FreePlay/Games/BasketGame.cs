using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PixelCrushers.DialogueSystem.UnityGUI.GUIProgressBar;

public class BasketGame : MiniGameBase
{
    [Header("Player")]
    [SerializeField] Transform player_L;
    [SerializeField] Transform player_R;
    [SerializeField] GameObject snowman_L, snowman_R, snow_L, snow_R;
    float moveSpeed = 9f;
    Rigidbody2D rigid_L, rigid_R;
    Vector2 inputDir_L, inputDir_R;
    SpriteRenderer sr_L, sr_R;
    Animator anim_L, anim_R;

    bool playing;

    [Header("DropItem")]
    [SerializeField] BasketDropItem itemPrefab;
    [SerializeField] Vector2 dropX;
    float dropY = 5;
    List<BasketDropItem> itemPool = new List<BasketDropItem>();
    Vector2[] delayPerLevel = { new Vector2(3f, 5f), new Vector2(2f, 4f), new Vector2(1f, 3f) };
    Vector2 delay;

    public override void Begin()
    {
        playing = true;
        clear = true;
        delay = delayPerLevel[MiniGameContext.gameLevel];
        SpawnItem();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //rigid = player.GetComponent<Rigidbody2D>();
        //sr= player.GetComponent<SpriteRenderer>();
        rigid_L = player_L.GetComponent<Rigidbody2D>();
        rigid_R = player_R.GetComponent<Rigidbody2D>();
        sr_L = player_L.GetComponent<SpriteRenderer>();
        sr_R = player_R.GetComponent<SpriteRenderer>();
        anim_L = player_L.GetComponentInChildren<Animator>();
        anim_R = player_R.GetComponentInChildren<Animator>();

        OnFinished += HandleFinish;
    }

    private void OnDestroy()
    {
        OnFinished -= HandleFinish;
    }

    private void Update()
    {
        if (!playing) return;

        inputDir_L = new Vector2(input.Move_WSAD.x, 0);
        inputDir_R = new Vector2(input.Move_Arrow.x, 0);
    }

    void FixedUpdate()
    {
        if (!playing || timer.Paused || !timer.Running) return;

        // 이동
        //Vector2 target = rigid.position + inputDir * moveSpeed * Time.fixedDeltaTime;
        //rigid.MovePosition(target);

        Vector2 target_L = rigid_L.position + inputDir_L * moveSpeed * Time.fixedDeltaTime;
        rigid_L.MovePosition(target_L);

        Vector2 target_R = rigid_R.position + inputDir_R * moveSpeed * Time.fixedDeltaTime;
        rigid_R.MovePosition(target_R);

        // 애니메이션 제어
        //bool isMoving = inputDir.sqrMagnitude > 0.001f;
        //anim_L.SetBool("isMoving", inputDir_L.sqrMagnitude > 0.001f);
        //anim_R.SetBool("isMoving", inputDir_R.sqrMagnitude > 0.001f);
        anim_L.SetFloat("dirX", inputDir_L.x);
        anim_R.SetFloat("dirX", inputDir_R.x);
        //anim.SetBool("isMoving", isMoving);
        sr_L.flipX = inputDir_L.x < 0;
        sr_R.flipX = inputDir_R.x < 0;
    }

    void SpawnItem()
    {
        BasketDropItem item;
        if (itemPool.Count > 0)
        {
            item = itemPool[0];
            itemPool.RemoveAt(0);
            item.transform.position = new Vector2(Random.Range(dropX.x, dropX.y), dropY);
            item.gameObject.SetActive(true);

        }
        else
        {
            item = Instantiate(itemPrefab, new Vector2(Random.Range(dropX.x, dropX.y), dropY), Quaternion.identity);
            item.Init(this);
        }

        Invoke(nameof(SpawnItem), Random.Range(delay.x, delay.y));
    }

    public void EndGame()
    {
        if (!playing) return;
        playing = false;
        clear = false;
        CancelInvoke();
        Finish();
    }

    public void AddScore(BasketDropItem item)
    {
        item.gameObject.SetActive(false);
        itemPool.Add(item);
        
        score.Add(50, false, true);
    }

    void HandleFinish(int score, bool clear)
    {
        playing = false; 
        CancelInvoke();
        if (clear)
        {
            anim_L.SetTrigger("doSuccess");
            anim_R.SetTrigger("doSuccess");
        }
        else
        {
            snowman_L.SetActive(false); snowman_R.SetActive(false);
            snow_L.SetActive(true); snow_R.SetActive(true);
        }
    }
}


