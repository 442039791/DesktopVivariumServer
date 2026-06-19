using UnityEngine;
using System.Collections.Generic;
using Common;


public partial  class  Gather : MonoSingleton<Gather>
{
    /// <summary>
    /// 通行证的处理
    /// </summary>
    public List<Reward> AdmissionReward(string gatherID)
    {
        List<Reward> rewards = new();

        for (int y = 0; y < gatherAddressData[gatherID].AdmissionNum; y++)
        {
            //所有设备的权重
            List<int> WeightList = new();
            //最大权重
            int WeightMax = 0;
            //奖励id的列表
            List<string> iDList = new();
            //整理权重
            for (int i = 0; i < GatherDataDic[gatherID].AdmissionIDList.Count; i++)
            {
                if (AdmissionUnlockDic.ContainsKey(GatherDataDic[gatherID].AdmissionIDList[i]) && !AdmissionUnlockDic[GatherDataDic[gatherID].AdmissionIDList[i]])
                {
                    WeightMax += GatherDataDic[gatherID].AdmissionWeightList[i];
                    iDList.Add(GatherDataDic[gatherID].AdmissionIDList[i]);
                    WeightList.Add(WeightMax);
                }
            }
            if (WeightMax == 0)
            {
                return rewards;
            }
            if (Random.Range(0f, 1f) < gatherAddressData[gatherID].AdmissionProbability)
            {
                var a = Random.Range(0, WeightMax);
                for (int x = 0; x < WeightList.Count; x++)
                {
                    if (a <= WeightList[x])
                    {
                        rewards.Add(new()
                        {
                            rewardType = RewardType.Admission,
                            rewardID = iDList[x],
                        });
                        AdmissionUnlockDic[iDList[x]] = true;
                        isNewAddmissonRewardGet = true;
                        break;
                    }
                }
            }
        }
        return rewards;
    }
}
