using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 引导遮罩
/// 用于创建半透明遮罩并高亮显示指定区域
/// 实现ICanvasRaycastFilter接口，让高亮区域的点击可以穿透
/// </summary>
[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class TutorialMask : MonoBehaviour
{
    public enum HighlightShape
    {
        Circle,     // 圆形高亮
        Rectangle   // 矩形高亮
    }

    [Header("遮罩设置")]
    [SerializeField] private Material maskMaterial;
    [SerializeField] private Color maskColor = new Color(0, 0, 0, 0f); // 全透明，但仍阻挡点击
    [SerializeField] private Image maskImage;

    [Header("高亮设置")]
    [SerializeField] private float highlightPadding = 20f;
    [SerializeField] private float animationSpeed = 5f;

    private RectTransform highlightTarget;
    private HighlightShape currentShape = HighlightShape.Circle;
    private Vector2 customPadding;
    private bool isHighlighting = false;

    // Shader属性
    private static readonly int ShaderCenter = Shader.PropertyToID("_Center");
    private static readonly int ShaderRadius = Shader.PropertyToID("_Radius");
    private static readonly int ShaderWidth = Shader.PropertyToID("_Width");
    private static readonly int ShaderHeight = Shader.PropertyToID("_Height");

    private Material materialInstance;

    // 用于射线检测穿透的组件
    private TutorialMaskRaycastFilter raycastFilter;

    private void Awake()
    {
        InitializeMaterial();
        EnsureRaycastFilterInitialized();
    }

    /// <summary>
    /// 初始化材质
    /// </summary>
    private void InitializeMaterial()
    {
        // 初始化材质（如果有自定义shader）
        if (maskImage != null && maskMaterial != null && materialInstance == null)
        {
            materialInstance = new Material(maskMaterial);
            maskImage.material = materialInstance;
            maskImage.color = maskColor;
        }
    }

    /// <summary>
    /// 确保射线过滤器已初始化（懒加载）
    /// </summary>
    private void EnsureRaycastFilterInitialized()
    {
        if (raycastFilter != null) return;

        // 尝试自动查找maskImage
        if (maskImage == null)
        {
            // 如果maskImage未赋值，尝试从子对象中查找
            maskImage = GetComponentInChildren<Image>(true);
            if (maskImage == null)
            {
                Debug.LogError("[Tutorial] TutorialMask: 找不到maskImage！");
                return;
            }
            Debug.Log($"[Tutorial] TutorialMask: 自动找到maskImage: {maskImage.name}");
        }

        // 添加射线过滤组件到遮罩Image上
        raycastFilter = maskImage.gameObject.GetComponent<TutorialMaskRaycastFilter>();
        if (raycastFilter == null)
        {
            raycastFilter = maskImage.gameObject.AddComponent<TutorialMaskRaycastFilter>();
        }
        raycastFilter.SetMask(this);
        Debug.Log("[Tutorial] TutorialMask: 射线过滤器已初始化");
    }

    /// <summary>
    /// 获取当前高亮目标
    /// </summary>
    public RectTransform HighlightTarget => highlightTarget;

    /// <summary>
    /// 是否正在高亮
    /// </summary>
    public bool IsHighlighting => isHighlighting;

    /// <summary>
    /// 获取当前高亮形状
    /// </summary>
    public HighlightShape CurrentShape => currentShape;

    /// <summary>
    /// 获取当前padding
    /// </summary>
    public Vector2 CurrentPadding => customPadding;

    private void Update()
    {
        if (isHighlighting && highlightTarget != null)
        {
            UpdateHighlightPosition();
        }
    }

    /// <summary>
    /// 显示高亮区域
    /// </summary>
    public void ShowHighlight(RectTransform target, HighlightShape shape = HighlightShape.Circle, Vector2 padding = default)
    {
        if (target == null) return;

        // 确保组件已初始化
        EnsureRaycastFilterInitialized();
        InitializeMaterial();

        highlightTarget = target;
        currentShape = shape;
        customPadding = padding == default ? Vector2.one * highlightPadding : padding;
        isHighlighting = true;

        // 显示遮罩Image（使用enabled而不是SetActive，避免禁用整个GameObject）
        if (maskImage != null)
        {
            maskImage.enabled = true;
        }

        UpdateHighlightPosition();
    }

    /// <summary>
    /// 隐藏高亮区域（但保持遮罩显示）
    /// </summary>
    public void HideHighlight()
    {
        isHighlighting = false;
        highlightTarget = null;

        // 注意：这里不隐藏遮罩，只是停止高亮效果
        // 遮罩的显示/隐藏由 ShowMask/HideMask 控制

        // 清除目标按钮
        if (raycastFilter != null)
        {
            raycastFilter.ClearTargetButton();
        }
    }

    /// <summary>
    /// 显示遮罩（不带高亮）
    /// </summary>
    public void ShowMask()
    {
        EnsureRaycastFilterInitialized();

        if (maskImage != null)
        {
            maskImage.enabled = true;
            // 确保遮罩拦截所有点击（清除目标按钮）
            if (raycastFilter != null)
            {
                raycastFilter.ClearTargetButton();
            }
        }
    }

    /// <summary>
    /// 隐藏遮罩
    /// </summary>
    public void HideMask()
    {
        if (maskImage != null)
        {
            maskImage.enabled = false;
        }

        // 清除目标按钮
        if (raycastFilter != null)
        {
            raycastFilter.ClearTargetButton();
        }
    }

    /// <summary>
    /// 设置点击穿透的目标按钮
    /// </summary>
    public void SetTargetButton(Button button)
    {
        // 确保raycastFilter已初始化
        EnsureRaycastFilterInitialized();

        if (raycastFilter != null)
        {
            raycastFilter.SetTargetButton(button);
        }
        else
        {
            Debug.LogError("[Tutorial] TutorialMask.SetTargetButton: raycastFilter为null！无法设置目标按钮");
        }
    }

    /// <summary>
    /// 更新高亮位置
    /// </summary>
    private void UpdateHighlightPosition()
    {
        if (highlightTarget == null || materialInstance == null) return;

        // 获取目标在Canvas中的位置
        Vector2 targetPosition = GetCanvasPosition(highlightTarget);
        Rect targetRect = highlightTarget.rect;

        // 归一化坐标（0-1范围）
        RectTransform canvasRect = maskImage.rectTransform;
        Vector2 normalizedCenter = new Vector2(
            targetPosition.x / canvasRect.rect.width,
            targetPosition.y / canvasRect.rect.height
        );

        materialInstance.SetVector(ShaderCenter, normalizedCenter);

        if (currentShape == HighlightShape.Circle)
        {
            // 圆形高亮
            float radius = Mathf.Max(targetRect.width, targetRect.height) / 2f + highlightPadding;
            float normalizedRadius = radius / Mathf.Max(canvasRect.rect.width, canvasRect.rect.height);
            materialInstance.SetFloat(ShaderRadius, normalizedRadius);
        }
        else
        {
            // 矩形高亮
            float width = (targetRect.width + customPadding.x * 2) / canvasRect.rect.width;
            float height = (targetRect.height + customPadding.y * 2) / canvasRect.rect.height;
            materialInstance.SetFloat(ShaderWidth, width);
            materialInstance.SetFloat(ShaderHeight, height);
        }
    }

    /// <summary>
    /// 获取RectTransform在Canvas中的位置
    /// </summary>
    private Vector2 GetCanvasPosition(RectTransform rectTransform)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return Vector2.zero;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position),
            canvas.worldCamera,
            out localPoint
        );

        return localPoint;
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
