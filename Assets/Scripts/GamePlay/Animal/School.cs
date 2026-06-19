
using System.Collections.Generic;
using UnityEngine;

public class School : MonoBehaviour,IStop
{
    public AnimalData FishData;
    public float schoolRatio;
    public List<Animal> fishList;
    public BioTankManager Parent;
    /// <summary>
    /// 处理鱼所在群落的死亡事件，包括清除群落数据、更新鱼的状态等逻辑。
    /// </summary>
    public void SchoolDie()
    {
        // 执行群落死亡的逻辑
    }
    /// <summary>
    /// 处理鱼所在群落的成长过程，包括增加群落成员或提升群落整体属性等逻辑。
    /// </summary>
    public void SchoolGrow()
    {
        // 执行群落成长的逻辑
    }
    /// <summary>
    /// 处理鱼所在群落的进食行为，包括群体寻找食物和分配食物资源等逻辑。
    /// </summary>
    public void SchoolEat()
    {
        //Parent.OnFoodLevelChanged(-fishList.Count * AnimalData.schoolRatio * AnimalData.foodConsumptionPerHour);
        // 执行群落进食的逻辑
    }

    public void StopChange(bool s)
    {

    }
}
