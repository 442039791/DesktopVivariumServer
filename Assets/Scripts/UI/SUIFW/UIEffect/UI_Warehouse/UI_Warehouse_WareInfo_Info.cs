using Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Warehouse_WareInfo_Info : ChangeLanugeBase
{
    public bool TestID;
    public TextMeshProUGUI TestIDText;
    public TextMeshProUGUI TestFishObjID;
    public Image Hp;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI MoneyText;
    public Icon _Icon;
    public Transform FishComponent;
    public TextMeshProUGUI FishGrow;
    public TextMeshProUGUI FishFood;
    //public TextMeshProUGUI JuvenileCount;
    //public TMP_InputField AdultInput;
    private WarehouseBaseData _data;

    // 依赖注入
    private IWarehouseService _warehouseService;
    //private int _AdultInputcount;
    //private int AdultInputcount
    //{
    //    get { return _AdultInputcount; }
    //    set {
    //        var fdata = _data as WarehouseFishStateData;
    //        if (fdata == null)
    //            return;
    //        if (value >= fdata.stateData.AdultNum)
    //        {
    //            value= (int)fdata.stateData.AdultNum;
    //        }
    //        if (value < 0)
    //        {
    //            value = 0;
    //        }
    //        _AdultInputcount = value;
    //        AdultInput.text = _AdultInputcount.ToString();
    //    }
    //}
    //public Button AdultDecrease;
    //public Button AdultIncrease;
    //public TMP_InputField JuvenileInput;
    //private int _JuvenileInputcount;
    //private int JuvenileInputcount
    //{
    //    get { return _JuvenileInputcount; }
    //    set
    //    {
    //        var fdata = _data as WarehouseFishStateData;
    //        if (fdata == null)
    //            return;
    //        if (value >= fdata.stateData.JuvenileNum)
    //        {
    //            value = (int)fdata.stateData.JuvenileNum;
    //        }
    //        if (value < 0)
    //        {
    //            value = 0;
    //        }
    //        _JuvenileInputcount = value;
    //        JuvenileInput.text = _JuvenileInputcount.ToString();
    //    }
    //}
    //public Button JuvenileDecrease;
    //public Button JuvenileIncrease;
    public Button AdultSell;
    //public Button JuvenilePut;
    //public Button JuvenileSell;
    public Transform PlantComponent;
    public TextMeshProUGUI PlantGorw;
    public GameObject PlantGorwObj;
    public Button PlantRemove;
    public Button Place;
    public void Init(WarehouseBaseData data)
    {
        // 初始化依赖注入
        if (_warehouseService == null)
        {
            _warehouseService = ServiceLocator.Get<IWarehouseService>();
        }

        _data = data;
        if (TestID)
            TestIDText.text = data.WarehouseBaseID.ToString();

        switch (data.DataType)
        {
            case WarehouseBaseDataType.Fish:
                var fdata = data as WarehouseAnimalStateData;
                if (fdata == null)
                    return;
                _Icon.Init(RewardType.Animal, fdata.stateData.Type);
                MoneyText??= transform.Find("MoneyText").GetComponent<TextMeshProUGUI>();
                FishGrow??= transform.Find("FishGrow").GetComponent<TextMeshProUGUI>();
                FishFood??= transform.Find("FishFood").GetComponent<TextMeshProUGUI>();
                NameText??= transform.Find("NameText").GetComponent<TextMeshProUGUI>();
                Hp??= transform.Find("Hp").GetComponent<Image>();
                AdultSell??= transform.Find("Sell").GetComponent<Button>();
                Place??= transform.Find("Place").GetComponent<Button>();
                if (PlantComponent != null)
                    PlantComponent?.gameObject.SetActive(false);
                if (TestID)
                    TestFishObjID.text = fdata.stateData.ObjID.ToString();
                if (MoneyText != null)
                    MoneyText.text = ((int)fdata.stateData.GetPrice()).ToString();
                int num = fdata.stateData.GetAnimalData().adultTime * 60 - (int)(fdata.stateData.GrowUpNum);
                if (num <= 0)
                {
                    LocalizationManager.SetText(FishGrow, SysDefine.UI_Language_GorwMax_ID);
                }
                else
                {
                    FishGrow.text = ((int)(fdata.stateData.GrowUpNum * 100 /(fdata.stateData.GetAnimalData().adultTime*60) ) ).ToString() + "%" + "(" + Tools.TimeString(num) + ")";
                    //FishGrow.text = (num).ConvertSecondsToHourSeconds();
                }
                FishFood.text = ((int)fdata.stateData.FoodReserve).ToString() + "/" + ((int)fdata.stateData.GetAnimalData().foodReserve).ToString();
                _Icon._Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(fdata.stateData.GetObjectQuality());
                // 使用SetText注册，支持语言切换时自动更新
                LocalizationManager.SetText(NameText, int.Parse(fdata.stateData.GetAnimalData().name));
                float sl = fdata.stateData.HP / 100;
                Hp.fillAmount = sl;
                if (sl > 0.67f)
                {
                    Hp.color = Color.green;
                }
                else if (sl > 0.33f)
                {
                    Hp.color = Color.yellow;
                }
                else
                {
                    Hp.color = Color.red;
                }
                //UI_Warehouse_WareInfo_Info_Count.text = "x" + fdata.stateData.AdultNum;
                //JuvenileCount.text = "x" + fdata.stateData.JuvenileNum;
                //AdultInputcount = 0;
                //AdultInput.text = AdultInputcount.ToString();
                //JuvenileInputcount = 0;
                //JuvenileInput.text = JuvenileInputcount.ToString();
                //AdultInput.onValueChanged.RemoveAllListeners();
                //AdultInput.onValueChanged.AddListener((v) =>
                //{
                //  //if(int.TryParse(v, out int num))
                //  //  {
                //  //    AdultInputcount = num;
                //  //  }
                //  //  else
                //  //  {
                //  //      AdultInputcount = 0;
                //  //  }
                //});
                //JuvenileInput.onValueChanged.RemoveAllListeners();
                //JuvenileInput.onValueChanged.AddListener((v) =>
                //{
                //    if (int.TryParse(v, out int num))
                //    {
                //        JuvenileInputcount = num;
                //    }
                //    else
                //    {
                //        JuvenileInputcount = 0;
                //    }
                //});
                //AdultDecrease.AddDeskTopListener(() =>
                //{
                //    AdultInputcount -= 1;
                //});
                //AdultIncrease.AddDeskTopListener(() =>
                //{
                //    AdultInputcount += 1;
                //});
                //JuvenileDecrease.AddDeskTopListener(() =>
                //{
                //    JuvenileInputcount -= 1;
                //});
                //JuvenileIncrease.AddDeskTopListener(() =>
                //{
                //    JuvenileInputcount += 1;
                //});
                Place.AddDeskTopListener(() =>
                {
                    var activeBioTank = BioTankRegistry.Instance.ActiveBioTank;
                    if (activeBioTank == null)
                    {
                        ToastManager.Show("没有活动的生态箱");
                        return;
                    }

                    var isBioTankFull = activeBioTank.IsBioTankFull();
                    if (isBioTankFull)
                    {
                        ToastManager.Show(87);
                        return;
                    }
                    // 当选择物品放置时，退出删除模式
                    if (UI_BioTank_Edit.CurrentInstance != null)
                    {
                        UI_BioTank_Edit.CurrentInstance.handle.ExitDeleteMode();
                    }
                    _warehouseService.PlaceItem(_data, 1, true);
                });
                AdultSell?.AddDeskTopListener(() =>
                {
                    _warehouseService.PlaceItem(_data, 1, true, true);
                });
                //JuvenileSell.AddDeskTopListener(() =>
                //{
                //    _warehouseService.PlaceItem(_data, 0, false,true);
                //});
                //JuvenilePut.AddDeskTopListener(() =>
                //{
                //    _warehouseService.PlaceItem(_data, 0,false);
                //});
                break;
            case WarehouseBaseDataType.Plant:
                var pdata = data as WarehousePlantStateData;
                if (pdata == null)
                    return;
                _Icon.Init(RewardType.Plant, pdata.data.Type);
                if (MoneyText != null)
                    MoneyText.text = ((int)pdata.data.GetPrice()).ToString();
                _Icon._Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White);
                MoneyText ??= transform.Find("MoneyText").GetComponent<TextMeshProUGUI>();
                FishGrow ??= transform.Find("FishGrow").GetComponent<TextMeshProUGUI>();
                FishFood ??= transform.Find("FishFood").GetComponent<TextMeshProUGUI>();
                NameText ??= transform.Find("NameText").GetComponent<TextMeshProUGUI>();
                Hp ??= transform.Find("Hp").GetComponent<Image>();
                AdultSell ??= transform.Find("Sell").GetComponent<Button>();
                Place ??= transform.Find("Place").GetComponent<Button>();
                // 使用SetText注册，支持语言切换时自动更新
                LocalizationManager.SetText(NameText, int.Parse(pdata.data.GetPlantData().name));
                if (PlantComponent != null)
                {
                    PlantComponent?.gameObject.SetActive(true);
                    //PlantGorw?.gameObject.SetActive(true);
                    //PlantGorwObj?.SetActive(true);
                    //UI_Warehouse_WareInfo_Info_Count.text = null;

                    //PlantGorw.text = pdata.data.GrowthProgress;
                }
                float psl = pdata.data.HP / 100;
                Hp.fillAmount = psl;
                if (psl > 0.67f)
                {
                    Hp.color = Color.green;
                }
                else if (psl > 0.33f)
                {
                    Hp.color = Color.yellow;
                }
                else
                {
                    Hp.color = Color.red;
                }
                if (pdata.data.GetGrowthProgress() == 1)
                {
                    PlantGorw.text = "100%";
                }
                else
                {
                    PlantGorw.text = ((int)(pdata.data.GetGrowthProgress()*100)).ToString() + "%" + "(" + Tools.TimeString(pdata.data.GetGrowthTime()) + ")";
                    //PlantGorw.text = (pnum).ConvertSecondsToHourSeconds();
                }
                PlantRemove.AddDeskTopListener(() =>
                {
                    _warehouseService.PlaceItem(_data, 1, false, true);
                });
                break;
            case WarehouseBaseDataType.Decoration:
                var ddata = data as WarehouseDecorationStateData;
                _Icon._Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White);
                if (ddata == null)
                    return;

                // 隐藏Fish和Plant特有的组件
                if (FishComponent != null)
                    FishComponent.gameObject.SetActive(false);
                if (PlantComponent != null)
                    PlantComponent.gameObject.SetActive(false);

                // 显示Decoration信息
                if (MoneyText != null)
                    MoneyText.text = ((int)ddata.stateData.GetPrice()).ToString();
                break;
            case WarehouseBaseDataType.Cube:
                _Icon._Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.White);

                var cdata = data as WarehouseCubeStateData;
                if (cdata == null)
                    return;

                // 隐藏Fish和Plant特有的组件
                if (FishComponent != null)
                    FishComponent.gameObject.SetActive(false);
                if (PlantComponent != null)
                    PlantComponent.gameObject.SetActive(false);

                // 显示Cube信息
                if (MoneyText != null)
                    MoneyText.text = ((int)cdata.stateData.GetPrice()).ToString();
                break;

            default:
                break;
        }
        //UI_Warehouse_WareInfo_Info_Count.text = "x"+data.Count;

        // 动态获取Icon，避免使用可能有HideFlags问题的缓存Sprite
        if (data.Icon != null)
        {
            _Icon._Icon.sprite = data.Icon;
        }
        else
        {
            // 如果Icon未设置，根据数据类型动态获取
            switch (data.DataType)
            {
                case WarehouseBaseDataType.Fish:
                    var fishData = (data as WarehouseAnimalStateData);
                    if (fishData != null)
                    {
                        var animalData = fishData.stateData.GetAnimalData();
                        _Icon._Icon.sprite = animalData.GetSprite(fishData.stateData.GetIsAdult(), fishData.stateData.GetObjectQuality());
                    }
                    break;
                case WarehouseBaseDataType.Plant:
                    var plantData = (data as WarehousePlantStateData);
                    if (plantData != null)
                    {
                        _Icon._Icon.sprite = plantData.data.GetPlantData().GetIconSprite();
                    }
                    break;
                case WarehouseBaseDataType.Decoration:
                    var decoData = (data as WarehouseDecorationStateData);
                    if (decoData != null)
                    {
                        _Icon._Icon.sprite = decoData.stateData.GetDecorationData().GetIconSprite();
                    }
                    break;
                case WarehouseBaseDataType.Cube:
                    var cubeData = (data as WarehouseCubeStateData);
                    if (cubeData != null)
                    {
                        _Icon._Icon.sprite = cubeData.stateData.GetCubeData().GetIconSprite();
                    }
                    break;
            }
        }
        //UI_Warehouse_WareInfo_Info_NewICon?.gameObject.SetActive(data.New);
        Place.AddDeskTopListener(() =>
        {
            var activeBioTank = BioTankRegistry.Instance.ActiveBioTank;
            if (activeBioTank == null)
            {
                ToastManager.Show("没有活动的生态箱");
                return;
            }

            // 只有放置动物时才检查生态箱空间是否足够
            // 植物、装饰物、方块不需要检查空间
            if (_data.DataType == WarehouseBaseDataType.Fish)
            {
                var isBioTankFull = activeBioTank.IsBioTankFull();
                if (isBioTankFull)
                {
                    ToastManager.Show(87);
                    return;
                }
            }

            // 当选择物品放置时，退出删除模式
            if (UI_BioTank_Edit.CurrentInstance != null)
            {
                UI_BioTank_Edit.CurrentInstance.handle.ExitDeleteMode();
            }
            _warehouseService.PlaceItem(_data, -1);
        });
        base.Init();
    }
    //public void on_event_handler(string obj)
    //{
    //    //if (!gameObject.activeInHierarchy)
    //    //    return;
    //    //string info = (string)obj;
    //    switch (obj)
    //    {
    //        //case "JuvenileNum":
    //        //    JuvenileNumChange.text = ((int)fish.JuvenileNum).ToString();
    //        //    break;
    //        //case "Hp":
    //        //    float sl = fish.HP / 100;
    //        //    Hp.fillAmount = sl;
    //        //    if (sl > 0.67f)
    //        //    {
    //        //        Hp.color = Color.green;
    //        //    }
    //        //    else if (sl > 0.33f)
    //        //    {
    //        //        Hp.color = Color.yellow;
    //        //    }
    //        //    else
    //        //    {
    //        //        Hp.color = Color.red;
    //        //    }
    //        //    break;
    //        case "Grow":
    //            int num = SysDefine.FishMaxFishGrow * 60 - (int)(fish.GrowUpNum * 60);

    //            //num = Mathf.Min(num, 0);
    //            if (num <= 0)
    //            {
    //                GrowChange.text = "�ѳ���";
    //            }
    //            else
    //            {
    //                GrowChange.text = (num).ConvertSecondsToHourSeconds();
    //            }

    //            break;
    //        case "FoodReserve":
    //            FoodReserveChange.text = ((int)fish.FoodReserve).ToString() + "/" + ((int)fish.data.foodReserve).ToString();
    //            break;
    //            //case "DeadFish":
    //            //    ChangeState(fish.State);
    //            //    break;
    //    }
    //}
}
