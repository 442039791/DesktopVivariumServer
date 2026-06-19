using SUIFW;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BluePrintPrefab : ChangeLanugeBase
{
    [SerializeField]
    public TextMeshProUGUI NameInfoMation;
    [SerializeField]
    private TextMeshProUGUI SizeInfoMation;
    [SerializeField]
    private Button SaveButton;
    [SerializeField]
    private Button DeleteButton;
    [SerializeField]
    private Button LoadButton;
    [SerializeField]
    private Button RenameButton;
    [SerializeField]
    private Image Icon;
    public int BluePrintID;
    public  void Init(BluePrintInfo b)
    {
        

        BluePrintID = b.ID;
        ChangeIconInfo(b.Icon);
        ChangeNameInfo(b.Name);
        ChangeSizeInfo(b.Size.x.ToString() + "*" + b.Size.y.ToString() + "*" + b.Size.z.ToString());
        Icon.sprite = b.Icon;
        if (InitCompelete)
            return;
        SaveButton.AddDeskTopListener(() =>
        {
            event_manager.instance.dispatch_UIevent(SysDefine.UI_BluePrintPrefab_Save, this);
        });
        DeleteButton.AddDeskTopListener(
            ()=>event_manager.instance.dispatch_UIevent(SysDefine.UI_BluePrintPrefab_Delete, BluePrintID)) ;
        LoadButton.AddDeskTopListener(() =>
        {
            event_manager.instance.dispatch_UIevent(SysDefine.UI_BluePrintPrefab_Load, BluePrintID);
        });
        RenameButton.AddDeskTopListener(() =>
        {
            UI_BluePrintSetName set = (UI_BluePrintSetName)UIManager.Instance.ShowUIForms(SysDefine.UI_BluePrintSetName);
            set.Init(UI_BluePrintSetName_Type.Rename, this, null);
        });
        //RenameButton.AddDeskTopListener();
        base.Init();
    }
    public void ChangeInfo(string name, Vector3 size, Sprite icon)
    {
        ChangeNameInfo(name);
        ChangeSizeInfo(size.x.ToString() + "*" + size.y.ToString() + "*" + size.z.ToString());  
        ChangeIconInfo(icon);
    }
    public void ChangeIconInfo(Sprite icon)
    {
        Icon.sprite = icon;
    }
    public void ChangeNameInfo(string name)
    {
        NameInfoMation.text = name;
    }
    public void ChangeSizeInfo(string size)
    {
        SizeInfoMation.text = size;
    }
}
