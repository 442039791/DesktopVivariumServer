/*****************************************************************
 * --@ FileName: UI_IllusrtatedGuide_List
 * --@ Description: 图鉴列表管理，自己读取数据并管理UI显示
 * --@ Author: paofan25, alivecn2@gmail.com
 * --@ Copyright: Copyright (c) 2025, paofan25
 ******************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using AssetBundles;

public class UI_IllusrtatedGuide_List : ChangeLanugeBase
{
    [Header("图鉴分类设置")]
    public E_IllustratedGuideCategory category = E_IllustratedGuideCategory.Fish;

    [Header("UI组件")]
    public RectTransform content;
    public ScrollRect scrollRect;
    public GameObject itemPrefab; // 列表项预制体

    [Header("布局设置")]
    public VerticalLayoutGroup gridLayout;
    public int itemsPerRow = 5; // 每行显示的项目数
    public float itemSpacing = 10f; // 项目间距

    [Header("自动高度设置")]
    public bool autoHeight = true; // 是否自动设置高度
    public float minHeight = 100f; // 最小高度
    public float maxHeight = 800f; // 最大高度
    public float padding = 20f; // 内容边距

    private List<IlluData> currentEntries;
    private List<GameObject> spawnedItems = new List<GameObject>();
    private List<GameObject> rowContainers = new List<GameObject>(); // 行容器列表
    private bool isInitialized = false;

    public override void Init()
    {
        base.Init(transform);

        // 初始化布局组件
        InitializeLayout();

        // 获取该列表对应的数据
        RefreshData();

        isInitialized = true;
    }

    /// <summary>
    /// 初始化布局组件
    /// </summary>
    private void InitializeLayout()
    {
        if (gridLayout == null)
        {
            gridLayout = content.GetComponent<VerticalLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            }
        }

        // 设置垂直布局参数
        gridLayout.spacing = itemSpacing;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.childControlHeight = false;
        gridLayout.childControlWidth = false;
        gridLayout.childForceExpandHeight = false;
        gridLayout.childForceExpandWidth = false;

        // 设置边距
        gridLayout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
    }

    /// <summary>
    /// 设置图鉴分类
    /// </summary>
    public void SetCategory(E_IllustratedGuideCategory newCategory)
    {
        category = newCategory;
        RefreshData();
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    public void RefreshData()
    {

        // 获取数据
        currentEntries = IlluGuideMgr.Instance.GetEntriesByCategory(category);
        
        // 在开始加载时禁用滚动并固定到顶部
        if (scrollRect != null) {
            scrollRect.enabled = false;
            scrollRect.verticalNormalizedPosition = 1f; // 固定到顶部
        }
        // 清理现有项目
        ClearItems();

        // 创建新的列表项
        CreateItems();

        // 更新布局和高度
        StartCoroutine(UpdateLayoutAndHeight());
    }

    /// <summary>
    /// 清理现有列表项
    /// </summary>
    private void ClearItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
            {
                DestroyImmediate(item);
            }
        }
        spawnedItems.Clear();

        // 清理行容器
        ClearRowContainers();
    }

    /// <summary>
    /// 创建列表项
    /// </summary>
    private void CreateItems()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("[图鉴列表] 列表项预制体未设置！");
            return;
        }

        // 清理现有行容器
        ClearRowContainers();

        // 计算需要的行数
        int totalItems = currentEntries.Count;
        int rowCount = Mathf.CeilToInt((float)totalItems / itemsPerRow);

        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            // 创建行容器
            GameObject rowContainer = CreateRowContainer(rowIndex);
            rowContainers.Add(rowContainer);

            // 计算当前行的项目数量
            int startIndex = rowIndex * itemsPerRow;
            int endIndex = Mathf.Min(startIndex + itemsPerRow, totalItems);

            // 为当前行创建项目
            for (int i = startIndex; i < endIndex; i++)
            {
                var entry = currentEntries[i];
                if (entry == null) continue;

                // 实例化预制体到行容器中
                GameObject itemObject = Instantiate(itemPrefab, rowContainer.transform);
                spawnedItems.Add(itemObject);

                // 初始化列表项组件
                InitializeListItem(i, itemObject, entry);
            }
        }

    }

    /// <summary>
    /// 创建行容器
    /// </summary>
    private GameObject CreateRowContainer(int rowIndex)
    {
        GameObject rowContainer = new GameObject($"Row_{rowIndex}");
        rowContainer.transform.SetParent(content, false);

        // 添加RectTransform
        RectTransform rowRect = rowContainer.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0, 1);
        rowRect.anchorMax = new Vector2(1, 1);
        rowRect.pivot = new Vector2(0.5f, 1);

        // 添加HorizontalLayoutGroup
        HorizontalLayoutGroup horizontalLayout = rowContainer.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.spacing = itemSpacing;
        horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
        horizontalLayout.childControlHeight = false;
        horizontalLayout.childControlWidth = false;
        horizontalLayout.childForceExpandHeight = false;
        horizontalLayout.childForceExpandWidth = false;

        // 添加ContentSizeFitter
        ContentSizeFitter sizeFitter = rowContainer.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rowContainer;
    }

    /// <summary>
    /// 清理行容器
    /// </summary>
    private void ClearRowContainers()
    {
        foreach (var container in rowContainers)
        {
            if (container != null)
            {
                DestroyImmediate(container);
            }
        }
        rowContainers.Clear();
    }

    /// <summary>
    /// 初始化列表项
    /// </summary>
    private void InitializeListItem(int index, GameObject itemObject, IlluData entry)
    {
        // 确保有Button组件
        if (itemObject.GetComponent<Button>() == null)
        {
            itemObject.AddComponent<Button>();
        }

        // 确保有列表项组件
        var itemComponent = itemObject.GetComponent<UI_IllustratedGuide_Item>();
        if (itemComponent == null)
        {
            itemComponent = itemObject.AddComponent<UI_IllustratedGuide_Item>();
        }

        // 初始化组件
        if (itemComponent != null)
        {
            itemComponent.Init(entry);
        }
        else
        {
            Debug.LogWarning($"[图鉴列表] 列表项组件初始化失败，分类: {category}, 索引: {index}");
        }
    }

    /// <summary>
    /// 更新布局和自动设置高度
    /// </summary>
    private IEnumerator UpdateLayoutAndHeight()
    {
        yield return null;

        // 强制重新计算布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // 自动设置高度
        if (autoHeight)
        {
            yield return StartCoroutine(CalculateAndSetHeight());
        }

        // 如果有ScrollRect，重置滚动位置
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0,1);
        }
    }

    /// <summary>
    /// 计算并设置内容高度
    /// </summary>
    private IEnumerator CalculateAndSetHeight()
    {
        // 等待一帧确保所有子对象都已布局完成
        yield return null;

        float totalHeight = CalculateContentHeight();

        // 应用高度限制
        totalHeight = Mathf.Clamp(totalHeight, minHeight, maxHeight);

        // 设置内容高度
        if (content != null)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
        }

        // 更新ScrollRect状态
        UpdateScrollRectState(totalHeight);
        yield return new WaitForSeconds(2f);
        // scrollRect.verticalNormalizedPosition = 0f;
    }

    /// <summary>
    /// 计算内容总高度
    /// </summary>
    private float CalculateContentHeight()
    {
        float totalHeight = 0f;
        int visibleRowCount = 0;

        // 计算所有可见行的高度
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (child != null && child.gameObject.activeInHierarchy)
            {
                var rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    totalHeight += rectTransform.rect.height;
                    visibleRowCount++;
                }
            }
        }

        // 添加行间距
        if (visibleRowCount > 1)
        {
            totalHeight += gridLayout.spacing * (visibleRowCount - 1);
        }

        // 添加边距
        totalHeight += gridLayout.padding.top + gridLayout.padding.bottom;

        return totalHeight;
    }

    /// <summary>
    /// 更新ScrollRect状态
    /// </summary>
    private void UpdateScrollRectState(float contentHeight)
    {
        if (scrollRect == null) return;

        // 获取视口高度
        float viewportHeight = scrollRect.viewport.rect.height;

        // 如果内容高度小于视口高度，禁用滚动
        if (contentHeight <= viewportHeight)
        {
            scrollRect.enabled = false;
            scrollRect.normalizedPosition = Vector2.zero;
        }
        else
        {
            scrollRect.enabled = true;
            // scrollRect.normalizedPosition = new Vector2(0,1);
            // scrollRect.verticalNormalizedPosition = 1f;
        }

    }

    /// <summary>
    /// 下一帧更新布局（保留原有方法以兼容）
    /// </summary>
    private IEnumerator UpdateLayoutNextFrame()
    {
        yield return StartCoroutine(UpdateLayoutAndHeight());
    }

    /// <summary>
    /// 根据分类获取预制体路径
    /// </summary>
    private string GetPrefabPathByCategory()
    {
        return category switch
        {
            E_IllustratedGuideCategory.Fish => SysDefine.PrefabAssetbundle + SysDefine.UI_IllustratedGuide_Entry,
            E_IllustratedGuideCategory.Plant => SysDefine.PrefabAssetbundle + SysDefine.UI_IllustratedGuide_Entry,
            E_IllustratedGuideCategory.Decoration => SysDefine.PrefabAssetbundle + SysDefine.UI_IllustratedGuide_Entry,
            E_IllustratedGuideCategory.Cube => SysDefine.PrefabAssetbundle + SysDefine.UI_IllustratedGuide_Entry,
            _ => SysDefine.PrefabAssetbundle + SysDefine.UI_IllustratedGuide_Entry
        };
    }

    /// <summary>
    /// 动态加载预制体
    /// </summary>
    public void LoadPrefabAsync()
    {
        if (itemPrefab != null) return; // 已经加载过了

        string prefabPath = GetPrefabPathByCategory();

        // 使用协程处理异步加载
        StartCoroutine(LoadPrefabCoroutine(prefabPath));
    }

    /// <summary>
    /// 预制体加载协程
    /// </summary>
    private IEnumerator LoadPrefabCoroutine(string prefabPath)
    {
        // 使用AssetBundleManager加载预制体
        var loader = AssetBundleManager.Instance.LoadAssetAsync(prefabPath, typeof(GameObject));
        if (loader != null)
        {
            // 等待加载完成
            while (!loader.isDone)
            {
                yield return null;
            }

            // 检查加载结果
            if (loader.asset is GameObject prefab)
            {
                itemPrefab = prefab;

                // 如果已经初始化，刷新数据
                if (isInitialized)
                {
                    RefreshData();
                }
            }
            else
            {
                Debug.LogError($"[图鉴列表] 预制体加载失败，路径: {prefabPath}");
            }

            // 清理加载器
            loader.Dispose();
        }
        else
        {
            Debug.LogError($"[图鉴列表] 无法创建加载器，路径: {prefabPath}");
        }
    }

    /// <summary>
    /// 获取当前分类的条目数量
    /// </summary>
    public int GetEntryCount()
    {
        return currentEntries?.Count ?? 0;
    }

    /// <summary>
    /// 获取指定索引的条目
    /// </summary>
    public IlluData GetEntry(int index)
    {
        if (index >= 0 && index < currentEntries.Count)
        {
            return currentEntries[index];
        }
        return null;
    }

    /// <summary>
    /// 根据ID查找条目
    /// </summary>
    public IlluData GetEntryById(string id)
    {
        return currentEntries?.Find(entry => entry.id == id);
    }

    /// <summary>
    /// 滚动到指定条目
    /// </summary>
    public void ScrollToEntry(string entryId)
    {
        if (scrollRect == null) return;

        var entry = GetEntryById(entryId);
        if (entry == null) return;

        int index = currentEntries.IndexOf(entry);
        if (index < 0) return;

        // 计算目标位置
        float targetPosition = (float)index / currentEntries.Count;
        scrollRect.normalizedPosition = new Vector2(0, 1 - targetPosition);
    }

    /// <summary>
    /// 设置每行显示的项目数
    /// </summary>
    public void SetItemsPerRow(int count)
    {
        itemsPerRow = count;
        if (isInitialized)
        {
            // 重新创建所有项目以应用新的行数
            RefreshData();
        }
    }

    /// <summary>
    /// 设置项目间距
    /// </summary>
    public void SetItemSpacing(float spacing)
    {
        itemSpacing = spacing;
        if (gridLayout != null)
        {
            gridLayout.spacing = spacing;
        }

        // 更新所有行容器的间距
        foreach (var rowContainer in rowContainers)
        {
            if (rowContainer != null)
            {
                var horizontalLayout = rowContainer.GetComponent<HorizontalLayoutGroup>();
                if (horizontalLayout != null)
                {
                    horizontalLayout.spacing = spacing;
                }
            }
        }

        if (isInitialized)
        {
            StartCoroutine(UpdateLayoutAndHeight());
        }
    }

    /// <summary>
    /// 设置自动高度
    /// </summary>
    public void SetAutoHeight(bool enabled)
    {
        autoHeight = enabled;
        if (enabled && isInitialized)
        {
            StartCoroutine(CalculateAndSetHeight());
        }
    }

    /// <summary>
    /// 设置高度限制
    /// </summary>
    public void SetHeightLimits(float min, float max)
    {
        minHeight = min;
        maxHeight = max;
        if (autoHeight && isInitialized)
        {
            StartCoroutine(CalculateAndSetHeight());
        }
    }

    /// <summary>
    /// 手动刷新高度
    /// </summary>
    [ContextMenu("手动刷新高度")]
    public void ManualRefreshHeight()
    {
        if (autoHeight)
        {
            StartCoroutine(CalculateAndSetHeight());
        }
    }
}
