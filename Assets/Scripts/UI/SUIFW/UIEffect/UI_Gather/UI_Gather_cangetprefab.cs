using UnityEngine;
using UnityEngine.UI;

public class UI_Gather_cangetprefab : MonoBehaviour
{

    public Icon _icon;
    public void Init(Sprite ICon,Sprite BgSprite,string name,string type,string describe, bool isEnabled)
    {
        var tipsTrigger= gameObject.GetComponent<IconTipsTrigger>();
        tipsTrigger.Name = name;
        tipsTrigger.Type = type;
        tipsTrigger.Describe = describe;
        tipsTrigger.isEnabled = isEnabled;
        _icon._Icon.sprite = ICon;
        _icon._Frame.sprite = BgSprite;

    }
}
