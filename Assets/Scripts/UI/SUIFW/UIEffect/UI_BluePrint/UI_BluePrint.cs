using SUIFW;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_BluePrint : UIBase
{
    public Button AddBlueprintButton;
    public Button CheckBlueprintButton;
    //public static Dictionary<int, BluePrintInfo> bluePrints = new Dictionary<int, BluePrintInfo>();
    public static Dictionary<int, BluePrintInfomation> bluePrintsInfo = new Dictionary<int, BluePrintInfomation>();
    public Dictionary<int, bool> reloadPrintsInfo = new Dictionary<int, bool>();
    [SerializeField]
    private ListController bluePrintListController;
    private int UniqueID = 0;
    public string InfoName;
    public override bool Init()
    {
        if(!initcomplete)
        {
            string _path = GetBluePrintPath(SysDefine.BluePrintMainPath) + SysDefine.DefaultBluePrintSaveDataName;
            if (File.Exists(_path))
            {
                string json = SaveCompression.LoadSmart(_path);
                BluePrintSaveData sd = JsonUtility.FromJson<BluePrintSaveData>(json);
                UniqueID = sd.ActiveID;
                //foreach (var item in sd._blueprints)
                //{
                //    bluePrintsInfo.Add(item.ID, item);
                //}
                bluePrintsInfo = sd._blueprints.ToDictionary(x => x.ID, x => x);
            }
            
        }
        bluePrintListController.Init(SysDefine.PrefabAssetbundle + SysDefine.UI_BluePrintnfomation, bluePrintsInfo.Count, InitItemFunc);
        if (initcomplete)
        {
            return true;
        }
        AddBlueprintButton.AddDeskTopListener(() => {
            UI_BluePrintSetName set = (UI_BluePrintSetName)UIManager.Instance.ShowUIForms(SysDefine.UI_BluePrintSetName);
            set.Init(UI_BluePrintSetName_Type.Add,null,this);
        });
        CheckBlueprintButton.AddDeskTopListener(() => {
            //if (Checked)
            //{
            //    return;
            //}
            //Checked = true;
            var sd= OTSHandle.Instance.InitializationCube();
            sd.ID = GetUniqueID();
            sd.IconPath = "";
            BluePrintInfomation info = new BluePrintInfomation();
            info.ID = sd.ID;
            info.Name = InfoName;
            info.Size = sd.Size;
            info.IconName = "";
            info.BlockModifyNewDataName = info.ID + "_" + SysDefine.DefaultBluePrintDataName;
            var json = JsonUtility.ToJson(sd);
            string BlockModifyNewDataPath = GetBluePrintPath(SysDefine.BluePrintDataPath) + info.BlockModifyNewDataName;
            SaveCompression.SaveCompressed(BlockModifyNewDataPath, json);
            bluePrintsInfo.Add(sd.ID, info);
            SaveBluePrintSaveData();
            bluePrintListController.AddItem(1);
        });
        event_manager.instance.add_UIevent_listener(SysDefine.UI_BluePrint_Add, UI_BluePrint_Add);
        event_manager.instance.add_UIevent_listener(SysDefine.UI_BluePrintPrefab_Save, UI_BluePrint_Save);
        event_manager.instance.add_UIevent_listener(SysDefine.UI_BluePrintPrefab_Delete, UI_BluePrint_Delete);
        event_manager.instance.add_UIevent_listener(SysDefine.UI_BluePrintPrefab_Load, UI_BluePrint_Load);
        event_manager.instance.add_UIevent_listener(SysDefine.UI_BluePrint_Save, UI_BluePrint_Save_ClientReturn);
        event_manager.instance.add_UIevent_listener(SysDefine.UI_BluePrint_SaveData, UI_BluePrint_SaveData);
        return base.Init();
    }
    public void UI_BluePrint_SaveData(string name, object udata)
    {
        SaveBluePrintSaveData();
    }
    private int l_SaveBluePrintID;
    private BluePrintInfomation l_SaveBluePrintInfo;
    public void TryAddBluePrintInfo()
    {
        BluePrintInfomation info = new BluePrintInfomation();
        info.ID = GetUniqueID();
        info.Name = InfoName;
        info.IconName = info.ID + "_" + SysDefine.DefaultBluePrintIconName;
        info.BlockModifyNewDataName = info.ID + "_" + SysDefine.DefaultBluePrintDataName;
        l_SaveBluePrintID = info.ID;
        l_SaveBluePrintInfo = info;
        WebSocketServerManager.GetActiveClient()?.SendBluePrintServerCommand(info.ID, BluePrintCommandType.Add, info.IconName, info.BlockModifyNewDataName);
    }
    public int GetUniqueID()
    {
        return ++UniqueID;
    }
    public  void UI_BluePrint_Add(string name, object udata)
    {
        var cdata = (BluePrintClientCommandData)udata;
        if(cdata.id!= l_SaveBluePrintID)
        {
            return;
        }
        l_SaveBluePrintInfo.Size = cdata.Size;
        bluePrintsInfo.Add(cdata.id, l_SaveBluePrintInfo);
        //var data = bluePrintsInfo[cdata.id];
        //if (data == null)
        //{
        //    bluePrintsInfo.Remove(cdata.id);
        //    return;
        //}
        //data.Size = cdata.Size;
        SaveBluePrintSaveData();
        bluePrintListController.AddItem(1);
    }
    public void UI_BluePrint_Save(string name, object udata)
    {
        UI_BluePrintPrefab pre = (UI_BluePrintPrefab)udata;
        
        var info = bluePrintsInfo[pre.BluePrintID];
        if (info == null)
        {
            return;
        }
        SaveUI_BluePrintPrefab = pre;
        WebSocketServerManager.GetActiveClient()?.SendBluePrintServerCommand(info.ID, BluePrintCommandType.Save, info.IconName, info.BlockModifyNewDataName);
    }
    public void UI_BluePrint_Delete(string name, object udata)
    {
        int id = (int)udata;
        var info = bluePrintsInfo[id];
        if (info == null)
        {
            return;
        }
        bluePrintsInfo.Remove(id);
        ResMgr.Instance.DeleteFileByIO(GetBluePrintPath(SysDefine.BluePrintIconPath)+info.IconName);
        ResMgr.Instance.DeleteFileByIO(GetBluePrintPath(SysDefine.BluePrintDataPath)+info.BlockModifyNewDataName);
        SaveBluePrintSaveData();
        bluePrintListController.RemoveItem(1);
    }
    private void SaveBluePrintSaveData()
    {
        var b = new BluePrintSaveData();
        b._blueprints = bluePrintsInfo.Values.ToList().ToArray();
        b.ActiveID = UniqueID;
        string json = JsonUtility.ToJson(b);
        string _path = GetBluePrintPath(SysDefine.BluePrintMainPath) + SysDefine.DefaultBluePrintSaveDataName;
        SaveCompression.SaveCompressed(_path, json);
    }
        
    
    public string GetBluePrintPath(string path)
    {
        string _path = Path.Combine(Application.streamingAssetsPath, path);
        return _path.Replace(@"\", "/");
    }
    public void UI_BluePrint_Load(string name, object udata)
    {
        int id = (int)udata;
        var info = bluePrintsInfo[id];
        if (info == null)
        {
            return;
        }
        WebSocketServerManager.GetActiveClient()?.SendBluePrintServerCommand(info.ID, BluePrintCommandType.Load, info.IconName, info.BlockModifyNewDataName);
    }
    private UI_BluePrintPrefab SaveUI_BluePrintPrefab;
    public void UI_BluePrint_Save_ClientReturn(string name, object udata)
    {
        int id = (int)udata;
        if(id!= SaveUI_BluePrintPrefab.BluePrintID)
        {
            SaveUI_BluePrintPrefab = null;
            return;
        }
        //reloadPrintsInfo[id] = true;
        if (!bluePrintsInfo.TryGetValue(id, out BluePrintInfomation info))
        {
            return; 
        }
        var data = BluePrintInfo.GetBluePrintInfoByBluePrintInfomation(info, true);
        SaveUI_BluePrintPrefab.ChangeInfo(data.Name, data.Size, data.Icon);
        SaveUI_BluePrintPrefab = null;
        SaveBluePrintSaveData();
        //bluePrintListController.Refresh();
    }

    public BluePrintInfomation GetBluePrintInfoByIO(int id)
    {
        var sd = GetBluePrintSaveData();
        if (sd == null)
        {
            return null;
        }
        foreach (var item in sd._blueprints)
        {
            if (item.ID == id)
            {
                return item;
            }
        }
        return null;
    }
    public BluePrintInfo GetBluePrintInfo(int id)
    {
        if (bluePrintsInfo.ContainsKey(id))
        {
            bool needReload = false;
            reloadPrintsInfo.TryGetValue(id, out needReload);
            return BluePrintInfo.GetBluePrintInfoByBluePrintInfomation(bluePrintsInfo[id],needReload);
        }
        return null;
    }
    public BluePrintSaveData GetBluePrintSaveData()
    {
        string _json = ResMgr.Instance.GetDataByIO(Application.streamingAssetsPath + "/"  + SysDefine.DefaultBluePrintSaveDataName);
        if (_json == null)
        {
            return null;
        }
        BluePrintSaveData sd = JsonUtility.FromJson<BluePrintSaveData>(_json);
        UniqueID = sd.ActiveID;
        return sd;
    }
    public void InitItemFunc(int inex, GameObject gameObject)
    {
        UI_BluePrintPrefab info = gameObject.GetComponent<UI_BluePrintPrefab>();
        if (info != null)
        {
            // 添加边界检查，防止索引越界
            var blueprintList = bluePrintsInfo.Values.ToList();
            if (inex >= 0 && inex < blueprintList.Count)
            {
                var data = blueprintList[inex];
                bool needReload = false;
                reloadPrintsInfo.TryGetValue(data.ID, out needReload);
                info.Init(BluePrintInfo.GetBluePrintInfoByBluePrintInfomation(data,needReload));
            }
            else
            {
                Debug.LogWarning($"[UI_BluePrint] InitItemFunc索引越界，index={inex}, blueprintsCount={blueprintList.Count}");
            }
        }
    }
}
public class BluePrintInfo
{
    public int ID;
    public string Name;
    public Vector3Int Size;
    public Sprite Icon;
    public static BluePrintInfo GetBluePrintInfoByBluePrintInfomation(BluePrintInfomation info,bool Reload = false)
    {
        BluePrintInfo bp = new BluePrintInfo();
        bp.ID = info.ID;
        bp.Name = info.Name;
        
        bp.Size = new Vector3Int((int)info.Size.x, (int)info.Size.y, (int)info.Size.z);

        bp.Icon = ResMgr.Instance.GetBluePrintIconByIO(info.IconName, Reload);
        return bp;
    }
}