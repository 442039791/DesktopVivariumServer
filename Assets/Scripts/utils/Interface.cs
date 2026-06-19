using UnityEngine;
using TMPro;
using System.Collections.Generic;
using SUIFW;

public interface IClick
{
    public Vector3 SavePos { get; set; }
    public bool CanClick { get; set; }
    public void AddListener(UnityEngine.Events.UnityAction action);
    public void RemoveListener();
    public void OnClick();
}
public interface IStop
{
    public void StopChange(bool stop);
}
public interface IChangeLanuge
{
    //public bool InitCompelete = true;
    public void ChangeLanuge(string name, object udata);

}
public abstract class ChangeLanugeBaseSUIFW : BaseUIForms, IChangeLanuge
{
    public int ID;//TODO:使用ID来提高遍历效率
    public bool InitCompelete;
    public Dictionary<string, List<TextMeshProUGUI>> text_list = new();
    public Dictionary<string, Dictionary<string, string>> lanuge_data = new();
    public void InitTextList(Dictionary<string, List<TextMeshProUGUI>> text_list)
    {
        this.text_list = text_list;
    }
    //if (InitCompelete)
    //return;
    //base.Init(transform);
    public override bool Init()
    {
        Init(transform);
        if (!InitCompelete)
        {
            event_manager.instance.add_event_listener("ChangeLanuge", ChangeLanuge);
        }
        InitCompelete = true;
        return base.Init();
    }
    public virtual void Init(Transform parent)
    {
        if (InitCompelete)
            return;

        // 自动应用当前语言的字体到所有文本组件
        LocalizationManager.ApplyCurrentFontToUI(parent);

        Dictionary<string, List<TextMeshProUGUI>> mtext_list = new();
        // 递归遍历所有子节点
        TraverseAllChildren(parent, mtext_list);
        Init(ID, mtext_list);
    }
    private void TraverseAllChildren(Transform node, Dictionary<string, List<TextMeshProUGUI>> mtext_list)
    {
        // 1. 检查当前节点是否有 Text 组件
        if (node.TryGetComponent(out TextMeshProUGUI text))
        {
            if (text.text != "")
            {
                if (mtext_list.ContainsKey(text.text))
                {
                    mtext_list[text.text].Add(text);
                }
                else
                {
                    mtext_list.Add(text.text, new List<TextMeshProUGUI>());
                    mtext_list[text.text].Add(text);
                }
            }

        }

        // 2. 递归遍历所有子节点
        foreach (Transform child in node)
        {
            TraverseAllChildren(child, mtext_list); // 递归调用
        }
    }
    public virtual void Init(int id, Dictionary<string, List<TextMeshProUGUI>> text_list)
    {
        InitTextList(text_list);
        ID = id;
        if (!InitCompelete)
        {
            event_manager.instance.add_event_listener("ChangeLanuge", ChangeLanuge);
        }
        InitCompelete = true;
        //var Languages = ResMgr.Instance.GetLaunguageList();

        lanuge_data.Add(SysDefine.Lauguage_Chinese, GetLanugeData(SysDefine.Lauguage_Chinese));
        lanuge_data.Add(SysDefine.Lauguage_English, GetLanugeData(SysDefine.Lauguage_English));
        lanuge_data.Add(SysDefine.Lauguage_Japanese, GetLanugeData(SysDefine.Lauguage_Japanese));
        lanuge_data.Add(SysDefine.Lauguage_Korean, GetLanugeData(SysDefine.Lauguage_Korean));
    }
    public Dictionary<string, string> GetLanugeData(string lanuge)
    {

        var data = new Dictionary<string, string>();
        foreach (var item in text_list.Keys)
        {
            data.Add(item, GetLanugeText(item, lanuge));
        }
        return data;
    }
    public string GetLanugeText(string key, string lanuge)
    {
        return "";
    }
    //public virtual new void Init()
    //{
        
    //}
    public void ChangeLanuge(string name, object udata)
    {
        if (!gameObject.activeInHierarchy)
            return;
        var lanuge = (string)udata;
        foreach (var item in text_list)
        {
            foreach (var value in item.Value)
            {
                value.text = lanuge_data[lanuge][item.Key];
            }
        }
    }
}
public abstract class ChangeLanugeBase : MonoBehaviour, IChangeLanuge
{
    public int ID;//TODO:使用ID来提高遍历效率
    public bool InitCompelete;
    public Dictionary<string,List<TextMeshProUGUI>> text_list = new();
    public Dictionary<string,Dictionary<string,string>> lanuge_data = new();
    public void InitTextList(Dictionary<string, List<TextMeshProUGUI>> text_list)
    {
        this.text_list = text_list;
    }
            //if (InitCompelete)
            //return;
            //base.Init(transform);

    public virtual void Init(Transform parent)
    {
        if (InitCompelete)
            return;

        // 自动应用当前语言的字体到所有文本组件
        LocalizationManager.ApplyCurrentFontToUI(parent);

        Dictionary<string, List<TextMeshProUGUI>> mtext_list = new();
        // 递归遍历所有子节点
        TraverseAllChildren(parent, mtext_list);
        Init(ID, mtext_list);
    }
    private void TraverseAllChildren(Transform node, Dictionary<string, List<TextMeshProUGUI>> mtext_list)
    {
        // 1. 检查当前节点是否有 Text 组件
        if (node.TryGetComponent(out TextMeshProUGUI text))
        {
            if(text.text!="")
            {
                if (text.text == null)
                    goto Label;
                if (mtext_list.ContainsKey(text.text))
                {
                    mtext_list[text.text].Add(text);
                }
                else
                {
                    mtext_list.Add(text.text, new List<TextMeshProUGUI>());
                    mtext_list[text.text].Add(text);
                }
            }
            
        }
        Label:
        // 2. 递归遍历所有子节点
        foreach (Transform child in node)
        {
            TraverseAllChildren(child, mtext_list); // 递归调用
        }
    }
    public virtual void Init(int id, Dictionary<string, List<TextMeshProUGUI>> text_list)
    {
        InitTextList(text_list);
        ID = id;
        if (!InitCompelete)
        {
            event_manager.instance.add_event_listener("ChangeLanuge", ChangeLanuge);
        }
        InitCompelete = true;
        

        //lanuge_data.Add(SysDefine.Lauguage_Chinese, GetLanugeData(SysDefine.Lauguage_Chinese));
        //lanuge_data.Add(SysDefine.Lauguage_English, GetLanugeData(SysDefine.Lauguage_English));
        //lanuge_data.Add(SysDefine.Lauguage_Japanese, GetLanugeData(SysDefine.Lauguage_Japanese));
        //lanuge_data.Add(SysDefine.Lauguage_Korean, GetLanugeData(SysDefine.Lauguage_Korean));
    }

    public virtual void Init()
    {
        if(!InitCompelete)
        {
            // 自动应用当前语言的字体到所有文本组件
            LocalizationManager.ApplyCurrentFontToUI(transform);

            event_manager.instance.add_event_listener("ChangeLanuge", ChangeLanuge);
        }
        InitCompelete = true;
    }
    public void ChangeLanuge(string name, object udata)
    {
        if (!gameObject.activeInHierarchy)
            return;
        var lanuge = (string)udata;
        foreach (var item in text_list)
        {
            foreach (var value in item.Value)
            {
                value.text = lanuge_data[lanuge][item.Key];
            }
        }
    }
    
}
public interface IShowAtUI
{
    public bool OnShow { get; set; }
}
