using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Steam成就配置数据
/// 在Unity编辑器中创建并配置成就信息
/// </summary>
[CreateAssetMenu(fileName = "SteamAchievementConfig", menuName = "Steam/Achievement Config")]
public class SteamAchievementConfig : ScriptableObject
{
    [Tooltip("所有Steam成就的配置列表")]
    public List<AchievementData> achievements = new List<AchievementData>();

    /// <summary>
    /// 根据成就ID获取成就数据
    /// </summary>
    public AchievementData GetAchievement(string achievementId)
    {
        return achievements.Find(a => a.achievementId == achievementId);
    }

    /// <summary>
    /// 检查成就ID是否存在
    /// </summary>
    public bool HasAchievement(string achievementId)
    {
        return achievements.Exists(a => a.achievementId == achievementId);
    }
}

/// <summary>
/// 单个成就的数据结构
/// </summary>
[Serializable]
public class AchievementData
{
    [Header("成就标识")]
    [Tooltip("Steam后台配置的成就API Name(必须与Steamworks后台一致)")]
    public string achievementId;

    [Header("成就信息(仅用于编辑器显示,实际信息由Steam后台管理)")]
    [Tooltip("成就名称")]
    public string displayName;

    [Tooltip("成就描述")]
    [TextArea(2, 4)]
    public string description;

    [Header("成就类型")]
    [Tooltip("是否为隐藏成就")]
    public bool isHidden = false;

    [Tooltip("成就分类标签")]
    public string category = "General";

    [Header("调试选项")]
    [Tooltip("是否在编辑器中启用调试日志")]
    public bool enableDebugLog = true;
}
