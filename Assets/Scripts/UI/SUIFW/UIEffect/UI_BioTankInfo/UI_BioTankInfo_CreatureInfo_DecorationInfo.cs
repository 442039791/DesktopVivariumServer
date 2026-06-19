

using TMPro;

using UnityEngine.UI;


public class UI_BioTankInfo_CreatureInfo_DecorationInfo : ChangeLanugeBase
{
    public Decoration _decoration;
    public Image image;
    public TextMeshProUGUI DecorationName;

    public TextMeshProUGUI DecorationNameChange;

    //private int _AdultInputcount;
    //public int AdultInputcount
    //{
    //    get { return _AdultInputcount; }
    //    set
    //    {

    //        if (fish == null)
    //            return;
    //        if (value >= fish.stateData.AdultNum)
    //        {
    //            value = (int)fish.stateData.AdultNum;
    //        }
    //        if (value < 0)
    //        {
    //            value = 0;
    //        }
    //        _AdultInputcount = value;
    //        AdultInput.text = _AdultInputcount.ToString();
    //    }
    //}
    //private int _JuvenileInputcount;
    //public int JuvenileInputcount
    //{
    //    get { return _JuvenileInputcount; }
    //    set
    //    {

    //        if (fish == null)
    //            return;
    //        if (value >= fish.stateData.JuvenileNum)
    //        {
    //            value = (int)fish.stateData.JuvenileNum;
    //        }
    //        if (value < 0)
    //        {
    //            value = 0;
    //        }
    //        _JuvenileInputcount = value;
    //        JuvenileInput.text = _JuvenileInputcount.ToString();
    //    }
    //}

    public Button SelectItem;
    public Button SellItem;
    public bool InitCompelte=false;
    public int fid=-1;
    public bool OnShow { get; set; }
    private bool OnSeleceted = false;
    public new void ChangeLanuge(string name, object udata)
    {
        throw new System.NotImplementedException();
    }

    public void Init(Decoration decoration)
    {
        this._decoration = decoration;

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
        //TODO:Ĭ�ϴ���AddNewTextByBeforeSymbol
        image.sprite = decoration.data.GetIconSprite();
        DecorationNameChange.text= decoration.data.name;
        //JuvenileNumChange.text=((int)fish.JuvenileNum).ToString();
        //AdultNumChange.text = ((int)fish.AdultNum).ToString();
        //FoodReserveChange.text = ((int)fish.FoodReserve).ToString();
        //DeadFishChange.text = (fish.AdultDeath + fish.JuvenileDeath).ToString();
        if(!InitCompelte)
        {
            event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
            SelectItem.AddDeskTopListener(() =>
            {
                OnSeleceted =!OnSeleceted;
                if (OnSeleceted)
                {
                    WebSocketServerManager.GetActiveClient()?.SendOutlinableModeCommand(decoration.ID, OutlinableMode.OnlySelect);
                }
                else
                {
                    WebSocketServerManager.GetActiveClient()?.SendOutlinableModeCommand(decoration.ID, OutlinableMode.Disable);
                }

                
                
                //event_manager.instance.dispatch_UIevent(this.fish.stateData.ID + SysDefine.UI_BioTankInfo_ButtonGroupEvent, new BioTankInfo_ButtonGroupMessage { name = SysDefine.FishName, obj = this });
            });
            SelectItem.AddDeskTopListener(() =>
            {
                _decoration.Destroy();
            });
            //this.Clear.AddDeskTopListener(() =>
            //{
            //    //if(this.fish == null)
            //    //{
            //    //    return;
            //    //}

            //    //this.fish.AdultDeath = 0;
            //    //this.fish.JuvenileDeath = 0;

            //    //if(this.fish.AdultNum==0&& this.fish.JuvenileNum==0)
            //    //{
            //    //    this.fish.Destroy();
            //    //    //this.fish = null;
            //    //}
            //    //JuvenileInputcount = JuvenileInputcount;
            //    //AdultInputcount = AdultInputcount;
            //});
            //TODO:
            //DesktopButtonManager.instance.AddListener(Clear, fish.);
        }

        //event_manager.instance.remove_UIevent_listener("Decoration" + fid);
        //fid = _decoration.ID;
        //event_manager.instance.add_UIevent_listener("Decoration" + fid, on_event_handler);
        InitCompelte = true;

    }

    //public void on_event_handler(string name, object obj)
    //{
    //    if (!gameObject.activeInHierarchy)
    //        return;
    //    string info = (string)obj;
    //    switch (info)
    //    {   
    //        //case "JuvenileNum":
    //        //    JuvenileNumChange.text = ((int)fish.JuvenileNum).ToString();
    //        //    break;
    //        //case "AdultNum":
    //        //    AdultNumChange.text = ((int)fish.AdultNum).ToString();
    //            //break;
    //        case "FoodReserve":
    //            FoodReserveChange.text = ((int)fish.FoodReserve).ToString();
    //            break;
    //        //case "DeadFish":
    //        //    DeadFishChange.text =((int)fish.AdultDeath + (int)fish.JuvenileDeath).ToString();
    //        //    break;
    //    }
    //}
}
