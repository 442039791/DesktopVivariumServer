using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    public float Volume
    {
        get { return stateData.Volume; }
        set { stateData.Volume = value; }
    }
    /// <summary>
    /// 得到已经使用的空间尺寸
    /// </summary>
    public float GetUseVolume()
    {
        float num = 0;
        foreach (var item in AnimalCollection.Entities)
        {
            num += item.data.activitySpace;
        }
        return num;
    }
    
    /// <summary>
    /// 鱼缸是否满了
    /// </summary>
    /// <returns></returns>
    public bool IsBioTankFull()
    {
        return GetUseVolume() >= GetMaxVolume();
    }
    
}
