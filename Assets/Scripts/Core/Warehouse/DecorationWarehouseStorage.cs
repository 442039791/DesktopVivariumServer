using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装饰物仓库存储 - 专门管理装饰物数据
/// </summary>
public class DecorationWarehouseStorage : WarehouseStorageBase<WarehouseDecorationStateData>
{
    /// <summary>
    /// 保存装饰物到仓库
    /// </summary>
    public int SaveDecoration(DecorationStateData decorationStateData, bool updateUI = true)
    {
        if (decorationStateData == null)
        {
            Debug.LogError("[DecorationWarehouseStorage] 装饰物数据为null");
            return -1;
        }

        // 获取装饰物配置数据以设置Name
        var decorationData = decorationStateData.GetDecorationData();

        var warehouseData = new WarehouseDecorationStateData
        {
            stateData = decorationStateData,
            Count = 1,
            DataType = WarehouseBaseDataType.Decoration,
            // Icon在UI层动态获取，避免Unity HideFlags问题
            Name = decorationData.name
        };

        return AddItem(warehouseData, updateUI);
    }

    /// <summary>
    /// 从仓库取出装饰物
    /// </summary>
    public WarehouseDecorationStateData DislodgeDecoration(DecorationStateData decorationStateData)
    {
        // 根据装饰物状态数据查找匹配的仓库数据
        foreach (var kvp in _dataDictionary)
        {
            var warehouseData = kvp.Value;
            if (warehouseData.stateData.Type == decorationStateData.Type)
            {
                return RemoveItem(kvp.Key);
            }
        }

        Debug.LogWarning($"[DecorationWarehouseStorage] 未找到匹配的装饰物: 类型={decorationStateData.Type}");
        return null;
    }
}
