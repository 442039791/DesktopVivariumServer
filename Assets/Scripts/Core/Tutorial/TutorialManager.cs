using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 引导系统管理器
/// 负责管理和控制整个引导流程
/// </summary>
public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance;
    public static TutorialManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TutorialManager");
                instance = go.AddComponent<TutorialManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    /// <summary>
    /// 当前引导流程
    /// </summary>
    private TutorialFlow currentFlow;

    /// <summary>
    /// 当前执行的步骤
    /// </summary>
    private TutorialStep currentStep;

    /// <summary>
    /// 当前步骤索引
    /// </summary>
    private int currentStepIndex = -1;

    /// <summary>
    /// 引导是否启用
    /// </summary>
    public bool IsTutorialEnabled { get; private set; } = true;

    /// <summary>
    /// 引导是否正在运行
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// 引导UI控制器
    /// </summary>
    public TutorialUIController UIController { get; private set; }

    /// <summary>
    /// 获取当前步骤
    /// </summary>
    public TutorialStep CurrentStep => currentStep;

    /// <summary>
    /// 引导流程完成事件
    /// </summary>
    public event System.Action<string> OnTutorialCompleted;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!IsRunning || currentStep == null) return;

        // 更新需要Update的步骤类型
        if (currentStep is DialogStep dialogStep)
        {
            dialogStep.Update(Time.deltaTime);
        }
        else if (currentStep is DelayStep delayStep)
        {
            delayStep.Update(Time.deltaTime);
        }
        else if (currentStep is ConditionStep conditionStep)
        {
            conditionStep.Update();
        }
        else if (currentStep is ClickUIStep clickUIStep)
        {
            clickUIStep.Update(Time.deltaTime);
        }
    }

    /// <summary>
    /// 设置UI控制器
    /// </summary>
    public void SetUIController(TutorialUIController controller)
    {
        UIController = controller;
    }

    /// <summary>
    /// 开始引导流程
    /// </summary>
    public void StartTutorial(TutorialFlow flow)
    {
        if (flow == null || flow.Steps == null || flow.Steps.Count == 0)
        {
            Debug.LogWarning("[Tutorial] 引导流程为空或没有步骤");
            return;
        }

        if (!IsTutorialEnabled)
        {
            Debug.Log("[Tutorial] 引导系统已禁用");
            return;
        }

        // 检查是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted(flow.FlowID))
        {
            return;
        }

        currentFlow = flow;
        currentStepIndex = -1;
        IsRunning = true;

        // 显示引导UI
        UIController?.ShowTutorial();

        // 执行第一个步骤
        NextStep();
    }

    /// <summary>
    /// 执行下一个步骤
    /// </summary>
    private void NextStep()
    {
        if (currentFlow == null) return;

        // 记录上一步是否为对话步骤
        bool previousWasDialog = currentStep is DialogStep;

        // 清理当前步骤
        if (currentStep != null)
        {
            currentStep.Cleanup();
            currentStep = null;
        }

        currentStepIndex++;

        // 检查是否所有步骤都完成
        if (currentStepIndex >= currentFlow.Steps.Count)
        {
            CompleteTutorial();
            return;
        }

        // 执行新步骤
        currentStep = currentFlow.Steps[currentStepIndex];
        currentStep.OnStepCompleted += OnStepCompleted;

        // 打印当前激活的引导信息
        Debug.Log($"[Tutorial] 激活引导: {currentFlow.FlowID} 第 {currentStepIndex + 1}/{currentFlow.Steps.Count} 步 - {currentStep.StepName} (ID: {currentStep.StepID})");

        // 如果当前步骤是点击步骤，设置上一步是否为对话步骤的标记
        if (currentStep is ClickUIStep clickStep)
        {
            clickStep.PreviousStepWasDialog = previousWasDialog;
        }

        // 先执行步骤（延迟查找按钮等操作在这里完成）
        currentStep.Execute();

        // 检查是否因找不到目标按钮而需要跳过整个流程
        if (currentStep is ClickUIStep clickStepCheck && clickStepCheck.TargetNotFound)
        {
            Debug.Log($"[Tutorial] 引导步骤 '{currentStep.StepName}' 找不到目标按钮，自动完成整个引导流程: {currentFlow?.FlowName}");
            CompleteTutorialDueToMissingTarget();
            return;
        }

        // 再更新UI（此时按钮已经找到，可以正确显示高亮）
        if (UIController != null)
        {
            UIController.UpdateStep(currentStep, currentStepIndex, currentFlow.Steps.Count);
        }
    }

    /// <summary>
    /// 步骤完成回调
    /// </summary>
    private void OnStepCompleted()
    {
        if (currentStep == null) return;

        // 保存步骤完成状态
        TutorialSaveManager.Instance.MarkStepCompleted(currentFlow.FlowID, currentStep.StepID);

        // 立即执行下一步
        NextStep();
    }

    /// <summary>
    /// 完成整个引导
    /// </summary>
    private void CompleteTutorial()
    {
        if (currentFlow == null) return;

        string completedFlowID = currentFlow.FlowID;

        // 保存流程完成状态
        TutorialSaveManager.Instance.MarkFlowCompleted(currentFlow.FlowID);

        // 清理
        if (currentStep != null)
        {
            currentStep.Cleanup();
            currentStep = null;
        }

        // 隐藏引导UI
        UIController?.HideTutorial();

        IsRunning = false;
        currentFlow = null;
        currentStepIndex = -1;

        // 触发引导完成事件
        OnTutorialCompleted?.Invoke(completedFlowID);
    }

    /// <summary>
    /// 因找不到目标按钮而完成引导流程
    /// 当引导中找不到目标按钮时，将整个引导流程标记为完成，避免玩家卡住
    /// </summary>
    private void CompleteTutorialDueToMissingTarget()
    {
        if (currentFlow == null) return;

        string completedFlowID = currentFlow.FlowID;

        Debug.Log($"[Tutorial] 因找不到目标按钮，自动完成引导流程: {currentFlow.FlowName} ({completedFlowID})");

        // 保存流程完成状态
        TutorialSaveManager.Instance.MarkFlowCompleted(currentFlow.FlowID);

        // 清理当前步骤
        if (currentStep != null)
        {
            currentStep.Cleanup();
            currentStep = null;
        }

        // 隐藏引导UI
        UIController?.HideTutorial();

        IsRunning = false;
        currentFlow = null;
        currentStepIndex = -1;

        // 触发引导完成事件
        OnTutorialCompleted?.Invoke(completedFlowID);
    }

    /// <summary>
    /// 跳过当前步骤
    /// </summary>
    public void SkipCurrentStep()
    {
        if (currentStep != null && currentStep.CanSkip)
        {
            currentStep.Skip();
        }
    }

    /// <summary>
    /// 跳过整个引导
    /// </summary>
    public void SkipTutorial()
    {
        if (!IsRunning) return;

        Debug.Log($"[Tutorial] 跳过引导流程: {currentFlow?.FlowName}");

        if (currentFlow != null)
        {
            TutorialSaveManager.Instance.MarkFlowCompleted(currentFlow.FlowID);
        }

        if (currentStep != null)
        {
            currentStep.Cleanup();
            currentStep = null;
        }

        UIController?.HideTutorial();

        IsRunning = false;
        currentFlow = null;
        currentStepIndex = -1;
    }

    /// <summary>
    /// 启用/禁用引导系统
    /// </summary>
    public void SetTutorialEnabled(bool enabled)
    {
        IsTutorialEnabled = enabled;
        Debug.Log($"[Tutorial] 引导系统 {(enabled ? "启用" : "禁用")}");
    }

    /// <summary>
    /// 重置引导进度
    /// </summary>
    public void ResetTutorial(string flowID = null)
    {
        if (string.IsNullOrEmpty(flowID))
        {
            TutorialSaveManager.Instance.ResetAllProgress();
            Debug.Log("[Tutorial] 重置所有引导进度");
        }
        else
        {
            TutorialSaveManager.Instance.ResetFlowProgress(flowID);
            Debug.Log($"[Tutorial] 重置引导流程: {flowID}");
        }
    }
}

/// <summary>
/// 引导流程
/// 包含一系列引导步骤
/// </summary>
[System.Serializable]
public class TutorialFlow
{
    /// <summary>
    /// 流程唯一ID
    /// </summary>
    public string FlowID;

    /// <summary>
    /// 流程名称
    /// </summary>
    public string FlowName;

    /// <summary>
    /// 流程描述
    /// </summary>
    public string FlowDescription;

    /// <summary>
    /// 引导步骤列表
    /// </summary>
    public List<TutorialStep> Steps;

    public TutorialFlow(string flowID, string flowName, string description = "")
    {
        FlowID = flowID;
        FlowName = flowName;
        FlowDescription = description;
        Steps = new List<TutorialStep>();
    }

    /// <summary>
    /// 添加步骤
    /// </summary>
    public void AddStep(TutorialStep step)
    {
        if (step != null)
        {
            Steps.Add(step);
        }
    }
}
