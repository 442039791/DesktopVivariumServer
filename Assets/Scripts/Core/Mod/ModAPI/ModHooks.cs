using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 模组钩子系统实现
/// </summary>
public class ModHooks : IModHooks
{
    private static ModHooks _instance;
    public static ModHooks Instance => _instance ??= new ModHooks();

    // 钩子注册表结构
    private class HookEntry<T> where T : HookParams
    {
        public string ModId;
        public int Priority;
        public Func<T, T> Hook;
    }

    // 各类钩子的注册表
    private readonly List<HookEntry<CreatureSpawnParams>> _creatureSpawnHooks = new();
    private readonly List<HookEntry<CreatureDamageParams>> _creatureDamageHooks = new();
    private readonly List<HookEntry<CreatureMoveParams>> _creatureMoveHooks = new();
    private readonly List<HookEntry<CreatureHungerParams>> _creatureHungerHooks = new();
    private readonly List<HookEntry<CreatureBreedParams>> _creatureBreedHooks = new();
    private readonly List<HookEntry<PlantGrowthParams>> _plantGrowthHooks = new();
    private readonly List<HookEntry<PlantYieldParams>> _plantYieldHooks = new();
    private readonly List<HookEntry<PriceParams>> _priceHooks = new();
    private readonly List<HookEntry<IncomeParams>> _incomeHooks = new();
    private readonly List<HookEntry<WaterQualityParams>> _waterQualityHooks = new();
    private readonly List<HookEntry<TemperatureParams>> _temperatureHooks = new();
    private readonly List<HookEntry<LightingParams>> _lightingHooks = new();
    private readonly List<HookEntry<ConfigLoadParams>> _configLoadHooks = new();
    private readonly List<HookEntry<SaveDataParams>> _saveDataHooks = new();
    private readonly List<HookEntry<LoadDataParams>> _loadDataHooks = new();

    // 当前模组ID（在初始化时设置）
    private string _currentModId = "unknown";

    /// <summary>
    /// 设置当前模组ID（用于钩子注册追踪）
    /// </summary>
    public void SetCurrentModId(string modId)
    {
        _currentModId = modId;
    }

    #region 生物钩子注册

    public void RegisterCreatureSpawnHook(Func<CreatureSpawnParams, CreatureSpawnParams> hook, int priority = 0)
    {
        RegisterHook(_creatureSpawnHooks, hook, priority);
    }

    public void RegisterCreatureDamageHook(Func<CreatureDamageParams, CreatureDamageParams> hook, int priority = 0)
    {
        RegisterHook(_creatureDamageHooks, hook, priority);
    }

    public void RegisterCreatureMoveHook(Func<CreatureMoveParams, CreatureMoveParams> hook, int priority = 0)
    {
        RegisterHook(_creatureMoveHooks, hook, priority);
    }

    public void RegisterCreatureHungerHook(Func<CreatureHungerParams, CreatureHungerParams> hook, int priority = 0)
    {
        RegisterHook(_creatureHungerHooks, hook, priority);
    }

    public void RegisterCreatureBreedHook(Func<CreatureBreedParams, CreatureBreedParams> hook, int priority = 0)
    {
        RegisterHook(_creatureBreedHooks, hook, priority);
    }

    #endregion

    #region 植物钩子注册

    public void RegisterPlantGrowthHook(Func<PlantGrowthParams, PlantGrowthParams> hook, int priority = 0)
    {
        RegisterHook(_plantGrowthHooks, hook, priority);
    }

    public void RegisterPlantYieldHook(Func<PlantYieldParams, PlantYieldParams> hook, int priority = 0)
    {
        RegisterHook(_plantYieldHooks, hook, priority);
    }

    #endregion

    #region 经济钩子注册

    public void RegisterPriceHook(Func<PriceParams, PriceParams> hook, int priority = 0)
    {
        RegisterHook(_priceHooks, hook, priority);
    }

    public void RegisterIncomeHook(Func<IncomeParams, IncomeParams> hook, int priority = 0)
    {
        RegisterHook(_incomeHooks, hook, priority);
    }

    #endregion

    #region 环境钩子注册

    public void RegisterWaterQualityHook(Func<WaterQualityParams, WaterQualityParams> hook, int priority = 0)
    {
        RegisterHook(_waterQualityHooks, hook, priority);
    }

    public void RegisterTemperatureHook(Func<TemperatureParams, TemperatureParams> hook, int priority = 0)
    {
        RegisterHook(_temperatureHooks, hook, priority);
    }

    public void RegisterLightingHook(Func<LightingParams, LightingParams> hook, int priority = 0)
    {
        RegisterHook(_lightingHooks, hook, priority);
    }

    #endregion

    #region 数据钩子注册

    public void RegisterConfigLoadHook(Func<ConfigLoadParams, ConfigLoadParams> hook, int priority = 0)
    {
        RegisterHook(_configLoadHooks, hook, priority);
    }

    public void RegisterSaveDataHook(Func<SaveDataParams, SaveDataParams> hook, int priority = 0)
    {
        RegisterHook(_saveDataHooks, hook, priority);
    }

    public void RegisterLoadDataHook(Func<LoadDataParams, LoadDataParams> hook, int priority = 0)
    {
        RegisterHook(_loadDataHooks, hook, priority);
    }

    #endregion

    #region 钩子执行方法（供游戏代码调用）

    /// <summary>
    /// 执行生物生成钩子链
    /// </summary>
    public CreatureSpawnParams ExecuteCreatureSpawnHooks(CreatureSpawnParams param)
    {
        return ExecuteHooks(_creatureSpawnHooks, param);
    }

    /// <summary>
    /// 执行生物伤害钩子链
    /// </summary>
    public CreatureDamageParams ExecuteCreatureDamageHooks(CreatureDamageParams param)
    {
        return ExecuteHooks(_creatureDamageHooks, param);
    }

    /// <summary>
    /// 执行生物移动钩子链
    /// </summary>
    public CreatureMoveParams ExecuteCreatureMoveHooks(CreatureMoveParams param)
    {
        return ExecuteHooks(_creatureMoveHooks, param);
    }

    /// <summary>
    /// 执行生物饥饿钩子链
    /// </summary>
    public CreatureHungerParams ExecuteCreatureHungerHooks(CreatureHungerParams param)
    {
        return ExecuteHooks(_creatureHungerHooks, param);
    }

    /// <summary>
    /// 执行生物繁殖钩子链
    /// </summary>
    public CreatureBreedParams ExecuteCreatureBreedHooks(CreatureBreedParams param)
    {
        return ExecuteHooks(_creatureBreedHooks, param);
    }

    /// <summary>
    /// 执行植物生长钩子链
    /// </summary>
    public PlantGrowthParams ExecutePlantGrowthHooks(PlantGrowthParams param)
    {
        return ExecuteHooks(_plantGrowthHooks, param);
    }

    /// <summary>
    /// 执行植物产出钩子链
    /// </summary>
    public PlantYieldParams ExecutePlantYieldHooks(PlantYieldParams param)
    {
        return ExecuteHooks(_plantYieldHooks, param);
    }

    /// <summary>
    /// 执行价格钩子链
    /// </summary>
    public PriceParams ExecutePriceHooks(PriceParams param)
    {
        return ExecuteHooks(_priceHooks, param);
    }

    /// <summary>
    /// 执行收入钩子链
    /// </summary>
    public IncomeParams ExecuteIncomeHooks(IncomeParams param)
    {
        return ExecuteHooks(_incomeHooks, param);
    }

    /// <summary>
    /// 执行水质钩子链
    /// </summary>
    public WaterQualityParams ExecuteWaterQualityHooks(WaterQualityParams param)
    {
        return ExecuteHooks(_waterQualityHooks, param);
    }

    /// <summary>
    /// 执行温度钩子链
    /// </summary>
    public TemperatureParams ExecuteTemperatureHooks(TemperatureParams param)
    {
        return ExecuteHooks(_temperatureHooks, param);
    }

    /// <summary>
    /// 执行光照钩子链
    /// </summary>
    public LightingParams ExecuteLightingHooks(LightingParams param)
    {
        return ExecuteHooks(_lightingHooks, param);
    }

    /// <summary>
    /// 执行配置加载钩子链
    /// </summary>
    public ConfigLoadParams ExecuteConfigLoadHooks(ConfigLoadParams param)
    {
        return ExecuteHooks(_configLoadHooks, param);
    }

    /// <summary>
    /// 执行存档保存钩子链
    /// </summary>
    public SaveDataParams ExecuteSaveDataHooks(SaveDataParams param)
    {
        return ExecuteHooks(_saveDataHooks, param);
    }

    /// <summary>
    /// 执行存档加载钩子链
    /// </summary>
    public LoadDataParams ExecuteLoadDataHooks(LoadDataParams param)
    {
        return ExecuteHooks(_loadDataHooks, param);
    }

    #endregion

    #region 钩子管理

    public void UnregisterAllHooks(string modId)
    {
        RemoveHooksForMod(_creatureSpawnHooks, modId);
        RemoveHooksForMod(_creatureDamageHooks, modId);
        RemoveHooksForMod(_creatureMoveHooks, modId);
        RemoveHooksForMod(_creatureHungerHooks, modId);
        RemoveHooksForMod(_creatureBreedHooks, modId);
        RemoveHooksForMod(_plantGrowthHooks, modId);
        RemoveHooksForMod(_plantYieldHooks, modId);
        RemoveHooksForMod(_priceHooks, modId);
        RemoveHooksForMod(_incomeHooks, modId);
        RemoveHooksForMod(_waterQualityHooks, modId);
        RemoveHooksForMod(_temperatureHooks, modId);
        RemoveHooksForMod(_lightingHooks, modId);
        RemoveHooksForMod(_configLoadHooks, modId);
        RemoveHooksForMod(_saveDataHooks, modId);
        RemoveHooksForMod(_loadDataHooks, modId);

        Debug.Log($"[ModHooks] 已移除模组 {modId} 的所有钩子");
    }

    /// <summary>
    /// 清除所有钩子
    /// </summary>
    public void ClearAllHooks()
    {
        _creatureSpawnHooks.Clear();
        _creatureDamageHooks.Clear();
        _creatureMoveHooks.Clear();
        _creatureHungerHooks.Clear();
        _creatureBreedHooks.Clear();
        _plantGrowthHooks.Clear();
        _plantYieldHooks.Clear();
        _priceHooks.Clear();
        _incomeHooks.Clear();
        _waterQualityHooks.Clear();
        _temperatureHooks.Clear();
        _lightingHooks.Clear();
        _configLoadHooks.Clear();
        _saveDataHooks.Clear();
        _loadDataHooks.Clear();

        Debug.Log("[ModHooks] 已清除所有钩子");
    }

    #endregion

    #region 工具方法

    private void RegisterHook<T>(List<HookEntry<T>> hooks, Func<T, T> hook, int priority) where T : HookParams
    {
        hooks.Add(new HookEntry<T>
        {
            ModId = _currentModId,
            Priority = priority,
            Hook = hook
        });

        // 按优先级排序
        hooks.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        Debug.Log($"[ModHooks] 模组 {_currentModId} 注册了 {typeof(T).Name} 钩子 (优先级: {priority})");
    }

    private T ExecuteHooks<T>(List<HookEntry<T>> hooks, T param) where T : HookParams
    {
        if (hooks.Count == 0) return param;

        var result = param;
        foreach (var entry in hooks)
        {
            try
            {
                result.ModId = entry.ModId;
                result = entry.Hook(result);

                if (result == null || result.Cancel)
                {
                    Debug.Log($"[ModHooks] 钩子被模组 {entry.ModId} 取消");
                    return result;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ModHooks] 模组 {entry.ModId} 的钩子执行异常: {e.Message}\n{e.StackTrace}");
            }
        }

        return result;
    }

    private void RemoveHooksForMod<T>(List<HookEntry<T>> hooks, string modId) where T : HookParams
    {
        hooks.RemoveAll(h => h.ModId == modId);
    }

    #endregion
}
