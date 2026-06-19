using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UI_Gather_Title : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    private bool InitCompelete = false;
    public UI_Gather Parent;
    public Button exitButton;
    public void ChangeLanuge(string name, object udata)
    {
        throw new System.NotImplementedException();
    }

    public void Init(UI_Gather p)
    {
        Parent = p;
        if (!InitCompelete)
        {
            event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
            exitButton.AddDeskTopListener(() =>
            {
                p.CloseOrReturnUIForms();
            });
        }
        InitCompelete = true;
    }
}
