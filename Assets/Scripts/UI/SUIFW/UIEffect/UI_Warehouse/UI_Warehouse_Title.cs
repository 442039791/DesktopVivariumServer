using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Warehouse_Title : MonoBehaviour,IChangeLanuge
{
    public TextMeshProUGUI textMesh;
    private bool InitCompelete = false;
    public UI_Warehouse Parent;
    public Button exitButton;
    public void ChangeLanuge(string name, object udata)
    {
        throw new System.NotImplementedException();
    }

    public void Init(UI_Warehouse p)
    {
        Parent=p;
        if (!InitCompelete) {
            event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
            //exitButton.AddDeskTopListener(() =>
            //{
            //    p.CloseOrReturnUIForms();
            //});
        }
        InitCompelete=true;
    }
}
