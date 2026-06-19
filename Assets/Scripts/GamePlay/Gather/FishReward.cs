using UnityEngine;
using System.Collections.Generic;
using Common;


public partial  class  Gather : MonoSingleton<Gather>
{
    /// <summary>
    /// 鱼缸解锁道具奖励的处理
    /// </summary>
    public List<Reward> FishReward(string gatherID)
    {
        List<Reward> rewards = new();


        //品质权重
        List<int> qualityWeight = new()
        {
            75,18,5,2,
        };

        //整理所有鱼的权重
        List<int> FishWeightList = new();
        List<int> qualityWeightList = new();
        int FishWeightMax = 0;
        int qualityWeightMax = 0;
        foreach (var item in GatherDataDic[gatherID].FishWeightList)
        {
            FishWeightMax += item;
            FishWeightList.Add(FishWeightMax);
        }
        foreach (var item in qualityWeight)
        {
            qualityWeightMax += item;
            qualityWeightList.Add(qualityWeightMax);
        }
        if (FishWeightMax == 0)
        {
            return rewards;
        }

        for (int i = 0; i < gatherAddressData[gatherID].FishNum; i++)
        {
            if (Random.Range(0f, 1f) < gatherAddressData[gatherID].FishProbability)
            {
                var a = Random.Range(0, FishWeightMax);
                for (int x = 0; x < FishWeightList.Count; x++)
                {
                    if (a <= FishWeightList[x])
                    {
                        var b = Random.Range(0, qualityWeightMax);
                        float quality = 0;
                        for (int y = 0; y < qualityWeightList.Count; y++)
                        {
                            if (b <= qualityWeightList[y])
                            {
                                quality += y + Random.Range(0f, 1f);
                                break;
                            }
                        }
                        rewards.Add(new()
                        {
                            rewardType = RewardType.Animal,
                            rewardID = GatherDataDic[gatherID].FishIDList[x],
                            parameter0 = quality,
                        });
                        if (FishUnlockDic.ContainsKey(GatherDataDic[gatherID].FishIDList[x]) && FishUnlockDic[GatherDataDic[gatherID].FishIDList[x]]< (int)quality)
                        {
                            FishUnlockDic[GatherDataDic[gatherID].FishIDList[x]] = (int)quality;
                        }
                        break;
                    }
                }
            }
        }
        return rewards;
    }
}
