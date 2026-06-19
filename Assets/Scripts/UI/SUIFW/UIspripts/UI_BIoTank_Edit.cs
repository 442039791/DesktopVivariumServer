using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SUIFW;


public class UI_BioTank_Edit : BaseUIForms_UIBase
{
    public static UI_BioTank_Edit CurrentInstance { get; private set; }

    public Button CloseBtn;
    public Button SaveBtn;
    public Button EliminateBtn;  // 隐藏光标上方cube的按钮
    public UI_Main_WareHouse_List fishlist;
    public UI_Main_WareHouse_List plantlist;
    public UI_Main_WareHouse_List Decorationlist;
    public UI_Main_WareHouse_List cubelist;
    public UI_BIoTank_Edit_Handle handle;
    public UI_BluePrint bluePrint;
    private int objid;
    private bool NextFrameRefresh = false;
    private bool _initcomplete = false;
    private bool _isHideAboveOn = false;  // 隐藏上方cube功能状态

    // 连续移动支持（参考MyButtonTriggerEvent和摄像机移动实现）
    private float _holdDuration = 0f;              // 按住持续时间
    private float _lastTriggerTime = 0f;           // 距离上次触发的间隔时间
    private float _activationDelay = 0.2f;         // 长按0.2秒后激活连续移动（加快响应）
    private float _repeatInterval = 0.0333f;       // 连续移动时每0.0333秒触发一次（约30fps，每秒30格）
    private bool _continuousMoveMode = false;      // 是否处于连续移动模式

    /// <summary>
    /// 当前正在建造的物品信息（用于金币检查和放置确认）
    /// </summary>
    public class CurrentBuildingItem
    {
        public string itemID;        // 物品ID（WarehouseBaseID或Type，字符串类型）
        public int blockType;        // 方块类型：0=Cube, 1=Decoration, 2=Plant
        public int price;            // 价格
        public int objID;            // 对象ID（用于重新发送CreatePlantCubeCommand）
        public float growthProgress; // 成长进度（仅Plant类型有效，0-100）
        public WarehouseBaseData baseData;  // 仓库数据引用
    }

    /// <summary>
    /// 当前正在建造的物品（用于放置和连续放置）
    /// 放置成功后由PlacementResultCommand决定是否清空
    /// </summary>
    public CurrentBuildingItem currentBuildingItem = null;

    /// <summary>
    /// 当前建造控制模式（Normal/Build/Delete/Move）
    /// </summary>
    private BuildControlMode _currentMode = BuildControlMode.Normal;

    public BuildControlMode currentMode
    {
        get => _currentMode;
        set
        {
            if (_currentMode != value)
            {
                var stackTrace = new System.Diagnostics.StackTrace(1, true);
                _currentMode = value;
            }
        }
    }

    public override void RefreshUI()
    {
        fishlist.Init();
        plantlist.Init();
        Decorationlist.Init();
        cubelist.Init();
        Show();
    }
    public void Close()
    {
        // 通知窗口管理器，由它决定是否恢复窗口尺寸
        BiotankWindowManager.OnWindowClose();

        WebSocketServerManager.GetActiveClient()?.SendSetCanUseButtonCommand(false);
        if (WebSocketServerManager.GetActiveClient() != null && this.objid != -1)
        {
            WebSocketServerManager.GetActiveClient().SendBluePrintBioTankCommand(BluePrintBioTankCommandType.ExitMode, Vector3.zero, this.objid);

        }
        handle.ExitBuildMode();
        // 重置隐藏上方功能
        ResetHideAboveState();
        // 清除建造物品信息
        currentBuildingItem = null;
        currentMode = BuildControlMode.Normal;
        CurrentInstance = null;
        CloseOrReturnUIForms();

        // 检查是否需要触发属性引导（在27步引导完成后）
        NotifyEditClosed();
    }

    /// <summary>
    /// 通知编辑界面已关闭，用于触发后续引导
    /// </summary>
    private void NotifyEditClosed()
    {
        Debug.Log("[UI_BioTank_Edit] NotifyEditClosed: 编辑界面关闭，尝试触发属性引导");

        // 使用FindObjectOfType更可靠地查找UI_Title组件
        UI_Title uiTitle = FindObjectOfType<UI_Title>();
        if (uiTitle != null)
        {
            Debug.Log("[UI_BioTank_Edit] NotifyEditClosed: 找到UI_Title组件，调用CheckAndStartAttributeTutorialAfterEditClose");
            uiTitle.CheckAndStartAttributeTutorialAfterEditClose();
        }
        else
        {
            Debug.LogWarning("[UI_BioTank_Edit] NotifyEditClosed: 未找到UI_Title组件");
        }
    }
    public bool Init(int objid=-1)
    {
        CurrentInstance = this;
        WebSocketServerManager.GetActiveClient()?.SendSetCanUseButtonCommand(true);

        // 窗口扩展后，将界面放到右侧扩展区域
        float offsetX = 445f;
        transform.localPosition = new Vector3(offsetX, 0f, 0f);
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(offsetX, 0f);
        }

        // 注册窗口打开，由BiotankWindowManager管理窗口尺寸
        BiotankWindowManager.OnWindowOpen();

        // 关闭模组管理界面（互斥逻辑）
        UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_ModManager);

        // 关闭生态箱列表界面（互斥逻辑）
        UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_Biotank_List);

        // 关闭新建生态箱界面（互斥逻辑）
        UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_Biotank_AddBiotank);

        // 从图鉴同步已解锁状态到Gather（修复历史数据不一致问题）
        if (IlluGuideMgr.Instance != null && Gather.Instance != null)
        {
            foreach (var entry in IlluGuideMgr.Instance.cubeEntries.Values)
            {
                if (entry.isUnlocked && Gather.Instance.CubeUnlockDic.ContainsKey(entry.id))
                {
                    Gather.Instance.CubeUnlockDic[entry.id] = true;
                }
            }

            foreach (var entry in IlluGuideMgr.Instance.decorationEntries.Values)
            {
                if (entry.isUnlocked && Gather.Instance.DecorationUnlockDic.ContainsKey(entry.id))
                {
                    Gather.Instance.DecorationUnlockDic[entry.id] = true;
                }
            }
        }

        // 从Gather同步已解锁的Cube和Decoration到仓库
        if (Gather.Instance == null)
        {
            Debug.LogError("[UI_BioTank_Edit] Gather未初始化");
            return true;
        }

        var warehouseService = ServiceLocator.Get<IWarehouseService>();
        if (warehouseService == null)
        {
            Debug.LogError("[UI_BioTank_Edit] WarehouseService未初始化");
            return true;
        }

        // 同步已解锁的Cube
        foreach (var kvp in Gather.Instance.CubeUnlockDic)
        {
            if (kvp.Value)
            {
                var existingCubes = warehouseService.GetWarehouseBaseDatas(ItemType.Cube);
                bool exists = existingCubes.Any(item =>
                {
                    var data = item as WarehouseCubeStateData;
                    return data != null && data.stateData.Type == kvp.Key;
                });

                if (!exists)
                {
                    warehouseService.SaveCube(new CubeStateData { Type = kvp.Key }, false);
                }
            }
        }

        // 同步已解锁的Decoration
        foreach (var kvp in Gather.Instance.DecorationUnlockDic)
        {
            if (kvp.Value)
            {
                var existingDecos = warehouseService.GetWarehouseBaseDatas(ItemType.Decoration);
                bool exists = existingDecos.Any(item =>
                {
                    var data = item as WarehouseDecorationStateData;
                    return data != null && data.stateData.Type == kvp.Key;
                });

                if (!exists)
                {
                    warehouseService.SaveDecoration(new DecorationStateData { Type = kvp.Key }, false);
                }
            }
        }

        // 设置每个列表的类型
        fishlist.type = ItemType.AdultFish;
        plantlist.type = ItemType.Plant;
        Decorationlist.type = ItemType.Decoration;
        cubelist.type = ItemType.Cube;

        gameObject.SetActive(true);
        this.objid = objid;

        // 扫描并注册所有本地化文本（自动处理所有"Id:xxx"格式的文本）
        LocalizationManager.ScanAndRegisterTextsStatic(transform);

        CloseBtn.AddDeskTopListener(() =>
        {
            // 通知窗口管理器，由它决定是否恢复窗口尺寸
            BiotankWindowManager.OnWindowClose();

            WebSocketServerManager.GetActiveClient()?.SendSetCanUseButtonCommand(false);
            if (WebSocketServerManager.GetActiveClient()!= null&&this.objid!= -1)
            {
                WebSocketServerManager.GetActiveClient().SendBluePrintBioTankCommand(BluePrintBioTankCommandType.ExitMode, Vector3.zero,this.objid);

            }
            handle.ExitBuildMode();
            // 清除建造物品信息
            currentBuildingItem = null;
            currentMode = BuildControlMode.Normal;
            CurrentInstance = null;
            CloseOrReturnUIForms();

            // 检查是否需要触发属性引导（在27步引导完成后）
            NotifyEditClosed();
        });
        if (objid != -1)
        {
            SaveBtn.gameObject.SetActive(true);
            SaveBtn.AddDeskTopListener(() =>
            {
                WebSocketServerManager.GetActiveClient()?.SendBluePrintBioTankCommand(BluePrintBioTankCommandType.Save, Vector3.zero,this.objid);
            });
        }
        //else
        //{
        //    SaveBtn.gameObject.SetActive(false);
        //}

        // 隐藏上方cube按钮
        if (EliminateBtn != null)
        {
            EliminateBtn.AddDeskTopListener(() =>
            {
                ToggleHideAbove();
            });
        }

        handle.Init();
        NextFrameRefresh = true;
        base.InitButtonClickEvent((obj) =>
        {

            NextFrameRefresh = true;

        }, null);
        bluePrint.Init();
        //if(initcomplete)
        //    return true;

        //RefreshUI();
        base.Init();
        if(!_initcomplete)
        {
            _initcomplete = true;
            event_manager.instance.add_event_listener(SysDefine.UI_WareHouse_List, on_event_handler);
        }

        // 检查并触发植物放置引导
        CheckAndStartPlantPlacementTutorial();

        return true;
    }

    /// <summary>
    /// 检查并启动植物放置引导
    /// 触发条件：
    /// 1. UI_BioTank_Edit界面打开
    /// 2. 仓库中有植物
    /// 3. biotank_tutorial（步骤20）已完成
    /// </summary>
    private void CheckAndStartPlantPlacementTutorial()
    {
        // 检查引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("biotank_edit_plant_tutorial"))
        {
            return;
        }

        // 检查前置引导（biotank_tutorial，步骤15-20）是否已完成
        if (!TutorialSaveManager.Instance.IsFlowCompleted("biotank_tutorial"))
        {
            return;
        }

        // 检查仓库中是否有植物
        var warehouseService = ServiceLocator.Get<IWarehouseService>();
        if (warehouseService == null)
        {
            return;
        }

        var plantList = warehouseService.GetWarehouseBaseDatas(ItemType.Plant);
        if (plantList == null || plantList.Count == 0)
        {
            return;
        }

        Debug.Log("[UI_BioTank_Edit] 触发植物放置引导");
        CreatePlantPlacementTutorial();
    }

    /// <summary>
    /// 供外部调用启动植物放置引导（由UI_Title在biotank_tutorial完成时调用）
    /// 此方法不再检查前置条件，因为调用方已经检查过
    /// </summary>
    public void StartPlantPlacementTutorialFromExternal()
    {
        // 检查引导是否已完成
        if (TutorialSaveManager.Instance.IsFlowCompleted("biotank_edit_plant_tutorial"))
        {
            Debug.Log("[UI_BioTank_Edit] 植物放置引导已完成，跳过");
            return;
        }

        Debug.Log("[UI_BioTank_Edit] 外部触发植物放置引导");
        CreatePlantPlacementTutorial();
    }

    /// <summary>
    /// 创建植物放置引导（三段对话 + 点击放置按钮）
    /// </summary>
    private void CreatePlantPlacementTutorial()
    {
        Debug.Log("[UI_BioTank_Edit] 创建植物放置引导流程");

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
            new Vector2(420f, 160f),
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
            new Vector2(420f, 160f),
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
            new Vector2(420f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(dialog3Step);

        // 步骤4: 点击植物的Place按钮（使用延迟查找，在步骤执行时才查找按钮）
        ClickUIStep placeClickStep = new ClickUIStep(
            "step_plant_placement_click_place",
            "点击植物放置按钮",
            () => FindPlantPlaceButton(0),  // 延迟查找，在步骤执行时才查找
            canSkip: false
        );
        placeClickStep.Pointer = new PointerConfig { Offset = new Vector2(1520, 700), Direction = TutorialPointer.PointerDirection.UpRight };
        flow.AddStep(placeClickStep);
        Debug.Log("[UI_BioTank_Edit] 添加植物Place按钮点击步骤（延迟查找）");

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
            new Vector2(420f, 160f),
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
            new Vector2(420f, 160f),
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
            new Vector2(420f, 160f),
            autoCompleteDelay: 0f,
            canSkip: false
        );
        flow.AddStep(editDialog3Step);

        Debug.Log($"[UI_BioTank_Edit] 植物放置引导流程创建完成，共 {flow.Steps.Count} 个步骤");

        // 启动引导
        TutorialManager.Instance.StartTutorial(flow);
    }

    /// <summary>
    /// 查找植物放置按钮（延迟查找，用于引导步骤执行时调用）
    /// ListController的结构：Row直接作为ListController的子项，每个Row下有多个植物项
    /// </summary>
    /// <param name="index">植物在列表中的索引</param>
    /// <returns>找到的Button，未找到返回null</returns>
    private Button FindPlantPlaceButton(int index)
    {
        if (plantlist == null || plantlist.listController == null)
        {
            Debug.LogWarning("[UI_BioTank_Edit] FindPlantPlaceButton: plantlist或listController为空");
            return null;
        }

        // ListController直接管理Row，Row作为其子项
        Transform listControllerTransform = plantlist.listController.transform;

        if (listControllerTransform.childCount == 0)
        {
            Debug.LogWarning("[UI_BioTank_Edit] FindPlantPlaceButton: ListController没有子项(Row)");
            return null;
        }

        // 遍历所有Row查找植物
        int currentIndex = 0;
        for (int rowIdx = 0; rowIdx < listControllerTransform.childCount; rowIdx++)
        {
            Transform row = listControllerTransform.GetChild(rowIdx);
            if (row == null || !row.gameObject.activeSelf) continue;

            // 遍历Row中的每个植物项
            for (int itemIdx = 0; itemIdx < row.childCount; itemIdx++)
            {
                Transform plantItem = row.GetChild(itemIdx);
                if (plantItem == null || !plantItem.gameObject.activeSelf) continue;

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
                        Transform placeTransform = plantItem.Find(placePath);
                        if (placeTransform != null)
                        {
                            Button placeButton = placeTransform.GetComponent<Button>();
                            if (placeButton != null)
                            {
                                Debug.Log($"[UI_BioTank_Edit] FindPlantPlaceButton: 找到植物放置按钮，路径={placePath}");
                                return placeButton;
                            }
                        }
                    }

                    // 如果上述路径都没找到，尝试递归查找Place按钮
                    Button foundButton = FindButtonByNameRecursive(plantItem, "Place");
                    if (foundButton != null)
                    {
                        Debug.Log("[UI_BioTank_Edit] FindPlantPlaceButton: 通过递归查找找到植物放置按钮");
                        return foundButton;
                    }

                    Debug.LogWarning($"[UI_BioTank_Edit] FindPlantPlaceButton: 在植物项中未找到Place按钮");
                    return null;
                }
                currentIndex++;
            }
        }

        Debug.LogWarning($"[UI_BioTank_Edit] FindPlantPlaceButton: 未找到索引为{index}的植物项，当前共有{currentIndex}个植物项");
        return null;
    }

    /// <summary>
    /// 递归查找指定名称的Button组件
    /// </summary>
    private Button FindButtonByNameRecursive(Transform parent, string buttonName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == buttonName)
            {
                Button button = child.GetComponent<Button>();
                if (button != null)
                {
                    return button;
                }
            }

            // 递归查找
            Button found = FindButtonByNameRecursive(child, buttonName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    public void TestRefreshNext()
    {
        //fishlist.Init();
        //plantlist.Init();
        //Decorationlist.Init();
        //cubelist.Init();
        NextFrameRefresh = true;
    }
    public void on_event_handler(string name, object udata)
    {
        if (gameObject.activeSelf == false)
            return;
        UI_WareHouse_List_EventData data = (UI_WareHouse_List_EventData)udata;

        NextFrameRefresh = true;
    }
    public void TestRefresh()
    {
        RefreshUI();
    }
    private int frame = 0;
    private void Update()
    {
        // 监听WASD键盘输入，发送移动命令给客户端
        HandleKeyboardInput();

        if (NextFrameRefresh)
        {
            ++frame;
            if (frame > 1)
            {
                frame = 0;
                NextFrameRefresh = false;
                RefreshUI();

            }
            else if (frame == 1)
            {
                Show();
            }

        }

    }

    /// <summary>
    /// 处理键盘输入并发送移动命令给客户端（支持长按持续移动）
    /// </summary>
    private void HandleKeyboardInput()
    {
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null)
        {
            return;
        }

        // 检测是否有方向键输入（使用GetKey检测长按）
        bool hasInput = GlobalInputManager.GetKey(KeyCode.W) || GlobalInputManager.GetKey(KeyCode.S) || GlobalInputManager.GetKey(KeyCode.A) || GlobalInputManager.GetKey(KeyCode.D) ||
                        GlobalInputManager.GetKey(KeyCode.UpArrow) || GlobalInputManager.GetKey(KeyCode.DownArrow) || GlobalInputManager.GetKey(KeyCode.LeftArrow) || GlobalInputManager.GetKey(KeyCode.RightArrow) ||
                        GlobalInputManager.GetKey(KeyCode.X) || GlobalInputManager.GetKey(KeyCode.Z);

        // 空格键 - 确认放置/删除操作（必须在方向键检查之前处理，否则会被return阻止）
        if (GlobalInputManager.GetKeyDown(KeyCode.Space))
        {
            HandleSpaceKeyConfirm(client);
        }

        // 参考MyButtonTriggerEvent的实现
        if (hasInput)
        {
            _holdDuration += Time.deltaTime;

            // 长按超过0.5秒后激活连续移动模式
            if (_holdDuration >= _activationDelay)
            {
                _continuousMoveMode = true;
            }
        }
        else
        {
            // 松开按键时重置所有状态
            _holdDuration = 0f;
            _lastTriggerTime = 0f;
            _continuousMoveMode = false;
            return;
        }

        float deltaX = 0f, deltaY = 0f, deltaZ = 0f;
        bool shouldSendCommand = false;

        // 单次按下模式
        if (!_continuousMoveMode)
        {
            // 只在第一次按下时触发（使用GetKeyDown）
            if (GlobalInputManager.GetKeyDown(KeyCode.W) || GlobalInputManager.GetKeyDown(KeyCode.UpArrow))
            {
                deltaZ = 1f;
                shouldSendCommand = true;
            }
            else if (GlobalInputManager.GetKeyDown(KeyCode.S) || GlobalInputManager.GetKeyDown(KeyCode.DownArrow))
            {
                deltaZ = -1f;
                shouldSendCommand = true;
            }

            if (GlobalInputManager.GetKeyDown(KeyCode.A) || GlobalInputManager.GetKeyDown(KeyCode.LeftArrow))
            {
                deltaX = -1f;
                shouldSendCommand = true;
            }
            else if (GlobalInputManager.GetKeyDown(KeyCode.D) || GlobalInputManager.GetKeyDown(KeyCode.RightArrow))
            {
                deltaX = 1f;
                shouldSendCommand = true;
            }

            if (GlobalInputManager.GetKeyDown(KeyCode.X))
            {
                deltaY = 1f;
                shouldSendCommand = true;
            }
            else if (GlobalInputManager.GetKeyDown(KeyCode.Z))
            {
                deltaY = -1f;
                shouldSendCommand = true;
            }

            // 单次移动后重置间隔计时器，为连续移动做准备
            if (shouldSendCommand)
            {
                _lastTriggerTime = 0f;
            }
        }
        else
        {
            // 连续移动模式：参考MyButtonTriggerEvent的实现
            // 先检查是否达到移动间隔
            if (_lastTriggerTime >= _repeatInterval)
            {
                // 检测当前按住的键（使用GetKey）
                if (GlobalInputManager.GetKey(KeyCode.W) || GlobalInputManager.GetKey(KeyCode.UpArrow))
                {
                    deltaZ = 1f;
                    shouldSendCommand = true;
                }
                else if (GlobalInputManager.GetKey(KeyCode.S) || GlobalInputManager.GetKey(KeyCode.DownArrow))
                {
                    deltaZ = -1f;
                    shouldSendCommand = true;
                }

                if (GlobalInputManager.GetKey(KeyCode.A) || GlobalInputManager.GetKey(KeyCode.LeftArrow))
                {
                    deltaX = -1f;
                    shouldSendCommand = true;
                }
                else if (GlobalInputManager.GetKey(KeyCode.D) || GlobalInputManager.GetKey(KeyCode.RightArrow))
                {
                    deltaX = 1f;
                    shouldSendCommand = true;
                }

                if (GlobalInputManager.GetKey(KeyCode.X))
                {
                    deltaY = 1f;
                    shouldSendCommand = true;
                }
                else if (GlobalInputManager.GetKey(KeyCode.Z))
                {
                    deltaY = -1f;
                    shouldSendCommand = true;
                }

                // 执行移动后重置间隔计时器
                if (shouldSendCommand)
                {
                    _lastTriggerTime = 0f;
                }
            }

            // 关键：持续累加间隔时间（放在if外面，参考MyButtonTriggerEvent第50行）
            _lastTriggerTime += Time.deltaTime;
        }

        // 发送移动命令
        if (shouldSendCommand)
        {
            client.SendCubeMoveCommand(deltaX, deltaY, deltaZ);
        }
    }

    /// <summary>
    /// 处理空格键确认操作
    /// 【设计原则】
    /// - Build模式: 需要先选择物品（currentBuildingItem不为空）才能放置
    /// - Delete模式: 不需要选择物品，直接删除光标位置的物体
    /// - Move模式: 不响应空格键
    /// - Normal模式: 不响应空格键
    /// </summary>
    private void HandleSpaceKeyConfirm(ControlBehavior client)
    {
        // Delete模式：不检查currentBuildingItem，直接发送确认命令删除
        if (currentMode == BuildControlMode.Delete)
        {
            client.SendCubeConfirmCommand();
            return;
        }

        // Move模式和Normal模式：不响应空格键
        if (currentMode == BuildControlMode.Move || currentMode == BuildControlMode.Normal)
        {
            return;
        }

        // Build模式：需要先选择物品
        if (currentBuildingItem == null)
        {
            return;
        }

        // 检查金币是否足够
        if (!PlayerManager.Instance.CheckMoneyEnough(currentBuildingItem.price))
        {
            ToastManager.Show(134);  // 134 = "生态币不足"
            Debug.LogWarning($"[HandleSpaceKeyConfirm] 金币不足，需要 {currentBuildingItem.price}");
            return;
        }


        // 发送确认命令（只发送确认信号，物品信息已通过CreatePlantCubeCommand传递）
        client.SendCubeConfirmCommand();
    }

    /// <summary>
    /// 清除当前建造物品信息（放置成功后调用）
    /// 注意：此方法不会修改currentMode，模式切换应通过SetBuildMode或其他专门的方法
    /// </summary>
    public void ClearCurrentBuildingItem()
    {
        currentBuildingItem = null;
    }

    /// <summary>
    /// 切换隐藏光标上方cube功能
    /// </summary>
    private void ToggleHideAbove()
    {
        _isHideAboveOn = !_isHideAboveOn;

        // 更新按钮颜色显示状态
        if (EliminateBtn != null)
        {
            EliminateBtn.image.color = _isHideAboveOn ? Color.black : Color.white;
        }

        // 发送命令到客户端
        WebSocketServerManager.GetActiveClient()?.SendHideAboveCursorCommand(_isHideAboveOn);

        Debug.Log($"[UI_BioTank_Edit] 隐藏上方cube: {(_isHideAboveOn ? "开启" : "关闭")}");
    }

    /// <summary>
    /// 重置隐藏上方功能状态
    /// </summary>
    private void ResetHideAboveState()
    {
        if (_isHideAboveOn)
        {
            _isHideAboveOn = false;
            if (EliminateBtn != null)
            {
                EliminateBtn.image.color = Color.white;
            }
            // 发送关闭命令
            WebSocketServerManager.GetActiveClient()?.SendHideAboveCursorCommand(false);
        }
    }

    private void OnDestroy()
    {
        // 清理静态引用，防止持有已销毁对象
        if (CurrentInstance == this)
        {
            CurrentInstance = null;
        }
    }

}
