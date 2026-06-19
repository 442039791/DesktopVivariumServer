using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 存档迁移服务 - 负责将旧格式存档迁移到新格式
/// </summary>
public class SaveMigrationService
{
    private readonly SaveSystem _saveSystem;

    public SaveMigrationService(SaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    /// <summary>
    /// 检查并迁移存档格式
    /// 在服务器启动时调用一次
    /// </summary>
    public void CheckAndMigrateSaveData()
    {
        SaveData saveData = _saveSystem.LoadSaveData();
        if (saveData == null)
        {
            Debug.Log("[SaveMigration] 没有存档，无需迁移");
            return;
        }

        bool needsMigration = NeedsMigration(saveData);
        if (!needsMigration)
        {
            Debug.Log("[SaveMigration] 存档已是新格式，无需迁移");
            return;
        }

        Debug.Log("[SaveMigration] 检测到旧格式存档，开始迁移...");
        MigrateSaveData(saveData);
        Debug.Log("[SaveMigration] 存档迁移完成！");
    }

    /// <summary>
    /// 检查是否需要迁移
    /// </summary>
    private bool NeedsMigration(SaveData saveData)
    {
        // 检查是否有生态箱数据
        if (saveData.BioTankStateDatas == null || saveData.BioTankStateDatas.Length == 0)
        {
            return false;
        }

        // 检查是否使用了旧格式（全局数组）
        bool hasGlobalAnimals = saveData.fishStates != null && saveData.fishStates.Length > 0;
        bool hasGlobalPlants = saveData.plantStateDatas != null && saveData.plantStateDatas.Length > 0;

        // 检查生态箱内部是否有数据（新格式）
        bool hasInlineData = saveData.BioTankStateDatas.Any(bt =>
            (bt.Animals != null && bt.Animals.Length > 0) ||
            (bt.Plants != null && bt.Plants.Length > 0) ||
            (bt.Facilities != null && bt.Facilities.Length > 0)
        );

        // 如果有全局数组但没有内嵌数据，需要迁移
        return (hasGlobalAnimals || hasGlobalPlants) && !hasInlineData;
    }

    /// <summary>
    /// 执行存档迁移
    /// </summary>
    private void MigrateSaveData(SaveData saveData)
    {
        Debug.Log($"[SaveMigration] 迁移前 - 全局动物: {saveData.fishStates?.Length ?? 0}, 全局植物: {saveData.plantStateDatas?.Length ?? 0}");

        // 为每个生态箱分配动物和植物
        foreach (var bioTankData in saveData.BioTankStateDatas)
        {
            // 迁移动物数据
            if (saveData.fishStates != null)
            {
                var animalsInThisTank = saveData.fishStates
                    .Where(a => a.ID == bioTankData.ID)
                    .ToArray();

                bioTankData.Animals = animalsInThisTank;
                Debug.Log($"[SaveMigration] 生态箱 {bioTankData.ID} - 迁移动物: {animalsInThisTank.Length}");
            }

            // 迁移植物数据
            if (saveData.plantStateDatas != null)
            {
                var plantsInThisTank = saveData.plantStateDatas
                    .Where(p => p.ID == bioTankData.ID)
                    .ToArray();

                bioTankData.Plants = plantsInThisTank;
                Debug.Log($"[SaveMigration] 生态箱 {bioTankData.ID} - 迁移植物: {plantsInThisTank.Length}");
            }

            // 迁移设备数据
            if (saveData.facilitiesStateDatas != null)
            {
                var facilitiesInThisTank = saveData.facilitiesStateDatas
                    .Where(f => f.ObjID == bioTankData.ID)
                    .ToArray();

                bioTankData.Facilities = facilitiesInThisTank;
                Debug.Log($"[SaveMigration] 生态箱 {bioTankData.ID} - 迁移设备: {facilitiesInThisTank.Length}");
            }
        }

        // 清空全局数组（已迁移到生态箱内部）
        saveData.fishStates = new AnimalStateData[0];
        saveData.plantStateDatas = new PlantStateData[0];
        // 注意：facilitiesStateDatas 仍然需要保留，因为还有解锁状态等全局数据

        // 保存迁移后的存档
        // 需要创建一个空的 GatherSaveData 用于保存
        GatherSaveData gatherSaveData = new GatherSaveData();
        _saveSystem.SaveInitialData(saveData, gatherSaveData);
        Debug.Log("[SaveMigration] 迁移后的存档已保存");
    }
}
