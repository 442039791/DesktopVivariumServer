using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Random = UnityEngine.Random;

[System.Serializable]
public class AnimalStateData
{
    public Vector3 Position;
	public int ID;
    /// <summary>
    /// 动物类型
    /// </summary>
    public string Type;
    ///<summary>
    ///动物的状态
    /// </summary>
    public AnimalAgeStatus State;
    ///<summary>
    ///品质
    /// </summary>
    public float Quality;
    ///<summary>
    ///拥有的特质列表
    /// </summary>
    public List<Feature> FeatureList;
    ///<summary>
    /// 是否怀孕,当时间为0的时候，可以怀孕
    /// </summary>
    public float ReproductionProgress;
    ///<summary>
    ///成长的进度
    /// </summary>
    public float GrowUpNum;
    ///<summary>
    ///鱼的血量
    /// </summary>
    public float HP;
    /// <summary>
    /// 食物储备数量
    /// </summary>
    public float FoodReserve;

    /// <summary>
    /// 属于哪个鱼缸
    /// </summary>
    public int ObjID;
    ///// <summary>
    ///// 成年鱼数量
    ///// </summary>
    ////public float AdultNum;
    ///// <summary>
    ///// 成年鱼死亡数量
    ///// </summary>
    //public float AdultDeath;
    ///// <summary>
    ///// 幼年鱼数量
    ///// </summary>
    //public float JuvenileNum;
    ///// <summary>
    ///// 幼年鱼死亡数量
    ///// </summary>
    //public float JuvenileDeath;

    public int GetPrice()
    {
        float Magnification = 1f;
        var fd = GetAnimalData();
        int price = GetIsAdult() ? fd.adultPurchasePrice : fd.juvenilePurchasePrice;
        switch (GetObjectQuality())
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

    public bool GetIsAdult()
    {
        switch (State)
        {
            case AnimalAgeStatus.Adult:
                return true;
            case AnimalAgeStatus.Juvenile:
                return false;
            default:
                return false;
        }
    }
    public ObjectQuality GetObjectQuality()
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
    public AnimalStateData DeepCopy()
    {
        return new AnimalStateData
        {
            //Position = new Vector3(Position.x, Position.y, Position.z),
            ID = this.ID,
            Type = this.Type != null ? string.Copy(this.Type) : null,
            ObjID = this.ObjID,
            State = this.State,
            Quality = this.Quality,
            //TODO:深拷贝特质列表
            FeatureList = this.FeatureList != null ?new List<Feature>(this.FeatureList) :null,
            ReproductionProgress = this.ReproductionProgress,
            GrowUpNum = this.GrowUpNum,
            HP = this.HP,   
            //AdultNum = this.AdultNum,
            //AdultDeath = this.AdultDeath,
            //JuvenileNum = this.JuvenileNum,
            //JuvenileDeath = this.JuvenileDeath,
            FoodReserve = this.FoodReserve,
        };
    }
    public AnimalData GetAnimalData()
    {
        return GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(Type),SysDefine.AnimalConfigData);
    }
    public static AnimalStateData GetStateDataByAnimalData(AnimalData data)
    {
        AnimalStateData f = new AnimalStateData()
        {
            //Position = Vector3.zero,
            Type = data.ID,
            ObjID = IDFactory.GetUniqueID(),
            ReproductionProgress = 0,
            HP = 100,
            //TODO:抽奖概率
            Quality = Random.Range(0, 4),
            State = AnimalAgeStatus.Juvenile,
            FoodReserve = data.foodReserve *0.2f,
        };
        return f;
    }

    public static AnimalStateData GetStateDataByInstance(Animal animal)
	{
		return new AnimalStateData();
    }
}


/// <summary>
/// 库存动物数据
/// </summary>
public class WarehouseAnimalStateData: WarehouseBaseData
{



    public AnimalStateData stateData;
    public new WarehouseAnimalStateData DeepCopy()
    {
        WarehouseAnimalStateData warehouseBaseData = new ()
        {
            DataType = DataType,
            Name = Name,
            Count = Count,
            Icon = Icon,
            New = New,
            stateData = stateData
        };
        return warehouseBaseData;
        
    }
}
