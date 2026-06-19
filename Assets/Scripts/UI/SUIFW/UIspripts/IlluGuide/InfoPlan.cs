using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InfoPlan : MonoBehaviour
{
    private PlantData data;
    public TextMeshProUGUI _Name;
    public TextMeshProUGUI _Describe;
    public TextMeshProUGUI _Money;
    public TextMeshProUGUI _AdultTimeNum;
    public TextMeshProUGUI _FoodNum;
    public TextMeshProUGUI _O2Num;
    public TextMeshProUGUI _CO2Num;
    public TextMeshProUGUI _WasteNum;
    public TextMeshProUGUI _NutrientNum;
    public void Init(PlantData plantData)
    {
        data = plantData;
        // 使用SetText注册，支持语言切换时自动更新
        LocalizationManager.SetText(_Name, int.Parse(data.name));
        LocalizationManager.SetText(_Describe, int.Parse(data.Describe));
        _AdultTimeNum.text =Tools.TimeString(data.AdultTime*60);
        _FoodNum.text ="+"+ ((int)(data.FoodNum * 60)).ToString()+"/h";
        _O2Num.text = "+" + ((int)(data.O2Produce * 60)).ToString() + "/h";
        _CO2Num.text = "-" + ((int)(data.CO2Cost * 60)).ToString() + "/h";
        _WasteNum.text = "-" + ((int)(data.WasteCost * 60)).ToString() + "/h";
        _NutrientNum.text = "-" + ((int)(data.NutrientCost * 60)).ToString() + "/h";
        _Money.text = Tools.MoneyString(data.Price);
    }
}
