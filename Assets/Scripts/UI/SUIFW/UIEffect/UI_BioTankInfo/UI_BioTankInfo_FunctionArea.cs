using SUIFW;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BioTankInfo_FunctionArea : UI_InfoFunctionArea
{
    

    public Button BuyFish;
    public Button BuyPlant;
    public Button BuyDecoration;
    public Button EditGrid;
    public Button LevelUP;

    public UI_BioTankInfo parent;

    public void Init(UI_BioTankInfo parent)
    {
        base.Init();
        this.parent = parent;
        if (!InitComplete)
        {
            DesktopButtonManager.instance.AddListener(BuyFish, () => { 
                //parent.CloseOrReturnUIForms();
                var u = UIManager.Instance.ShowUIForms(SysDefine.UI_BuyAnimal);
                u.transform.localPosition = parent.transform.localPosition;
            });
            DesktopButtonManager.instance.AddListener(BuyPlant, () =>
            {
                //parent.CloseOrReturnUIForms();
                var u = UIManager.Instance.ShowUIForms(SysDefine.UI_BuyPlant);
                u.transform.localPosition = parent.transform.localPosition;
            });
            DesktopButtonManager.instance.AddListener(BuyDecoration, () =>
            {
                //parent.CloseOrReturnUIForms();
                var u = UIManager.Instance.ShowUIForms(SysDefine.UI_BuyDecoration);
                u.transform.localPosition = parent.transform.localPosition;
            });
            DesktopButtonManager.instance.AddListener(EditGrid, () =>
            {
                //parent.CloseOrReturnUIForms();
                PlayerManager.Instance.ChangeBuildMode(true);
            });
            DesktopButtonManager.instance.AddListener(LevelUP, () =>
            {
                //TODO:
            });
            container = GetComponent<RectTransform>();
            event_manager.instance.add_event_listener(SysDefine.BioTankInfo_FunctionArea_ActionButton, ActionButton);
            rects.Add(BuyFish.name, BuyFish.GetComponent<RectTransform>());
            rects.Add(BuyPlant.name, BuyPlant.GetComponent<RectTransform>());
            rects.Add(BuyDecoration.name, BuyDecoration.GetComponent<RectTransform>());
            rects.Add(EditGrid.name, EditGrid.GetComponent<RectTransform>());
            rects.Add(LevelUP.name, LevelUP.GetComponent<RectTransform>());
        }
        InitComplete = true;
        SortRects();
        
    }
   
}
