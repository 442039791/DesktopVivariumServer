using UnityEngine;
using static Gather;
[System.Serializable]
public class GatherSaveData
{
   public UnlockSaveData gatherUnlockSaveData;
   public GatherStateData gatherStateData;
   public bool isFirstGather10001Completed;
}
[System.Serializable]
public class GatherStateData
{
    public GatherState gatherState;
    public string GatherID;
    public float StartTime;
    public Reward[] rewards;
}
