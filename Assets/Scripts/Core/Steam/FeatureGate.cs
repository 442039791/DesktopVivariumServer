#if !DISABLESTEAMWORKS && STEAMWORKSNET
using UnityEngine;
using System;

/// <summary>
/// 功能门控静态辅助类
/// 提供简便的方法来检查功能是否可用（基于完整版所有权）
/// </summary>
public static class FeatureGate
{
    private static SteamOwnershipConfig _config;

    /// <summary>
    /// 设置所有权配置
    /// </summary>
    public static void SetConfig(SteamOwnershipConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// 检查是否拥有完整版
    /// </summary>
    public static bool HasFullVersion
    {
        get
        {
            if (SteamOwnershipManager.Instance == null)
            {
                Debug.LogWarning("[FeatureGate] SteamOwnershipManager未初始化");
                return false;
            }
            return SteamOwnershipManager.Instance.IsFullVersionOwned;
        }
    }

    /// <summary>
    /// 检查功能是否可访问
    /// </summary>
    /// <param name="featureId">功能ID</param>
    /// <returns>功能是否可用</returns>
    public static bool CanAccess(string featureId)
    {
        // 如果拥有完整版，所有功能都可用
        if (HasFullVersion)
        {
            return true;
        }

        // 如果没有配置，默认允许访问
        if (_config == null)
        {
            return true;
        }

        // 检查功能是否被锁定
        return !_config.IsFeatureLocked(featureId);
    }

    /// <summary>
    /// 检查功能是否可访问，如果不可访问则显示购买提示
    /// </summary>
    /// <param name="featureId">功能ID</param>
    /// <param name="onLocked">功能被锁定时的回调（可选，用于显示自定义UI）</param>
    /// <returns>功能是否可用</returns>
    public static bool TryAccess(string featureId, Action<LockedFeature> onLocked = null)
    {
        if (CanAccess(featureId))
        {
            return true;
        }

        // 功能被锁定
        var feature = _config?.GetFeature(featureId);

        if (onLocked != null)
        {
            // 使用自定义回调处理
            onLocked.Invoke(feature);
        }
        else
        {
            // 默认行为：打开Steam商店页面
            SteamOwnershipManager.Instance?.OpenStorePage();
        }

        Debug.Log($"[FeatureGate] 功能 [{featureId}] 需要购买完整版");
        return false;
    }

    /// <summary>
    /// 执行需要完整版的操作
    /// 如果拥有完整版则执行，否则显示购买提示
    /// </summary>
    /// <param name="featureId">功能ID</param>
    /// <param name="action">要执行的操作</param>
    /// <param name="onLocked">功能被锁定时的回调</param>
    public static void Execute(string featureId, Action action, Action<LockedFeature> onLocked = null)
    {
        if (TryAccess(featureId, onLocked))
        {
            action?.Invoke();
        }
    }

    /// <summary>
    /// 获取功能的锁定信息
    /// </summary>
    /// <param name="featureId">功能ID</param>
    /// <returns>锁定功能配置，如果未配置返回null</returns>
    public static LockedFeature GetFeatureInfo(string featureId)
    {
        return _config?.GetFeature(featureId);
    }

    /// <summary>
    /// 打开Steam商店购买完整版
    /// </summary>
    public static void OpenPurchasePage()
    {
        SteamOwnershipManager.Instance?.OpenStorePage();
    }
}
#endif
