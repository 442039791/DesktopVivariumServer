/*****************************************************************
 * --@ FileName: UI_IlluGuide
 * --@ Description: UI主面板，初始化按钮及数据
 * --@ Author: paofan25, alivecn2@gmail.com
 * --@ Copyright: Copyright (c) 2025, paofan25
 ******************************************************************/

using Common;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_IlluGuide : MonoSingleton<UI_IlluGuide>
{
    private bool hasRegisteredLanguageEvent = false;

    [Header("各个道具的列表")]
    public GameObject _AnimalList;
    public GameObject _PlantList;
    public GameObject _FacilitiesList;
    public GameObject _CubeList;
    public GameObject _DecorationList;
    public GameObject _TankList;
    public GameObject _AdmissionList;

    [Header("各个道具的详情页签")]
    public GameObject _Info_Fish;
    public GameObject _Info_Plant;
    public GameObject _Info_Item;
    [Header("各个道具的标题")]
    public TextMeshProUGUI _AnimalText;
    public TextMeshProUGUI _PlantText;
    public TextMeshProUGUI _FacilitiesText;
    public TextMeshProUGUI _CubeText;
    public TextMeshProUGUI _DecorationText;
    public TextMeshProUGUI _TankText;
    public TextMeshProUGUI _AdmissionText;
    [Header("总列表")]
    public GameObject _ListGo;
    [Header("各个列表的开关")]
    public GameObject _AnimalSwitch;
    public GameObject _PlantSwitch;
    public GameObject _FacilitiesSwitch;
    public GameObject _CubeSwitch;
    public GameObject _DecorationSwitch;
    public GameObject _TankSwitch;
    public GameObject _AdmissionSwitch;
    /// <summary>
    /// 当前选中的物品
    /// </summary>
    public (RewardType rewardType, string id) Selected;

    Gather _Gather;
    List<AnimalData> animalDataList = new();
    List<PlantData> plantDataList = new();
    List<FacilitiesData> facilitiesDataList = new();
    List<CubeData> cubeDataList = new();
    List<DecorationData> decorationDataList = new();
    List<AdmissionData> admissionDataList = new();
    List<TankData> tankDataList = new();
    /// <summary>
    /// 所有道具icon的集合
    /// </summary>
    readonly Dictionary<RewardType, Dictionary<string, GameObject>> ItemIconDic = new();
    bool isInit = false;
    public void OnEnable()
    {
        if (!isInit)
        {
            _Gather = MonoSingleton<Gather>.Instance;
            InitData();
            ItemIconDic.Clear();
            ItemIconDic.Add(RewardType.Animal, new());
            ItemIconDic.Add(RewardType.Plant, new());
            ItemIconDic.Add(RewardType.Facilities, new());
            ItemIconDic.Add(RewardType.Cube, new());
            ItemIconDic.Add(RewardType.Decoration, new());
            ItemIconDic.Add(RewardType.Admission, new());
            ItemIconDic.Add(RewardType.Tank, new());
            Selected = new(RewardType.Animal, animalDataList[0].ID);
            var itemIconGo = (GameObject)ResMgr.Instance.GetAssetCache(SysDefine.PrefabAssetbundle + SysDefine.UI_ItemIconPrefab, null);
            InitcanAnimalObject(itemIconGo);
            InitcanPlantObject(itemIconGo);
            InitcanFacilitiesObject(itemIconGo);
            InitcanCubeObject(itemIconGo);
            InitcanDecorationObject(itemIconGo);
            InitcanAdmissionObject(itemIconGo);
            InitcanTankObject(itemIconGo);
            isInit =true;
        }
        else
        {
            foreach (var item in ItemIconDic[RewardType.Animal].Values)
            {
                UpdateAnimalObject(item);
            }
            foreach (var item in ItemIconDic[RewardType.Plant].Values)
            {
                UpdateAnimalObject(item);
            }
            foreach (var item in ItemIconDic[RewardType.Cube].Values)
            {
                UpdateAnimalObject(item);
            }
            foreach (var item in ItemIconDic[RewardType.Tank].Values)
            {
                UpdateAnimalObject(item);
            }
            foreach (var item in ItemIconDic[RewardType.Admission].Values)
            {
                UpdateAnimalObject(item);
            }
            foreach (var item in ItemIconDic[RewardType.Facilities].Values)
            {
                UpdateAnimalObject(item);
            }
            foreach (var item in ItemIconDic[RewardType.Decoration].Values)
            {
                UpdateAnimalObject(item);
            }
        }
        //更新标题
        UpdateTitle();
        //更新详情信息
        UpdateInfo();
    }
    /// <summary>
    /// 动物的初始化
    /// </summary>
    /// <param name="index"></param>
    /// <param name="object"></param>
    public void InitcanAnimalObject(GameObject itemIconGo)
    {
        foreach (var item in animalDataList)
        {
            GameObject instance = Instantiate(itemIconGo, _AnimalList.transform);
            ItemIconDic[RewardType.Animal].Add(item.ID, instance);
            if (instance.TryGetComponent<ItemIcon>(out var itemIconPrefab))
            {
                itemIconPrefab.uI_IlluGuide = this;
                itemIconPrefab.Selected.rewardType = RewardType.Animal;
                itemIconPrefab.Selected.id = item.ID;
            }
            UpdateAnimalObject(instance);
        }
    }
    /// <summary>
    /// 植物的初始化
    /// </summary>
    /// <param name="index"></param>
    /// <param name="object"></param>
    public void InitcanPlantObject(GameObject itemIconGo)
    {
        foreach (var item in plantDataList)
        {
            GameObject instance = Instantiate(itemIconGo, _PlantList.transform);
            ItemIconDic[RewardType.Plant].Add(item.ID, instance);
            if (instance.TryGetComponent<ItemIcon>(out var itemIconPrefab))
            {
                itemIconPrefab.uI_IlluGuide = this;
                itemIconPrefab.Selected.rewardType = RewardType.Plant;
                itemIconPrefab.Selected.id = item.ID;
            }
            UpdateAnimalObject(instance);
        }
    }
    /// <summary>
    /// 的初始化
    /// </summary>
    /// <param name="index"></param>
    /// <param name="object"></param>
    public void InitcanFacilitiesObject(GameObject itemIconGo)
    {
        foreach (var item in facilitiesDataList)
        {
            GameObject instance = Instantiate(itemIconGo, _FacilitiesList.transform);
            ItemIconDic[RewardType.Facilities].Add(item.ID, instance);
            if (instance.TryGetComponent<ItemIcon>(out var itemIconPrefab))
            {
                itemIconPrefab.uI_IlluGuide = this;
                itemIconPrefab.Selected.rewardType = RewardType.Facilities;
                itemIconPrefab.Selected.id = item.ID;
            }
            UpdateAnimalObject(instance);
        }
    }
    public void InitcanCubeObject(GameObject itemIconGo)
    {
        foreach (var item in cubeDataList)
        {
            GameObject instance = Instantiate(itemIconGo, _CubeList.transform);
            ItemIconDic[RewardType.Cube].Add(item.ID, instance);
            if (instance.TryGetComponent<ItemIcon>(out var itemIconPrefab))
            {
                itemIconPrefab.uI_IlluGuide = this;
                itemIconPrefab.Selected.rewardType = RewardType.Cube;
                itemIconPrefab.Selected.id = item.ID;
            }
            UpdateAnimalObject(instance);
        }
    }
    public void InitcanDecorationObject(GameObject itemIconGo)
    {
        foreach (var item in decorationDataList)
        {
            GameObject instance = Instantiate(itemIconGo, _DecorationList.transform);
            ItemIconDic[RewardType.Decoration].Add(item.ID, instance);
            if (instance.TryGetComponent<ItemIcon>(out var itemIconPrefab))
            {
                itemIconPrefab.uI_IlluGuide = this;
                itemIconPrefab.Selected.rewardType = RewardType.Decoration;
                itemIconPrefab.Selected.id = item.ID;
            }
            UpdateAnimalObject(instance);
        }
    }
    public void InitcanAdmissionObject(GameObject itemIconGo)
    {
        foreach (var item in admissionDataList)
        {
            GameObject instance = Instantiate(itemIconGo, _AdmissionList.transform);
            ItemIconDic[RewardType.Admission].Add(item.ID, instance);
            if (instance.TryGetComponent<ItemIcon>(out var itemIconPrefab))
            {
                itemIconPrefab.uI_IlluGuide = this;
                itemIconPrefab.Selected.rewardType = RewardType.Admission;
                itemIconPrefab.Selected.id = item.ID;
            }
            UpdateAnimalObject(instance);
        }
    }
    public void InitcanTankObject(GameObject itemIconGo)
    {
        foreach (var item in tankDataList)
        {
            GameObject instance = Instantiate(itemIconGo, _TankList.transform);
            ItemIconDic[RewardType.Tank].Add(item.ID, instance);
            if (instance.TryGetComponent<ItemIcon>(out var itemIconPrefab))
            {
                itemIconPrefab.uI_IlluGuide = this;
                itemIconPrefab.Selected.rewardType = RewardType.Tank;
                itemIconPrefab.Selected.id = item.ID;
            }
            UpdateAnimalObject(instance);
        }
    }
    /// <summary>
    /// 更新动物的预制体
    /// </summary>
    public void UpdateAnimalObject(GameObject instance)
    {
        if (instance.TryGetComponent<ItemIcon>(out var itemIconPrefab))
        {
            //处理图标的展示
            switch (itemIconPrefab.Selected.rewardType) 
            {
                case RewardType.Animal:
                    itemIconPrefab.Init(Tools.GetFishIcon(
                        itemIconPrefab.Selected.id, _Gather.FishUnlockDic[itemIconPrefab.Selected.id], true),
                        Tools.GetFrame(_Gather.FishUnlockDic[itemIconPrefab.Selected.id]));
                    if (Selected.rewardType == RewardType.Animal && Selected.id == itemIconPrefab.Selected.id)
                    {
                        itemIconPrefab.Choice.SetActive(true);
                    }
                    //处理按钮，如果没有解锁则按钮不支持点击
                    if (!_Gather.FishUnlockDic.TryGetValue(itemIconPrefab.Selected.id, out var quality))
                    {
                        break;
                    }
                    if (quality < 0)
                    {
                        break;
                    }
                    itemIconPrefab.button.enabled = true;
                    break;
                case RewardType.Plant:
                    itemIconPrefab.Init(
                        Tools.GetItemIcon(RewardType.Plant, itemIconPrefab.Selected.id, _Gather.PlantUnlockDic[itemIconPrefab.Selected.id]), 
                        Tools.GetFrame(0));
                    if (Selected.rewardType == RewardType.Plant && Selected.id == itemIconPrefab.Selected.id)
                    {
                        itemIconPrefab.Choice.SetActive(true);
                    }

                    //处理按钮，如果没有解锁则按钮不支持点击
                    if (!_Gather.PlantUnlockDic.TryGetValue(itemIconPrefab.Selected.id, out var isPlant))
                    {
                        break;
                    }
                    if (!isPlant)
                    {
                        break;
                    }
                    itemIconPrefab.button.enabled = true;
                    break;
                case RewardType.Facilities:
                    itemIconPrefab.Init(
                        Tools.GetItemIcon(RewardType.Facilities, itemIconPrefab.Selected.id, _Gather.FacilitiesUnlockDic[itemIconPrefab.Selected.id]),
                        Tools.GetFrame(0));
                    if (Selected.rewardType == RewardType.Facilities && Selected.id == itemIconPrefab.Selected.id)
                    {
                        itemIconPrefab.Choice.SetActive(true);
                    }

                    if (!_Gather.FacilitiesUnlockDic.TryGetValue(itemIconPrefab.Selected.id, out var isFacilities))
                    {
                        break;
                    }
                    if (!isFacilities)
                    {
                        break;
                    }
                    itemIconPrefab.button.enabled = true;
                    break;
                case RewardType.Cube:
                    itemIconPrefab.Init(
                        Tools.GetItemIcon(RewardType.Cube, itemIconPrefab.Selected.id, _Gather.CubeUnlockDic[itemIconPrefab.Selected.id]),
                        Tools.GetFrame(0));
                    if (Selected.rewardType == RewardType.Cube && Selected.id == itemIconPrefab.Selected.id)
                    {
                        itemIconPrefab.Choice.SetActive(true);
                    }

                    if (!_Gather.CubeUnlockDic.TryGetValue(itemIconPrefab.Selected.id, out var isCube))
                    {
                        break;
                    }
                    if (!isCube)
                    {
                        break;
                    }
                    itemIconPrefab.button.enabled = true;
                    break;
                case RewardType.Decoration:
                    itemIconPrefab.Init(
                        Tools.GetItemIcon(RewardType.Decoration, itemIconPrefab.Selected.id, _Gather.DecorationUnlockDic[itemIconPrefab.Selected.id]),
                        Tools.GetFrame(0));
                    if (Selected.rewardType == RewardType.Decoration && Selected.id == itemIconPrefab.Selected.id)
                    {
                        itemIconPrefab.Choice.SetActive(true);
                    }

                    if (!_Gather.DecorationUnlockDic.TryGetValue(itemIconPrefab.Selected.id, out var isDecoration))
                    {
                        break;
                    }
                    if (!isDecoration)
                    {
                        break;
                    }
                    itemIconPrefab.button.enabled = true;
                    break;
                case RewardType.Tank:
                    itemIconPrefab.Init(
                        Tools.GetItemIcon(RewardType.Tank, itemIconPrefab.Selected.id, _Gather.TankUnlockDic[itemIconPrefab.Selected.id]),
                        Tools.GetFrame(0));
                    if (Selected.rewardType == RewardType.Tank && Selected.id == itemIconPrefab.Selected.id)
                    {
                        itemIconPrefab.Choice.SetActive(true);
                    }

                    if (!_Gather.TankUnlockDic.TryGetValue(itemIconPrefab.Selected.id, out var isTank))
                    {
                        break;
                    }
                    if (!isTank)
                    {
                        break;
                    }
                    itemIconPrefab.button.enabled = true;
                    break;
                case RewardType.Admission:
                    itemIconPrefab.Init(
                        Tools.GetItemIcon(RewardType.Admission, itemIconPrefab.Selected.id, _Gather.AdmissionUnlockDic[itemIconPrefab.Selected.id]),
                        Tools.GetFrame(0));
                    if (Selected.rewardType == RewardType.Admission && Selected.id == itemIconPrefab.Selected.id)
                    {
                        itemIconPrefab.Choice.SetActive(true);
                    }

                    if (!_Gather.AdmissionUnlockDic.TryGetValue(itemIconPrefab.Selected.id, out var isAdmission))
                    {
                        break;
                    }
                    if (!isAdmission)
                    {
                        break;
                    }
                    itemIconPrefab.button.enabled = true;
                    break;
            }
        }

    }
    /// <summary>
    /// 修改当前选中的物品
    /// </summary>
    /// <param name="rewardType"></param>
    /// <param name="id"></param>
    public void SetSelected(RewardType rewardType, string id)
    {
        if (ItemIconDic[Selected.rewardType][Selected.id].TryGetComponent<ItemIcon>(out var itemIconPrefab))
        {
            itemIconPrefab.Choice.SetActive(false);
        }
        if (ItemIconDic[rewardType][id].TryGetComponent<ItemIcon>(out var newItemIconPrefab))
        {
            Selected.rewardType = rewardType;
            Selected.id = id;
            //取消选中
            newItemIconPrefab.Choice.SetActive(true);
            //更新详情信息
            UpdateInfo();
        }
    }
    /// <summary>
    /// 滚动列表，将当前选中的道具居中展示
    /// </summary>
    public void ListScroll()
    {
        //先将目标展开
        switch (Selected.rewardType)
        {
            case RewardType.Animal:
                if (!isAnimalList)
                {
                    OpenList(RewardType.Animal);
                }
                break;
            case RewardType.Plant:
                if (!isPlantList)
                {
                    OpenList(RewardType.Plant);
                }
                break;
            case RewardType.Facilities:
                if (!isFacilitiesList)
                {
                    OpenList(RewardType.Facilities);
                }
                break;
            case RewardType.Cube:
                if (!isCubeList)
                {
                    OpenList(RewardType.Cube);
                }
                break;
            case RewardType.Decoration:
                if (!isDecorationList)
                {
                    OpenList(RewardType.Decoration);
                }
                break;
            case RewardType.Tank:
                if (!isTankList)
                {
                    OpenList(RewardType.Tank);
                }
                break;
            case RewardType.Admission:

                if (!isAdmissionList)
                {
                    OpenList(RewardType.Admission);
                }
                break;
        }
        //然后计算长度
        int num = 0;
        switch (Selected.rewardType)
        {
            case RewardType.Animal:
                //加上其他物体的长度
                num+=ListLength(RewardType.Animal);
                //加上自己的长度
                for (int i = 0; i < animalDataList.Count;i++)
                {
                    if (animalDataList[i].ID== Selected.id)
                    {
                        num += 35+(int)(i / 5)*70;
                        break;
                    }
                }
                break;
            case RewardType.Plant:
                //加上其他的长度
                num += ListLength(RewardType.Plant);
                //加上自己的长度
                for (int i = 0; i < plantDataList.Count; i++)
                {
                    if (plantDataList[i].ID == Selected.id)
                    {
                        num += 35 + (int)(i / 5) * 70;
                        break;
                    }
                }
                break;
            case RewardType.Facilities:
                //加上其他的长度
                num += ListLength(RewardType.Facilities);
                //加上自己的长度
                for (int i = 0; i < facilitiesDataList.Count; i++)
                {
                    if (facilitiesDataList[i].ID == Selected.id)
                    {
                        num += 35 + (int)(i / 5) * 70;
                        break;
                    }
                }
                break;
            case RewardType.Cube:
                //加上其他的长度
                num += ListLength(RewardType.Cube);
                //加上自己的长度
                for (int i = 0; i < cubeDataList.Count; i++)
                {
                    if (cubeDataList[i].ID == Selected.id)
                    {
                        num += 35 + (int)(i / 5) * 70;
                        break;
                    }
                }
                break;
            case RewardType.Decoration:
                //加上其他的长度
                num += ListLength(RewardType.Decoration);
                //加上自己的长度
                for (int i = 0; i < decorationDataList.Count; i++)
                {
                    if (decorationDataList[i].ID == Selected.id)
                    {
                        num += 35 + (int)(i / 5) * 70;
                        break;
                    }
                }
                break;
            case RewardType.Tank:
                //加上其他的长度
                num += ListLength(RewardType.Tank);
                //加上自己的长度
                for (int i = 0; i < tankDataList.Count; i++)
                {
                    if (tankDataList[i].ID == Selected.id)
                    {
                        num += 35 + (int)(i / 5) * 70;
                        break;
                    }
                }
                break;
            case RewardType.Admission:
                //加上其他的长度
                num += ListLength(RewardType.Admission);
                //加上自己的长度
                for (int i = 0; i < admissionDataList.Count; i++)
                {
                    if (admissionDataList[i].ID == Selected.id)
                    {
                        num += 35 + (int)(i / 5) * 70;
                        break;
                    }
                }
                break;
        }

        //调整长度
        if (num>=226)
        {
            _ListGo.transform.position=new Vector3(_ListGo.transform.position.x, num, _ListGo.transform.position.z);
        }
    }
    int ListLength(RewardType rewardType)
    {
        int num = 0;
        num+=44 * ((int)rewardType + 1);
        for (int i = 0; i < (int)rewardType; i++)
        {
            switch (i)
            {
                case (int)RewardType.Animal:
                    if (isAnimalList)
                    {
                        num += 1400;
                    }
                    break;
                case (int)RewardType.Plant:
                    if (isAnimalList)
                    {
                        num += 700;
                    }
                    break;
                case (int)RewardType.Facilities:
                    if (isAnimalList)
                    {
                        num += 280;
                    }
                    break;
                case (int)RewardType.Cube:
                    if (isAnimalList)
                    {
                        num += 700;
                    }
                    break;
                case (int)RewardType.Decoration:
                    if (isAnimalList)
                    {
                        num += 280;
                    }
                    break;
                case (int)RewardType.Tank:
                    if (isAnimalList)
                    {
                        num += 210;
                    }
                    break;
            }
        }
        return num;
    }
    public void UpdateTitle()
    {
        // 注册语言切换事件（只注册一次）
        if (!hasRegisteredLanguageEvent)
        {
            event_manager.instance.add_event_listener(SysDefine.ChangeLauguageEvent, OnLanguageChanged);
            hasRegisteredLanguageEvent = true;
        }

        //更新各个标题的数据
        int fishUnlock = 0;
        foreach (var item in _Gather.FishUnlockDic.Values)
        {
            if (item >= 0)
            {
                fishUnlock++;
            }
        }
        _AnimalText.text = LocalizationManager.GetCurrentLanguageByID(15)
            + "<size=12><color=#207193>(" + fishUnlock + "/" + animalDataList.Count + ")</color></size>";

        int plantUnlock = 0;
        foreach (var item in _Gather.PlantUnlockDic.Values)
        {
            if (item)
            {
                plantUnlock++;
            }
        }
        _PlantText.text = LocalizationManager.GetCurrentLanguageByID(16)
            + "<size=12><color=#207193>(" + plantUnlock + "/" + plantDataList.Count + ")</color></size>";

        int cubeUnlock = 0;
        foreach (var item in _Gather.CubeUnlockDic.Values)
        {
            if (item)
            {
                cubeUnlock++;
            }
        }
        _CubeText.text = LocalizationManager.GetCurrentLanguageByID(67)
            + "<size=12><color=#207193>(" + cubeUnlock + "/" + cubeDataList.Count + ")</color></size>";

        int decorationUnlock = 0;
        foreach (var item in _Gather.DecorationUnlockDic.Values)
        {
            if (item)
            {
                decorationUnlock++;
            }
        }
        _DecorationText.text = LocalizationManager.GetCurrentLanguageByID(68)
            + "<size=12><color=#207193>(" + decorationUnlock + "/" + decorationDataList.Count + ")</color></size>";

        int facilitiesUnlock = 0;
        foreach (var item in _Gather.FacilitiesUnlockDic.Values)
        {
            if (item)
            {
                facilitiesUnlock++;
            }
        }
        _FacilitiesText.text = LocalizationManager.GetCurrentLanguageByID(97)
            + "<size=12><color=#207193>(" + facilitiesUnlock + "/" + facilitiesDataList.Count + ")</color></size>";

        int admissionUnlock = 0;
        foreach (var item in _Gather.AdmissionUnlockDic.Values)
        {
            if (item)
            {
                admissionUnlock++;
            }
        }
        _AdmissionText.text = LocalizationManager.GetCurrentLanguageByID(96)
            + "<size=12><color=#207193>(" + admissionUnlock + "/" + admissionDataList.Count + ")</color></size>";

        int tankUnlock = 0;
        foreach (var item in _Gather.TankUnlockDic.Values)
        {
            if (item)
            {
                tankUnlock++;
            }
        }
        _TankText.text = LocalizationManager.GetCurrentLanguageByID(98)
            + "<size=12><color=#207193>(" + tankUnlock + "/" + tankDataList.Count + ")</color></size>";
    }

    /// <summary>
    /// 语言切换时的回调
    /// </summary>
    private void OnLanguageChanged(string name, object udata)
    {
        // 重新更新所有标题文本
        UpdateTitle();
    }
    /// <summary>
    /// 更新详情信息
    /// </summary>
    public void UpdateInfo()
    {
        //更新详情信息界面
        switch (Selected.rewardType)
        {
            case RewardType.Animal:
                _Info_Fish.SetActive(true);
                _Info_Plant.SetActive(false);
                _Info_Item.SetActive(false);
                foreach (var item in animalDataList)
                {
                    if (item.ID== Selected.id)
                    {

                        if (_Info_Fish.TryGetComponent<InfoFish>(out var infoFish))
                        {
                            infoFish._Quality = _Gather.FishUnlockDic[Selected.id];
                            infoFish.Init(item);
                        }
                        break;
                        //var nameGo= _Info_Fish.GameObjectFinder.FindGameObject(item.Name);
                    }
                }
                break;
            case RewardType.Cube:
                _Info_Fish.SetActive(false);
                _Info_Plant.SetActive(false);
                _Info_Item.SetActive(true);
                foreach (var item in cubeDataList)
                {
                    if (item.ID == Selected.id)
                    {
                        if (_Info_Item.TryGetComponent<InfoItem>(out var info))
                        {
                            info.Init(item.name, item.Describe, item.Price);
                        }
                        break;
                    }
                }
                break;
            case RewardType.Decoration:
                _Info_Fish.SetActive(false);
                _Info_Plant.SetActive(false);
                _Info_Item.SetActive(true);
                foreach (var item in decorationDataList)
                {
                    if (item.ID == Selected.id)
                    {
                        if (_Info_Item.TryGetComponent<InfoItem>(out var info))
                        {
                            info.Init(item.name,item.Describe,item.Price);
                        }
                        break;
                    }
                }
                break;
            case RewardType.Plant:
                _Info_Fish.SetActive(false);
                _Info_Plant.SetActive(true);
                _Info_Item.SetActive(false);
                foreach (var item in plantDataList)
                {
                    if (item.ID== Selected.id)
                    {
                        if (_Info_Plant.TryGetComponent<InfoPlan>(out var info))
                        {
                            info.Init(item);
                        }
                        break;
                    }

                }
                break;
            case RewardType.Admission:
                _Info_Fish.SetActive(false);
                _Info_Plant.SetActive(false);
                _Info_Item.SetActive(true);
                foreach (var item in admissionDataList)
                {
                    if (item.ID == Selected.id)
                    {
                        if (_Info_Item.TryGetComponent<InfoItem>(out var info))
                        {
                            info.Init(item.Name, item.Describe, -1);
                        }
                        break;
                    }
                }
                break;
            case RewardType.Facilities:
                _Info_Fish.SetActive(false);
                _Info_Plant.SetActive(false);
                _Info_Item.SetActive(true);
                foreach (var item in facilitiesDataList)
                {
                    if (item.ID == Selected.id)
                    {
                        if (_Info_Item.TryGetComponent<InfoItem>(out var info))
                        {
                            info.Init(item.Name, item.Describe, item.Price);
                        }
                        break;
                    }
                }
                break;
            case RewardType.Tank:
                _Info_Fish.SetActive(false);
                _Info_Plant.SetActive(false);
                _Info_Item.SetActive(true);
                foreach (var item in tankDataList)
                {
                    if (item.ID == Selected.id)
                    {
                        if (_Info_Item.TryGetComponent<InfoItem>(out var info))
                        {
                            info.Init(item.name, item.Describe, item.Price);
                        }
                        break;
                    }
                }
                break;
        }
        //列表根据选中修改列表长度
    }
    /// <summary>
    /// 对图鉴功能进行初始化处理
    /// </summary>
    private void InitData()
    {
        foreach (var data in GameConfigDataBase.GetConfigDatas<AnimalData>(SysDefine.AnimalConfigData))
        {
            if (data.Display >= 1)
            {
                animalDataList.Add(data);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<PlantData>(SysDefine.PlantConfigData))
        {
            if (data.Display >= 1)
            {
                plantDataList.Add(data);
                //PlayerManager.Instance.PlantDisplayList.Add(data.ID);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<FacilitiesData>(SysDefine.FacilitiesConfigData))
        {
            if (data.Display >= 1)
            {
                facilitiesDataList.Add(data);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<CubeData>(SysDefine.CubeConfigData))
        {
            if (data.Display >= 1)
            {
                cubeDataList.Add(data);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<DecorationData>(SysDefine.DecorationConfigData))
        {
            if (data.Display >= 1)
            {
                decorationDataList.Add(data);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<AdmissionData>(SysDefine.AdmissionConfigData))
        {
            if (data.Display >= 1)
            {
                admissionDataList.Add(data);
            }
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<TankData>(SysDefine.TankConfigData))
        {
            if (data.Display >= 1)
            {
                tankDataList.Add(data);
            }
        }

        //初始选择的道具
        //UI_IlluGuide.Selected = new(RewardType.Animal, GameConfigDataBase.GetConfigDatas<AnimalData>(SysDefine.AnimalConfigData)[0].ID);
    }
    bool isAnimalList=true;
    bool isPlantList = true;
    bool isFacilitiesList = true;
    bool isCubeList = true;
    bool isDecorationList = true;
    bool isTankList = true;
    bool isAdmissionList = true;
    /// <summary>
    /// 开关列表
    /// </summary>
    public void OpenList(RewardType rewardType)
    {

        switch (rewardType)
        {
            case RewardType.Animal:
                isAnimalList = !isAnimalList;
                if (!isAnimalList)
                {
                    _AnimalList.SetActive(false);
                    _AnimalSwitch.transform.localEulerAngles = new Vector3(
                    _AnimalSwitch.transform.localEulerAngles.x,
                    _AnimalSwitch.transform.localEulerAngles.y,
                    90f);
                }
                else
                {
                    _AnimalList.SetActive(true);
                    _AnimalSwitch.transform.localEulerAngles = new Vector3(
                    _AnimalSwitch.transform.localEulerAngles.x,
                    _AnimalSwitch.transform.localEulerAngles.y,
                    0f);
                }
                break;
            case RewardType.Plant:
                isPlantList = !isPlantList;
                if (!isPlantList)
                {
                    _PlantList.SetActive(false);
                    _PlantSwitch.transform.localEulerAngles = new Vector3(
                    _PlantSwitch.transform.localEulerAngles.x,
                    _PlantSwitch.transform.localEulerAngles.y,
                    90f);
                }
                else
                {
                    _PlantList.SetActive(true);
                    _PlantSwitch.transform.localEulerAngles = new Vector3(
                    _PlantSwitch.transform.localEulerAngles.x,
                    _PlantSwitch.transform.localEulerAngles.y,
                    0f);
                }
                break;
            case RewardType.Facilities:
                isFacilitiesList = !isFacilitiesList;
                if (!isFacilitiesList)
                {
                    _FacilitiesList.SetActive(false);
                    _FacilitiesSwitch.transform.localEulerAngles = new Vector3(
                    _FacilitiesSwitch.transform.localEulerAngles.x,
                    _FacilitiesSwitch.transform.localEulerAngles.y,
                    90f);
                }
                else
                {
                    _FacilitiesList.SetActive(true);
                    _FacilitiesSwitch.transform.localEulerAngles = new Vector3(
                    _FacilitiesSwitch.transform.localEulerAngles.x,
                    _FacilitiesSwitch.transform.localEulerAngles.y,
                    0f);
                }
                break;
            case RewardType.Cube:
                isCubeList = !isCubeList;
                if (!isCubeList)
                {
                    _CubeList.SetActive(false);
                    _CubeSwitch.transform.localEulerAngles = new Vector3(
                    _CubeSwitch.transform.localEulerAngles.x,
                    _CubeSwitch.transform.localEulerAngles.y,
                    90f);
                }
                else
                {
                    _CubeList.SetActive(true);
                    _CubeSwitch.transform.localEulerAngles = new Vector3(
                    _CubeSwitch.transform.localEulerAngles.x,
                    _CubeSwitch.transform.localEulerAngles.y,
                    0f);
                }
                break;
            case RewardType.Decoration:
                isDecorationList = !isDecorationList;
                if (!isDecorationList)
                {
                    _DecorationList.SetActive(false);
                    _DecorationSwitch.transform.localEulerAngles = new Vector3(
                    _DecorationSwitch.transform.localEulerAngles.x,
                    _DecorationSwitch.transform.localEulerAngles.y,
                    90f);
                }
                else
                {
                    _DecorationList.SetActive(true);
                    _DecorationSwitch.transform.localEulerAngles = new Vector3(
                    _DecorationSwitch.transform.localEulerAngles.x,
                    _DecorationSwitch.transform.localEulerAngles.y,
                    0f);
                }
                break;
            case RewardType.Tank:
                isTankList = !isTankList;
                if (!isTankList)
                {
                    _TankList.SetActive(false);
                    _TankSwitch.transform.localEulerAngles = new Vector3(
                    _TankSwitch.transform.localEulerAngles.x,
                    _TankSwitch.transform.localEulerAngles.y,
                    90f);
                }
                else
                {
                    _TankList.SetActive(true);
                    _TankSwitch.transform.localEulerAngles = new Vector3(
                    _TankSwitch.transform.localEulerAngles.x,
                    _TankSwitch.transform.localEulerAngles.y,
                    0f);
                }
                break;
            case RewardType.Admission:
                isAdmissionList = !isAdmissionList;
                if (!isAdmissionList)
                {
                    _AdmissionList.SetActive(false);
                    _AdmissionSwitch.transform.localEulerAngles = new Vector3(
                    _AdmissionSwitch.transform.localEulerAngles.x,
                    _AdmissionSwitch.transform.localEulerAngles.y,
                    90f);
                }
                else
                {
                    _AdmissionList.SetActive(true);
                    _AdmissionSwitch.transform.localEulerAngles = new Vector3(
                    _AdmissionSwitch.transform.localEulerAngles.x,
                    _AdmissionSwitch.transform.localEulerAngles.y,
                    0f);
                }
                break;
        }

    }
}
