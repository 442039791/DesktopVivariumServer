using TMPro;
using UnityEngine;

public class InfoItem : MonoBehaviour
{
    public TextMeshProUGUI _Name;
    public TextMeshProUGUI _Describe;
    public GameObject _Money;

    public void Init(string name,string des,int price)
    {
        // 使用SetText注册，支持语言切换时自动更新
        LocalizationManager.SetText(_Name, int.Parse(name));
        LocalizationManager.SetText(_Describe, int.Parse(des));
        if (price>=0&&_Money.TryGetComponent<TextMeshProUGUI>(out var adult0M))
        {
            adult0M.text = Tools.MoneyString(price);
            _Money.SetActive(true);
        }else
        {
            _Money.SetActive(false);
        }
    }
}
