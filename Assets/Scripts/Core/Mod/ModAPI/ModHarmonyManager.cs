using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Harmony 补丁管理器实现
/// 负责加载 Harmony 库并提供补丁功能
/// </summary>
public class ModHarmonyManager : IModHarmonyManager
{
    private static ModHarmonyManager _instance;
    public static ModHarmonyManager Instance => _instance ??= new ModHarmonyManager();

    private Assembly _harmonyAssembly;
    private Type _harmonyType;
    private bool _isInitialized;
    private bool _harmonyAvailable;
    private readonly Dictionary<string, object> _harmonyInstances = new();

    public bool IsHarmonyAvailable
    {
        get
        {
            EnsureHarmonyAvailable();
            return _harmonyAvailable;
        }
    }

    /// <summary>
    /// 初始化 Harmony 管理器
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            EnsureHarmonyAvailable();
            return;
        }
        _isInitialized = true;

        try
        {
            EnsureHarmonyAvailable(logWhenUnavailable: true);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ModHarmonyManager] 初始化失败: {e.Message}");
        }
    }

    private void EnsureHarmonyAvailable(bool logWhenUnavailable = false)
    {
        if (_harmonyAvailable && _harmonyAssembly != null && _harmonyType != null)
        {
            return;
        }

        _harmonyAssembly = TryLoadHarmonyAssembly();
        if (_harmonyAssembly == null)
        {
            _harmonyType = null;
            _harmonyAvailable = false;
            if (logWhenUnavailable)
            {
                Debug.Log("[ModHarmonyManager] Harmony 库未找到，补丁功能不可用。模组可以在 Plugins 目录放置 0Harmony.dll 来启用");
            }
            return;
        }

        _harmonyType = _harmonyAssembly.GetType("HarmonyLib.Harmony");
        if (_harmonyType != null)
        {
            if (!_harmonyAvailable)
            {
                Debug.Log("[ModHarmonyManager] Harmony 加载成功");
            }
            _harmonyAvailable = true;
        }
        else
        {
            _harmonyAvailable = false;
            Debug.LogWarning("[ModHarmonyManager] 找不到 HarmonyLib.Harmony 类型");
        }
    }

    private Assembly TryLoadHarmonyAssembly()
    {
        // 检查是否已加载
        var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "0Harmony");
        if (loadedAssembly != null)
        {
            Debug.Log("[ModHarmonyManager] 使用已加载的 Harmony 程序集");
            return loadedAssembly;
        }

        // 从已加载的模组中搜索 0Harmony.dll
        // 玩家可以先加载一个专门的 Harmony 模组来提供此依赖
        if (ModManager.Instance != null && ModManager.Instance.IsInitialized)
        {
            foreach (var mod in ModManager.Instance.GetLoadedMods())
            {
                // 搜索模组的 Plugins 目录
                var paths = new[]
                {
                    Path.Combine(mod.RootPath, "Plugins", "0Harmony.dll"),
                    Path.Combine(mod.RootPath, "Plugins", "Server", "0Harmony.dll")
                };

                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            Debug.Log($"[ModHarmonyManager] 从模组 {mod.ModId} 加载 Harmony: {path}");
                            var bytes = File.ReadAllBytes(path);
                            return Assembly.Load(bytes);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[ModHarmonyManager] 加载失败: {path} - {e.Message}");
                        }
                    }
                }
            }
        }

        return null;
    }

    public IHarmonyInstance CreateHarmony(string harmonyId)
    {
        EnsureHarmonyAvailable();
        if (!_harmonyAvailable)
        {
            Debug.LogError("[ModHarmonyManager] Harmony 不可用");
            return null;
        }

        try
        {
            // 如果已存在，先卸载
            if (_harmonyInstances.ContainsKey(harmonyId))
            {
                UnpatchAll(harmonyId);
            }

            // 创建 Harmony 实例: new Harmony(harmonyId)
            var harmonyInstance = Activator.CreateInstance(_harmonyType, harmonyId);
            _harmonyInstances[harmonyId] = harmonyInstance;

            return new HarmonyInstanceWrapper(harmonyId, harmonyInstance, _harmonyAssembly);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ModHarmonyManager] 创建 Harmony 实例失败: {e.Message}");
            return null;
        }
    }

    public void UnpatchAll(string harmonyId)
    {
        if (!_harmonyAvailable) return;

        try
        {
            if (_harmonyInstances.TryGetValue(harmonyId, out var instance))
            {
                // 调用 UnpatchAll()
                var unpatchMethod = _harmonyType.GetMethod("UnpatchAll", new[] { typeof(string) });
                unpatchMethod?.Invoke(instance, new object[] { harmonyId });

                _harmonyInstances.Remove(harmonyId);
                Debug.Log($"[ModHarmonyManager] 已卸载 Harmony 补丁: {harmonyId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ModHarmonyManager] 卸载补丁失败: {e.Message}");
        }
    }

    /// <summary>
    /// 卸载所有模组的补丁
    /// </summary>
    public void UnpatchAllMods()
    {
        foreach (var harmonyId in _harmonyInstances.Keys.ToList())
        {
            UnpatchAll(harmonyId);
        }
    }
}

/// <summary>
/// Harmony 实例包装器实现
/// </summary>
public class HarmonyInstanceWrapper : IHarmonyInstance
{
    private readonly string _id;
    private readonly object _harmonyInstance;
    private readonly Assembly _harmonyAssembly;
    private readonly Type _harmonyType;
    private readonly Type _harmonyMethodType;
    private readonly Type _harmonyPatchTypeEnum;

    public string Id => _id;

    public HarmonyInstanceWrapper(string id, object harmonyInstance, Assembly harmonyAssembly)
    {
        _id = id;
        _harmonyInstance = harmonyInstance;
        _harmonyAssembly = harmonyAssembly;
        _harmonyType = harmonyAssembly.GetType("HarmonyLib.Harmony");
        _harmonyMethodType = harmonyAssembly.GetType("HarmonyLib.HarmonyMethod");
        _harmonyPatchTypeEnum = harmonyAssembly.GetType("HarmonyLib.HarmonyPatchType");
    }

    public void Prefix(MethodInfo original, MethodInfo prefix)
    {
        try
        {
            var harmonyMethod = CreateHarmonyMethod(prefix);
            var patchMethod = _harmonyType.GetMethod("Patch", new[] { typeof(MethodBase), _harmonyMethodType, _harmonyMethodType, _harmonyMethodType, _harmonyMethodType, _harmonyMethodType });
            patchMethod?.Invoke(_harmonyInstance, new object[] { original, harmonyMethod, null, null, null, null });
            Debug.Log($"[Harmony:{_id}] 添加前缀补丁: {original.DeclaringType?.Name}.{original.Name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Harmony:{_id}] 添加前缀补丁失败: {e.Message}");
        }
    }

    public void Postfix(MethodInfo original, MethodInfo postfix)
    {
        try
        {
            var harmonyMethod = CreateHarmonyMethod(postfix);
            var patchMethod = _harmonyType.GetMethod("Patch", new[] { typeof(MethodBase), _harmonyMethodType, _harmonyMethodType, _harmonyMethodType, _harmonyMethodType, _harmonyMethodType });
            patchMethod?.Invoke(_harmonyInstance, new object[] { original, null, harmonyMethod, null, null, null });
            Debug.Log($"[Harmony:{_id}] 添加后缀补丁: {original.DeclaringType?.Name}.{original.Name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Harmony:{_id}] 添加后缀补丁失败: {e.Message}");
        }
    }

    public void Transpiler(MethodInfo original, MethodInfo transpiler)
    {
        try
        {
            var harmonyMethod = CreateHarmonyMethod(transpiler);
            var patchMethod = _harmonyType.GetMethod("Patch", new[] { typeof(MethodBase), _harmonyMethodType, _harmonyMethodType, _harmonyMethodType, _harmonyMethodType, _harmonyMethodType });
            patchMethod?.Invoke(_harmonyInstance, new object[] { original, null, null, harmonyMethod, null, null });
            Debug.Log($"[Harmony:{_id}] 添加转译器补丁: {original.DeclaringType?.Name}.{original.Name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Harmony:{_id}] 添加转译器补丁失败: {e.Message}");
        }
    }

    public void PatchAll(Type patchType)
    {
        try
        {
            var patchAllMethod = _harmonyType.GetMethod("PatchAll", new[] { typeof(Type) });
            patchAllMethod?.Invoke(_harmonyInstance, new object[] { patchType });
            Debug.Log($"[Harmony:{_id}] 应用补丁类型: {patchType.Name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Harmony:{_id}] 应用补丁类型失败: {e.Message}");
        }
    }

    public void PatchAll(Assembly assembly)
    {
        try
        {
            var patchAllMethod = _harmonyType.GetMethod("PatchAll", new[] { typeof(Assembly) });
            patchAllMethod?.Invoke(_harmonyInstance, new object[] { assembly });
            Debug.Log($"[Harmony:{_id}] 应用程序集补丁: {assembly.GetName().Name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Harmony:{_id}] 应用程序集补丁失败: {e.Message}");
        }
    }

    public void UnpatchAll()
    {
        try
        {
            var unpatchMethod = _harmonyType.GetMethod("UnpatchAll", new[] { typeof(string) });
            unpatchMethod?.Invoke(_harmonyInstance, new object[] { _id });
            Debug.Log($"[Harmony:{_id}] 已移除所有补丁");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Harmony:{_id}] 移除补丁失败: {e.Message}");
        }
    }

    private object CreateHarmonyMethod(MethodInfo method)
    {
        // new HarmonyMethod(method)
        var constructor = _harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
        return constructor?.Invoke(new object[] { method });
    }
}
