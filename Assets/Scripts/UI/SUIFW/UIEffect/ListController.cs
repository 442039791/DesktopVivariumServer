using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using Common;

using System.Linq;

using Debug = UnityEngine.Debug;
using System.Diagnostics;

/// <summary>
/// 定义列表的滚动方向
/// </summary>
public enum ListDirection
{
    Vertical,
    Horizontal
}

/// <summary>
/// ListController 用于管理动态列表，支持垂直和水平滚动，并且每行（或每列）可以显示多个物体。
/// </summary>
public class ListController : MonoBehaviour
{
    [Header("UI Components")]
    public RectTransform content; // Scroll View的Content
    public ScrollRect scrollRect; // Scroll Rect组件

    [Header("Debug")]
    public bool debug = false; // 是否开启调试模式
    [Header("List Settings")]
    public ListDirection direction = ListDirection.Vertical; // 列表方向
    public int itemsPerRow = 3; // 每行显示的物体数量（垂直方向）或每列显示的物体数量（水平方向）
    private float itemSize = 100f; // 每个列表项的尺寸（宽度或高度，取决于方向）
    public float spacing = 1f; // 列表项之间的间距
    public int bufferCount = 2; // 缓冲区数量

    private int visibleCount; // 可见的行数（包括缓冲区）
    private int firstVisibleRowIndex = -1; // 当前第一个可见的行索引
    private Dictionary<int, GameObject> activeRows = new Dictionary<int, GameObject>(); // 活动行的字典，key是行索引，value是行GameObject

    private List<int> removeIndices = new();
    private List<int> addIndices = new();

    public RectTransform rectTransform;
    public int ListCount;
    //public delegate bool DealList(int index,bool incr);
    //public DealList dealList;
    public delegate void InitGameobject(int index,GameObject @object);
    //public List<object> GameobjectInfos = new List<object>(); // 数据列表
    public event InitGameobject initGameobject; // 初始化GameObject的事件
    public string gameObjectName; // Prefab名称
    public GameObject gamePrefab; // 预制体
    public GameObject rowGameprefab;
    public float gamePrefabSize; // 预制体的尺寸（宽度或高度，取决于方向）
    public float gamePrefabSize_x;
    public float gamePrefabSize_y;
    public bool InitComplete = false; // 初始化完成标志
    public int id = -1;
    //void Start()
    //{
    //    // 初始化可以通过 Init 方法完成
    //}
    public RectTransform ExpandScrollBaseParent;//扩展的滚动父物体
    public RectTransform Parent;//父物体判断listrect大小
    private bool ExpandScroll = false;
    private float ExpandScrollvisibleCountAndScroll()
    {
        bool up = false;
        bool down = false;
        // 步骤 2：创建存储四个角坐标的数组
        Vector3[] worldCorners = new Vector3[4];

        // 步骤 3：获取世界空间角坐标（自动计算旋转/缩放/锚点影响）
        ExpandScrollBaseParent.GetWorldCorners(worldCorners);

        // 步骤 4：提取左下角坐标（Array Index 0）
        var bottomLeftWorldPos = worldCorners[0];
        // 步骤 5：提取右上角坐标（Array Index 2）
        var topRightWorldPos = worldCorners[2];
        Vector3[] MworldCorners = new Vector3[4];
        Parent.GetWorldCorners(MworldCorners);
        var bottomLeftMWorldPos = MworldCorners[0];
        var topRightMWorldPos = MworldCorners[2];
        if (bottomLeftWorldPos.y < bottomLeftMWorldPos.y)
            down = true;
        if (topRightWorldPos.y > topRightMWorldPos.y)
            up = true;
        //var upanddown = ExpandScrollUpAndVisible();
        Rect rect = new(0, 0, 0, 0)
        {
            width = Parent.rect.width,
            center = rectTransform.rect.center
        };
        float upheight ;
        float downheight ;
        if(!up)
        {
            upheight = topRightWorldPos.y;
        }
        else
        {
            upheight = topRightMWorldPos.y;
        }
        if (!down)
        {
            downheight = bottomLeftWorldPos.y;
        }
        else
        {
            downheight = bottomLeftMWorldPos.y;
        }
        rect.height = upheight- downheight  ;
        float scorll = topRightMWorldPos.y-topRightWorldPos.y  ;
        if (scorll < 0)
        {
            scorll = 0;
        }
        if (rect.height < 0)
        {
            visibleCount = 0;
            return scorll;
        }
        float viewportSize = (direction == ListDirection.Vertical) ? rect.height : rect.width;
        float rowSize = (direction == ListDirection.Vertical) ? (itemSize + spacing) : (itemSize + spacing);
        visibleCount = Mathf.CeilToInt(viewportSize / rowSize) + bufferCount;
        if(visibleCount ==0)
        {
            visibleCount = 1;
        }
        return scorll;
    }
    private (bool,bool) ExpandScrollUpAndVisible()
    {
        bool up = false;
        bool down = false;
        // 步骤 2：创建存储四个角坐标的数组
        Vector3[] worldCorners = new Vector3[4];

        // 步骤 3：获取世界空间角坐标（自动计算旋转/缩放/锚点影响）
        ExpandScrollBaseParent.GetWorldCorners(worldCorners);

        // 步骤 4：提取左下角坐标（Array Index 0）
        var bottomLeftWorldPos = worldCorners[0];
        // 步骤 5：提取右上角坐标（Array Index 2）
        var topRightWorldPos = worldCorners[2];
        Vector3[] MworldCorners = new Vector3[4];
        Parent.GetWorldCorners(MworldCorners);
        var bottomLeftMWorldPos = MworldCorners[0];
        var topRightMWorldPos = MworldCorners[2];
        if(bottomLeftWorldPos.y < bottomLeftMWorldPos.y)
            down = true;
        if (topRightWorldPos.y > topRightMWorldPos.y)
            up = true;
        return (up, down);
    }

    /// <summary>
    /// 初始化列表
    /// </summary>
    /// <param name="GameObjectName">Prefab 名称</param>
    /// <param name="gameobjectinfo">数据列表</param>
    /// <param name="createGameobjectAction">初始化 GameObject 的方法</param>
    public void Init(string GameObjectName, int count, InitGameobject createGameobjectAction/*, DealList dealList*/)
    {

        if (!InitComplete)
        {
            //ExpandScroll = false;
            Parent = scrollRect.transform.GetComponentInParent<RectTransform>();
            id = IDFactory.GetUniqueID();
            
            
            rowGameprefab = new GameObject("Row", typeof(RectTransform));
            if (direction == ListDirection.Vertical)
            {
                HorizontalLayoutGroup hl = rowGameprefab.AddComponent<HorizontalLayoutGroup>();
                hl.spacing = 0;
                hl.childForceExpandWidth = false;
                hl.childForceExpandHeight = false;
                hl.childAlignment = TextAnchor.MiddleLeft;
                hl.childControlHeight = false;
                hl.childControlWidth = false;
            }
            else
            {
                VerticalLayoutGroup vl = rowGameprefab.AddComponent<VerticalLayoutGroup>();
                vl.spacing = 0;
                vl.childForceExpandWidth = false;
                vl.childForceExpandHeight = false;
                vl.childControlHeight = false;
                vl.childControlWidth = false;
                vl.childAlignment = TextAnchor.MiddleLeft;
            }

            // 添加滚动事件监听
            scrollRect.onValueChanged.AddListener(OnScroll);
        }
        gamePrefab = (GameObject)ResMgr.Instance.GetAssetCache(GameObjectName, null);

        SetList(GameObjectName, count, createGameobjectAction/*, dealList*/);
        InitComplete = true;
    }
    ForwardScrollDrag forwardScrollDrag;
    public void InitExpandScroll(RectTransform ExpandScrollBaseParent,ScrollRect _scrollRect,float scrollSpeed =0.01f)
    {
        this.ExpandScrollBaseParent = ExpandScrollBaseParent;
        ExpandScroll = true;

        scrollRect.onValueChanged.RemoveAllListeners();
        if(scrollRect.transform.TryGetComponent(out forwardScrollDrag))
        {
            forwardScrollDrag.enabled = true;
        }
        else
        {
            forwardScrollDrag = scrollRect.gameObject.AddComponent<ForwardScrollDrag>();
        }
        forwardScrollDrag.scrollSensitivity = scrollSpeed;
        forwardScrollDrag.externalScrollRect = _scrollRect;
    }
    /// <summary>
    /// 处理滚动事件
    /// </summary>
    /// <param name="normalizedPosition">滚动位置</param>
    public void OnScroll(Vector2 normalizedPosition)
    {
        UpdateVisibleItems();
    }
    public float GetBestMaxSize(float maxsize)
    {
        if (!InitComplete)
        {
            if (debug)
            Debug.LogError("列表没有初始化");
            return -1;
        }
        int totalRows = Mathf.CeilToInt((float)ListCount / itemsPerRow);
        float bestsize = totalRows * (gamePrefabSize + spacing);
        //if (bestsize > maxsize)
        //{
        //    return maxsize;
        //}
        return bestsize;
    }
    /// <summary>
    /// 添加一个列表项
    /// </summary>
    /// <param name="obj">列表项数据</param>
    public void AddItem(int index)
    {
        //if(!dealList?.Invoke(index,true) == true)
        //{
        //    return;
        //}
        ListCount++;
        
        AdjustContentSize(); // 增加一个列表项的尺寸

        foreach (var rowIndex in activeRows.Keys.ToList()) // ToList 避免迭代时修改
        {
            CollectActiveRow(rowIndex);
        }
        activeRows.Clear();

        // 清理所有非动态创建的子对象（比如prefab中直接放置的对象）
        // 这些对象不在activeRows中，但可能会干扰列表显示
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(false);
        }

        UpdateVisibleItems();
    }
    public void Refresh()
    {
        AdjustContentSize(); // 增加一个列表项的尺寸

        foreach (var rowIndex in activeRows.Keys.ToList()) // ToList 避免迭代时修改
        {
            CollectActiveRow(rowIndex);
        }
        activeRows.Clear();

        // 清理所有非动态创建的子对象（比如prefab中直接放置的对象）
        // 这些对象不在activeRows中，但可能会干扰列表显示
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(false);
        }

        UpdateVisibleItems();
    }
    /// <summary>
    /// 移除一个列表项
    /// </summary>
    /// <param name="obj">列表项数据</param>
    public void RemoveItem(int index)
    {
        //if (!dealList?.Invoke(index,false) == true)
        //{
        //    return;
        //}
        ListCount--;
        
        if (index == -1) return; // 未找到


        AdjustContentSize(); // 减少一个列表项的尺寸

        foreach (var rowIndex in activeRows.Keys.ToList()) // ToList 避免迭代时修改
        {
            CollectActiveRow(rowIndex);
        }
        activeRows.Clear();

        // 清理所有非动态创建的子对象（比如prefab中直接放置的对象）
        // 这些对象不在activeRows中，但可能会干扰列表显示
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(false);
        }

        UpdateVisibleItems();
    }

    /// <summary>
    /// 设置列表数据
    /// </summary>
    /// <param name="name">Prefab 名称</param>
    /// <param name="list">数据列表</param>
    /// <param name="createGameobjectAction">初始化 GameObject 的方法</param>
    public void SetList(string name, int count,InitGameobject createGameobjectAction/*,DealList dealList*/)
    {
        // 根据方向添加布局组件
        
        if (gamePrefab != null)
        {
            var pr = gamePrefab.GetComponent<RectTransform>();
            gamePrefabSize_x = pr.rect.width;
            gamePrefabSize_y = pr.rect.height;
            gamePrefabSize = (direction == ListDirection.Vertical) ? gamePrefab.GetComponent<RectTransform>().rect.height : gamePrefab.GetComponent<RectTransform>().rect.width;
        }
        else
        {
            return;
        }
        itemSize = gamePrefabSize;
        // 根据列表方向计算可见的行数
        float viewportSize = (direction == ListDirection.Vertical) ? scrollRect.viewport.rect.height : scrollRect.viewport.rect.width;
        float rowSize = (direction == ListDirection.Vertical) ? (itemSize + spacing) : (itemSize + spacing);
        visibleCount = Mathf.CeilToInt(viewportSize / rowSize) + bufferCount;
        itemsPerRow = Mathf.RoundToInt((direction == ListDirection.Vertical) ? scrollRect.viewport.rect.width / gamePrefabSize_x : scrollRect.viewport.rect.height / gamePrefabSize_y);
        rectTransform ??= transform.GetComponent<RectTransform>();
        //this.dealList = dealList;
        content.localPosition=new Vector2(content.localPosition.x,0);
        initGameobject = createGameobjectAction;
        gameObjectName = name;
        ListCount = count;

        gamePrefab = (GameObject)ResMgr.Instance.GetAssetCache(name, null);
        if (gamePrefab != null)
        {
            gamePrefabSize = (direction == ListDirection.Vertical) ? gamePrefab.GetComponent<RectTransform>().rect.height : gamePrefab.GetComponent<RectTransform>().rect.width;
        }
        else
        {
            if (debug)
            Debug.LogError($"Prefab with name {name} not found in ResMgr.");
            return;
        }

        // 设置Content的尺寸
        AdjustContentSize();

        // 回收现有的活动行
        foreach (var rowIndex in activeRows.Keys.ToList()) // ToList 避免迭代时修改
        {
            CollectActiveRow(rowIndex);
        }
        activeRows.Clear();

        // 清理所有非动态创建的子对象（比如prefab中直接放置的对象）
        // 这些对象不在activeRows中，但可能会干扰列表显示
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(false);
        }

        UpdateVisibleItems();
    }

    /// <summary>
    /// 调整 Content 的尺寸
    /// </summary>
    /// <param name="countChange">列表项数量变化</param>
    
    private void AdjustContentSize()
    {
        int totalRows = ListCount;
        if (itemsPerRow!=1)
        {
            totalRows = Mathf.CeilToInt((float)ListCount / itemsPerRow);
        }
        
        float newSize = totalRows * (gamePrefabSize + spacing);

        if (direction == ListDirection.Vertical)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, newSize);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newSize);
        }
        else
        {
            content.sizeDelta = new Vector2(newSize, content.sizeDelta.y);
            rectTransform.sizeDelta = new Vector2(newSize, rectTransform.sizeDelta.y);
        }
    }

    /// <summary>
    /// 显示新的行
    /// </summary>
    /// <param name="rowIndex">行索引</param>
    /// <param name="startIndex">该行的第一个列表项索引</param>
    /// <returns>新创建的行 GameObject</returns>
    private GameObject ShowNewRow(int rowIndex, int startIndex)
    {
        //TODO:优化
        // 创建行
        var row = GameObjectPool.Instance.CreateObject(id+direction + "rowGameprefab", rowGameprefab);
        row.transform.SetParent(transform, false);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        int _rowcount = startIndex > ListCount ? (itemsPerRow - (startIndex - ListCount)) : itemsPerRow;
        rowRect.sizeDelta = new Vector2(_rowcount * gamePrefabSize_x, gamePrefabSize_y);
        row.transform.localPosition = new Vector3(0, rectTransform.rect.height / 2 - gamePrefabSize_y / 2 - rowIndex * gamePrefabSize_y- rowIndex* spacing, 0);
        // 填充行中的物体
        for (int i = 0; i < itemsPerRow; i++)
        {
            int itemIndex = startIndex + i;
            if (itemIndex >= ListCount)
                break;

            var g = ShowNewGameobject(itemIndex);

            g.transform.SetParent(row.transform, false);

        }

        return row;
    }
    //private GameObject ShowNewRow(int rowIndex, int startIndex)
    //{
    //    // 创建行
    //    GameObject row = new GameObject($"Row_{rowIndex}", typeof(RectTransform));
    //    row.transform.SetParent(content, false);
    //    RectTransform rowRect = row.GetComponent<RectTransform>();

    //    // 根据方向添加布局组件
    //    if (direction == ListDirection.Vertical)
    //    {
    //        HorizontalLayoutGroup hl = row.AddComponent<HorizontalLayoutGroup>();
    //        hl.spacing = spacing;
    //        hl.childForceExpandWidth = false;
    //        hl.childForceExpandHeight = true; // 让子项高度填充
    //        hl.childAlignment = TextAnchor.UpperLeft;
    //    }
    //    else
    //    {
    //        VerticalLayoutGroup vl = row.AddComponent<VerticalLayoutGroup>();
    //        vl.spacing = spacing;
    //        vl.childForceExpandWidth = true; // 让子项宽度填充
    //        vl.childForceExpandHeight = false;
    //        vl.childAlignment = TextAnchor.UpperLeft;
    //    }

    //    // 填充行中的物体
    //    for (int i = 0; i < itemsPerRow; i++)
    //    {
    //        int itemIndex = startIndex + i;
    //        if (itemIndex >= GameobjectInfos.Count)
    //            break;

    //        var g = ShowNewGameobject(itemIndex);
    //        // 已在 ShowNewGameobject 中设置父级，不再重复设置
    //    }

    //    return row;
    //}

    /// <summary>
    /// 显示新的列表项
    /// </summary>
    /// <param name="count">列表项索引</param>
    /// <returns>新创建的 GameObject</returns>
    /// 

    public GameObject ShowNewGameobject(int count)
    {
        var g = GameObjectPool.Instance.CreateObject(gameObjectName, gamePrefab);
        var component = g.GetComponent<IShowAtUI>();
        if (component != null)
        {
            component.OnShow = true;
        }
        initGameobject?.Invoke(count, g);
        g.SetActive(true);
        
        return g;
    }

    //private GameObject ShowNewGameobject(int count)
    //{
    //    var g = GameObjectPool.Instance.CreateObject(gameObjectName, gamePrefab);
    //    initGameobject?.Invoke(GameobjectInfos[count], g);
    //    g.GetComponent<IShowAtUI>().OnShow = true;
    //    // 确保父级在 ShowNewRow 中正确设置，无需在此方法中设置
    //    return g;
    //}

    /// <summary>
    /// 回收一个行
    /// </summary>
    /// <param name="rowIndex">行索引</param>
    private void CollectActiveRow(int rowIndex)
    {
        if (activeRows.TryGetValue(rowIndex, out GameObject row))
        {
            foreach (Transform child in row.transform)
            {
                var showAtUI = child.GetComponent<IShowAtUI>();
                if (showAtUI != null)
                {
                    showAtUI.OnShow = false;
                }
                GameObjectPool.Instance.CollectObject(child.gameObject);
            }
            GameObjectPool.Instance.CollectObject(row);
        }
    }

    /// <summary>
    /// 更新可见的列表项
    /// </summary>
    public void UpdateVisibleItems()
    {
        int lastvisibleCount = visibleCount;
        float scrollPos = 0;
        if (ExpandScroll)
            scrollPos = ExpandScrollvisibleCountAndScroll();
        if (visibleCount == 0)
            return;
        bool visibleCountChanged = visibleCount != lastvisibleCount;
        
        if (!ExpandScroll)
        {
             scrollPos = (direction == ListDirection.Vertical)
            ? content.localPosition.y
            : content.localPosition.x;
        }
        // 根据方向获取当前的滚动位置

        //if (scrollPos < 0)
        //{
        //    return;
        //}
        //if(debug)

        float viewportSize = (direction == ListDirection.Vertical)
            ? scrollRect.viewport.rect.height
            : scrollRect.viewport.rect.width;

        // 计算总行数
        int totalRows = Mathf.CeilToInt((float)ListCount / itemsPerRow);
        float totalSize = totalRows * (gamePrefabSize + spacing) - viewportSize;
        totalSize = Mathf.Max(totalSize, 0);

        // 不对scrollPos进行额外变换，直接使用
        float currentPosition = scrollPos;

        // 根据物品大小和间距计算可见行起始索引
        float rowSize = gamePrefabSize + spacing;
        int newFirstVisibleIndex = Mathf.FloorToInt(Mathf.Abs(currentPosition) / rowSize);
        newFirstVisibleIndex = Mathf.Clamp(newFirstVisibleIndex, 0, Mathf.Max(0, totalRows - visibleCount));

        // 如果可见起始索引发生了变化，进行更新
        if (newFirstVisibleIndex != firstVisibleRowIndex|| activeRows.Count==0|| visibleCountChanged)
        {
            firstVisibleRowIndex = newFirstVisibleIndex;

            int startRowIndex = firstVisibleRowIndex;
            int endRowIndex = Mathf.Min(firstVisibleRowIndex + visibleCount, totalRows);

            HashSet<int> newVisibleRowIndices = new HashSet<int>();
            for (int i = startRowIndex; i < endRowIndex; i++)
            {
                newVisibleRowIndices.Add(i);
            }

            removeIndices.Clear();
            addIndices.Clear();

            foreach (var kvp in activeRows)
            {
                if (!newVisibleRowIndices.Contains(kvp.Key))
                {
                    removeIndices.Add(kvp.Key);
                }
            }

            foreach (int rowIndex in newVisibleRowIndices)
            {
                if (!activeRows.ContainsKey(rowIndex))
                {
                    addIndices.Add(rowIndex);
                }
            }

            RefreshItems();
        }
    }

    //void UpdateVisibleItems()
    //{
    //    // 获取当前滚动位置
    //    float scrollPos = (direction == ListDirection.Vertical) ? scrollRect.verticalNormalizedPosition : scrollRect.horizontalNormalizedPosition;
    //    float viewportSize = (direction == ListDirection.Vertical) ? scrollRect.viewport.rect.height : scrollRect.viewport.rect.width;

    //    // 计算总行数
    //    int totalRows = Mathf.CeilToInt((float)GameobjectInfos.Count / itemsPerRow);
    //    totalRows = Mathf.Max(1, totalRows); // 至少有一行

    //    // 计算总内容尺寸
    //    float totalSize = totalRows * (gamePrefabSize + spacing) - viewportSize;
    //    totalSize = Mathf.Max(totalSize, 0); // 确保总尺寸非负

    //    // 计算当前滚动位置对应的实际位置
    //    float currentPosition = 0f;
    //    if (direction == ListDirection.Vertical)
    //    {
    //        // 纵向列表通常从底部开始，所以使用 (1 - scrollPos)
    //        currentPosition = (1 - scrollPos) * totalSize;
    //    }
    //    else
    //    {
    //        // 横向列表从左到右滚动，直接使用 scrollPos
    //        currentPosition = scrollPos * totalSize;
    //    }

    //    // 计算第一个可见的行索引
    //    int newFirstVisibleIndex = Mathf.FloorToInt(currentPosition / (gamePrefabSize + spacing));
    //    newFirstVisibleIndex = Mathf.Clamp(newFirstVisibleIndex, 0, Mathf.Max(0, totalRows - visibleCount));

    //    // 如果第一个可见索引发生变化，更新可见列表项
    //    if (newFirstVisibleIndex != firstVisibleRowIndex)
    //    {
    //        firstVisibleRowIndex = newFirstVisibleIndex;

    //        // 计算新的可见范围
    //        int startRowIndex = firstVisibleRowIndex;
    //        int endRowIndex = Mathf.Min(firstVisibleRowIndex + visibleCount, totalRows);

    //        // 创建一个新的HashSet来存储当前可见的行索引
    //        HashSet<int> newVisibleRowIndices = new HashSet<int>();
    //        for (int i = startRowIndex; i < endRowIndex; i++)
    //        {
    //            newVisibleRowIndices.Add(i);
    //        }

    //        removeIndices.Clear();
    //        addIndices.Clear();

    //        // 找出需要移除的行索引
    //        foreach (var kvp in activeRows)
    //        {
    //            if (!newVisibleRowIndices.Contains(kvp.Key))
    //            {
    //                removeIndices.Add(kvp.Key);
    //            }
    //        }

    //        // 找出需要添加的行索引
    //        foreach (int rowIndex in newVisibleRowIndices)
    //        {
    //            if (!activeRows.ContainsKey(rowIndex))
    //            {
    //                addIndices.Add(rowIndex);
    //            }
    //        }

    //        RefreshItems();
    //    }
    //}

    /// <summary>
    /// 刷新列表项，添加或移除行
    /// </summary>
    void RefreshItems()
    {
        // 回收所有需要移除的行
        foreach (var rowIndex in removeIndices)
        {
            CollectActiveRow(rowIndex);
            activeRows.Remove(rowIndex);
        }

        // 显示所有需要添加的行
        foreach (var rowIndex in addIndices)
        {
            int startIndex = rowIndex * itemsPerRow;
            var row = ShowNewRow(rowIndex, startIndex);
            activeRows[rowIndex] = row;
        }
    }
    //void RefreshItems()
    //{
    //    // 回收所有需要移除的行
    //    foreach (var rowIndex in removeIndices)
    //    {
    //        if (activeRows.ContainsKey(rowIndex))
    //        {
    //            CollectActiveRow(rowIndex);
    //            activeRows.Remove(rowIndex);
    //        }
    //    }

    //    // 显示所有需要添加的行
    //    foreach (var rowIndex in addIndices)
    //    {
    //        if (!activeRows.ContainsKey(rowIndex))
    //        {
    //            int startIndex = rowIndex * itemsPerRow;
    //            var row = ShowNewRow(rowIndex, startIndex);
    //            activeRows[rowIndex] = row;
    //        }
    //    }
    //}

    /// <summary>
    /// 自动通过反射获取所有方法并映射到枚举
    /// </summary>
    /// <param name="instance">ListController 实例</param>
    /// <returns>枚举到方法的字典</returns>
    //public static Dictionary<MethodType, Action<ListController>> GetMethodsDictionary(ListController instance)
    //{
    //    var methodsDictionary = new Dictionary<MethodType, Action<ListController>>();

    //    // 获取所有公共实例方法
    //    var methods = typeof(ListController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    //    // 获取枚举的所有值
    //    var methodTypes = Enum.GetValues(typeof(MethodType)).Cast<MethodType>();

    //    foreach (var methodType in methodTypes)
    //    {
    //        var methodName = methodType.ToString();
    //        var method = methods.FirstOrDefault(m => m.Name == methodName && m.ReturnType == typeof(void));

    //        if (method != null)
    //        {
    //            // 创建委托并将其添加到字典中
    //            var action = (Action<ListController>)Delegate.CreateDelegate(typeof(Action<ListController>), instance, method);
    //            methodsDictionary[methodType] = action;
    //        }
    //    }

    //    return methodsDictionary;
    //}
}
