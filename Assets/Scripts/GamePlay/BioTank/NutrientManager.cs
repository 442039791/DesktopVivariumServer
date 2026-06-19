using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    public float Nutrient
    {
        get { return stateData.Nutrient; }
        set { stateData.Nutrient = value; }
    }
    /// <summary>
    /// 获取营养最大数量
    /// </summary>
    public float GetNutrientMax()
    {
        return Volume * 10;
    }
    /// <summary>
    /// 获取水中营养的比例
    /// </summary>
    public float NutrientProportion()
    {
        return Nutrient / GetNutrientMax();
    }


    /// <summary>
    /// 修改鱼缸中营养的含量，正为加负为减。
    /// </summary>
    /// <param name="num">变化的幅度，正是加，负是减。</param>
    public void OnNutrientChanged(float num)
    {
        Nutrient += num;
        if (Nutrient > GetNutrientMax())
        {
            Nutrient = GetNutrientMax();
        }
        else if (Nutrient < 0)
        {
            Nutrient = 0;
        }
    }
    /// <summary>
    /// 添加营养的行为
    /// </summary>
    public void AddNutrientBehavior()
    {
        OnNutrientChanged(10);
    }
    /// <summary>
    /// 获取养分1小时数据的变化量
    /// </summary>
    public float GetNutrientHourChange()
    {
        float num = 0;
        //首先获取各个动物的产出或者消耗数量，然后获取植物的产出或者消耗数量
        foreach (var item in PlantCollection.Entities)
        {
            num += item.GetNutrientHourChange();
        }
        return num;
    }
    /// <summary>
    /// 营养是否满足消耗
    /// </summary>
    public bool IsNutrientConsume()
    {        
        if (Nutrient < -GetNutrientHourChange() / 60)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
