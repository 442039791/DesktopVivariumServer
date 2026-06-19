/*****************************************************************
 * --@ FileName: UI_IllustratedGuide_Item
 * --@ Description: 图鉴列表项组件，显示单个图鉴条目
 * --@ Author: paofan25, alivecn2@gmail.com
 * --@ Copyright: Copyright (c) 2025, paofan25
 ******************************************************************/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_IllustratedGuide_Item : MonoBehaviour, IShowAtUI
{
    [Header("UI组件")]
    public Image Icon;
    public Image Bg;
    public Button itemButton;

    [Header("状态")]
    public bool OnShow { get; set; } = false;

    private IlluData currentData;
    private bool isInitialized = false;

    /// <summary>
    /// 初始化图鉴条目
    /// </summary>
    public void Init(IlluData data)
    {
        itemButton = GetComponent<Button>();

        // 检查并自动获取组件
        if (Icon == null)
        {
            Icon = transform.Find("Icon")?.GetComponent<Image>();
            if (Icon == null)
            {
                Debug.LogWarning($"[图鉴条目] 未找到Icon组件，尝试在子对象中查找");
                Icon = GetComponentInChildren<Image>();
            }
        }

        if (Bg == null)
        {
            Bg = transform.Find("Bg")?.GetComponent<Image>();
            if (Bg == null)
            {
                Debug.LogWarning($"[图鉴条目] 未找到Bg组件，尝试获取第二个Image组件");
                var images = GetComponentsInChildren<Image>();
                if (images.Length > 1)
                {
                    Bg = images[1]; // 假设第二个Image是背景
                }
            }
        }

        currentData = data;

        UpdateUI();

        // 自动添加按钮点击事件
        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(OnItemClick);
        }
        else
        {
            Debug.LogWarning("[图鉴条目] 按钮组件为空，无法添加点击事件");
        }

        isInitialized = true;
        OnShow = true;
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (currentData == null)
        {
            Debug.LogWarning("[图鉴条目] 当前数据为空，无法更新UI");
            return;
        }

        // 更新图标 - 使用try-catch确保不会阻断后续更新
        try
        {
            UpdateIcon();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[图鉴条目] 更新图标时出错: {e.Message}");
        }

        // 更新背景 - 使用try-catch确保不会阻断
        try
        {
            UpdateBackground();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[图鉴条目] 更新背景时出错: {e.Message}");
        }
    }

    /// <summary>
    /// 更新图标
    /// </summary>
    private void UpdateIcon()
    {
        if (Icon == null)
        {
            Debug.LogWarning($"[图鉴条目] Icon组件为空，跳过图标更新: {currentData?.name ?? "null"}");
            return;
        }

        if (currentData.isUnlocked)
        {
            // 已解锁，显示实际图标
            if (currentData.GetIcon() != null)
            {
                Icon.sprite = currentData.GetIcon();
                Icon.color = Color.white;
            }
            else
            {
                // 直接使用GetIcon获取图标
                Icon.sprite = currentData.GetIcon();
                Icon.color = Color.white;
            }
        }
        else
        {
            // 未解锁，显示未知图标
            Icon.sprite = CoreSetting.Instance.UI_UnKnow_sprite;
            Icon.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        }
    }

    /// <summary>
    /// 更新背景
    /// </summary>
    private void UpdateBackground()
    {
        if (Bg == null)
        {
            Debug.LogWarning($"[图鉴条目] 背景组件为空，跳过背景更新: {currentData?.name ?? "null"}");
            return;
        }

        // 根据解锁状态设置背景颜色
        if (currentData.isUnlocked)
        {
            // 已解锁，使用正常背景
            Bg.color = Color.white;
        }
        else
        {
            // 未解锁，使用灰色背景
            Bg.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        }
    }

    /// <summary>
    /// 异步加载图标的协程
    /// </summary>
    private IEnumerator LoadIconCoroutine(AssetBundles.BaseAssetAsyncLoader loader)
    {
        // 等待加载完成
        while (!loader.isDone)
        {
            yield return null;
        }

        // 加载完成后处理
        if (loader.asset is Sprite sprite)
        {
            // 不再需要手动设置缓存，请使用CacheSprites()
            Icon.sprite = sprite;
            Icon.color = Color.white;
        }
        else
        {
            Debug.LogError($"[图鉴条目] 加载的资源不是Sprite类型: {loader.asset?.GetType()}");
        }
    }

    /// <summary>
    /// 条目点击事件
    /// </summary>
    private void OnItemClick()
    {
        if (currentData == null)
        {
            Debug.LogWarning("[图鉴条目] 当前数据为空，无法处理点击事件");
            return;
        }

        // 如果已解锁，显示详细信息
        if (currentData.isUnlocked)
        {
            ShowDetailInfo();
        }
        else
        {
            // 如果未解锁，显示解锁提示
            ShowUnlockHint();
        }
    }

    /// <summary>
    /// 显示详细信息
    /// </summary>
    private void ShowDetailInfo()
    {
        // 触发显示详细信息事件
        event_manager.instance.dispatch_UIevent("IllustratedGuide_ShowDetail", currentData);
    }

    /// <summary>
    /// 显示解锁提示
    /// </summary>
    private void ShowUnlockHint()
    {
        // 触发显示解锁提示事件
        event_manager.instance.dispatch_UIevent("IllustratedGuide_ShowUnlockHint", currentData);
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    public void Refresh()
    {
        if (isInitialized && currentData != null)
        {
            UpdateUI();
        }
        else
        {
            Debug.LogWarning($"[图鉴条目] 刷新条件不满足: 已初始化={isInitialized}, 数据存在={currentData != null}");
        }
    }

    /// <summary>
    /// 获取当前数据
    /// </summary>
    public IlluData GetCurrentData()
    {
        return currentData;
    }
}
