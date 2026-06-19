using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 模组钩子系统接口
/// 允许模组拦截和修改游戏行为
/// </summary>
public interface IModHooks
{
    #region 生物钩子

    /// <summary>
    /// 注册生物生成钩子
    /// </summary>
    /// <param name="hook">钩子函数，返回修改后的参数，返回null取消生成</param>
    /// <param name="priority">优先级，数值越小越先执行</param>
    void RegisterCreatureSpawnHook(Func<CreatureSpawnParams, CreatureSpawnParams> hook, int priority = 0);

    /// <summary>
    /// 注册生物伤害钩子
    /// </summary>
    void RegisterCreatureDamageHook(Func<CreatureDamageParams, CreatureDamageParams> hook, int priority = 0);

    /// <summary>
    /// 注册生物移动钩子
    /// </summary>
    void RegisterCreatureMoveHook(Func<CreatureMoveParams, CreatureMoveParams> hook, int priority = 0);

    /// <summary>
    /// 注册生物饥饿计算钩子
    /// </summary>
    void RegisterCreatureHungerHook(Func<CreatureHungerParams, CreatureHungerParams> hook, int priority = 0);

    /// <summary>
    /// 注册生物繁殖钩子
    /// </summary>
    void RegisterCreatureBreedHook(Func<CreatureBreedParams, CreatureBreedParams> hook, int priority = 0);

    #endregion

    #region 植物钩子

    /// <summary>
    /// 注册植物生长钩子
    /// </summary>
    void RegisterPlantGrowthHook(Func<PlantGrowthParams, PlantGrowthParams> hook, int priority = 0);

    /// <summary>
    /// 注册植物产出钩子
    /// </summary>
    void RegisterPlantYieldHook(Func<PlantYieldParams, PlantYieldParams> hook, int priority = 0);

    #endregion

    #region 经济钩子

    /// <summary>
    /// 注册价格计算钩子
    /// </summary>
    void RegisterPriceHook(Func<PriceParams, PriceParams> hook, int priority = 0);

    /// <summary>
    /// 注册收入计算钩子
    /// </summary>
    void RegisterIncomeHook(Func<IncomeParams, IncomeParams> hook, int priority = 0);

    #endregion

    #region 环境钩子

    /// <summary>
    /// 注册水质计算钩子
    /// </summary>
    void RegisterWaterQualityHook(Func<WaterQualityParams, WaterQualityParams> hook, int priority = 0);

    /// <summary>
    /// 注册温度计算钩子
    /// </summary>
    void RegisterTemperatureHook(Func<TemperatureParams, TemperatureParams> hook, int priority = 0);

    /// <summary>
    /// 注册光照计算钩子
    /// </summary>
    void RegisterLightingHook(Func<LightingParams, LightingParams> hook, int priority = 0);

    #endregion

    #region 数据钩子

    /// <summary>
    /// 注册配置数据加载钩子
    /// </summary>
    void RegisterConfigLoadHook(Func<ConfigLoadParams, ConfigLoadParams> hook, int priority = 0);

    /// <summary>
    /// 注册存档数据保存钩子
    /// </summary>
    void RegisterSaveDataHook(Func<SaveDataParams, SaveDataParams> hook, int priority = 0);

    /// <summary>
    /// 注册存档数据加载钩子
    /// </summary>
    void RegisterLoadDataHook(Func<LoadDataParams, LoadDataParams> hook, int priority = 0);

    #endregion

    /// <summary>
    /// 移除模组注册的所有钩子
    /// </summary>
    /// <param name="modId">模组ID</param>
    void UnregisterAllHooks(string modId);
}

#region 钩子参数类

/// <summary>
/// 基础钩子参数
/// </summary>
public abstract class HookParams
{
    /// <summary>
    /// 是否取消操作
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// 注册此钩子的模组ID
    /// </summary>
    public string ModId { get; set; }
}

/// <summary>
/// 生物生成参数
/// </summary>
public class CreatureSpawnParams : HookParams
{
    public int BioTankId { get; set; }
    public int TypeId { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public float Health { get; set; }
    public float Hunger { get; set; }
    public float Size { get; set; }
    public Dictionary<string, object> CustomData { get; set; } = new();
}

/// <summary>
/// 生物伤害参数
/// </summary>
public class CreatureDamageParams : HookParams
{
    public int BioTankId { get; set; }
    public int CreatureId { get; set; }
    public int TypeId { get; set; }
    public float Damage { get; set; }
    public string DamageSource { get; set; }
    public int AttackerId { get; set; }
}

/// <summary>
/// 生物移动参数
/// </summary>
public class CreatureMoveParams : HookParams
{
    public int BioTankId { get; set; }
    public int CreatureId { get; set; }
    public int TypeId { get; set; }
    public Vector3 CurrentPosition { get; set; }
    public Vector3 TargetPosition { get; set; }
    public float Speed { get; set; }
}

/// <summary>
/// 生物饥饿参数
/// </summary>
public class CreatureHungerParams : HookParams
{
    public int BioTankId { get; set; }
    public int CreatureId { get; set; }
    public int TypeId { get; set; }
    public float CurrentHunger { get; set; }
    public float HungerDecay { get; set; }
    public float DeltaTime { get; set; }
}

/// <summary>
/// 生物繁殖参数
/// </summary>
public class CreatureBreedParams : HookParams
{
    public int BioTankId { get; set; }
    public int Parent1Id { get; set; }
    public int Parent2Id { get; set; }
    public int OffspringTypeId { get; set; }
    public int OffspringCount { get; set; }
    public Vector3 SpawnPosition { get; set; }
}

/// <summary>
/// 植物生长参数
/// </summary>
public class PlantGrowthParams : HookParams
{
    public int BioTankId { get; set; }
    public int PlantId { get; set; }
    public int TypeId { get; set; }
    public int CurrentStage { get; set; }
    public float GrowthRate { get; set; }
    public float DeltaTime { get; set; }
}

/// <summary>
/// 植物产出参数
/// </summary>
public class PlantYieldParams : HookParams
{
    public int BioTankId { get; set; }
    public int PlantId { get; set; }
    public int TypeId { get; set; }
    public int YieldItemId { get; set; }
    public int YieldAmount { get; set; }
}

/// <summary>
/// 价格参数
/// </summary>
public class PriceParams : HookParams
{
    public int ItemId { get; set; }
    public string ItemType { get; set; }
    public long BasePrice { get; set; }
    public long FinalPrice { get; set; }
    public float Discount { get; set; }
    public bool IsBuying { get; set; }
}

/// <summary>
/// 收入参数
/// </summary>
public class IncomeParams : HookParams
{
    public int BioTankId { get; set; }
    public string IncomeSource { get; set; }
    public long BaseAmount { get; set; }
    public long FinalAmount { get; set; }
    public float Multiplier { get; set; }
}

/// <summary>
/// 水质参数
/// </summary>
public class WaterQualityParams : HookParams
{
    public int BioTankId { get; set; }
    public float CurrentQuality { get; set; }
    public float QualityChange { get; set; }
    public float Ph { get; set; }
    public float Ammonia { get; set; }
    public float Nitrate { get; set; }
}

/// <summary>
/// 温度参数
/// </summary>
public class TemperatureParams : HookParams
{
    public int BioTankId { get; set; }
    public float CurrentTemperature { get; set; }
    public float TargetTemperature { get; set; }
    public float ChangeRate { get; set; }
}

/// <summary>
/// 光照参数
/// </summary>
public class LightingParams : HookParams
{
    public int BioTankId { get; set; }
    public float Intensity { get; set; }
    public Color LightColor { get; set; }
    public float DayNightCycle { get; set; }
}

/// <summary>
/// 配置加载参数
/// </summary>
public class ConfigLoadParams : HookParams
{
    public string ConfigName { get; set; }
    public string ConfigContent { get; set; }
    public object ParsedConfig { get; set; }
}

/// <summary>
/// 存档保存参数
/// </summary>
public class SaveDataParams : HookParams
{
    public int SlotId { get; set; }
    public string SavePath { get; set; }
    public Dictionary<string, object> SaveData { get; set; } = new();
    public Dictionary<string, string> ModData { get; set; } = new();
}

/// <summary>
/// 存档加载参数
/// </summary>
public class LoadDataParams : HookParams
{
    public int SlotId { get; set; }
    public string LoadPath { get; set; }
    public Dictionary<string, object> LoadedData { get; set; } = new();
    public Dictionary<string, string> ModData { get; set; } = new();
}

#endregion
