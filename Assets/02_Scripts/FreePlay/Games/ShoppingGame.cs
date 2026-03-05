using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class ShoppingProduct
{
    public int id;
    public int price;
    public Button productButton;
}

public class ShoppingGame : MiniGameBase
{
    [SerializeField] ShoppingProduct[] shoppingProducts;
    [SerializeField] Transform playerCartTR;
    List<ShoppingProduct> playerCart = new List<ShoppingProduct>();
    HashSet<ShoppingProduct> requiredHashSet = new HashSet<ShoppingProduct>();
    int requiredSize = 4;
    int leftRequired;
    ShoppingProduct[] array;

    [SerializeField] Button finishButton;
    [SerializeField] Image[] requiredImages;

    [SerializeField] RectTransform scrollContent;
    [SerializeField] float scrollContentX;
    [SerializeField] Button leftButton, rightButton;

    bool playing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 버튼 이벤트 연결
        for(int i = 0; i < shoppingProducts.Length; i++)
        {
            int index = i;
            shoppingProducts[i].productButton.onClick.AddListener(() => OnClickProductButton(index));
            shoppingProducts[i].productButton.GetComponentInChildren<TextMeshProUGUI>().text = shoppingProducts[i].price + "원";
        }
        finishButton.onClick.AddListener(OnClickFinishButton);
        leftButton.onClick.AddListener(() => OnClickArrowButton(-1));
        rightButton.onClick.AddListener(() => OnClickArrowButton(1));

        // 필수 구매상품 랜덤으로 뽑기
        leftRequired = requiredSize;
        int it = 0;
        while (requiredHashSet.Count < requiredSize)
        {
            requiredHashSet.Add(shoppingProducts[Random.Range(0, shoppingProducts.Length)]);
            it++;
            if (it > 100) break;
        }
        array = requiredHashSet.ToArray();
        for (int i = 0; i < array.Length; i++) requiredImages[i].sprite = array[i].productButton.image.sprite;

        OnFinished += HandleFinish;
    }

    public override void Begin()
    {
        playing = true;
    }

    void OnClickProductButton(int index)
    {
        if (!playing) return;

        ShoppingProduct product = shoppingProducts[index];
        playerCart.Add(product);
        product.productButton.gameObject.SetActive(false);

        // 고스트 생성
        SpriteRenderer ghostProduct = new GameObject("ghost").AddComponent<SpriteRenderer>();
        ghostProduct.sprite = product.productButton.image.sprite;
        Vector3 worldPos = GetUIWorldPosition(product.productButton.GetComponent<RectTransform>(), Camera.main);
        ghostProduct.transform.position = worldPos;
        ghostProduct.transform.DOMove(playerCartTR.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-.5f, .5f), 0), 0.5f).SetTarget(this);

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == product)
            {
                requiredImages[i].gameObject.SetActive(false);
                leftRequired--;
                return;
            }
        }
    }

    Vector3 GetUIWorldPosition(RectTransform rect, Camera cam, float zOffset = 5f)
    {
        Vector3 screenPos = rect.position;
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zOffset));
        Debug.Log(worldPos);
        return worldPos;
    }

    void OnClickArrowButton(int dir)
    {
        if (dir == 1)
        {
            scrollContent.DOAnchorPosX(scrollContentX * -1, 0.5f).SetTarget(this);
        }
        else if (dir == -1)
        {
            scrollContent.DOAnchorPosX(scrollContentX, 0.5f).SetTarget(this);
        }
    }

    void OnClickFinishButton()
    {
        if (!playing) return;

        // 필수 항목 검사
        if(leftRequired > 0)
        {
            Debug.Log("아직 담지 않은 상품이 있어요!");
            return;
        }

        CheckCart();

    }

    void CheckCart()
    {
        playing = false;

        // 금액 검사
        int total = 0;
        for (int i = 0; i < playerCart.Count; i++)
        {
            total += playerCart[i].price;
        }

        if (total == 10000)
        {
            Debug.Log("만원 맞추기 성공");
            score.Add((int)timer.Remaining * playerCart.Count);
            clear = true;
        }
        else Debug.Log("실패: " + total);

        Finish();
    }

    void HandleFinish(int score, bool clear)
    {
        CheckCart();
    }

    private void OnDestroy()
    {
        OnFinished -= HandleFinish;
        DOTween.Kill(this);
    }
}
