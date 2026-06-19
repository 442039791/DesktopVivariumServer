using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InfoFish : MonoBehaviour
{
    public AnimalData fishData;
    public TextMeshProUGUI _Name;
    public TextMeshProUGUI _Describe;
    public TextMeshProUGUI _juvenileSpaceNum;
    public TextMeshProUGUI _juvenileAdultTimeNum;
    public TextMeshProUGUI _juvenileFoodParameterNum;
    public TextMeshProUGUI _juvenileFoodCostNum;
    public TextMeshProUGUI _juvenileO2CostNum;
    public TextMeshProUGUI _juvenileO2MinProportionNum;
    public TextMeshProUGUI _juvenileCO2ProduceNum;
    public TextMeshProUGUI _juvenileCO2MaxProportionNum;
    public TextMeshProUGUI _juvenileWasteProduceNum;
    public TextMeshProUGUI _juvenileWasteMaxProduceNum;
    public TextMeshProUGUI _adultSpaceNum;
    public TextMeshProUGUI _adultReproductionNum;
    public TextMeshProUGUI _adultFoodParameterNum;
    public TextMeshProUGUI _adultFoodCostNum;
    public TextMeshProUGUI _adultO2CostNum;
    public TextMeshProUGUI _adultO2MinProportionNum;
    public TextMeshProUGUI _adultCO2ProduceNum;
    public TextMeshProUGUI _adultCO2MaxProduceNum;
    public TextMeshProUGUI _adultWasteProduceNum;
    public TextMeshProUGUI _adultWasteMaxProduceNum;
    public Image juvenile0Icon;
    public Image juvenile0Frame;
    public GameObject juvenile0Money;
    public Image juvenile1Icon;
    public Image juvenile1Frame;
    public GameObject juvenile1Money;
    public Image juvenile2Icon;
    public Image juvenile2Frame;
    public GameObject juvenile2Money;
    public Image juvenile3Icon;
    public Image juvenile3Frame;
    public GameObject juvenile3Money;
    public Image adult0Icon;
    public Image adult0Frame;
    public GameObject adult0Money;
    public Image adult1Icon;
    public Image adult1Frame;
    public GameObject adult1Money;
    public Image adult2Icon;
    public Image adult2Frame;
    public GameObject adult2Money;
    public Image adult3Icon;
    public Image adult3Frame;
    public GameObject adult3Money;

    /// <summary>
    /// 鱼的品质
    /// </summary>
    public int _Quality;
    public void Init(AnimalData fishData)
    {
        this.fishData = fishData;
        // 使用SetText注册，支持语言切换时自动更新
        LocalizationManager.SetText(_Name, int.Parse(fishData.name));
        LocalizationManager.SetText(_Describe, int.Parse(fishData.Describe));
        _juvenileSpaceNum.text = ((int)fishData.activitySpace).ToString();
        _juvenileAdultTimeNum.text =Tools.TimeString(fishData.adultTime*60);
        _juvenileFoodCostNum.text = ((int)(fishData.juvenileFoodCost * 60)).ToString()+"/h";
        _juvenileO2CostNum.text = ((int)(fishData.juvenileO2Cost * 60)).ToString() + "/h";
        _juvenileO2MinProportionNum.text = ((int)(fishData.o2MinProportion * 100)).ToString()+"%";
        _juvenileCO2ProduceNum.text = ((int)(fishData.juvenileCO2Produce * 60)).ToString() + "/h";
        _juvenileCO2MaxProportionNum.text = ((int)(fishData.co2MaxProportion * 100)).ToString()+"%";
        _juvenileWasteProduceNum.text = ((int)(fishData.juvenileWasteProduce * 60)).ToString() + "/h";
        _juvenileWasteMaxProduceNum.text = ((int)(fishData.wasteMaxProportion * 100)).ToString()+"%";
        _adultSpaceNum.text = ((int)fishData.activitySpace).ToString();
        _adultReproductionNum.text = Tools.TimeString(fishData.reproductionTime*60);
        _adultFoodCostNum.text = ((int)(fishData.adultFoodCost * 60)).ToString() + "/h";
        _adultO2CostNum.text = ((int)(fishData.adultO2Cost * 60)).ToString() + "/h";
        _adultO2MinProportionNum.text = ((int)(fishData.o2MinProportion * 100)).ToString() + "%";
        _adultCO2ProduceNum.text = ((int)(fishData.adultCO2Produce * 60)).ToString() + "/h";
        _adultCO2MaxProduceNum.text = ((int)(fishData.co2MaxProportion * 100)).ToString() + "%";
        _adultWasteProduceNum.text = ((int)(fishData.adultWasteProduce * 60)).ToString() + "/h";
        _adultWasteMaxProduceNum.text = ((int)(fishData.wasteMaxProportion * 100)).ToString() + "%";

        //icon及icon的价格展示
        if (_Quality>=0)
        {
            juvenile0Icon.sprite = Tools.GetFishIcon(fishData.ID,0,false);
            juvenile1Icon.sprite = Tools.GetFishIcon(fishData.ID, -1, false);
            juvenile2Icon.sprite = Tools.GetFishIcon(fishData.ID, -1, false);
            juvenile3Icon.sprite = Tools.GetFishIcon(fishData.ID, -1, false);
            adult0Icon.sprite= Tools.GetFishIcon(fishData.ID, 0, true);
            adult1Icon.sprite = Tools.GetFishIcon(fishData.ID, -1, true);
            adult2Icon.sprite = Tools.GetFishIcon(fishData.ID, -1, true);
            adult3Icon.sprite = Tools.GetFishIcon(fishData.ID, -1, true);
            juvenile0Frame.sprite= CoreSetting.Instance.GetQualityBgSprite(0);
            juvenile1Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(0);
            juvenile2Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(0);
            juvenile3Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(0);
            adult0Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(0);
            adult1Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(0);
            adult2Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(0);
            adult3Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(0);
            if (juvenile0Money.TryGetComponent<TextMeshProUGUI>(out var juvenile0M))
            {
                juvenile0M.text = Tools.MoneyString(Tools.GetFishPrice(fishData.ID,0,false));
                juvenile0Money.transform.parent.gameObject.SetActive(true);
            }
            if (adult0Money.TryGetComponent<TextMeshProUGUI>(out var adult0M))
            {
                adult0M.text = Tools.MoneyString(Tools.GetFishPrice(fishData.ID, 0, true));
                adult0Money.transform.parent.gameObject.SetActive(true);
            }
            juvenile1Money.transform.parent.gameObject.SetActive(false);
            adult1Money.transform.parent.gameObject.SetActive(false);
            juvenile2Money.transform.parent.gameObject.SetActive(false);
            adult2Money.transform.parent.gameObject.SetActive(false);
            juvenile3Money.transform.parent.gameObject.SetActive(false);
            adult3Money.transform.parent.gameObject.SetActive(false);
        }
        if (_Quality >= 1)
        {
            juvenile1Icon.sprite = Tools.GetFishIcon(fishData.ID, 1, false);
            adult1Icon.sprite = Tools.GetFishIcon(fishData.ID, 1, true);
            juvenile1Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.Blue);
            adult1Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.Blue);

            if (juvenile1Money.TryGetComponent<TextMeshProUGUI>(out var juvenile1M))
            {
                juvenile1M.text = Tools.MoneyString(Tools.GetFishPrice(fishData.ID, 1, false));
                juvenile1Money.transform.parent.gameObject.SetActive(true);
            }
            if (adult1Money.TryGetComponent<TextMeshProUGUI>(out var adult1M))
            {
                adult1M.text = Tools.MoneyString(Tools.GetFishPrice(fishData.ID, 1, true));
                adult1Money.transform.parent.gameObject.SetActive(true);
            }
        }
        if (_Quality >= 2)
        {
            juvenile2Icon.sprite = Tools.GetFishIcon(fishData.ID, 2, false);
            adult2Icon.sprite = Tools.GetFishIcon(fishData.ID, 2, true);
            juvenile2Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.Purple);
            adult2Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.Purple);

            if (juvenile2Money.TryGetComponent<TextMeshProUGUI>(out var juvenile2M))
            {
                juvenile2M.text = Tools.MoneyString(Tools.GetFishPrice(fishData.ID, 2, false));
                juvenile2Money.transform.parent.gameObject.SetActive(true);
            }
            if (adult2Money.TryGetComponent<TextMeshProUGUI>(out var adult2M))
            {
                adult2M.text = Tools.MoneyString(Tools.GetFishPrice(fishData.ID, 2, true));
                adult2Money.transform.parent.gameObject.SetActive(true);
            }
        }
        if (_Quality >= 3)
        {
            juvenile3Icon.sprite = Tools.GetFishIcon(fishData.ID, 3, false);
            adult3Icon.sprite = Tools.GetFishIcon(fishData.ID, 3, true);
            juvenile3Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.Gold);
            adult3Frame.sprite = CoreSetting.Instance.GetQualityBgSprite(ObjectQuality.Gold);

            if (juvenile3Money.TryGetComponent<TextMeshProUGUI>(out var juvenile3M))
            {
                juvenile3M.text = Tools.MoneyString(Tools.GetFishPrice(fishData.ID, 3, false));
                juvenile3Money.transform.parent.gameObject.SetActive(true);
            }
            if (adult3Money.TryGetComponent<TextMeshProUGUI>(out var adult3M))
            {
                adult3M.text = Tools.MoneyString(Tools.GetFishPrice(fishData.ID, 3, true));
                adult3Money.transform.parent.gameObject.SetActive(true);
            }
        }
    }
}
