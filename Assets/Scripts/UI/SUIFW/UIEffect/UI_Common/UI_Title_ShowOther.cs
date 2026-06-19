using UnityEngine;
using UnityEngine.UI;

public class UI_Title_ShowOther : UIIOnceInit
{
    public Button button;
    public override void Init()
    {
        
        if(Inited)
            return;
        button.AddDeskTopListener(() =>
        {
            event_manager.instance.dispatch_UIevent(SysDefine.UI_Title_ShowOther, null);
        })
       ;
        base.Init();
    }
}
