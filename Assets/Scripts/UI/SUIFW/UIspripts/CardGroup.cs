//using System.Collections;
//using System.Collections.Generic;
//using SUIFW;
//using UnityEngine;

//public class CardGroup : BaseUIForms
//{
//    private List<UIScrollbar> list;
//    void Awake()
//    {
//        list = new List<UIScrollbar>();
//        base.CurrentUIType.UIForms_ShowMode = UIFormsShowMode.Normal;
//        RigisteButtonObjectEvent("Btn_ReturnBat", ReturnBat);
//        foreach (var val in GetComponentsInChildren<UIScrollbar>())
//        {
//            list.Add( val);
            
//        }
//    }
//    public override void Display()
//    {

//            base.Display();
//            GetComponentInChildren<UiScrollbarRaycastHit>().enabled = true;
//            foreach (var val in list)
//            {
//                switch (val.DeckName)
//                {
//                case "CardDeck":
//                    val.Init(GlobalSetting.Instance.Player.deck.cards);
//                    break;
//                case "SaveDeck":
//                    val.Init(GlobalSetting.Instance.Player.deck.cards);
//                    break;
//                default:
//                    val.Init(new List<CardAsset>());
//                    break;
//                }
                
//            }

//    }

//    private void ReturnBat(GameObject go)
//    {
        
//       // GlobalSetting.Instance.SetMainCamera(CameraName.MainCamera);
//        foreach (var val in list)
//        {
//            switch (val.DeckName)
//            {
//                case "CardDeck":
//                    GlobalSetting.Instance.Player.deck.cards = val.ReturnAssets();
//                    break;
//                case "SaveDeck":
//                    GlobalSetting.Instance.Player.deck.cards = val.ReturnAssets();
//                    break;

//            }
            
//            val.ClearAndExit();
//        }
//        ShowUIForms(SysDefine.LowArea);

//        //Handvisual.Instance.col.enabled = true;
//        //Handvisual.Instance.enabled = true;
//        GetComponentInChildren<UiScrollbarRaycastHit>().enabled = false;
//        GlobalSetting.Instance.ChangeCardScenceSetting(true);
//        CloseOrReturnUIForms();
//        //UIManager.GetInstance().CloseOrReturnUIForms(SysDefine.CardGroup);
//    }
//}
