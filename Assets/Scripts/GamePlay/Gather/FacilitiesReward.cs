using UnityEngine;
using System.Collections.Generic;
using Common;


public partial  class  Gather : MonoSingleton<Gather>
{
    /// <summary>
    /// 设备奖励的处理
    /// </summary>
    public List<Reward> FacilitiesReward(string gatherID)
    {
        List<Reward> rewards = new();

        for (int y = 0; y < gatherAddressData[gatherID].FacilitiesNum; y++)
        {
            //所有设备的权重
            List<int> WeightList = new();
            //最大权重
            int WeightMax = 0;
            //奖励id的列表
            List<string> iDList = new();
            //整理权重
            for (int i = 0; i < GatherDataDic[gatherID].FacilitiesIDList.Count; i++)
            {
                if (FacilitiesUnlockDic.ContainsKey(GatherDataDic[gatherID].FacilitiesIDList[i]) && !FacilitiesUnlockDic[GatherDataDic[gatherID].FacilitiesIDList[i]])
                {
                    WeightMax += GatherDataDic[gatherID].FacilitiesWeightList[i];
                    iDList.Add(GatherDataDic[gatherID].FacilitiesIDList[i]);
                    WeightList.Add(WeightMax);
                }
            }
            if (WeightMax == 0)
            {
                return rewards;
            }
            if (Random.Range(0f, 1f) < gatherAddressData[gatherID].FacilitiesProbability)
            {
                var a = Random.Range(0, WeightMax);
                for (int x = 0; x < WeightList.Count; x++)
                {
                    if (a <= WeightList[x])
                    {
                        rewards.Add(new()
                        {
                            rewardType = RewardType.Facilities,
                            rewardID = iDList[x],
                        });
                        FacilitiesUnlockDic[iDList[x]] = true;
                        FacilityManager.Instance.AddUnlockFacility(int.Parse(iDList[x]));
                        break;
                    }
                }
            }
        }
        return rewards;
    }
}
