using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    public float Waste
    {
        get { return stateData.Waste; }
        set { stateData.Waste = value; }
    }
    /// <summary>
    /// 获取垃圾的上限
    /// </summary>
    public float GetWasteMax()
    {
        return Volume * 10;
    }
    /// <summary>
    /// 获取垃圾的比例
    /// </summary>
    public float WasteProportion()
    {
        return Waste / GetWasteMax();
    }
    /// <summary>
    /// 修改鱼缸中垃圾的含量，正为加负为减。
    /// </summary>
    /// <param name="num">变化的幅度，正是加，负是减。</param>
    public void OnWasteChanged(float num)
    {
        Waste += num;
        if (Waste > GetWasteMax())
        {
            Waste = GetWasteMax();
        }
        else if (Waste < 0)
        {
            Waste = 0;
        }
    }
    /// <summary>
    /// 去除垃圾的行为
    /// </summary>
    public void RemoveWasteBehavior()
    {
        OnWasteChanged(-10);
    }
    /// <summary>
    /// 获取垃圾1小时数据的变化量
    /// </summary>
    public float GetWasteHourChange()
    {
        float num = 0;
        //首先获取各个动物的产出或者消耗数量，然后获取植物的产出或者消耗数量
        if (IsNutrientConsume())
        {
            foreach (var item in PlantCollection.Entities)
            {
                num += item.GetWasteHourChange();
            }
        }

        foreach (var item in AnimalCollection.Entities)
        {
            num += item.GetWasteHourChange();
        }

        return num;
    }
}
