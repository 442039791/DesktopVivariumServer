using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum UI_BioTankInfo_SellOrSaveType
{
    Sell,
    Save
}

public class UI_BioTankInfo_SellOrSave : ChangeLanugeBaseSUIFW
{
    //public UI_BioTankInfo_SellOrSaveType type;
    //public Button AdultIncrementTenButton;
    //public Button ChildIncrementTenButton;
    //public Button AdultIncrementOneButton;
    //public Button ChildIncrementOneButton;
    //public Button AdultDecrementTenButton;
    //public Button ChildDecrementTenButton;
    //public Button AdultDecrementOneButton;
    //public Button ChildDecrementOneButton;
    //public Button AdultMaxButton;
    //public Button ChildMaxButton;
    //public Button AdultMinButton;
    //public Button ChildMinButton;
    //public Button ConfirmButton;
    //public TextMeshProUGUI AdultCountText;
    //public TextMeshProUGUI ChildCountText;
    //public Button CloseButton;
    //public Fish fish;
    //private int _adultCount;
    //public int adultCount
    //{
    //    get { return _adultCount; }
    //    set
    //    {
    //        if (value < 0)
    //            value = 0;
    //        if (value > fish.AdultNum)
    //            value = (int)fish.AdultNum;
    //        _adultCount = value;
    //        AdultCountText.text = _adultCount.ToString();
    //    }
    //}
    //private int _childCount;
    //public int childCount
    //{
    //    get { return _childCount; }
    //    set
    //    {
    //        if (value < 0)
    //            value = 0;
    //        if (value > fish.JuvenileNum)
    //            value = (int)fish.JuvenileNum;
    //        _childCount = value;
    //        ChildCountText.text = _childCount.ToString();
    //    }
    //}


    //public void Init(Fish _fish)
    //{
    //    fish = _fish;
    //    if (InitCompelete)
    //        return;
    //    base.Init();
    //    AdultIncrementTenButton.AddDeskTopListener(() => { adultCount += 10; });
    //    ChildIncrementTenButton.AddDeskTopListener(() => { childCount += 10; });
    //    AdultIncrementOneButton.AddDeskTopListener(() => { adultCount += 1; });
    //    ChildIncrementOneButton.AddDeskTopListener(() => { childCount += 1; });
    //    AdultDecrementTenButton.AddDeskTopListener(() => { adultCount -= 10; });
    //    ChildDecrementTenButton.AddDeskTopListener(() => { childCount -= 10; });
    //    AdultDecrementOneButton.AddDeskTopListener(() => { adultCount -= 1; });
    //    ChildDecrementOneButton.AddDeskTopListener(() => { childCount -= 1; });
    //    AdultMaxButton.AddDeskTopListener(() => { adultCount = (int)fish.AdultNum; });
    //    ChildMaxButton.AddDeskTopListener(() => { childCount = (int)fish.JuvenileNum; });
    //    AdultMinButton.AddDeskTopListener(() => { adultCount = 0; });
    //    ChildMinButton.AddDeskTopListener(() => { childCount = 0; });
    //    ConfirmButton.AddDeskTopListener(() =>
    //    {
    //        fish.AdultNum -= adultCount;
    //        fish.JuvenileNum -= childCount;
    //        if (type == UI_BioTankInfo_SellOrSaveType.Sell)
    //        {
    //            PlayerManager.Instance.OnMoneyChanged(fish.data.adultPurchasePrice * adultCount + fish.data.juvenilePurchasePrice * childCount);
    //        }
    //        else if (type == UI_BioTankInfo_SellOrSaveType.Save)
    //        {
    //            WarehouseManager.Instance.SaveAnimal(fish.stateData, true, adultCount);
    //            WarehouseManager.Instance.SaveAnimal(fish.stateData, false, childCount);
    //        }
    //    });
    //    CloseButton.AddDeskTopListener(() =>
    //    {
    //        CloseOrReturnUIForms();
    //    });

    //}
}
