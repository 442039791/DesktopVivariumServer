using Common;
using SUIFW;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UI_BioTankInfo_CreatureInfo_PlantInfo : MonoBehaviour,IChangeLanuge,IShowAtUI
{
    public Plant plant;
    /// <summary>
    /// 物品图标
    /// </summary>
    public Icon _Icon;
    //public Image Hp;
    public TextMeshProUGUI PlantName;
    public TextMeshProUGUI GrowthProgress;
    public TextMeshProUGUI PlantNameChange;
    public TextMeshProUGUI GrowthProgressChange;
    public TextMeshProUGUI PlantIsDead;
    public TextMeshProUGUI Money;
    public Button Save;
    public Button Sell;
    public Button SelectItem;
    public Image SelectIcon;
    public bool InitCompelte = false;
    private int pid=-1;
    private bool _onShow = false;
    public bool Debug = false;
    public TextMeshProUGUI PlantStateID;

    // 依赖注入
    private IWarehouseService _warehouseService;

    private static int editPlantID = -1;
    private static Dictionary<int,bool> editPlantDic = new Dictionary<int, bool>();
    public bool OnShow { get { return _onShow; } set {
            _onShow = value;
            if (this.plant!= null)
                this.plant.OnShow = value;
        } }
    public void ChangeLanuge(string name, object udata)
    {
        throw new System.NotImplementedException();
    }
    public void Select(string name, object udata)
    {
        SelectIcon.color = Color.white;
    }
    public void Init(Plant plant)
    {
        // 初始化依赖注入
        if (_warehouseService == null)
        {
            _warehouseService = ServiceLocator.Get<IWarehouseService>();
        }

        SelectIcon = SelectItem.transform.GetComponent<Image>();
        this.plant = plant;
        _Icon.Init(RewardType.Plant, plant.data.ID);
        Money.text = ((int)this.plant.stateData.GetPrice()).ToString();
        if (Debug)
            PlantStateID.text = this.plant.stateData.ObjID.ToString();
        if(!UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.ContainsKey(this.plant.ID))
        {
            UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.Add(this.plant.ID, false);
        }
        if (UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.plant.ID])
        {
            SelectIcon.color = new Color32(224, 224, 224, 255);
        }
        else
        {
            SelectIcon.color = Color.white;
        }
        //OnShow = true;
        on_event_handler("null", "Hp");
        on_event_handler("null", "GrowthProgress");
        on_event_handler("null", "AsDeath");
        if (editPlantDic.ContainsKey(this.plant.ID))
        {
            Save.gameObject.SetActive(!editPlantDic[this.plant.ID]);
        }
        else
        {
            Save.gameObject.SetActive(true);
        }
        this.plant.OnShow = true;
        if (InitCompelte)
        {
            // 已初始化完成，直接返回
        }
        if (!InitCompelte)
        {
            SelectIcon = SelectItem.transform.GetComponent<Image>();
            event_manager.instance.add_event_listener("SelectPlant" , on_event_Selecthandler);
            event_manager.instance.add_event_listener("PlantReEdit", on_event_ReEdithandler);
            event_manager.instance.add_event_listener(SysDefine.UI_CreatureInfo_Select, Select);
            event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
            SelectItem.AddDeskTopListener(() =>
            {
                if (editPlantID!=-1)
                    return;
                event_manager.instance.dispatch_event("SelectPlant", this.plant.ID);
                if (!UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.ContainsKey(this.plant.ID))
                {
                    UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.Add(this.plant.ID, false);
                }
                UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.plant.ID] = !UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.plant.ID];
                bool isSelect = UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.plant.ID];
                if (UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.plant.ID])
                {
                    WebSocketServerManager.GetActiveClient()?.SendOutlinableModeCommand(this.plant.stateData.ObjID, OutlinableMode.OnlySelect);
                }
                else
                {
                    WebSocketServerManager.GetActiveClient()?.SendOutlinableModeCommand(this.plant.stateData.ObjID, OutlinableMode.Disable);
                }
                event_manager.instance.dispatch_event(SysDefine.UI_CreatureInfo_Select, null);
                foreach (var key in UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.Keys.ToList())
                {
                    UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[key] = false;
                }
                UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.plant.ID] = isSelect;
                if (UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[this.plant.ID])
                SelectIcon.color = new Color32(224, 224, 224, 255);
            });
            this.Save.AddDeskTopListener(() =>
            {
                _warehouseService.SavePlant(this.plant.stateData);
                this.plant.Destroy();
            });
            this.Sell.AddDeskTopListener(() =>
            {
                PlayerManager.Instance.OnMoneyChanged(plant.stateData.GetPrice());
                this.plant.Destroy();
            });
            //TODO:
            //DesktopButtonManager.instance.AddListener(Clear, fish.);
        }

        //this.plant.OnShow = true;
        _Icon._Icon.sprite = plant.data.GetIconSprite();
        // 使用SetText注册，支持语言切换时自动更新
        LocalizationManager.SetText(PlantNameChange, int.Parse(plant.data.name));
        if (this.plant.GrowthProgress==1)
        {
            GrowthProgressChange.text = "100%";
        }
        else
        {
            GrowthProgressChange.text = ((int)(this.plant.GrowthProgress * 100) ).ToString() + "%" + "(" +Tools.TimeString(plant.stateData.GetGrowthTime())  + ")";
        }
        //GrowthProgressChange.text=plant.GrowthProgress.ToString();
        //PlantIsDead.text = plant.AsDeath ? "植物已死亡" : "";

        pid =this.plant.ID;
        InitCompelte = true;

    }
    public static void ResetEditPlantDic()
    {
        editPlantID = -1;
        var keys = new List<int>(editPlantDic.Keys);

        foreach (var key in keys)
        {


                editPlantDic[key] = false; // 安全修改
            
        }
    }
    public void on_event_ReEdithandler(string name, object obj)
    {
        if((int)obj == -99)
        {
            if(editPlantDic.ContainsKey(this.plant.ID))
            {
                Save.gameObject.SetActive(true);
            }
        }
        if (this.plant.ID != editPlantID&& editPlantDic.ContainsKey(this.plant.ID))
        {
            
            //editPlantDic[this.plant.ID] = false;
            Save.gameObject.SetActive(!editPlantDic[this.plant.ID]);
            //WebSocketServerManager.GetAcitveClient()?.SendPlantReEditCommand(this.plant.ID, this.plant.stateData.Type, editPlantDic[this.plant.ID] ? 1 : 0);
        }
    }
    public void on_event_Selecthandler(string name, object obj)
    {
        int id = (int)obj;
        if (id != plant.ID)
        {
            //this.OnSeleceted = false;
            if (!UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.ContainsKey(plant.ID))
            {
                UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic.Add(plant.ID, false);
            }
            else
            {
                UI_BioTankInfo_CreatureInfo_Extension.SelectedPlantDic[plant.ID] = false;
            }
        }
    }
    public void on_event_handler(string name, object obj)
    {
        if (!OnShow)
            return;
        string info = (string)obj;
        switch (info)
        {
            //case "Hp":
            //    float sl = this.plant.Hp / 100;
            //    Hp.fillAmount = sl;
            //    if (sl > 0.67f)
            //    {
            //        Hp.color = Color.green;
            //    }
            //    else if (sl > 0.33f)
            //    {
            //        Hp.color = Color.yellow;
            //    }
            //    else
            //    {
            //        Hp.color = Color.red;
            //    }
            //    break;



            case "GrowthProgress":

                //num = Mathf.Min(num, 0);
                if (plant.GrowthProgress == 1)
                {
                    GrowthProgressChange.text = "100%";
                }
                else
                {
                    GrowthProgressChange.text = ((int)(plant.GrowthProgress * 100)).ToString() + "%" + "(" + Tools.TimeString(plant.stateData.GetGrowthTime()) + ")";
                }
                
                //GrowthProgressChange.text = plant.GrowthProgress.ToString();
                break;
            case "AsDeath":
                break;
        }
    }

    public void Update()
    {
        if (plant.GrowthProgress == 1)
        {
            GrowthProgressChange.text = "100%";
        }
        else
        {
            GrowthProgressChange.text = ((int)(plant.GrowthProgress * 100)).ToString() + "%" + "(" + Tools.TimeString(plant.stateData.GetGrowthTime()) + ")";
        }
    }
}
public  class UI_BioTankInfo_CreatureInfo_Extension
{
    public static Dictionary<int, bool> SelectedPlantDic = new Dictionary<int, bool>();
}