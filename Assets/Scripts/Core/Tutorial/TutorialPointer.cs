using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 引导指针/手指动画
/// 显示引导箭头或手指指向目标
/// </summary>
public class TutorialPointer : MonoBehaviour
{
    public enum PointerDirection
    {
        Auto,       // 自动检测方向
        Up,
        Down,
        Left,
        Right,
        UpLeft,     // 左上
        UpRight,    // 右上
        DownLeft,   // 左下
        DownRight   // 右下
    }

    public enum PointerType
    {
        Arrow,      // 箭头
        Finger      // 手指
    }

    [Header("指针设置")]
    [SerializeField] private PointerType pointerType = PointerType.Finger;
    [SerializeField] private RectTransform pointerTransform;
    [SerializeField] private Image pointerImage;

    [Header("动画设置")]
    [SerializeField] private bool enableAnimation = true;
    [SerializeField] private float animationDistance = 20f;
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private Vector2 offset = new Vector2(0, 50);

    private RectTransform targetTransform;
    private PointerDirection currentDirection = PointerDirection.Auto;
    private Vector3 basePosition;  // 基础位置（不含动画偏移）
    private bool isShowing = false;

    private void Awake()
    {
        // 初始化时获取引用
        if (pointerTransform == null)
        {
            pointerTransform = GetComponent<RectTransform>();
        }
        if (pointerImage == null)
        {
            pointerImage = GetComponent<Image>();
        }

        // 初始隐藏
        SetPointerVisible(false);
    }

    /// <summary>
    /// 手动更新指针（由外部调用，如TutorialUIController的Update）
    /// </summary>
    public void ManualUpdate()
    {
        if (!isShowing || pointerTransform == null) return;

        UpdatePointerPosition();

        if (enableAnimation)
        {
            ApplyAnimation();
        }
    }

    /// <summary>
    /// 设置指针可见性（通过Image组件，保持GameObject激活）
    /// </summary>
    private void SetPointerVisible(bool visible)
    {
        if (pointerImage != null)
        {
            pointerImage.enabled = visible;
        }
    }

    /// <summary>
    /// 显示指针
    /// </summary>
    public void ShowPointer(RectTransform target, PointerDirection direction = PointerDirection.Auto)
    {
        ShowPointer(target, offset, direction);
    }

    /// <summary>
    /// 显示指针（带自定义偏移）
    /// </summary>
    public void ShowPointer(RectTransform target, Vector2 customOffset, PointerDirection direction = PointerDirection.Auto)
    {
        if (pointerTransform == null) return;

        targetTransform = target;
        currentDirection = direction;
        offset = customOffset;
        isShowing = true;

        // 设置指针的锚点和轴心点为左下角，避免窗口尺寸变化时位置偏移
        pointerTransform.anchorMin = new Vector2(0f, 0f);
        pointerTransform.anchorMax = new Vector2(0f, 0f);
        pointerTransform.pivot = new Vector2(0.5f, 0.5f);

        // 使用anchoredPosition直接设置绝对坐标
        pointerTransform.anchoredPosition = customOffset;

        // 设置指针旋转
        UpdatePointerRotation();

        // 显示指针
        SetPointerVisible(true);
    }

    /// <summary>
    /// 隐藏指针
    /// </summary>
    public void HidePointer()
    {
        isShowing = false;
        targetTransform = null;

        SetPointerVisible(false);
    }

    /// <summary>
    /// 更新指针位置
    /// </summary>
    private void UpdatePointerPosition()
    {
        if (pointerTransform == null) return;

        // 使用绝对坐标（offset直接作为位置）
        basePosition = new Vector3(offset.x, offset.y, 0);

        // 如果没有动画，直接设置位置
        if (!enableAnimation)
        {
            pointerTransform.anchoredPosition = offset;
        }
    }

    /// <summary>
    /// 更新指针旋转
    /// </summary>
    private void UpdatePointerRotation()
    {
        if (pointerTransform == null) return;

        PointerDirection direction = currentDirection;

        // 如果是自动模式，根据偏移方向判断
        if (direction == PointerDirection.Auto)
        {
            if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
            {
                direction = offset.x > 0 ? PointerDirection.Left : PointerDirection.Right;
            }
            else
            {
                direction = offset.y > 0 ? PointerDirection.Down : PointerDirection.Up;
            }
        }

        // 设置旋转角度
        float angle = 0f;
        switch (direction)
        {
            case PointerDirection.Up:
                angle = 0f;
                break;
            case PointerDirection.Down:
                angle = 180f;
                break;
            case PointerDirection.Left:
                angle = 90f;
                break;
            case PointerDirection.Right:
                angle = -90f;
                break;
            case PointerDirection.UpLeft:
                angle = 45f;
                break;
            case PointerDirection.UpRight:
                angle = -45f;
                break;
            case PointerDirection.DownLeft:
                angle = 135f;
                break;
            case PointerDirection.DownRight:
                angle = -135f;
                break;
        }

        pointerTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// 应用指针动画
    /// </summary>
    private void ApplyAnimation()
    {
        if (pointerTransform == null) return;

        // 简单的上下/左右移动动画
        float animValue = Mathf.Sin(Time.time * animationSpeed) * animationDistance;

        PointerDirection direction = currentDirection;
        if (direction == PointerDirection.Auto)
        {
            // 根据偏移方向自动判断动画方向
            if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
            {
                direction = offset.x > 0 ? PointerDirection.Left : PointerDirection.Right;
            }
            else
            {
                direction = offset.y > 0 ? PointerDirection.Down : PointerDirection.Up;
            }
        }

        Vector3 animOffset = Vector3.zero;
        float diagValue = animValue * 0.707f; // 对角线方向的分量（1/√2）
        switch (direction)
        {
            case PointerDirection.Up:
                animOffset = new Vector3(0, -animValue, 0);  // 向上指，动画向下移动
                break;
            case PointerDirection.Down:
                animOffset = new Vector3(0, animValue, 0);   // 向下指，动画向上移动
                break;
            case PointerDirection.Left:
                animOffset = new Vector3(-animValue, 0, 0);  // 向左指，动画向右移动
                break;
            case PointerDirection.Right:
                animOffset = new Vector3(animValue, 0, 0);   // 向右指，动画向左移动
                break;
            case PointerDirection.UpLeft:
                animOffset = new Vector3(diagValue, -diagValue, 0);  // 向左上指，动画向右下移动
                break;
            case PointerDirection.UpRight:
                animOffset = new Vector3(-diagValue, -diagValue, 0); // 向右上指，动画向左下移动
                break;
            case PointerDirection.DownLeft:
                animOffset = new Vector3(diagValue, diagValue, 0);   // 向左下指，动画向右上移动
                break;
            case PointerDirection.DownRight:
                animOffset = new Vector3(-diagValue, diagValue, 0);  // 向右下指，动画向左上移动
                break;
        }

        // 应用基础位置 + 动画偏移
        pointerTransform.anchoredPosition = new Vector2(basePosition.x + animOffset.x, basePosition.y + animOffset.y);
    }

    /// <summary>
    /// 设置指针类型
    /// </summary>
    public void SetPointerType(PointerType type)
    {
        pointerType = type;
        // 这里可以切换不同的Sprite
    }

    /// <summary>
    /// 设置偏移量
    /// </summary>
    public void SetOffset(Vector2 newOffset)
    {
        offset = newOffset;
    }
}
