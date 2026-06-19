using SUIFW;
using UnityEngine;
using UnityEngine.UI;

public class UI_BioTankInfo_Title : MonoBehaviour
{
    public Button Exit;
    public BaseUIForms Parent;
    private bool Initcompelete;
    public void Init(BaseUIForms b)
    {
        if (!Initcompelete)
        {
            Parent = b;
            Exit.AddDeskTopListener(() => {
                Parent.CloseOrReturnUIForms();
            });
            Initcompelete = true;
        }
    }
}
