using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mod 加载状态
/// </summary>
public enum ModLoadState
{
    /// <summary>
    /// 未加载
    /// </summary>
    Unloaded,
    /// <summary>
    /// 正在加载
    /// </summary>
    Loading,
    /// <summary>
    /// 已加载
    /// </summary>
    Loaded,
    /// <summary>
    /// 加载失败
    /// </summary>
    Failed,
    /// <summary>
    /// 已禁用
    /// </summary>
    Disabled
}

/// <summary>
/// Mod 类型
/// </summary>
public enum ModType
{
    /// <summary>
    /// 本地 Mod
    /// </summary>
    Local,
    /// <summary>
    /// Steam Workshop Mod
    /// </summary>
    Workshop
}

/// <summary>
/// Mod 运行时信息
/// 包含 Mod 的加载状态、路径等运行时数据
/// </summary>
[System.Serializable]
public class ModInfo
{
    /// <summary>
    /// Mod 唯一标识符 (来自 manifest.json)
    /// </summary>
    public string ModId;

    /// <summary>
    /// Mod 清单信息
    /// </summary>
    public ModManifest Manifest;

    /// <summary>
    /// Mod 类型（本地或创意工坊）
    /// </summary>
    public ModType Type;

    /// <summary>
    /// Mod 根目录路径
    /// </summary>
    public string RootPath;

    /// <summary>
    /// 当前加载状态
    /// </summary>
    public ModLoadState LoadState;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled;

    /// <summary>
    /// 加载优先级（数字越小越先加载）
    /// </summary>
    public int LoadOrder;

    /// <summary>
    /// Steam Workshop Item ID（仅 Workshop 类型有效）
    /// </summary>
    public ulong WorkshopItemId;

    /// <summary>
    /// 加载失败时的错误信息
    /// </summary>
    public string ErrorMessage;

    /// <summary>
    /// 已加载的 AssetBundle 列表
    /// </summary>
    public List<string> LoadedBundles;

    /// <summary>
    /// Mod 加载时间戳
    /// </summary>
    public DateTime LoadedTime;

    public ModInfo()
    {
        LoadedBundles = new List<string>();
        LoadState = ModLoadState.Unloaded;
        IsEnabled = true;
        LoadOrder = 100;
    }

    /// <summary>
    /// 获取 Mod 显示名称
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (Manifest != null && !string.IsNullOrEmpty(Manifest.Name))
                return Manifest.Name;
            return ModId ?? "Unknown Mod";
        }
    }

    /// <summary>
    /// 获取配置文件目录路径
    /// </summary>
    public string GetConfigPath()
    {
        return System.IO.Path.Combine(RootPath, "ConfigData");
    }

    /// <summary>
    /// 获取 AssetBundle 目录路径
    /// </summary>
    public string GetAssetBundlePath()
    {
        return System.IO.Path.Combine(RootPath, "AssetBundles");
    }

    /// <summary>
    /// 检查是否可以加载
    /// </summary>
    public bool CanLoad()
    {
        return IsEnabled &&
               LoadState != ModLoadState.Loading &&
               LoadState != ModLoadState.Loaded;
    }

    /// <summary>
    /// 检查是否可以卸载
    /// </summary>
    public bool CanUnload()
    {
        return LoadState == ModLoadState.Loaded;
    }

    /// <summary>
    /// 深拷贝
    /// </summary>
    public ModInfo DeepCopy()
    {
        var copy = new ModInfo
        {
            ModId = this.ModId,
            Manifest = this.Manifest?.DeepCopy(),
            Type = this.Type,
            RootPath = this.RootPath,
            LoadState = this.LoadState,
            IsEnabled = this.IsEnabled,
            LoadOrder = this.LoadOrder,
            WorkshopItemId = this.WorkshopItemId,
            ErrorMessage = this.ErrorMessage,
            LoadedTime = this.LoadedTime
        };
        copy.LoadedBundles.AddRange(this.LoadedBundles);
        return copy;
    }
}
