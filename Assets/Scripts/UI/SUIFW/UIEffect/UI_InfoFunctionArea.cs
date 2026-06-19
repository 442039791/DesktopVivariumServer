using Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_InfoFunctionArea : MonoBehaviour,IChangeLanuge
{
    protected bool InitComplete = false;
    public RectTransform container; // 容器 RectTransform
    public Dictionary<string, RectTransform> rects = new(); // 要排序的矩形列表
    public virtual void Init()
    {
        if (!InitComplete)
        {
            container??= GetComponent<RectTransform>();
            event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
        }
    }
    public virtual void ChangeLanuge(string name, object udata)
    {
        throw new System.NotImplementedException();
    }

    protected void ActionButton(string name, object udata)
    {
        if ((bool)udata)
        {
            Button button = rects[name].gameObject.GetComponent<Button>();
            DesktopButtonManager.instance.ButtonShowTrue(button);
        }
        else
        {
            Button button = rects[name].gameObject.GetComponent<Button>();
            DesktopButtonManager.instance.ButtonShowFalse(button);
        }
        SortRects();
    }
    protected void SortRects()
    {
        // 容器的宽度和高度
        float containerWidth = container.rect.width;
        float containerHeight = container.rect.height;

        // 初始位置
        bool first = false;
        float currentX = -containerWidth/2;
        float currentY = -containerHeight/2;
        float rowHeight = 0;
        
        string text  = string.Empty;

        // 遍历所有矩形
        foreach (var rect in rects.Values)
        {
            GameObject rectGO = rect.gameObject;

            // 判断物体是否启用并且在层级中
            bool isVisible = rectGO.activeInHierarchy;

            // 当前矩形的宽度和高度
            float rectWidth = rect.rect.width;
            float rectHeight = rect.rect.height;
            if(!first)
            {
                currentX += rect.rect.width / 2;
                currentY += rect.rect.height / 2;
                first = true;
            }
            if (isVisible)
            {
                // 如果当前矩形放不下，则换行
                if (currentX + rectWidth / 2 > containerWidth / 2)
                {
                    // 换行，重置 X 为 0，Y 增加
                    currentX = -containerWidth / 2+ rect.rect.width / 2;
                    currentY += rowHeight;
                    rowHeight = rectHeight; // 更新当前行的高度
                }
                else
                {
                    // 更新当前行的最大高度
                    rowHeight = Mathf.Max(rowHeight, rectHeight);
                }

                // 设置矩形的位置
                rect.transform.localPosition = new Vector2(currentX, -currentY);

                // 更新当前行的 X 位置
                currentX += rectWidth;
            }
        }
    }
}
