

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
//using WebSocketSharp;

public class UI_BioTankInfo_CreatureInfo_AnimalInfo : ChangeLanugeBase
{
    public Animal fish;
    /// <summary>
    /// 物品图标
    /// </summary>
    public Icon _Icon;
    public Image Hp;
    public GameObject HpGameObject;
    public TextMeshProUGUI FishNameChange;
    public TextMeshProUGUI FoodReserveChange;
    public TextMeshProUGUI GrowChange;
    public TextMeshProUGUI Money;
    public Transform DeadFishGameobj;
    public Transform SaveFishObj;
    public Button Save;
    public Button Sell;

    public Button Clear;
    public Button SelectItem;
    public Image SelectIcon;
    public bool InitCompelte=false;
    public int fid=-1;
    public bool OnShow { get; set; }

    // 依赖注入
    private IWarehouseService _warehouseService;
    public void Select(string name, object udata)
    {
        if(udata != null)
        {
            fid = (int)udata;
            if(fid == fish.ID)
            {
                SelectIcon.color = new Color32(224, 224, 224, 255);
                if (UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.ContainsKey(fid))
                {
                    UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[fid] = true;
                }
                else
                {
                    UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.Add(fid, true);
                }
            }
        }
        SelectIcon.color = Color.white;
    }
    public new void ChangeLanuge(string name, object udata)
    {
        throw new System.NotImplementedException();
    }
    private void ChangeState(AnimalAgeStatus ageStatus)
    {
        switch (ageStatus)
        {
            case AnimalAgeStatus.AdultDeath:
            case AnimalAgeStatus.JuvenileDeath:
                DeadFishGameobj.gameObject.SetActive(true);
                SaveFishObj.gameObject.SetActive(false);
                break;
            default:
                DeadFishGameobj.gameObject.SetActive(false);
                SaveFishObj.gameObject.SetActive(true);
                break;
        }
    }
    public void Init(Animal fish)
    {
        // 初始化依赖注入
        if (_warehouseService == null)
        {
            _warehouseService = ServiceLocator.Get<IWarehouseService>();
        }

        this.fish = fish;
        Money.text = ((int)this.fish.stateData.GetPrice()).ToString();
        ChangeState(fish.State);

        _Icon.Init(RewardType.Animal,fish.data.ID);

        SelectIcon = SelectItem.transform.GetComponent<Image>();
        if (!UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.ContainsKey(this.fish.ID))
        {
            UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.Add(this.fish.ID, false);
        }
        if (UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.fish.ID])
        {
            SelectIcon.color = new Color32(224, 224, 224, 255);
        }
        else
        {
            SelectIcon.color = Color.white;
        }
        //AdultInput.onValueChanged.RemoveAllListeners();
        //AdultInput.onValueChanged.AddListener((v) =>
        //{
        //    if (int.TryParse(v, out int num))
        //    {
        //        AdultInputcount = num;
        //    }
        //    else
        //    {
        //        AdultInputcount = 0;
        //    }
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
        //TODO:默认大鱼AddNewTextByBeforeSymbol
        on_event_handler("null", "Hp");
        on_event_handler("null", "Grow");
        on_event_handler("null", "FoodReserve");
        _Icon._Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(fish.stateData.GetObjectQuality());
        _Icon._Icon.sprite = fish.data.GetIconSprite(fish.stateData.GetObjectQuality(), fish.stateData.GetIsAdult());
        // 使用SetText注册，支持语言切换时自动更新
        LocalizationManager.SetText(FishNameChange, int.Parse(fish.data.name));
        //GrowChange.text = ((int)fish.GrowUpNum).ToString();
        //JuvenileNumChange.text=((int)fish.JuvenileNum).ToString();
        //AdultNumChange.text = ((int)fish.AdultNum).ToString();
        //FoodReserveChange.text = ((int)fish.FoodReserve).ToString();
        //DeadFishChange.text = (fish.AdultDeath + fish.JuvenileDeath).ToString();
        if(!InitCompelte)
        {
            SelectIcon = SelectItem.transform.GetComponent<Image>();
            event_manager.instance.add_event_listener(SysDefine.UI_CreatureInfo_Select, Select);
            event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
            SelectItem.AddDeskTopListener(() =>
            {
                if (!UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.ContainsKey(this.fish.ID))
                {
                    UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.Add(this.fish.ID, false);
                }
                UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.fish.ID] = !UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.fish.ID];
                bool isSelect = UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.fish.ID];
                if (UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.fish.ID])
                {
                    WebSocketServerManager.GetActiveClient()?.SendOutlinableModeCommand(this.fish.ID, OutlinableMode.OnlySelect);
                }
                else
                {
                    WebSocketServerManager.GetActiveClient()?.SendOutlinableModeCommand(this.fish.ID, OutlinableMode.Disable);
                }
                event_manager.instance.dispatch_event(SysDefine.UI_CreatureInfo_Select, null);
                
                foreach (var key in UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.Keys.ToList())
                {
                    UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[key] = false;
                }
                UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.fish.ID] = isSelect;
                if (UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.fish.ID])
                    SelectIcon.color = new Color32(224, 224, 224, 255);

                //event_manager.instance.dispatch_UIevent(this.fish.stateData.ID + SysDefine.UI_BioTankInfo_ButtonGroupEvent, new BioTankInfo_ButtonGroupMessage { name = SysDefine.FishName, obj = this });
            });

            this.Clear.AddDeskTopListener(() =>
            {
                this.fish.Destroy();
                //if(this.fish == null)
                //{
                //    return;
                //}

                //this.fish.AdultDeath = 0;
                //this.fish.JuvenileDeath = 0;

                //if(this.fish.AdultNum==0&& this.fish.JuvenileNum==0)
                //{
                //    this.fish.Destroy();
                //    //this.fish = null;
                //}
                //JuvenileInputcount = JuvenileInputcount;
                //AdultInputcount = AdultInputcount;
            });
            this.Save.AddDeskTopListener(() =>
            {
                _warehouseService.SaveAnimal(this.fish.stateData, this.fish.stateData.GetIsAdult(), true);
                this.fish.Destroy();
            });
            Sell.AddDeskTopListener(() =>
            {
                PlayerManager.Instance.OnMoneyChanged(this.fish.stateData.GetPrice());
                this.fish.Destroy();
            });
            //TODO:
            //DesktopButtonManager.instance.AddListener(Clear, fish.);
        }

        fid = fish.ID;
        InitCompelte = true;

    }

    public void on_event_handler(string name, object obj)
    {
        if (!gameObject.activeInHierarchy)
            return;
        string info = (string)obj;
        switch (info)
        {
            //case "JuvenileNum":
            //    JuvenileNumChange.text = ((int)fish.JuvenileNum).ToString();
            //    break;
            case "Hp":
                float sl = fish.HP / 100;
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
                break;
            case "Grow":
                int num = fish.data.adultTime * 60 - (int)(fish.GrowUpNum);
                
                //num = Mathf.Min(num, 0);
                if(num<=0)
                {
                    LocalizationManager.SetText(GrowChange, SysDefine.UI_Language_GorwMax_ID);
                    //image.sprite = fish.data.GetIconSprite(fish.stateData.GetObjectQuality(), true);
                }
                else
                {
                    GrowChange.text = ((int)(fish.GrowUpNum * 100 / (fish.data.adultTime*60)) ).ToString() + "%" + "(" +Tools.TimeString(num)  + ")"; /*(num).ConvertSecondsToHourSeconds();*/
                    //image.sprite = fish.data.GetIconSprite(fish.stateData.GetObjectQuality(), false);
                }
               
                break;
            case "FoodReserve":
                FoodReserveChange.text = ((int)fish.FoodReserve).ToString() + "/" +((int)fish.data.foodReserve).ToString();
                break;
            case "GrowUp":
                _Icon._Icon.sprite = fish.data.GetIconSprite(fish.stateData.GetObjectQuality(), fish.stateData.GetIsAdult());
                break;
            case "DeadFish":
                Money.text = ((int)this.fish.stateData.GetPrice()).ToString();
                ChangeState(fish.State);
                break;
        }
    }

    // 数据刷新间隔
    private float refreshInterval = 1f;
    private float lastRefreshTime;
    private AnimalAgeStatus lastState;

    public void Update()
    {
        // 对象池复用时，CreateObject会先SetActive(true)，然后才调用Init设置fish
        // 因此需要检查fish是否已初始化
        if (fish == null)
            return;

        // 定时刷新数据
        if (Time.time - lastRefreshTime >= refreshInterval)
        {
            lastRefreshTime = Time.time;
            RefreshAllData();
        }

        // 成长进度需要实时更新
        if (fish.State== AnimalAgeStatus.Juvenile)
        {
            int num = fish.data.adultTime * 60 - (int)fish.GrowUpNum;
            if (num <= 0)
            {
                LocalizationManager.SetText(GrowChange, SysDefine.UI_Language_GorwMax_ID);
                //image.sprite = fish.data.GetIconSprite(fish.stateData.GetObjectQuality(), true);
            }
            else
            {
                GrowChange.text = ((int)(fish.GrowUpNum * 100 / (fish.data.adultTime*60))).ToString() + "%" + "(" + Tools.TimeString(num) + ")"; /*(num).ConvertSecondsToHourSeconds();*/
                //image.sprite = fish.data.GetIconSprite(fish.stateData.GetObjectQuality(), false);
            }
        }
        bool shouldShowHp = fish.HP > 0 && fish.HP < 100;
        if (HpGameObject.activeSelf != shouldShowHp)
        {
            HpGameObject.SetActive(shouldShowHp);
        }

        // 检测状态变化（死亡等）
        if (lastState != fish.State)
        {
            lastState = fish.State;
            Money.text = ((int)this.fish.stateData.GetPrice()).ToString();
            ChangeState(fish.State);
            _Icon._Icon.sprite = fish.data.GetIconSprite(fish.stateData.GetObjectQuality(), fish.stateData.GetIsAdult());
        }
    }

    /// <summary>
    /// 主动拉取所有数据刷新UI
    /// </summary>
    private void RefreshAllData()
    {
        if (fish == null) return;

        // 刷新HP
        float sl = fish.HP / 100;
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

        // 刷新食物储备
        FoodReserveChange.text = ((int)fish.FoodReserve).ToString() + "/" +((int)fish.data.foodReserve).ToString();
    }
}
