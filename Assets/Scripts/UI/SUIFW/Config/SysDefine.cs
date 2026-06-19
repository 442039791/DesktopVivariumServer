/***
 * 
 *    Title: "SUIFW" UI框架项目
 *           主题： 框架本身系统核心参数定义集  
 *    Description: 
 *           功能： 提供本框架范围内的如下系统定义：
 *               1：系统常量。
 *               2：全局性变量
 *               3：系统枚举
 *               4：委托定义
 *               5：框架接口              
 * 
 *    Date: 2017
 *    Version: 0.1版本
 *    Modify Recoder: 
 *    
 *   
 */
using UnityEngine;



    #region 系统枚举
    /// <summary>
    /// UI窗体类型
    /// </summary>
    public enum UIFormsType
    {
        Normal,             // 普通全屏界面(例如主城UI界面)
        Fixed,              // 固定界面(例如“英雄信息条” [HeroTopBar])
        PopUp,              // 弹出模式(小窗口)窗口 (例如：商场、背包、确认窗口等)
    }

    /// <summary>
    /// UI窗体显示类型
    /// </summary>
    public enum UIFormsShowMode
    {
        Normal,             //普通显示
        ReverseChange,      //反向切换      
        HideOther,          //隐藏其他界面 
    }

    /// <summary>
    /// UI窗体透明度类型
    /// </summary>
    public enum UIFormsLucencyType
    {
        Lucency,            //完全透明,但不能穿透。
        Translucence,       //半透明度,不能穿透。
        Impenetrable,       //低透明度,不能穿透,
        Penetrate,          //可以穿透
    }

    public enum AnimationStatus
    {
        Player1_Idle1,
        Player1_Idle2,
        Player1_Run1,
        Player1_Run2,
        Player1_Move1,
        Player1_Move2,
        Player1_Attack_Sweep1,
        Player1_Attack_Sweep2,
        Player1_Attack_Spike1,
        Player1_Attack_Spike2,
        Player1_Buff1,
        Player1_Buff2,
        Player1_Block1,
        Player1_Block2,
        Player1_Attack_Juhe1,
        Player1_Attack_Juhe2,
        Player1_Buff_Block1,
        Player1_Buff_Block2
    }
    #endregion

    /// <summary>
    /// 系统定义静态类
    /// </summary>
    internal static class SysDefine
    {
        #region 系统常量
        public const float SYS_LOGIC_UPDATE_TIME = 0.066f;
        /* 路径常量 */
        public const string SYS_PATH_CANVAS = "Canvas";
        public const string SYS_PATH_SysConfigJson = "SysConfigInfo";
        public const string SYS_PATH_UIFormConfigJson = "UIFormsConfigInfo";

        /* 标签常量 */
        public const string SYS_TAG_CANVAS = "_TagCanvas";
        public const string SYS_TAG_UICAMERA = "_TagUICamera";
        public const string SYS_TAG_MAINCAMERA = "MainCamera";
        /* Canvas节点名称 */
        public const string SYS_CANVAS_NORMAL_NODE_NAME = "Normal";
        public const string SYS_CANVAS_FIXED_NODE_NAME = "Fixed";
        public const string SYS_CANVAS_POPUP_NODE_NAME = "PopUp";
        public const string SYS_CANVAS_UISCRIPTS_NODE_NAME = "_UIScripts";
        public const string SYS_CANVAS_UIMASKPANELS_NODE_NAME = "UIMaskPanels";
        /* 遮罩管理器常量 */
        //完全透明度
        public const float SYS_UIMASK_LUCENCY_COLOR_RGB = 255F / 255F;
        public const float SYS_UIMASK_LUCENCY_COLOR_A = 0F / 255F;
        //半透明度
        public const float SYS_UIMASK_TRANSLUCENCY_COLOR_RGB = 220F / 255F;
        public const float SYS_UIMASK_TRANSLUCENCY_COLOR_A = 50F / 255F;
        //低透明度
        public const float SYS_UIMASK_IMPENETRABLE_COLOR_RGB = 50F / 255F;
        public const float SYS_UIMASK_IMPENETRABLE_COLOR_A = 200F / 255F;
        /// <summary>
        /// UI摄像机，层深增加量
        /// </summary>
        public const int SYS_UICAMERA_DEPTH_INCREMENT = 100;
        #endregion
        /*UI预设名称*/
        public const string CardGroup = "CardGroup";
        public const string LowArea = "LowArea";
        public const string FirePos = "FirePos";
        public const string StaticFirePos = "StatickFirePos";
        public const string BloodUI = "BloodUI";
        public const string ChangeLanugeEventName = "ChangeLanuge";

        public const string CsvDefaultNumber = "0";
        public const string CsvDefaultString = "null";
        public const string DefaultSavePath = "Save/";
        public const string DefaultSaveBioTankDataName = "BioTankSaveData.txt";
        public const string DefaultSaveBioTankLandDataName = "LandSaveData.txt"; // 地形数据文件名，与BioTankSaveData分开
        public const string DefaultSaveDataName = "SaveData.txt";
        public const string DefaultSettingDataName = "SettingData.txt";
        public const string DefaultGatherSaveDataName = "GatherSaveData.txt";
        public const string DefaultGatherUnlockSaveDataName="GatherUnlockSaveData.txt";
        public const string DefaultBluePrintSaveDataName = "BluePrintSaveDate.txt";
        public const string BluePrintIconPath = "BluePrint/BluePrintIcon/";
        public const string DefaultFirstSaveData = "BluePrint/DefaultBioTank/";
        public const string BlockSaveDataPath = "Save/(name_conflict)_save";
        public const string BioTankPrefabName = "BioTankPrefab";
        public const string AnimalPrefabName = "FishPrefab";
        public const string AnimalDisPlayPrefabName = "FishDisPlayPrefab";
        public const string PlantPrefabName = "PlantPrefabName";
        public const string PlayerUIPrefabName = "PlayerUIPrefabName";
        public const string GatherAddressData = "GatherAddressData.json";
        public const string GatherDecorationData = "GatherDecorationData.json";
        public const string GatherAnimalData = "GatherFishData.json";
        public const string GatherPlantData = "GatherPlantData.json";
        public const string GatherAdmissionData = "GatherAdmissionData.json";
        public const string GatherFacilitiesData = "GatherFacilitiesData.json";
        public const string GatherCubeData = "GatherCubeData.json";
        public const string GatherTankData = "GatherTankData.json";
        public const string AnimalConfigData = "FishData.json";
        public const string PlantConfigData = "PlantData.json";
        public const string DecorationConfigData = "DecorationData.json";
        public const string CubeConfigData = "CubeData.json";
        public const string AdmissionConfigData = "AdmissionData.json";
        public const string FacilitiesConfigData = "FacilitiesData.json";
        public const string TankConfigData = "TankData.json";
        public const string HelpConfigData = "Help.json";
        public const string BioTankInfo_FunctionArea_ActionButton = "BioTankInfo_FunctionArea_ActionButton";
        public const string AnimalName = "Fish";
        public const string PlantName = "Plant";
        public const string UI_BioTankInfo = "UI_BioTankInfo";
        public const string UI_BioTankInfo_BaseInfo_info_O2 = "UI_BioTankInfo_BaseInfo_info_O2";
        public const string UI_BioTankInfo_BaseInfo_info_CO2 = "UI_BioTankInfo_BaseInfo_info_CO2";
        public const string UI_BioTankInfo_BaseInfo_info_Waste= "UI_BioTankInfo_BaseInfo_info_Waste";
        public const string UI_BioTankInfo_BaseInfo_info_Vegetable = "UI_BioTankInfo_BaseInfo_info_Vegetable";
        public const string UI_BioTankInfo_BaseInfo_info_Meat = "UI_BioTankInfo_BaseInfo_info_Meat";
        public const string UI_BioTankInfo_BaseInfo_info_PlantNutrition = "UI_BioTankInfo_BaseInfo_info_PlantNutrition";
        public const string UI_BioTankInfo_BaseInfo_info_Size = "UI_BioTankInfo_BaseInfo_info_Size";
        public const string UI_BioTankInfo_CreatureInfo_AnimalInfo ="UI_BioTankInfo_CreatureInfo_AnimalInfo.prefab";
        public const string UI_BioTankInfo_CreatureInfo_PlantInfo="UI_BioTankInfo_CreatureInfo_PlantInfo.prefab";
        public const string UI_BioTankInfo_CreatureInfo_FacilityInfo="UI_BioTankInfo_CreatureInfo_FacilityInfo.prefab";
        public const string UI_BioTankInfo_CreatureInfo_DecorationInfo = "UI_BioTankInfo_CreatureInfo_DecorationInfo.prefab";
        public const string UI_BluePrintnfomation="UI_BluePrintnfomation.prefab";
        public const string UI_BioTankInfo_CreatureInfo_ChangePorF = "UI_BioTankInfo_CreatureInfo_ChangePorF";
        public const string UI_Warehouse = "UI_Warehouse";
        public const string UI_Warehouse_change = "UI_Warehouse_change";
        public const string UI_Warehouse_WarehouseInfo = "UI_Warehouse_WarehouseInfo";
        public const string UI_Warehouse_WareInfo_Info = "UI_Warehouse_WareInfo_Info.prefab";
        public const string UI_Warehouse_WareInfo_Creature_Info = "UI_Warehouse_WareInfo_Creature_Info.prefab";
        public const string UI_BluePrintPrefab_Delete = "UI_BluePrintPrefab_Delete";
        public const string UI_BluePrintPrefab_Save = "UI_BluePrintPrefab_Save";
        public const string UI_BluePrintPrefab_Load = "UI_BluePrintPrefab_Load";
        public const string UI_BluePrint_Add= "UI_BluePrint_Add";
        public const string UI_BluePrint_Delete = "UI_BluePrint_Delete";
        public const string UI_BluePrint_Save = "UI_BluePrint_Save";
        public const string UI_BluePrint_SaveData = "UI_BluePrint_SaveData";
        public const string UI_BluePrint_Save_ClientReturn = "UI_BluePrint_Save_ClientReturn";
        public const string UI_BluePrint_Load = "UI_BluePrint_Load";
        public const string UI_BuyAnimal = "UI_BuyFish";
        public const string UI_BuyPlant = "UI_BuyPlant";
        public const string UI_BuyDecoration = "UI_BuyDecoration";
        public const string UI_CubeGenera = "UI_CubeGenera";
        public const string UI_BioTankInfo_ButtonGroupEvent = "UI_BioTankInfo_ButtonGroupEvent";
        public const string UI_WindPickingInfo_change = "UI_WindPickingInfo_change";
        public const string UI_GatherStateComplete = "UI_GatherStateComplete";
        public const string UI_GatherStateReward = "UI_GatherStateReward";
        public const string UI_MoneyChangeEvent = "Money";
        public const string UI_LllustratedGuideEvent = "LllustratedGuideEvent";
        public const string On_CloseUI_UMMove = "On_CloseUI_UMMove";
        
        /*预设数据*/
        public const string PlayerMoney = "";

        public const string ImageAssetbundle = "Image/";
        public const string AssetbundleSpritePath = "Sprite/";
        public const string AssetbundleMaterialPath = "Material/";
        public const string AssetbundleIconSpritePath = "IconSprite/";
        public const string AnimalSpritePath = "Fish/";
        public const string PlantSpritePath = "Plant/";
        public const string DecorationSpritePath = "Decoration/";
        public const string FacilitiesSpritePath = "Facilities/";
        public const string TankSpritePath = "Tank/";
        public const string AdmissionSpritePath = "Admission/";
        public const string CubeMaterialPath = "Cube/";
        public const string GatherSpritePath = "Gather/";

        public const string PrefabAssetbundle = "GamePrefab/";
        public const string BehaviorTreeDefaultTree = "BT_Fish";
        
        public const string ChangeLauguageEvent = "ChangeLauguage";
        public const string Lauguage_Chinese = "Chinese";
        public const string Lauguage_English = "English";
        public const string Lauguage_Japanese = "Japanese";
        public const string Lauguage_Korean = "Korean";
        
        //public const string Lauuguage_French = "French";
        //public const string Lauuguage_Spanish = "Spanish";
        //public const string Lauuguage_German = "German";
        //public const string Lauuguage_Italian = "Italian";
        //public const string Lauuguage_Russian = "Russian";
        //public const string Lauuguage_Portuguese = "Portuguese";
        //public const string Lauuguage_Arabic = "Arabic";
        //public const string Lauuguage_Turkish = "Turkish";
        //public const string Lauuguage_Thai = "Thai";
        //public const string Lauuguage_Hindi = "Hindi";
        //public const string Lauuguage_Indonesian = "Indonesian";
        //public const string Lauuguage_Malay = "Malay";
        //public const string Lauuguage_Vietnamese = "Vietnamese";
    /*测试*/
        public const string Test_QC2 = "Test_QC2";
        public const string TestLog = "TestLog";

        public const string UI_Title_ButtonGroup_Button = "UI_Title_ButtonGroup_Button_";
    public const string UI_Title_ShowOther = "UI_Title_ShowOther";
    public const string UI_Title_Money = "UI_Title_Money";
    public const string UI_BioTankInfo_SellOrSave = "UI_BioTankInfo_SellOrSave";
    public const string DefaultBluePrintIconName = "BluePrintIcon.jpg";
    public const string DefaultBluePrintDataName = "BluePrintData.txt";
    public const string BluePrintDataPath = "BluePrint/BluePrintData/";
    public const string BluePrintMainPath = "BluePrint/";
    public const string UI_BluePrintSetName = "UI_BluePrintSetName";

    public const string InitUI_BioTankInfo = "InitUI_BioTankInfo";
    public const string UI_BioTank_Edit = "UI_BioTank_Edit";

    public const string UI_BluePrint_Save_BioTank = "UI_BluePrint_Save_BioTank";
    public const string BluePrintBioTankDefaultName = "BluePrintBioTank.txt";
    public const string BluePrintBioTankIconPath = "BluePrintBIoTank/BluePrintBIoTankIcon/";
    public const string BluePrintBioTankSaveDataPath = "BluePrintBIoTank/BluePrintBIoTank/";
    public const string BluePrintBioTankDataPath = "BluePrintBIoTank/";
    public const string BluePrintBioTankIconDefaultName = "BluePrintBIoTankIcon.jpg";
    public const string BluePrintBioTankSaveDataDefaultName = "BluePrintBioTankSaveData.txt";
    public const string UI_Biotank_AddBiotank_BioInfoPrefab = "UI_Biotank_AddBiotank_BioInfo.prefab";

    public const string UI_Biotank_AddBiotank_Remove = "UI_Biotank_AddBiotank_Remove";
    public const string UI_Biotank_AddBiotank_Revise = "UI_Biotank_AddBiotank_Revise";
    public const string UI_Biotank_AddBiotank_Save = "UI_Biotank_AddBiotank_Save";
    public const string UI_Biotank_List_Init = "UI_Biotank_List_Init";
    public const string UI_Biotank_List = "UI_Biotank_List";
    public const string UI_Biotank_Move = "UI_Biotank_Move";
    public const string UI_Biotank_AddBiotank = "UI_Biotank_AddBiotank";

    public const string UI_WareHouse_List= "UI_WareHouse_List";
    public const string cangetprefab = "cangetprefab.prefab";
    public const string UI_ItemIconPrefab = "ItemIcon.prefab";
    public const string UI_HelpInfoPrefab = "HelpInfo.prefab";
    public const string UI_GatherSelect = "UI_GatherSelect";
    public const string TestMessage = "TestMessage";
    public const string UI_CreatureInfo_Select = "UI_CreatureInfo_Select";
    
    public const string UI_IllustratedGuide= "UI_IllustratedGuide";
    public const string UI_IllustratedGuide_Entry = "UI_IllustratedGuide_Entry.prefab";
    public const string IllustratedGuide_Open = "IllustratedGuide_Open";
    public const string IllustratedGuide_Close = "IllustratedGuide_Close";
    public const string IllustratedGuide_Unlock_Animal= "IllustratedGuide_Unlock_Fish";
    public const string IllustratedGuide_Unlock_Plant= "IllustratedGuide_Unlock_Plant";
    public const string UI_IllustratedGuide_Detail = "UI_IllustratedGuide_Detail";

    // 模组管理界面
    public const string UI_ModManager = "UI_ModManager";
    public const string UI_ModManager_Item = "UI_ModManager_Item.prefab";
    public const string UI_ModManager_Refresh = "UI_ModManager_Refresh";

    public static Vector2Int DefaultWindowSize = new Vector2Int(1280, 720);
    public const int WindowMoveY = 10;
    public const int WindowMoveX = 10;
    public const float Price_Magnification_White = 0.8f;
    public const float Price_Magnification_Blue = 1f;
    public const float Price_Magnification_Purple = 1.05f;
    public const float Price_Magnification_Gold = 1.2f;
    public const int AnimalMaxAnimalGrow = 30;  // FishMaxFishGrow的原始值

    public const int UI_Language_GorwMax_ID = 78;

    public const int Language_Free = 19;

    #region 全局性变量（方法）


        //得到"UI窗体预设"配置文件(XML)路径
        public static string GetUIFormsConfigFilePath()
        {
            string logPath = null;

            //Android 或者Iphone 环境
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                logPath = Application.streamingAssetsPath + "/UIFormsConfigInfo.xml";
            }
            //Win环境
            else
            {
                logPath = "file://" + Application.streamingAssetsPath + "/UIFormsConfigInfo.xml";
            }

            return logPath;
        }
        //得到"UI窗体预设"配置文件(XML)的根节点名称
        public static string GetUIFormsConfigFileRootNodeName()
        {
            string strReturnXMLRootNodeName = null;

            strReturnXMLRootNodeName = "UIFormsConfigInfo";
            return strReturnXMLRootNodeName;
        }

        /* 由于使用Json 技术解析大量中文信息，所以不用本xml 路径了 */
        //得到"UI窗体预设"配置文件(XML)路径
        //public static string GetLauguageConfigFilePath()
        //{
        //    string logPath = null;

        //    //Android 或者Iphone 环境
        //    if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        //    {
        //        logPath = Application.streamingAssetsPath + "/ChineseLauguageConfigData.xml";
        //    }
        //    //Win环境
        //    else
        //    {
        //        logPath = "file://" + Application.streamingAssetsPath + "/ChineseLauguageConfigData.xml";
        //    }

        //    return logPath;
        //}
        ////得到“中文XML”配置文件根节点名称
        //public static string GetLauguageConfigFileRootNodeName()
        //{
        //    string strReturnXMLRootNodeName = null;

        //    strReturnXMLRootNodeName = "ChineseConfigData";
        //    return strReturnXMLRootNodeName;
        //}

        #endregion

        #region 委托定义

        #endregion

        #region 框架接口

        #endregion
    }//Class_end

/// <summary>
/// UI（窗体）类型
/// </summary>
    [System.Serializable]
    public class UIType
    {
        //是否需要清空“反向切换”
        public bool IsClearReverseChange = false;
        //UI窗体类型
        public UIFormsType UIForms_Type = UIFormsType.Normal;
        //UI窗体显示类型
        public UIFormsShowMode UIForms_ShowMode = UIFormsShowMode.Normal;
        //UI窗体透明度类型
        public UIFormsLucencyType UIForms_LucencyType = UIFormsLucencyType.Lucency;
    }

