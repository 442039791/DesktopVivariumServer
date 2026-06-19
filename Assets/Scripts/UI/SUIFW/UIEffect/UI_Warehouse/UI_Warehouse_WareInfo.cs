using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Warehouse_WareInfo : ChangeLanugeBase
{
    [HideInInspector]
    public ItemType type=ItemType.AdultFish;
    public ListController listController;
    public Button UI_Warehouse_WareInfo_Sort;
    public Button UI_Warehouse_WareInfo_Filter;
    public List<WarehouseBaseData> list;

    // 依赖注入
    private IWarehouseService _warehouseService;
    public override void Init()
    {
        // 初始化依赖注入
        if (_warehouseService == null)
        {
            _warehouseService = ServiceLocator.Get<IWarehouseService>();
        }

        if(!InitCompelete)
        {//TODO: ����
            event_manager.instance.add_UIevent_listener(SysDefine.UI_Warehouse_WarehouseInfo, ListChange);
        }
        base.Init();
        list = _warehouseService.GetWarehouseBaseDatas(type);

        // 添加空值检查，防止崩溃
        if (list == null)
        {
            Debug.LogWarning($"[UI_Warehouse_WareInfo] GetWarehouseBaseDatas返回null，type={type}");
            list = new List<WarehouseBaseData>();
        }

        listController.Init(SysDefine.PrefabAssetbundle+SysDefine.UI_Warehouse_WareInfo_Info, list.Count, InitItemFunc/*, DealList*/);
    }
    public void ListChange(string name, object udata)
    {
        if (!gameObject.activeInHierarchy)
            return;
        string n = udata as string;
        switch (n)
        {
            case "Fish":
                type = ItemType.AdultFish;
                list = _warehouseService.GetWarehouseBaseDatas(type);
                if (list == null) list = new List<WarehouseBaseData>();
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_Warehouse_WareInfo_Info, list.Count, InitItemFunc/*, DealList*/);
                break;
            case "Plant":
                type = ItemType.Plant;
                list = _warehouseService.GetWarehouseBaseDatas(type);
                if (list == null) list = new List<WarehouseBaseData>();
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_Warehouse_WareInfo_Info, list.Count, InitItemFunc/*, DealList*/);
                break;
            case "Decoration":
                type = ItemType.Decoration;
                list = _warehouseService.GetWarehouseBaseDatas(type);
                if (list == null) list = new List<WarehouseBaseData>();
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_Warehouse_WareInfo_Info, list.Count, InitItemFunc/*, DealList*/);
                break;
            case "Cube":
                type = ItemType.Cube;
                list = _warehouseService.GetWarehouseBaseDatas(type);
                if (list == null) list = new List<WarehouseBaseData>();
                listController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_Warehouse_WareInfo_Info, list.Count, InitItemFunc/*, DealList*/);
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
                Debug.LogWarning($"[UI_Warehouse_WareInfo] InitItemFunc索引越界或list为null，index={inex}, listCount={list?.Count ?? 0}");
            }
        }
    }
}
