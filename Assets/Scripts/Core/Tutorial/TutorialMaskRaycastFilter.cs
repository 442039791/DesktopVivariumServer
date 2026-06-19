using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 引导遮罩射线过滤器
/// 实现ICanvasRaycastFilter接口，只有点击目标按钮时才穿透遮罩
/// </summary>
public class TutorialMaskRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private TutorialMask tutorialMask;
    private Button targetButton;

    private void Awake()
    {
        Debug.Log($"[Tutorial] TutorialMaskRaycastFilter Awake - 组件已创建在: {gameObject.name}");
    }

    public void SetMask(TutorialMask mask)
    {
        tutorialMask = mask;
        Debug.Log($"[Tutorial] TutorialMaskRaycastFilter.SetMask - mask: {(mask != null ? mask.name : "null")}");
    }

    /// <summary>
    /// 设置当前需要点击的目标按钮
    /// </summary>
    public void SetTargetButton(Button button)
    {
        targetButton = button;
    }

    /// <summary>
    /// 清除目标按钮
    /// </summary>
    public void ClearTargetButton()
    {
        targetButton = null;
    }

    /// <summary>
    /// 判断射线位置是否有效（是否应该被遮罩拦截）
    /// 返回true = 遮罩拦截点击
    /// 返回false = 点击穿透遮罩
    /// </summary>
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // 如果没有设置目标按钮，正常拦截所有点击
        if (targetButton == null)
        {
            return true;
        }

        // 获取目标按钮的RectTransform
        RectTransform buttonRect = targetButton.GetComponent<RectTransform>();
        if (buttonRect == null)
        {
            return true;
        }

        // 获取按钮所在Canvas的相机（而不是使用遮罩的eventCamera）
        Camera buttonCamera = GetCanvasCamera(buttonRect);

        // 检查点击位置是否在目标按钮的矩形范围内
        bool isInsideButton = RectTransformUtility.RectangleContainsScreenPoint(
            buttonRect, screenPoint, buttonCamera);

        if (isInsideButton)
        {
            // 点击在目标按钮上，允许穿透
            return false;
        }

        // 点击不在目标按钮上，遮罩拦截
        return true;
    }

    /// <summary>
    /// 获取RectTransform所在Canvas的相机
    /// </summary>
    private Camera GetCanvasCamera(RectTransform rectTransform)
    {
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        if (canvas == null) return null;

        // 获取根Canvas
        Canvas rootCanvas = canvas.rootCanvas;
        if (rootCanvas == null) rootCanvas = canvas;

        // 根据Canvas的渲染模式返回相应的相机
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null; // Overlay模式不需要相机
        }
        else
        {
            return rootCanvas.worldCamera;
        }
    }
}
