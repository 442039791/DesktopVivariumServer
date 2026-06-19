using TMPro;
using UnityEngine;

public class UI_Title_Money : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    private bool Initcomplete = false;
    public void Init()
    {
        if (Initcomplete) return;
        Initcomplete = true;
        event_manager.instance.add_UIevent_listener(SysDefine.UI_Title_Money, On_MoneyChange);
    }
    public  void On_MoneyChange(string name, object udata)
    {
        int money = (int)udata;
        moneyText.text = Tools.MoneyString(money);
    }
}
