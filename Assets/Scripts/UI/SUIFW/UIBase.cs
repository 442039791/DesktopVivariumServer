using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.HableCurve;

public class UIBase : MonoBehaviour
{
    [HideInInspector]
    public bool OnShow = false;
    public UIFormInfo[] UIForms;
    //public Button[] Buttons;
    public float MaxSize = 350f;
    public RectTransform Size;
    public RectTransform content;
    public Transform childbase;
    //public Dictionary<Button, bool> IsActive = new Dictionary<Button, bool>();
    public delegate void ButtonClick(object obj);
    public ButtonClick ButtonClickEvent;
    public object ButtonClickObj;
    public bool initcomplete = false;
    public VerticalLayoutGroup layoutGroup;
    public bool ListControllerExpandScroll;
    public ScrollRect scrollRect;
    protected Vector3 bottomLeftWorldPos;
    protected Vector3 topRightWorldPos;
    public virtual bool Init()
    {
        if (initcomplete)
        {
            return true;
        }

        // 自动应用当前语言的字体到所有文本组件
        LocalizationManager.ApplyCurrentFontToUI(transform);

        initcomplete = true;
        // ???? 1????? RectTransform ???
        RectTransform targetRect = GetComponent<RectTransform>();

        // ???? 2??????????????????????
        Vector3[] worldCorners = new Vector3[4];

        // ???? 3??????????????????????????/????/ê?????
        targetRect.GetWorldCorners(worldCorners);

        // ???? 4??????????????Array Index 0??
        bottomLeftWorldPos = worldCorners[0];
        // ???? 5??????????????Array Index 2??
        topRightWorldPos = worldCorners[2];
        //layoutGroup = GetComponent<VerticalLayoutGroup>();
        //int i = 0;
        foreach (var val in UIForms)
        {
            val.isShow = true;
            val.button.AddDeskTopListener(() =>
            {
                val.isShow = !val.isShow;
                Show();
                ButtonClickEvent(ButtonClickObj);
            });
            if (val.ListControllerExpandScroll)
            {
                val.listController.InitExpandScroll(targetRect, scrollRect, 0.001f);
                scrollRect.onValueChanged.AddListener(val.listController.OnScroll);
            }

            //IsActive.Add(button, true);
            //i++;
        }
        //Show();
        //ButtonClickEvent(ButtonClickObj);
        return true;
    }
    public void InitButtonClickEvent(ButtonClick buttonClickEvent, object obj)
    {
        ButtonClickEvent = buttonClickEvent;
        ButtonClickObj = obj;
    }
    public void Show()
    {
        //int y = 0;
        //foreach (var b in IsActive)
        //{
        //    if (b.Value)
        //    {
        //        y++;
        //    }
        //}
        //float size = MaxSize / y;
        //int i = 0;
        //var isactivelist = IsActive.Values.ToList();
        bool allNotShow = true;
        int v = 0;
        foreach (var val in UIForms)
        {
            if (val.isShow)
            {
                v += (val.listController.ListCount == 0 ? 0 : 1);
                val.button.transform.eulerAngles = new Vector3(0, 0, 0);
                var maxsize = -(bottomLeftWorldPos.y - (val.content.position.y));
                maxsize = maxsize < MaxSize ? MaxSize : maxsize;
                float bestsize = val.listController.GetBestMaxSize(maxsize);

                val.content.sizeDelta = new Vector2(val.content.sizeDelta.x, bestsize);
            }
            else
            {
                val.button.transform.eulerAngles = new Vector3(0, 0, 90);
                val.content.sizeDelta = new Vector2(val.content.sizeDelta.x, 0);
            }
           
        }

        if (v == 0)
        {
            allNotShow = true;
        }
        else
        {
            allNotShow = false;
        }
        if (content != null)
        {
            float height = 0;
            for (int i = 0; i < childbase.childCount; i++)
            {
                Transform child = childbase.GetChild(i);
                if (child.TryGetComponent(out RectTransform rect))
                {
                    height += rect.rect.height;
                }
            }
            //foreach (Transform val in content.transform.)
            //{
            //    height += val.GetComponent<RectTransform>().rect.height;
            //}
            height += layoutGroup.spacing * (childbase.childCount - 1) + layoutGroup.padding.top /*+ layoutGroup.padding.bottom*/;
            content.sizeDelta = new Vector2(content.sizeDelta.x, height);
        }
        if (allNotShow)
        {
            if (scrollRect != null)
            {
                scrollRect.enabled = false;
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }

        }
        else
        {
            scrollRect.enabled = true;
        }
        //int y = 0;
        //foreach(var b in IsActive)
        //{
        //    if (b.Value)
        //    {
        //        y++;
        //    }
        //}
        //float size = MaxSize / y;
        //int i = 0;
        //var isactivelist = IsActive.Values.ToList();
        //foreach (var rectTransform in RectTransforms)
        //{
        //    if (isactivelist[i])
        //    {
        //        rectTransform.sizeDelta = new Vector2(
        //            rectTransform.sizeDelta.x, // ????????
        //            size // ????
        //        );
        //    }
        //    else
        //    {
        //        rectTransform.sizeDelta = new Vector2(
        //            rectTransform.sizeDelta.x, // ????????
        //            0 // ????
        //        );
        //    }
        //    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);

        //    i++;
        //}
    }
}
