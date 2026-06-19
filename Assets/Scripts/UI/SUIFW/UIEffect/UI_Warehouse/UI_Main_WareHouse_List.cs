using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Main_WareHouse_List : ChangeLanugeBase
{

    public ItemType type = ItemType.AdultFish;
    public ListController listController;
    public Button UI_Warehouse_WareInfo_Sort;
    public Button UI_Warehouse_WareInfo_Filter;
    public List<WarehouseBaseData> list;
    private bool _InitComplete = false;

    // 依赖注入
    private IWarehouseService _warehouseService;
    public override void Init()
    {
        // 初始化依赖注入
        if (_warehouseService == null)
        {
            _warehouseService = ServiceLocator.Get<IWarehouseService>();
        }

        base.Init(transform);
        list = _warehouseService.GetWarehouseBaseDatas(type);

        // 添加空值检查，防止崩溃
        if (list == null)
        {
            Debug.LogWarning($"[UI_Main_WareHouse_List] GetWarehouseBaseDatas返回null，type={type}");
            list = new List<WarehouseBaseData>();
        }

        switch (type)
        {
            case ItemType.AdultFish:
            case ItemType.Plant:
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_Warehouse_WareInfo_Creature_Info, list.Count, InitItemFunc/*, DealList*/);
                break;
            case ItemType.Cube:
            case ItemType.Decoration:
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_Warehouse_WareInfo_Info, list.Count, InitItemFunc/*, DealList*/);
                break;
        }


        if (_InitComplete)
            return;
        _InitComplete = true;
        event_manager.instance.add_event_listener(SysDefine.UI_WareHouse_List, on_event_handler);
    }
    public  void on_event_handler(string name, object udata)
    {
        if (gameObject.activeSelf == false)
            return;
        UI_WareHouse_List_EventData data = (UI_WareHouse_List_EventData)udata;
        if (data.type != type)
            return;
        switch (data.num)
        {
            case 0:
                listController.Refresh();
                break;
            case -1:
                listController.RemoveItem(1);
                break;
            case 1:
                listController.AddItem(1);
                break;
        }

    }
    public void InitItemFunc(int inex, GameObject gameObject)
    {
        UI_Warehouse_WareInfo_Info info = gameObject.GetComponent<UI_Warehouse_WareInfo_Info>();
        if (info != null)
        {
            // 添加边界检查，防止索引越界
            if (list != null && inex >= 0 && inex < list.Count)
            {
                info.Init(list[inex]);
            }
            else
            {
                Debug.LogWarning($"[UI_Main_WareHouse_List] InitItemFunc索引越界或list为null，index={inex}, listCount={list?.Count ?? 0}");
            }
        }
    }

}
public struct UI_WareHouse_List_EventData
{
    public ItemType type;
    public int num;
}
