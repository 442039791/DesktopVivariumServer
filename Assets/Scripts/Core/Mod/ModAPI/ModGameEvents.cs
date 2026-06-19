using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏事件管理器实现
/// 连接游戏核心事件和模组订阅
/// </summary>
public class ModGameEvents : IModGameEvents
{
    private static ModGameEvents _instance;
    public static ModGameEvents Instance => _instance ??= new ModGameEvents();

    #region 生态箱事件

    public event Action<BioTankEventArgs> OnBioTankCreated;
    public event Action<BioTankEventArgs> OnBioTankDestroyed;
    public event Action<BioTankEventArgs> OnBioTankActivated;
    public event Action<BioTankUpdateEventArgs> OnBioTankStateUpdate;

    #endregion

    #region 生物事件

    public event Action<CreatureEventArgs> OnCreatureSpawned;
    public event Action<CreatureEventArgs> OnCreatureDied;
    public event Action<CreatureBreedEventArgs> OnCreatureBreed;
    public event Action<CreatureStateChangedEventArgs> OnCreatureStateChanged;

    #endregion

    #region 植物事件

    public event Action<PlantEventArgs> OnPlantPlaced;
    public event Action<PlantEventArgs> OnPlantRemoved;
    public event Action<PlantGrowthEventArgs> OnPlantGrowth;

    #endregion

    #region 装饰事件

    public event Action<DecorationEventArgs> OnDecorationPlaced;
    public event Action<DecorationEventArgs> OnDecorationRemoved;

    #endregion

    #region 经济事件

    public event Action<CurrencyEventArgs> OnCurrencyChanged;
    public event Action<PurchaseEventArgs> OnItemPurchased;
    public event Action<SellEventArgs> OnItemSold;

    #endregion

    #region 游戏流程事件

    public event Action<SaveEventArgs> OnGameSaving;
    public event Action<SaveEventArgs> OnGameSaved;
    public event Action<LoadEventArgs> OnGameLoading;
    public event Action<LoadEventArgs> OnGameLoaded;
    public event Action<TimeEventArgs> OnTimeTick;

    #endregion

    #region 内部触发方法（供游戏代码调用）

    /// <summary>
    /// 触发生态箱创建事件
    /// </summary>
    public void RaiseBioTankCreated(int bioTankId, BioTankManager bioTank)
    {
        SafeInvoke(OnBioTankCreated, new BioTankEventArgs
        {
            BioTankId = bioTankId,
            BioTank = bioTank
        });
    }

    /// <summary>
    /// 触发生态箱销毁事件
    /// </summary>
    public void RaiseBioTankDestroyed(int bioTankId, BioTankManager bioTank)
    {
        SafeInvoke(OnBioTankDestroyed, new BioTankEventArgs
        {
            BioTankId = bioTankId,
            BioTank = bioTank
        });
    }

    /// <summary>
    /// 触发生态箱激活事件
    /// </summary>
    public void RaiseBioTankActivated(int bioTankId, BioTankManager bioTank)
    {
        SafeInvoke(OnBioTankActivated, new BioTankEventArgs
        {
            BioTankId = bioTankId,
            BioTank = bioTank
        });
    }

    /// <summary>
    /// 触发生态箱状态更新事件
    /// </summary>
    public void RaiseBioTankStateUpdate(int bioTankId, BioTankManager bioTank, int deltaMinutes)
    {
        SafeInvoke(OnBioTankStateUpdate, new BioTankUpdateEventArgs
        {
            BioTankId = bioTankId,
            BioTank = bioTank,
            DeltaMinutes = deltaMinutes
        });
    }

    /// <summary>
    /// 触发生物生成事件
    /// </summary>
    public bool RaiseCreatureSpawned(int bioTankId, int creatureId, int typeId, string typeName, Vector3 position, object instance)
    {
        var args = new CreatureEventArgs
        {
            BioTankId = bioTankId,
            CreatureId = creatureId,
            TypeId = typeId,
            TypeName = typeName,
            Position = position,
            CreatureInstance = instance
        };
        SafeInvoke(OnCreatureSpawned, args);
        return !args.Cancel;
    }

    /// <summary>
    /// 触发生物死亡事件
    /// </summary>
    public void RaiseCreatureDied(int bioTankId, int creatureId, int typeId, string typeName, Vector3 position, object instance)
    {
        SafeInvoke(OnCreatureDied, new CreatureEventArgs
        {
            BioTankId = bioTankId,
            CreatureId = creatureId,
            TypeId = typeId,
            TypeName = typeName,
            Position = position,
            CreatureInstance = instance
        });
    }

    /// <summary>
    /// 触发生物繁殖事件
    /// </summary>
    public bool RaiseCreatureBreed(int bioTankId, int parentId1, int parentId2, int offspringId, int typeId, string typeName)
    {
        var args = new CreatureBreedEventArgs
        {
            BioTankId = bioTankId,
            ParentId1 = parentId1,
            ParentId2 = parentId2,
            OffspringId = offspringId,
            TypeId = typeId,
            TypeName = typeName
        };
        SafeInvoke(OnCreatureBreed, args);
        return !args.Cancel;
    }

    /// <summary>
    /// 触发生物状态变化事件
    /// </summary>
    public void RaiseCreatureStateChanged(int bioTankId, int creatureId, int typeId, string stateName, float oldValue, float newValue)
    {
        SafeInvoke(OnCreatureStateChanged, new CreatureStateChangedEventArgs
        {
            BioTankId = bioTankId,
            CreatureId = creatureId,
            TypeId = typeId,
            StateName = stateName,
            OldValue = oldValue,
            NewValue = newValue
        });
    }

    /// <summary>
    /// 触发植物种植事件
    /// </summary>
    public bool RaisePlantPlaced(int bioTankId, int plantId, int typeId, string typeName, Vector3 position, object instance)
    {
        var args = new PlantEventArgs
        {
            BioTankId = bioTankId,
            PlantId = plantId,
            TypeId = typeId,
            TypeName = typeName,
            Position = position,
            PlantInstance = instance
        };
        SafeInvoke(OnPlantPlaced, args);
        return !args.Cancel;
    }

    /// <summary>
    /// 触发植物移除事件
    /// </summary>
    public void RaisePlantRemoved(int bioTankId, int plantId, int typeId, string typeName, Vector3 position, object instance)
    {
        SafeInvoke(OnPlantRemoved, new PlantEventArgs
        {
            BioTankId = bioTankId,
            PlantId = plantId,
            TypeId = typeId,
            TypeName = typeName,
            Position = position,
            PlantInstance = instance
        });
    }

    /// <summary>
    /// 触发植物生长事件
    /// </summary>
    public void RaisePlantGrowth(int bioTankId, int plantId, int typeId, string typeName, int oldStage, int newStage)
    {
        SafeInvoke(OnPlantGrowth, new PlantGrowthEventArgs
        {
            BioTankId = bioTankId,
            PlantId = plantId,
            TypeId = typeId,
            TypeName = typeName,
            OldGrowthStage = oldStage,
            NewGrowthStage = newStage
        });
    }

    /// <summary>
    /// 触发装饰物放置事件
    /// </summary>
    public bool RaiseDecorationPlaced(int bioTankId, int decorationId, int typeId, string typeName, Vector3 position, Quaternion rotation, object instance)
    {
        var args = new DecorationEventArgs
        {
            BioTankId = bioTankId,
            DecorationId = decorationId,
            TypeId = typeId,
            TypeName = typeName,
            Position = position,
            Rotation = rotation,
            DecorationInstance = instance
        };
        SafeInvoke(OnDecorationPlaced, args);
        return !args.Cancel;
    }

    /// <summary>
    /// 触发装饰物移除事件
    /// </summary>
    public void RaiseDecorationRemoved(int bioTankId, int decorationId, int typeId, string typeName, Vector3 position, object instance)
    {
        SafeInvoke(OnDecorationRemoved, new DecorationEventArgs
        {
            BioTankId = bioTankId,
            DecorationId = decorationId,
            TypeId = typeId,
            TypeName = typeName,
            Position = position,
            DecorationInstance = instance
        });
    }

    /// <summary>
    /// 触发货币变化事件
    /// </summary>
    public void RaiseCurrencyChanged(string currencyType, long oldAmount, long newAmount, string reason)
    {
        SafeInvoke(OnCurrencyChanged, new CurrencyEventArgs
        {
            CurrencyType = currencyType,
            OldAmount = oldAmount,
            NewAmount = newAmount,
            Delta = newAmount - oldAmount,
            Reason = reason
        });
    }

    /// <summary>
    /// 触发购买事件
    /// </summary>
    public bool RaiseItemPurchased(int itemId, string itemType, int quantity, long price, string currencyType)
    {
        var args = new PurchaseEventArgs
        {
            ItemId = itemId,
            ItemType = itemType,
            Quantity = quantity,
            Price = price,
            CurrencyType = currencyType
        };
        SafeInvoke(OnItemPurchased, args);
        return !args.Cancel;
    }

    /// <summary>
    /// 触发出售事件
    /// </summary>
    public bool RaiseItemSold(int itemId, string itemType, int quantity, long price, string currencyType)
    {
        var args = new SellEventArgs
        {
            ItemId = itemId,
            ItemType = itemType,
            Quantity = quantity,
            Price = price,
            CurrencyType = currencyType
        };
        SafeInvoke(OnItemSold, args);
        return !args.Cancel;
    }

    /// <summary>
    /// 触发游戏保存事件
    /// </summary>
    public void RaiseGameSaving(string savePath, int slotId)
    {
        SafeInvoke(OnGameSaving, new SaveEventArgs
        {
            SavePath = savePath,
            SlotId = slotId
        });
    }

    /// <summary>
    /// 触发游戏保存完成事件
    /// </summary>
    public void RaiseGameSaved(string savePath, int slotId)
    {
        SafeInvoke(OnGameSaved, new SaveEventArgs
        {
            SavePath = savePath,
            SlotId = slotId
        });
    }

    /// <summary>
    /// 触发游戏加载事件
    /// </summary>
    public void RaiseGameLoading(string loadPath, int slotId)
    {
        SafeInvoke(OnGameLoading, new LoadEventArgs
        {
            LoadPath = loadPath,
            SlotId = slotId
        });
    }

    /// <summary>
    /// 触发游戏加载完成事件
    /// </summary>
    public void RaiseGameLoaded(string loadPath, int slotId)
    {
        SafeInvoke(OnGameLoaded, new LoadEventArgs
        {
            LoadPath = loadPath,
            SlotId = slotId
        });
    }

    /// <summary>
    /// 触发时间流逝事件
    /// </summary>
    public void RaiseTimeTick(int gameMinutes, int gameHours, int gameDays, float realDeltaTime)
    {
        SafeInvoke(OnTimeTick, new TimeEventArgs
        {
            GameMinutes = gameMinutes,
            GameHours = gameHours,
            GameDays = gameDays,
            RealDeltaTime = realDeltaTime
        });
    }

    #endregion

    #region 工具方法

    private void SafeInvoke<T>(Action<T> eventHandler, T args) where T : ModEventArgs
    {
        if (eventHandler == null) return;

        foreach (var handler in eventHandler.GetInvocationList())
        {
            try
            {
                ((Action<T>)handler)(args);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ModGameEvents] 事件处理器异常: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    /// <summary>
    /// 清除所有事件订阅
    /// </summary>
    public void ClearAllSubscriptions()
    {
        OnBioTankCreated = null;
        OnBioTankDestroyed = null;
        OnBioTankActivated = null;
        OnBioTankStateUpdate = null;
        OnCreatureSpawned = null;
        OnCreatureDied = null;
        OnCreatureBreed = null;
        OnCreatureStateChanged = null;
        OnPlantPlaced = null;
        OnPlantRemoved = null;
        OnPlantGrowth = null;
        OnDecorationPlaced = null;
        OnDecorationRemoved = null;
        OnCurrencyChanged = null;
        OnItemPurchased = null;
        OnItemSold = null;
        OnGameSaving = null;
        OnGameSaved = null;
        OnGameLoading = null;
        OnGameLoaded = null;
        OnTimeTick = null;

        Debug.Log("[ModGameEvents] 已清除所有事件订阅");
    }

    #endregion
}
