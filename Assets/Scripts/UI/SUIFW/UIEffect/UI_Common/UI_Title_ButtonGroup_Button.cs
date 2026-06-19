using UnityEngine;
using UnityEngine.UI;

public class UI_Title_ButtonGroup_Button : MonoBehaviour
{
    public Button button;
    public ButtonType ButtonType;
    private bool Compelted;
    public Image buttonImage;
    public Sprite[] buttonImages;
    public void Init()
    {
        if(!Compelted)
        {
            button.AddDeskTopListener(() => { event_manager.instance.dispatch_event(SysDefine.UI_Title_ButtonGroup_Button + ButtonType.ToString(), null); });

            Compelted = true;
        }
    }
    public void ShowButton(bool show)
    {
        int index = show ? 0 : 1;
        buttonImage.sprite = buttonImages[index];
    }
}
public enum ButtonType
{
    Biotank,
    Wildpicking,
    Warehouse,
    Bill,
    IllustratedGuide,
    Settings,
    Help,
    Log,
}