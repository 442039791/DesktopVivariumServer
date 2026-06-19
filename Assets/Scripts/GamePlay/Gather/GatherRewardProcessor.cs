using System.Collections.Generic;
using UnityEngine;
using static Gather;

/// <summary>
/// 采集奖励处理器 - 负责处理采集系统的奖励分发
/// 从PlayerManager中拆分出来,符合单一职责原则
/// 使用IWarehouseService接口进行依赖注入
/// </summary>
public class GatherRewardProcessor
{
    private static GatherRewardProcessor _instance;
    public static GatherRewardProcessor Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GatherRewardProcessor();
            }
            return _instance;
        }
    }

    private IWarehouseService _warehouseService;

    private GatherRewardProcessor()
    {
        // 从ServiceLocator获取仓库服务
        _warehouseService = ServiceLocator.Get<IWarehouseService>();
    }

    /// <summary>
    /// 处理采集奖励并添加到仓库
    /// </summary>
    public void ProcessGatherRewards(List<Reward> rewards)
    {
        if (rewards == null || rewards.Count == 0)
        {
            Debug.LogWarning("[GatherRewardProcessor] 奖励列表为空");
            return;
        }

        // 分类奖励
        var categorizedRewards = CategorizeRewards(rewards);

        // 处理各类奖励
        ProcessAnimalRewards(categorizedRewards.Animals);
        ProcessPlantRewards(categorizedRewards.Plants);
        ProcessCubeRewards(categorizedRewards.Cubes);
        ProcessDecorationRewards(categorizedRewards.Decorations);
        ProcessBioTankRewards(categorizedRewards.BioTanks);
        ProcessAdmissionRewards(categorizedRewards.Admissions);
        ProcessFacilityRewards(categorizedRewards.Facilities);

        // 刷新仓库UI
        RefreshWarehouseUI();

        // 触发特殊奖励事件
        if (categorizedRewards.Admissions.Count > 0 ||
            categorizedRewards.Facilities.Count > 0 ||
            categorizedRewards.BioTanks.Count > 0)
        {
            event_manager.instance.dispatch_event(SysDefine.UI_WindPickingInfo_change, null);
        }

    }

    /// <summary>
    /// 从特定采集点获取奖励
    /// </summary>
    public void ProcessGatherById(string gatherId)
    {
        if (Gather.Instance == null)
        {
            Debug.LogError("[GatherRewardProcessor] Gather系统未初始化");
            return;
        }

        var rewards = Gather.Instance.GetGather();
        ProcessGatherRewards(rewards);
    }

    #region 分类和处理

    /// <summary>
    /// 将奖励分类
    /// </summary>
    private CategorizedRewards CategorizeRewards(List<Reward> rewards)
    {
        var result = new CategorizedRewards();

        foreach (var reward in rewards)
        {
            switch (reward.rewardType)
            {
                case RewardType.Animal:
                    result.Animals.Add(reward);
                    break;
                case RewardType.Plant:
                    result.Plants.Add(reward.rewardID);
                    break;
                case RewardType.Cube:
                    result.Cubes.Add(reward.rewardID);
                    break;
                case RewardType.Decoration:
                    result.Decorations.Add(reward.rewardID);
                    break;
                case RewardType.Tank:
                    result.BioTanks.Add(reward.rewardID);
                    break;
                case RewardType.Admission:
                    result.Admissions.Add(reward.rewardID);
                    break;
                case RewardType.Facilities:
                    result.Facilities.Add(reward.rewardID);
                    break;
                default:
                    Debug.LogWarning($"[GatherRewardProcessor] 未知奖励类型: {reward.rewardType}");
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// 处理动物奖励
    /// </summary>
    private void ProcessAnimalRewards(List<Reward> animals)
    {
        foreach (var reward in animals)
        {
            var animalData = GameConfigDataBase.GetConfigData<AnimalData>(
                int.Parse(reward.rewardID),
                SysDefine.AnimalConfigData);

            if (animalData == null)
            {
                ToastManager.Show($"未找到动物配置: {reward.rewardID}");
                Debug.LogError($"[GatherRewardProcessor] 未找到动物: ID={reward.rewardID}");
                continue;
            }

            var stateData = AnimalStateData.GetStateDataByAnimalData(animalData);
            stateData.Quality = reward.parameter0;

            _warehouseService.SaveAnimal(stateData, false, false);
        }
    }

    /// <summary>
    /// 处理植物奖励
    /// </summary>
    private void ProcessPlantRewards(List<string> plants)
    {
        foreach (var plantId in plants)
        {
            var plantData = GameConfigDataBase.GetConfigData<PlantData>(
                int.Parse(plantId),
                SysDefine.PlantConfigData);

            if (plantData == null)
            {
                ToastManager.Show($"未找到植物配置: {plantId}");
                Debug.LogError($"[GatherRewardProcessor] 未找到植物: ID={plantId}");
                continue;
            }

            var stateData = PlantStateData.GetStateDataByPlantData(plantData);
            _warehouseService.SavePlant(stateData, false);
        }
    }

    /// <summary>
    /// 处理方块奖励
    /// </summary>
    private void ProcessCubeRewards(List<string> cubes)
    {
        foreach (var cubeId in cubes)
        {
            var cubeData = GameConfigDataBase.GetConfigData<CubeData>(
                int.Parse(cubeId),
                SysDefine.CubeConfigData);

            if (cubeData == null)
            {
                Debug.LogError($"[GatherRewardProcessor] 未找到方块: ID={cubeId}");
                continue;
            }

            var stateData = CubeStateData.GetStateDataByCubeData(cubeData);
            _warehouseService.SaveCube(stateData, false);
        }
    }

    /// <summary>
    /// 处理装饰物奖励
    /// </summary>
    private void ProcessDecorationRewards(List<string> decorations)
    {
        foreach (var decoId in decorations)
        {
            var decoData = GameConfigDataBase.GetConfigData<DecorationData>(
                int.Parse(decoId),
                SysDefine.DecorationConfigData);

            if (decoData == null)
            {
                Debug.LogError($"[GatherRewardProcessor] 未找到装饰物: ID={decoId}");
                continue;
            }

            var stateData = DecorationStateData.GetStateDataByDecorationData(decoData);
            _warehouseService.SaveDecoration(stateData, false);
        }
    }

    /// <summary>
    /// 处理生物罐奖励
    /// </summary>
    private void ProcessBioTankRewards(List<string> bioTanks)
    {
        foreach (var tankId in bioTanks)
        {
            var tankData = GameConfigDataBase.GetConfigData<TankData>(int.Parse(tankId));

            if (tankData == null)
            {
                Debug.LogError($"[GatherRewardProcessor] 未找到生物罐配置: ID={tankId}");
                continue;
            }

            // TODO: 实现生物罐奖励处理
        }
    }

    /// <summary>
    /// 处理门票奖励
    /// </summary>
    private void ProcessAdmissionRewards(List<string> admissions)
    {
        // TODO: 实现门票奖励处理
    }

    /// <summary>
    /// 处理设施奖励
    /// </summary>
    private void ProcessFacilityRewards(List<string> facilities)
    {
        // TODO: 实现设施奖励处理
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 刷新仓库UI - 使用事件系统通知
    /// </summary>
    private void RefreshWarehouseUI()
    {
        // 使用事件系统通知UI刷新，而不是直接访问WarehouseManager的UI组件
        event_manager.instance.dispatch_event(SysDefine.UI_Warehouse_change, null);
    }

    #endregion

    #region 内部类

    /// <summary>
    /// 分类后的奖励
    /// </summary>
    private class CategorizedRewards
    {
        public List<Reward> Animals { get; set; } = new List<Reward>();
        public List<string> Plants { get; set; } = new List<string>();
        public List<string> Cubes { get; set; } = new List<string>();
        public List<string> Decorations { get; set; } = new List<string>();
        public List<string> BioTanks { get; set; } = new List<string>();
        public List<string> Admissions { get; set; } = new List<string>();
        public List<string> Facilities { get; set; } = new List<string>();
    }

    #endregion
}
