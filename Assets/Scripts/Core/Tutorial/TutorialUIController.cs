using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 引导UI控制器
/// 负责控制引导界面的显示、遮罩、高亮等
/// </summary>
public class TutorialUIController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private TutorialMask tutorialMask;
    [SerializeField] private TutorialPointer tutorialPointer;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button dialogConfirmButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("设置")]
    [SerializeField] private float dialogFadeSpeed = 2f;
    [SerializeField] private float typewriterSpeed = 0.05f; // 打字机速度（每个字符的间隔）

    private TypewriterEffect typewriterEffect;
    private GameObject maskImage; // 遮罩图片
    private GameObject dialogBackground; // 对话框背景

    private void Awake()
    {
        // 先注册到TutorialManager（必须在HideTutorial之前，因为HideTutorial会禁用GameObject）
        TutorialManager.Instance.SetUIController(this);

        // 初始化打字机效果组件
        if (dialogText != null)
        {
            typewriterEffect = dialogText.gameObject.GetComponent<TypewriterEffect>();
            if (typewriterEffect == null)
            {
                typewriterEffect = dialogText.gameObject.AddComponent<TypewriterEffect>();
            }
            typewriterEffect.SetTypeSpeed(typewriterSpeed);
        }

        // 设置点击检测
        SetupClickDetection();

        // 绑定按钮事件
        if (dialogConfirmButton != null)
        {
            dialogConfirmButton.onClick.AddListener(OnDialogConfirm);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipClick);
        }

        // 初始化隐藏
        HideTutorial();
    }

    private void Update()
    {
        // 驱动指针动画更新
        if (tutorialPointer != null)
        {
            tutorialPointer.ManualUpdate();
        }
    }

    /// <summary>
    /// 设置点击检测
    /// 在MaskImage和DialogPanel的Background上添加点击检测
    /// </summary>
    private void SetupClickDetection()
    {
        if (tutorialCanvas == null) return;

        // 1. 查找并设置MaskImage的点击检测
        Transform maskTransform = tutorialCanvas.transform.Find("MaskImage");
        if (maskTransform != null)
        {
            maskImage = maskTransform.gameObject;

            // 确保Image的raycastTarget是开启的
            UnityEngine.UI.Image maskImg = maskImage.GetComponent<UnityEngine.UI.Image>();
            if (maskImg != null)
            {
                maskImg.raycastTarget = true;
            }

            // 在MaskImage上添加Button组件
            Button maskButton = maskImage.GetComponent<Button>();
            if (maskButton == null)
            {
                maskButton = maskImage.AddComponent<Button>();
            }
            maskButton.transition = UnityEngine.UI.Selectable.Transition.None;
            maskButton.onClick.AddListener(OnClickDetected);

            Debug.Log("[Tutorial] MaskImage点击检测设置成功");
        }
        else
        {
            Debug.LogWarning("[Tutorial] 找不到MaskImage对象");
        }

        // 2. 查找并设置DialogPanel的Background点击检测
        if (dialogPanel != null)
        {
            Transform backgroundTransform = dialogPanel.transform.Find("Background");
            if (backgroundTransform != null)
            {
                dialogBackground = backgroundTransform.gameObject;

                // 确保Image的raycastTarget是开启的
                UnityEngine.UI.Image bgImg = dialogBackground.GetComponent<UnityEngine.UI.Image>();
                if (bgImg != null)
                {
                    bgImg.raycastTarget = true;
                }

                // 在Background上添加Button组件
                Button bgButton = dialogBackground.GetComponent<Button>();
                if (bgButton == null)
                {
                    bgButton = dialogBackground.AddComponent<Button>();
                }
                bgButton.transition = UnityEngine.UI.Selectable.Transition.None;
                bgButton.onClick.AddListener(OnClickDetected);

                Debug.Log("[Tutorial] DialogPanel Background点击检测设置成功");
            }
            else
            {
                Debug.LogWarning("[Tutorial] 找不到DialogPanel下的Background对象");
            }
        }

    }

    /// <summary>
    /// 显示引导界面
    /// </summary>
    public void ShowTutorial()
    {
        if (tutorialCanvas != null)
        {
            tutorialCanvas.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏引导界面
    /// </summary>
    public void HideTutorial()
    {
        if (tutorialCanvas != null)
        {
            tutorialCanvas.gameObject.SetActive(false);
        }

        // 隐藏遮罩
        if (tutorialMask != null)
        {
            tutorialMask.HideMask();
        }

        HideAllElements();
    }

    /// <summary>
    /// 更新引导步骤UI
    /// </summary>
    public void UpdateStep(TutorialStep step, int currentIndex, int totalSteps)
    {
        // 检查当前步骤是否需要保留对话框（点击步骤且上一步是对话步骤）
        bool keepDialog = step is ClickUIStep clickStepCheck && clickStepCheck.PreviousStepWasDialog;

        if (keepDialog)
        {
            // 只隐藏遮罩和指针，保留对话框
            HideHighlightAndPointer();
        }
        else
        {
            HideAllElements();
        }

        // 更新进度文本
        if (progressText != null)
        {
            progressText.text = $"{currentIndex + 1}/{totalSteps}";
        }

        // 根据步骤类型显示不同UI
        if (step is ClickUIStep clickStep)
        {
            ShowClickUIGuide(clickStep);
        }
        else if (step is DialogStep dialogStep)
        {
            ShowDialog(dialogStep);
        }
    }

    /// <summary>
    /// 显示点击UI引导
    /// </summary>
    private void ShowClickUIGuide(ClickUIStep step)
    {
        // 获取目标按钮的RectTransform
        if (step.TargetButton != null)
        {
            RectTransform targetRectTransform = step.TargetButton.GetComponent<RectTransform>();
            if (targetRectTransform != null)
            {
                // 显示遮罩高亮(圆形)
                SetHighlight(targetRectTransform, TutorialMask.HighlightShape.Circle, Vector2.one * 20f);

                // 设置目标按钮，只允许点击该按钮时穿透遮罩
                if (tutorialMask != null)
                {
                    tutorialMask.SetTargetButton(step.TargetButton);
                }
                else
                {
                    Debug.LogError("[Tutorial] ShowClickUIGuide: tutorialMask为null！无法设置目标按钮");
                }

                // 显示箭头指针（根据配置）
                if (step.Pointer != null && step.Pointer.ShowPointer)
                {
                    ShowPointer(targetRectTransform, step.Pointer.Offset, step.Pointer.Direction);
                }
            }
        }
    }

    /// <summary>
    /// 显示对话框
    /// </summary>
    private void ShowDialog(DialogStep step)
    {
        // 显示遮罩（阻挡除对话框外的点击）
        if (tutorialMask != null)
        {
            tutorialMask.ShowMask();
        }

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);

            // 设置对话框位置
            if (step.Position == DialogPosition.Custom)
            {
                SetDialogPosition(step.CustomPosition);
            }
            else
            {
                SetDialogPosition(step.Position);
            }
        }

        // 使用打字机效果显示文本
        if (dialogText != null && typewriterEffect != null)
        {
            typewriterEffect.StartTyping(step.DialogText);
        }

        if (dialogConfirmButton != null)
        {
            dialogConfirmButton.gameObject.SetActive(step.CanSkip);
        }
    }

    /// <summary>
    /// 设置对话框位置（使用预设位置）
    /// </summary>
    private void SetDialogPosition(DialogPosition position)
    {
        if (dialogPanel == null) return;

        RectTransform rectTransform = dialogPanel.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 统一将锚点设置到左下角，避免窗口尺寸变化时对话框位置偏移
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);

        // 根据位置枚举设置偏移位置
        switch (position)
        {
            case DialogPosition.Center:
                rectTransform.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
                break;
            case DialogPosition.TopCenter:
                rectTransform.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height - 150f);
                break;
            case DialogPosition.BottomCenter:
                rectTransform.anchoredPosition = new Vector2(Screen.width / 2f, 150f);
                break;
            case DialogPosition.TopLeft:
                rectTransform.anchoredPosition = new Vector2(150f, Screen.height - 150f);
                break;
            case DialogPosition.TopRight:
                rectTransform.anchoredPosition = new Vector2(Screen.width - 150f, Screen.height - 150f);
                break;
            case DialogPosition.BottomLeft:
                rectTransform.anchoredPosition = new Vector2(150f, 150f);
                break;
            case DialogPosition.BottomRight:
                rectTransform.anchoredPosition = new Vector2(Screen.width - 150f, 150f);
                break;
        }
    }

    /// <summary>
    /// 设置对话框位置（使用自定义坐标）
    /// </summary>
    private void SetDialogPosition(Vector2 customPosition)
    {
        if (dialogPanel == null) return;

        RectTransform rectTransform = dialogPanel.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 统一将锚点设置到左下角，避免窗口尺寸变化时对话框位置偏移
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.anchoredPosition = customPosition;
    }

    /// <summary>
    /// 隐藏所有UI元素
    /// </summary>
    private void HideAllElements()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        // 停止打字机效果
        if (typewriterEffect != null)
        {
            typewriterEffect.StopTyping();
        }

        if (tutorialMask != null)
        {
            tutorialMask.HideHighlight();
        }

        if (tutorialPointer != null)
        {
            tutorialPointer.HidePointer();
        }
    }

    /// <summary>
    /// 只隐藏高亮遮罩和指针，保留对话框
    /// </summary>
    private void HideHighlightAndPointer()
    {
        if (tutorialMask != null)
        {
            tutorialMask.HideHighlight();
        }

        if (tutorialPointer != null)
        {
            tutorialPointer.HidePointer();
        }
    }

    /// <summary>
    /// 隐藏对话框
    /// </summary>
    public void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        // 停止打字机效果
        if (typewriterEffect != null)
        {
            typewriterEffect.StopTyping();
        }
    }

    /// <summary>
    /// 设置遮罩高亮区域
    /// </summary>
    public void SetHighlight(RectTransform target, TutorialMask.HighlightShape shape = TutorialMask.HighlightShape.Circle, Vector2 padding = default)
    {
        if (tutorialMask != null && target != null)
        {
            tutorialMask.ShowHighlight(target, shape, padding);
        }
    }

    /// <summary>
    /// 显示引导指针
    /// </summary>
    public void ShowPointer(RectTransform target, TutorialPointer.PointerDirection direction = TutorialPointer.PointerDirection.Auto)
    {
        if (tutorialPointer != null && target != null)
        {
            tutorialPointer.ShowPointer(target, direction);
        }
    }

    /// <summary>
    /// 显示引导指针（带自定义偏移）
    /// </summary>
    public void ShowPointer(RectTransform target, Vector2 offset, TutorialPointer.PointerDirection direction = TutorialPointer.PointerDirection.Auto)
    {
        if (tutorialPointer != null && target != null)
        {
            tutorialPointer.ShowPointer(target, offset, direction);
        }
    }

    /// <summary>
    /// 显示提示文本
    /// </summary>
    public void ShowTipText(string text, Vector3 worldPosition)
    {
        // 可以扩展显示浮动提示文本
    }

    #region 按钮回调

    private void OnDialogConfirm()
    {
        // 通知当前DialogStep确认
        if (TutorialManager.Instance.CurrentStep is DialogStep dialogStep)
        {
            dialogStep.OnConfirm();
        }
    }

    private void OnSkipClick()
    {
        TutorialManager.Instance.SkipTutorial();
    }

    /// <summary>
    /// 点击遮罩或对话框的回调
    /// 如果正在打字，则跳过打字动画；如果已经打完，则确认对话
    /// 注意：只对DialogStep有效，ClickUIStep时不处理
    /// </summary>
    private void OnClickDetected()
    {
        // 如果当前是ClickUIStep，不处理点击（让点击穿透到目标按钮）
        if (TutorialManager.Instance.CurrentStep is ClickUIStep)
        {
            return;
        }

        if (typewriterEffect != null && typewriterEffect.IsTyping)
        {
            // 正在打字，跳过打字动画直接显示完整文本
            typewriterEffect.SkipTyping();
        }
        else
        {
            // 已经打完字，等同于点击确认按钮
            OnDialogConfirm();
        }
    }

    #endregion
}
