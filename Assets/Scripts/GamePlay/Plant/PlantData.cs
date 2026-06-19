using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlantData : GameConfigDataBase
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
    public List<string> foregroundSprite;
    public List<string> midgroundSprite;
    public List<string> backgroundSprite;
    public string PlantModel;
    public string IconSprite;
    /// <summary>
    /// 价格
    /// </summary>
    public int Price;
    /// <summary>
    /// 每分钟提供的食物数量
    /// </summary>
    public float FoodNum;
    /// <summary>
    /// 每分钟消耗的二氧化碳数量
    /// </summary>
    public float CO2Cost;
    /// <summary>
    /// 每分钟产生的氧气数量
    /// </summary>
    public float O2Produce;
    /// <summary>
    /// 每分钟营养消耗的数量
    /// </summary>
    public float NutrientCost;
    /// <summary>
    /// 每分钟消耗的垃圾
    /// </summary>
    public float WasteCost;
    /// <summary>
    /// 垃圾的最大含量百分比
    /// </summary>
    public float WasteMaxProportion;
    /// <summary>
    /// 因为垃圾过量死亡时，最大分钟死亡数
    /// </summary>
    public int WasteDeath;
    /// <summary>
    /// 因为营养不足死亡时，最大分钟死亡数
    /// </summary>
    public int NutrientDeath;
    /// <summary>
    /// 成年时间
    /// </summary>
    public int AdultTime;
    /// <summary>
    /// 死亡后二氧化碳的产出量
    /// </summary>
    public float DeathCO2Produce;
    /// <summary>
    /// 死亡后氧气的消耗量
    /// </summary>
    public float DeathO2Cost;
    /// <summary>
    /// 死亡后垃圾的产出量
    /// </summary>
    public float DeathWasteProduce;
    public string ModelDataName;
    public string ModName = "";
    public Sprite GetIconSprite()
    {
        if(!(IconSprite.Contains(".png")))
            IconSprite += ".png";
        return GetSprite(SysDefine.AssetbundleIconSpritePath + IconSprite);
    }
    public List<Sprite> GetAliveSprite()
    {
        int num = Random.Range(0, foregroundSprite.Count);
        return new List<Sprite> { GetSprite(SysDefine.AssetbundleSpritePath + foregroundSprite[num]), GetSprite(SysDefine.AssetbundleSpritePath + midgroundSprite[num]),
        GetSprite(SysDefine.AssetbundleSpritePath+ backgroundSprite[num])};
        

    }
    public List<Sprite> GetDeathSprite() 
    {
        return new List<Sprite> { GetSprite(SysDefine.AssetbundleSpritePath + foregroundSprite[1]), GetSprite(SysDefine.AssetbundleSpritePath + midgroundSprite[1]),
        GetSprite(SysDefine.AssetbundleSpritePath+ backgroundSprite[1])};
    }
    private Sprite GetSprite(string type)
    {
        Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle+SysDefine.PlantSpritePath+ type, null) as Sprite;
        return sprite;
    }
}
