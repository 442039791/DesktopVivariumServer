using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class UnlockSaveData
{
    [System.Serializable]
    public class GatherUnlockData
    {
        public string ItemID;
        public bool isUnlocked;
        public int Quality;
    }


    public GatherUnlockData[] FishUnlockDic;
    public GatherUnlockData[] PlantUnlockDic;
    public GatherUnlockData[] DecorationUnlockDic;
    public GatherUnlockData[] CubeUnlockDic;
    public GatherUnlockData[] FacilitiesUnlockDic;
    public GatherUnlockData[] AdmissionUnlockDic;
    public GatherUnlockData[] TankUnlockDic;
    public void GetGatherUnlockData(out Dictionary<string, int> fishUnlockDic, out Dictionary<string, bool> plantUnlockDic, out Dictionary<string, bool> cubeUnlockDic, out Dictionary<string, bool> decorationUnlockDic, out Dictionary<string, bool> facilitiesUnlockDic, out Dictionary<string, bool> admissionUnlockDic, out Dictionary<string, bool> tankUnlockDic)
    {
        fishUnlockDic = new Dictionary<string, int>();
        plantUnlockDic = new Dictionary<string, bool>();
        decorationUnlockDic = new Dictionary<string, bool>();
        cubeUnlockDic = new Dictionary<string, bool>();
        facilitiesUnlockDic = new Dictionary<string, bool>();
        admissionUnlockDic = new Dictionary<string, bool>();
        tankUnlockDic=new Dictionary<string, bool>();

        if (FishUnlockDic!=null)
        {
            for (int i = 0; i < FishUnlockDic.Length; i++)
            {
                fishUnlockDic.Add(FishUnlockDic[i].ItemID, FishUnlockDic[i].Quality);
            }
        }

        if (PlantUnlockDic!= null)
        {
            for (int i = 0; i < PlantUnlockDic.Length; i++)
            {
                plantUnlockDic.Add(PlantUnlockDic[i].ItemID, PlantUnlockDic[i].isUnlocked);
            }
        }
        if (DecorationUnlockDic!= null)
        {
            for (int i = 0; i < DecorationUnlockDic.Length; i++)
            {
                decorationUnlockDic.Add(DecorationUnlockDic[i].ItemID, DecorationUnlockDic[i].isUnlocked);
            }
        }
        if(CubeUnlockDic!=null)
        {
            for (int i = 0; i < CubeUnlockDic.Length; i++)
            {
                cubeUnlockDic.Add(CubeUnlockDic[i].ItemID, CubeUnlockDic[i].isUnlocked);
            }
        }
        if (FacilitiesUnlockDic != null)
        {
            for (int i = 0; i < FacilitiesUnlockDic.Length; i++)
            {
                facilitiesUnlockDic.Add(FacilitiesUnlockDic[i].ItemID, FacilitiesUnlockDic[i].isUnlocked);
            }
        }
        if (AdmissionUnlockDic != null)
        {
            for (int i = 0; i < AdmissionUnlockDic.Length; i++)
            {
                admissionUnlockDic.Add(AdmissionUnlockDic[i].ItemID, AdmissionUnlockDic[i].isUnlocked);
            }
        }
        if (TankUnlockDic != null)
        {
            for (int i = 0; i < TankUnlockDic.Length; i++)
            {
                tankUnlockDic.Add(TankUnlockDic[i].ItemID, TankUnlockDic[i].isUnlocked);
            }
        }

        foreach (var data in GameConfigDataBase.GetConfigDatas<AnimalData>(SysDefine.AnimalConfigData))
        {
            if (!fishUnlockDic.ContainsKey(data.ID))
            {
                fishUnlockDic.Add(data.ID,-1);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<PlantData>(SysDefine.PlantConfigData))
        {
            if (!plantUnlockDic.ContainsKey(data.ID))
            {
                plantUnlockDic.Add(data.ID, false);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<CubeData>(SysDefine.CubeConfigData))
        {
            if (!cubeUnlockDic.ContainsKey(data.ID))
            {
                cubeUnlockDic.Add(data.ID, false);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<DecorationData>(SysDefine.DecorationConfigData))
        {
            if (!decorationUnlockDic.ContainsKey(data.ID))
            {
                decorationUnlockDic.Add(data.ID, false);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<AdmissionData>(SysDefine.AdmissionConfigData))
        {
            if (!admissionUnlockDic.ContainsKey(data.ID))
            {
                admissionUnlockDic.Add(data.ID, false);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<FacilitiesData>(SysDefine.FacilitiesConfigData))
        {
            if (!facilitiesUnlockDic.ContainsKey(data.ID))
            {
                facilitiesUnlockDic.Add(data.ID, false);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<TankData>(SysDefine.TankConfigData))
        {
            if (!tankUnlockDic.ContainsKey(data.ID))
            {
                tankUnlockDic.Add(data.ID, false);
            }
        }
    }
    public bool SetGetGatherUnlockData(Dictionary<string, int> fishUnlockDic, Dictionary<string, bool> plantUnlockDic, Dictionary<string, bool> cubeUnlockDic, Dictionary<string, bool> decorationUnlockDic, Dictionary<string, bool> facilitiesUnlockDic, Dictionary<string, bool> admissionUnlockDic, Dictionary<string, bool> tankUnlockDic)
    {
        if (fishUnlockDic == null || plantUnlockDic == null || cubeUnlockDic == null || decorationUnlockDic == null || facilitiesUnlockDic == null || admissionUnlockDic == null || tankUnlockDic == null)
        {
            return false;
        }
        FishUnlockDic = new GatherUnlockData[fishUnlockDic.Count];
        PlantUnlockDic = new GatherUnlockData[plantUnlockDic.Count];
        DecorationUnlockDic = new GatherUnlockData[decorationUnlockDic.Count];
        CubeUnlockDic = new GatherUnlockData[cubeUnlockDic.Count];
        FacilitiesUnlockDic = new GatherUnlockData[facilitiesUnlockDic.Count];
        AdmissionUnlockDic = new GatherUnlockData[admissionUnlockDic.Count];
        TankUnlockDic = new GatherUnlockData[tankUnlockDic.Count];
        int index = 0;
        foreach (var item in fishUnlockDic)
        {
            FishUnlockDic[index] = new GatherUnlockData() { ItemID = item.Key, Quality = item.Value };
            index++;
        }
        index = 0;
        foreach (var item in plantUnlockDic)
        {
            PlantUnlockDic[index] = new GatherUnlockData() { ItemID = item.Key, isUnlocked = item.Value };
            index++;
        }
        index = 0;
        foreach (var item in decorationUnlockDic)
        {
            DecorationUnlockDic[index] = new GatherUnlockData() { ItemID = item.Key, isUnlocked = item.Value };
            index++;
        }
        index = 0;
        foreach (var item in cubeUnlockDic)
        {
            CubeUnlockDic[index] = new GatherUnlockData() { ItemID = item.Key, isUnlocked = item.Value };
            index++;
        }
        index = 0;
        foreach (var item in facilitiesUnlockDic)
        {
            FacilitiesUnlockDic[index] = new GatherUnlockData() { ItemID = item.Key, isUnlocked = item.Value };
            index++;
        }
        index = 0;
        foreach (var item in admissionUnlockDic)
        {
            AdmissionUnlockDic[index] = new GatherUnlockData() { ItemID = item.Key, isUnlocked = item.Value };
            index++;
        }
        index = 0;
        foreach (var item in tankUnlockDic)
        {
            TankUnlockDic[index] = new GatherUnlockData() { ItemID = item.Key, isUnlocked = item.Value };
            index++;
        }
        return true;
    }
}
