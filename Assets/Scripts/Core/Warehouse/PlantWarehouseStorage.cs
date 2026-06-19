using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 植物仓库存储 - 专门管理植物数据
/// </summary>
public class PlantWarehouseStorage : WarehouseStorageBase<WarehousePlantStateData>
{
    /// <summary>
    /// 保存植物到仓库
    /// </summary>
    public int SavePlant(PlantStateData plantStateData, bool updateUI = true)
    {
        if (plantStateData == null)
        {
            Debug.LogError("[PlantWarehouseStorage] 植物数据为null");
            return -1;
        }

        // 获取植物配置数据以设置Name
        var plantData = plantStateData.GetPlantData();

        var warehouseData = new WarehousePlantStateData
        {
            data = plantStateData,
            Count = 1,
            DataType = WarehouseBaseDataType.Plant,
            // Icon在UI层动态获取，避免Unity HideFlags问题
            Name = plantData.name
        };

        return AddItem(warehouseData, updateUI);
    }

    /// <summary>
    /// 从仓库取出植物
    /// </summary>
    public WarehousePlantStateData DislodgePlant(int warehouseBaseId)
    {
        return RemoveItem(warehouseBaseId);
    }
}
