/*****************************************************************
 * --@ FileName: UnlockableItemManager
 * --@ Description: 解锁型物品管理器 - 专门管理Cube和Decoration等解锁后可无限使用的物品
 * --@ Features:
 *    1. 追踪Cube和Decoration的解锁状态
 *    2. 提供解锁/查询接口
 *    3. 自动同步到仓库系统
 *    4. 支持数据持久化
 * --@ Author: Claude Code
 * --@ Copyright: Copyright (c) 2025
 ******************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

/// <summary>
/// 解锁型物品数据 - 用于序列化保存
/// </summary>
[Serializable]
public class UnlockableItemData
{
    public List<string> unlockedCubeIds = new List<string>();
    public List<string> unlockedDecorationIds = new List<string>();
}

/// <summary>
/// 解锁型物品管理器
/// 与消耗型物品（鱼、植物）不同，Cube和Decoration是解锁型物品：
/// - 解锁后永久可用
/// - 可以无限次放置（每次放置消耗金币，但不消耗物品本身）
/// - 不存在数量概念
/// </summary>
public class UnlockableItemManager : MonoSingleton<UnlockableItemManager>
{
    #region 数据存储

    /// <summary>
    /// 已解锁的Cube ID集合
    /// </summary>
    private HashSet<string> _unlockedCubes = new HashSet<string>();

    /// <summary>
    /// 已解锁的Decoration ID集合
    /// </summary>
    private HashSet<string> _unlockedDecorations = new HashSet<string>();

    #endregion

    #region 初始化

    public override void Init()
    {
        base.Init();

        // 如果需要从图鉴同步已解锁状态
        SyncFromIllustratedGuide();

        // 自动同步到仓库
        SyncToWarehouse();
    }

    /// <summary>
    /// 从图鉴系统同步已解锁状态
    /// 用于首次初始化或修复数据不一致问题
    /// </summary>
    public void SyncFromIllustratedGuide()
    {
        if (IlluGuideMgr.Instance == null)
        {
            Debug.LogWarning("[UnlockableItemManager] IlluGuideMgr未初始化，跳过同步");
            return;
        }

        int cubeSynced = 0;
        int decoSynced = 0;

        // 同步Cube
        foreach (var entry in IlluGuideMgr.Instance.cubeEntries.Values)
        {
            if (entry.isUnlocked && !_unlockedCubes.Contains(entry.id))
            {
                _unlockedCubes.Add(entry.id);
                cubeSynced++;
            }
        }

        // 同步Decoration
        foreach (var entry in IlluGuideMgr.Instance.decorationEntries.Values)
        {
            if (entry.isUnlocked && !_unlockedDecorations.Contains(entry.id))
            {
                _unlockedDecorations.Add(entry.id);
                decoSynced++;
            }
        }
    }

    #endregion

    #region 解锁功能

    /// <summary>
    /// 解锁Cube
    /// </summary>
    /// <param name="cubeId">Cube的ID</param>
    /// <param name="syncToWarehouse">是否立即同步到仓库</param>
    /// <returns>是否成功解锁（如果已解锁则返回false）</returns>
    public bool UnlockCube(string cubeId, bool syncToWarehouse = true)
    {
        if (string.IsNullOrEmpty(cubeId))
        {
            Debug.LogError("[UnlockableItemManager] Cube ID不能为空");
            return false;
        }

        if (_unlockedCubes.Contains(cubeId))
        {
            return false;
        }

        _unlockedCubes.Add(cubeId);

        // 触发解锁事件
        event_manager.instance.dispatch_event("UnlockableItem_CubeUnlocked", cubeId);

        // 同步到仓库
        if (syncToWarehouse)
        {
            SyncCubeToWarehouse(cubeId);
        }

        return true;
    }

    /// <summary>
    /// 解锁Decoration
    /// </summary>
    /// <param name="decorationId">Decoration的ID</param>
    /// <param name="syncToWarehouse">是否立即同步到仓库</param>
    /// <returns>是否成功解锁（如果已解锁则返回false）</returns>
    public bool UnlockDecoration(string decorationId, bool syncToWarehouse = true)
    {
        if (string.IsNullOrEmpty(decorationId))
        {
            Debug.LogError("[UnlockableItemManager] Decoration ID不能为空");
            return false;
        }

        if (_unlockedDecorations.Contains(decorationId))
        {
            return false;
        }

        _unlockedDecorations.Add(decorationId);

        // 触发解锁事件
        event_manager.instance.dispatch_event("UnlockableItem_DecorationUnlocked", decorationId);

        // 同步到仓库
        if (syncToWarehouse)
        {
            SyncDecorationToWarehouse(decorationId);
        }

        return true;
    }

    /// <summary>
    /// 批量解锁Cube
    /// </summary>
    public void UnlockCubes(IEnumerable<string> cubeIds)
    {
        foreach (var cubeId in cubeIds)
        {
            UnlockCube(cubeId, false);
        }
        SyncToWarehouse();
    }

    /// <summary>
    /// 批量解锁Decoration
    /// </summary>
    public void UnlockDecorations(IEnumerable<string> decorationIds)
    {
        foreach (var decorationId in decorationIds)
        {
            UnlockDecoration(decorationId, false);
        }
        SyncToWarehouse();
    }

    #endregion

    #region 查询功能

    /// <summary>
    /// 检查Cube是否已解锁
    /// </summary>
    public bool IsCubeUnlocked(string cubeId)
    {
        return _unlockedCubes.Contains(cubeId);
    }

    /// <summary>
    /// 检查Decoration是否已解锁
    /// </summary>
    public bool IsDecorationUnlocked(string decorationId)
    {
        return _unlockedDecorations.Contains(decorationId);
    }

    /// <summary>
    /// 检查物品是否已解锁（通用方法）
    /// </summary>
    public bool IsUnlocked(ItemType type, string itemId)
    {
        switch (type)
        {
            case ItemType.Cube:
                return IsCubeUnlocked(itemId);
            case ItemType.Decoration:
                return IsDecorationUnlocked(itemId);
            default:
                Debug.LogWarning($"[UnlockableItemManager] 不支持的物品类型: {type}");
                return false;
        }
    }

    /// <summary>
    /// 获取所有已解锁的Cube ID列表
    /// </summary>
    public List<string> GetUnlockedCubeIds()
    {
        return _unlockedCubes.ToList();
    }

    /// <summary>
    /// 获取所有已解锁的Decoration ID列表
    /// </summary>
    public List<string> GetUnlockedDecorationIds()
    {
        return _unlockedDecorations.ToList();
    }

    /// <summary>
    /// 获取已解锁的Cube数量
    /// </summary>
    public int GetUnlockedCubeCount()
    {
        return _unlockedCubes.Count;
    }

    /// <summary>
    /// 获取已解锁的Decoration数量
    /// </summary>
    public int GetUnlockedDecorationCount()
    {
        return _unlockedDecorations.Count;
    }

    #endregion

    #region 仓库同步

    /// <summary>
    /// 同步所有已解锁物品到仓库
    /// </summary>
    public void SyncToWarehouse()
    {
        var warehouseService = ServiceLocator.Get<IWarehouseService>();
        if (warehouseService == null)
        {
            Debug.LogError("[UnlockableItemManager] 无法获取WarehouseService");
            return;
        }

        int cubeSynced = 0;
        int decoSynced = 0;

        // 同步所有已解锁的Cube
        foreach (var cubeId in _unlockedCubes)
        {
            if (SyncCubeToWarehouse(cubeId))
            {
                cubeSynced++;
            }
        }

        // 同步所有已解锁的Decoration
        foreach (var decorationId in _unlockedDecorations)
        {
            if (SyncDecorationToWarehouse(decorationId))
            {
                decoSynced++;
            }
        }

        // 触发同步完成事件
        event_manager.instance.dispatch_event("UnlockableItem_SyncComplete", null);
    }

    /// <summary>
    /// 同步单个Cube到仓库
    /// </summary>
    private bool SyncCubeToWarehouse(string cubeId)
    {
        var warehouseService = ServiceLocator.Get<IWarehouseService>();
        if (warehouseService == null) return false;

        // 检查仓库中是否已存在
        var cubeList = warehouseService.GetWarehouseBaseDatas(ItemType.Cube);
        bool existsInWarehouse = cubeList.Any(item =>
        {
            var cubeData = item as WarehouseCubeStateData;
            return cubeData != null && cubeData.stateData.Type == cubeId;
        });

        if (!existsInWarehouse)
        {
            var cubeStateData = new CubeStateData { Type = cubeId };
            warehouseService.SaveCube(cubeStateData, false);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 同步单个Decoration到仓库
    /// </summary>
    private bool SyncDecorationToWarehouse(string decorationId)
    {
        var warehouseService = ServiceLocator.Get<IWarehouseService>();
        if (warehouseService == null) return false;

        // 检查仓库中是否已存在
        var decoList = warehouseService.GetWarehouseBaseDatas(ItemType.Decoration);
        bool existsInWarehouse = decoList.Any(item =>
        {
            var decoData = item as WarehouseDecorationStateData;
            return decoData != null && decoData.stateData.Type == decorationId;
        });

        if (!existsInWarehouse)
        {
            var decoStateData = new DecorationStateData { Type = decorationId };
            warehouseService.SaveDecoration(decoStateData, false);
            return true;
        }

        return false;
    }

    #endregion

    #region 调试工具

    /// <summary>
    /// 解锁所有配置的Cube
    /// </summary>
    public void UnlockAllCubes()
    {
        var cubeDataList = GameConfigDataBase.GetConfigDatasByDic<CubeData>(SysDefine.CubeConfigData);
        if (cubeDataList == null || cubeDataList.Count == 0)
        {
            Debug.LogWarning("[UnlockableItemManager] 没有Cube配置数据");
            return;
        }

        int unlocked = 0;
        foreach (var cubeData in cubeDataList.Values)
        {
            if (UnlockCube(cubeData.ID, false))
            {
                unlocked++;
            }
        }

        SyncToWarehouse();
    }

    /// <summary>
    /// 解锁所有配置的Decoration
    /// </summary>
    public void UnlockAllDecorations()
    {
        var decorationDataList = GameConfigDataBase.GetConfigDatasByDic<DecorationData>(SysDefine.DecorationConfigData);
        if (decorationDataList == null || decorationDataList.Count == 0)
        {
            Debug.LogWarning("[UnlockableItemManager] 没有Decoration配置数据");
            return;
        }

        int unlocked = 0;
        foreach (var decorationData in decorationDataList.Values)
        {
            if (UnlockDecoration(decorationData.ID, false))
            {
                unlocked++;
            }
        }

        SyncToWarehouse();
    }

    /// <summary>
    /// 打印所有已解锁物品信息
    /// </summary>
    public void PrintUnlockedItems()
    {
    }

    #endregion

    #region 数据持久化

    /// <summary>
    /// 获取序列化数据
    /// </summary>
    public UnlockableItemData GetSaveData()
    {
        return new UnlockableItemData
        {
            unlockedCubeIds = _unlockedCubes.ToList(),
            unlockedDecorationIds = _unlockedDecorations.ToList()
        };
    }

    /// <summary>
    /// 加载序列化数据
    /// </summary>
    public void LoadSaveData(UnlockableItemData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[UnlockableItemManager] 加载数据为null");
            return;
        }

        _unlockedCubes = new HashSet<string>(data.unlockedCubeIds);
        _unlockedDecorations = new HashSet<string>(data.unlockedDecorationIds);

        // 加载后自动同步到仓库
        SyncToWarehouse();
    }

    #endregion
}
