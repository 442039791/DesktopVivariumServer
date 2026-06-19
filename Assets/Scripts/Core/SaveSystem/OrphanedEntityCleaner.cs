using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 孤儿实体清理器 - 用于清理模组卸载后遗留的无效实体
/// 当玩家安装模组获得了模组中的动植物，之后卸载模组时，
/// 这些已经没有配置数据的实体需要被清理掉
/// </summary>
public class OrphanedEntityCleaner
{
    /// <summary>
    /// 清理结果
    /// </summary>
    public class CleanupResult
    {
        public int RemovedAnimals { get; set; }
        public int RemovedPlants { get; set; }
        public int RemovedDecorations { get; set; }
        public int RemovedWarehouseAnimals { get; set; }
        public int RemovedWarehousePlants { get; set; }
        public int RemovedWarehouseDecorations { get; set; }
        public int RemovedWarehouseCubes { get; set; }
        public List<string> RemovedTypes { get; set; } = new List<string>();

        public int TotalRemoved => RemovedAnimals + RemovedPlants + RemovedDecorations +
                                   RemovedWarehouseAnimals + RemovedWarehousePlants +
                                   RemovedWarehouseDecorations + RemovedWarehouseCubes;

        public bool HasRemovedEntities => TotalRemoved > 0;
    }

    /// <summary>
    /// 清理存档数据中的孤儿实体
    /// 在加载存档后、创建游戏对象前调用
    /// </summary>
    public CleanupResult CleanupOrphanedEntities(SaveData saveData)
    {
        var result = new CleanupResult();

        if (saveData == null)
        {
            return result;
        }

        // 清理生态箱中的动植物
        if (saveData.BioTankStateDatas != null)
        {
            foreach (var bioTank in saveData.BioTankStateDatas)
            {
                // 清理动物
                if (bioTank.Animals != null && bioTank.Animals.Length > 0)
                {
                    var validAnimals = new List<AnimalStateData>();
                    foreach (var animal in bioTank.Animals)
                    {
                        if (IsValidAnimal(animal))
                        {
                            validAnimals.Add(animal);
                        }
                        else
                        {
                            result.RemovedAnimals++;
                            if (!result.RemovedTypes.Contains($"Animal:{animal.Type}"))
                            {
                                result.RemovedTypes.Add($"Animal:{animal.Type}");
                            }
                        }
                    }
                    bioTank.Animals = validAnimals.ToArray();
                }

                // 清理植物
                if (bioTank.Plants != null && bioTank.Plants.Length > 0)
                {
                    var validPlants = new List<PlantStateData>();
                    foreach (var plant in bioTank.Plants)
                    {
                        if (IsValidPlant(plant))
                        {
                            validPlants.Add(plant);
                        }
                        else
                        {
                            result.RemovedPlants++;
                            if (!result.RemovedTypes.Contains($"Plant:{plant.Type}"))
                            {
                                result.RemovedTypes.Add($"Plant:{plant.Type}");
                            }
                        }
                    }
                    bioTank.Plants = validPlants.ToArray();
                }
            }
        }

        // 清理旧格式的全局动植物数组（兼容旧存档）
        if (saveData.fishStates != null && saveData.fishStates.Length > 0)
        {
            var validAnimals = saveData.fishStates.Where(a => IsValidAnimal(a)).ToArray();
            int removed = saveData.fishStates.Length - validAnimals.Length;
            if (removed > 0)
            {
                result.RemovedAnimals += removed;
                saveData.fishStates = validAnimals;
            }
        }

        if (saveData.plantStateDatas != null && saveData.plantStateDatas.Length > 0)
        {
            var validPlants = saveData.plantStateDatas.Where(p => IsValidPlant(p)).ToArray();
            int removed = saveData.plantStateDatas.Length - validPlants.Length;
            if (removed > 0)
            {
                result.RemovedPlants += removed;
                saveData.plantStateDatas = validPlants;
            }
        }

        // 清理装饰物
        if (saveData.decorationStateDatas != null && saveData.decorationStateDatas.Length > 0)
        {
            var validDecorations = new List<DecorationStateData>();
            foreach (var decoration in saveData.decorationStateDatas)
            {
                if (IsValidDecoration(decoration))
                {
                    validDecorations.Add(decoration);
                }
                else
                {
                    result.RemovedDecorations++;
                    if (!result.RemovedTypes.Contains($"Decoration:{decoration.Type}"))
                    {
                        result.RemovedTypes.Add($"Decoration:{decoration.Type}");
                    }
                }
            }
            saveData.decorationStateDatas = validDecorations.ToArray();
        }

        // 清理仓库数据
        if (saveData.warehouseSaveData != null)
        {
            CleanupWarehouseData(saveData.warehouseSaveData, result);
        }

        // 输出清理结果日志
        if (result.HasRemovedEntities)
        {
            Debug.Log($"[OrphanedEntityCleaner] 清理完成 - " +
                      $"动物: {result.RemovedAnimals}, 植物: {result.RemovedPlants}, 装饰: {result.RemovedDecorations}, " +
                      $"仓库动物: {result.RemovedWarehouseAnimals}, 仓库植物: {result.RemovedWarehousePlants}, " +
                      $"仓库装饰: {result.RemovedWarehouseDecorations}, 仓库方块: {result.RemovedWarehouseCubes}");
            Debug.Log($"[OrphanedEntityCleaner] 已删除的类型: {string.Join(", ", result.RemovedTypes)}");
        }

        return result;
    }

    /// <summary>
    /// 清理仓库数据中的孤儿实体
    /// </summary>
    private void CleanupWarehouseData(WarehouseSaveData warehouseData, CleanupResult result)
    {
        // 清理仓库动物
        if (warehouseData.fishStates != null && warehouseData.fishStates.Length > 0)
        {
            var validAnimals = warehouseData.fishStates.Where(a => IsValidAnimal(a)).ToArray();
            result.RemovedWarehouseAnimals = warehouseData.fishStates.Length - validAnimals.Length;
            warehouseData.fishStates = validAnimals;
        }

        // 清理仓库植物
        if (warehouseData.plantStateDatas != null && warehouseData.plantStateDatas.Length > 0)
        {
            var validPlants = warehouseData.plantStateDatas.Where(p => IsValidPlant(p)).ToArray();
            result.RemovedWarehousePlants = warehouseData.plantStateDatas.Length - validPlants.Length;
            warehouseData.plantStateDatas = validPlants;
        }

        // 清理仓库装饰物
        if (warehouseData.decorationStateDatas != null && warehouseData.decorationStateDatas.Length > 0)
        {
            var validDecorations = warehouseData.decorationStateDatas.Where(d => IsValidDecoration(d)).ToArray();
            result.RemovedWarehouseDecorations = warehouseData.decorationStateDatas.Length - validDecorations.Length;
            warehouseData.decorationStateDatas = validDecorations;
        }

        // 清理仓库方块
        if (warehouseData.cubeStateDatas != null && warehouseData.cubeStateDatas.Length > 0)
        {
            var validCubes = warehouseData.cubeStateDatas.Where(c => IsValidCube(c)).ToArray();
            result.RemovedWarehouseCubes = warehouseData.cubeStateDatas.Length - validCubes.Length;
            warehouseData.cubeStateDatas = validCubes;
        }
    }

    /// <summary>
    /// 检查动物配置是否有效
    /// </summary>
    private bool IsValidAnimal(AnimalStateData animal)
    {
        if (animal == null || string.IsNullOrEmpty(animal.Type))
        {
            return false;
        }
        return GameConfigDataBase.HasConfigData<AnimalData>(animal.Type, SysDefine.AnimalConfigData);
    }

    /// <summary>
    /// 检查植物配置是否有效
    /// </summary>
    private bool IsValidPlant(PlantStateData plant)
    {
        if (plant == null || string.IsNullOrEmpty(plant.Type))
        {
            return false;
        }
        return GameConfigDataBase.HasConfigData<PlantData>(plant.Type, SysDefine.PlantConfigData);
    }

    /// <summary>
    /// 检查装饰物配置是否有效
    /// </summary>
    private bool IsValidDecoration(DecorationStateData decoration)
    {
        if (decoration == null || string.IsNullOrEmpty(decoration.Type))
        {
            return false;
        }
        return GameConfigDataBase.HasConfigData<DecorationData>(decoration.Type, SysDefine.DecorationConfigData);
    }

    /// <summary>
    /// 检查方块配置是否有效
    /// </summary>
    private bool IsValidCube(CubeStateData cube)
    {
        if (cube == null || string.IsNullOrEmpty(cube.Type))
        {
            return false;
        }
        return GameConfigDataBase.HasConfigData<CubeData>(cube.Type, SysDefine.CubeConfigData);
    }
}
