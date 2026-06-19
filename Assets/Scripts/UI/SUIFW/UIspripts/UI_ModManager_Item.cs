using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

/// <summary>
/// 模组管理列表项（简化版）
/// 每个列表项显示模组名称、描述和控制按钮
/// 支持拖拽排序
/// </summary>
public class UI_ModManager_Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("基本信息")]
    public TextMeshProUGUI ModNameText;      // 模组名称
    public TextMeshProUGUI ModDescText;      // 描述

    [Header("控制按钮")]
    public Button EnableBtn;                  // 启用/禁用按钮

    [Header("状态显示")]
    public Image Background;                  // 背景
    public GameObject LoadedIcon;             // 已加载图标

    [Header("颜色设置")]
    public Color NormalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color DisabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private ModInfo _modInfo;
    private UI_ModManager _manager;

    // 拖拽相关
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Canvas _dragCanvas;              // 拖拽时使用的Canvas
    private RectTransform _canvasRectTransform;
    private Transform _originalParent;
    private int _originalSiblingIndex;
    private bool _isDragging = false;
    private Vector2 _grabOffset;             // 抓取时鼠标相对于物体中心的偏移
    private GameObject _placeholder;          // 拖拽时的占位符，保持布局稳定

    public ModInfo ModInfo => _modInfo;

    /// <summary>
    /// 初始化列表项
    /// </summary>
    public void Initialize(ModInfo modInfo, UI_ModManager manager)
    {
        _modInfo = modInfo;
        _manager = manager;

        // 初始化拖拽组件
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 获取拖拽用的Canvas
        _dragCanvas = GetComponentInParent<Canvas>();
        if (_dragCanvas != null)
        {
            _canvasRectTransform = _dragCanvas.GetComponent<RectTransform>();
        }

        // 设置模组名称
        if (ModNameText != null)
        {
            ModNameText.text = modInfo.DisplayName;
        }

        // 设置描述
        if (ModDescText != null)
        {
            string desc = modInfo.Manifest?.Description ?? "";
            // 限制描述长度，避免过长
            if (desc.Length > 100)
            {
                desc = desc.Substring(0, 100) + "...";
            }
            ModDescText.text = desc;
        }

        // 设置启用按钮
        if (EnableBtn != null)
        {
            EnableBtn.onClick.RemoveAllListeners();
            EnableBtn.AddDeskTopListener(OnEnableBtnClicked);
            UpdateEnableButtonText();
        }

        // 更新状态图标
        UpdateStatusIcons();

        // 更新背景颜色
        UpdateBackgroundColor();
    }

    /// <summary>
    /// 启用按钮点击
    /// </summary>
    private void OnEnableBtnClicked()
    {
        if (_modInfo != null && _manager != null)
        {
            _manager.ToggleModEnabled(_modInfo);
        }
        UpdateEnableButtonText();
        UpdateBackgroundColor();
        UpdateStatusIcons();
    }

    /// <summary>
    /// 更新启用按钮文本
    /// </summary>
    private void UpdateEnableButtonText()
    {
        if (EnableBtn == null || _modInfo == null) return;

        var btnText = EnableBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = _modInfo.IsEnabled ? "禁用" : "启用";
        }
    }

    /// <summary>
    /// 更新状态图标
    /// </summary>
    private void UpdateStatusIcons()
    {
        if (_modInfo == null) return;

        // 已加载图标
        if (LoadedIcon != null)
        {
            LoadedIcon.SetActive(_modInfo.LoadState == ModLoadState.Loaded);
        }
    }

    /// <summary>
    /// 更新背景颜色
    /// </summary>
    private void UpdateBackgroundColor()
    {
        if (Background == null || _modInfo == null) return;

        if (!_modInfo.IsEnabled)
        {
            Background.color = DisabledColor;
        }
        else
        {
            Background.color = NormalColor;
        }
    }

    private void OnDestroy()
    {
        if (EnableBtn != null)
        {
            EnableBtn.onClick.RemoveAllListeners();
        }
    }

    #region 拖拽排序

    /// <summary>
    /// 开始拖拽
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_manager == null || this == null || gameObject == null) return;
        if (_dragCanvas == null || _canvasRectTransform == null) return;

        _isDragging = true;

        // 记录原始信息
        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();

        // 创建占位符，保持布局稳定
        CreatePlaceholder();

        // 将元素移到Canvas根节点，脱离LayoutGroup控制
        Vector3 localScale = transform.localScale;
        transform.SetParent(_dragCanvas.transform, true);
        transform.localScale = localScale;

        // SetParent之后再计算偏移（此时anchoredPosition已更新）
        Vector2 localPointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPos);

        _grabOffset = _rectTransform.anchoredPosition - localPointerPos;

        // 设置半透明效果
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0.8f;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 创建占位符，保持LayoutGroup布局稳定
    /// </summary>
    private void CreatePlaceholder()
    {
        // 创建一个空的占位符
        _placeholder = new GameObject("DragPlaceholder");
        _placeholder.transform.SetParent(_originalParent);
        _placeholder.transform.SetSiblingIndex(_originalSiblingIndex);

        // 添加RectTransform并设置与当前元素相同的尺寸
        var placeholderRect = _placeholder.AddComponent<RectTransform>();
        placeholderRect.sizeDelta = _rectTransform.sizeDelta;

        // 添加LayoutElement来确保占位符占据正确的空间
        var layoutElement = _placeholder.AddComponent<UnityEngine.UI.LayoutElement>();
        layoutElement.preferredWidth = _rectTransform.rect.width;
        layoutElement.preferredHeight = _rectTransform.rect.height;
        layoutElement.flexibleWidth = 0;
        layoutElement.flexibleHeight = 0;
    }

    /// <summary>
    /// 销毁占位符（立即销毁，不等待帧结束）
    /// </summary>
    private void DestroyPlaceholder()
    {
        if (_placeholder != null)
        {
            DestroyImmediate(_placeholder);
            _placeholder = null;
        }
    }

    /// <summary>
    /// 拖拽中
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || _manager == null || this == null || gameObject == null) return;
        if (_rectTransform == null || _canvasRectTransform == null) return;

        // 将屏幕坐标转换为Canvas本地坐标
        Vector2 localPointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPos);

        // 设置物体位置（加上抓取偏移）
        _rectTransform.anchoredPosition = localPointerPos + _grabOffset;
    }

    /// <summary>
    /// 结束拖拽
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging || _manager == null || this == null || gameObject == null) return;

        _isDragging = false;

        // 恢复透明度
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        // 计算放置位置（排除占位符的列表，所以直接得到正确的索引）
        int newIndex = CalculateDropIndex(eventData.position);

        // 销毁占位符（必须在计算真实索引之前）
        DestroyPlaceholder();

        // 计算真实的sibling索引（需要考虑隐藏的模板等非活动元素）
        // newIndex是基于活动元素的索引，需要转换为实际的sibling索引
        int realIndex = 0;
        int activeCount = 0;
        for (int i = 0; i < _originalParent.childCount; i++)
        {
            Transform child = _originalParent.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                if (activeCount == newIndex)
                {
                    realIndex = i;
                    break;
                }
                activeCount++;
            }
            // 如果遍历完了还没找到，说明要插入到最后
            realIndex = i + 1;
        }

        // 放回原父节点
        if (_originalParent != null)
        {
            transform.SetParent(_originalParent, false);
            transform.SetSiblingIndex(realIndex);
        }

        // 如果位置改变，更新加载顺序
        if (realIndex != _originalSiblingIndex)
        {
            _manager.OnModItemDropped(this, realIndex);
        }
    }

    /// <summary>
    /// 根据鼠标位置计算放置索引
    /// 使用元素上下边界的中点来判断：鼠标在哪两个元素之间
    /// </summary>
    private int CalculateDropIndex(Vector2 screenPosition)
    {
        if (_originalParent == null) return _originalSiblingIndex;

        // 获取Canvas的相机（用于坐标转换）
        Camera canvasCamera = null;
        if (_dragCanvas != null)
        {
            if (_dragCanvas.renderMode == RenderMode.ScreenSpaceCamera ||
                _dragCanvas.renderMode == RenderMode.WorldSpace)
            {
                canvasCamera = _dragCanvas.worldCamera;
            }
        }

        // 收集所有有效的子元素（排除占位符）
        var children = new System.Collections.Generic.List<RectTransform>();
        for (int i = 0; i < _originalParent.childCount; i++)
        {
            Transform child = _originalParent.GetChild(i);
            if (child == null || !child.gameObject.activeSelf) continue;
            // 跳过占位符
            if (_placeholder != null && child.gameObject == _placeholder) continue;

            RectTransform childRect = child as RectTransform;
            if (childRect != null)
            {
                children.Add(childRect);
            }
        }

        if (children.Count == 0) return 0;

        // 计算每个元素的屏幕位置中心点
        var elementCenters = new System.Collections.Generic.List<float>();
        for (int i = 0; i < children.Count; i++)
        {
            RectTransform childRect = children[i];
            Vector3[] corners = new Vector3[4];
            childRect.GetWorldCorners(corners);
            // corners: 0=左下, 1=左上, 2=右上, 3=右下
            float top = RectTransformUtility.WorldToScreenPoint(canvasCamera, corners[1]).y;
            float bottom = RectTransformUtility.WorldToScreenPoint(canvasCamera, corners[0]).y;
            elementCenters.Add((top + bottom) / 2f);
        }

        // 使用每两个相邻元素之间的中点作为分界线
        // 检查是否在第一个元素的中心上方
        if (screenPosition.y >= elementCenters[0])
        {
            return 0;
        }

        // 检查中间位置
        for (int i = 0; i < children.Count - 1; i++)
        {
            // 如果鼠标在当前元素中心下方，但在下一个元素中心上方，插入到i+1位置
            if (screenPosition.y < elementCenters[i] && screenPosition.y >= elementCenters[i + 1])
            {
                return i + 1;
            }
        }

        // 鼠标在最后一个元素中心下方
        return children.Count;
    }

    #endregion
}
