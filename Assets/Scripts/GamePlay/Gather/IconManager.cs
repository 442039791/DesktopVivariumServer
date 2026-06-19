using UnityEngine;
using System.Collections.Generic;
using Common;
using System;

/// <summary>
/// Icon显示数据结构，用于存储物品图标的显示信息
/// </summary>
[Serializable]
public struct IconDisplayData
{
    public Sprite Icon;           // 物品图标
    public Sprite Background;     // 背景图
    public string Name;           // 名称
    public string NameLangId;     // 名称多语言ID
    public string Description;    // 描述
    public bool IsUnlocked;       // 是否已解锁

    public IconDisplayData(Sprite icon, Sprite bg, string name, string nameLangId, string desc, bool unlocked)
    {
        Icon = icon;
        Background = bg;
        Name = name;
        NameLangId = nameLangId;
        Description = desc;
        IsUnlocked = unlocked;
    }

    /// <summary>
    /// 创建未解锁状态的显示数据
    /// </summary>
    public static IconDisplayData CreateLocked(Sprite bg)
    {
        return new IconDisplayData(CoreSetting.Instance.UI_UnKnow_sprite, bg, "", "", "", false);
    }
}

public partial class Gather : MonoSingleton<Gather>
{
    // 奖励类型与解锁字典的映射
    private Dictionary<RewardType, Dictionary<string, bool>> _unlockDicMap;

    private Dictionary<RewardType, Dictionary<string, bool>> GetUnlockDicMap()
    {
        if (_unlockDicMap == null)
        {
            _unlockDicMap = new Dictionary<RewardType, Dictionary<string, bool>>
            {
                { RewardType.Cube, CubeUnlockDic },
                { RewardType.Decoration, DecorationUnlockDic },
                { RewardType.Plant, PlantUnlockDic },
                { RewardType.Admission, AdmissionUnlockDic },
                { RewardType.Facilities, FacilitiesUnlockDic },
                { RewardType.Tank, TankUnlockDic }
            };
        }
        return _unlockDicMap;
    }

    private void GenerateSpritesFromCache(Dictionary<RewardType, Dictionary<string, IconDisplayData>> cache)
    {
        var unlockMap = GetUnlockDicMap();

        foreach (var rewardGroup in cache)
        {
            if (rewardGroup.Key == RewardType.Animal)
            {
                ProcessFishRewards(rewardGroup.Value);
            }
            else if (unlockMap.TryGetValue(rewardGroup.Key, out var unlockDic))
            {
                ProcessRewardType(rewardGroup.Value, unlockDic);
            }
            else
            {
                ProcessDefaultRewards(rewardGroup.Value);
            }
        }
    }
    /// <summary>
    /// 抽卡结果中icon的展示效果
    /// </summary>
    private void InitRewardGameobject(int index, GameObject @object)
    {
        if (@object.TryGetComponent<UI_Gather_cangetprefab>(out var prefab))
        {
            var data = AllRewardSprites[index];
            prefab.Init(data.Icon, data.Background, data.Name, data.NameLangId, data.Description, data.IsUnlocked);
        }
    }

    /// <summary>
    /// 抽卡界面中icon的展示效果
    /// </summary>
    private void InitcanGetRewardObject(int index, GameObject @object)
    {
        if (@object.TryGetComponent<UI_Gather_cangetprefab>(out var prefab))
        {
            var data = AllGatherSprites[index];
            prefab.Init(data.Icon, data.Background, data.Name, data.NameLangId, data.Description, data.IsUnlocked);
        }
    }
    /// <summary>
    /// 将每个道具的奖励样式存储起来
    /// </summary>
    private Dictionary<RewardType, Dictionary<string, IconDisplayData>> BuildSpriteCache()
    {
        var spriteDic = new Dictionary<RewardType, Dictionary<string, IconDisplayData>>();

        // 检查 GatherDataDic 是否包含当前 GatherID
        if (!GatherDataDic.TryGetValue(GatherID, out var gatherData))
        {
            Debug.LogError($"[BuildSpriteCache] GatherDataDic 中没有找到 GatherID={GatherID}, Count={GatherDataDic.Count}");
            // 返回空字典，避免崩溃
            foreach (RewardType type in Enum.GetValues(typeof(RewardType)))
            {
                spriteDic[type] = new Dictionary<string, IconDisplayData>();
            }
            return spriteDic;
        }

        spriteDic[RewardType.Animal] = BuildFishSprites(gatherData);
        spriteDic[RewardType.Plant] = BuildSprites(GetPlantSortList(), SysDefine.PlantConfigData, "16",
            (PlantData data) => (data.GetIconSprite(), data.name, data.Describe));
        spriteDic[RewardType.Decoration] = BuildSprites(gatherData.DecorationIDList, SysDefine.DecorationConfigData, "68",
            (DecorationData data) => (data.GetIconSprite(), data.name, data.Describe));
        spriteDic[RewardType.Cube] = BuildSprites(gatherData.CubeIDList, SysDefine.CubeConfigData, "67",
            (CubeData data) => (data.GetIconSprite(), data.name, data.Describe));
        spriteDic[RewardType.Facilities] = BuildSprites(gatherData.FacilitiesIDList, SysDefine.FacilitiesConfigData, "97",
            (FacilitiesData data) => (data.GetIconSprite(), data.Name, data.Describe));
        spriteDic[RewardType.Tank] = BuildSprites(gatherData.TankIDList, SysDefine.TankConfigData, "98",
            (TankData data) => (data.GetIconSprite(), data.name, data.Describe));
        spriteDic[RewardType.Admission] = BuildSprites(gatherData.AdmissionIDList, SysDefine.AdmissionConfigData, "96",
            (AdmissionData data) => (data.GetIconSprite(), data.Name, data.Describe));
        return spriteDic;
    }

    /// <summary>
    /// 通用的精灵构建方法
    /// </summary>
    private Dictionary<string, IconDisplayData> BuildSprites<T>(
        List<string> idList,
        string configPath,
        string nameLangId,
        Func<T, (Sprite icon, string name, string desc)> dataExtractor) where T : GameConfigDataBase
    {
        var dic = new Dictionary<string, IconDisplayData>();
        var defaultBg = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White);

        foreach (string id in idList)
        {
            var data = GameConfigDataBase.GetConfigData<T>(int.Parse(id), configPath);
            if (data == null) continue;

            var (icon, name, desc) = dataExtractor(data);
            dic.Add(id, new IconDisplayData(icon, defaultBg, name, nameLangId, desc, true));
        }
        return dic;
    }


    #region Fish Specific Handling
    private static readonly ObjectQuality[] AllQualities = { ObjectQuality.White, ObjectQuality.Blue, ObjectQuality.Purple, ObjectQuality.Gold };
    private const string WhiteSuffix = "_White";

    private void ProcessFishRewards(Dictionary<string, IconDisplayData> fishRewards)
    {
        foreach (var fishEntry in fishRewards)
        {
            if (!fishEntry.Key.EndsWith(WhiteSuffix)) continue;

            string baseFishID = fishEntry.Key.Substring(0, fishEntry.Key.Length - WhiteSuffix.Length);
            if (!FishUnlockDic.TryGetValue(baseFishID, out int unlockLevel))
            {
                Debug.LogError($"FishUnlockDic not contain key: {baseFishID}");
                continue;
            }

            AllGatherSprites.Add(unlockLevel >= 0
                ? fishEntry.Value
                : IconDisplayData.CreateLocked(fishEntry.Value.Background));
        }
    }

    private Dictionary<string, IconDisplayData> BuildFishSprites(GatherData gatherData)
    {
        var dic = new Dictionary<string, IconDisplayData>();
        var fishIDList = GetFishSortList();

        foreach (string fishID in fishIDList)
        {
            var fishData = GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(fishID), SysDefine.AnimalConfigData);
            if (fishData == null) continue;

            foreach (var quality in AllQualities)
            {
                var icon = fishData.GetIconSprite(quality, true);
                var bg = CoreSetting.Instance.GetQualityBgSprite(quality);
                dic.Add($"{fishID}_{quality}", new IconDisplayData(icon, bg, fishData.name, "15", fishData.Describe, true));
            }
        }
        return dic;
    }
    #endregion

    #region Generic Reward Processing
    private void ProcessRewardType(Dictionary<string, IconDisplayData> rewards, Dictionary<string, bool> unlockDic)
    {
        foreach (var entry in rewards)
        {
            if (!unlockDic.TryGetValue(entry.Key, out bool isUnlocked)) continue;

            AllGatherSprites.Add(isUnlocked
                ? entry.Value
                : IconDisplayData.CreateLocked(entry.Value.Background));
        }
    }

    private void ProcessDefaultRewards(Dictionary<string, IconDisplayData> rewards)
    {
        foreach (var entry in rewards)
        {
            AllGatherSprites.Add(entry.Value);
        }
    }
    #endregion

    public List<IconDisplayData> GetAllRewardSprite()
    {
        AllRewardSprites.Clear();

        // 检查缓存是否存在，如果不存在则构建
        if (!AllGatherSpritesCache.TryGetValue(GatherID, out var cachedData))
        {
            Debug.LogWarning($"[GetAllRewardSprite] 缓存中没有 GatherID={GatherID}，尝试构建缓存");
            cachedData = BuildSpriteCache();
            AllGatherSpritesCache.Add(GatherID, cachedData);
        }

        foreach (var item in rewardList)
        {
            string key = item.rewardType == RewardType.Animal
                ? $"{item.rewardID}_{AnimalData.GetObjectQuality(item.parameter0)}"
                : item.rewardID;

            if (cachedData[item.rewardType].TryGetValue(key, out var displayData))
            {
                AllRewardSprites.Add(displayData);
            }
        }
        return AllRewardSprites;
    }
}
