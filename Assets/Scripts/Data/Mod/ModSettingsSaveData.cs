using System;
using System.Collections.Generic;

/// <summary>
/// 用户的 Mod 配置保存数据
/// 存储用户的 Mod 启用状态和加载顺序等设置
/// </summary>
[System.Serializable]
public class ModSettingsSaveData
{
    /// <summary>
    /// 各个 Mod 的配置
    /// </summary>
    public List<ModUserSetting> ModSettings;

    /// <summary>
    /// 全局启用 Mod 系统
    /// </summary>
    public bool ModSystemEnabled;

    /// <summary>
    /// 当前运行环境是否允许激活 Mod（例如未拥有完整版时为 false）
    /// </summary>
    public bool RuntimeModAccessAllowed;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public string LastModified;

    public ModSettingsSaveData()
    {
        ModSettings = new List<ModUserSetting>();
        ModSystemEnabled = true;
        RuntimeModAccessAllowed = true;
    }

    /// <summary>
    /// 获取指定 Mod 的设置
    /// </summary>
    public ModUserSetting GetSetting(string modId)
    {
        return ModSettings?.Find(s => s.ModId == modId);
    }

    /// <summary>
    /// 设置 Mod 的启用状态
    /// </summary>
    public void SetEnabled(string modId, bool enabled)
    {
        var setting = GetSetting(modId);
        if (setting == null)
        {
            setting = new ModUserSetting { ModId = modId };
            ModSettings.Add(setting);
        }
        setting.IsEnabled = enabled;
        LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 设置 Mod 的加载顺序
    /// </summary>
    public void SetLoadOrder(string modId, int order)
    {
        var setting = GetSetting(modId);
        if (setting == null)
        {
            setting = new ModUserSetting { ModId = modId };
            ModSettings.Add(setting);
        }
        setting.LoadOrder = order;
        LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 移除 Mod 设置
    /// </summary>
    public bool RemoveSetting(string modId)
    {
        var index = ModSettings?.FindIndex(s => s.ModId == modId) ?? -1;
        if (index >= 0)
        {
            ModSettings.RemoveAt(index);
            LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return true;
        }
        return false;
    }
}

/// <summary>
/// 单个 Mod 的用户设置
/// </summary>
[System.Serializable]
public class ModUserSetting
{
    /// <summary>
    /// Mod 标识符
    /// </summary>
    public string ModId;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled;

    /// <summary>
    /// 加载顺序（数字越小越先加载）
    /// </summary>
    public int LoadOrder;

    public ModUserSetting()
    {
        IsEnabled = true;
        LoadOrder = 100;
    }
}
