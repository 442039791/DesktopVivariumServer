using System;
using UnityEngine;

/// <summary>
/// 游戏事件接口 - 允许模组订阅游戏核心事件
/// </summary>
public interface IModGameEvents
{
    #region 生态箱事件

    /// <summary>
    /// 生态箱创建时触发
    /// </summary>
    event Action<BioTankEventArgs> OnBioTankCreated;

    /// <summary>
    /// 生态箱销毁时触发
    /// </summary>
    event Action<BioTankEventArgs> OnBioTankDestroyed;

    /// <summary>
    /// 生态箱激活时触发（切换到此生态箱）
    /// </summary>
    event Action<BioTankEventArgs> OnBioTankActivated;

    /// <summary>
    /// 生态箱状态更新时触发（每分钟）
    /// </summary>
    event Action<BioTankUpdateEventArgs> OnBioTankStateUpdate;

    #endregion

    #region 生物事件

    /// <summary>
    /// 生物生成时触发
    /// </summary>
    event Action<CreatureEventArgs> OnCreatureSpawned;

    /// <summary>
    /// 生物死亡时触发
    /// </summary>
    event Action<CreatureEventArgs> OnCreatureDied;

    /// <summary>
    /// 生物繁殖时触发
    /// </summary>
    event Action<CreatureBreedEventArgs> OnCreatureBreed;

    /// <summary>
    /// 生物状态变化时触发（饥饿、健康等）
    /// </summary>
    event Action<CreatureStateChangedEventArgs> OnCreatureStateChanged;

    #endregion

    #region 植物事件

    /// <summary>
    /// 植物种植时触发
    /// </summary>
    event Action<PlantEventArgs> OnPlantPlaced;

    /// <summary>
    /// 植物移除时触发
    /// </summary>
    event Action<PlantEventArgs> OnPlantRemoved;

    /// <summary>
    /// 植物生长时触发
    /// </summary>
    event Action<PlantGrowthEventArgs> OnPlantGrowth;

    #endregion

    #region 物品/装饰事件

    /// <summary>
    /// 装饰物放置时触发
    /// </summary>
    event Action<DecorationEventArgs> OnDecorationPlaced;

    /// <summary>
    /// 装饰物移除时触发
    /// </summary>
    event Action<DecorationEventArgs> OnDecorationRemoved;

    #endregion

    #region 经济事件

    /// <summary>
    /// 金币变化时触发
    /// </summary>
    event Action<CurrencyEventArgs> OnCurrencyChanged;

    /// <summary>
    /// 购买物品时触发
    /// </summary>
    event Action<PurchaseEventArgs> OnItemPurchased;

    /// <summary>
    /// 出售物品时触发
    /// </summary>
    event Action<SellEventArgs> OnItemSold;

    #endregion

    #region 游戏流程事件

    /// <summary>
    /// 游戏保存时触发
    /// </summary>
    event Action<SaveEventArgs> OnGameSaving;

    /// <summary>
    /// 游戏保存完成时触发
    /// </summary>
    event Action<SaveEventArgs> OnGameSaved;

    /// <summary>
    /// 游戏加载时触发
    /// </summary>
    event Action<LoadEventArgs> OnGameLoading;

    /// <summary>
    /// 游戏加载完成时触发
    /// </summary>
    event Action<LoadEventArgs> OnGameLoaded;

    /// <summary>
    /// 时间流逝时触发（游戏内时间）
    /// </summary>
    event Action<TimeEventArgs> OnTimeTick;

    #endregion
}

#region 事件参数类

/// <summary>
/// 基础事件参数
/// </summary>
public abstract class ModEventArgs : EventArgs
{
    /// <summary>
    /// 事件发生时间
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.Now;

    /// <summary>
    /// 是否取消事件（某些事件支持取消）
    /// </summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// 生态箱事件参数
/// </summary>
public class BioTankEventArgs : ModEventArgs
{
    public int BioTankId { get; set; }
    public BioTankManager BioTank { get; set; }
}

/// <summary>
/// 生态箱更新事件参数
/// </summary>
public class BioTankUpdateEventArgs : BioTankEventArgs
{
    public int DeltaMinutes { get; set; }
}

/// <summary>
/// 生物事件参数
/// </summary>
public class CreatureEventArgs : ModEventArgs
{
    public int BioTankId { get; set; }
    public int CreatureId { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; }
    public Vector3 Position { get; set; }
    public object CreatureInstance { get; set; }
}

/// <summary>
/// 生物繁殖事件参数
/// </summary>
public class CreatureBreedEventArgs : CreatureEventArgs
{
    public int ParentId1 { get; set; }
    public int ParentId2 { get; set; }
    public int OffspringId { get; set; }
}

/// <summary>
/// 生物状态变化事件参数
/// </summary>
public class CreatureStateChangedEventArgs : CreatureEventArgs
{
    public string StateName { get; set; }
    public float OldValue { get; set; }
    public float NewValue { get; set; }
}

/// <summary>
/// 植物事件参数
/// </summary>
public class PlantEventArgs : ModEventArgs
{
    public int BioTankId { get; set; }
    public int PlantId { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; }
    public Vector3 Position { get; set; }
    public object PlantInstance { get; set; }
}

/// <summary>
/// 植物生长事件参数
/// </summary>
public class PlantGrowthEventArgs : PlantEventArgs
{
    public int OldGrowthStage { get; set; }
    public int NewGrowthStage { get; set; }
}

/// <summary>
/// 装饰物事件参数
/// </summary>
public class DecorationEventArgs : ModEventArgs
{
    public int BioTankId { get; set; }
    public int DecorationId { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public object DecorationInstance { get; set; }
}

/// <summary>
/// 货币事件参数
/// </summary>
public class CurrencyEventArgs : ModEventArgs
{
    public string CurrencyType { get; set; }
    public long OldAmount { get; set; }
    public long NewAmount { get; set; }
    public long Delta { get; set; }
    public string Reason { get; set; }
}

/// <summary>
/// 购买事件参数
/// </summary>
public class PurchaseEventArgs : ModEventArgs
{
    public int ItemId { get; set; }
    public string ItemType { get; set; }
    public int Quantity { get; set; }
    public long Price { get; set; }
    public string CurrencyType { get; set; }
}

/// <summary>
/// 出售事件参数
/// </summary>
public class SellEventArgs : ModEventArgs
{
    public int ItemId { get; set; }
    public string ItemType { get; set; }
    public int Quantity { get; set; }
    public long Price { get; set; }
    public string CurrencyType { get; set; }
}

/// <summary>
/// 保存事件参数
/// </summary>
public class SaveEventArgs : ModEventArgs
{
    public string SavePath { get; set; }
    public int SlotId { get; set; }
}

/// <summary>
/// 加载事件参数
/// </summary>
public class LoadEventArgs : ModEventArgs
{
    public string LoadPath { get; set; }
    public int SlotId { get; set; }
}

/// <summary>
/// 时间事件参数
/// </summary>
public class TimeEventArgs : ModEventArgs
{
    public int GameMinutes { get; set; }
    public int GameHours { get; set; }
    public int GameDays { get; set; }
    public float RealDeltaTime { get; set; }
}

#endregion
