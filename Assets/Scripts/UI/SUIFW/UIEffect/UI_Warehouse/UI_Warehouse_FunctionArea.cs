using UnityEngine;
using UnityEngine.UI;

public class UI_Warehouse_FunctionArea : UI_InfoFunctionArea
{
    public Button UI_Warehouse_FunctionArea_ChangeFish;
    public Button UI_Warehouse_FunctionArea_ChangePlant;
    public Button UI_Warehouse_FunctionArea_ChangeDecoration;
    public Button UI_Warehouse_FunctionArea_ChangeGird;
    public override void Init()
    {
        base.Init();
        if(!InitComplete)
        {
            UI_Warehouse_FunctionArea_ChangeFish.AddDeskTopListener(() => {
                event_manager.instance.dispatch_UIevent(SysDefine.UI_Warehouse_WarehouseInfo, "Fish");
            });
            UI_Warehouse_FunctionArea_ChangePlant.AddDeskTopListener(() => {
                event_manager.instance.dispatch_UIevent(SysDefine.UI_Warehouse_WarehouseInfo, "Plant");
            });
            UI_Warehouse_FunctionArea_ChangeDecoration.AddDeskTopListener(() => {
                event_manager.instance.dispatch_UIevent(SysDefine.UI_Warehouse_WarehouseInfo, "Decoration");
            });
            UI_Warehouse_FunctionArea_ChangeGird.AddDeskTopListener(() => {
                event_manager.instance.dispatch_UIevent(SysDefine.UI_Warehouse_WarehouseInfo, "Cube");
            });
            rects["UI_Warehouse_FunctionArea_ChangeFish"] = UI_Warehouse_FunctionArea_ChangeFish.GetComponent<RectTransform>();
            rects["UI_Warehouse_FunctionArea_ChangePlant"] = UI_Warehouse_FunctionArea_ChangePlant.GetComponent<RectTransform>();
            rects["UI_Warehouse_FunctionArea_ChangeDecoration"] = UI_Warehouse_FunctionArea_ChangeDecoration.GetComponent<RectTransform>();
            rects["UI_Warehouse_FunctionArea_ChangeGird"] = UI_Warehouse_FunctionArea_ChangeGird.GetComponent<RectTransform>();
        }
        InitComplete = true;
        SortRects();
    }
}
