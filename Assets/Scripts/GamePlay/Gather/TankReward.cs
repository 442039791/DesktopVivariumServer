using UnityEngine;
using System.Collections.Generic;
using Common;


public partial  class  Gather : MonoSingleton<Gather>
{
    /// <summary>
    /// 鱼缸奖励的处理
    /// </summary>
    public List<Reward> TankReward(string gatherID)
    {
        List<Reward> rewards = new();

        for (int y = 0; y < gatherAddressData[gatherID].TankNum; y++)
        {
            //所有设备的权重
            List<int> WeightList = new();
            //最大权重
            int WeightMax = 0;
            //奖励id的列表
            List<string> iDList = new();
            //整理权重
            for (int i = 0; i < GatherDataDic[gatherID].TankIDList.Count; i++)
            {
                if (TankUnlockDic.ContainsKey(GatherDataDic[gatherID].TankIDList[i]) && !TankUnlockDic[GatherDataDic[gatherID].TankIDList[i]])
                {
                    WeightMax += GatherDataDic[gatherID].TankWeightList[i];
                    iDList.Add(GatherDataDic[gatherID].TankIDList[i]);
                    WeightList.Add(WeightMax);
                }
            }
            if (WeightMax == 0)
            {
                return rewards;
            }
            if (Random.Range(0f, 1f) < gatherAddressData[gatherID].TankProbability)
            {
                var a = Random.Range(0, WeightMax);
                for (int x = 0; x < WeightList.Count; x++)
                {
                    if (a <= WeightList[x])
                    {
                        rewards.Add(new()
                        {
                            rewardType = RewardType.Tank,
                            rewardID = iDList[x],
                        });
                        TankUnlockDic[iDList[x]] = true;
                        break;
                    }
                }
            }
        }
        return rewards;
    }
}
