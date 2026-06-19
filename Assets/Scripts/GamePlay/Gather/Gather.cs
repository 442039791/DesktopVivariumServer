using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Common;
using System.Linq;
using TMPro;
using Direction = SeamlessScroller.Direction;

/// <summary>
/// 奖励类型
/// </summary>
public enum RewardType
{
    /// <summary>
    /// 动物
    /// </summary>
    Animal=0,
    /// <summary>
    /// 植物
    /// </summary>
    Plant=1,
    /// <summary>
    /// 结构
    /// </summary>
    Cube=3,
    /// <summary>
    /// 设备
    /// </summary>
    Facilities=2,
    /// <summary>
    /// 装饰物
    /// </summary>
    Decoration=4,
    /// <summary>
    /// 通行证
    /// </summary>
    Admission=6,
    /// <summary>
    /// 鱼缸
    /// </summary>
    Tank=5,
}
public partial class Gather : MonoSingleton<Gather>
{
    /// <summary>
    /// 后景
    /// </summary>
    public GameObject Background;
    /// <summary>
    /// 角色
    /// </summary>
    public GameObject Role;
    /// <summary>
    /// 卡池详情
    /// </summary>
    public GameObject info;
    /// <summary>
    /// 进行中
    /// </summary>
    public GameObject perform;
    /// <summary>
    /// 宝箱
    /// </summary>
    public GameObject box;
    /// <summary>
    /// 奖励
    /// </summary>
    public GameObject reward;
    public GatherStateData stateData;
    public AnimationController Character;
    public SeamlessScroller Foreground;
    public SeamlessScroller Midground;
    public List<Reward> rewardList;
    public ListController canGetRewardListController;
    public ListController rewardListController;
    public TextMeshProUGUI infoText;
    public GameObject MoneyIconObj;
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI TimeText;
    public Transform content;
    public List<WindPickingInfo> windPickingInfos = new();

    private bool isNewAddmissonRewardGet = false;
    public bool isFirstGather10001Completed = false;

    // 缓存当前GatherID对应的配置数据，避免Update中每帧查询
    private GatherAddressData _cachedAddressData;
    private string _cachedGatherID;
    
    /// <summary>
    /// 野采状态
    /// </summary>
    [System.Serializable]
    public enum GatherState
    {
        /// <summary>
        /// 未开始
        /// </summary>
        rest,
        /// <summary>
        /// 进行中
        /// </summary>
        perform,
        /// <summary>
        /// 已完成未打开奖励
        /// </summary>
        complete,
        /// <summary>
        /// 已打开奖励未领取奖励
        /// </summary>
        reward

    }

    /// <summary>
    /// 奖励结构体
    /// </summary>
    [System.Serializable]
    public struct Reward
    {
        /// <summary>
        /// 奖励类型
        /// </summary>
        public RewardType rewardType;
        /// <summary>
        /// 奖励ID
        /// </summary>
        public string rewardID;
        /// <summary>
        /// 参数0
        /// </summary>
        public float parameter0;
    }

    public Dictionary<string, GatherData> GatherDataDic = new();
    
    public Dictionary<string, int> FishUnlockDic = new();
    public Dictionary<string, bool> PlantUnlockDic = new();
    public Dictionary<string, bool> CubeUnlockDic = new();
    public Dictionary<string, bool> DecorationUnlockDic = new();
    //设备解锁状态
    public Dictionary<string, bool> FacilitiesUnlockDic = new();
    //卡池的解锁状态
    public Dictionary<string, bool> AdmissionUnlockDic = new();
    //鱼缸的解锁状态
    public Dictionary<string, bool> TankUnlockDic = new();
    //卡池的解锁状态
    public Dictionary<string, GatherAddressData> gatherAddressData;
    public bool GetAdmissionIsUnlock(string admissionID)
    {
        // 安全检查：admissionID 为空时返回 false
        if (string.IsNullOrEmpty(admissionID))
        {
            return false;
        }

        // 检查配置是否存在
        if (gatherAddressData == null || !gatherAddressData.ContainsKey(admissionID))
        {
            Debug.LogWarning($"GetAdmissionIsUnlock: 找不到配置 ID={admissionID}");
            return false;
        }

        var admissionData = gatherAddressData[admissionID];
        // 检查是否默认解锁：空字符串或"null"表示无解锁条件，即默认解锁
        if(string.IsNullOrEmpty(admissionData.Unlock) || admissionData.Unlock=="null")
            return true;
        if(AdmissionUnlockDic.TryGetValue(admissionData.Unlock,out var isUnlock))
        {
            return isUnlock;
        }

        return false;
        // return AdmissionUnlockDic.ContainsKey(admissionID) && AdmissionUnlockDic[admissionID];
    }
    /// <summary>
    /// 当前野采状态
    /// </summary>
    public GatherState State
    {
        get
        {
            if (stateData == null)
                return GatherState.rest;
            return stateData.gatherState;
        }
        set
        {
            stateData.gatherState = value;
            //UIUpdate();
        }
    }
    /// <summary>
    /// 野采开始时间
    /// </summary>
    public float StartTime;


    /// <summary>
    /// 进行野采的ID
    /// </summary>
    public string GatherID
    {
        get { return stateData.GatherID; }
        set
        {
            stateData.GatherID = value;
            //UIUpdate();
        }
    }

    public override void Init()
    {

    }

    /// <summary>
    /// 语言切换时更新TimeText
    /// </summary>
    private void OnLanguageChanged(string name, object udata)
    {
        // 只在rest状态且GatherID有效时更新TimeText
        if (State == GatherState.rest && !string.IsNullOrEmpty(GatherID) && gatherAddressData != null && gatherAddressData.ContainsKey(GatherID))
        {
            var admissionData = gatherAddressData[GatherID];
            TimeText.text = LocalizationManager.GetCurrentLanguageByID(21) + Tools.TimeString((int)admissionData.SustainTime);
        }
    }
    private bool Inited = false;
    public bool IsInited()
    {
        return Inited;
    }
    private void InitGatherData()
    {
        // 重新初始化前先清理字典，防止重复Add抛异常
        GatherDataDic.Clear();
        FishUnlockDic.Clear();
        PlantUnlockDic.Clear();
        CubeUnlockDic.Clear();
        DecorationUnlockDic.Clear();
        FacilitiesUnlockDic.Clear();
        AdmissionUnlockDic.Clear();
        TankUnlockDic.Clear();
        // 注意：不清空 AllGatherSpritesCache，因为它会在需要时自动重建

        gatherAddressData = GameConfigDataBase.GetConfigDatasByDic<GatherAddressData>(SysDefine.GatherAddressData);
        var gatherDecorationData = GameConfigDataBase.GetConfigDatasByDic<GatherDecorationData>(SysDefine.GatherDecorationData);
        var gatherAnimalData = GameConfigDataBase.GetConfigDatasByDic<GatherFishData>(SysDefine.GatherAnimalData);
        var gatherPlantData = GameConfigDataBase.GetConfigDatasByDic<GatherPlantData>(SysDefine.GatherPlantData);
        var gatherCubeData = GameConfigDataBase.GetConfigDatasByDic<GatherCubeData>(SysDefine.GatherCubeData);
        var gatherFacilitiesData = GameConfigDataBase.GetConfigDatasByDic<GatherFacilitiesData>(SysDefine.GatherFacilitiesData);
        var gatherAdmissionData = GameConfigDataBase.GetConfigDatasByDic<GatherAdmissionData>(SysDefine.GatherAdmissionData);
        var gatherTankData= GameConfigDataBase.GetConfigDatasByDic<GatherTankData>(SysDefine.GatherTankData);

        base.Init();
        foreach (var item in gatherAddressData.Values)
        {
            GatherDataDic.Add(item.ID, new()
            {
                FishIDList = new(),
                FishWeightList = new(),
                PlantIDList = new(),
                PlantWeightList = new(),
                DecorationIDList = new(),
                DecorationWeightList = new(),
                CubeIDList = new(),
                CubeWeightList = new(),
                FacilitiesIDList = new(),
                FacilitiesWeightList = new(),
                AdmissionIDList = new(),
                AdmissionWeightList = new(),
                TankIDList = new(),
                TankWeightList = new(),
            });
        }

        //鱼处理
        foreach (var item in gatherAnimalData.Values)
        {
            GatherDataDic[item.GatherAddressID].FishIDList.Add(item.FishID);
            GatherDataDic[item.GatherAddressID].FishWeightList.Add(item.Weight);
        }
        //植物处理
        foreach (var item in gatherPlantData.Values)
        {
            GatherDataDic[item.GatherAddressID].PlantIDList.Add(item.PlantID);
            GatherDataDic[item.GatherAddressID].PlantWeightList.Add(item.Weight);
        }

        //装饰物处理
        foreach (var item in gatherDecorationData.Values)
        {
            GatherDataDic[item.GatherAddressID].DecorationIDList.Add(item.DecorationID);
            GatherDataDic[item.GatherAddressID].DecorationWeightList.Add(item.Weight);
        }

        //立方体的处理
        foreach (var item in gatherCubeData.Values)
        {
            GatherDataDic[item.GatherAddressID].CubeIDList.Add(item.CubeID);
            GatherDataDic[item.GatherAddressID].CubeWeightList.Add(item.Weight);
        }

        //设备的处理
        foreach (var item in gatherFacilitiesData.Values)
        {
            GatherDataDic[item.GatherAddressID].FacilitiesIDList.Add(item.FacilitiesID);
            GatherDataDic[item.GatherAddressID].FacilitiesWeightList.Add(item.Weight);
        }

        //鱼缸的处理
        foreach (var item in gatherTankData.Values)
        {
            GatherDataDic[item.GatherAddressID].TankIDList.Add(item.TankID);
            GatherDataDic[item.GatherAddressID].TankWeightList.Add(item.Weight);
        }

        //卡池解锁状态的处理
        foreach (var item in gatherAdmissionData.Values)
        {
            GatherDataDic[item.GatherAddressID].AdmissionIDList.Add(item.AdmissionID);
            GatherDataDic[item.GatherAddressID].AdmissionWeightList.Add(item.Weight);
        }
        //所有道具解锁初始化的处理
        foreach (var data in GameConfigDataBase.GetConfigDatas<AnimalData>(SysDefine.AnimalConfigData))
        {
            FishUnlockDic.Add(data.ID, -1);
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<PlantData>(SysDefine.PlantConfigData))
        {
            PlantUnlockDic.Add(data.ID, false);
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<DecorationData>(SysDefine.DecorationConfigData))
        {
            DecorationUnlockDic.Add(data.ID, false);
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<CubeData>(SysDefine.CubeConfigData))
        {
            CubeUnlockDic.Add(data.ID, false);
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<FacilitiesData>(SysDefine.FacilitiesConfigData))
        {
            FacilitiesUnlockDic.Add(data.ID, false);
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<TankData>(SysDefine.TankConfigData))
        {
            TankUnlockDic.Add(data.ID, false);
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<AdmissionData>(SysDefine.AdmissionConfigData))
        {
            AdmissionUnlockDic.Add(data.ID, false);
        }
        InitWindPickingInfo();
    }
    /// <summary>
    /// Place预制体
    /// </summary>
    public GameObject placePrefab;

    public void InitWindPickingInfo()
    {
        windPickingInfos.Clear();

        // 空数据源检查
        if (gatherAddressData?.Values == null || content == null) return;

        if (placePrefab == null)
        {
            Debug.LogError("InitWindPickingInfo: Place预制体未设置");
            return;
        }

        // 清除所有现有子物体
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        // 安全排序处理
        var sortedData = gatherAddressData.Values
            .OrderBy(ads => ads.Sort)
            .ToList();

        // 根据配置数量动态实例化Place物体
        for (int i = 0; i < sortedData.Count; i++)
        {
            var newPlace = Instantiate(placePrefab, content);
            newPlace.name = "Place_" + sortedData[i].ID;

            var windPickingInfo = newPlace.GetComponent<WindPickingInfo>();
            if (windPickingInfo != null)
            {
                windPickingInfo.windPickID = sortedData[i].ID;

                // 如果IsUnlocked字段为空，尝试自动查找Unlock子物体
                if (windPickingInfo.IsUnlocked == null)
                {
                    windPickingInfo.IsUnlocked = newPlace.transform.Find("Unlock");
                }

                // 设置Unlock遮罩状态和文本
                bool isUnlocked = GetAdmissionIsUnlock(sortedData[i].ID);
                if (windPickingInfo.IsUnlocked != null)
                {
                    windPickingInfo.IsUnlocked.gameObject.SetActive(!isUnlocked);

                    // 设置锁定原因文本
                    // Sort >= 2 的卡池（第二个及以后）显示文本ID 132
                    // Sort == 1 的卡池（第一个）显示文本ID 23
                    if (windPickingInfo.unlockReasonText != null)
                    {
                        int textId = sortedData[i].Sort >= 2 ? 132 : 23;
                        LocalizationManager.SetText(windPickingInfo.unlockReasonText, textId);
                    }
                }

                // 默认隐藏选中状态
                if (windPickingInfo.Selected != null)
                {
                    windPickingInfo.Selected.gameObject.SetActive(false);
                }

                windPickingInfos.Add(windPickingInfo);
            }
        }
    }

    public UnlockSaveData GetGatherUnlockSaveData()
    {
        var gatherUnlockSaveData = new UnlockSaveData();
        bool isFishUnlock = gatherUnlockSaveData.SetGetGatherUnlockData(FishUnlockDic, PlantUnlockDic, CubeUnlockDic, DecorationUnlockDic, FacilitiesUnlockDic, AdmissionUnlockDic,TankUnlockDic);
        return isFishUnlock ? gatherUnlockSaveData : null;
    }
    public GatherStateData GetStateData()
    {
        if (stateData == null)
            return null;
        if (rewardList.Count == 0)
        {
            stateData.rewards = new Reward[0];
        }
        else
        {
            stateData.rewards = rewardList.ToArray();
        }

        stateData.StartTime = Time.time - (StartTime == 0 ? Time.time : StartTime);
        return stateData;
    }
    public void SetAddress(string addressID)
    {
        if (State == GatherState.rest)
        {
            if (GatherDataDic.ContainsKey(addressID))
            {
                GatherID = addressID;
            }
            var admissionData = GameConfigDataBase.GetConfigData<GatherAddressData>(int.Parse(addressID));
            // 价格文本：如果有价格显示价格，否则显示"免费"
            if (admissionData.Price != 0)
            {
                MoneyText.text = Tools.MoneyString(admissionData.Price);
            }
            else
            {
                LocalizationManager.SetText(MoneyText, SysDefine.Language_Free);
            }
            // 时间文本：取消本地化注册后设置动态内容
            LocalizationManager.UnregisterText(TimeText);
            TimeText.text = LocalizationManager.GetCurrentLanguageByID(21) + Tools.TimeString((int)admissionData.SustainTime);
            MoneyIconObj.SetActive(admissionData.Price != 0);
            GetAllGatherSprite();
            OpenUi();
            InitcanGetRewardListController();
        }
    }


    private bool isInit = false;
    private void OnEnableInit()
    {
        if (isInit)
            return;

        // 安全检查：确保字典已初始化，否则跳过（等待 InitStateData 调用）
        if (GatherDataDic == null || GatherDataDic.Count == 0)
        {
            return;
        }

        isInit = true;
        // 注册语言切换事件，用于更新TimeText
        event_manager.instance.add_event_listener(SysDefine.ChangeLauguageEvent, OnLanguageChanged);
        Character.Init();
        if (stateData == null || stateData.GatherID == null || stateData.GatherID == "")
        {
            stateData = new GatherStateData();
            GatherID = GatherDataDic.Keys.ToArray()[0];
            StartTime = Time.time;
            rewardList = new();
            State = GatherState.rest;
            GetAllGatherSprite();
            OpenUi();
            InitcanGetRewardListController();
        }
        else
        {
            GatherID = stateData.GatherID;
            StartTime = Time.time - stateData.StartTime;
            rewardList = new List<Reward>(stateData.rewards);
            State = stateData.gatherState;
            switch (stateData.gatherState)
            {
                case GatherState.rest:
                    GetAllGatherSprite();
                    OpenUi();
                    InitcanGetRewardListController();
                    break;
                case GatherState.perform:
                    GetAllGatherSprite();
                    OpenUi();
                    break;
                case GatherState.complete:
                    GetAllGatherSprite();
                    OpenUi();
                    break;
                case GatherState.reward:
                    GetAllGatherSprite();
                    GetAllRewardSprite();
                    OpenUi();
                    rewardListController.Init(SysDefine.PrefabAssetbundle + SysDefine.cangetprefab, AllRewardSprites.Count, InitRewardGameobject);
                    break;
                default:
                    break;
            }

        }
    }
    public void InitStateData(GatherSaveData gatherSaveData)
    {
        Instance = this;
        InitGatherData();

        if (gatherSaveData == null)
        {
            stateData = null;
            isFirstGather10001Completed = false;
            ApplyInitialUnlocks();
            EnsureInitialDataIfEmpty();
            OnEnableInit();
            Inited = true;
            return;
        }

        stateData = gatherSaveData.gatherStateData;
        isFirstGather10001Completed = gatherSaveData.isFirstGather10001Completed;
        // 兼容空存档：若未保存或字段为空，回退到初始状态
        if (stateData == null || string.IsNullOrEmpty(stateData.GatherID))
        {
            stateData = null;
        }

        var unlockdata = gatherSaveData.gatherUnlockSaveData;
        if (unlockdata != null)
        {
            unlockdata.GetGatherUnlockData(out FishUnlockDic, out PlantUnlockDic, out CubeUnlockDic, out DecorationUnlockDic, out FacilitiesUnlockDic, out AdmissionUnlockDic, out TankUnlockDic);
        }
        else
        {
            ApplyInitialUnlocks();
        }

        EnsureInitialDataIfEmpty();
        OnEnableInit();
        Inited = true;
    }

    /// <summary>
    /// 应用初始解锁数据
    /// </summary>
    private void ApplyInitialUnlocks()
    {
        var initData = GameConfigDataBase.GetConfigData<InitialData>(1);

        foreach (var fd in initData.Fish)
        {
            if (FishUnlockDic.ContainsKey(fd))
            {
                FishUnlockDic[fd] = 0;
            }
        }
        foreach (var pd in initData.Plant)
        {
            if (PlantUnlockDic.ContainsKey(pd))
            {
                PlantUnlockDic[pd] = true;
            }
        }
        foreach (var cd in initData.Cube)
        {
            if (CubeUnlockDic.ContainsKey(cd))
            {
                CubeUnlockDic[cd] = true;
            }
        }
        foreach (var ad in initData.Admission)
        {
            if (AdmissionUnlockDic.ContainsKey(ad))
            {
                AdmissionUnlockDic[ad] = true;
            }
        }
        foreach (var td in initData.Tank)
        {
            if (TankUnlockDic.ContainsKey(td))
            {
                TankUnlockDic[td] = true;
            }
        }
    }

    /// <summary>
    /// 确保空字典有初始数据
    /// </summary>
    private void EnsureInitialDataIfEmpty()
    {
        var initData = GameConfigDataBase.GetConfigData<InitialData>(1);

        if (FishUnlockDic != null && FishUnlockDic.Count == 0)
        {
            foreach (var fd in initData.Fish)
            {
                if (FishUnlockDic.ContainsKey(fd))
                {
                    FishUnlockDic[fd] = 0;
                }
            }
        }
        if (PlantUnlockDic != null && PlantUnlockDic.Count == 0)
        {
            foreach (var pd in initData.Plant)
            {
                if (PlantUnlockDic.ContainsKey(pd))
                {
                    PlantUnlockDic[pd] = true;
                }
            }
        }
        if (CubeUnlockDic != null && CubeUnlockDic.Count == 0)
        {
            foreach (var cd in initData.Cube)
            {
                if (CubeUnlockDic.ContainsKey(cd))
                {
                    CubeUnlockDic[cd] = true;
                }
            }
        }
        if (TankUnlockDic != null && TankUnlockDic.Count == 0)
        {
            foreach (var td in initData.Tank)
            {
                if (TankUnlockDic.ContainsKey(td))
                {
                    TankUnlockDic[td] = true;
                }
            }
        }
        if (AdmissionUnlockDic != null && AdmissionUnlockDic.Count == 0)
        {
            foreach (var item in gatherAddressData.Values)
            {
                AdmissionUnlockDic.Add(item.ID, false);
            }
            foreach (var ad in initData.Admission)
            {
                if (AdmissionUnlockDic.ContainsKey(ad))
                {
                    AdmissionUnlockDic[ad] = true;
                }
            }
        }
    }
    private void InitState(GatherState gatherState)
    {
        State = gatherState;
        switch (gatherState)
        {
            case GatherState.rest:
                GetAllGatherSprite();
                OpenUi();
                InitcanGetRewardListController();
                break;
            case GatherState.perform:
                break;
            case GatherState.complete:
                break;
            case GatherState.reward:
                break;
            default:
                break;
        }
    }
    private void OnEnable()
    {
        OnEnableInit();
        // 如果已经初始化过，需要刷新UI（确保界面状态正确）
        if (isInit)
        {
            OpenUi();
            // 刷新价格/免费显示
            RefreshMoneyDisplay();
        }
        UIUpdate();
    }

    /// <summary>
    /// 刷新价格/免费显示
    /// </summary>
    private void RefreshMoneyDisplay()
    {
        if (State != GatherState.rest || string.IsNullOrEmpty(GatherID))
            return;

        if (gatherAddressData == null || !gatherAddressData.ContainsKey(GatherID))
            return;

        var admissionData = gatherAddressData[GatherID];
        // 价格文本：如果有价格显示价格，否则显示"免费"
        if (admissionData.Price != 0)
        {
            MoneyText.text = Tools.MoneyString(admissionData.Price);
        }
        else
        {
            LocalizationManager.SetText(MoneyText, SysDefine.Language_Free);
        }
        MoneyIconObj.SetActive(admissionData.Price != 0);
    }

    private void InitcanGetRewardListController()
    {
        canGetRewardListController.Init(SysDefine.PrefabAssetbundle + SysDefine.cangetprefab, AllGatherSprites.Count, InitcanGetRewardObject);
        if (AllGatherSprites.Count < 16)
        {
            canGetRewardListController.scrollRect.gameObject.SetActive(false);
        }
        else
        {
            canGetRewardListController.scrollRect.gameObject.SetActive(true);
        }
    }



    private Dictionary<string, Dictionary<RewardType, Dictionary<string, IconDisplayData>>> AllGatherSpritesCache = new();
    private List<IconDisplayData> AllGatherSprites = new();
    private List<IconDisplayData> AllRewardSprites = new();

    public List<IconDisplayData> GetAllGatherSprite()
    {
        AllGatherSprites.Clear();

        if (AllGatherSpritesCache.TryGetValue(GatherID, out var cache))
        {
            GenerateSpritesFromCache(cache);
            return AllGatherSprites;
        }

        var spriteDic = BuildSpriteCache();
        AllGatherSpritesCache.Add(GatherID, spriteDic);
        GenerateSpritesFromCache(spriteDic);
        return AllGatherSprites;
    }

    private List<string> GetFishSortList()
    {
        List<string> sortedFishIDList = GatherDataDic[GatherID].FishIDList
        .Select(idStr => new { AnimalData = GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(idStr), SysDefine.AnimalConfigData) })
        .OrderBy(x => x.AnimalData.Sort) // 升序排序，如果要降序则用OrderByDescending
        .Select(x => x.AnimalData.ID)
        .ToList();
        return sortedFishIDList;
    }
    private List<string> GetPlantSortList()
    {
        List<string> sortedPlantIDList = GatherDataDic[GatherID].PlantIDList
        .Select(idStr => new { PlantData = GameConfigDataBase.GetConfigData<PlantData>(int.Parse(idStr), SysDefine.PlantConfigData) })
        .OrderBy(x => x.PlantData.Sort) // 升序排序，如果要降序则用OrderByDescending
        .Select(x => x.PlantData.ID)
        .ToList();
        return sortedPlantIDList;
    }

    public string GetGatherID()
    {
        return GatherID;
    }
    public List<Reward> GetGather()
    {
        State = GatherState.rest;
        // 清除当前卡池的图标缓存，确保下次显示时反映最新的解锁状态
        if (AllGatherSpritesCache.ContainsKey(GatherID))
        {
            AllGatherSpritesCache.Remove(GatherID);
        }
        GetAllGatherSprite();
        OpenUi();
        InitcanGetRewardListController();

        return rewardList;

    }
    public List<Reward> TestGetSomeAnimalGather(string rwid = "null")
    {
        List<Reward> rewardList = new();
        rewardList.Add(new()
        {
            rewardType = RewardType.Animal,
            rewardID = rwid,
            parameter0 = 0,
        }
        );
        return rewardList;
    }
    public List<Reward> TestGetSomePlantGather(string rwid = "null")
    {
        List<Reward> rewardList = new();
        rewardList.Add(new()
        {
            rewardType = RewardType.Plant,
            rewardID = rwid,
        }
        );
        return rewardList;
    }
    public List<Reward> TestGetAnimalGather(string gatherID = "null")
    {
        if (gatherID == "null")
            gatherID = GatherID;
        List<Reward> rewardList = new();
        foreach (var item in GatherDataDic[gatherID].FishIDList)
        {
            rewardList.Add(new()
            {
                rewardType = RewardType.Animal,
                rewardID = item,
                parameter0 = 0,
            });
        }

        return rewardList;
    }
    public List<Reward> TestGetPlantGather(string gatherID = "null")
    {
        if (gatherID == "null")
            gatherID = GatherID;
        List<Reward> rewardList = new();

        foreach (var item in GatherDataDic[gatherID].PlantIDList)
        {
            rewardList.Add(new()
            {
                rewardType = RewardType.Plant,
                rewardID = item,
            });
        }
        return rewardList;
    }
    public List<Reward> TestGetAllGather(string gatherID = "null")
    {
        if (gatherID == "null")
            gatherID = GatherID;
        List<Reward> rewardList = new();
        foreach (var item in GatherDataDic[gatherID].FishIDList)
        {
            rewardList.Add(new()
            {
                rewardType = RewardType.Animal,
                rewardID = item,
                parameter0 = 0,
            });
        }
        foreach (var item in GatherDataDic[gatherID].PlantIDList)
        {
            rewardList.Add(new()
            {
                rewardType = RewardType.Plant,
                rewardID = item,
            });
        }
        return rewardList;
    }
    /// <summary>
    /// 得到奖励
    /// </summary>
    public List<Reward> GetGather(string gatherID)
    {
        // 首次对卡池10001进行野采时，返回固定奖励
        if (gatherID == "10001" && !isFirstGather10001Completed)
        {
            isFirstGather10001Completed = true;
            List<Reward> firstRewardList = new()
            {
                new Reward
                {
                    rewardType = RewardType.Animal,
                    rewardID = "1001",
                    parameter0 = 0.5f // 品质为0.5
                },
                new Reward
                {
                    rewardType = RewardType.Animal,
                    rewardID = "1001",
                    parameter0 = 0.5f // 品质为0.5
                },
                new Reward
                {
                    rewardType = RewardType.Plant,
                    rewardID = "2001",
                    parameter0 = 0 // 成长度为0%
                }
            };
            return firstRewardList;
        }

        List<Reward> rewardList = new();
        //处理鱼
        rewardList.AddRange(FishReward(gatherID));
        //处理植物
        rewardList.AddRange(PlantReward(gatherID));
        //处理立方体
        rewardList.AddRange(CubeReward(gatherID));
        //处理装饰物
        rewardList.AddRange(DecorationReward(gatherID));
        //设备的处理
        rewardList.AddRange(FacilitiesReward(gatherID));
        //鱼缸的处理
        rewardList.AddRange(TankReward(gatherID));
        //通行证设备
        rewardList.AddRange(AdmissionReward(gatherID));
        return rewardList;
    }
    /// <summary>
    /// 开始采集
    /// </summary>
    public void StartGather()
    {
        // 播放点击音效
        AudioSourceManager.Instance.PlayButtonClip();

        if (State == GatherState.rest)
        {
            if (string.IsNullOrEmpty(GatherID))
            {
                GatherID = 10001.ToString();
            }
            //记得扣费
            var admissionData = GameConfigDataBase.GetConfigData<GatherAddressData>(int.Parse(GatherID));
            if (!PlayerManager.Instance.CheckMoneyEnough(admissionData.Price))
            {
                ToastManager.Show(85);
                return;
            }
            PlayerManager.Instance.OnMoneyChanged(admissionData.Price * -1);
            State = GatherState.perform;
            StartTime = Time.time;
            OpenUi();
        }
    }
    /// <summary>
    /// 展示奖励
    /// </summary>
    public void ShowGather()
    {
        if (State == GatherState.complete)
        {
            //记得扣费
            rewardList = GetGather(GatherID);
            State = GatherState.reward;
            // 触发野采状态变为reward事件，通知UI层（用于引导系统）
            event_manager.instance.dispatch_event(SysDefine.UI_GatherStateReward, null);
            GetAllRewardSprite();
            OpenUi();
            rewardListController.Init(SysDefine.PrefabAssetbundle + SysDefine.cangetprefab, AllRewardSprites.Count, InitRewardGameobject);

        }
    }

    public TextMeshProUGUI LeftTime;
    public void MyUpdate()
    {
        if (State == GatherState.perform/*&& (Time.time- StartTime)> gatherAddressData[GatherID].SustainTime*/)
        {
            LeftTime.text = Tools.TimeString((int)(gatherAddressData[GatherID].SustainTime - (Time.time - StartTime)));
            if ((Time.time - StartTime) > gatherAddressData[GatherID].SustainTime)
            {
                State = GatherState.complete;
                // 触发野采完成事件，通知UI层（用于引导系统）
                event_manager.instance.dispatch_event(SysDefine.UI_GatherStateComplete, null);
                OpenUi();
            }

        }
        UIUpdate();
    }
    public void Update()
    {
        if (!Inited) return;
        MyUpdate();
        if (State == GatherState.rest)
        {
            var admissionData = GetCachedAddressData();
            if (admissionData != null && admissionData.Price > 0 && !PlayerManager.Instance.CheckMoneyEnough(admissionData.Price))
            {
                MoneyText.color = Color.red;
            }
            else
            {
                MoneyText.color = Color.black;
            }
        }
    }

    /// <summary>
    /// 获取缓存的地址配置数据，避免每帧查询
    /// </summary>
    private GatherAddressData GetCachedAddressData()
    {
        // 安全检查：GatherID 为空时返回 null
        if (string.IsNullOrEmpty(GatherID))
        {
            return null;
        }
        if (_cachedGatherID != GatherID || _cachedAddressData == null)
        {
            _cachedGatherID = GatherID;
            _cachedAddressData = GameConfigDataBase.GetConfigData<GatherAddressData>(int.Parse(GatherID));
        }
        return _cachedAddressData;
    }

    /// <summary>
    /// 当玩家处于野采界面时，通过这里刷新野采进度
    /// </summary>
    public void UIUpdate()
    {
        if (State == GatherState.perform)
        {
            UpdateBackgroundPosition();
            UpdateRolePosition();
            //如果已经经历了一半时间，则中前景开始往回走
            if (Time.time - StartTime > gatherAddressData[GatherID].SustainTime / 2)
            {
                StartAnimation(Direction.Right);
            }
            else
            {
                StartAnimation(Direction.Left);
            }
        }

    }
    /// <summary>
    /// 打开界面时调用
    /// </summary>
    public void OpenUi()
    {
        //未开始
        if (info == null)
            return;
        if (State == GatherState.rest)
        {
            info.SetActive(true);
            perform.SetActive(false);
            box.SetActive(false);
            reward.SetActive(false);
            //中前景动画暂停
            StopAnimation();

            if (isNewAddmissonRewardGet)
            {
                event_manager.instance.dispatch_event(SysDefine.UI_WindPickingInfo_change,null);
                isNewAddmissonRewardGet = false;
            }
        }
        //进行中
        else if (State == GatherState.perform)
        {
            info.SetActive(false);
            perform.SetActive(true);
            box.SetActive(false);
            reward.SetActive(false);
            Character.PlayAnimation();
            //中前景开始播放移动动画
            UIUpdate();
            // 使用SetText注册，支持语言切换时自动更新
            LocalizationManager.SetText(infoText, int.Parse(gatherAddressData[GatherID].Name));
        }
        //完成未打开奖励
        else if (State == GatherState.complete)
        {
            info.SetActive(false);
            perform.SetActive(true);
            box.SetActive(true);
            reward.SetActive(false);
            LeftTime.text = Tools.TimeString(0);
            //中前景动画暂停
            StopAnimation();
            // 设置野采名称
            LocalizationManager.SetText(infoText, int.Parse(gatherAddressData[GatherID].Name));
        }
        //已打开奖励未领奖
        else
        {
            info.SetActive(false);
            perform.SetActive(true);
            reward.SetActive(true);
            StopAnimation();
            // 设置野采名称
            LocalizationManager.SetText(infoText, int.Parse(gatherAddressData[GatherID].Name));
        }
    }


    public void StartAnimation(Direction direction)
    {
        if (direction == Direction.Left)
        {
            Character.SetForward();
        }
        else
        {
            Character.SetBackward();
        }

        Foreground.SetScrollDirection(direction);
        Midground.SetScrollDirection(direction);
        Foreground.StartScroll();
        Midground.StartScroll();
    }
    public void StopAnimation()
    {
        Character.stopPlay();
        Foreground.StopScroll();
        Midground.StopScroll();
    }
    /// <summary>
    /// 更新后景位置
    /// </summary>
    public void UpdateBackgroundPosition()
    {
        //经历的时间
        var time = Time.time - StartTime;
        //野采所需时间
        var sustainTime = gatherAddressData[GatherID].SustainTime;
        //前往
        if (time <= sustainTime / 2)
        {
            float x = -time / (sustainTime / 2) * 400;
            Background.transform.localPosition = new Vector3(x, 0, 0);
        }
        //返回
        else if (time <= sustainTime)
        {
            float x = (time / (sustainTime / 2) - 1) * 400 - 400;
            Background.transform.localPosition = new Vector3(x, 0, 0);
        }
        else
        {
            Background.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
    /// <summary>
    /// 更新角色位置
    /// </summary>
    public void UpdateRolePosition()
    {
        //经历的时间
        var time = Time.time - StartTime;
        //野采所需时间
        var sustainTime = gatherAddressData[GatherID].SustainTime;
        //角色的宽度
        int width = (int)Character.GetSize().x;
        //前往
        if (time <= sustainTime / 2)
        {
            float x = (time / (sustainTime / 2)) * (400 + width) - width;
            Role.transform.localPosition = new Vector3(x, Role.transform.localPosition.y, Role.transform.localPosition.z);
        }
        //返回
        else
        {
            float x = 400 + width - (time / (sustainTime / 2) - 1) * (400 + width);
            Role.transform.localPosition = new Vector3(x, Role.transform.localPosition.y, Role.transform.localPosition.z);
        }
    }
}
