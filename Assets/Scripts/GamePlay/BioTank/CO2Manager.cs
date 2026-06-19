using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    public float CO2
    {
        get { return stateData.CO2; }
        set { stateData.CO2 = value; }
    }
    /// <summary>
    /// 获取二氧化碳的最大数量
    /// </summary>
    public float GetCO2Max()
    {
        return Volume * 10;
    }
    /// <summary>
    /// 获取二氧化碳的比例
    /// </summary>
    public float CO2Proportion()
    {
        return CO2 / GetCO2Max();
    }
    /// <summary>
    /// 修改鱼缸中二氧化碳的含量，正为加负为减。
    /// </summary>
    /// <param name="num">变化的幅度，正是加，负是减。</param>
    public void OnCO2Changed(float num)
    {
        CO2 += num;
        if (CO2 > GetCO2Max())
        {
            CO2 = GetCO2Max();
        }
        else if (CO2 < 0)
        {
            CO2 = 0;
        }
    }
    /// <summary>
    /// 去除二氧化碳的行为
    /// </summary>
    public void RemoveCO2Behavior()
    {
        OnCO2Changed(-10);
    }
    /// <summary>
    /// 获取二氧化碳1小时数据的变化量
    /// </summary>
    public float GetCO2HourChange()
    {
        float num = 0;
        //首先获取各个动物的产出或者消耗数量，然后获取植物的产出或者消耗数量
        if (IsNutrientConsume())
        {
            foreach (var item in PlantCollection.Entities)
            {
                num += item.GetCO2HourChange();
            }
        }

        foreach (var item in AnimalCollection.Entities)
        {
            num += item.GetCO2HourChange();
        }
        return num;
    }
}
