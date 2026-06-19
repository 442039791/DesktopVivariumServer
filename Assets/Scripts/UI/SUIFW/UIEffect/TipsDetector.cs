using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SUIFW;

/// <summary>
/// 全局Tips检测器 - 每帧检测鼠标下方是否有Tips触发器
/// 如果没有，则关闭所有已打开的Tips
/// </summary>
public class TipsDetector : MonoBehaviour
{
    private static TipsDetector instance;
    public static TipsDetector Instance => instance;

    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    // 缓存的Tips管理器引用
    private TipsManager tipsManager;
    private IconTipsManager iconTipsManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 每帧检测鼠标下方是否有Tips触发器
        CheckAndHideTips();
    }

    private void CheckAndHideTips()
    {
        if (EventSystem.current == null) return;

        // 检测鼠标是否在窗口内
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x < 0 || mousePos.x > Screen.width ||
            mousePos.y < 0 || mousePos.y > Screen.height)
        {
            // 鼠标在窗口外，关闭所有Tips
            HideAllTips();
            return;
        }

        if (pointerEventData == null)
        {
            pointerEventData = new PointerEventData(EventSystem.current);
        }
        pointerEventData.position = mousePos;

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        // 检查是否有TipsTrigger
        bool hasTipsTrigger = false;
        bool hasIconTipsTrigger = false;

        foreach (var result in raycastResults)
        {
            // 检查TipsTrigger
            var tipsTrigger = result.gameObject.GetComponentInParent<TipsTrigger>();
            if (tipsTrigger != null && tipsTrigger.enabled && tipsTrigger.gameObject.activeInHierarchy)
            {
                hasTipsTrigger = true;
            }

            // 检查IconTipsTrigger
            var iconTipsTrigger = result.gameObject.GetComponentInParent<IconTipsTrigger>();
            if (iconTipsTrigger != null && iconTipsTrigger.enabled && iconTipsTrigger.isEnabled && iconTipsTrigger.gameObject.activeInHierarchy)
            {
                hasIconTipsTrigger = true;
            }

            // 如果两种都找到了，可以提前退出
            if (hasTipsTrigger && hasIconTipsTrigger)
            {
                break;
            }
        }

        // 如果鼠标下方没有TipsTrigger，隐藏TipsManager
        if (!hasTipsTrigger)
        {
            HideTipsManager();
        }

        // 如果鼠标下方没有IconTipsTrigger，隐藏IconTipsManager
        if (!hasIconTipsTrigger)
        {
            HideIconTipsManager();
        }
    }

    private void HideAllTips()
    {
        HideTipsManager();
        HideIconTipsManager();
    }

    private void HideTipsManager()
    {
        if (tipsManager == null)
        {
            tipsManager = FindAnyObjectByType<TipsManager>();
        }

        if (tipsManager != null && tipsManager.gameObject.activeInHierarchy)
        {
            tipsManager.ForceHide();
        }
    }

    private void HideIconTipsManager()
    {
        if (iconTipsManager == null)
        {
            iconTipsManager = FindAnyObjectByType<IconTipsManager>();
        }

        if (iconTipsManager != null && iconTipsManager.gameObject.activeInHierarchy)
        {
            iconTipsManager.ForceHide();
        }
    }

    /// <summary>
    /// 注册TipsManager引用
    /// </summary>
    public void RegisterTipsManager(TipsManager manager)
    {
        tipsManager = manager;
    }

    /// <summary>
    /// 注册IconTipsManager引用
    /// </summary>
    public void RegisterIconTipsManager(IconTipsManager manager)
    {
        iconTipsManager = manager;
    }
}
