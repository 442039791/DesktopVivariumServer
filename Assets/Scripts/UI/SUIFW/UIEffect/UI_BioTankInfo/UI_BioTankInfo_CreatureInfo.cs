using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using SUIFW;
[System.Serializable]
public enum UI_Main_Biotank_Type
{
    Fish,
    Plant,
    Facilities,
    /// <summary>
    /// 图鉴中的动物
    /// </summary>
    Guide_Animal,
    /// <summary>
    /// 图鉴中的植物
    /// </summary>
    Guide_Plant,
    /// <summary>
    /// 图鉴中的设备
    /// </summary>
    Guide_Facilities,
    /// <summary>
    /// 图鉴中的装饰物
    /// </summary>
    Guide_Deceration,
    /// <summary>
    /// 图鉴中的生态箱
    /// </summary>
    Guide_Tank,
    /// <summary>
    /// 图鉴中的车票
    /// </summary>
    Guide_Admission,
    /// <summary>
    /// 图鉴中的方块
    /// </summary>
    Guide_Cube,
}
public class UI_BioTankInfo_CreatureInfo : ChangeLanugeBase
{
    public ListController listController;
    public TextMeshProUGUI Title;
    public UI_Main_Biotank_Type type;
    public bool InitCompelte = false;
    private int BiotankID = -1;


    public void Init(int bid)
    {
        BiotankID = bid;
        base.Init(transform);

        // 获取BioTank引用，避免多次创建字典副本
        var bioTank = BioTankRegistry.Instance.GetBioTank(BiotankID);
        if (bioTank == null)
        {
            Debug.LogWarning($"[UI_BioTankInfo_CreatureInfo] 未找到BioTank: {BiotankID}");
            return;
        }

        switch (type)
        {
            case UI_Main_Biotank_Type.Plant:
                //从ab包加载物体的方法
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_BioTankInfo_CreatureInfo_PlantInfo,
    bioTank.PlantCollection.Count,
    InitPlantItemFunc
    );
                break;
            case UI_Main_Biotank_Type.Fish:
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_BioTankInfo_CreatureInfo_AnimalInfo,
                bioTank.AnimalCollection.Count,
                InitFishItemFunc
                );
                break;
            case UI_Main_Biotank_Type.Facilities:
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_BioTankInfo_CreatureInfo_FacilityInfo,
                    FacilityManager.Instance.GetFacilitiesStateDataByBioTankId(BiotankID).Count,
                    InitFacilityItemFunc);
                break;
            case UI_Main_Biotank_Type.Guide_Animal:
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_ItemIconPrefab,
                    PlayerManager.Instance.AnimalDisplayList.Count, InitcanAnimalObject);
                break;
            case UI_Main_Biotank_Type.Guide_Plant:
                break;
            case UI_Main_Biotank_Type.Guide_Facilities:
                break;
            case UI_Main_Biotank_Type.Guide_Deceration:
                break;
            case UI_Main_Biotank_Type.Guide_Tank:
                break;
            case UI_Main_Biotank_Type.Guide_Admission:
                break;
            case UI_Main_Biotank_Type.Guide_Cube:
                break;
            default:
                break;
        }
        InitCompelte = true;
    }


    public void InitFacilityItemFunc(int count, GameObject gameObject)
    {
        UI_BioTankInfo_CreatureInfo_FacilityInfo info = gameObject.GetComponent<UI_BioTankInfo_CreatureInfo_FacilityInfo>();
        if (info != null)
        {
            info.Init(FacilityManager.Instance.GetFacilitiesStateDataByBioTankId(BiotankID)[count]);
        }
    }

    public void InitDecorationItemFunc(int count, GameObject gameObject)
    {
        UI_BioTankInfo_CreatureInfo_DecorationInfo info = gameObject.GetComponent<UI_BioTankInfo_CreatureInfo_DecorationInfo>();
        if (info != null)
        {
            var bioTank = BioTankRegistry.Instance.GetBioTank(BiotankID);
            if (bioTank != null)
            {
                info.Init(bioTank.DecorationCollection.GetByIndex(count));
            }
        }
    }
    public void InitFishItemFunc(int count, GameObject gameObject)
    {
        UI_BioTankInfo_CreatureInfo_AnimalInfo info = gameObject.GetComponent<UI_BioTankInfo_CreatureInfo_AnimalInfo>();
        if (info != null)
        {
            var bioTank = BioTankRegistry.Instance.GetBioTank(BiotankID);
            if (bioTank != null)
            {
                info.Init(bioTank.AnimalCollection.GetByIndex(count));
            }
        }
    }
    public void InitPlantItemFunc(int count, GameObject gameObject)
    {
        if (gameObject.TryGetComponent<UI_BioTankInfo_CreatureInfo_PlantInfo>(out var info))
        {
            var bioTank = BioTankRegistry.Instance.GetBioTank(BiotankID);
            if (bioTank != null)
            {
                info.Init(bioTank.PlantCollection.GetByIndex(count));
            }
        }
    }
    /// <summary>
    /// 动物的初始化
    /// </summary>
    /// <param name="index"></param>
    /// <param name="object"></param>
    public void InitcanAnimalObject(int index, GameObject @object)
    {
        var id = PlayerManager.Instance.AnimalDisplayList[index];
        if (@object.TryGetComponent<ItemIcon>(out var itemIconPrefab))
        {
            //itemIconPrefab.Init(Tools.GetFishIcon(id, Gather.Instance.FishUnlockDic[id], true), Tools.GetFrame(Gather.Instance.FishUnlockDic[id]));
            //itemIconPrefab.uI_IlluGuide = this;
            //itemIconPrefab.Selected.rewardType = RewardType.Animal;
            //itemIconPrefab.Selected.id = id;
            //if (Selected.rewardType == RewardType.Animal && Selected.id == id)
            //{
            //    itemIconPrefab.Choice.SetActive(true);

            //}
        }
    }
}