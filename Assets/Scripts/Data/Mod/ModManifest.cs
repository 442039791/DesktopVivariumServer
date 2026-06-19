using System;
using System.Collections.Generic;

/// <summary>
/// Mod 清单文件 - 简化版
/// Mod 资源按项目目录结构放置，自动覆盖同名资源
/// </summary>
[System.Serializable]
public class ModManifest
{
    /// <summary>
    /// Mod 唯一标识符 (如 "SoftKitty.TropicalFish")
    /// </summary>
    public string Id;

    /// <summary>
    /// Mod 显示名称
    /// </summary>
    public string Name;

    /// <summary>
    /// Mod 版本号 (如 "1.0.0")
    /// </summary>
    public string Version;

    /// <summary>
    /// Mod 作者
    /// </summary>
    public string Author;

    /// <summary>
    /// Mod 描述
    /// </summary>
    public string Description;

    /// <summary>
    /// 最低游戏版本要求
    /// </summary>
    public string MinGameVersion;

    /// <summary>
    /// 依赖的其他 Mod ID 列表（按顺序加载）
    /// </summary>
    public List<string> Dependencies;

    /// <summary>
    /// 与该 Mod 不兼容的 Mod ID 列表
    /// </summary>
    public List<string> Incompatibles;

    /// <summary>
    /// 脚本配置（区分Server/Client的DLL）
    /// </summary>
    public ModScriptsConfig Scripts;

    public ModManifest()
    {
        Version = "1.0.0";
        Dependencies = new List<string>();
        Incompatibles = new List<string>();
        Scripts = new ModScriptsConfig();
    }

    public bool Validate(out string errorMessage)
    {
        if (string.IsNullOrEmpty(Id)) { errorMessage = "Mod Id 不能为空"; return false; }
        if (string.IsNullOrEmpty(Name)) { errorMessage = "Mod Name 不能为空"; return false; }
        if (string.IsNullOrEmpty(Version)) { errorMessage = "Mod Version 不能为空"; return false; }
        errorMessage = null;
        return true;
    }

    public bool DependsOn(string modId) => Dependencies != null && Dependencies.Contains(modId);
    public bool IsIncompatibleWith(string modId) => Incompatibles != null && Incompatibles.Contains(modId);

    /// <summary>
    /// 检查是否有Server端DLL
    /// </summary>
    public bool HasServerScripts => Scripts?.Server != null && Scripts.Server.Count > 0;

    /// <summary>
    /// 检查是否有Client端DLL
    /// </summary>
    public bool HasClientScripts => Scripts?.Client != null && Scripts.Client.Count > 0;

    /// <summary>
    /// 深拷贝
    /// </summary>
    public ModManifest DeepCopy()
    {
        return new ModManifest
        {
            Id = this.Id,
            Name = this.Name,
            Version = this.Version,
            Author = this.Author,
            Description = this.Description,
            MinGameVersion = this.MinGameVersion,
            Dependencies = this.Dependencies != null ? new List<string>(this.Dependencies) : new List<string>(),
            Incompatibles = this.Incompatibles != null ? new List<string>(this.Incompatibles) : new List<string>(),
            Scripts = this.Scripts?.DeepCopy() ?? new ModScriptsConfig()
        };
    }
}

/// <summary>
/// 模组脚本配置 - 区分Server和Client端DLL
/// </summary>
[System.Serializable]
public class ModScriptsConfig
{
    /// <summary>
    /// Server端DLL列表（在Server项目中加载）
    /// 用于：游戏逻辑、存档操作、配置修改等
    /// </summary>
    public List<string> Server;

    /// <summary>
    /// Client端DLL列表（在Client项目中加载）
    /// 用于：渲染效果、客户端UI、视觉表现等
    /// </summary>
    public List<string> Client;

    public ModScriptsConfig()
    {
        Server = new List<string>();
        Client = new List<string>();
    }

    public ModScriptsConfig DeepCopy()
    {
        return new ModScriptsConfig
        {
            Server = this.Server != null ? new List<string>(this.Server) : new List<string>(),
            Client = this.Client != null ? new List<string>(this.Client) : new List<string>()
        };
    }
}
