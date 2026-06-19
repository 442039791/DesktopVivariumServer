using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Steam所有权配置
/// 用于配置Demo/完整版的App ID和功能解锁设置
/// </summary>
[CreateAssetMenu(fileName = "SteamOwnershipConfig", menuName = "Steam/Ownership Config")]
public class SteamOwnershipConfig : ScriptableObject
{
    [Header("App ID 配置")]
    [Tooltip("当前Demo的App ID")]
    public uint demoAppId;

    [Tooltip("完整版游戏的App ID")]
    public uint fullGameAppId;

    [Header("功能解锁配置")]
    [Tooltip("需要完整版才能解锁的功能列表")]
    public List<LockedFeature> lockedFeatures = new List<LockedFeature>();

    /// <summary>
    /// 检查功能是否被锁定（需要完整版）
    /// </summary>
    /// <param name="featureId">功能ID</param>
    /// <returns>功能配置，如果未找到返回null</returns>
    public LockedFeature GetFeature(string featureId)
    {
        return lockedFeatures.Find(f => f.featureId == featureId);
    }

    /// <summary>
    /// 检查功能是否需要完整版
    /// </summary>
    /// <param name="featureId">功能ID</param>
    /// <returns>是否需要完整版（未配置的功能默认不需要）</returns>
    public bool IsFeatureLocked(string featureId)
    {
        var feature = GetFeature(featureId);
        return feature != null && feature.requiresFullVersion;
    }
}

/// <summary>
/// 锁定功能配置
/// </summary>
[System.Serializable]
public class LockedFeature
{
    [Tooltip("功能的唯一标识符")]
    public string featureId;

    [Tooltip("功能的显示名称")]
    public string displayName;

    [Tooltip("功能描述")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("是否需要完整版才能使用")]
    public bool requiresFullVersion = true;

    [Tooltip("提示用户购买时显示的消息")]
    [TextArea(2, 4)]
    public string purchasePromptMessage;
}
