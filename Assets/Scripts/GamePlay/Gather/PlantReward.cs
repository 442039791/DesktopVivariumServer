using UnityEngine;
using System.Collections.Generic;
using Common;


public partial  class  Gather : MonoSingleton<Gather>
{
    /// <summary>
    /// 鱼缸解锁道具奖励的处理
    /// </summary>
    public List<Reward> PlantReward(string gatherID)
    {
        List<Reward> rewards = new();

        //整理所有鱼的权重
        List<int> WeightList = new();
        int WeightMax = 0;
        foreach (var item in GatherDataDic[gatherID].PlantWeightList)
        {
            WeightMax += item;
            WeightList.Add(WeightMax);
        }
        if (WeightMax == 0)
        {
            return rewards;
        }
        for (int i = 0; i < gatherAddressData[gatherID].PlantNum; i++)
        {
            if (Random.Range(0f, 1f) < gatherAddressData[gatherID].PlantProbability)
            {
                var a = Random.Range(0, WeightMax);
                for (int x = 0; x < WeightList.Count; x++)
                {
                    if (a <= WeightList[x])
                    {
                        rewards.Add(new()
                        {
                            rewardType = RewardType.Plant,
                            rewardID = GatherDataDic[gatherID].PlantIDList[x],

                        });
                        if (PlantUnlockDic.ContainsKey(GatherDataDic[gatherID].PlantIDList[x]) && !PlantUnlockDic[GatherDataDic[gatherID].PlantIDList[x]])
                        {
                            PlantUnlockDic[GatherDataDic[gatherID].PlantIDList[x]] = true;
                        }
                        break;
                    }
                }
            }
        }
        return rewards;
    }
}
