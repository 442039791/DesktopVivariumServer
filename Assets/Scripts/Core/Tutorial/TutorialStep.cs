using UnityEngine;
using System;

/// <summary>
/// 引导步骤基类
/// 所有引导步骤都继承此类
/// </summary>
public abstract class TutorialStep
{
    /// <summary>
    /// 步骤唯一ID
    /// </summary>
    public string StepID { get; protected set; }

    /// <summary>
    /// 步骤名称（用于调试）
    /// </summary>
    public string StepName { get; protected set; }

    /// <summary>
    /// 步骤是否完成
    /// </summary>
    public bool IsCompleted { get; protected set; }

    /// <summary>
    /// 步骤是否可以跳过
    /// </summary>
    public bool CanSkip { get; protected set; }

    /// <summary>
    /// 步骤完成回调
    /// </summary>
    public Action OnStepCompleted;

    /// <summary>
    /// 步骤开始回调
    /// </summary>
    public Action OnStepStarted;

    protected TutorialStep(string stepID, string stepName, bool canSkip = false)
    {
        StepID = stepID;
        StepName = stepName;
        CanSkip = canSkip;
        IsCompleted = false;
    }

    /// <summary>
    /// 开始执行步骤
    /// </summary>
    public virtual void Execute()
    {
        OnStepStarted?.Invoke();
    }

    /// <summary>
    /// 完成步骤
    /// </summary>
    public virtual void Complete()
    {
        if (IsCompleted) return;

        IsCompleted = true;
        OnStepCompleted?.Invoke();
    }

    /// <summary>
    /// 跳过步骤
    /// </summary>
    public virtual void Skip()
    {
        if (!CanSkip) return;
        Complete();
    }

    /// <summary>
    /// 清理步骤（取消监听等）
    /// </summary>
    public virtual void Cleanup()
    {
        OnStepCompleted = null;
        OnStepStarted = null;
    }
}
