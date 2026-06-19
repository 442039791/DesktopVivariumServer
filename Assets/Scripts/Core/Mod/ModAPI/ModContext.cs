using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 模组上下文实现
/// </summary>
public class ModContext : IModContext
{
    private readonly ModInfo _modInfo;
    private readonly ModLogger _logger;
    private readonly ModAssetLoaderAdapter _assetLoader;
    private readonly ModConfigLoaderAdapter _configLoader;
    private readonly ModEventBus _eventBus;
    private readonly ModGameServices _gameServices;
    private readonly ModHooksWrapper _hooksWrapper;
    private readonly ModUIServices _uiServices;

    public string ModPath => _modInfo.RootPath;
    public string ModId => _modInfo.ModId;
    public IModLogger Logger => _logger;
    public IModAssetLoader AssetLoader => _assetLoader;
    public IModConfigLoader ConfigLoader => _configLoader;
    public IModEventBus EventBus => _eventBus;
    public IModGameServices GameServices => _gameServices;
    public IModGameEvents GameEvents => ModGameEvents.Instance;
    public IModHooks Hooks => _hooksWrapper;
    public IModHarmonyManager HarmonyManager => ModHarmonyManager.Instance;
    public IModUIServices UIServices => _uiServices;

    public ModContext(ModInfo modInfo)
    {
        _modInfo = modInfo;
        _logger = new ModLogger(modInfo.ModId);
        _assetLoader = new ModAssetLoaderAdapter(modInfo);
        _configLoader = new ModConfigLoaderAdapter(modInfo);
        _eventBus = new ModEventBus();
        _gameServices = new ModGameServices();
        _hooksWrapper = new ModHooksWrapper(modInfo.ModId);
        _uiServices = new ModUIServices(modInfo.ModId, modInfo);
    }
}

/// <summary>
/// 钩子包装器 - 自动设置模组ID
/// </summary>
public class ModHooksWrapper : IModHooks
{
    private readonly string _modId;

    public ModHooksWrapper(string modId)
    {
        _modId = modId;
    }

    private void SetModId()
    {
        ModHooks.Instance.SetCurrentModId(_modId);
    }

    public void RegisterCreatureSpawnHook(Func<CreatureSpawnParams, CreatureSpawnParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterCreatureSpawnHook(hook, priority);
    }

    public void RegisterCreatureDamageHook(Func<CreatureDamageParams, CreatureDamageParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterCreatureDamageHook(hook, priority);
    }

    public void RegisterCreatureMoveHook(Func<CreatureMoveParams, CreatureMoveParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterCreatureMoveHook(hook, priority);
    }

    public void RegisterCreatureHungerHook(Func<CreatureHungerParams, CreatureHungerParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterCreatureHungerHook(hook, priority);
    }

    public void RegisterCreatureBreedHook(Func<CreatureBreedParams, CreatureBreedParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterCreatureBreedHook(hook, priority);
    }

    public void RegisterPlantGrowthHook(Func<PlantGrowthParams, PlantGrowthParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterPlantGrowthHook(hook, priority);
    }

    public void RegisterPlantYieldHook(Func<PlantYieldParams, PlantYieldParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterPlantYieldHook(hook, priority);
    }

    public void RegisterPriceHook(Func<PriceParams, PriceParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterPriceHook(hook, priority);
    }

    public void RegisterIncomeHook(Func<IncomeParams, IncomeParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterIncomeHook(hook, priority);
    }

    public void RegisterWaterQualityHook(Func<WaterQualityParams, WaterQualityParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterWaterQualityHook(hook, priority);
    }

    public void RegisterTemperatureHook(Func<TemperatureParams, TemperatureParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterTemperatureHook(hook, priority);
    }

    public void RegisterLightingHook(Func<LightingParams, LightingParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterLightingHook(hook, priority);
    }

    public void RegisterConfigLoadHook(Func<ConfigLoadParams, ConfigLoadParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterConfigLoadHook(hook, priority);
    }

    public void RegisterSaveDataHook(Func<SaveDataParams, SaveDataParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterSaveDataHook(hook, priority);
    }

    public void RegisterLoadDataHook(Func<LoadDataParams, LoadDataParams> hook, int priority = 0)
    {
        SetModId();
        ModHooks.Instance.RegisterLoadDataHook(hook, priority);
    }

    public void UnregisterAllHooks(string modId)
    {
        ModHooks.Instance.UnregisterAllHooks(modId);
    }
}

/// <summary>
/// 模组日志记录器实现
/// </summary>
public class ModLogger : IModLogger
{
    private readonly string _modId;

    public ModLogger(string modId)
    {
        _modId = modId;
    }

    public void Log(string message)
    {
        Debug.Log($"[Mod:{_modId}] {message}");
    }

    public void LogWarning(string message)
    {
        Debug.LogWarning($"[Mod:{_modId}] {message}");
    }

    public void LogError(string message)
    {
        Debug.LogError($"[Mod:{_modId}] {message}");
    }
}

/// <summary>
/// 模组资源加载器适配器
/// </summary>
public class ModAssetLoaderAdapter : IModAssetLoader
{
    private readonly ModInfo _modInfo;

    public ModAssetLoaderAdapter(ModInfo modInfo)
    {
        _modInfo = modInfo;
    }

    public Texture2D LoadTexture(string relativePath)
    {
        return ModManager.Instance.GetTexture(relativePath);
    }

    public Mesh LoadMesh(string relativePath)
    {
        return ModManager.Instance.GetMesh(relativePath);
    }

    public Material LoadMaterial(string relativePath)
    {
        return ModManager.Instance.GetMaterial(relativePath);
    }

    public string LoadText(string relativePath)
    {
        var fullPath = System.IO.Path.Combine(_modInfo.RootPath, relativePath);
        if (System.IO.File.Exists(fullPath))
        {
            return System.IO.File.ReadAllText(fullPath);
        }
        return null;
    }
}

/// <summary>
/// 模组配置加载器适配器
/// </summary>
public class ModConfigLoaderAdapter : IModConfigLoader
{
    private readonly ModInfo _modInfo;

    public ModConfigLoaderAdapter(ModInfo modInfo)
    {
        _modInfo = modInfo;
    }

    public string GetConfigText(string configName)
    {
        return ModManager.Instance.GetConfigText($"ConfigData/{configName}");
    }

    public T GetConfig<T>(string configName) where T : class
    {
        var text = GetConfigText(configName);
        if (string.IsNullOrEmpty(text))
            return null;
        try
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(text);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// 模组事件总线实现
/// </summary>
public class ModEventBus : IModEventBus
{
    private readonly Dictionary<string, List<Action<object>>> _handlers = new();

    public void Subscribe(string eventName, Action<object> handler)
    {
        if (!_handlers.ContainsKey(eventName))
        {
            _handlers[eventName] = new List<Action<object>>();
        }
        _handlers[eventName].Add(handler);
    }

    public void Unsubscribe(string eventName, Action<object> handler)
    {
        if (_handlers.TryGetValue(eventName, out var handlers))
        {
            handlers.Remove(handler);
        }
    }

    public void Publish(string eventName, object data)
    {
        if (_handlers.TryGetValue(eventName, out var handlers))
        {
            foreach (var handler in handlers.ToArray())
            {
                try
                {
                    handler?.Invoke(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ModEventBus] Error handling event {eventName}: {e.Message}");
                }
            }
        }
    }
}

/// <summary>
/// 游戏服务实现
/// </summary>
public class ModGameServices : IModGameServices
{
    private float _speedMultiplier = 1.0f;

    public IReadOnlyList<BioTankManager> GetAllBioTanks()
    {
        return BioTankRegistry.Instance.GetAllBioTanks();
    }

    public BioTankManager GetActiveBioTank()
    {
        var activeBioTankId = PlayerManager.ActiveBioTanksID;
        return BioTankRegistry.Instance.GetBioTank(activeBioTankId);
    }

    public void SetBioTankSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Mathf.Max(0.1f, multiplier);
        ModScriptLoader.Instance?.SetSpeedMultiplier(_speedMultiplier);
        Debug.Log($"[ModGameServices] 生态箱速度倍率设置为: {_speedMultiplier}x");
    }

    public float GetBioTankSpeedMultiplier()
    {
        return _speedMultiplier;
    }
}
