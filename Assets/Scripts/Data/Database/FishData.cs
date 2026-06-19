using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimalData : GameConfigDataBase
{
    /// <summary>
    /// id
    /// </summary>
    public string ID;
    /// <summary>
    /// 名称
    /// </summary>
    public string name;
    /// <summary>
    /// 描述
    /// </summary>
    public string Describe;
    /// <summary>
    /// 排序优先级
    /// </summary>
    public int Sort;
    /// <summary>
    /// 是否在图鉴中展示：0不展示、1展示
    /// </summary>
    public int Display;
    /// <summary>
    /// 美术资源
    /// </summary>
    // 成年鱼图标
    public string adultIcon_white;
    public string adultIcon_blue;
    public string adultIcon_purple;
    public string adultIcon_gold;
    // 幼年鱼图标
    public string juvenileIcon_white;
    public string juvenileIcon_blue;
    public string juvenileIcon_purple;
    public string juvenileIcon_gold;
    // 成年鱼模型
    public string adultResources_white;
    public string adultResources_blue;
    public string adultResources_purple;
    public string adultResources_gold;
    // 幼年鱼模型
    public string juvenileResources_white;
    public string juvenileResources_blue;
    public string juvenileResources_purple;
    public string juvenileResources_gold;
    /// <summary>
    /// 每只鱼需要的空间
    /// </summary>
    public float activitySpace;
    /// <summary>
    /// 食物类型
    /// </summary>
    public FoodType foodType;
    /// <summary>
    /// 食物参数,当目标是指定植物或者动物时，这里配置对方的ID
    /// </summary>
    public string foodParameter;

    /// <summary>
    /// 成年鱼购买价格
    /// </summary>
    public int adultPurchasePrice;
    /// <summary>
    /// 幼年鱼购买价格
    /// </summary>
    public int juvenilePurchasePrice;
    /// <summary>
    /// 每只成年每间隔多少分钟产下一个幼崽
    /// </summary>
    public int reproductionTime;
    /// <summary>
    /// 成年时间
    /// </summary>
    public int adultTime;
    /// <summary>
    /// 成年鱼每分钟的氧气消耗量
    /// </summary>
    public float adultO2Cost;
    /// <summary>
    /// 幼年鱼每分钟的氧气消耗量
    /// </summary>
    public float juvenileO2Cost;
    /// <summary>
    /// 成年死鱼每分钟的氧气消耗量
    /// </summary>
    public float adultDeathO2Cost;
    /// <summary>
    /// 幼年死鱼每分钟的氧气消耗量
    /// </summary>
    public float juvenileDeathO2Cost;
    /// <summary>
    /// 氧气的最低含量百分比
    /// </summary>
    public float o2MinProportion;
    /// <summary>
    /// 成年鱼因为氧气不足死亡时，最大分钟死亡数
    /// </summary>
    public int adultO2Death;
    /// <summary>
    /// 幼年鱼因为氧气不足死亡时，最大分钟死亡数
    /// </summary>
    public int juvenileO2Death;

    /// <summary>
    /// 成年鱼每分钟的二氧化碳产出
    /// </summary>
    public float adultCO2Produce;
    /// <summary>
    /// 幼年鱼每分钟的二氧化碳产出
    /// </summary>
    public float juvenileCO2Produce;
    /// <summary>
    /// 成年死鱼每分钟的二氧化碳产出
    /// </summary>
    public float adultDeathCO2Produce;
    /// <summary>
    /// 幼年死鱼每分钟的二氧化碳产出
    /// </summary>
    public float juvenileDeathCO2Produce;
    /// <summary>
    /// 二氧化碳的最大含量百分比
    /// </summary>
    public float co2MaxProportion;
    /// <summary>
    /// 成年鱼因为二氧化碳过量死亡时，最大分钟死亡数
    /// </summary>
    public int adultCO2Death;
    /// <summary>
    /// 幼年鱼因为二氧化碳过量死亡时，最大分钟死亡数
    /// </summary>
    public int juvenileCO2Death;

    /// <summary>
    /// 成年鱼每分钟的垃圾产出
    /// </summary>
    public float adultWasteProduce;
    /// <summary>
    /// 幼年鱼每分钟的垃圾产出
    /// </summary>
    public float juvenileWasteProduce;
    /// <summary>
    /// 成年死鱼每分钟的垃圾产出
    /// </summary>
    public float adultDeathWasteProduce;
    /// <summary>
    /// 幼年死鱼每分钟的垃圾产出
    /// </summary>
    public float juvenileDeathWasteProduce;
    /// <summary>
    /// 垃圾的最大含量百分比
    /// </summary>
    public float wasteMaxProportion;
    /// <summary>
    /// 成年鱼因为垃圾过量死亡时，最大分钟死亡数
    /// </summary>
    public int adultWasteDeath;
    /// <summary>
    /// 幼年鱼因为垃圾过量死亡时，最大分钟死亡数
    /// </summary>
    public int juvenileWasteDeath;

    /// <summary>
    /// 每只鱼可以提供的食物存储数量
    /// </summary>
    public float foodReserve;
    /// <summary>
    /// 成年鱼每分钟食物消耗
    /// </summary>
    public float adultFoodCost;
    /// <summary>
    /// 幼年鱼每分钟食物消耗
    /// </summary>
    public float juvenileFoodCost;
    /// <summary>
    /// 成年鱼因为食物不足死亡时，最大分钟死亡数
    /// </summary>
    public float adultFoodDeath;
    /// <summary>
    /// 幼年鱼因为食物不足死亡时，最大分钟死亡数
    /// </summary>
    public float juvenileFoodDeath;

    /// <summary>
    /// 每分钟提供的食物数量
    /// </summary>
    public float FoodNum;
    public static ObjectQuality GetObjectQuality(float Quality)
    {
        ObjectQuality quality = ObjectQuality.White;
        if (Quality <= 0)
        {
            quality = (ObjectQuality)0;
            return quality;
        }

        int max = (int)Enum.GetValues(typeof(ObjectQuality)).Cast<ObjectQuality>().Max();
        if (Quality >= max)
        {
            quality = (ObjectQuality)max;
            return quality;
        }
        return (ObjectQuality)Quality;
    }
    public int GetPrice(ObjectQuality quality,bool adult)
    {
        float Magnification = 1f;
        int price = adult? adultPurchasePrice: juvenilePurchasePrice;
        switch (quality)
        {
            case ObjectQuality.White:
                Magnification = SysDefine.Price_Magnification_White;
                break;
            case ObjectQuality.Blue:
                Magnification = SysDefine.Price_Magnification_Blue;
                break;
            case ObjectQuality.Purple:
                Magnification = SysDefine.Price_Magnification_Purple;
                break;
            case ObjectQuality.Gold:
                Magnification = SysDefine.Price_Magnification_Gold;
                break;
        }
        return (int)(price * Magnification);

    }
    protected override string getFilePath ()
	{
		return "FishData.json";
	}
    public Sprite GetIconSprite(ObjectQuality quality,bool isAdult)
    {
        string Iconpath = "";
        if (isAdult)
        {
            switch (quality)
            {
                case ObjectQuality.White:
                    Iconpath = adultIcon_white;
                    break;
                case ObjectQuality.Blue:
                    Iconpath = adultIcon_blue;
                    break;
                case ObjectQuality.Purple:
                    Iconpath = adultIcon_purple;
                    break;
                case ObjectQuality.Gold:
                    Iconpath = adultIcon_gold;
                    break;
            }
        }
        else
        {
            switch (quality)
            {
                case ObjectQuality.White:
                    Iconpath = juvenileIcon_white;
                    break;
                case ObjectQuality.Blue:
                    Iconpath = juvenileIcon_blue;
                    break;
                case ObjectQuality.Purple:
                    Iconpath = juvenileIcon_purple;
                    break;
                case ObjectQuality.Gold:
                    Iconpath = juvenileIcon_gold;
                    break;
            }
        }
        if (!Iconpath.Contains(".png"))
        {
            Iconpath += ".png";
        }
        string path = SysDefine.ImageAssetbundle + SysDefine.AnimalSpritePath + SysDefine.AssetbundleIconSpritePath + Iconpath;

        // ResMgr.GetAssetCache 内部已自动处理模组资源覆盖
        Sprite sprite = ResMgr.Instance.GetAssetCache(path, null) as Sprite;
        return sprite;
    }
    public Sprite GetIconSprte()
    {
        // 默认返回成年白色品质的图标
        return GetIconSprite(ObjectQuality.White, true);
    }
    public Sprite GetAdultSprite()
    {

        return null;

    }
    public Sprite GetJuvenileSprite()
    {
        return null;
    }
    public Sprite GetSprite(bool isAdult, ObjectQuality quality)
    {
        string spritepath = SysDefine.ImageAssetbundle + SysDefine.AnimalSpritePath + SysDefine.AssetbundleIconSpritePath;
        string iconPath = "";
        if (isAdult)
        {
            switch (quality)
            {
                case ObjectQuality.White:
                    iconPath = adultIcon_white;
                    break;
                case ObjectQuality.Blue:
                    iconPath = adultIcon_blue;
                    break;
                case ObjectQuality.Purple:
                    iconPath = adultIcon_purple;
                    break;
                case ObjectQuality.Gold:
                    iconPath = adultIcon_gold;
                    break;
            }
        }
        else
        {
            switch (quality)
            {
                case ObjectQuality.White:
                    iconPath = juvenileIcon_white;
                    break;
                case ObjectQuality.Blue:
                    iconPath = juvenileIcon_blue;
                    break;
                case ObjectQuality.Purple:
                    iconPath = juvenileIcon_purple;
                    break;
                case ObjectQuality.Gold:
                    iconPath = juvenileIcon_gold;
                    break;
            }
        }
        if(!(iconPath.Contains(".png")))
        {
            iconPath += ".png";
        }
        string fullPath = spritepath + iconPath;

        // ResMgr.GetAssetCache 内部已自动处理模组资源覆盖
        Sprite sprite = ResMgr.Instance.GetAssetCache(fullPath, null) as Sprite;
        return sprite;

    }
    private Sprite GetSprite(List<string> list)
    {
        Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle + SysDefine.AnimalSpritePath + SysDefine.AssetbundleSpritePath + list[Random.Range(0, list.Count)], null) as Sprite;
        return sprite;

    }
}
