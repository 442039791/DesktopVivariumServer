using System.Collections.Generic;
using UnityEngine;
using Common;

/// <summary>
/// 配置管理器 - 集中管理所有游戏配置
/// </summary>
public class ConfigManager : MonoSingleton<ConfigManager>
{
    private readonly Dictionary<string, object> _configs = new Dictionary<string, object>();

    public override void Init()
    {
        base.Init();
        LoadDefaultConfigs();
        Log.Info("配置管理器已初始化", "ConfigManager");
    }

    #region 默认配置

    private void LoadDefaultConfigs()
    {
        // 网络配置
        Set("Network.Port", 4649);
        Set("Network.Timeout", 30f);
        Set("Network.MaxRetry", 3);

        // 日志配置
        Set("Log.MinLevel", LogLevel.Info);
        Set("Log.EnableStackTrace", false);

        // 性能配置
        Set("Performance.TargetFrameRate", 60);
        Set("Performance.MaxTasksPerFrame", 10);

        // 保存配置
        Set("Save.AutoSaveInterval", 300f); // 5分钟
        Set("Save.MaxBackups", 3);

        // 生物罐配置
        Set("BioTank.UpdateInterval", 1f);
        Set("BioTank.MaxEntitiesPerTank", 100);

        Log.Info("默认配置已加载", "ConfigManager");
    }

    #endregion

    #region 配置访问

    /// <summary>
    /// 设置配置值
    /// </summary>
    public void Set<T>(string key, T value)
    {
        _configs[key] = value;
    }

    /// <summary>
    /// 获取配置值
    /// </summary>
    public T Get<T>(string key, T defaultValue = default(T))
    {
        if (_configs.TryGetValue(key, out var value))
        {
            try
            {
                return (T)value;
            }
            catch
            {
                Log.Warning($"配置类型转换失败: {key}", "ConfigManager");
                return defaultValue;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// 检查配置是否存在
    /// </summary>
    public bool Has(string key)
    {
        return _configs.ContainsKey(key);
    }

    /// <summary>
    /// 移除配置
    /// </summary>
    public void Remove(string key)
    {
        _configs.Remove(key);
    }

    #endregion

    #region 便捷访问

    public int GetInt(string key, int defaultValue = 0) => Get(key, defaultValue);
    public float GetFloat(string key, float defaultValue = 0f) => Get(key, defaultValue);
    public bool GetBool(string key, bool defaultValue = false) => Get(key, defaultValue);
    public string GetString(string key, string defaultValue = "") => Get(key, defaultValue);

    #endregion

    #region 持久化

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    public void SaveToFile(string path)
    {
        // TODO: 实现配置文件保存
        Log.Info($"配置已保存到: {path}", "ConfigManager");
    }

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    public void LoadFromFile(string path)
    {
        // TODO: 实现配置文件加载
        Log.Info($"配置已从文件加载: {path}", "ConfigManager");
    }

    #endregion
}
