#if !DISABLESTEAMWORKS && STEAMWORKSNET
using UnityEngine;
using System;
using System.Collections.Generic;
using Steamworks;

/// <summary>
/// Steam成就管理器
/// 负责解锁、查询和管理Steam成就
/// </summary>
public class SteamAchievementManager : MonoBehaviour
{
    [Header("成就配置")]
    [SerializeField] private SteamAchievementConfig _achievementConfig;

    [Header("设置")]
    [Tooltip("是否启用详细日志")]
    [SerializeField] private bool _enableDebugLog = true;

    // 单例
    private static SteamAchievementManager _instance;
    public static SteamAchievementManager Instance => _instance;

    // Steam API是否已初始化
    private bool _steamInitialized = false;

    // Stats是否已就绪
    private bool _statsReady = false;

    // 回调委托
    private Callback<UserStatsReceived_t> _userStatsReceivedCallback;
    private Callback<UserStatsStored_t> _userStatsStoredCallback;
    private Callback<UserAchievementStored_t> _userAchievementStoredCallback;

    // 成就解锁事件
    public event Action<string> OnAchievementUnlocked;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeSteamAchievements();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 初始化Steam成就系统
    /// </summary>
    private void InitializeSteamAchievements()
    {
        try
        {
            // 检查Steam是否初始化
            _steamInitialized = SteamAPI.IsSteamRunning();

            if (!_steamInitialized)
            {
                LogWarning("Steam未运行或未初始化,成就系统将不可用");
                return;
            }

            // 注册回调
            _userStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            _userStatsStoredCallback = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            _userAchievementStoredCallback = Callback<UserAchievementStored_t>.Create(OnUserAchievementStored);

            // 注意: 新版Steamworks.NET中,Steam客户端会自动同步统计数据
            // 不需要手动调用RequestCurrentStats(),等待UserStatsReceived_t回调即可
            // Stats会在Steam初始化后自动就绪

            Log("Steam成就管理器初始化成功,等待统计数据就绪...");
        }
        catch (Exception e)
        {
            LogError($"初始化Steam成就系统异常: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 检查统计数据是否就绪
    /// 注意: 新版Steamworks.NET会自动同步,无需手动请求
    /// </summary>
    public bool IsStatsReady()
    {
        return _steamInitialized && _statsReady;
    }

    /// <summary>
    /// 解锁成就
    /// </summary>
    /// <param name="achievementId">成就ID(Steam后台的API Name)</param>
    /// <returns>是否成功标记解锁(仍需调用StoreStats保存)</returns>
    public bool UnlockAchievement(string achievementId)
    {
        if (!_steamInitialized)
        {
            LogWarning($"Steam未初始化,无法解锁成就: {achievementId}");
            return false;
        }

        if (!_statsReady)
        {
            LogWarning($"Stats尚未就绪,无法解锁成就: {achievementId}");
            return false;
        }

        // 检查是否已解锁
        if (IsAchievementUnlocked(achievementId))
        {
            Log($"成就已经解锁: {achievementId}");
            return true;
        }

        try
        {
            // 标记成就为已解锁
            bool result = SteamUserStats.SetAchievement(achievementId);

            if (result)
            {
                Log($"成就已标记为解锁: {achievementId}");

                // 立即保存到Steam服务器
                StoreStats();

                return true;
            }
            else
            {
                LogError($"标记成就解锁失败: {achievementId}");
                return false;
            }
        }
        catch (Exception e)
        {
            LogError($"解锁成就异常 [{achievementId}]: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 保存统计数据到Steam服务器
    /// </summary>
    public bool StoreStats()
    {
        if (!_steamInitialized || !_statsReady)
        {
            LogWarning("Steam未初始化或Stats未就绪,无法保存");
            return false;
        }

        bool result = SteamUserStats.StoreStats();

        if (result)
        {
            Log("统计数据已提交到Steam服务器");
        }
        else
        {
            LogWarning("保存统计数据失败");
        }

        return result;
    }

    /// <summary>
    /// 检查成就是否已解锁
    /// </summary>
    public bool IsAchievementUnlocked(string achievementId)
    {
        if (!_steamInitialized || !_statsReady)
        {
            return false;
        }

        bool achieved;
        bool result = SteamUserStats.GetAchievement(achievementId, out achieved);

        if (!result)
        {
            LogWarning($"获取成就状态失败: {achievementId}");
            return false;
        }

        return achieved;
    }

    /// <summary>
    /// 清除成就(仅用于测试,正式发布时应禁用)
    /// </summary>
    public bool ClearAchievement(string achievementId)
    {
        if (!_steamInitialized || !_statsReady)
        {
            LogWarning("Steam未初始化或Stats未就绪,无法清除成就");
            return false;
        }

        bool result = SteamUserStats.ClearAchievement(achievementId);

        if (result)
        {
            Log($"成就已清除: {achievementId}");
            StoreStats();
        }
        else
        {
            LogWarning($"清除成就失败: {achievementId}");
        }

        return result;
    }

    /// <summary>
    /// 获取成就解锁时间
    /// </summary>
    public bool GetAchievementUnlockTime(string achievementId, out uint unlockTime)
    {
        unlockTime = 0;

        if (!_steamInitialized || !_statsReady)
        {
            return false;
        }

        bool achieved;
        bool result = SteamUserStats.GetAchievementAndUnlockTime(achievementId, out achieved, out unlockTime);

        return result && achieved;
    }

    /// <summary>
    /// 获取成就的全局解锁百分比
    /// </summary>
    public bool GetAchievementGlobalPercentage(string achievementId, out float percentage)
    {
        percentage = 0f;

        if (!_steamInitialized)
        {
            return false;
        }

        return SteamUserStats.GetAchievementAchievedPercent(achievementId, out percentage);
    }

    /// <summary>
    /// 重置所有成就(仅用于测试)
    /// </summary>
    public bool ResetAllAchievements()
    {
        if (!_steamInitialized)
        {
            LogWarning("Steam未初始化,无法重置成就");
            return false;
        }

        bool result = SteamUserStats.ResetAllStats(true);

        if (result)
        {
            Log("所有成就已重置,等待统计数据重新同步...");
            _statsReady = false; // 等待重新同步
        }
        else
        {
            LogError("重置成就失败");
        }

        return result;
    }

    #region Steam回调处理

    /// <summary>
    /// 当用户统计数据接收完成时调用
    /// 注意: Steam会自动触发此回调,无需手动请求
    /// </summary>
    private void OnUserStatsReceived(UserStatsReceived_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            _statsReady = true;
            Log("用户统计数据已就绪,成就系统可用");
        }
        else
        {
            LogError($"接收用户统计数据失败: {callback.m_eResult}");
        }
    }

    /// <summary>
    /// 当用户统计数据保存完成时调用
    /// </summary>
    private void OnUserStatsStored(UserStatsStored_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            Log("用户统计数据保存成功");
        }
        else
        {
            LogError($"保存用户统计数据失败: {callback.m_eResult}");
        }
    }

    /// <summary>
    /// 当成就解锁并保存到服务器时调用
    /// </summary>
    private void OnUserAchievementStored(UserAchievementStored_t callback)
    {
        string achievementId = callback.m_rgchAchievementName;

        Log($"成就已成功保存到Steam服务器: {achievementId}");

        // 触发成就解锁事件
        OnAchievementUnlocked?.Invoke(achievementId);

        // 如果有配置,显示成就信息
        if (_achievementConfig != null)
        {
            var achievementData = _achievementConfig.GetAchievement(achievementId);
            if (achievementData != null)
            {
                Log($"成就解锁: {achievementData.displayName} - {achievementData.description}");
            }
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 设置成就配置
    /// </summary>
    public void SetAchievementConfig(SteamAchievementConfig config)
    {
        _achievementConfig = config;
    }

    /// <summary>
    /// 获取所有配置的成就
    /// </summary>
    public List<AchievementData> GetAllAchievements()
    {
        return _achievementConfig?.achievements ?? new List<AchievementData>();
    }

    /// <summary>
    /// 检查Steam是否可用
    /// </summary>
    public bool IsSteamAvailable()
    {
        return _steamInitialized && _statsReady;
    }

    #endregion

    #region 日志方法

    private void Log(string message)
    {
        if (_enableDebugLog)
        {
            Debug.Log($"[SteamAchievements] {message}");
        }
    }

    private void LogWarning(string message)
    {
        if (_enableDebugLog)
        {
            Debug.LogWarning($"[SteamAchievements] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[SteamAchievements] {message}");
    }

    #endregion
}
#endif
