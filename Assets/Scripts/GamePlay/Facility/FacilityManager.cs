/*****************************************************************
 * --@ FileName: FacilityManager
 * --@ Description: 设备管理器
 * --@ Date: 2025/08/20 08:12
 * --@ Author: MagicianJoker, fengliudianshao@gmail.com
 * --@ Copyright:  Copyright (c) 2025, MagicianJoker
 ******************************************************************/

using System;
using System.Collections.Generic;
using Common;

public class FacilityManager : MonoSingleton<FacilityManager>
{
    //已经解锁的设备列表
    private List<int> _unlockFacilityList = new();

    //各个水箱的设备字典
    private Dictionary<int, Dictionary<int, FacilitiesStateData>> _bioTankOfFacilitiesDic = new();

    /// <summary>
    /// 初始化解锁设备列表
    /// </summary>
    /// <param name="facilityList"></param>
    public void InitUnLockFacilityList(List<int> facilityList)
    {
        if (facilityList == null)
            return;
        _unlockFacilityList.Clear();
        _unlockFacilityList.AddRange(facilityList);
    }


    /// <summary>
    /// 添加新解鎖的设备
    /// </summary>
    /// <param name="facilityId"></param>
    public void AddUnlockFacility(int facilityId)
    {
        if (!_unlockFacilityList.Contains(facilityId))
        {
            _unlockFacilityList.Add(facilityId);
            // 直接使用BioTankRegistry，避免创建字典副本
            var bioTanksDic = BioTankRegistry.Instance.BioTanks;
            foreach (var kvp in bioTanksDic)
            {
                AddFacilityToBioTank(kvp.Key, facilityId);
            }
        }
    }

    /// <summary>
    /// 获取已解锁的设备列表
    /// </summary>
    /// <returns></returns>
    public List<int> GetUnlockFacilityList()
    {
        return _unlockFacilityList;
    }


    /// <summary>
    /// 初始化水箱中的设备列表
    /// </summary>
    /// <param name="facilityArray"></param>
    public void InitBioTankFacilityList(FacilitiesStateData[] facilityArray)
    {
        if (facilityArray == null)
            return;
        _bioTankOfFacilitiesDic.Clear();
        foreach (var tempFacilityInfo in facilityArray)
        {
            if (!_bioTankOfFacilitiesDic.TryGetValue(tempFacilityInfo.ObjID, out var facilitiesDic))
            {
                facilitiesDic = new Dictionary<int, FacilitiesStateData>();
                _bioTankOfFacilitiesDic[tempFacilityInfo.ObjID] = facilitiesDic;
            }

            if (!facilitiesDic.TryGetValue(tempFacilityInfo.Id, out var facilityInfo))
            {
                facilityInfo = tempFacilityInfo;
                facilitiesDic[tempFacilityInfo.Id] = facilityInfo;
            }
        }
    }


    /// <summary>
    /// 向水箱中添加设备
    /// </summary>
    /// <param name="bioTankId"></param>
    /// <param name="facilityId"></param>
    public void AddFacilityToBioTank(int bioTankId, int facilityId)
    {
        if (!_bioTankOfFacilitiesDic.TryGetValue(bioTankId, out var facilitiesDic))
        {
            facilitiesDic = new Dictionary<int, FacilitiesStateData>();
            _bioTankOfFacilitiesDic[bioTankId] = facilitiesDic;
        }

        if (!facilitiesDic.TryGetValue(facilityId, out var facilityInfo))
        {
            facilityInfo = new FacilitiesStateData();
            facilityInfo.Id = facilityId;
            facilityInfo.State = FacilityState.NoBuy;
            facilityInfo.ObjID = bioTankId;
            facilitiesDic[facilityId] = facilityInfo;
        }
    }

    /// <summary>
    /// 刷新水箱中设备的状态
    /// </summary>
    /// <param name="bioTankId"></param>
    /// <param name="facilityId"></param>
    /// <param name="state"></param>
    public void RefreshBioTankFacilityStatus(int bioTankId, int facilityId, FacilityState state)
    {
        if (!_bioTankOfFacilitiesDic.ContainsKey(bioTankId))
            return;
        if (!_bioTankOfFacilitiesDic[bioTankId].TryGetValue(facilityId, out var facilityInfo))
            return;
        facilityInfo.State = state;
    }

    /// <summary>
    /// 刷新水箱中自动出售鱼设备的鱼的列表
    /// </summary>
    /// <param name="bioTankId"></param>
    /// <param name="facilityId"></param>
    public List<AutoSellFishInfo> GetBioTankFacilityStatus(int bioTankId, int facilityId)
    {
        var result = new List<AutoSellFishInfo>();
        if (!_bioTankOfFacilitiesDic.ContainsKey(bioTankId))
            return result;
        if (!_bioTankOfFacilitiesDic[bioTankId].TryGetValue(facilityId, out var facilityInfo))
            return result;
        if (facilityInfo.AutoSellFishList != null)
        {
            result.AddRange(facilityInfo.AutoSellFishList);
        }

        return result;
    }


    /// <summary>
    /// 自动售卖鱼信息是否存在
    /// </summary>
    /// <param name="bioTankId"></param>
    /// <param name="facilityId"></param>
    /// <param name="fishId"></param>
    /// <param name="ageStatus"></param>
    /// <param name="quality"></param>
    /// <returns></returns>
    public bool IsAutoSellFishInfoExist(int bioTankId, int facilityId, string fishId, AnimalAgeStatus ageStatus,
        ObjectQuality quality)
    {
        if (!_bioTankOfFacilitiesDic.ContainsKey(bioTankId))
            return false;
        if (!_bioTankOfFacilitiesDic[bioTankId].TryGetValue(facilityId, out var facilityInfo))
            return false;
        if (facilityInfo.AutoSellFishList != null)
        {
            foreach (var item in facilityInfo.AutoSellFishList)
            {
                if (item.ID == fishId && item.State == ageStatus && item.Quality == quality)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取指定水箱的设备信息
    /// 基于全局解锁列表返回设备，如果生态箱中没有该设备的状态数据则自动创建
    /// </summary>
    /// <param name="bioTankId"></param>
    /// <returns></returns>
    public List<FacilitiesStateData> GetFacilitiesStateDataByBioTankId(int bioTankId)
    {
        var result = new List<FacilitiesStateData>();

        // 确保生态箱的设备字典存在
        if (!_bioTankOfFacilitiesDic.TryGetValue(bioTankId, out var facilitiesDic))
        {
            facilitiesDic = new Dictionary<int, FacilitiesStateData>();
            _bioTankOfFacilitiesDic[bioTankId] = facilitiesDic;
        }

        // 基于全局解锁列表遍历，确保所有已解锁设备都有对应的状态数据
        foreach (var facilityId in _unlockFacilityList)
        {
            if (!facilitiesDic.TryGetValue(facilityId, out var facilityInfo))
            {
                // 如果该生态箱没有这个设备的状态数据，则自动创建
                facilityInfo = new FacilitiesStateData
                {
                    Id = facilityId,
                    State = FacilityState.NoBuy,
                    ObjID = bioTankId
                };
                facilitiesDic[facilityId] = facilityInfo;
            }
            result.Add(facilityInfo);
        }

        return result;
    }

    /// <summary>
    /// 添加鱼缸内自动出售鱼的信息
    /// </summary>
    /// <param name="bioTankId"></param>
    /// <param name="facilityId"></param>
    /// <param name="fishId"></param>
    /// <param name="ageStatus"></param>
    /// <param name="quality"></param>
    public void AddBioTankAutoSellFishInfo(int bioTankId, int facilityId, string fishId, AnimalAgeStatus ageStatus,
        ObjectQuality quality)
    {
        if (!_bioTankOfFacilitiesDic.ContainsKey(bioTankId))
            return;
        if (!_bioTankOfFacilitiesDic[bioTankId].TryGetValue(facilityId, out var facilityInfo))
            return;
        if (facilityInfo.AutoSellFishList != null)
        {
            for (int i = facilityInfo.AutoSellFishList.Count - 1; i >= 0; i--)
            {
                if (facilityInfo.AutoSellFishList[i].ID == fishId && 
                    facilityInfo.AutoSellFishList[i].State == ageStatus && 
                    facilityInfo.AutoSellFishList[i].Quality == quality)
                    return;
            }
        }

        facilityInfo.AutoSellFishList ??= new List<AutoSellFishInfo>();
        facilityInfo.AutoSellFishList.Add(new AutoSellFishInfo
        {
            ID = fishId,
            State = ageStatus,
            Quality = quality
        });
    }

    /// <summary>
    /// 移除鱼缸内自动出售鱼的信息
    /// </summary>
    /// <param name="bioTankId"></param>
    /// <param name="facilityId"></param>
    /// <param name="fishId"></param>
    /// <param name="ageStatus"></param>
    /// <param name="quality"></param>
    public void RemoveBioTankAutoSellAnimalInfo(int bioTankId, int facilityId, string fishId, AnimalAgeStatus ageStatus,
        ObjectQuality quality)
    {
        if (!_bioTankOfFacilitiesDic.ContainsKey(bioTankId))
            return;
        if (!_bioTankOfFacilitiesDic[bioTankId].TryGetValue(facilityId, out var facilityInfo))
            return;
        for (int i = facilityInfo.AutoSellFishList.Count - 1; i >= 0; i--)
        {
            if (facilityInfo.AutoSellFishList[i].ID != fishId)
                continue;
            if (facilityInfo.AutoSellFishList[i].State != ageStatus)
                continue;
            if (facilityInfo.AutoSellFishList[i].Quality != quality)
                continue;
            facilityInfo.AutoSellFishList.RemoveAt(i);
            break;
        }
    }


    /// <summary>
    /// 获取所有水箱的设备信息
    /// </summary>
    /// <returns></returns>
    public List<FacilitiesStateData> GetAllFacilitiesStateData()
    {
        var result = new List<FacilitiesStateData>();
        foreach (var facilityDic in _bioTankOfFacilitiesDic.Values)
        {
            foreach (var facilityInfo in facilityDic.Values)
            {
                result.Add(facilityInfo);
            }
        }

        return result;
    }


    /// <summary>
    /// 设备产生作用
    /// </summary>
    public void FacilitiesUpdate(int bioTankId, BioTankManager bioTank)
    {
        if (!_bioTankOfFacilitiesDic.TryGetValue(bioTankId, out var facilitiesStateDic))
            return;
        //自动增加植物营养
        if (facilitiesStateDic.ContainsKey(5001) && facilitiesStateDic[5001].State == FacilityState.Activation)
        {
            bioTank.AutoAddNutrition();
        }

        if (facilitiesStateDic.ContainsKey(5002) && facilitiesStateDic[5002].State == FacilityState.Activation)
        {
            bioTank.AutoSellAnimal(bioTankId);
        }
    }
}