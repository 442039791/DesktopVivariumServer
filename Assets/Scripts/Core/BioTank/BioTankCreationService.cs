using UnityEngine;

/// <summary>
/// 生态箱创建失败原因
/// </summary>
public enum BioTankCreationFailReason
{
    /// <summary>
    /// 无错误
    /// </summary>
    None = 0,

    /// <summary>
    /// 金币不足
    /// </summary>
    InsufficientFunds = 1,

    /// <summary>
    /// 生态箱数量已达上限
    /// </summary>
    MaxCountReached = 2,

    /// <summary>
    /// 无效的生态箱类型
    /// </summary>
    InvalidTankType = 3,

    /// <summary>
    /// 创建失败（内部错误）
    /// </summary>
    CreationFailed = 4
}

/// <summary>
/// 生态箱创建费用计算结果
/// </summary>
public class BioTankCreationCostResult
{
    /// <summary>
    /// 生态箱基础费用（读取TankData表）
    /// </summary>
    public int TankBaseCost { get; set; }

    /// <summary>
    /// 生态箱数量费用（读取GridData表，第N个生态箱的费用）
    /// </summary>
    public int TankCountCost { get; set; }

    /// <summary>
    /// 布局内容费用（cube + 装饰物 + 植物）
    /// </summary>
    public LayoutCostResult LayoutCost { get; set; }

    /// <summary>
    /// 总费用
    /// </summary>
    public int TotalCost => TankBaseCost + TankCountCost + (LayoutCost?.TotalCost ?? 0);

    /// <summary>
    /// 生态箱尺寸信息（用于显示）
    /// </summary>
    public string TankSizeInfo { get; set; }

    /// <summary>
    /// 使用的TankData ID
    /// </summary>
    public string TankDataId { get; set; }
}

/// <summary>
/// 生态箱创建结果
/// </summary>
public class BioTankCreationResult
{
    /// <summary>
    /// 是否创建成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    public BioTankCreationFailReason FailReason { get; set; } = BioTankCreationFailReason.None;

    /// <summary>
    /// 错误消息（如果失败）
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 创建的生态箱管理器
    /// </summary>
    public BioTankManager BioTank { get; set; }

    /// <summary>
    /// 实际扣除的费用
    /// </summary>
    public int CostDeducted { get; set; }
}

/// <summary>
/// 生态箱创建服务 - 处理新建生态箱的完整流程
/// 包括：尺寸选择、布局选择、费用计算、扣费、创建
/// </summary>
public class BioTankCreationService
{
    private static BioTankCreationService _instance;
    public static BioTankCreationService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BioTankCreationService();
            }
            return _instance;
        }
    }

    private BioTankCreationService() { }

    #region 费用计算

    /// <summary>
    /// 计算创建生态箱的总费用
    /// </summary>
    /// <param name="tankDataId">TankData表中的ID（如 "7001"）</param>
    /// <param name="layoutId">布局ID（可为null表示空布局）</param>
    /// <returns>费用计算结果</returns>
    public BioTankCreationCostResult CalculateCreationCost(string tankDataId, string layoutId = null)
    {
        var result = new BioTankCreationCostResult
        {
            TankDataId = tankDataId,
            LayoutCost = new LayoutCostResult()
        };

        // 1. 获取生态箱基础费用（从TankData表）
        var tankData = GameConfigDataBase.GetConfigData<TankData>(int.Parse(tankDataId), SysDefine.TankConfigData);
        if (tankData != null)
        {
            result.TankBaseCost = tankData.Price;
            result.TankSizeInfo = $"{tankData.X}x{tankData.Y}x{tankData.Z}";
        }
        else
        {
            Debug.LogWarning($"[BioTankCreationService] 未找到TankData: {tankDataId}");
        }

        // 2. 获取生态箱数量费用（从GridData表）
        // 当前生态箱数量 + 1 = 新建的是第几个
        int currentTankCount = BioTankRegistry.Instance.Count;
        int newTankIndex = currentTankCount + 1;
        var gridData = GameConfigDataBase.GetConfigData<GridData>(newTankIndex, null);
        if (gridData != null)
        {
            result.TankCountCost = gridData.Price;
        }
        else
        {
            Debug.LogWarning($"[BioTankCreationService] 未找到GridData: {newTankIndex}，使用0费用");
            result.TankCountCost = 0;
        }

        // 3. 获取布局内容费用（如果有选择布局）
        if (!string.IsNullOrEmpty(layoutId))
        {
            result.LayoutCost = TankInitialLayoutManager.Instance.CalculateLayoutCostById(layoutId);
        }

        return result;
    }

    /// <summary>
    /// 根据尺寸计算创建费用
    /// </summary>
    /// <param name="sizeX">尺寸X</param>
    /// <param name="sizeY">尺寸Y</param>
    /// <param name="sizeZ">尺寸Z</param>
    /// <param name="layoutId">布局ID（可为null）</param>
    /// <returns>费用计算结果</returns>
    public BioTankCreationCostResult CalculateCreationCostBySize(int sizeX, int sizeY, int sizeZ, string layoutId = null)
    {
        // 查找匹配尺寸的TankData
        string tankDataId = FindTankDataIdBySize(sizeX, sizeY, sizeZ);
        if (string.IsNullOrEmpty(tankDataId))
        {
            Debug.LogWarning($"[BioTankCreationService] 未找到匹配尺寸的TankData: {sizeX}x{sizeY}x{sizeZ}");
            return new BioTankCreationCostResult
            {
                TankSizeInfo = $"{sizeX}x{sizeY}x{sizeZ}",
                LayoutCost = new LayoutCostResult()
            };
        }

        return CalculateCreationCost(tankDataId, layoutId);
    }

    /// <summary>
    /// 根据尺寸查找TankData ID
    /// </summary>
    private string FindTankDataIdBySize(int sizeX, int sizeY, int sizeZ)
    {
        // 遍历所有TankData查找匹配的尺寸
        var allTankData = GameConfigDataBase.GetConfigDatas<TankData>(SysDefine.TankConfigData);
        if (allTankData != null)
        {
            foreach (var tankData in allTankData)
            {
                if (tankData.X == sizeX && tankData.Y == sizeY && tankData.Z == sizeZ)
                {
                    return tankData.ID;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 检查玩家是否有足够的钱
    /// </summary>
    public bool CanAfford(int cost)
    {
        return PlayerManager.Instance.CheckMoneyEnough(cost);
    }

    /// <summary>
    /// 获取生态箱最大数量（等于TankData表中的条目数量）
    /// </summary>
    public int GetMaxBioTankCount()
    {
        var allTankData = GameConfigDataBase.GetConfigDatas<TankData>(SysDefine.TankConfigData);
        return allTankData?.Count ?? 0;
    }

    /// <summary>
    /// 检查是否可以创建新的生态箱（数量未达上限）
    /// </summary>
    public bool CanCreateMoreBioTanks()
    {
        return BioTankRegistry.Instance.Count < GetMaxBioTankCount();
    }

    /// <summary>
    /// 获取当前生态箱数量
    /// </summary>
    public int GetCurrentBioTankCount()
    {
        return BioTankRegistry.Instance.Count;
    }

    #endregion

    #region 创建生态箱

    /// <summary>
    /// 创建新生态箱（支持预览模式）
    /// 如果当前处于预览模式，则将预览客户端转为正常客户端
    /// </summary>
    /// <param name="tankDataId">TankData表中的ID</param>
    /// <param name="layoutId">布局ID（可为null或"empty"表示空布局）</param>
    /// <param name="usePreviewClient">是否使用预览客户端</param>
    /// <param name="showToast">是否显示提示语，默认为true</param>
    /// <returns>创建结果</returns>
    public BioTankCreationResult CreateBioTankWithPreview(string tankDataId, string layoutId = null, bool usePreviewClient = false, bool showToast = true)
    {
        var result = new BioTankCreationResult();

        // 1. 检查生态箱数量是否已达上限
        int maxCount = GetMaxBioTankCount();
        int currentCount = GetCurrentBioTankCount();
        if (currentCount >= maxCount)
        {
            result.Success = false;
            result.FailReason = BioTankCreationFailReason.MaxCountReached;
            result.ErrorMessage = $"生态箱数量已达上限（{currentCount}/{maxCount}）";

            if (showToast)
            {
                ToastManager.Show($"生态箱数量已达上限");
            }

            Debug.LogWarning($"[BioTankCreationService] {result.ErrorMessage}");
            return result;
        }

        // 2. 计算费用
        var costResult = CalculateCreationCost(tankDataId, layoutId);
        int totalCost = costResult.TotalCost;

        // 3. 检查玩家是否有足够的钱
        if (!CanAfford(totalCost))
        {
            result.Success = false;
            result.FailReason = BioTankCreationFailReason.InsufficientFunds;
            result.ErrorMessage = $"金币不足，需要 {totalCost}";

            if (showToast)
            {
                ToastManager.Show(134);
            }

            Debug.LogWarning($"[BioTankCreationService] {result.ErrorMessage}");
            return result;
        }

        // 4. 获取TankData
        var tankData = GameConfigDataBase.GetConfigData<TankData>(int.Parse(tankDataId), SysDefine.TankConfigData);
        if (tankData == null)
        {
            result.Success = false;
            result.FailReason = BioTankCreationFailReason.InvalidTankType;
            result.ErrorMessage = $"无效的生态箱类型: {tankDataId}";

            if (showToast)
            {
                ToastManager.Show("无效的生态箱类型");
            }

            Debug.LogError($"[BioTankCreationService] {result.ErrorMessage}");
            return result;
        }

        // 5. 扣除费用
        PlayerManager.Instance.OnMoneyChanged(-totalCost);
        result.CostDeducted = totalCost;

        // 6. 计算资源初始值
        float volume = tankData.X * tankData.Y * tankData.Z / 1000f;
        float maxResource = volume * 10;
        float halfMax = maxResource / 2;

        // 7. 创建生态箱状态数据
        BioTankStateData stateData = new BioTankStateData
        {
            area_x = tankData.X,
            area_y = tankData.Y,
            area_z = tankData.Z,
            Pos = Vector3.zero,
            ID = IDFactory.GetUniqueID(),
            Name = GetBiotankDefaultName(tankData),
            Type = tankDataId,
            Volume = volume,
            O2 = halfMax,
            CO2 = 0f,
            Waste = 0f,
            Nutrient = halfMax,
            AsO2Auto = false,
            AsCO2Auto = false,
            AsWasteAuto = false,
            AsNutrientAuto = false,
            AsWhiteAuto = false,
            AsMeatAuto = false
        };

        // 7.1 从布局中获取摄像机参数
        bool isEmptyLayout = string.IsNullOrEmpty(layoutId);
        TankInitialLayoutData selectedLayout = null;

        if (!isEmptyLayout)
        {
            selectedLayout = TankInitialLayoutManager.Instance.GetLayoutById(layoutId);
        }

        if (selectedLayout?.defaultCameraPos != null)
        {
            stateData.DefaultCameraPos = selectedLayout.defaultCameraPos.ToCameraPosOutput();
            Debug.Log($"[BioTankCreationService] 使用布局 '{layoutId}' 的摄像机参数");
        }
        else
        {
            var firstLayout = TankInitialLayoutManager.Instance.GetFirstLayout(tankData.X, tankData.Y, tankData.Z);
            if (firstLayout?.defaultCameraPos != null)
            {
                stateData.DefaultCameraPos = firstLayout.defaultCameraPos.ToCameraPosOutput();
                Debug.Log($"[BioTankCreationService] 使用第一个布局的摄像机参数");
            }
        }

        // 8. 创建生态箱
        BioTankManager bioTank;

        if (isEmptyLayout)
        {
            bioTank = EntityFactory.Instance.CreateBioTank(stateData);
        }
        else
        {
            bioTank = CreateBioTankWithLayout(stateData, layoutId);
        }

        if (bioTank == null)
        {
            result.Success = false;
            result.FailReason = BioTankCreationFailReason.CreationFailed;
            result.ErrorMessage = "创建生态箱失败";

            PlayerManager.Instance.OnMoneyChanged(totalCost);
            result.CostDeducted = 0;

            if (showToast)
            {
                ToastManager.Show("创建生态箱失败");
            }

            Debug.LogError($"[BioTankCreationService] {result.ErrorMessage}");
            return result;
        }

        // 9. 将新建的生态箱设置为当前活动生态箱
        PlayerManager.Instance.SetActionBioTankManager(bioTank);

        // 10. 保存存档
        Debug.Log($"[BioTankCreationService] 保存新生态箱存档 ID={stateData.ID}");
        GameManager.Instance.Save(false);

        // 11. 启动客户端
        // 如果使用预览客户端，则将预览客户端转为正常客户端
        // 否则使用常规方式启动
        if (usePreviewClient && PreloadClientPoolManager.Instance != null && PreloadClientPoolManager.Instance.IsInPreviewMode)
        {
            ActivatePreviewClient(stateData.ID);
        }
        else
        {
            CheckAndLaunchNewClient();
        }

        result.Success = true;
        result.FailReason = BioTankCreationFailReason.None;
        result.BioTank = bioTank;

        Debug.Log($"[BioTankCreationService] 成功创建生态箱 ID={stateData.ID}, 尺寸={tankData.X}x{tankData.Y}x{tankData.Z}, 布局={layoutId ?? "空"}, 费用={totalCost}, 使用预览={usePreviewClient}");

        return result;
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 使用指定布局创建生态箱
    /// </summary>
    private BioTankManager CreateBioTankWithLayout(BioTankStateData stateData, string layoutId)
    {
        var bioTank = new BioTankManager();
        bioTank.Init(stateData);

        // 注册到注册表
        BioTankRegistry.Instance.RegisterBioTank(bioTank);

        // 应用指定布局
        bool layoutApplied = TankInitialLayoutManager.Instance.ApplyLayoutById(stateData.ID, layoutId);
        if (!layoutApplied)
        {
            Debug.LogWarning($"[BioTankCreationService] 应用布局 '{layoutId}' 失败");
        }

        event_manager.instance.dispatch_UIevent(SysDefine.UI_Biotank_List_Init, null);

        return bioTank;
    }

    /// <summary>
    /// 检查并启动新客户端
    /// 优先使用预加载池中的休眠客户端，如果没有则启动新进程
    /// </summary>
    private void CheckAndLaunchNewClient()
    {
        int activeClientCount = ControlBehavior.clients.Count;
        int bioTankCount = BioTankRegistry.Instance.Count;

        if (bioTankCount > activeClientCount)
        {
            // 获取新创建的生态箱ID
            var allBioTanks = BioTankRegistry.Instance.GetAllBioTanks();
            int newBioTankId = -1;

            foreach (var bioTank in allBioTanks)
            {
                if (!ControlBehavior.clients.ContainsKey(bioTank.stateData.ID))
                {
                    newBioTankId = bioTank.stateData.ID;
                    break;
                }
            }

            if (newBioTankId == -1)
            {
                Debug.LogError("[BioTankCreationService] 无法找到新创建的生态箱ID");
                HiddenProcessLauncher.Instance.LaunchHiddenProcess();
                return;
            }

            // 优先尝试使用预加载池中的休眠客户端
            if (PreloadClientPoolManager.Instance != null &&
                PreloadClientPoolManager.Instance.HasAvailableDormantClient)
            {
                Debug.Log($"[BioTankCreationService] 使用预加载池中的休眠客户端激活生态箱 ID={newBioTankId}");

                bool success = PreloadClientPoolManager.Instance.TryActivateDormantClient(newBioTankId);
                if (success)
                {
                    Debug.Log($"[BioTankCreationService] 休眠客户端激活成功，生态箱 ID={newBioTankId}");
                    return;
                }
                else
                {
                    Debug.LogWarning("[BioTankCreationService] 休眠客户端激活失败，回退到启动新进程");
                }
            }
            else
            {
                Debug.Log("[BioTankCreationService] 预加载池中没有可用的休眠客户端，启动新进程");
            }

            // 回退方案：启动新的客户端进程，服务端会分配ID
            HiddenProcessLauncher.Instance.LaunchHiddenProcess();
        }
    }

    /// <summary>
    /// 使用预览客户端启动生态箱
    /// </summary>
    /// <param name="newBioTankId">新生态箱ID</param>
    private void ActivatePreviewClient(int newBioTankId)
    {
        if (PreloadClientPoolManager.Instance != null && PreloadClientPoolManager.Instance.IsInPreviewMode)
        {
            Debug.Log($"[BioTankCreationService] 使用预览客户端激活生态箱 ID={newBioTankId}");
            PreloadClientPoolManager.Instance.ConfirmPreviewCreation(newBioTankId);
        }
        else
        {
            Debug.LogWarning("[BioTankCreationService] 预览客户端不可用，使用常规方式启动");
            CheckAndLaunchNewClient();
        }
    }

    /// <summary>
    /// 获取生态箱的默认名称（文本ID 3 + 序号）
    /// </summary>
    private string GetBiotankDefaultName(TankData tankData)
    {
        // 计算所有生态箱的总序号
        int index = GetNextBiotankIndex();

        // 使用文本ID 3 作为基础名称
        string baseName = LocalizationManager.GetCurrentLanguageByID(3);
        return baseName + index;
    }

    /// <summary>
    /// 获取下一个生态箱序号（所有生态箱共用序号）
    /// </summary>
    private int GetNextBiotankIndex()
    {
        return BioTankRegistry.Instance.Count + 1;
    }

    /// <summary>
    /// 生成生态箱默认名称（文本ID 3 + 指定序号）
    /// 供外部调用，用于旧存档兼容
    /// </summary>
    public static string GenerateDefaultName(int index)
    {
        string baseName = LocalizationManager.GetCurrentLanguageByID(3);
        return baseName + index;
    }

    #endregion
}
