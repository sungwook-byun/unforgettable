using UnityEngine;
using DG.Tweening;

public class RunnerScroll : MonoBehaviour
{
    [SerializeField] float randomY;
    [SerializeField] bool rotating, autoSpawning;
    [SerializeField] float scrollSpeed;
    RunnerGame game;
    Vector3 spawnPos;
    Tween rotTween;
    bool added;

    private void Awake()
    {
        spawnPos = transform.position;
    }

    private void OnEnable()
    {
        if (spawnPos != Vector3.zero) transform.position = spawnPos + new Vector3(0, Random.Range(0, randomY), 0);

        if (rotating)
        {
            added = false;

            rotTween?.Kill();
            transform.localRotation = Quaternion.identity;
            rotTween = transform.DOLocalRotate(new Vector3(0, 0, 360f), 1f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).SetLoops(-1);
        }

    }

    private void OnDisable()
    {
        if (rotating)
        {
            rotTween?.Kill();
            rotTween = null;
        }
    }

    public void Init(RunnerGame rg)
    {
        game = rg;
    }

    void Update()
    {
        // 게임이 진행 중일 때만 이동
        if (game == null || !game.IsActive()) return;

        float dt = Time.deltaTime;

        // 왼쪽으로 이동
        transform.position += Vector3.left * ((scrollSpeed == 0 ? game.GetScrollSpeed() : scrollSpeed) * dt);


        if (transform.position.x < game.GetKillX())
        {
            if (autoSpawning)
            {
                if (spawnPos != Vector3.zero) transform.position = spawnPos + new Vector3(0, Random.Range(0, randomY), 0);
            }
            else
            {
                game.Despawn(this);
            }
        }

        if (rotating && !added && transform.position.x < game.GetPlayerX())
        {
            added = true;
            game.AddScore();
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (!game.IsActive()) return;

        if (other.CompareTag("Player"))
        {
            game.OnPlayerHitObstacle();
        }
    }
}
