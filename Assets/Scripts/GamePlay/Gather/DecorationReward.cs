using UnityEngine;
using System.Collections.Generic;
using Common;


public partial  class  Gather : MonoSingleton<Gather>
{
    /// <summary>
    /// 装饰物奖励的处理
    /// </summary>
    public List<Reward> DecorationReward(string gatherID)
    {
        List<Reward> rewards = new();

        for (int y = 0; y < gatherAddressData[gatherID].DecorationNum; y++)
        {
            //所有设备的权重
            List<int> WeightList = new();
            //最大权重
            int WeightMax = 0;
            //奖励id的列表
            List<string> iDList = new();
            //整理权重
            for (int i = 0; i < GatherDataDic[gatherID].DecorationIDList.Count; i++)
            {
                if (DecorationUnlockDic.ContainsKey(GatherDataDic[gatherID].DecorationIDList[i]) && !DecorationUnlockDic[GatherDataDic[gatherID].DecorationIDList[i]])
                {
                    WeightMax += GatherDataDic[gatherID].DecorationWeightList[i];
                    iDList.Add(GatherDataDic[gatherID].DecorationIDList[i]);
                    WeightList.Add(WeightMax);
                }
            }
            if (WeightMax == 0)
            {
                return rewards;
            }
            if (Random.Range(0f, 1f) < gatherAddressData[gatherID].DecorationProbability)
            {
                var a = Random.Range(0, WeightMax);
                for (int x = 0; x < WeightList.Count; x++)
                {
                    if (a <= WeightList[x])
                    {
                        rewards.Add(new()
                        {
                            rewardType = RewardType.Decoration,
                            rewardID = iDList[x],
                        });
                        DecorationUnlockDic[iDList[x]] = true;
                        break;
                    }
                }
            }
        }
        return rewards;
    }
}
