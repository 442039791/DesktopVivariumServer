using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 方块仓库存储 - 专门管理方块数据
/// </summary>
public class CubeWarehouseStorage : WarehouseStorageBase<WarehouseCubeStateData>
{
    /// <summary>
    /// 保存方块到仓库
    /// </summary>
    public int SaveCube(CubeStateData cubeStateData, bool updateUI = true)
    {
        if (cubeStateData == null)
        {
            Debug.LogError("[CubeWarehouseStorage] 方块数据为null");
            return -1;
        }

        // 获取方块配置数据以设置Name
        var cubeData = cubeStateData.GetCubeData();

        var warehouseData = new WarehouseCubeStateData
        {
            stateData = cubeStateData,
            Count = 1,
            DataType = WarehouseBaseDataType.Cube,
            // Icon在UI层动态获取，避免Unity HideFlags问题
            Name = cubeData.name
        };

        int id = AddItem(warehouseData, updateUI);
        return id;
    }

    /// <summary>
    /// 从仓库取出方块
    /// </summary>
    public WarehouseCubeStateData DislodgeCube(CubeStateData cubeStateData)
    {
        // 根据方块状态数据查找匹配的仓库数据
        foreach (var kvp in _dataDictionary)
        {
            var warehouseData = kvp.Value;
            if (warehouseData.stateData.Type == cubeStateData.Type)
            {
                return RemoveItem(kvp.Key);
            }
        }

        Debug.LogWarning($"[CubeWarehouseStorage] 未找到匹配的方块: 类型={cubeStateData.Type}");
        return null;
    }
}
