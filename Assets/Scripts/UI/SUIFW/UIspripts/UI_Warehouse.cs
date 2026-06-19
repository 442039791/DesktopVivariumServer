using UnityEngine;
using SUIFW;
public class UI_Warehouse : UIBase
{
    //public UI_Warehouse_Title _title;
    public UI_Warehouse_FunctionArea _FunctionArea;
    public UI_Main_WareHouse_List _WareInfo_Animal;
    public UI_Main_WareHouse_List _WareInfo_Plant;
    public UI_Main_WareHouse_List _WareInfo_Decoration;
    public UI_Main_WareHouse_List _WareInfo_Cube;
    //public UI_Warehouse_WareInfo _WareInfo_Decoration;

    public void RefreshUI()
    {
        _WareInfo_Animal.Init();
        _WareInfo_Plant.Init();
        _WareInfo_Decoration.Init();
        _WareInfo_Cube.Init();
    }
    public override bool Init()
    {
        OnShow = true;
        WarehouseManager.Instance.uI_Warehouse = this;

        // 关闭生态箱列表界面（互斥逻辑）
        UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_Biotank_List);

        // 设置每个列表的类型
        _WareInfo_Animal.type = ItemType.AdultFish;
        _WareInfo_Plant.type = ItemType.Plant;
        _WareInfo_Decoration.type = ItemType.Decoration;
        _WareInfo_Cube.type = ItemType.Cube;

        //gameObject.SetActive(true);
        //_title.Init(this);
        //_FunctionArea.Init();
        _WareInfo_Animal.Init();
        _WareInfo_Plant.Init();
        _WareInfo_Decoration.Init();
        _WareInfo_Cube.Init();
        base.InitButtonClickEvent((obj) =>
        {
            RefreshUI();
                //event_manager.instance.dispatch_UIevent(SysDefine.UI_Warehouse_WarehouseInfo, "Fish");


            //event_manager.instance.dispatch_UIevent(SysDefine.UI_Warehouse_WarehouseInfo, "Plant");


            //event_manager.instance.dispatch_UIevent(SysDefine.UI_Warehouse_WarehouseInfo, "Decoration");


            //event_manager.instance.dispatch_UIevent(SysDefine.UI_Warehouse_WarehouseInfo, "Cube");

        },null);
        //RefreshUI();
        base.Init();
        return true;
    }
    //public override void CloseOrReturnUIForms()
    //{
    //    OnShow = false;
    //    base.CloseOrReturnUIForms();
    //}
}
