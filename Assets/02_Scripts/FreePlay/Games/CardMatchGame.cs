using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Coffee.UIExtensions;

public class CardMatchGame : MiniGameBase
{
    class Card
    {
        public int value;
        public Button button;
    }

    [SerializeField] Sprite backSprite;
    [SerializeField] Sprite[] cardSprites;
    [SerializeField] Button[] cardButtons;
    [SerializeField] UIParticle[] matchParticles;
    [SerializeField] AudioClip flipClip;

    Card[] cards;
    Card prevCard;

    float cardRotateTime = 0.1f;
    float cardFlipTime = 0.2f;

    int totalPairs, matchedPairs;
    int[] cardsPerLevel = { 12, 18, 24 };
    bool playing;

    public override void Setup(Score s, TimerController t, InputReader i)
    {
        base.Setup(s, t, i);
        SetCards();

        ShowCardImages(true);

        totalPairs = cards.Length / 2;
        matchedPairs = 0;
    }

    public override void Begin()
    {
        ShowCardImages(false);
        playing = true;
    }

    int[] RandomPair()
    {
        int[] arr = new int[cardsPerLevel[MiniGameContext.gameLevel]];
        int index = 0;

        // 0~n까지 두 번씩 넣기
        for (int i = 0; i < arr.Length / 2; i++)
        {
            arr[index++] = i;
            arr[index++] = i;
        }

        // Fisher-Yates 셔플로 랜덤 섞기
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = arr[i];
            arr[i] = arr[randomIndex];
            arr[randomIndex] = temp;
        }
        return arr;
    }

    void SetCards()
    {
        int[] cardValues = RandomPair();
        cards = new Card[cardsPerLevel[MiniGameContext.gameLevel]];
        for(int i = 0; i < cardButtons.Length; i++)
        {
            if(i >= cardsPerLevel[MiniGameContext.gameLevel])
            {
                break;
            }
            Card newCard = new Card();
            newCard.value = cardValues[i];
            newCard.button = cardButtons[i];
            newCard.button.onClick.AddListener(() => CheckCard(newCard));
            newCard.button.gameObject.SetActive(true);
            cards[i] = newCard;
        }
    }

    // 카드 뒤집기
    void CheckCard(Card card)
    {
        if (!playing) return;           // 플레이 중 아닐 때
        if (card == prevCard) return;   // 같은 카드를 다시 선택

        playing = false;

        AudioManager.Instance.PlaySFX(flipClip);
        // 카드 회전 트윈
        Sequence seq = DOTween.Sequence();
        seq.Append(card.button.transform.DORotate(new Vector3(0, 0, Random.Range(-20f, 20f)), cardRotateTime).SetEase(Ease.Linear));
        seq.Append(card.button.transform.DORotate(new Vector3(0, 180, 0), cardFlipTime).SetEase(Ease.Linear));
        //seq.Append(card.button.transform.DOLocalRotate(new Vector3(0, 180, 0), cardFlipTime, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
        //seq.Join(card.button.transform.DOBlendableLocalRotateBy(new Vector3(0, 0, Random.Range(-15f, 15f)), 0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo));
        seq.InsertCallback(cardFlipTime * 0.5f + cardRotateTime, () =>
        {
            card.button.image.sprite = cardSprites[card.value];
        });
        seq.OnComplete(() =>
        {
            if (prevCard == null) { prevCard = card; playing = true; }
            else CheckPair(card);
        });
    }

    // 카드 쌍 확인
    void CheckPair(Card card)
    {
        if (card.value == prevCard.value)
        {
            card.button.interactable = false;
            prevCard.button.interactable = false;
            matchParticles[0].GetComponent<RectTransform>().position = card.button.GetComponent<RectTransform>().position;
            matchParticles[0].Play();
            matchParticles[1].GetComponent<RectTransform>().position = prevCard.button.GetComponent<RectTransform>().position;
            matchParticles[1].Play();
            prevCard = null;
            score.Add(100, true, true);
            matchedPairs++;

            if(matchedPairs >= totalPairs)
            {
                // 남은 시간 보너스 점수 추가
                score.Add((int)(timer.Remaining * 10));
                clear = true;
                Finish();
            }
            else
            {
                playing = true;
            }
        }
        else 
        {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(cardFlipTime);
            seq.Append(card.button.transform.DORotate(Vector3.zero, cardFlipTime).SetEase(Ease.Linear));
            seq.Join(prevCard.button.transform.DORotate(Vector3.zero, cardFlipTime).SetEase(Ease.Linear));
            seq.InsertCallback(cardFlipTime * 1.5f, () =>
            {
                card.button.image.sprite = backSprite;
                prevCard.button.image.sprite = backSprite;
            });
            seq.OnComplete(() =>
            {
                prevCard = null; 
                score.Miss();
                playing = true;
            });
        }
    }


    void ShowCardImages(bool show = true)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            Sequence seq = DOTween.Sequence();
            seq.SetTarget(this);
            seq.Append(card.button.transform.DORotate(show ? new Vector3(0, 180, 0) : Vector3.zero, cardFlipTime).SetEase(Ease.Linear));
            seq.InsertCallback(cardFlipTime * 0.5f, () =>
            {
                card.button.image.sprite = show ? cardSprites[card.value] : backSprite;
            });
        }
    }

    void Start()
    {
        OnFinished += HandleFinish;
        for (int i = 0; i < cardButtons.Length; i++) cardButtons[i].gameObject.SetActive(false);
    }

    void HandleFinish(int score, bool clear)
    {
        playing = false;
    }

    private void OnDestroy()
    {
        OnFinished -= HandleFinish;
        DOTween.Kill(this);
    }
}
