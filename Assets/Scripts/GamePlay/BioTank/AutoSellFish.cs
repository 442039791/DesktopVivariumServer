using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    /// <summary>
    /// 自动出售动物
    /// </summary>
    /// <param name="bioTankId"></param>
    public void AutoSellAnimal(int bioTankId)
    {
        // 清理无效的自动售卖动物配置
        CleanInvalidAutoSellAnimalConfigs(bioTankId);

        var autoSellAnimalInfos = FacilityManager.Instance.GetBioTankFacilityStatus(bioTankId, 5002);
        var result = AnimalCollection.Entities
            .Where(animal => autoSellAnimalInfos.Any(cond =>
                animal.stateData.Type == cond.ID &&
                animal.stateData.GetObjectQuality() == cond.Quality &&
                animal.State == cond.State))
            .ToList();
        if (result.Count <= 0) return;
        foreach (var animalInfo in result)
        {
            // 获取动物的配置数据
            var animalData = GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(animalInfo.stateData.Type));

            // 计算动物的价格(根据品质和年龄状态)
            bool isAdult = (animalInfo.State == AnimalAgeStatus.Adult);
            int price = animalData.GetPrice(animalInfo.stateData.GetObjectQuality(), isAdult);

            // 给玩家加钱
            PlayerManager.Instance.OnMoneyChanged(price);

            // 删除动物
            animalInfo.Destroy();
        }
    }

    /// <summary>
    /// 清理自动售卖动物配置中不存在的动物种类
    /// </summary>
    /// <param name="bioTankId"></param>
    public void CleanInvalidAutoSellAnimalConfigs(int bioTankId)
    {
        var autoSellAnimalInfos = FacilityManager.Instance.GetBioTankFacilityStatus(bioTankId, 5002);
        if (autoSellAnimalInfos.Count <= 0) return;

        // 获取当前生态缸中所有动物的种类
        var currentAnimalTypes = new HashSet<string>(
            AnimalCollection.Entities
                .Select(animal => animal.stateData.Type)
                .Distinct()
        );

        // 找出配置中不存在于当前生态缸的动物种类
        var invalidConfigs = autoSellAnimalInfos
            .Where(config => !currentAnimalTypes.Contains(config.ID))
            .ToList();

        if (invalidConfigs.Count <= 0) return;

        // 从配置中删除无效的动物种类
        foreach (var invalidConfig in invalidConfigs)
        {
            FacilityManager.Instance.RemoveBioTankAutoSellAnimalInfo(
                bioTankId,
                5002,
                invalidConfig.ID,
                invalidConfig.State,
                invalidConfig.Quality
            );
        }
    }
}