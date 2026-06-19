using UnityEngine;
using UnityEngine.UI;
using System;
using SUIFW;

/// <summary>
/// 指针配置
/// </summary>
public class PointerConfig
{
    /// <summary>
    /// 是否显示指针
    /// </summary>
    public bool ShowPointer { get; set; } = true;

    /// <summary>
    /// 指针相对于目标的偏移位置
    /// </summary>
    public Vector2 Offset { get; set; } = new Vector2(0, 80);

    /// <summary>
    /// 指针方向
    /// </summary>
    public TutorialPointer.PointerDirection Direction { get; set; } = TutorialPointer.PointerDirection.Auto;

    /// <summary>
    /// 默认配置
    /// </summary>
    public static PointerConfig Default => new PointerConfig();

    /// <summary>
    /// 不显示指针的配置
    /// </summary>
    public static PointerConfig None => new PointerConfig { ShowPointer = false };
}

/// <summary>
/// 点击UI引导步骤
/// 等待玩家点击指定UI元素
/// </summary>
public class ClickUIStep : TutorialStep
{
    private Button targetButton;
    private string targetButtonPath;
    private string rootObjectName;
    private bool useMessageCenter;
    private string messageType;
    private bool useDeferredFind;
    private Func<Button> buttonFinder;

    /// <summary>
    /// 自动完成超时时间（秒）
    /// 当玩家在指定时间内没有执行点击操作时，步骤自动完成
    /// </summary>
    private float autoCompleteTimeout = 5f;

    /// <summary>
    /// 已等待的时间
    /// </summary>
    private float elapsedTime;

    /// <summary>
    /// 是否正在等待超时
    /// </summary>
    private bool isWaitingForTimeout;

    /// <summary>
    /// 获取目标按钮(用于UI高亮)
    /// </summary>
    public Button TargetButton => targetButton;

    /// <summary>
    /// 上一步是否为对话步骤
    /// 如果为true，则在点击操作完成后才关闭对话框
    /// </summary>
    public bool PreviousStepWasDialog { get; set; }

    /// <summary>
    /// 指针配置
    /// </summary>
    public PointerConfig Pointer { get; set; } = PointerConfig.Default;

    /// <summary>
    /// 通过Button引用创建点击引导
    /// </summary>
    public ClickUIStep(string stepID, string stepName, Button button, bool canSkip = false)
        : base(stepID, stepName, canSkip)
    {
        targetButton = button;
        useMessageCenter = false;
        useDeferredFind = false;
    }

    /// <summary>
    /// 通过消息中心创建点击引导
    /// </summary>
    public ClickUIStep(string stepID, string stepName, string msgType, bool canSkip = false)
        : base(stepID, stepName, canSkip)
    {
        messageType = msgType;
        useMessageCenter = true;
        useDeferredFind = false;
    }

    /// <summary>
    /// 通过路径延迟查找按钮创建点击引导
    /// 在Execute时才查找按钮，解决UI未打开时无法找到按钮的问题
    /// </summary>
    /// <param name="stepID">步骤ID</param>
    /// <param name="stepName">步骤名称</param>
    /// <param name="rootObject">根对象名称（用GameObject.Find查找）</param>
    /// <param name="buttonPath">按钮相对于根对象的路径</param>
    /// <param name="canSkip">是否可跳过</param>
    public ClickUIStep(string stepID, string stepName, string rootObject, string buttonPath, bool canSkip = false)
        : base(stepID, stepName, canSkip)
    {
        rootObjectName = rootObject;
        targetButtonPath = buttonPath;
        useMessageCenter = false;
        useDeferredFind = true;
    }

    /// <summary>
    /// 通过自定义查找函数延迟查找按钮创建点击引导
    /// 在Execute时才调用查找函数，解决UI未打开时无法找到按钮的问题
    /// </summary>
    /// <param name="stepID">步骤ID</param>
    /// <param name="stepName">步骤名称</param>
    /// <param name="finder">返回Button的查找函数</param>
    /// <param name="canSkip">是否可跳过</param>
    public ClickUIStep(string stepID, string stepName, Func<Button> finder, bool canSkip = false)
        : base(stepID, stepName, canSkip)
    {
        buttonFinder = finder;
        useMessageCenter = false;
        useDeferredFind = true;
    }

    /// <summary>
    /// 标记是否因找不到目标按钮而需要跳过整个流程
    /// </summary>
    public bool TargetNotFound { get; private set; }

    public override void Execute()
    {
        base.Execute();
        TargetNotFound = false;
        elapsedTime = 0f;
        isWaitingForTimeout = true;

        // 延迟查找按钮
        if (useDeferredFind && targetButton == null)
        {
            if (buttonFinder != null)
            {
                // 使用自定义查找函数
                targetButton = buttonFinder();
            }
            else if (!string.IsNullOrEmpty(rootObjectName) && !string.IsNullOrEmpty(targetButtonPath))
            {
                // 使用路径查找
                GameObject rootObj = GameObject.Find(rootObjectName);
                if (rootObj != null)
                {
                    Transform buttonTransform = rootObj.transform.Find(targetButtonPath);
                    if (buttonTransform != null)
                    {
                        targetButton = buttonTransform.GetComponent<Button>();
                    }
                }
            }
        }

        if (useMessageCenter)
        {
            MessageCenter.AddMsgListener(messageType, OnMessageReceived);
        }
        else if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnButtonClicked);
        }
        else if (!useMessageCenter)
        {
            // 找不到目标按钮且不是消息中心模式，标记需要跳过流程
            Debug.LogWarning($"[Tutorial] 找不到目标按钮，将跳过当前引导流程。步骤: {StepName}, 根对象: {rootObjectName}, 路径: {targetButtonPath}");
            TargetNotFound = true;
        }
    }

    /// <summary>
    /// 更新超时检测（需要在外部Update中调用）
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!isWaitingForTimeout || IsCompleted) return;

        elapsedTime += deltaTime;
        if (elapsedTime >= autoCompleteTimeout)
        {
            Debug.Log($"[Tutorial] 点击引导步骤 '{StepName}' 超时 {autoCompleteTimeout} 秒，自动完成");

            // 如果上一步是对话步骤，需要关闭对话框
            if (PreviousStepWasDialog)
            {
                TutorialManager.Instance.UIController?.HideDialog();
            }

            isWaitingForTimeout = false;
            Complete();
        }
    }

    private void OnButtonClicked()
    {
        isWaitingForTimeout = false;

        // 如果上一步是对话步骤，需要关闭对话框
        if (PreviousStepWasDialog)
        {
            TutorialManager.Instance.UIController?.HideDialog();
        }

        Complete();
    }

    private void OnMessageReceived(KeyValuesUpdate kv)
    {
        isWaitingForTimeout = false;

        // 如果上一步是对话步骤，需要关闭对话框
        if (PreviousStepWasDialog)
        {
            TutorialManager.Instance.UIController?.HideDialog();
        }

        Complete();
    }

    public override void Cleanup()
    {
        if (useMessageCenter)
        {
            MessageCenter.RemoveMsgListener(messageType, OnMessageReceived);
        }
        else if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(OnButtonClicked);
        }

        base.Cleanup();
    }
}

/// <summary>
/// 对话框位置枚举
/// </summary>
public enum DialogPosition
{
    Center,         // 屏幕中央（默认）
    TopCenter,      // 中上
    BottomCenter,   // 中下
    TopLeft,        // 左上
    TopRight,       // 右上
    BottomLeft,     // 左下
    BottomRight,    // 右下
    Custom          // 自定义位置
}

/// <summary>
/// 对话/提示引导步骤
/// 显示文本提示，等待玩家确认
/// </summary>
public class DialogStep : TutorialStep
{
    public string DialogText { get; private set; }
    public float AutoCompleteDelay { get; private set; }
    public DialogPosition Position { get; private set; }
    /// <summary>
    /// 自定义位置坐标（仅当Position为Custom时使用）
    /// </summary>
    public Vector2 CustomPosition { get; private set; }
    private float elapsedTime;
    private bool isWaiting;

    public DialogStep(string stepID, string stepName, string dialogText, float autoCompleteDelay = 0f, bool canSkip = true, DialogPosition position = DialogPosition.Center)
        : base(stepID, stepName, canSkip)
    {
        DialogText = dialogText;
        AutoCompleteDelay = autoCompleteDelay;
        Position = position;
        CustomPosition = Vector2.zero;
    }

    /// <summary>
    /// 使用自定义坐标创建对话步骤
    /// </summary>
    public DialogStep(string stepID, string stepName, string dialogText, Vector2 customPosition, float autoCompleteDelay = 0f, bool canSkip = true)
        : base(stepID, stepName, canSkip)
    {
        DialogText = dialogText;
        AutoCompleteDelay = autoCompleteDelay;
        Position = DialogPosition.Custom;
        CustomPosition = customPosition;
    }

    public override void Execute()
    {
        base.Execute();
        elapsedTime = 0f;
        isWaiting = AutoCompleteDelay > 0f;
    }

    /// <summary>
    /// 更新等待时间（需要在外部Update中调用）
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!isWaiting || IsCompleted) return;

        elapsedTime += deltaTime;
        if (elapsedTime >= AutoCompleteDelay)
        {
            Complete();
        }
    }

    /// <summary>
    /// 玩家点击确认
    /// </summary>
    public void OnConfirm()
    {
        Complete();
    }
}

/// <summary>
/// 条件引导步骤
/// 等待某个条件满足后完成
/// </summary>
public class ConditionStep : TutorialStep
{
    private Func<bool> condition;
    private float checkInterval;
    private float nextCheckTime;

    public ConditionStep(string stepID, string stepName, Func<bool> checkCondition, float interval = 0.5f, bool canSkip = false)
        : base(stepID, stepName, canSkip)
    {
        condition = checkCondition;
        checkInterval = interval;
    }

    public override void Execute()
    {
        base.Execute();
        nextCheckTime = Time.time;
    }

    /// <summary>
    /// 检查条件（需要在外部Update中调用）
    /// </summary>
    public void Update()
    {
        if (IsCompleted) return;

        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;

            if (condition != null && condition())
            {
                Complete();
            }
        }
    }
}

/// <summary>
/// 延迟引导步骤
/// 等待一段时间后自动完成
/// </summary>
public class DelayStep : TutorialStep
{
    private float delayTime;
    private float elapsedTime;

    public DelayStep(string stepID, string stepName, float delay, bool canSkip = true)
        : base(stepID, stepName, canSkip)
    {
        delayTime = delay;
    }

    public override void Execute()
    {
        base.Execute();
        elapsedTime = 0f;
    }

    /// <summary>
    /// 更新延迟时间（需要在外部Update中调用）
    /// </summary>
    public void Update(float deltaTime)
    {
        if (IsCompleted) return;

        elapsedTime += deltaTime;
        if (elapsedTime >= delayTime)
        {
            Complete();
        }
    }
}

/// <summary>
/// 自定义引导步骤
/// 通过回调函数自定义执行逻辑
/// </summary>
public class CustomStep : TutorialStep
{
    private Action executeAction;
    private Action cleanupAction;

    public CustomStep(string stepID, string stepName, Action onExecute, Action onCleanup = null, bool canSkip = false)
        : base(stepID, stepName, canSkip)
    {
        executeAction = onExecute;
        cleanupAction = onCleanup;
    }

    public override void Execute()
    {
        base.Execute();
        executeAction?.Invoke();
    }

    public override void Cleanup()
    {
        cleanupAction?.Invoke();
        base.Cleanup();
    }

    /// <summary>
    /// 手动完成步骤（从外部调用）
    /// </summary>
    public void CompleteManually()
    {
        Complete();
    }
}
