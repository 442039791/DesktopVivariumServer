using UnityEngine;
using SUIFW;
using UnityEngine.UI;
using TMPro;

public class UI_BluePrintSetName : BaseUIForms
{
    public Button ExitButton;
   public Button ConfirmButton;
   public TMP_InputField NameInput;
    public UI_BluePrintSetName_Type type;
    private UI_BluePrintPrefab pre;
    private UI_BluePrint p;
    public override bool Init()
    {
        // 扫描并注册所有本地化文本（自动处理所有"Id:xxx"格式的文本）
        LocalizationManager.ScanAndRegisterTextsStatic(transform);

        NameInput.text = "";
        ExitButton.AddDeskTopListener(() =>
        {
            CloseOrReturnUIForms();
        });
        ConfirmButton.AddDeskTopListener(() =>
        {
            Confirm();
        });
        return true;
    }
    public void Confirm()
    {
        switch (type)
        {
            case UI_BluePrintSetName_Type.Add:
                p.InfoName = NameInput.text;
                p.TryAddBluePrintInfo();
                break;
            case UI_BluePrintSetName_Type.Rename:
                pre.ChangeNameInfo(NameInput.text);
                UI_BluePrint.bluePrintsInfo[pre.ID].Name=NameInput.text;
                event_manager.instance.dispatch_UIevent(SysDefine.UI_BluePrint_SaveData, null);
                break;
            default:
                break;
        }
        CloseOrReturnUIForms();
    }
    public void Init(UI_BluePrintSetName_Type type,UI_BluePrintPrefab pre=null,UI_BluePrint p=null)
    {
        switch (type)
        {
            case UI_BluePrintSetName_Type.Add:
                this.pre = null;
                this.p = p;
                NameInput.text = p.InfoName;
                this.type = type;
                break;
            case UI_BluePrintSetName_Type.Rename:
                this.type = type;
                this.pre = pre;
                this.p = null;
                NameInput.text = pre.NameInfoMation.text;
                break;
            default:
                break;
        }
    }
}
public enum UI_BluePrintSetName_Type
{
    Add,
    Rename,
}