using Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum UI_Title_ButtonGroup_Button_Type
{
    Biotank,
    Wildpicking,
    Warehouse,
    Bill,
    IllustratedGuide,
    Settings,
    Help,
    Log,
}
public class UI_Title : ChangeLanugeBase
{
    public UI_Title_ButtonGroup_Button_Type currentButtonType;
    public UI_Title_ButtonGroup_Button UI_Title_ButtonGroup_Button_Biotank;
    public UI_Title_ButtonGroup_Button UI_Title_ButtonGroup_Button_Wildpicking;
    public UI_Title_ButtonGroup_Button UI_Title_ButtonGroup_Button_Warehouse;
    public UI_Title_ButtonGroup_Button UI_Title_ButtonGroup_Button_Bill;
    public UI_Title_ButtonGroup_Button UI_Title_ButtonGroup_Button_IllustratedGuide;
    public UI_Title_ButtonGroup_Button UI_Title_ButtonGroup_Button_Settings;
    public UI_Title_ButtonGroup_Button UI_Title_ButtonGroup_Button_Help;
    public UI_Title_ButtonGroup_Button UI_Title_ButtonGroup_Button_Log;

    public Transform UI_Main_Biotank;
    public Transform UI_Main_IllustratedGuide;
    public Transform UI_Main_WareHouse;
    public Transform UI_Main_Bill;
    public Transform UI_Main_Wildpicking;
    public Transform UI_Main_Settings;
    public Transform UI_Main_Help;
    public Transform UI_Main_Log;

    public Dictionary<UI_Title_ButtonGroup_Button_Type, Transform> UI_Main_Dict = new();
    public Dictionary<UI_Title_ButtonGroup_Button_Type, UI_Title_ButtonGroup_Button> UI_Button_Dict = new ();
    public Dictionary<UI_Title_ButtonGroup_Button_Type, UIBase> UI_Information_Dict = new ();
    public Dictionary<UI_Title_ButtonGroup_Button_Type, int> UI_Information_Name_DictID = new ();
    public Dictionary<UI_Title_ButtonGroup_Button_Type, string> UI_Information_Name_Dic = new ();
    public TextMeshProUGUI UI_Title_Money;
    public UIBase UI_Main_Biotank_Information;
    public UIBase UI_Main_IllustratedGuide_Information;
    public UIBase UI_Main_Helpe_Information;
    public UIBase UI_Main_WareHouse_Information;
    public UIBase UI_Main_Bill_Information;
    public UIBase UI_Main_Wildpicking_Information;
    public UIBase UI_Main_Settings_Information;
    public UIBase UI_Main_Log_Information;
    public TextMeshProUGUI UI_Title_Name;
    public Button UI_Title_Button_Minimize;
    public new void Init()
    {
        if (InitCompelete)
            return;
        event_manager.instance.add_event_listener(SysDefine.ChangeLauguageEvent, OnLanguageChanged);
        event_manager.instance.add_event_listener(SysDefine.UI_GatherStateComplete, OnGatherStateComplete);
        event_manager.instance.add_event_listener(SysDefine.UI_GatherStateReward, OnGatherStateReward);
        TutorialManager.Instance.OnTutorialCompleted += OnTutorialCompleted;
        UI_Title_Button_Minimize.AddDeskTopListener(() => { event_manager.instance.dispatch_UIevent(SysDefine.On_CloseUI_UMMove,null); WallpaperManager.Instance.MinimizeWindow(); });
        UI_Main_Dict.Add(UI_Title_ButtonGroup_Button_Type.Biotank, UI_Main_Biotank);
        UI_Main_Dict.Add(UI_Title_ButtonGroup_Button_Type.Wildpicking, UI_Main_Wildpicking);
        UI_Main_Dict.Add(UI_Title_ButtonGroup_Button_Type.Warehouse, UI_Main_WareHouse);
        UI_Main_Dict.Add(UI_Title_ButtonGroup_Button_Type.Bill, UI_Main_Bill);
        UI_Main_Dict.Add(UI_Title_ButtonGroup_Button_Type.IllustratedGuide, UI_Main_IllustratedGuide);
        UI_Main_Dict.Add(UI_Title_ButtonGroup_Button_Type.Settings, UI_Main_Settings);
        UI_Main_Dict.Add(UI_Title_ButtonGroup_Button_Type.Help, UI_Main_Help);
        UI_Main_Dict.Add(UI_Title_ButtonGroup_Button_Type.Log, UI_Main_Log);

        UI_Button_Dict.Add(UI_Title_ButtonGroup_Button_Type.Biotank, UI_Title_ButtonGroup_Button_Biotank);
        UI_Button_Dict.Add(UI_Title_ButtonGroup_Button_Type.Wildpicking, UI_Title_ButtonGroup_Button_Wildpicking);
        UI_Button_Dict.Add(UI_Title_ButtonGroup_Button_Type.Warehouse, UI_Title_ButtonGroup_Button_Warehouse);
        UI_Button_Dict.Add(UI_Title_ButtonGroup_Button_Type.Bill, UI_Title_ButtonGroup_Button_Bill);
        UI_Button_Dict.Add(UI_Title_ButtonGroup_Button_Type.IllustratedGuide, UI_Title_ButtonGroup_Button_IllustratedGuide);
        UI_Button_Dict.Add(UI_Title_ButtonGroup_Button_Type.Settings, UI_Title_ButtonGroup_Button_Settings);
        UI_Button_Dict.Add(UI_Title_ButtonGroup_Button_Type.Help, UI_Title_ButtonGroup_Button_Help);
        UI_Button_Dict.Add(UI_Title_ButtonGroup_Button_Type.Log, UI_Title_ButtonGroup_Button_Log);

        UI_Information_Dict.Add(UI_Title_ButtonGroup_Button_Type.Biotank, UI_Main_Biotank_Information);
        UI_Information_Dict.Add(UI_Title_ButtonGroup_Button_Type.Wildpicking, UI_Main_Wildpicking_Information);
        UI_Information_Dict.Add(UI_Title_ButtonGroup_Button_Type.Warehouse, UI_Main_WareHouse_Information);
        UI_Information_Dict.Add(UI_Title_ButtonGroup_Button_Type.Bill, UI_Main_Bill_Information);
        UI_Information_Dict.Add(UI_Title_ButtonGroup_Button_Type.IllustratedGuide, UI_Main_IllustratedGuide_Information);
        UI_Information_Dict.Add(UI_Title_ButtonGroup_Button_Type.Help, UI_Main_Helpe_Information);
        UI_Information_Dict.Add(UI_Title_ButtonGroup_Button_Type.Settings, UI_Main_Settings_Information);
        UI_Information_Dict.Add(UI_Title_ButtonGroup_Button_Type.Log, UI_Main_Log_Information);

        UI_Information_Name_DictID.Add(UI_Title_ButtonGroup_Button_Type.Biotank, 2);
        UI_Information_Name_DictID.Add(UI_Title_ButtonGroup_Button_Type.Wildpicking, 17);
        UI_Information_Name_DictID.Add(UI_Title_ButtonGroup_Button_Type.Warehouse, -1);
        UI_Information_Name_DictID.Add(UI_Title_ButtonGroup_Button_Type.Bill, -1);
        UI_Information_Name_DictID.Add(UI_Title_ButtonGroup_Button_Type.IllustratedGuide, 131);
        UI_Information_Name_DictID.Add(UI_Title_ButtonGroup_Button_Type.Settings, 18);
        UI_Information_Name_DictID.Add(UI_Title_ButtonGroup_Button_Type.Help, 129);
        UI_Information_Name_DictID.Add(UI_Title_ButtonGroup_Button_Type.Log, 18);

        UI_Information_Name_Dic.Add(UI_Title_ButtonGroup_Button_Type.Biotank, "Biotank");
        UI_Information_Name_Dic.Add(UI_Title_ButtonGroup_Button_Type.Wildpicking, "Wildpicking");
        UI_Information_Name_Dic.Add(UI_Title_ButtonGroup_Button_Type.Warehouse, "Warehouse");
        UI_Information_Name_Dic.Add(UI_Title_ButtonGroup_Button_Type.Bill, "Bill");
        UI_Information_Name_Dic.Add(UI_Title_ButtonGroup_Button_Type.IllustratedGuide, "Illustrated Guide");
        UI_Information_Name_Dic.Add(UI_Title_ButtonGroup_Button_Type.Settings, "Settings");
        UI_Information_Name_Dic.Add(UI_Title_ButtonGroup_Button_Type.Help, "Help");
        UI_Information_Name_Dic.Add(UI_Title_ButtonGroup_Button_Type.Log, "Log");
        base.Init(transform);
        event_manager.instance.add_UIevent_listener(SysDefine.UI_MoneyChangeEvent, on_moneychange);
        event_manager.instance.add_UIevent_listener(SysDefine.UI_LllustratedGuideEvent, LllustratedGuide);
        UI_Title_ButtonGroup_Button_Biotank.button.AddDeskTopListener(() => { ShowUI_Main(UI_Title_ButtonGroup_Button_Type.Biotank); });
        UI_Title_ButtonGroup_Button_Wildpicking.button.AddDeskTopListener(() => { ShowUI_Main(UI_Title_ButtonGroup_Button_Type.Wildpicking); });
        UI_Title_ButtonGroup_Button_Warehouse.button.AddDeskTopListener(() => { ShowUI_Main(UI_Title_ButtonGroup_Button_Type.Warehouse); });
        UI_Title_ButtonGroup_Button_Bill.button.AddDeskTopListener(() => { ShowUI_Main(UI_Title_ButtonGroup_Button_Type.Bill); });
        UI_Title_ButtonGroup_Button_IllustratedGuide.button.AddDeskTopListener(() => { ShowUI_Main(UI_Title_ButtonGroup_Button_Type.IllustratedGuide); });
        UI_Title_ButtonGroup_Button_Settings.button.AddDeskTopListener(() => { ShowUI_Main(UI_Title_ButtonGroup_Button_Type.Settings); });
        UI_Title_ButtonGroup_Button_Help.button.AddDeskTopListener(() => { ShowUI_Main(UI_Title_ButtonGroup_Button_Type.Help); });
        // 隐藏日志按钮和相关UI
        UI_Title_ButtonGroup_Button_Log.gameObject.SetActive(false);
        if (UI_Main_Log != null) UI_Main_Log.gameObject.SetActive(false);

        ShowUI_Main(UI_Title_ButtonGroup_Button_Type.Biotank);
        StartCoroutine(FistOnLanguageChanged());

        // 启动首次引导
        StartCoroutine(CheckAndStartFirstTutorial());

        // 检查是否需要在上线时触发属性引导（玩家下线再上线时）
        CheckAndStartAttributeTutorialOnLogin();
    }

    /// <summary>
    /// 玩家上线时检查是否需要触发属性引导
    /// 触发条件：biotank_edit_plant_tutorial已完成 + biotank_attribute_tutorial未完成
    /// </summary>
    private void CheckAndStartAttributeTutorialOnLogin()
    {
        // 检查步骤27引导是否已完成
        if (!TutorialSaveManager.Instance.IsFlowCompleted("biotank_edit_plant_tutorial"))
        {
            return;
        }

        // 检查属性引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("biotank_attribute_tutorial"))
        {
            return;
        }

        Debug.Log("[UI_Title] 玩家上线，检测到biotank_edit_plant_tutorial已完成但biotank_attribute_tutorial未完成，触发属性引导");
        CreateBiotankAttributeTutorial();
    }
    private IEnumerator FistOnLanguageChanged()
    {
        yield return LocalizationManager.langDatabaseisInit;
        OnLanguageChanged("", "");
    }
    public void InitMoneyEvent()
    {
        event_manager.instance.add_UIevent_listener(SysDefine.UI_MoneyChangeEvent, on_moneychange);
    }
    public  void on_moneychange(string name, object udata)
    {
        UI_Title_Money.text = Tools.MoneyString((int)(float.Parse(((string)udata))));
    }
    public  void OnLanguageChanged(string name, object udata)
    {
        //string langCode = (string)udata;
        foreach (var item in UI_Information_Name_DictID)
        {
            UI_Information_Name_Dic[item.Key] = LocalizationManager.GetCurrentLanguageByID(item.Value);
        }
        UI_Title_Name.text = UI_Information_Name_Dic[type];
    }

    /// <summary>
    /// 野采状态变为完成时的事件处理
    /// 当玩家已在野采界面等待时，野采完成后触发引导
    /// </summary>
    public void OnGatherStateComplete(string name, object udata)
    {
        // 检查当前是否在野采界面
        if (currentButtonType == UI_Title_ButtonGroup_Button_Type.Wildpicking)
        {
            CheckAndStartWildpickingRewardTutorial();
        }
    }

    /// <summary>
    /// 野采状态变为奖励已打开时的事件处理
    /// 当玩家已在野采界面时，打开奖励后触发引导
    /// </summary>
    public void OnGatherStateReward(string name, object udata)
    {
        // 检查当前是否在野采界面
        if (currentButtonType == UI_Title_ButtonGroup_Button_Type.Wildpicking)
        {
            CheckAndStartWildpickingPlaceRewardTutorial();
        }
    }

    /// <summary>
    /// 引导流程完成事件处理
    /// 用于链接多个引导流程
    /// </summary>
    private void OnTutorialCompleted(string flowID)
    {
        // 当野采奖励放入生态箱引导完成后，触发生态箱引导（步骤15-20）
        if (flowID == "wildpicking_place_reward_tutorial")
        {
            // 使用协程延迟触发，等待UI界面切换完成
            StartCoroutine(DelayedCheckAndStartBiotankTutorial());
        }
        // 当生态箱引导（步骤20）完成后，触发植物放置引导（步骤21-27）
        else if (flowID == "biotank_tutorial")
        {
            // 使用协程延迟触发，等待UI界面完全激活
            StartCoroutine(DelayedTryStartPlantPlacementTutorial());
        }
        // 当植物放置引导（27步）完成后，等待UI_BioTank_Edit关闭后触发属性引导
        else if (flowID == "biotank_edit_plant_tutorial")
        {
            // 标记需要在编辑界面关闭后触发属性引导
            _pendingAttributeTutorial = true;
            Debug.Log("[UI_Title] biotank_edit_plant_tutorial完成，设置_pendingAttributeTutorial=true，等待编辑界面关闭");
        }
        // 当属性引导（步骤33）完成后，触发调整引导（步骤34-37）
        else if (flowID == "biotank_attribute_tutorial")
        {
            // 使用协程延迟触发，等待UI界面完全激活
            StartCoroutine(DelayedCheckAndStartAdjustTutorial());
        }
    }

    /// <summary>
    /// 延迟触发生态箱引导，等待UI界面切换完成
    /// </summary>
    private IEnumerator DelayedCheckAndStartBiotankTutorial()
    {
        // 等待两帧，确保UI界面切换和激活完成
        yield return null;
        yield return null;
        CheckAndStartBiotankTutorial();
    }

    /// <summary>
    /// 延迟触发植物放置引导，等待UI_BioTank_Edit界面完全激活
    /// </summary>
    private IEnumerator DelayedTryStartPlantPlacementTutorial()
    {
        // 等待UI_BioTank_Edit界面打开，最多等待2秒
        float waitTime = 0f;
        float maxWaitTime = 2f;
        while (UI_BioTank_Edit.CurrentInstance == null && waitTime < maxWaitTime)
        {
            yield return null;
            waitTime += Time.deltaTime;
        }
        
        // 额外等待两帧，确保UI完全初始化
        yield return null;
        yield return null;
        
        TryStartPlantPlacementTutorial();
    }

    /// <summary>
    /// 延迟触发调整引导，等待UI界面完全激活
    /// </summary>
    private IEnumerator DelayedCheckAndStartAdjustTutorial()
    {
        // 等待两帧，确保UI界面完全激活
        yield return null;
        yield return null;
        CheckAndStartAdjustTutorial();
    }

    /// <summary>
    /// 尝试启动植物放置引导（步骤21-27）
    /// 方式A：biotank_tutorial（步骤20）完成时立即触发
    /// 条件：引导未完成 + 仓库有植物
    /// </summary>
    private void TryStartPlantPlacementTutorial()
    {
        // 检查引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("biotank_edit_plant_tutorial"))
        {
            Debug.Log("[UI_Title] 植物放置引导已完成，跳过");
            return;
        }

        // 检查仓库中是否有植物
        var warehouseService = ServiceLocator.Get<IWarehouseService>();
        if (warehouseService == null)
        {
            Debug.LogWarning("[UI_Title] 无法获取IWarehouseService");
            return;
        }

        var plantList = warehouseService.GetWarehouseBaseDatas(ItemType.Plant);
        if (plantList == null || plantList.Count == 0)
        {
            Debug.Log("[UI_Title] 仓库中没有植物，跳过植物放置引导");
            return;
        }

        // 检查UI_BioTank_Edit是否打开且plantlist已初始化
        if (UI_BioTank_Edit.CurrentInstance != null && 
            UI_BioTank_Edit.CurrentInstance.plantlist != null && 
            UI_BioTank_Edit.CurrentInstance.plantlist.listController != null)
        {
            Debug.Log("[UI_Title] biotank_tutorial完成，UI_BioTank_Edit已打开且plantlist已初始化，触发植物放置引导");
            UI_BioTank_Edit.CurrentInstance.StartPlantPlacementTutorialFromExternal();
        }
        else if (UI_BioTank_Edit.CurrentInstance != null)
        {
            // UI_BioTank_Edit已打开但plantlist未初始化，等待初始化完成
            Debug.Log("[UI_Title] UI_BioTank_Edit已打开但plantlist未初始化，启动等待协程");
            StartCoroutine(WaitForPlantlistAndStartTutorial());
        }
        else
        {
            // UI_BioTank_Edit未打开，在UI_Title中直接创建引导
            Debug.Log("[UI_Title] biotank_tutorial完成，UI_BioTank_Edit未打开，直接创建植物放置引导");
            CreatePlantPlacementTutorialInTitle();
        }
    }
    
    /// <summary>
    /// 等待plantlist初始化完成后启动植物放置引导
    /// </summary>
    private IEnumerator WaitForPlantlistAndStartTutorial()
    {
        float waitTime = 0f;
        float maxWaitTime = 3f;
        
        while (waitTime < maxWaitTime)
        {
            if (UI_BioTank_Edit.CurrentInstance != null && 
                UI_BioTank_Edit.CurrentInstance.plantlist != null && 
                UI_BioTank_Edit.CurrentInstance.plantlist.listController != null &&
                UI_BioTank_Edit.CurrentInstance.plantlist.listController.transform.childCount > 0)
            {
                Debug.Log("[UI_Title] plantlist已初始化且有子项，触发植物放置引导");
                UI_BioTank_Edit.CurrentInstance.StartPlantPlacementTutorialFromExternal();
                yield break;
            }
            
            yield return null;
            waitTime += Time.deltaTime;
        }
        
        Debug.LogWarning("[UI_Title] 等待plantlist初始化超时，尝试直接创建引导");
        CreatePlantPlacementTutorialInTitle();
    }

    /// <summary>
    /// 在UI_Title中创建植物放置引导（当UI_BioTank_Edit未打开时使用）
    /// </summary>
    private void CreatePlantPlacementTutorialInTitle()
    {
        Debug.Log("[UI_Title] 创建植物放置引导流程");

        // 创建引导流程
        TutorialFlow flow = new TutorialFlow(
            "biotank_edit_plant_tutorial",
            "生态箱植物放置引导",
            "引导玩家在生态箱编辑界面中放置植物"
        );

        // 第一段对话 - 使用本地化文本ID 5014
        string dialog1Text = LocalizationManager.GetCurrentLanguageByID(5014);
        if (string.IsNullOrEmpty(dialog1Text))
        {
            dialog1Text = "看起来你的仓库里有一些植物！\n\n植物可以为生态箱提供氧气和美化环境。";
        }
        DialogStep dialog1Step = new DialogStep(
            "step_plant_placement_dialog1",
            "植物放置引导1",
            dialog1Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(dialog1Step);

        // 第二段对话 - 使用本地化文本ID 5015
        string dialog2Text = LocalizationManager.GetCurrentLanguageByID(5015);
        if (string.IsNullOrEmpty(dialog2Text))
        {
            dialog2Text = "点击植物标签页，然后选择一株植物。\n\n你可以把它放置到生态箱中，让你的生态系统更加完整！";
        }
        DialogStep dialog2Step = new DialogStep(
            "step_plant_placement_dialog2",
            "植物放置引导2",
            dialog2Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(dialog2Step);

        // 第三段对话 - 使用本地化文本ID 5016
        string dialog3Text = LocalizationManager.GetCurrentLanguageByID(5016);
        if (string.IsNullOrEmpty(dialog3Text))
        {
            dialog3Text = "植物会随着时间生长成熟。\n\n成熟的植物可以出售获得生态币，快去尝试吧！";
        }
        DialogStep dialog3Step = new DialogStep(
            "step_plant_placement_dialog3",
            "植物放置引导3",
            dialog3Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(dialog3Step);

        // 步骤4: 点击植物的Place按钮（使用延迟查找）
        ClickUIStep placeClickStep = new ClickUIStep(
            "step_plant_placement_click_place",
            "点击植物放置按钮",
            () => FindPlantPlaceButtonInTitle(),
            canSkip: false
        );
        placeClickStep.Pointer = new PointerConfig { Offset = new Vector2(1520, 700), Direction = TutorialPointer.PointerDirection.UpRight };
        flow.AddStep(placeClickStep);
        Debug.Log("[UI_Title] 添加植物Place按钮点击步骤（延迟查找）");

        // 步骤5-7: 编辑操作说明（三段对话）
        // 第一段对话 - 使用本地化文本ID 5017
        string editDialog1Text = LocalizationManager.GetCurrentLanguageByID(5017);
        if (string.IsNullOrEmpty(editDialog1Text))
        {
            editDialog1Text = "你可以使用WASD或方向键来移动物品的位置。\n\n按X键向上移动，按Z键向下移动。";
        }
        DialogStep editDialog1Step = new DialogStep(
            "step_edit_operation_dialog1",
            "编辑操作说明1",
            editDialog1Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(editDialog1Step);

        // 第二段对话 - 使用本地化文本ID 5018
        string editDialog2Text = LocalizationManager.GetCurrentLanguageByID(5018);
        if (string.IsNullOrEmpty(editDialog2Text))
        {
            editDialog2Text = "当物品位置合适时，按空格键确认放置。\n\n如果位置不合适会显示红色提示。";
        }
        DialogStep editDialog2Step = new DialogStep(
            "step_edit_operation_dialog2",
            "编辑操作说明2",
            editDialog2Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(editDialog2Step);

        // 第三段对话 - 使用本地化文本ID 5019
        string editDialog3Text = LocalizationManager.GetCurrentLanguageByID(5019);
        if (string.IsNullOrEmpty(editDialog3Text))
        {
            editDialog3Text = "你也可以使用删除模式来移除已放置的物品。\n\n现在试试把植物放到生态箱中吧！";
        }
        DialogStep editDialog3Step = new DialogStep(
            "step_edit_operation_dialog3",
            "编辑操作说明3",
            editDialog3Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(editDialog3Step);

        Debug.Log($"[UI_Title] 植物放置引导流程创建完成，共 {flow.Steps.Count} 个步骤");

        // 启动引导
        TutorialManager.Instance.StartTutorial(flow);
    }

    /// <summary>
    /// 在UI_Title中查找植物放置按钮（当从UI_Title触发引导时使用）
    /// </summary>
    private Button FindPlantPlaceButtonInTitle()
    {
        // 检查UI_BioTank_Edit是否已打开
        if (UI_BioTank_Edit.CurrentInstance == null)
        {
            Debug.LogWarning("[UI_Title] FindPlantPlaceButtonInTitle: UI_BioTank_Edit未打开");
            return null;
        }

        // 获取plantlist
        var plantlist = UI_BioTank_Edit.CurrentInstance.plantlist;
        if (plantlist == null || plantlist.listController == null)
        {
            Debug.LogWarning("[UI_Title] FindPlantPlaceButtonInTitle: plantlist或listController为空");
            return null;
        }

        // ListController直接管理Row，Row作为其子项
        Transform listControllerTransform = plantlist.listController.transform;

        if (listControllerTransform.childCount == 0)
        {
            Debug.LogWarning("[UI_Title] FindPlantPlaceButtonInTitle: ListController没有子项(Row)");
            return null;
        }

        // 遍历所有Row查找第一个植物
        for (int rowIdx = 0; rowIdx < listControllerTransform.childCount; rowIdx++)
        {
            Transform row = listControllerTransform.GetChild(rowIdx);
            if (row == null || !row.gameObject.activeSelf) continue;

            // 遍历Row中的每个植物项
            for (int itemIdx = 0; itemIdx < row.childCount; itemIdx++)
            {
                Transform plantItem = row.GetChild(itemIdx);
                if (plantItem == null || !plantItem.gameObject.activeSelf) continue;

                // 尝试多种可能的Place按钮路径
                string[] possiblePlacePaths = new string[]
                {
                    "SaveAndSell/Place",
                    "Place",
                    "Buttons/Place"
                };

                foreach (string placePath in possiblePlacePaths)
                {
                    Transform placeTransform = plantItem.Find(placePath);
                    if (placeTransform != null)
                    {
                        Button placeButton = placeTransform.GetComponent<Button>();
                        if (placeButton != null)
                        {
                            Debug.Log($"[UI_Title] FindPlantPlaceButtonInTitle: 找到植物放置按钮，路径={placePath}");
                            return placeButton;
                        }
                    }
                }

                // 如果上述路径都没找到，尝试递归查找Place按钮
                Transform placeBtn = FindButtonInHierarchy(plantItem, "Place");
                if (placeBtn != null)
                {
                    Button button = placeBtn.GetComponent<Button>();
                    if (button != null)
                    {
                        Debug.Log("[UI_Title] FindPlantPlaceButtonInTitle: 通过递归查找找到植物放置按钮");
                        return button;
                    }
                }

                Debug.LogWarning("[UI_Title] FindPlantPlaceButtonInTitle: 在植物项中未找到Place按钮");
                return null;
            }
        }

        Debug.LogWarning("[UI_Title] FindPlantPlaceButtonInTitle: 未找到任何植物项");
        return null;
    }

    /// <summary>
    /// 是否有待触发的属性引导
    /// 当biotank_edit_plant_tutorial完成后设置为true，
    /// 在UI_BioTank_Edit关闭后检查并触发
    /// </summary>
    private bool _pendingAttributeTutorial = false;

    /// <summary>
    /// 检查并启动属性引导（由外部在UI_BioTank_Edit关闭时调用）
    /// </summary>
    public void CheckAndStartAttributeTutorialAfterEditClose()
    {
        Debug.Log($"[UI_Title] CheckAndStartAttributeTutorialAfterEditClose被调用，_pendingAttributeTutorial={_pendingAttributeTutorial}");

        if (!_pendingAttributeTutorial)
        {
            Debug.Log("[UI_Title] _pendingAttributeTutorial为false，跳过属性引导");
            return;
        }

        _pendingAttributeTutorial = false;

        // 检查此引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("biotank_attribute_tutorial"))
        {
            Debug.Log("[UI_Title] biotank_attribute_tutorial已完成，跳过");
            return;
        }

        // 检查当前是否处于Biotank界面
        if (currentButtonType != UI_Title_ButtonGroup_Button_Type.Biotank)
        {
            Debug.Log("[UI_Title] 当前不在Biotank界面，不触发属性引导");
            return;
        }

        Debug.Log("[UI_Title] 触发生态箱属性引导");
        CreateBiotankAttributeTutorial();
    }

    /// <summary>
    /// 创建生态箱属性引导（步骤28-33）
    /// 三段对话说明属性 + 点击素食Deal按钮 + 一段对话 + 点击植物营养Deal按钮
    /// </summary>
    private void CreateBiotankAttributeTutorial()
    {
        Debug.Log("[UI_Title] 创建生态箱属性引导流程");

        // 创建引导流程
        TutorialFlow flow = new TutorialFlow(
            "biotank_attribute_tutorial",
            "生态箱属性引导",
            "引导玩家了解生态箱的属性系统"
        );

        // 步骤28: 第一段对话 - 使用本地化文本ID 5020
        string dialog28Text = LocalizationManager.GetCurrentLanguageByID(5020);
        if (string.IsNullOrEmpty(dialog28Text))
        {
            dialog28Text = "生态箱需要维持多种属性的平衡。\n\n氧气和二氧化碳由动植物相互调节，保持适当的比例对生物健康至关重要。";
        }
        DialogStep step28 = new DialogStep(
            "step_attribute_dialog1",
            "属性说明1",
            dialog28Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step28);

        // 步骤29: 第二段对话 - 使用本地化文本ID 5021
        string dialog29Text = LocalizationManager.GetCurrentLanguageByID(5021);
        if (string.IsNullOrEmpty(dialog29Text))
        {
            dialog29Text = "素食是草食动物的主要食物来源。\n\n当素食不足时，草食动物的健康会下降，记得及时补充！";
        }
        DialogStep step29 = new DialogStep(
            "step_attribute_dialog2",
            "属性说明2",
            dialog29Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step29);

        // 步骤30: 第三段对话 - 使用本地化文本ID 5022
        string dialog30Text = LocalizationManager.GetCurrentLanguageByID(5022);
        if (string.IsNullOrEmpty(dialog30Text))
        {
            dialog30Text = "植物营养是植物生长的必需品。\n\n营养不足会影响植物的生长速度，让我们来看看如何补充营养吧！";
        }
        DialogStep step30 = new DialogStep(
            "step_attribute_dialog3",
            "属性说明3",
            dialog30Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step30);

        // 步骤31: 点击素食Deal按钮（使用延迟查找）
        // 路径: UI_BioTankInfo_BaseInfo_info_Vegetable/Deal (在UI_Main_Biotank_Information中)
        ClickUIStep step31 = new ClickUIStep(
            "step_attribute_click_vegetable",
            "点击素食添加按钮",
            () => FindAttributeDealButton("Vegetable"),
            canSkip: false
        );
        step31.Pointer = new PointerConfig { Offset = new Vector2(465, 1000), Direction = TutorialPointer.PointerDirection.Up };
        flow.AddStep(step31);
        Debug.Log("[UI_Title] 成功添加步骤31：点击素食Deal按钮（延迟查找）");

        // 步骤32: 一段对话 - 使用本地化文本ID 5023
        string dialog32Text = LocalizationManager.GetCurrentLanguageByID(5023);
        if (string.IsNullOrEmpty(dialog32Text))
        {
            dialog32Text = "点击这里可以为生态箱添加素食。\n\n确保你的动物们永远不会饿肚子！";
        }
        DialogStep step32 = new DialogStep(
            "step_attribute_dialog4",
            "素食引导说明",
            dialog32Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step32);

        // 步骤33: 点击植物营养Deal按钮（使用延迟查找）
        // 路径: UI_BioTankInfo_BaseInfo_info_PlantNutrition/Deal
        ClickUIStep step33 = new ClickUIStep(
            "step_attribute_click_nutrition",
            "点击植物营养添加按钮",
            () => FindAttributeDealButton("PlantNutrition"),
            canSkip: false
        );
        step33.Pointer = new PointerConfig { Offset = new Vector2(465, 830), Direction = TutorialPointer.PointerDirection.Up };
        flow.AddStep(step33);
        Debug.Log("[UI_Title] 成功添加步骤33：点击植物营养Deal按钮（延迟查找）");

        Debug.Log($"[UI_Title] 生态箱属性引导流程创建完成，共 {flow.Steps.Count} 个步骤");

        // 启动引导
        TutorialManager.Instance.StartTutorial(flow);
    }

    /// <summary>
    /// 查找属性Deal按钮
    /// </summary>
    /// <param name="attributeType">属性类型名称（如 Vegetable, PlantNutrition）</param>
    /// <returns>找到的Button，未找到返回null</returns>
    private Button FindAttributeDealButton(string attributeType)
    {
        // 查找Canvas
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogWarning("[UI_Title] FindAttributeDealButton: 未找到Canvas对象");
            return null;
        }

        string infoName = $"UI_BioTankInfo_BaseInfo_info_{attributeType}";

        // 尝试多种可能的路径
        string[] possiblePaths = new string[]
        {
            // 路径1: 完整路径（Deal按钮）
            $"Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information/info/{infoName}/Deal",
            // 路径2: 完整路径（Add按钮）
            $"Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information/info/{infoName}/Add",
            // 路径3: 简化路径（Deal按钮）
            $"Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/UI_Main_Biotank_Information/info/{infoName}/Deal",
            // 路径4: 简化路径（Add按钮）
            $"Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/UI_Main_Biotank_Information/info/{infoName}/Add"
        };

        Transform buttonTransform = null;
        foreach (string path in possiblePaths)
        {
            buttonTransform = canvas.transform.Find(path);
            if (buttonTransform != null)
            {
                Debug.Log($"[UI_Title] FindAttributeDealButton: 通过路径找到按钮 - {path}");
                break;
            }
        }

        // 如果上述路径都没找到，尝试递归查找
        if (buttonTransform == null)
        {
            // 查找UI_Main_Biotank
            Transform biotankTransform = canvas.transform.Find("Panel/Normal/UI/UI_Main/UI_Main_Biotank");
            if (biotankTransform != null)
            {
                buttonTransform = FindAttributeButtonInHierarchy(biotankTransform, infoName);
            }
        }

        if (buttonTransform == null)
        {
            Debug.LogWarning($"[UI_Title] FindAttributeDealButton: 未找到 {attributeType} 的按钮");
            return null;
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"[UI_Title] FindAttributeDealButton: {attributeType} 按钮未找到Button组件");
            return null;
        }

        Debug.Log($"[UI_Title] FindAttributeDealButton: 成功找到 {attributeType} 的按钮");
        return button;
    }

    /// <summary>
    /// 在层级中递归查找属性按钮
    /// </summary>
    private Transform FindAttributeButtonInHierarchy(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == targetName)
            {
                // 找到目标对象，查找其中的Deal或Add按钮
                Transform dealButton = child.Find("Deal");
                if (dealButton != null)
                {
                    return dealButton;
                }

                Transform addButton = child.Find("Add");
                if (addButton != null)
                {
                    return addButton;
                }

                // 如果没有Deal/Add子对象，检查自身是否有Button组件
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    return child;
                }

                // 查找第一个Button组件
                btn = child.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    return btn.transform;
                }
            }

            // 递归查找
            Transform result = FindAttributeButtonInHierarchy(child, targetName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    /// <summary>
    /// 在子对象中递归查找按钮（备用方法）
    /// </summary>
    private Transform FindButtonInChildren(Transform parent, string infoName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == infoName)
            {
                // 找到目标info对象，查找其中的Deal或Add按钮
                Transform dealButton = child.Find("Deal");
                if (dealButton != null) return dealButton;

                Transform addButton = child.Find("Add");
                if (addButton != null) return addButton;

                // 查找第一个Button组件
                Button btn = child.GetComponentInChildren<Button>();
                if (btn != null) return btn.transform;
            }

            // 递归查找
            Transform result = FindButtonInChildren(child, infoName);
            if (result != null) return result;
        }
        return null;
    }
    private UI_Title_ButtonGroup_Button_Type type = UI_Title_ButtonGroup_Button_Type.Biotank;
    public void ShowUI_Main(UI_Title_ButtonGroup_Button_Type buttonType)
    {
        type = buttonType;
        // 实时获取当前语言的标题文本，而不是使用缓存的字典值
        int langId = UI_Information_Name_DictID[buttonType];
        string titleText = LocalizationManager.GetCurrentLanguageByID(langId);
        // 如果本地化文本为空或ID无效(-1)，则使用默认英文
        UI_Title_Name.text = string.IsNullOrEmpty(titleText) ? UI_Information_Name_Dic[buttonType] : titleText;
        foreach (var item in UI_Button_Dict)
        {
            if (item.Key == buttonType)
            {
                item.Value.ShowButton(true);
            }
            else
            {
                item.Value.ShowButton(false);
            }
        }

        foreach (var item in UI_Main_Dict)
        {
            if (item.Key == buttonType)
            {
                if (item.Value == null)
                    continue;
                item.Value.gameObject.SetActive(true);
            }
            else
            {
                if (item.Value == null)
                    continue;
                item.Value.gameObject.SetActive(false);
            }
        }

        foreach (var item in UI_Information_Dict)
        {
            if (item.Key == buttonType)
            {
                if (item.Value == null)
                    continue;
                UI_Information_Dict[buttonType].Init();
                UI_Information_Dict[buttonType].OnShow = true;
            }
            else
            {
                if (item.Value == null)
                    continue;
                UI_Information_Dict[item.Key].OnShow = false;
            }
        }

        currentButtonType = buttonType;

        // 检查是否需要触发野采相关引导
        if (buttonType == UI_Title_ButtonGroup_Button_Type.Wildpicking)
        {
            CheckAndStartWildpickingRewardTutorial();
            CheckAndStartWildpickingPlaceRewardTutorial();
        }
        // 检查是否需要触发生态箱引导（步骤15-20）
        // 触发条件：步骤12完成 + 当前处于Biotank界面
        else if (buttonType == UI_Title_ButtonGroup_Button_Type.Biotank)
        {
            CheckAndStartBiotankTutorial();
            // 检查是否需要触发调整引导（步骤34-37）
            // 触发条件：属性引导完成 + 调整引导未完成
            CheckAndStartAdjustTutorialOnBiotankShow();
        }
    }

    UI_IlluGuide uI_IlluGuide;
    /// <summary>
    /// 快速打开图鉴界面
    /// </summary>
    public void LllustratedGuide(string name, object udata)
    {
        (RewardType rewardType, string id) = ((RewardType, string))udata;
        ShowUI_Main(UI_Title_ButtonGroup_Button_Type.IllustratedGuide);
        if (uI_IlluGuide == null)
        {
            uI_IlluGuide = MonoSingleton<UI_IlluGuide>.Instance;
        }
        uI_IlluGuide.SetSelected(rewardType, id);
        uI_IlluGuide.ListScroll();
    }

    /// <summary>
    /// 检查并启动首次引导
    /// </summary>
    private IEnumerator CheckAndStartFirstTutorial()
    {
        // 等待本地化系统初始化完成
        yield return LocalizationManager.langDatabaseisInit;

        // 等待一帧确保UI完全初始化
        yield return null;

        // 检查是否是首次进入游戏（使用统一的引导ID）
        if (!TutorialSaveManager.Instance.IsFlowCompleted("game_opening_tutorial"))
        {
            // 特殊逻辑：检查步骤8是否完成
            // 如果步骤8未完成，清除所有前面步骤的进度，强制从步骤1重新开始
            bool isStep8Completed = TutorialSaveManager.Instance.IsStepCompleted("game_opening_tutorial", "step_opening_goto_wildpicking");

            if (!isStep8Completed)
            {
                // 检查是否有任何步骤已完成（说明玩家中途退出）
                bool hasAnyStepCompleted =
                    TutorialSaveManager.Instance.IsStepCompleted("game_opening_tutorial", "step_opening_welcome") ||
                    TutorialSaveManager.Instance.IsStepCompleted("game_opening_tutorial", "step_opening_guide") ||
                    TutorialSaveManager.Instance.IsStepCompleted("game_opening_tutorial", "step_opening_ready") ||
                    TutorialSaveManager.Instance.IsStepCompleted("game_opening_tutorial", "step_opening_click_wildpicking_intro") ||
                    TutorialSaveManager.Instance.IsStepCompleted("game_opening_tutorial", "step_opening_click_wildpicking") ||
                    TutorialSaveManager.Instance.IsStepCompleted("game_opening_tutorial", "step_opening_wild_gather_intro1") ||
                    TutorialSaveManager.Instance.IsStepCompleted("game_opening_tutorial", "step_opening_wild_gather_intro2");

                if (hasAnyStepCompleted)
                {
                    Debug.Log("[UI_Title] 步骤8未完成，清除引导进度，从步骤1重新开始");
                    // 清除整个引导流程的进度
                    TutorialSaveManager.Instance.ResetFlowProgress("game_opening_tutorial");
                }
            }

            CreateFirstMainUITutorial();
        }
    }

    /// <summary>
    /// 创建首次主界面引导
    /// </summary>
    private void CreateFirstMainUITutorial()
    {
        Debug.Log("[UI_Title] 创建游戏开场引导流程");

        // 创建引导流程（使用统一的FlowID）
        TutorialFlow flow = new TutorialFlow(
            "game_opening_tutorial",
            "游戏开场引导",
            "玩家首次进入游戏时的欢迎引导"
        );

        // 步骤1: 欢迎对话 - 使用本地化文本ID 5000
        string welcomeText = LocalizationManager.GetCurrentLanguageByID(5000);
        if (string.IsNullOrEmpty(welcomeText))
        {
            welcomeText = "欢迎来到生态缸世界！\n\n在这里，你将创建和管理自己的微型生态系统。";
        }

        DialogStep welcomeStep = new DialogStep(
            "step_opening_welcome",
            "欢迎对话",
            welcomeText,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f, // 不自动完成,等待玩家点击
            canSkip: false
        );
        flow.AddStep(welcomeStep);

        // 步骤2: 引导说明 - 使用本地化文本ID 5001
        string guideText = LocalizationManager.GetCurrentLanguageByID(5001);
        if (string.IsNullOrEmpty(guideText))
        {
            guideText = "让我们开始你的生态之旅吧！\n\n点击继续，我将带你了解基本操作。";
        }

        DialogStep guideStep = new DialogStep(
            "step_opening_guide",
            "引导说明",
            guideText,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f, // 不自动完成,等待玩家点击
            canSkip: false
        );
        flow.AddStep(guideStep);

        // 步骤3: 准备开始 - 使用本地化文本ID 5002
        string readyText = LocalizationManager.GetCurrentLanguageByID(5002);
        if (string.IsNullOrEmpty(readyText))
        {
            readyText = "准备好了吗？\n\n点击继续开始你的冒险！";
        }

        DialogStep readyStep = new DialogStep(
            "step_opening_ready",
            "准备开始",
            readyText,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f, // 不自动完成,等待玩家点击
            canSkip: false
        );
        flow.AddStep(readyStep);

        // 步骤4: 引导说明 - 告诉玩家接下来要点击野外采集按钮 - 使用本地化文本ID 5003
        string clickGuideIntroText = LocalizationManager.GetCurrentLanguageByID(5003);
        if (string.IsNullOrEmpty(clickGuideIntroText))
        {
            clickGuideIntroText = "接下来，让我们去野外采集吧！\n\n在野外你可以采集各种植物和资源。";
        }

        DialogStep clickGuideIntroStep = new DialogStep(
            "step_opening_click_wildpicking_intro",
            "野外采集引导说明",
            clickGuideIntroText,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f, // 不自动完成,等待玩家点击
            canSkip: false
        );
        flow.AddStep(clickGuideIntroStep);

        // 步骤5: 等待玩家点击野外采集按钮
        ClickUIStep clickGuideStep = new ClickUIStep(
            "step_opening_click_wildpicking",
            "点击野外采集按钮",
            UI_Title_ButtonGroup_Button_Wildpicking.button,
            canSkip: false
        );
        // 箭头指向右上角
        clickGuideStep.Pointer = new PointerConfig { Offset = new Vector2(1050, 1200), Direction = TutorialPointer.PointerDirection.UpRight };
        flow.AddStep(clickGuideStep);

        // 步骤6: 第一段野采介绍 - 使用本地化文本ID 5004
        string wildGatherIntro1Text = LocalizationManager.GetCurrentLanguageByID(5004);
        if (string.IsNullOrEmpty(wildGatherIntro1Text))
        {
            wildGatherIntro1Text = "在野采中,你将遇到各种独特的动植物。\n\n赶快出发吧,看看大自然为你准备了什么惊喜!";
        }

        DialogStep wildGatherIntro1Step = new DialogStep(
            "step_opening_wild_gather_intro1",
            "野采介绍1",
            wildGatherIntro1Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f, // 不自动完成,等待玩家点击
            canSkip: false
        );
        flow.AddStep(wildGatherIntro1Step);

        // 步骤7: 第二段野采介绍 - 使用本地化文本ID 5005
        string wildGatherIntro2Text = LocalizationManager.GetCurrentLanguageByID(5005);
        if (string.IsNullOrEmpty(wildGatherIntro2Text))
        {
            wildGatherIntro2Text = "现在让我们一起前往野外探索吧！\n\n点击前往按钮开始你的野采之旅。";
        }

        DialogStep wildGatherIntro2Step = new DialogStep(
            "step_opening_wild_gather_intro2",
            "野采介绍2",
            wildGatherIntro2Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f, // 不自动完成,等待玩家点击
            canSkip: false
        );
        flow.AddStep(wildGatherIntro2Step);

        // 步骤8: 点击Goto按钮前往野采
        // 查找Canvas下的Goto按钮
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform gotoTransform = canvas.transform.Find("Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/info/Goto");
            if (gotoTransform != null)
            {
                Button gotoButton = gotoTransform.GetComponent<Button>();
                if (gotoButton != null)
                {
                    ClickUIStep gotoClickStep = new ClickUIStep(
                        "step_opening_goto_wildpicking",
                        "点击前往野采",
                        gotoButton,
                        canSkip: false
                    );
                    gotoClickStep.Pointer = new PointerConfig { Offset = new Vector2(552, 517), Direction = TutorialPointer.PointerDirection.Down };
                    flow.AddStep(gotoClickStep);
                    Debug.Log("[UI_Title] 成功添加野采Goto按钮点击步骤");
                }
                else
                {
                    Debug.LogWarning("[UI_Title] 在路径 Canvas/Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/info/Goto 未找到Button组件");
                }
            }
            else
            {
                Debug.LogWarning("[UI_Title] 未找到路径: Canvas/Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/info/Goto");
            }
        }
        else
        {
            Debug.LogWarning("[UI_Title] 未找到Canvas对象");
        }

        Debug.Log($"[UI_Title] 引导流程创建完成，共 {flow.Steps.Count} 个步骤");

        // 启动引导
        TutorialManager.Instance.StartTutorial(flow);
    }

    /// <summary>
    /// 检查并启动野采完成引导
    /// 触发条件：打开野采界面且处于已完成未打开奖励状态(GatherState.complete)
    /// </summary>
    private void CheckAndStartWildpickingRewardTutorial()
    {
        // 检查Gather单例是否存在
        if (Gather.Instance == null || !Gather.Instance.IsInited())
        {
            return;
        }

        // 检查是否处于已完成未打开奖励状态
        if (Gather.Instance.State != Gather.GatherState.complete)
        {
            return;
        }

        // 检查此引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("wildpicking_reward_tutorial"))
        {
            return;
        }

        // 特殊逻辑：检查步骤11是否完成
        // 如果步骤11未完成，清除步骤9和10的进度，强制从步骤9重新开始
        bool isStep11Completed = TutorialSaveManager.Instance.IsStepCompleted("wildpicking_reward_tutorial", "step_wildpicking_reward_click_get");

        if (!isStep11Completed)
        {
            // 检查是否有步骤9或10已完成（说明玩家中途退出）
            bool hasAnyStepCompleted =
                TutorialSaveManager.Instance.IsStepCompleted("wildpicking_reward_tutorial", "step_wildpicking_reward_dialog1") ||
                TutorialSaveManager.Instance.IsStepCompleted("wildpicking_reward_tutorial", "step_wildpicking_reward_dialog2");

            if (hasAnyStepCompleted)
            {
                Debug.Log("[UI_Title] 步骤11未完成，清除野采完成引导进度，从步骤9重新开始");
                // 清除整个引导流程的进度
                TutorialSaveManager.Instance.ResetFlowProgress("wildpicking_reward_tutorial");
            }
        }

        Debug.Log("[UI_Title] 触发野采完成引导");
        CreateWildpickingRewardTutorial();
    }

    /// <summary>
    /// 创建野采未领奖引导（两段对话）
    /// </summary>
    private void CreateWildpickingRewardTutorial()
    {
        Debug.Log("[UI_Title] 创建野采未领奖引导流程");

        // 创建引导流程
        TutorialFlow flow = new TutorialFlow(
            "wildpicking_reward_tutorial",
            "野采未领奖引导",
            "引导玩家在野采完成后领取奖励"
        );

        // 步骤1: 第一段对话 - 使用本地化文本ID 5006
        string dialog1Text = LocalizationManager.GetCurrentLanguageByID(5006);
        if (string.IsNullOrEmpty(dialog1Text))
        {
            dialog1Text = "太棒了！你的野采已经完成了！\n\n看看你这次收获了什么宝贝吧。";
        }

        DialogStep dialog1Step = new DialogStep(
            "step_wildpicking_reward_dialog1",
            "野采完成提示",
            dialog1Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(dialog1Step);

        // 步骤2: 第二段对话 - 使用本地化文本ID 5007
        string dialog2Text = LocalizationManager.GetCurrentLanguageByID(5007);
        if (string.IsNullOrEmpty(dialog2Text))
        {
            dialog2Text = "点击下方的奖励物品，就可以将它们收入仓库啦！\n\n快去领取你的战利品吧！";
        }

        DialogStep dialog2Step = new DialogStep(
            "step_wildpicking_reward_dialog2",
            "领取奖励引导",
            dialog2Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(dialog2Step);

        // 步骤3: 点击宝箱Get按钮
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform getTransform = canvas.transform.Find("Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/box/Get");
            if (getTransform != null)
            {
                Button getButton = getTransform.GetComponent<Button>();
                if (getButton != null)
                {
                    ClickUIStep getClickStep = new ClickUIStep(
                        "step_wildpicking_reward_click_get",
                        "点击宝箱领取奖励",
                        getButton,
                        canSkip: false
                    );
                    getClickStep.Pointer = new PointerConfig { Offset = new Vector2(540, 567), Direction = TutorialPointer.PointerDirection.Up };
                    flow.AddStep(getClickStep);
                    Debug.Log("[UI_Title] 成功添加宝箱Get按钮点击步骤");
                }
                else
                {
                    Debug.LogWarning("[UI_Title] 在路径 Canvas/Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/box/Get 未找到Button组件");
                }
            }
            else
            {
                Debug.LogWarning("[UI_Title] 未找到路径: Canvas/Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/box/Get");
            }
        }
        else
        {
            Debug.LogWarning("[UI_Title] 未找到Canvas对象");
        }

        Debug.Log($"[UI_Title] 野采完成引导流程创建完成，共 {flow.Steps.Count} 个步骤");

        // 启动引导
        TutorialManager.Instance.StartTutorial(flow);
    }

    /// <summary>
    /// 检查并启动野采奖励放入生态箱引导
    /// 触发条件：打开野采界面且处于已打开奖励未领取奖励状态(GatherState.reward)
    /// </summary>
    private void CheckAndStartWildpickingPlaceRewardTutorial()
    {
        // 检查Gather单例是否存在
        if (Gather.Instance == null || !Gather.Instance.IsInited())
        {
            return;
        }

        // 检查是否处于已打开奖励未领取奖励状态
        if (Gather.Instance.State != Gather.GatherState.reward)
        {
            return;
        }

        // 检查此引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("wildpicking_place_reward_tutorial"))
        {
            return;
        }

        // 特殊逻辑：如果步骤13（点击Get按钮）已完成（说明玩家中途下线再上线）
        // 则直接标记整个引导流程为已完成，不再触发
        bool isStep13Completed = TutorialSaveManager.Instance.IsStepCompleted("wildpicking_place_reward_tutorial", "step_wildpicking_place_reward_click_get");

        if (isStep13Completed)
        {
            Debug.Log("[UI_Title] 步骤13已完成，玩家中途下线再上线，直接标记引导流程为已完成");
            TutorialSaveManager.Instance.MarkFlowCompleted("wildpicking_place_reward_tutorial");
            return;
        }

        Debug.Log("[UI_Title] 触发野采奖励放入生态箱引导");
        CreateWildpickingPlaceRewardTutorial();
    }

    /// <summary>
    /// 创建野采奖励放入生态箱引导（步骤12-13）
    /// </summary>
    private void CreateWildpickingPlaceRewardTutorial()
    {
        Debug.Log("[UI_Title] 创建野采奖励放入生态箱引导流程");

        // 创建引导流程
        TutorialFlow flow = new TutorialFlow(
            "wildpicking_place_reward_tutorial",
            "野采奖励放入生态箱引导",
            "引导玩家将野采的奖励放入生态箱中"
        );

        // 步骤12: 说明将奖励放入生态箱 - 使用本地化文本ID 5008
        string dialogText = LocalizationManager.GetCurrentLanguageByID(5008);
        if (string.IsNullOrEmpty(dialogText))
        {
            dialogText = "选择你想要放入生态箱的奖励物品！\n\n点击它们，就可以将这些动植物直接放入生态箱中。";
        }

        DialogStep dialogStep = new DialogStep(
            "step_wildpicking_place_reward_dialog",
            "放入生态箱引导",
            dialogText,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(dialogStep);

        // 步骤13: 点击奖励界面的Get按钮
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform getTransform = canvas.transform.Find("Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/reward/Get");
            if (getTransform != null)
            {
                Button getButton = getTransform.GetComponent<Button>();
                if (getButton != null)
                {
                    ClickUIStep getClickStep = new ClickUIStep(
                        "step_wildpicking_place_reward_click_get",
                        "点击Get按钮领取奖励",
                        getButton,
                        canSkip: false
                    );
                    getClickStep.Pointer = new PointerConfig { Offset = new Vector2(540, 482), Direction = TutorialPointer.PointerDirection.Up };
                    flow.AddStep(getClickStep);
                    Debug.Log("[UI_Title] 成功添加奖励界面Get按钮点击步骤");
                }
                else
                {
                    Debug.LogWarning("[UI_Title] 在路径 Canvas/Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/reward/Get 未找到Button组件");
                }
            }
            else
            {
                Debug.LogWarning("[UI_Title] 未找到路径: Canvas/Panel/Normal/UI/UI_Main/UI_Main_Wildpicking/reward/Get");
            }
        }
        else
        {
            Debug.LogWarning("[UI_Title] 未找到Canvas对象");
        }

        // 步骤14: 点击生态箱按钮
        ClickUIStep biotankClickStep = new ClickUIStep(
            "step_wildpicking_place_reward_click_biotank",
            "点击生态箱按钮",
            UI_Title_ButtonGroup_Button_Biotank.button,
            canSkip: false
        );
        biotankClickStep.Pointer = new PointerConfig { Offset = new Vector2(1045, 1360), Direction = TutorialPointer.PointerDirection.UpRight };
        flow.AddStep(biotankClickStep);
        Debug.Log("[UI_Title] 成功添加生态箱按钮点击步骤");

        Debug.Log($"[UI_Title] 野采奖励放入生态箱引导流程创建完成，共 {flow.Steps.Count} 个步骤");

        // 启动引导
        TutorialManager.Instance.StartTutorial(flow);
    }

    /// <summary>
    /// 检查并启动生态箱引导（步骤15-20）
    /// 触发条件：步骤13（点击Get按钮）完成 + 当前处于UI_Main_Biotank界面
    /// </summary>
    public void CheckAndStartBiotankTutorial()
    {
        // 检查此引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("biotank_tutorial"))
        {
            return;
        }

        // 检查步骤13（点击Get按钮）是否已完成
        bool isFlowCompleted = TutorialSaveManager.Instance.IsFlowCompleted("wildpicking_place_reward_tutorial");
        bool isStep13Completed = TutorialSaveManager.Instance.IsStepCompleted("wildpicking_place_reward_tutorial", "step_wildpicking_place_reward_click_get");

        if (!isFlowCompleted && !isStep13Completed)
        {
            return;
        }

        // 检查当前是否处于Biotank界面
        if (currentButtonType != UI_Title_ButtonGroup_Button_Type.Biotank)
        {
            return;
        }

        // 特殊逻辑：如果步骤19b（第一个动物放置按钮点击）已完成，直接标记整个引导为完成
        bool isStep19bCompleted = TutorialSaveManager.Instance.IsStepCompleted("biotank_tutorial", "step_place_animal_click1");
        if (isStep19bCompleted)
        {
            TutorialSaveManager.Instance.MarkFlowCompleted("biotank_tutorial");
            return;
        }

        CreateBiotankTutorial();
    }

    /// <summary>
    /// 创建生态箱引导（步骤15-20）
    /// 步骤15：仓库入口说明
    /// 步骤16：编辑按钮说明
    /// 步骤17：点击编辑按钮
    /// 步骤18：仓库介绍
    /// 步骤19：放置第一只动物
    /// 步骤20：放置第二只动物
    /// </summary>
    private void CreateBiotankTutorial()
    {

        // 创建引导流程
        TutorialFlow flow = new TutorialFlow(
            "biotank_tutorial",
            "生态箱引导",
            "引导玩家了解生态箱相关功能"
        );

        // 步骤15: 说明仓库入口 - 使用本地化文本ID 5009
        string dialog15Text = LocalizationManager.GetCurrentLanguageByID(5009);
        if (string.IsNullOrEmpty(dialog15Text))
        {
            dialog15Text = "这里是仓库入口！\n\n在仓库中，你可以查看和管理所有收集到的动植物和物品。点击这里随时进入仓库吧！";
        }
        DialogStep step15 = new DialogStep(
            "step_warehouse_entrance_dialog",
            "仓库入口说明",
            dialog15Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step15);

        // 步骤16: 说明编辑按钮 - 使用本地化文本ID 5010
        string dialog16Text = LocalizationManager.GetCurrentLanguageByID(5010);
        if (string.IsNullOrEmpty(dialog16Text))
        {
            dialog16Text = "点击这个按钮可以进入编辑模式！\n\n在编辑模式中，你可以自由地在生态箱中放置、移动或删除动植物和装饰物。";
        }
        DialogStep step16 = new DialogStep(
            "step_edit_button_dialog",
            "编辑按钮说明",
            dialog16Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step16);

        // 步骤17: 点击编辑按钮（使用函数延迟查找）
        // 路径: Canvas/Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information/info/Vertical Layout Group/Edit/Button 
        ClickUIStep step17 = new ClickUIStep(
            "step_edit_button_click",
            "点击编辑按钮",
            () => FindEditButton(),
            canSkip: false
        );
        step17.Pointer = new PointerConfig { Offset = new Vector2(815, 1242), Direction = TutorialPointer.PointerDirection.UpRight };
        flow.AddStep(step17);

        // 步骤18: 仓库介绍 - 使用本地化文本ID 5011
        string dialog18Text = LocalizationManager.GetCurrentLanguageByID(5011);
        if (string.IsNullOrEmpty(dialog18Text))
        {
            dialog18Text = "这就是仓库！\n\n仓库会存储你收集到的所有物品，同时你可以将仓库中的动植物和装饰物放置到生态箱中。";
        }
        DialogStep step18 = new DialogStep(
            "step_warehouse_intro_dialog",
            "仓库介绍",
            dialog18Text,
            new Vector2(420f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step18);

        // 步骤19a: 放置第一只动物说明 - 使用本地化文本ID 5012
        string dialog19aText = LocalizationManager.GetCurrentLanguageByID(5012);
        if (string.IsNullOrEmpty(dialog19aText))
        {
            dialog19aText = "点击放置按钮，将这只动物放入生态箱中！\n\n让我们先把第一只动物安置到它的新家吧。";
        }
        DialogStep step19a = new DialogStep(
            "step_place_animal_dialog1",
            "放置第一只动物说明",
            dialog19aText,
            new Vector2(420f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step19a);

        // 步骤19b: 点击第一个动物的放置按钮（使用延迟查找）
        // 路径: UI_BioTank_Edit/Scroll View/Viewport/Content/ExpandUI/Content/UI_Main_Biotank_Animal/Scroll View/Viewport/Content/ListControl/Row(Clone)/UI_Warehouse_WareInfo_Creature_Info(Clone)/SaveAndSell/Place
        ClickUIStep step19b = new ClickUIStep(
            "step_place_animal_click1",
            "点击第一个动物放置按钮",
            () => FindAnimalPlaceButton(0),
            canSkip: false
        );
        step19b.Pointer = new PointerConfig { Offset = new Vector2(1533, 740), Direction = TutorialPointer.PointerDirection.UpRight };
        flow.AddStep(step19b);

        // 步骤20a: 放置第二只动物说明 - 使用本地化文本ID 5013
        string dialog20aText = LocalizationManager.GetCurrentLanguageByID(5013);
        if (string.IsNullOrEmpty(dialog20aText))
        {
            dialog20aText = "很好！现在把第二只动物也放进去！\n\n这样它们就可以在生态箱中一起生活了。";
        }
        DialogStep step20a = new DialogStep(
            "step_place_animal_dialog2",
            "放置第二只动物说明",
            dialog20aText,
            new Vector2(420f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step20a);

        // 步骤20b: 点击第二个动物的放置按钮（使用延迟查找）
        // 放置第一个后，第二个会变成第一个位置，所以还是查找索引0
        ClickUIStep step20b = new ClickUIStep(
            "step_place_animal_click2",
            "点击第二个动物放置按钮",
            () => FindAnimalPlaceButton(0),
            canSkip: false
        );
        step20b.Pointer = new PointerConfig { Offset = new Vector2(1533, 740), Direction = TutorialPointer.PointerDirection.UpRight };
        flow.AddStep(step20b);

        // 启动引导
        TutorialManager.Instance.StartTutorial(flow);
    }

    /// <summary>
    /// 查找动物放置按钮
    /// </summary>
    /// <param name="index">动物在列表中的索引（放置后列表会更新，所以通常传0获取第一个）</param>
    /// <returns>找到的Button，未找到返回null</returns>
    private Button FindAnimalPlaceButton(int index)
    {
        // 优先使用静态实例获取UI_BioTank_Edit
        UI_BioTank_Edit bioTankEditInstance = UI_BioTank_Edit.CurrentInstance;
        Transform bioTankEditTransform = null;

        if (bioTankEditInstance != null)
        {
            bioTankEditTransform = bioTankEditInstance.transform;
        }
        else
        {
            // 备用方案：通过GameObject.Find查找
            GameObject bioTankEdit = GameObject.Find("UI_BioTank_Edit");
            if (bioTankEdit != null)
            {
                bioTankEditTransform = bioTankEdit.transform;
            }
        }

        if (bioTankEditTransform == null)
        {
            return null;
        }

        // 尝试多种可能的路径查找ListControl
        string[] possibleListControlPaths = new string[]
        {
            "Scroll View/Viewport/Content/ExpandUI/Content/UI_Main_Biotank_Animal/Scroll View/Viewport/Content/ListControl",
            "Scroll View/Viewport/Content/UI_Main_Biotank_Animal/Scroll View/Viewport/Content/ListControl",
            "Content/ExpandUI/Content/UI_Main_Biotank_Animal/Scroll View/Viewport/Content/ListControl"
        };

        Transform listControlTransform = null;
        foreach (string path in possibleListControlPaths)
        {
            listControlTransform = bioTankEditTransform.Find(path);
            if (listControlTransform != null)
            {
                break;
            }
        }

        // 如果路径查找失败，尝试递归查找
        if (listControlTransform == null)
        {
            listControlTransform = FindTransformByName(bioTankEditTransform, "ListControl");
        }

        if (listControlTransform == null)
        {
            return null;
        }

        if (listControlTransform.childCount == 0)
        {
            return null;
        }

        // 遍历所有Row查找动物
        int currentIndex = 0;
        for (int rowIdx = 0; rowIdx < listControlTransform.childCount; rowIdx++)
        {
            Transform row = listControlTransform.GetChild(rowIdx);
            if (row == null) continue;

            for (int itemIdx = 0; itemIdx < row.childCount; itemIdx++)
            {
                Transform animalItem = row.GetChild(itemIdx);
                if (animalItem == null) continue;

                if (currentIndex == index)
                {
                    // 尝试多种可能的Place按钮路径
                    string[] possiblePlacePaths = new string[]
                    {
                        "SaveAndSell/Place",
                        "Place",
                        "Buttons/Place"
                    };

                    foreach (string placePath in possiblePlacePaths)
                    {
                        Transform placeTransform = animalItem.Find(placePath);
                        if (placeTransform != null)
                        {
                            Button placeButton = placeTransform.GetComponent<Button>();
                            if (placeButton != null)
                            {
                                return placeButton;
                            }
                        }
                    }

                    // 如果上述路径都没找到，尝试递归查找Place按钮
                    Transform placeBtn = FindTransformByName(animalItem, "Place");
                    if (placeBtn != null)
                    {
                        Button button = placeBtn.GetComponent<Button>();
                        if (button != null)
                        {
                            return button;
                        }
                    }

                }
                currentIndex++;
            }
        }

        return null;
    }

    /// <summary>
    /// 递归查找指定名称的Transform
    /// </summary>
    private Transform FindTransformByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name || child.name.StartsWith(name))
            {
                return child;
            }

            Transform result = FindTransformByName(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    /// <summary>
    /// 查找编辑按钮
    /// </summary>
    /// <returns>找到的Button，未找到返回null</returns>
    private Button FindEditButton()
    {
        // 查找Canvas
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogWarning("[UI_Title] FindEditButton: 未找到Canvas对象");
            return null;
        }

        // 首先检查UI_Main_Biotank_Information是否存在且激活
        Transform biotankInfoTransform = canvas.transform.Find("Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information");
        if (biotankInfoTransform == null)
        {
            Debug.LogWarning("[UI_Title] FindEditButton: 未找到UI_Main_Biotank_Information");
            // 尝试使用GetComponentsInChildren查找（包含inactive对象）
            return FindEditButtonByName(canvas.transform);
        }
        
        if (!biotankInfoTransform.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[UI_Title] FindEditButton: UI_Main_Biotank_Information未激活，尝试使用GetComponentsInChildren");
            return FindEditButtonByName(canvas.transform);
        }

        // 尝试多种可能的路径
        string[] possiblePaths = new string[]
        {
            // 路径1: 完整路径
            "Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information/info/Vertical Layout Group/Edit/Button ",
            // 路径2: 简化路径（直接查找ButtonGroup）
            "Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information/info/Edit/Button ",
            // 路径3: 尝试不同的info子路径
            "Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/UI_Main_Biotank_Information/info/Vertical Layout Group/Edit/Button "
        };

        Transform buttonTransform = null;
        foreach (string path in possiblePaths)
        {
            buttonTransform = canvas.transform.Find(path);
            if (buttonTransform != null)
            {
                Debug.Log("[UI_Title] FindEditButton: 通过路径找到按钮 - " + path);
                break;
            }
        }

        // 如果上述路径都没找到，尝试递归查找
        if (buttonTransform == null)
        {
            // 查找UI_Main_Biotank
            Transform biotankTransform = canvas.transform.Find("Panel/Normal/UI/UI_Main/UI_Main_Biotank");
            if (biotankTransform != null)
            {
                buttonTransform = FindButtonInHierarchy(biotankTransform, "Edit");
                if (buttonTransform != null)
                {
                    Debug.Log("[UI_Title] FindEditButton: 通过递归查找找到按钮");
                }
            }
        }

        // 如果还是没找到，使用GetComponentsInChildren（包含inactive对象）
        if (buttonTransform == null)
        {
            Debug.LogWarning("[UI_Title] FindEditButton: 路径查找失败，尝试使用GetComponentsInChildren");
            return FindEditButtonByName(canvas.transform);
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("[UI_Title] FindEditButton: 找到对象但未找到Button组件");
            return null;
        }

        return button;
    }
    
    /// <summary>
    /// 通过名称查找编辑按钮（包含inactive对象）
    /// </summary>
    private Button FindEditButtonByName(Transform root)
    {
        // 使用GetComponentsInChildren查找所有Button（包含inactive）
        Button[] allButtons = root.GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            // 查找父对象名称为Edit的按钮
            if (btn.name == "Button" && btn.transform.parent != null && 
                btn.transform.parent.name == "Edit")
            {
                Debug.Log("[UI_Title] FindEditButtonByName: 通过GetComponentsInChildren找到编辑按钮");
                return btn;
            }
        }
        
        Debug.LogWarning("[UI_Title] FindEditButtonByName: 未找到编辑按钮");
        return null;
    }

    /// <summary>
    /// 在层级中递归查找指定名称的按钮（包含inactive对象）
    /// </summary>
    private Transform FindButtonInHierarchy(Transform parent, string targetName)
    {
        // 使用索引遍历以包含inactive的子对象
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == targetName)
            {
                // 找到目标对象，查找其中的Button
                Transform buttonTransform = child.Find("Button");
                if (buttonTransform != null)
                {
                    return buttonTransform;
                }

                // 如果没有Button子对象，检查自身是否有Button组件
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    return child;
                }

                // 查找第一个Button组件（包含inactive）
                btn = child.GetComponentInChildren<Button>(true);
                if (btn != null)
                {
                    return btn.transform;
                }
            }

            // 递归查找
            Transform result = FindButtonInHierarchy(child, targetName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    /// <summary>
    /// 在Biotank界面显示时检查并启动调整引导
    /// 用于玩家重新上线后的引导恢复
    /// </summary>
    private void CheckAndStartAdjustTutorialOnBiotankShow()
    {
        // 检查此引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("biotank_adjust_tutorial"))
        {
            return;
        }

        // 检查属性引导是否已完成（步骤28是属性引导的第一步）
        bool isAttributeTutorialCompleted = TutorialSaveManager.Instance.IsFlowCompleted("biotank_attribute_tutorial");

        if (!isAttributeTutorialCompleted)
        {
            return;
        }

        Debug.Log("[UI_Title] 属性引导已完成，触发调整引导（界面显示时检查）");
        CreateBiotankAdjustTutorial();
    }

    /// <summary>
    /// 检查并启动生态箱调整引导（步骤34-37）
    /// 触发条件：biotank_attribute_tutorial完成 + 当前处于主界面
    /// </summary>
    public void CheckAndStartAdjustTutorial()
    {
        // 检查此引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("biotank_adjust_tutorial"))
        {
            Debug.Log("[UI_Title] 调整引导已完成，跳过");
            return;
        }

        // 检查当前是否处于Biotank界面（主界面）
        if (currentButtonType != UI_Title_ButtonGroup_Button_Type.Biotank)
        {
            Debug.Log("[UI_Title] 当前不在Biotank界面，不触发调整引导");
            return;
        }

        Debug.Log("[UI_Title] 触发生态箱调整引导");
        CreateBiotankAdjustTutorial();
    }

    /// <summary>
    /// 创建生态箱调整引导（步骤34-37）
    /// 两段对话 + 点击Adjust按钮 + 一段对话
    /// </summary>
    private void CreateBiotankAdjustTutorial()
    {
        Debug.Log("[UI_Title] 创建生态箱调整引导流程");

        // 创建引导流程
        TutorialFlow flow = new TutorialFlow(
            "biotank_adjust_tutorial",
            "生态箱调整引导",
            "引导玩家了解生态箱的调整功能"
        );

        // 步骤34: 第一段对话 - 使用本地化文本ID 5025
        string dialog34Text = LocalizationManager.GetCurrentLanguageByID(5025);
        if (string.IsNullOrEmpty(dialog34Text))
        {
            dialog34Text = "你已经了解了生态箱的基本属性！\n\n现在让我们来看看如何调整生态箱的显示设置。";
        }
        DialogStep step34 = new DialogStep(
            "step_adjust_dialog1",
            "调整引导说明1",
            dialog34Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step34);

        // 步骤35: 第二段对话 - 使用本地化文本ID 5026
        string dialog35Text = LocalizationManager.GetCurrentLanguageByID(5026);
        if (string.IsNullOrEmpty(dialog35Text))
        {
            dialog35Text = "通过调整功能，你可以改变生态箱在桌面上的位置和观看角度。\n\n这样可以让你的生态箱看起来更加舒适美观！";
        }
        DialogStep step35 = new DialogStep(
            "step_adjust_dialog2",
            "调整引导说明2",
            dialog35Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step35);

        // 步骤36: 点击Adjust按钮（使用延迟查找）
        // 路径: Canvas/Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information/info/Vertical Layout Group/Adjust/Button
        ClickUIStep step36 = new ClickUIStep(
            "step_adjust_click_button",
            "点击调整按钮",
            () => FindAdjustButton(),
            canSkip: false
        );
        step36.Pointer = new PointerConfig { Offset = new Vector2(803, 1333), Direction = TutorialPointer.PointerDirection.UpRight };
        flow.AddStep(step36);
        Debug.Log("[UI_Title] 成功添加步骤36：点击Adjust按钮（延迟查找）");

        // 步骤37: 点击后一段对话 - 使用本地化文本ID 5027
        string dialog37Text = LocalizationManager.GetCurrentLanguageByID(5027);
        if (string.IsNullOrEmpty(dialog37Text))
        {
            dialog37Text = "太棒了！你已经学会了如何调整生态箱！\n\n现在可以自由探索和管理你的生态系统了，祝你游戏愉快！";
        }
        DialogStep step37 = new DialogStep(
            "step_adjust_dialog3",
            "调整完成说明",
            dialog37Text,
            new Vector2(550f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(step37);

        Debug.Log($"[UI_Title] 生态箱调整引导流程创建完成，共 {flow.Steps.Count} 个步骤");

        // 启动引导
        TutorialManager.Instance.StartTutorial(flow);
    }

    /// <summary>
    /// 查找Adjust（调整）按钮
    /// </summary>
    /// <returns>找到的Button，未找到返回null</returns>
    private Button FindAdjustButton()
    {
        // 查找Canvas
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogWarning("[UI_Title] FindAdjustButton: 未找到Canvas对象");
            return null;
        }

        // 首先检查UI_Main_Biotank_Information是否存在且激活
        Transform biotankInfoTransform = canvas.transform.Find("Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information");
        if (biotankInfoTransform == null || !biotankInfoTransform.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[UI_Title] FindAdjustButton: UI_Main_Biotank_Information未找到或未激活，尝试使用GetComponentsInChildren");
            return FindAdjustButtonByName(canvas.transform);
        }

        // 尝试多种可能的路径
        string[] possiblePaths = new string[]
        {
            // 路径1: 完整路径
            "Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information/info/Vertical Layout Group/Adjust/Button",
            // 路径2: 简化路径（直接查找ButtonGroup）
            "Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/Content/UI_Main_Biotank_Information/info/Adjust/Button",
            // 路径3: 尝试不同的info子路径
            "Panel/Normal/UI/UI_Main/UI_Main_Biotank/Content/UI_Main_Biotank_Information/info/Vertical Layout Group/Adjust/Button"
        };

        Transform buttonTransform = null;
        foreach (string path in possiblePaths)
        {
            buttonTransform = canvas.transform.Find(path);
            if (buttonTransform != null)
            {
                Debug.Log("[UI_Title] FindAdjustButton: 通过路径找到按钮 - " + path);
                break;
            }
        }

        // 如果上述路径都没找到，尝试递归查找
        if (buttonTransform == null)
        {
            // 查找UI_Main_Biotank
            Transform biotankTransform = canvas.transform.Find("Panel/Normal/UI/UI_Main/UI_Main_Biotank");
            if (biotankTransform != null)
            {
                buttonTransform = FindButtonInHierarchy(biotankTransform, "Adjust");
            }
        }

        // 如果还是没找到，使用GetComponentsInChildren（包含inactive对象）
        if (buttonTransform == null)
        {
            Debug.LogWarning("[UI_Title] FindAdjustButton: 路径查找失败，尝试使用GetComponentsInChildren");
            return FindAdjustButtonByName(canvas.transform);
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("[UI_Title] FindAdjustButton: 找到对象但未找到Button组件");
            return null;
        }

        Debug.Log("[UI_Title] FindAdjustButton: 成功找到调整按钮");
        return button;
    }
    
    /// <summary>
    /// 通过名称查找调整按钮（包含inactive对象）
    /// </summary>
    private Button FindAdjustButtonByName(Transform root)
    {
        // 使用GetComponentsInChildren查找所有Button（包含inactive）
        Button[] allButtons = root.GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            // 查找父对象名称为Adjust的按钮
            if (btn.name == "Button" && btn.transform.parent != null && 
                btn.transform.parent.name == "Adjust")
            {
                Debug.Log("[UI_Title] FindAdjustButtonByName: 通过GetComponentsInChildren找到调整按钮");
                return btn;
            }
        }
        
        Debug.LogWarning("[UI_Title] FindAdjustButtonByName: 未找到调整按钮");
        return null;
    }
}
