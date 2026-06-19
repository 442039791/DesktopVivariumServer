using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 仓库存储基类 - 提供通用的仓库数据管理功能
/// </summary>
/// <typeparam name="TWarehouseData">仓库数据类型（继承自WarehouseBaseData）</typeparam>
public abstract class WarehouseStorageBase<TWarehouseData> where TWarehouseData : WarehouseBaseData
{
    /// <summary>
    /// 数据字典（ID -> 数据）
    /// </summary>
    protected readonly Dictionary<int, TWarehouseData> _dataDictionary = new Dictionary<int, TWarehouseData>();

    /// <summary>
    /// 数据列表（用于UI显示）
    /// </summary>
    protected readonly List<WarehouseBaseData> _dataList = new List<WarehouseBaseData>();

    /// <summary>
    /// 获取数据字典（只读）
    /// </summary>
    public IReadOnlyDictionary<int, TWarehouseData> DataDictionary => _dataDictionary;

    /// <summary>
    /// 获取数据列表（只读）
    /// </summary>
    public IReadOnlyList<WarehouseBaseData> DataList => _dataList;

    /// <summary>
    /// 获取数据数量
    /// </summary>
    public int Count => _dataDictionary.Count;

    /// <summary>
    /// 下一个可用ID
    /// </summary>
    protected int _nextId = 0;

    /// <summary>
    /// 添加物品到仓库
    /// </summary>
    /// <param name="data">仓库数据</param>
    /// <param name="updateUI">是否更新UI</param>
    /// <returns>仓库基础ID</returns>
    public virtual int AddItem(TWarehouseData data, bool updateUI = true)
    {
        if (data == null)
        {
            Debug.LogError($"[{GetType().Name}] 数据为null");
            return -1;
        }

        // 分配ID
        int warehouseBaseId = _nextId++;
        data.WarehouseBaseID = warehouseBaseId;

        // 添加到字典和列表
        _dataDictionary.Add(warehouseBaseId, data);
        _dataList.Add(data);

        if (updateUI)
        {
            OnDataChanged();
        }

        return warehouseBaseId;
    }

    /// <summary>
    /// 从仓库移除物品
    /// </summary>
    /// <param name="warehouseBaseId">仓库基础ID</param>
    /// <returns>被移除的数据</returns>
    public virtual TWarehouseData RemoveItem(int warehouseBaseId)
    {
        if (!_dataDictionary.TryGetValue(warehouseBaseId, out var data))
        {
            Debug.LogWarning($"[{GetType().Name}] 未找到物品: ID={warehouseBaseId}");
            return null;
        }

        _dataDictionary.Remove(warehouseBaseId);
        _dataList.Remove(data);

        OnDataChanged();
        return data;
    }

    /// <summary>
    /// 尝试获取物品
    /// </summary>
    public bool TryGetItem(int warehouseBaseId, out TWarehouseData data)
    {
        return _dataDictionary.TryGetValue(warehouseBaseId, out data);
    }

    /// <summary>
    /// 检查物品是否存在
    /// </summary>
    public bool ContainsItem(int warehouseBaseId)
    {
        return _dataDictionary.ContainsKey(warehouseBaseId);
    }

    /// <summary>
    /// 清空所有数据
    /// </summary>
    public virtual void Clear()
    {
        _dataDictionary.Clear();
        _dataList.Clear();
        _nextId = 0;

        OnDataChanged();
    }

    /// <summary>
    /// 使用自定义比较器排序数据列表
    /// </summary>
    public virtual void Sort(System.Comparison<WarehouseBaseData> comparison)
    {
        _dataList.Sort(comparison);
    }

    /// <summary>
    /// 数据变化时触发（子类可重写以实现特定的UI更新逻辑）
    /// </summary>
    protected virtual void OnDataChanged()
    {
        // 由WarehouseManager负责触发具体的UI更新事件
        // 存储类只负责数据管理
    }

    /// <summary>
    /// 获取所有数据的副本
    /// </summary>
    public List<TWarehouseData> GetAllData()
    {
        return _dataDictionary.Values.ToList();
    }
}
