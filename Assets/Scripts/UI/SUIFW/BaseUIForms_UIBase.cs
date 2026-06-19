using UnityEngine;
using SUIFW;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class BaseUIForms_UIBase : BaseUIForms
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
    public override bool Init()
    {
        if (initcomplete)
        {
            return true;
        }
        initcomplete = true;

        // 扫描并注册所有本地化文本（自动处理所有"Id:xxx"格式的文本）
        LocalizationManager.ScanAndRegisterTextsStatic(transform);

        // ���� 1����ȡ RectTransform ���
        RectTransform targetRect = GetComponent<RectTransform>();

        // ���� 2�������洢�ĸ������������
        Vector3[] worldCorners = new Vector3[4];

        // ���� 3����ȡ����ռ�����꣨�Զ�������ת/����/ê��Ӱ�죩
        targetRect.GetWorldCorners(worldCorners);

        // ���� 4����ȡ���½����꣨Array Index 0��
        bottomLeftWorldPos = worldCorners[0];
        // ���� 5����ȡ���Ͻ����꣨Array Index 2��
        topRightWorldPos = worldCorners[2];
        //layoutGroup = GetComponent<VerticalLayoutGroup>();
        //int i = 0;

        // 添加空值检查，防止 UIForms 未配置导致崩溃
        if (UIForms == null || UIForms.Length == 0)
        {
            return base.Init();
        }

        foreach (var val in UIForms)
        {
            // 添加空值检查，防止组件未配置导致崩溃
            if (val == null || val.button == null)
            {
                continue;
            }

            val.isShow = true;
            val.button.AddDeskTopListener(() =>
            {
                val.isShow = !val.isShow;
                Show();
                ButtonClickEvent(ButtonClickObj);
            });
            // 添加空值检查，防止 listController 未初始化导致崩溃
            if (val.ListControllerExpandScroll && val.listController != null && scrollRect != null)
            {

                val.listController.InitExpandScroll(targetRect, scrollRect,0.01f);
                scrollRect.onValueChanged.AddListener(val.listController.OnScroll);
            }

            //IsActive.Add(button, true);
            //i++;
        }
        Show();
        //ButtonClickEvent(ButtonClickObj);
        return base.Init();
    }
    public void InitButtonClickEvent(ButtonClick buttonClickEvent, object obj)
    {
        ButtonClickEvent = buttonClickEvent;
        ButtonClickObj = obj;
    }
    public virtual void RefreshUI()
    {
        // 添加空值检查，防止 UIForms 未配置导致崩溃
        if (UIForms == null || UIForms.Length == 0)
        {
            return;
        }

        foreach (var val in UIForms)
        {
            // 添加空值检查，防止组件未配置导致崩溃
            if (val == null)
            {
                continue;
            }

            if (val.isShow)
            {
                // 添加空值检查，防止 listController 未初始化导致崩溃
                if (val.ListControllerExpandScroll && val.listController != null)
                {
                    //val.listController.InitExpandScroll(targetRect, scrollRect);
                    val.listController.UpdateVisibleItems();
                }
            }



            //IsActive.Add(button, true);
            //i++;
        }
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

        // 添加空值检查，防止 UIForms 未配置导致崩溃
        if (UIForms == null || UIForms.Length == 0)
        {
            return;
        }

        bool allNotShow = true;
        int v = 0;
        foreach (var val in UIForms)
        {
            // 添加空值检查，防止组件未配置导致崩溃
            if (val == null || val.button == null || val.content == null)
            {
                continue;
            }

            if (val.isShow)
            {
                // 添加空值检查，防止 listController 未初始化导致崩溃
                if (val.listController != null)
                {
                    v+= val.listController.ListCount == 0?0:1;
                    val.button.transform.eulerAngles = new Vector3(0, 0, 0);
                    var maxsize =-(bottomLeftWorldPos.y - (val.content.position.y));
                    maxsize = maxsize < MaxSize ? MaxSize : maxsize;
                    float bestsize = val.listController.GetBestMaxSize(maxsize);

                    val.content.sizeDelta = new Vector2(val.content.sizeDelta.x, bestsize);
                }
                else
                {
                    // listController 为 null，设置默认大小
                    val.button.transform.eulerAngles = new Vector3(0, 0, 0);
                    val.content.sizeDelta = new Vector2(val.content.sizeDelta.x, 0);
                }
            }
            else
            {
                val.button.transform.eulerAngles = new Vector3(0, 0, 90);
                val.content.sizeDelta  = new Vector2(val.content.sizeDelta.x, 0);
            }
            //if (isactivelist[i])
            //{
            //    rectTransform.sizeDelta = new Vector2(
            //        rectTransform.sizeDelta.x, // ԭ���Ȳ���
            //        size // �¸߶�
            //    );
            //}
            //else
            //{
            //    rectTransform.sizeDelta = new Vector2(
            //        rectTransform.sizeDelta.x, // ԭ���Ȳ���
            //        0 // �¸߶�
            //    );
            //}
            ////rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);

            //i++;
        }

        if (v == 0)
            allNotShow = true;
        else
            allNotShow = false;
        // 添加空值检查，防止 content/layoutGroup/childbase 未配置导致崩溃
        if (content != null && layoutGroup != null && childbase != null)
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
            height+= layoutGroup.spacing * (childbase.childCount - 1)+ layoutGroup.padding.top /*+ layoutGroup.padding.bottom*/;
            content.sizeDelta = new Vector2(content.sizeDelta.x, height);
        }
        // 添加空值检查，防止 scrollRect 未配置导致崩溃
        if (scrollRect != null)
        {
            if (allNotShow)
            {
                scrollRect.enabled = false;
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }
            else
            {
                scrollRect.enabled = true;
            }
        }
    }
}
[System.Serializable]
public class UIFormInfo
{
    public Button button;
    public ListController listController;
    public RectTransform content;
    public bool isShow = false;
    public bool ListControllerExpandScroll= false;
}