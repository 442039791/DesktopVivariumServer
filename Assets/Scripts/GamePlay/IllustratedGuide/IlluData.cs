/*****************************************************************
 * --@ FileName: IlluData
 * --@ Description: 图鉴数据类，单个条目的数据
 * --@ Author: paofan25, alivecn2@gmail.com
 * --@ Copyright: Copyright (c) 2025, paofan25
 ******************************************************************/

using AssetBundles;
using UnityEngine;
[System.Serializable]
public class IlluData
{
    public string id;
    public string name;
    public string description;
    public string iconPath; // 保留用于兼容，但不再使用
    public E_IllustratedGuideCategory category;
    public string unlockHint;
    public bool isUnlocked;
    public int collectCount; //收集次数
    public Sprite icon;
    public Sprite bg;

    // 直接从配置数据获取图标
    public Sprite GetIcon()
    {
        if (!isUnlocked)
        {
            return CoreSetting.Instance.UI_UnKnow_sprite;
        }

        // 如果已经缓存了图标，直接返回
        if (icon != null)
        {
            return icon;
        }

        // 根据分类从对应的配置数据获取图标
        return GetIconFromConfigData();
    }

    // 根据分类从配置数据获取图标
    private Sprite GetIconFromConfigData()
    {
        try
        {
            switch (category)
            {
                case E_IllustratedGuideCategory.Fish:
                    var fishData = GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(id), SysDefine.AnimalConfigData);
                    return fishData?.GetIconSprite(ObjectQuality.White, true);

                case E_IllustratedGuideCategory.Plant:
                    var plantData = GameConfigDataBase.GetConfigData<PlantData>(int.Parse(id), SysDefine.PlantConfigData);
                    return plantData?.GetIconSprite();

                case E_IllustratedGuideCategory.Decoration:
                    var decorationData = GameConfigDataBase.GetConfigData<DecorationData>(int.Parse(id), SysDefine.DecorationConfigData);
                    return decorationData?.GetIconSprite();

                case E_IllustratedGuideCategory.Cube:
                    var cubeData = GameConfigDataBase.GetConfigData<CubeData>(int.Parse(id), SysDefine.CubeConfigData);
                    return cubeData?.GetIconSprite();

                case E_IllustratedGuideCategory.Facilities:
                    var facilitiesData = GameConfigDataBase.GetConfigData<FacilitiesData>(int.Parse(id), SysDefine.FacilitiesConfigData);
                    return facilitiesData?.GetIconSprite();

                case E_IllustratedGuideCategory.Tank:
                    var tankData = GameConfigDataBase.GetConfigData<TankData>(int.Parse(id), SysDefine.TankConfigData);
                    return tankData?.GetIconSprite();

                default:
                    Debug.LogWarning($"[IlluData] 未知分类: {category}");
                    return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[IlluData] 获取图标失败: ID={id}, Category={category}, Error={e.Message}");
            return null;
        }
    }

    // 获取背景图标
    public Sprite GetBgIcon()
    {
        if (bg != null)
        {
            return bg;
        }

        // 对于鱼类，可能需要根据品质设置不同背景
        if (category == E_IllustratedGuideCategory.Fish)
        {
            return CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White);
        }

        // 其他类型使用默认白色背景
        return CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White);
    }

    // 缓存图标和背景（可选，用于性能优化）
    public void CacheSprites()
    {
        if (icon == null)
        {
            icon = GetIconFromConfigData();
        }
        if (bg == null)
        {
            bg = GetBgIcon();
        }
    }

    // 兼容旧的异步加载方法，但现在不再使用
    [System.Obsolete("不再使用异步加载，请直接使用GetIcon()")]
    public BaseAssetAsyncLoader LoadIconAsync()
    {
        return null;
    }

    // 兼容旧的设置缓存方法
    [System.Obsolete("不再需要手动设置缓存，请使用CacheSprites()")]
    public void SetCachedIcon(Sprite sprite)
    {
        icon = sprite;
    }
}

public enum E_IllustratedGuideCategory
{
    All, //全部
    Fish,
    Plant,
    Decoration,
    Cube,
    Facilities,
    Tank, //鱼缸
}