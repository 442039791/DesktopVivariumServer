using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Common;

/// <summary>
/// 模组脚本加载器 - 负责加载和管理DLL模组及内置模组
/// </summary>
public class ModScriptLoader : MonoSingleton<ModScriptLoader>
{
    /// <summary>
    /// 已加载的模组入口
    /// </summary>
    private readonly Dictionary<string, IModEntry> _loadedMods = new();

    /// <summary>
    /// 模组上下文缓存
    /// </summary>
    private readonly Dictionary<string, ModContext> _modContexts = new();

    /// <summary>
    /// 速度倍率
    /// </summary>
    private float _speedMultiplier = 1.0f;

    /// <summary>
    /// 上次更新时间
    /// </summary>
    private float _lastUpdateTime;

    /// <summary>
    /// 累积的更新时间
    /// </summary>
    private float _accumulatedTime;

    /// <summary>
    /// 基础更新间隔（1秒）
    /// </summary>
    private const float BaseUpdateInterval = 1.0f;

    /// <summary>
    /// 是否已加载内置模组
    /// </summary>
    private bool _builtInModsLoaded = false;

    public event Action<string> OnModLoaded;
    public event Action<string> OnModUnloaded;

    public override void Init()
    {
        _lastUpdateTime = Time.time;

        // 初始化 Harmony 管理器
        ModHarmonyManager.Instance.Initialize();

        // 初始化模组 UI 管理器
        ModUIManager.Instance.Init();
    }

    /// <summary>
    /// 加载模组的DLL脚本（Server端）
    /// </summary>
    /// <param name="modInfo">模组信息</param>
    public void LoadModScripts(ModInfo modInfo)
    {
        // 优先使用manifest中指定的DLL列表
        if (modInfo.Manifest?.HasServerScripts == true)
        {
            LoadScriptsFromManifest(modInfo);
            return;
        }

        // 兼容旧结构：直接扫描Plugins目录或Plugins/Server目录
        LoadScriptsFromDirectory(modInfo);
    }

    /// <summary>
    /// 从manifest.json中指定的DLL列表加载
    /// </summary>
    private void LoadScriptsFromManifest(ModInfo modInfo)
    {
        var serverScripts = modInfo.Manifest.Scripts.Server;
        foreach (var dllName in serverScripts)
        {
            // 优先从 Plugins/Server 目录加载
            var serverPath = Path.Combine(modInfo.RootPath, "Plugins", "Server", dllName);
            if (File.Exists(serverPath))
            {
                LoadDll(modInfo, serverPath);
                continue;
            }

            // 兼容旧结构：从 Plugins 根目录加载
            var rootPath = Path.Combine(modInfo.RootPath, "Plugins", dllName);
            if (File.Exists(rootPath))
            {
                LoadDll(modInfo, rootPath);
                continue;
            }

            Debug.LogWarning($"[ModScriptLoader] Server端DLL未找到: {dllName}");
        }
    }

    /// <summary>
    /// 从目录扫描DLL（兼容旧结构）
    /// </summary>
    private void LoadScriptsFromDirectory(ModInfo modInfo)
    {
        // 优先检查 Plugins/Server 目录
        var serverPluginsPath = Path.Combine(modInfo.RootPath, "Plugins", "Server");
        if (Directory.Exists(serverPluginsPath))
        {
            var dllFiles = Directory.GetFiles(serverPluginsPath, "*.dll");
            foreach (var dllPath in dllFiles)
            {
                LoadDll(modInfo, dllPath);
            }
            return;
        }

        // 兼容旧结构：检查 Plugins 根目录
        var pluginsPath = Path.Combine(modInfo.RootPath, "Plugins");
        if (!Directory.Exists(pluginsPath))
        {
            return;
        }

        var rootDllFiles = Directory.GetFiles(pluginsPath, "*.dll");
        if (rootDllFiles.Length > 0)
        {
            Debug.Log($"[ModScriptLoader] 使用旧目录结构加载 {modInfo.ModId} 的DLL");
            foreach (var dllPath in rootDllFiles)
            {
                LoadDll(modInfo, dllPath);
            }
        }
    }

    /// <summary>
    /// 加载单个DLL
    /// </summary>
    private void LoadDll(ModInfo modInfo, string dllPath)
    {
        try
        {
            Debug.Log($"[ModScriptLoader] 正在加载DLL: {dllPath}");

            // 读取DLL文件
            byte[] dllBytes = File.ReadAllBytes(dllPath);

            // 检查是否有PDB文件（用于调试）
            string pdbPath = Path.ChangeExtension(dllPath, ".pdb");
            byte[] pdbBytes = null;
            if (File.Exists(pdbPath))
            {
                pdbBytes = File.ReadAllBytes(pdbPath);
            }

            // 加载程序集
            Assembly assembly = pdbBytes != null
                ? Assembly.Load(dllBytes, pdbBytes)
                : Assembly.Load(dllBytes);

            // 查找实现IModEntry接口的类型
            var modEntryTypes = assembly.GetTypes()
                .Where(t => typeof(IModEntry).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            if (modEntryTypes.Count == 0)
            {
                Debug.LogWarning($"[ModScriptLoader] DLL中未找到IModEntry实现: {dllPath}");
                return;
            }

            foreach (var entryType in modEntryTypes)
            {
                try
                {
                    // 创建模组入口实例
                    var modEntry = (IModEntry)Activator.CreateInstance(entryType);

                    // 创建模组上下文
                    var context = new ModContext(modInfo);
                    _modContexts[modEntry.ModId] = context;

                    // 初始化模组
                    modEntry.Initialize(context);
                    modEntry.OnEnable();

                    // 保存模组引用
                    _loadedMods[modEntry.ModId] = modEntry;

                    Debug.Log($"[ModScriptLoader] 成功加载模组脚本: {modEntry.ModName} v{modEntry.Version}");
                    OnModLoaded?.Invoke(modEntry.ModId);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ModScriptLoader] 初始化模组入口失败 {entryType.Name}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ModScriptLoader] 加载DLL失败 {dllPath}: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 卸载模组脚本
    /// </summary>
    /// <param name="modId">模组ID</param>
    public void UnloadModScripts(string modId)
    {
        if (_loadedMods.TryGetValue(modId, out var modEntry))
        {
            try
            {
                modEntry.OnDisable();
                modEntry.OnUnload();
                Debug.Log($"[ModScriptLoader] 卸载模组脚本: {modEntry.ModName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ModScriptLoader] 卸载模组失败 {modId}: {e.Message}");
            }

            // 卸载模组注册的钩子
            ModHooks.Instance.UnregisterAllHooks(modId);

            // 卸载模组的 Harmony 补丁
            ModHarmonyManager.Instance.UnpatchAll(modId);

            // 卸载模组的 UI
            if (ModUIManager.Instance != null)
            {
                ModUIManager.Instance.UnregisterAllUI(modId);
            }

            _loadedMods.Remove(modId);
            _modContexts.Remove(modId);
            OnModUnloaded?.Invoke(modId);
        }
    }

    /// <summary>
    /// 卸载所有模组脚本
    /// </summary>
    public void UnloadAllModScripts()
    {
        foreach (var modId in _loadedMods.Keys.ToList())
        {
            UnloadModScripts(modId);
        }

        // 清理所有钩子和事件订阅
        ModHooks.Instance.ClearAllHooks();
        ModGameEvents.Instance.ClearAllSubscriptions();
        ModHarmonyManager.Instance.UnpatchAllMods();

        // 清理所有模组 UI
        if (ModUIManager.Instance != null)
        {
            ModUIManager.Instance.ClearAll();
        }
    }

    /// <summary>
    /// 设置速度倍率
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Mathf.Max(0.1f, multiplier);
        Debug.Log($"[ModScriptLoader] 速度倍率设置为: {_speedMultiplier}x");
    }

    /// <summary>
    /// 获取速度倍率
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return _speedMultiplier;
    }

    /// <summary>
    /// 每帧更新
    /// </summary>
    private void Update()
    {
        // 计算实际经过的时间
        float currentTime = Time.time;
        float deltaTime = currentTime - _lastUpdateTime;
        _lastUpdateTime = currentTime;

        // 累积时间（应用速度倍率）
        _accumulatedTime += deltaTime * _speedMultiplier;

        // 当累积时间超过基础间隔时，触发额外的更新
        while (_accumulatedTime >= BaseUpdateInterval)
        {
            _accumulatedTime -= BaseUpdateInterval;

            // 如果速度倍率大于1，触发额外的生态箱更新
            if (_speedMultiplier > 1.0f)
            {
                TriggerExtraBioTankUpdate();
            }
        }

        // 更新所有模组的OnUpdate
        foreach (var modEntry in _loadedMods.Values)
        {
            try
            {
                modEntry.OnUpdate();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ModScriptLoader] 模组Update异常 {modEntry.ModId}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 触发额外的生态箱更新
    /// </summary>
    private void TriggerExtraBioTankUpdate()
    {
        try
        {
            var bioTanks = BioTankRegistry.Instance.GetAllBioTanks();
            foreach (var bioTank in bioTanks)
            {
                // 调用StateUpdate，传入1分钟的间隔
                bioTank.StateUpdate(1);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ModScriptLoader] 触发额外生态箱更新失败: {e.Message}");
        }
    }

    /// <summary>
    /// 获取已加载的模组列表
    /// </summary>
    public IReadOnlyDictionary<string, IModEntry> GetLoadedMods()
    {
        return _loadedMods;
    }

    /// <summary>
    /// 检查模组是否已加载
    /// </summary>
    public bool IsModLoaded(string modId)
    {
        return _loadedMods.ContainsKey(modId);
    }
}
