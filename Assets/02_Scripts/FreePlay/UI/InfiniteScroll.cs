
//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections.Generic;

//public class InfiniteScroll : MonoBehaviour
//{


//    ScrollRect scrollRect;
//    RectTransform viewPortRT, contentRT;
//    HorizontalLayoutGroup layout;

//    [SerializeField] RectTransform[] itemList;

//    Vector2 oldVelocity;
//    bool isUpdated;

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        scrollRect = GetComponentInChildren<ScrollRect>();
//        viewPortRT = scrollRect.viewport;
//        contentRT = scrollRect.content;
//        layout = GetComponentInChildren<HorizontalLayoutGroup>();

//        int itemsToAdd = Mathf.CeilToInt(viewPortRT.rect.width / (itemList[0].rect.width + layout.spacing));

//        for (int i = 0; i < itemsToAdd; i++)
//        {
//            RectTransform rt = Instantiate(itemList[i % itemList.Length], contentRT);
//            rt.SetAsLastSibling();
//        }

//        for (int i = 0; i < itemsToAdd; i++)
//        {
//            int num = itemList.Length - i - 1;
//            while (num < 0)
//            {
//                num += itemList.Length;
//            }
//            RectTransform rt = Instantiate(itemList[num], contentRT);
//            rt.SetAsFirstSibling();
//        }

//        contentRT.localPosition = new Vector3((0 - (itemList[0].rect.width + layout.spacing) * itemsToAdd), contentRT.localPosition.y, contentRT.localPosition.z);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (isUpdated)
//        {
//            isUpdated = false;
//            scrollRect.velocity = oldVelocity;
//        }

//        if (contentRT.localPosition.x > 0)
//        {
//            Canvas.ForceUpdateCanvases();
//            oldVelocity = scrollRect.velocity;
//            contentRT.localPosition -= new Vector3(itemList.Length * (itemList[0].rect.width + layout.spacing), 0, 0);
//            isUpdated = true;
//        }

//        if (contentRT.localPosition.x < 0 - (itemList.Length * (itemList[0].rect.width + layout.spacing)))
//        {
//            Canvas.ForceUpdateCanvases();
//            oldVelocity = scrollRect.velocity;
//            contentRT.localPosition += new Vector3(itemList.Length * (itemList[0].rect.width + layout.spacing), 0, 0);
//            isUpdated = true;
//        }
//    }
//}
