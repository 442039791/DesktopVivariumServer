using SUIFW;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

using TMPro;

public class UI_BioTankInfo_ButtonGroup : MonoBehaviour,IChangeLanuge
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Dictionary<string,Button> buttons= new Dictionary<string,Button>();
    bool Initcompelete = false;
    bool PorF=false;
    public Button SellAdult;
    public Button SellChild;
    public Button SaveAdult;
    public Button SaveChild;
    public Button ChangePlantORAnimal;

    public TextMeshProUGUI SellAdultText;
    public TextMeshProUGUI SellChildText;
    public TextMeshProUGUI SaveAdultText;
    public TextMeshProUGUI SaveChildText;
    public TextMeshProUGUI ChangePlantORAnimalText;
    private int bid;
    public UI_BioTankInfo_CreatureInfo_AnimalInfo _fish;
    public Plant _plant;
    private UI_BioTankInfo parent;

    // 依赖注入
    private IWarehouseService _warehouseService;
    //TODO: 动态创建button
    public void Init(int _bid,UI_BioTankInfo Parent)
    {
        // 初始化依赖注入
        if (_warehouseService == null)
        {
            _warehouseService = ServiceLocator.Get<IWarehouseService>();
        }

        parent = Parent;
        event_manager.instance.remove_UIevent_listener(bid + SysDefine.UI_BioTankInfo_ButtonGroupEvent);
        bid = _bid;
        event_manager.instance.add_UIevent_listener(bid + SysDefine.UI_BioTankInfo_ButtonGroupEvent, SetGroupEvent);
        if(!Initcompelete)
        {
            event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
            SellAdult ??= transform.Find("SellAdult").GetComponent<Button>();
            SellAdultText ??= SellAdult.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            SellChild ??= transform.Find("SellChild").GetComponent<Button>();
            SellChildText ??= SellChild.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            SaveAdult ??= transform.Find("SaveAdult").GetComponent<Button>();
            SaveAdultText ??= SaveAdult.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            SaveChild ??= transform.Find("SaveChild").GetComponent<Button>();
            SaveChildText ??= SaveChild.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            ChangePlantORAnimal ??= transform.Find("ChangePlantORAnimal").GetComponent<Button>();
            ChangePlantORAnimalText ??= ChangePlantORAnimal.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            SellAdult.AddDeskTopListener(() => { SellAdultFunc(); });
            //SellChild.AddDeskTopListener(() => { SellChildFunc(); });
            //SaveAdult.AddDeskTopListener(() => { SaveAdultFunc(); });
            //SaveChild.AddDeskTopListener(() => { SaveChildFunc(); });
            ChangePlantORAnimal.AddDeskTopListener(() => {
                _fish = null;
                _plant = null;
                PorF = !PorF;
                if (PorF)
                {
                    SellChild.DeskTopShowFalse();
                    SaveChild.DeskTopShowFalse();
                    SellAdult.RemoveDeskTopListener();
                    SellAdult.AddDeskTopListener(() => { SellPlantFunc(); });
                    //TODO:语言改变
                    SellAdultText.text = "出售植物";
                    SaveAdult.RemoveDeskTopListener();
                    SaveAdult.AddDeskTopListener(() => { SavePlantFunc(); });
                    SaveAdultText.text = "存储植物";
                    ChangePlantORAnimalText.text = "切换鱼类";
                }
                else
                {
                    SellChild.DestTopShowTrue();
                    SaveChild.DestTopShowTrue();
                    SellAdult.RemoveDeskTopListener();
                    SellAdult.AddDeskTopListener(() => { SellAdultFunc(); });
                    //TODO:语言改变
                    SellAdultText.text = "出售成鱼";
                    SaveAdult.RemoveDeskTopListener();
                    //SaveAdult.AddDeskTopListener(() => { SaveAdultFunc(); });
                    SaveAdultText.text = "存储成鱼";
                    ChangePlantORAnimalText.text = "切换植物";
                }
                //event_manager.instance.dispatch_UIevent(bid + SysDefine.UI_BioTankInfo_CreatureInfo_ChangePorF, PorF);
            });
        }
        Initcompelete=true;

    }
    public void SellAdultFunc()
    {
        if (_fish == null)
            return;
        var fish = _fish.fish;
        //_fish.AdultInputcount = _fish.AdultInputcount;
        //fish.AdultNum -= _fish.AdultInputcount;
        //_fish.AdultInputcount = 0;
        //if (fish.AdultNum == 0&& fish.JuvenileNum==0&&fish.AdultDeath==0&&fish.JuvenileDeath==0)
        //{
            fish.Destroy();
            _fish = null;
        //}
    }
    //public void SellChildFunc() { 
    //    if (_fish == null) return;
    //    var fish = _fish.fish;
    //    _fish.JuvenileInputcount = _fish.JuvenileInputcount;
    //    PlayerManager.Instance.OnMoneyChanged(_fish.JuvenileInputcount * fish.data.adultPurchasePrice);
    //    fish.JuvenileNum -= _fish.JuvenileInputcount;
    //    _fish.JuvenileInputcount = 0;
    //    if (fish.AdultNum == 0 && fish.JuvenileNum == 0 && fish.AdultDeath == 0 && fish.JuvenileDeath == 0)
    //    {
    //        fish.Destroy();
    //        _fish = null;
    //    }

    //}
    //public void SaveAdultFunc()
    //{
    //    if ( _fish == null) return;
    //    var fish = _fish.fish;
    //    _fish.AdultInputcount = _fish.AdultInputcount;
    //    WarehouseManager.Instance.SaveAnimal(fish.stateData, true, _fish.AdultInputcount);
    //    fish.AdultNum-= _fish.AdultInputcount;
    //    _fish.AdultInputcount = 0;
    //    if (fish.AdultNum == 0 && fish.JuvenileNum == 0)
    //    {
    //        fish.Destroy();
    //        //PlayerManager.Instance.BioTanks[_fish.stateData.ID].RemoveFish(_fish);
    //        _fish = null;
            
    //    }
        
    //}
    //public void SaveChildFunc()
    //{
    //    if ( !_fish) return;
    //    var fish = _fish.fish;
    //    _fish.JuvenileInputcount = _fish.JuvenileInputcount;
    //    WarehouseManager.Instance.SaveAnimal(fish.stateData, false, _fish.JuvenileInputcount);
    //    fish.JuvenileNum -= _fish.JuvenileInputcount;
    //    _fish.JuvenileInputcount = 0;
    //    if (fish.AdultNum == 0 && fish.JuvenileNum == 0)
    //    {
    //        fish.Destroy();
    //        //PlayerManager.Instance.BioTanks[_fish.stateData.ID].RemoveFish(_fish);
    //        _fish = null;

    //    }

    //}
    public void SellPlantFunc()
    {
        if(_plant==null) return;
        _plant.Destroy();
        _plant = null;
    }
    public void SavePlantFunc()
    {
        if (_plant == null) return;
        _warehouseService.SavePlant(_plant.stateData);
        _plant.Destroy();
        _plant = null;


    }
    public void SetGroupEvent(string name, object udata)
    {
        BioTankInfo_ButtonGroupMessage t = (BioTankInfo_ButtonGroupMessage)udata;
        switch (t.name)
        {
            case SysDefine.AnimalName:
                UI_BioTankInfo_CreatureInfo_AnimalInfo fish  = t.obj as UI_BioTankInfo_CreatureInfo_AnimalInfo;
                _fish= fish;
                _plant = null;
                break;
            case SysDefine.PlantName:
                Plant plant = t.obj as Plant;
                _plant= plant;
                _fish = null;
                break;
        }
    }
    public class BioTankInfo_ButtonGroupMessage
    {
        public string name;
        public object obj;
    }
    public void ChangeLanuge(string name, object udata)
    {
        throw new System.NotImplementedException();
    }
}
