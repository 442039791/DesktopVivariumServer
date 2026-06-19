//using System.Collections;
//using System.Collections.Generic;
//using SUIFW;
//using UnityEngine;
//using UnityEngine.UI;

//public class UIButton : MonoBehaviour
//{
//    // Start is called before the first frame update
//    public Button button;

//    void Awake()
//    {
//        button = GetComponentInChildren<Button>();
//        button.onClick.AddListener(setcamera);
//    }

//    public void setcamera()
//    {
        
//        UIManager.GetInstance().ShowUIForms(SysDefine.CardGroup);
        
//        Handvisual.Instance.col.enabled = false;
//        Handvisual.Instance.enabled = false;

//        GlobalSetting.Instance.SetMainCamera(CameraName.UiCamera);
//    }
//}
