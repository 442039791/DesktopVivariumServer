using UnityEngine;
using System.Collections.Generic;
using Common;


public partial  class  Gather : MonoSingleton<Gather>
{
    /// <summary>
    /// 方块奖励的处理
    /// </summary>
    public List<Reward> CubeReward(string gatherID)
    {
        List<Reward> rewards = new();

        for (int y = 0; y < gatherAddressData[gatherID].CubeNum; y++)
        {
            //所有设备的权重
            List<int> WeightList = new();
            //最大权重
            int WeightMax = 0;
            //奖励id的列表
            List<string> iDList = new();
            //整理权重
            for (int i = 0; i < GatherDataDic[gatherID].CubeIDList.Count; i++)
            {
                if (CubeUnlockDic.ContainsKey(GatherDataDic[gatherID].CubeIDList[i]) && !CubeUnlockDic[GatherDataDic[gatherID].CubeIDList[i]])
                {
                    WeightMax += GatherDataDic[gatherID].CubeWeightList[i];
                    iDList.Add(GatherDataDic[gatherID].CubeIDList[i]);
                    WeightList.Add(WeightMax);
                }
            }
            if (WeightMax == 0)
            {
                return rewards;
            }
            if (Random.Range(0f, 1f) < gatherAddressData[gatherID].CubeProbability)
            {
                var a = Random.Range(0, WeightMax);
                for (int x = 0; x < WeightList.Count; x++)
                {
                    if (a <= WeightList[x])
                    {
                        rewards.Add(new()
                        {
                            rewardType = RewardType.Cube,
                            rewardID = iDList[x],
                        });
                        CubeUnlockDic[iDList[x]] = true;
                        break;
                    }
                }
            }
        }
        return rewards;
    }
}
