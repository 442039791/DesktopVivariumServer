using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    public float O2
    {
        get { return stateData.O2; }
        set { stateData.O2 = value; }
    }
    /// <summary>
    /// 获取氧气的上限
    /// </summary>
    public float GetO2Max()
    {
        return Volume * 10;
    }
    /// <summary>
    /// 获取水中氧气的比例
    /// </summary>
    public float O2Proportion()
    {
        return O2 / GetO2Max();
    }
    /// <summary>
    /// 修改鱼缸中氧气的含量，正为加负为减。
    /// </summary>
    /// <param name="num">变化的幅度，正是加，负是减。</param>
    public void OnO2Changed(float num)
    {
        O2 += num;
        if (O2 > GetO2Max())
        {
            O2 = GetO2Max();
        }
        else if (O2 < 0)
        {
            O2 = 0;
        }
    }
    /// <summary>
    /// 添加氧气的行为
    /// </summary>
    public void AddO2Behavior()
    {
        OnO2Changed(10);
    }
    /// <summary>
    /// 获取氧气1小时数据的变化量
    /// </summary>
    public float GetO2HourChange()
    {
        float num = 0;
        //首先获取各个动物的产出或者消耗数量，然后获取植物的产出或者消耗数量
        if (IsNutrientConsume())
        {
            foreach (var item in PlantCollection.Entities)
            {
                num += item.GetO2HourChange();
            }
        }

        foreach (var item in AnimalCollection.Entities)
        {
            num += item.GetO2HourChange();
        }
        return num;
    }
}
