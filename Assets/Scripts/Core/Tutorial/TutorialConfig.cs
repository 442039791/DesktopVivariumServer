using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 引导配置
/// 使用ScriptableObject方式配置引导流程
/// </summary>
[CreateAssetMenu(fileName = "TutorialConfig", menuName = "Tutorial/Tutorial Config")]
public class TutorialConfig : ScriptableObject
{
    [Header("流程信息")]
    public string flowID;
    public string flowName;
    [TextArea(3, 5)]
    public string flowDescription;

    [Header("步骤配置")]
    public List<StepConfig> steps = new List<StepConfig>();

    /// <summary>
    /// 创建引导流程
    /// </summary>
    public TutorialFlow CreateFlow()
    {
        TutorialFlow flow = new TutorialFlow(flowID, flowName, flowDescription);

        foreach (var stepConfig in steps)
        {
            TutorialStep step = CreateStep(stepConfig);
            if (step != null)
            {
                flow.AddStep(step);
            }
        }

        return flow;
    }

    /// <summary>
    /// 根据配置创建步骤
    /// </summary>
    private TutorialStep CreateStep(StepConfig config)
    {
        switch (config.stepType)
        {
            case StepType.Dialog:
                return new DialogStep(
                    config.stepID,
                    config.stepName,
                    config.dialogText,
                    config.autoCompleteDelay,
                    config.canSkip
                );

            case StepType.Delay:
                return new DelayStep(
                    config.stepID,
                    config.stepName,
                    config.delayTime,
                    config.canSkip
                );

            case StepType.ClickUI:
                // 点击UI步骤需要在运行时动态创建，因为需要引用场景中的UI
                // 这里返回null，由外部代码处理
                Debug.LogWarning($"[Tutorial] ClickUI步骤需要在运行时创建: {config.stepName}");
                return null;

            case StepType.Custom:
                // 自定义步骤需要在代码中创建
                Debug.LogWarning($"[Tutorial] Custom步骤需要在代码中创建: {config.stepName}");
                return null;

            default:
                Debug.LogWarning($"[Tutorial] 未知的步骤类型: {config.stepType}");
                return null;
        }
    }
}

/// <summary>
/// 步骤配置
/// </summary>
[System.Serializable]
public class StepConfig
{
    [Header("基础信息")]
    public string stepID;
    public string stepName;
    public StepType stepType;
    public bool canSkip = true;

    [Header("对话步骤配置")]
    [TextArea(2, 4)]
    public string dialogText;
    public float autoCompleteDelay = 0f;

    [Header("延迟步骤配置")]
    public float delayTime = 1f;

    [Header("点击UI步骤配置")]
    public string targetUIPath;  // UI路径或名称
    public string messageType;   // 或者使用消息类型

    [Header("UI提示配置")]
    public bool showHighlight = true;
    public HighlightShape highlightShape = HighlightShape.Circle;
    public bool showPointer = true;
    public PointerDirection pointerDirection = PointerDirection.Auto;
}

/// <summary>
/// 步骤类型枚举
/// </summary>
public enum StepType
{
    Dialog,      // 对话/提示
    ClickUI,     // 点击UI
    Delay,       // 延迟
    Condition,   // 条件
    Custom       // 自定义
}

/// <summary>
/// 高亮形状枚举
/// </summary>
public enum HighlightShape
{
    Circle,
    Rectangle
}

/// <summary>
/// 指针方向枚举
/// </summary>
public enum PointerDirection
{
    Auto,
    Up,
    Down,
    Left,
    Right
}
