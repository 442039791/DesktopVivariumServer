/*****************************************************************
 * --@ FileName: IGuideMgr
 * --@ Description: 图鉴管理类，读取数据，分发数据
 * --@ Author: paofan25, alivecn2@gmail.com
 * --@ Copyright: Copyright (c) 2025, paofan25
 ******************************************************************/

using System.Collections.Generic;
using Common;
using UnityEngine;
using System.Linq;
public class IlluGuideMgr : MonoSingleton<IlluGuideMgr>
{
    [Header("IllustratedGuideData")]
    public Dictionary<string, IlluData> fishEntries = new();
    public Dictionary<string, IlluData> plantEntries = new();
    public Dictionary<string, IlluData> decorationEntries = new();
    public Dictionary<string, IlluData> cubeEntries = new();
    public Dictionary<string, IlluData> facilityEntries = new();
    [Header("StatisticsData")]
    public int totalFishCount = 0;
    public int unlockedFishCount = 0;
    string fishCompletionString = "";
    public override void Init()
    {
        base.Init();
        event_manager.instance.add_event_listener("Gather_OnFishCaught", (n, o) =>
        {
            if (o is string id) UnlockEntry(id, E_IllustratedGuideCategory.Fish);
        });
        event_manager.instance.add_event_listener("Gather_OnPlantHarvested", (n, o) =>
        {
            if (o is string id) UnlockEntry(id, E_IllustratedGuideCategory.Plant);
        });
        event_manager.instance.add_event_listener("Shop_OnDecorationPurchased", (n, o) =>
        {
            if (o is string id) UnlockEntry(id, E_IllustratedGuideCategory.Decoration);
        });
        event_manager.instance.add_event_listener("Craft_OnCubeObtained", (n, o) =>
        {
            if (o is string id) UnlockEntry(id, E_IllustratedGuideCategory.Cube);
        });
        TryInitializeData();
    }
    public void UnlockEntry(string itemId, E_IllustratedGuideCategory category)
    {
        var entry = GetEntry(itemId, category);
        if (entry != null && !entry.isUnlocked)
        {
            entry.isUnlocked = true;
            entry.collectCount++;

            // 对于Cube和Decoration,同步到Gather(单一数据源)
            if (category == E_IllustratedGuideCategory.Cube)
            {
                if (Gather.Instance != null && Gather.Instance.CubeUnlockDic.ContainsKey(itemId))
                {
                    Gather.Instance.CubeUnlockDic[itemId] = true;
                }
            }
            else if (category == E_IllustratedGuideCategory.Decoration)
            {
                if (Gather.Instance != null && Gather.Instance.DecorationUnlockDic.ContainsKey(itemId))
                {
                    Gather.Instance.DecorationUnlockDic[itemId] = true;
                }
            }

            // 触发解锁事件
            string eventName = $"IllustratedGuide_Unlock_{category}";
            event_manager.instance.dispatch_UIevent(eventName, itemId);

            // 更新统计数据
            UpdateStatistics();
        }
    }

    /// <summary>
    /// 获取指定分类的所有条目
    /// </summary>
    public List<IlluData> GetEntriesByCategory(E_IllustratedGuideCategory category)
    {
        switch (category)
        {
            case E_IllustratedGuideCategory.Fish:
                return fishEntries.Values.ToList();
            case E_IllustratedGuideCategory.Plant:
                return plantEntries.Values.ToList();
            case E_IllustratedGuideCategory.Decoration:
                return decorationEntries.Values.ToList();
            case E_IllustratedGuideCategory.Cube:
                return cubeEntries.Values.ToList();
            case E_IllustratedGuideCategory.Facilities:
                return facilityEntries.Values.ToList();
            default:
                return new List<IlluData>();
        }
    }
    /// <summary>
    /// 获取指定条目
    /// </summary>
    private IlluData GetEntry(string itemId, E_IllustratedGuideCategory category)
    {
        return category switch
        {
            E_IllustratedGuideCategory.Fish => fishEntries.GetValueOrDefault(itemId),
            E_IllustratedGuideCategory.Plant => plantEntries.GetValueOrDefault(itemId),
            E_IllustratedGuideCategory.Decoration => decorationEntries.GetValueOrDefault(itemId),
            E_IllustratedGuideCategory.Cube => cubeEntries.GetValueOrDefault(itemId),
            _ => null
        };
    }
    /// <summary>
    /// 尝试初始化数据，返回是否成功
    /// </summary>
    private bool TryInitializeData()
    {
        try
        {
            fishEntries.Clear();
            plantEntries.Clear();
            decorationEntries.Clear();
            cubeEntries.Clear();
            facilityEntries.Clear();

            InitializeGuideData();
            InitializePlantData();
            InitializeDecorationData();
            InitializeCubeData();

            int totalLoaded = fishEntries.Count + plantEntries.Count + decorationEntries.Count + cubeEntries.Count;
            return totalLoaded > 0;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 更新统计数据
    /// </summary>
    private void UpdateStatistics()
    {
        totalFishCount = fishEntries.Count;
        unlockedFishCount = fishEntries.Values.Count(e => e.isUnlocked);

        if (totalFishCount > 0)
        {
            fishCompletionString = $"{unlockedFishCount} /{totalFishCount}";
        }
    }

    #region 价格,数据初始化,提示

    private string GetFishUnlockHint(AnimalData fishData)
    {
        return $"通过{fishData.name}卡池获得";
    }

    private void InitializeGuideData()
    {
        var fishDataList = GameConfigDataBase.GetConfigDatasByDic<AnimalData>(SysDefine.AnimalConfigData);

        if (fishDataList == null || fishDataList.Count == 0)
        {
            return;
        }

        foreach (var fishData in fishDataList.Values)
        {
            if (fishData == null) continue;

            try
            {
                var entry = new IlluData
                {
                    id = fishData.ID,
                    name = fishData.name,
                    description = fishData.Describe,
                    iconPath = "",
                    category = E_IllustratedGuideCategory.Fish,
                    unlockHint = GetFishUnlockHint(fishData),
                    isUnlocked = false,
                    icon = fishData.GetIconSprite(ObjectQuality.White, true),
                    bg = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White)
                };

                fishEntries.Add(fishData.ID, entry);
            }
            catch (System.Exception)
            {
            }
        }
    }

    /// <summary>
    /// 初始化植物图鉴数据
    /// </summary>
    private void InitializePlantData()
    {
        try
        {
            var plantDataList = GameConfigDataBase.GetConfigDatasByDic<PlantData>(SysDefine.PlantConfigData);

            if (plantDataList == null || plantDataList.Count == 0)
            {
                return;
            }

            foreach (var plantData in plantDataList.Values)
            {
                if (plantData == null) continue;

                try
                {
                    var entry = new IlluData
                    {
                        id = plantData.ID,
                        name = plantData.name,
                        description = plantData.Describe,
                        iconPath = "",
                        category = E_IllustratedGuideCategory.Plant,
                        unlockHint = GetPlantUnlockHint(plantData),
                        isUnlocked = false,
                        icon = plantData.GetIconSprite(),
                        bg = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White)
                    };

                    plantEntries.Add(plantData.ID, entry);
                }
                catch (System.Exception)
                {
                }
            }
        }
        catch (System.Exception)
        {
        }
    }

    /// <summary>
    /// 初始化装饰物图鉴数据
    /// </summary>
    private void InitializeDecorationData()
    {
        try
        {
            var decorationDataList = GameConfigDataBase.GetConfigDatasByDic<DecorationData>(SysDefine.DecorationConfigData);

            if (decorationDataList == null || decorationDataList.Count == 0)
            {
                return;
            }

            foreach (var decorationData in decorationDataList.Values)
            {
                if (decorationData == null) continue;

                try
                {
                    var entry = new IlluData
                    {
                        id = decorationData.ID,
                        name = decorationData.name,
                        description = decorationData.UniqueType,
                        iconPath = "",
                        category = E_IllustratedGuideCategory.Decoration,
                        unlockHint = GetDecorationUnlockHint(decorationData),
                        isUnlocked = false,
                        icon = decorationData.GetIconSprite(),
                        bg = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White)
                    };
                    decorationEntries.Add(decorationData.ID, entry);
                }
                catch (System.Exception)
                {
                }
            }
        }
        catch (System.Exception)
        {
        }
    }

    /// <summary>
    /// 初始化方块图鉴数据
    /// </summary>
    private void InitializeCubeData()
    {
        try
        {
            var cubeDataList = GameConfigDataBase.GetConfigDatasByDic<CubeData>(SysDefine.CubeConfigData);

            if (cubeDataList == null || cubeDataList.Count == 0)
            {
                return;
            }

            foreach (var cubeData in cubeDataList.Values)
            {
                if (cubeData == null) continue;

                try
                {
                    var entry = new IlluData
                    {
                        id = cubeData.ID,
                        name = cubeData.name,
                        description = $"方块材料，价格: {cubeData.Price}",
                        iconPath = "",
                        category = E_IllustratedGuideCategory.Cube,
                        unlockHint = GetCubeUnlockHint(cubeData),
                        isUnlocked = false,
                        icon = cubeData.GetIconSprite(),
                        bg = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White)
                    };
                    cubeEntries.Add(cubeData.ID, entry);
                }
                catch (System.Exception)
                {
                    // 忽略错误条目
                }
            }
        }
        catch (System.Exception)
        {
            // 初始化失败
        }
    }

    private ObjectQuality GetPlantRarity(PlantData plantData)
    {
        // 根据植物价格决定稀有度
        if (plantData.Price >= 1000) return ObjectQuality.Gold;
        if (plantData.Price >= 500) return ObjectQuality.Purple;
        if (plantData.Price >= 200) return ObjectQuality.Blue;
        return ObjectQuality.White;
    }

    private string GetPlantUnlockHint(PlantData plantData)
    {
        return $"通过种植或购买{plantData.name}获得";
    }

    private ObjectQuality GetDecorationRarity(DecorationData decorationData)
    {
        // 根据装饰物价格决定稀有度
        if (decorationData.Price >= 2000) return ObjectQuality.Gold;
        if (decorationData.Price >= 1000) return ObjectQuality.Purple;
        if (decorationData.Price >= 500) return ObjectQuality.Blue;
        return ObjectQuality.White;
    }

    private string GetDecorationUnlockHint(DecorationData decorationData)
    {
        return $"通过商店购买{decorationData.name}获得";
    }

    private ObjectQuality GetCubeRarity(CubeData cubeData)
    {
        // 根据方块价格决定稀有度
        if (cubeData.Price >= 100) return ObjectQuality.Purple;
        if (cubeData.Price >= 50) return ObjectQuality.Blue;
        if (cubeData.Price >= 10) return ObjectQuality.White;
        return ObjectQuality.White;
    }

    private string GetCubeUnlockHint(CubeData cubeData)
    {
        return $"通过挖掘或合成{cubeData.name}获得";
    }


    #endregion

    /// <summary>
    /// 解锁所有图鉴条目
    /// </summary>
    public void UnlockAllEntries()
    {
        int totalUnlocked = 0;
        int fishUnlocked = 0;
        int plantUnlocked = 0;
        int decorationUnlocked = 0;
        int cubeUnlocked = 0;
        int facilityUnlocked = 0;

        var warehouseService = ServiceLocator.Get<IWarehouseService>();

        // 解锁所有鱼类条目
        foreach (var entry in fishEntries.Values)
        {
            if (!entry.isUnlocked)
            {
                entry.isUnlocked = true;
                entry.collectCount++;
                fishUnlocked++;
            }
        }

        // 解锁所有植物条目
        foreach (var entry in plantEntries.Values)
        {
            if (!entry.isUnlocked)
            {
                entry.isUnlocked = true;
                entry.collectCount++;
                plantUnlocked++;
            }
        }

        // 解锁所有装饰条目,并同步到仓库
        foreach (var entry in decorationEntries.Values)
        {
            if (!entry.isUnlocked)
            {
                entry.isUnlocked = true;
                entry.collectCount++;
                decorationUnlocked++;

                // 同步到仓库
                if (warehouseService != null)
                {
                    var decoList = warehouseService.GetWarehouseBaseDatas(ItemType.Decoration);
                    bool existsInWarehouse = decoList.Any(item =>
                    {
                        var decoData = item as WarehouseDecorationStateData;
                        return decoData != null && decoData.stateData.Type == entry.id;
                    });

                    if (!existsInWarehouse)
                    {
                        var decoStateData = new DecorationStateData { Type = entry.id };
                        warehouseService.SaveDecoration(decoStateData, false);
                    }
                }
            }
        }

        // 解锁所有方块条目,并同步到仓库
        foreach (var entry in cubeEntries.Values)
        {
            if (!entry.isUnlocked)
            {
                entry.isUnlocked = true;
                entry.collectCount++;
                cubeUnlocked++;

                // 同步到仓库
                if (warehouseService != null)
                {
                    var cubeList = warehouseService.GetWarehouseBaseDatas(ItemType.Cube);
                    bool existsInWarehouse = cubeList.Any(item =>
                    {
                        var cubeData = item as WarehouseCubeStateData;
                        return cubeData != null && cubeData.stateData.Type == entry.id;
                    });

                    if (!existsInWarehouse)
                    {
                        var cubeStateData = new CubeStateData { Type = entry.id };
                        warehouseService.SaveCube(cubeStateData, false);
                    }
                }
            }
        }

        // 解锁所有设施条目
        foreach (var entry in facilityEntries.Values)
        {
            if (!entry.isUnlocked)
            {
                entry.isUnlocked = true;
                entry.collectCount++;
                facilityUnlocked++;
            }
        }

        totalUnlocked = fishUnlocked + plantUnlocked + decorationUnlocked + cubeUnlocked + facilityUnlocked;

        // 更新统计数据
        UpdateStatistics();

        // 触发全部解锁事件
        event_manager.instance.dispatch_UIevent("IllustratedGuide_UnlockAll", totalUnlocked);
    }

    /// <summary>
    /// 解锁指定分类的所有图鉴条目
    /// </summary>
    /// <param name="category">要解锁的分类</param>
    public void UnlockAllEntriesByCategory(E_IllustratedGuideCategory category)
    {
        int unlockedCount = 0;
        Dictionary<string, IlluData> targetEntries = null;

        // 根据分类选择目标条目字典
        switch (category)
        {
            case E_IllustratedGuideCategory.Fish:
                targetEntries = fishEntries;
                break;
            case E_IllustratedGuideCategory.Plant:
                targetEntries = plantEntries;
                break;
            case E_IllustratedGuideCategory.Decoration:
                targetEntries = decorationEntries;
                break;
            case E_IllustratedGuideCategory.Cube:
                targetEntries = cubeEntries;
                break;
            case E_IllustratedGuideCategory.Facilities:
                targetEntries = facilityEntries;
                break;
            default:
                return;
        }

        if (targetEntries == null || targetEntries.Count == 0)
        {
            return;
        }

        // 解锁该分类的所有条目
        foreach (var entry in targetEntries.Values)
        {
            if (!entry.isUnlocked)
            {
                entry.isUnlocked = true;
                entry.collectCount++;
                unlockedCount++;
            }
        }

        // 更新统计数据
        UpdateStatistics();

        // 触发分类解锁事件
        string eventName = $"IllustratedGuide_UnlockAll_{category}";
        event_manager.instance.dispatch_UIevent(eventName, unlockedCount);
    }
}
