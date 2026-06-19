using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BioTankInfo_CreatureInfo_FacilityInfo : ChangeLanugeBase
{
    private FacilitiesStateData facility;
    public Image image;
    public Image Frame;
    public TextMeshProUGUI FacilityName;
    public TextMeshProUGUI FacilityNameChange;
    public TextMeshProUGUI FacilityDesc;

    public GameObject PriceObj;
    public TextMeshProUGUI FacilityPrice;

    public TextMeshProUGUI FacilityStatus;
    public Button BuyItem;
    public Button ActiveItem;
    public Button CloseItem;
    public bool InitCompelte = false;

    public GameObject AutoSellFishPreviewObj;
    public List<GameObject> AutoSellFishPreviewList = new();
    public Button UpFlagBtn;
    public Button DownFalgBtn;

    public GameObject SellFishPanelObj;
    private List<GameObject> SellFishList = new();
    public GameObject SellFishPre;


    public new void ChangeLanuge(string name, object udata)
    {
    }

    public void Init(FacilitiesStateData _facility)
    {
        facility = _facility;
        image.sprite = facility.GetFacilityData().GetIconSprite();
        // 使用SetText注册，支持语言切换时自动更新
        LocalizationManager.SetText(FacilityNameChange, int.Parse(facility.GetFacilityData().Name));
        LocalizationManager.SetText(FacilityDesc, int.Parse(facility.GetFacilityData().Describe));
        FacilityPrice.text = Tools.MoneyString(facility.GetPrice());
        BuyItem.gameObject.SetActive(facility.State == FacilityState.NoBuy);
        PriceObj.gameObject.SetActive(facility.State == FacilityState.NoBuy);
        ActiveItem.gameObject.SetActive(facility.State == FacilityState.NoActivation);
        CloseItem.gameObject.SetActive(facility.State == FacilityState.Activation);

        switch (facility.State)
        {
            case FacilityState.Unlock:
                break;
            case FacilityState.NoBuy:
                FacilityStatus.text = string.Empty;
                break;
            case FacilityState.NoActivation:
                AutoSellFishPreviewObj.SetActive(facility.Id == 5002);
                FacilityStatus.text = LocalizationManager.GetCurrentLanguageByID(144);
                FacilityStatus.color = Color.black;
                break;
            case FacilityState.Activation:
                AutoSellFishPreviewObj.SetActive(facility.Id == 5002);
                FacilityStatus.text = LocalizationManager.GetCurrentLanguageByID(82);
                FacilityStatus.color = new Color32(22,212,0,255);
                break;
        }
        SellFishPre.SetActive(false);

        if (facility.Id == 5002)
        {
            RefreshAutoSellFishPreview();
        }

        if (!InitCompelte)
        {
            event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
            BuyItem.onClick.AddListener(() =>
            {
                // 先检查费用是否足够
                if (!PlayerManager.Instance.CheckMoneyEnough(facility.GetPrice()))
                {
                    // 费用不足，显示提示
                    ToastManager.Show(134);
                    return;
                }
                // 费用足够，扣费并购买设备
                PlayerManager.Instance.OnMoneyChanged(-facility.GetPrice());
                FacilityManager.Instance.RefreshBioTankFacilityStatus(facility.ObjID, facility.Id,
                    FacilityState.NoActivation);
                BioTankRegistry.Instance.GetBioTank(facility.ObjID)?.RefreshBitTankInfo();
            });
            ActiveItem.onClick.AddListener(() =>
            {
                FacilityManager.Instance.RefreshBioTankFacilityStatus(facility.ObjID, facility.Id,
                    FacilityState.Activation);
                BioTankRegistry.Instance.GetBioTank(facility.ObjID)?.RefreshBitTankInfo();
            });
            CloseItem.onClick.AddListener(() =>
            {
                FacilityManager.Instance.RefreshBioTankFacilityStatus(facility.ObjID, facility.Id,
                    FacilityState.NoActivation);
                BioTankRegistry.Instance.GetBioTank(facility.ObjID)?.RefreshBitTankInfo();
            });
            UpFlagBtn.onClick.AddListener(() =>
            {
                UpFlagBtn.gameObject.SetActive(false);
                DownFalgBtn.gameObject.SetActive(true);
                SellFishPanelObj.SetActive(true);
                RefreshAutoSellFishList();
            });
            DownFalgBtn.onClick.AddListener(() =>
            {
                DownFalgBtn.gameObject.SetActive(false);
                UpFlagBtn.gameObject.SetActive(true);
                SellFishPanelObj.SetActive(false);
                RefreshAutoSellFishPreview();
            });
        }

        InitCompelte = true;
    }

    public void RefreshAutoSellFishPreview()
    {
        if (facility.Id != 5002)
            return;
        foreach (var fishInfoObj in AutoSellFishPreviewList)
        {
            fishInfoObj.SetActive(false);
        }

        var autoSellFishInfos = FacilityManager.Instance.GetBioTankFacilityStatus(facility.ObjID, facility.Id);
        for (int i = 0; i < autoSellFishInfos.Count; i++)
        {
            AutoSellFishPreviewList[i].SetActive(true);
            var frame = AutoSellFishPreviewList[i].transform.Find("frame").GetComponent<Image>();
            frame.sprite = CoreSetting.Instance.GetQualityBgSprite(autoSellFishInfos[i].Quality);
            var icon = AutoSellFishPreviewList[i].transform.Find("icon").GetComponent<Image>();
            var fishData =
                GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(autoSellFishInfos[i].ID),
                    SysDefine.AnimalConfigData);
            icon.sprite = fishData.GetIconSprite(autoSellFishInfos[i].Quality,
                autoSellFishInfos[i].State == AnimalAgeStatus.Adult);
        }
    }

    public void RefreshAutoSellFishList()
    {
        foreach (var sellFishObj in SellFishList)
        {
            Destroy(sellFishObj);
        }

        SellFishList.Clear();

        // var allFish = PlayerManager.Instance.BioTanks[facility.ObjID].fishList.Values.ToList();
        // var fishArray = allFish.GroupBy(f => new { f.stateData.Type, f.FishObjectQuality, f.State })
        //     .Select(g => new AutoSellFishInfo
        //     {
        //         ID = g.Key.Type,
        //         Quality = g.Key.FishObjectQuality,
        //         State = g.Key.State
        //     })
        //     .OrderBy(fish => fish.ID)
        //     .ThenBy(fish => (int)fish.State)
        //     .ThenBy(fish => (int)fish.Quality)
        //     .ToList();
        // var result = fishArray
        //     .Where(fish => fish.State is AnimalAgeStatus.Juvenile or AnimalAgeStatus.Adult).ToList();

        var qualities = new[]
        {
            ObjectQuality.White,
            ObjectQuality.Blue,
            ObjectQuality.Purple,
            ObjectQuality.Gold
        };
        var ageStates = new[] { AnimalAgeStatus.Juvenile, AnimalAgeStatus.Adult };
        var bioTank = BioTankRegistry.Instance.GetBioTank(facility.ObjID);
        if (bioTank == null) return;
        var allFish = bioTank.AnimalCollection.Entities.ToList();
        var fishIds = allFish.Select(f => f.stateData.Type).Distinct();
        var result = new List<AutoSellFishInfo>();
        foreach (var id in fishIds)
        {
            foreach (var state in ageStates)
            {
                foreach (var quality in qualities)
                {
                    result.Add(new AutoSellFishInfo
                    {
                        ID = id,
                        Quality = quality,
                        State = state
                    });
                }
            }
        }

        foreach (var autoSellInfo in result)
        {
            var sellFishObj = GameObject.Instantiate(SellFishPre, SellFishPre.transform.parent, false);
            sellFishObj.SetActive(true);
            var frame = sellFishObj.transform.Find("Icon/frame").GetComponent<Image>();
            frame.sprite = CoreSetting.Instance.GetQualityBgSprite(autoSellInfo.Quality);
            var icon = sellFishObj.transform.Find("Icon/icon").GetComponent<Image>();
            var fishData =
                GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(autoSellInfo.ID),
                    SysDefine.AnimalConfigData);
            icon.sprite = fishData.GetIconSprite(autoSellInfo.Quality,
                autoSellInfo.State == AnimalAgeStatus.Adult);
            var isExist = FacilityManager.Instance.IsAutoSellFishInfoExist(facility.ObjID, facility.Id,
                autoSellInfo.ID, autoSellInfo.State, autoSellInfo.Quality);
            sellFishObj.transform.Find("selected").gameObject.SetActive(isExist);
            sellFishObj.transform.Find("unSelected").gameObject.SetActive(!isExist);
            sellFishObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (FacilityManager.Instance.IsAutoSellFishInfoExist(facility.ObjID, facility.Id, autoSellInfo.ID,
                        autoSellInfo.State, autoSellInfo.Quality))
                {
                    FacilityManager.Instance.RemoveBioTankAutoSellAnimalInfo(facility.ObjID, facility.Id,
                        autoSellInfo.ID, autoSellInfo.State, autoSellInfo.Quality);
                    sellFishObj.transform.Find("selected").gameObject.SetActive(false);
                    sellFishObj.transform.Find("unSelected").gameObject.SetActive(true);
                }
                else
                {
                    FacilityManager.Instance.AddBioTankAutoSellFishInfo(facility.ObjID, facility.Id,
                        autoSellInfo.ID, autoSellInfo.State, autoSellInfo.Quality);
                    sellFishObj.transform.Find("selected").gameObject.SetActive(true);
                    sellFishObj.transform.Find("unSelected").gameObject.SetActive(false);
                }
            });
            var colorTxtId = 92 + (int)autoSellInfo.Quality;
            var descText = sellFishObj.transform.Find("Desc").GetComponent<TextMeshProUGUI>();
            // 应用当前字体到动态创建的文本组件
            LocalizationManager.ApplyCurrentFont(descText);
            int stateTextId = autoSellInfo.State == AnimalAgeStatus.Adult ? 145 : 146;
            string stateText = LocalizationManager.GetCurrentLanguageByID(stateTextId);
            descText.text =
                LocalizationManager.GetCurrentLanguageByID(colorTxtId) +
                stateText +
                LocalizationManager.GetCurrentLanguageByID(int.Parse(fishData.name));
            SellFishList.Add(sellFishObj);
        }
    }

    public void Update()
    {
        if (facility.State== FacilityState.NoBuy)
        {
            if (!PlayerManager.Instance.CheckMoneyEnough(facility.GetPrice()))
            {
                FacilityPrice.color = Color.red;
            }
            else
            {
                FacilityPrice.color = Color.black;
            }
        }
    }
}
