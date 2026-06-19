using UnityEngine;
using SUIFW;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UI_Biotank_AddBiotank : BaseUIForms
{
    private Vector3 _Size;
    public Dictionary<Button,Vector3> buttonSize = new Dictionary<Button, Vector3>();
    /// <summary>
    /// 尺寸按钮的父容器Content
    /// </summary>
    public Transform SizeButtonContent;
    /// <summary>
    /// 尺寸按钮模板
    /// </summary>
    public Button SizeButtonTemplate;
    /// <summary>
    /// 当前选中的尺寸按钮
    /// </summary>
    private Button _selectedSizeButton;
    /// <summary>
    /// 已创建的尺寸按钮列表
    /// </summary>
    private List<Button> _sizeButtons = new List<Button>();

    #region 初始布局相关字段
    /// <summary>
    /// 布局按钮的父容器Content
    /// </summary>
    public Transform LayoutButtonContent;
    /// <summary>
    /// 布局按钮模板
    /// </summary>
    public Button LayoutButtonTemplate;
    /// <summary>
    /// 当前选中的布局按钮
    /// </summary>
    private Button _selectedLayoutButton;
    /// <summary>
    /// 已创建的布局按钮列表
    /// </summary>
    private List<Button> _layoutButtons = new List<Button>();
    /// <summary>
    /// 布局按钮与布局数据的映射
    /// </summary>
    private Dictionary<Button, TankInitialLayoutData> _buttonLayoutMap = new Dictionary<Button, TankInitialLayoutData>();
    /// <summary>
    /// 当前选中的布局数据
    /// </summary>
    private TankInitialLayoutData _selectedLayout;
    /// <summary>
    /// 当前选中的TankData（用于计算价格）
    /// </summary>
    private TankData _selectedTankData;
    /// <summary>
    /// 费用显示文本
    /// </summary>
    public TextMeshProUGUI CostText;
    /// <summary>
    /// 新建生态箱按钮
    /// </summary>
    public Button CreateBioTankBtn;
    #endregion

    #region 预览模式相关字段
    /// <summary>
    /// 是否已进入预览模式
    /// </summary>
    private bool _isPreviewMode = false;
    /// <summary>
    /// 是否正在执行创建操作（防止重复点击）
    /// </summary>
    private bool _isCreating = false;
    #endregion

    public Button ExitBtn;

    public override bool Init()
    {
        this.gameObject.SetActive(true);

        // 窗口扩展后，将界面放到右侧扩展区域
        float offsetX = 445f;
        transform.localPosition = new Vector3(offsetX, 0f, 0f);
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(offsetX, 0f);
        }

        // 扫描并注册所有本地化文本（自动处理所有"Id:xxx"格式的文本）
        LocalizationManager.ScanAndRegisterTextsStatic(transform);

        // 先注册窗口打开，再关闭其他界面，确保窗口计数正确不会闪烁
        BiotankWindowManager.OnWindowOpen();

        // 关闭编辑界面（互斥逻辑）
        if (UI_BioTank_Edit.CurrentInstance != null && UI_BioTank_Edit.CurrentInstance.gameObject.activeSelf)
        {
            UI_BioTank_Edit.CurrentInstance.Close();
        }

        // 关闭模组管理界面（互斥逻辑）
        UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_ModManager);

        ExitBtn.AddDeskTopListener(() =>
        {
            CloseOrReturnUIForms();
        });

        // 初始化尺寸按钮列表
        InitSizeButtons();

        // 新建生态箱按钮 - 使用BioTankCreationService创建
        if (CreateBioTankBtn != null)
        {
            CreateBioTankBtn.AddDeskTopListener(OnCreateBioTankClicked);
        }

        // 进入预览模式
        EnterPreviewMode();

        return base.Init();
    }

    /// <summary>
    /// 界面关闭/隐藏时调用
    /// </summary>
    private void OnDisable()
    {
        // 如果还在预览模式且没有执行创建操作，则退出预览模式
        if (_isPreviewMode && !_isCreating)
        {
            ExitPreviewMode();
        }
    }

    /// <summary>
    /// 初始化尺寸按钮列表
    /// 从Gather获取已解锁的生态箱尺寸，按Sort参数排序，动态创建按钮
    /// </summary>
    private void InitSizeButtons()
    {
        // 清理之前的按钮
        ClearSizeButtons();

        // 获取所有已解锁的TankData，按Sort排序
        var unlockedTanks = GetUnlockedTankDataSorted();

        if (unlockedTanks.Count == 0)
        {
            Debug.LogWarning("[UI_Biotank_AddBiotank] 没有已解锁的生态箱尺寸");
            return;
        }

        // 隐藏模板按钮
        if (SizeButtonTemplate != null)
        {
            SizeButtonTemplate.gameObject.SetActive(false);
        }

        // 为每个解锁的尺寸创建按钮
        for (int i = 0; i < unlockedTanks.Count; i++)
        {
            var tankData = unlockedTanks[i];
            CreateSizeButton(tankData, i == 0);
        }
    }

    /// <summary>
    /// 清理已创建的尺寸按钮
    /// </summary>
    private void ClearSizeButtons()
    {
        foreach (var btn in _sizeButtons)
        {
            if (btn != null)
            {
                Destroy(btn.gameObject);
            }
        }
        _sizeButtons.Clear();
        buttonSize.Clear();
        _selectedSizeButton = null;
    }

    /// <summary>
    /// 获取已解锁的TankData列表，按Sort参数升序排序
    /// </summary>
    private List<TankData> GetUnlockedTankDataSorted()
    {
        var result = new List<TankData>();

        // 从Gather获取TankUnlockDic
        var tankUnlockDic = Gather.Instance?.TankUnlockDic;
        if (tankUnlockDic == null)
        {
            Debug.LogWarning("[UI_Biotank_AddBiotank] Gather.Instance.TankUnlockDic为空");
            return result;
        }

        // 获取所有TankData配置
        var allTankData = GameConfigDataBase.GetConfigDatas<TankData>(SysDefine.TankConfigData);

        // 筛选已解锁的TankData
        foreach (var tankData in allTankData)
        {
            if (tankUnlockDic.TryGetValue(tankData.ID, out bool isUnlocked) && isUnlocked)
            {
                result.Add(tankData);
            }
        }

        // 按Sort参数升序排序
        result.Sort((a, b) => a.Sort.CompareTo(b.Sort));

        return result;
    }

    /// <summary>
    /// 创建尺寸按钮
    /// </summary>
    /// <param name="tankData">TankData配置数据</param>
    /// <param name="selectByDefault">是否默认选中</param>
    private void CreateSizeButton(TankData tankData, bool selectByDefault)
    {
        if (SizeButtonTemplate == null || SizeButtonContent == null)
        {
            Debug.LogError("[UI_Biotank_AddBiotank] SizeButtonTemplate或SizeButtonContent未设置");
            return;
        }

        // 实例化按钮
        var buttonObj = Instantiate(SizeButtonTemplate.gameObject, SizeButtonContent);
        buttonObj.SetActive(true);

        var button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[UI_Biotank_AddBiotank] 按钮模板缺少Button组件");
            Destroy(buttonObj);
            return;
        }

        // 设置按钮文本为 X*Y*Z 格式
        var sizeText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (sizeText != null)
        {
            sizeText.text = $"{tankData.X}*{tankData.Y}*{tankData.Z}";
        }

        // 存储按钮对应的尺寸
        Vector3 size = new Vector3(tankData.X, tankData.Y, tankData.Z);
        buttonSize.Add(button, size);
        _sizeButtons.Add(button);

        // 添加点击事件，传递tankData用于价格计算
        button.AddDeskTopListener(() => OnSizeButtonClicked(button, size, tankData));

        // 默认选中第一个
        if (selectByDefault)
        {
            OnSizeButtonClicked(button, size, tankData);
        }
    }

    /// <summary>
    /// 尺寸按钮点击回调
    /// </summary>
    /// <param name="clickedButton">被点击的按钮</param>
    /// <param name="size">对应的尺寸</param>
    /// <param name="tankData">对应的TankData配置</param>
    private void OnSizeButtonClicked(Button clickedButton, Vector3 size, TankData tankData)
    {
        // 重置所有按钮颜色
        foreach (var btn in buttonSize.Keys)
        {
            btn.image.color = Color.white;
        }

        // 设置选中按钮颜色
        clickedButton.image.color = Color.black;
        _selectedSizeButton = clickedButton;
        _Size = size;
        _selectedTankData = tankData;

        // 刷新该尺寸的初始布局列表
        RefreshLayoutButtons((int)size.x, (int)size.y, (int)size.z);

        // 更新预览（尺寸改变时会自动选中第一个布局，RefreshLayoutButtons中会调用UpdatePreview）
    }

    #region 初始布局相关方法

    /// <summary>
    /// 刷新指定尺寸的布局按钮列表
    /// </summary>
    /// <param name="sizeX">尺寸X</param>
    /// <param name="sizeY">尺寸Y</param>
    /// <param name="sizeZ">尺寸Z</param>
    private void RefreshLayoutButtons(int sizeX, int sizeY, int sizeZ)
    {
        // 清理之前的布局按钮
        ClearLayoutButtons();

        // 获取该尺寸的所有布局（不包含空布局选项）
        var layouts = TankInitialLayoutManager.Instance.GetLayoutsForSize(sizeX, sizeY, sizeZ);

        if (layouts.Count == 0)
        {
            Debug.Log($"[UI_Biotank_AddBiotank] 尺寸 {sizeX}*{sizeY}*{sizeZ} 没有可用的布局");
            return;
        }

        // 隐藏模板按钮
        if (LayoutButtonTemplate != null)
        {
            LayoutButtonTemplate.gameObject.SetActive(false);
        }

        // 为每个布局创建按钮
        for (int i = 0; i < layouts.Count; i++)
        {
            var layout = layouts[i];
            CreateLayoutButton(layout, i == 0);
        }
    }

    /// <summary>
    /// 清理已创建的布局按钮
    /// </summary>
    private void ClearLayoutButtons()
    {
        foreach (var btn in _layoutButtons)
        {
            if (btn != null)
            {
                Destroy(btn.gameObject);
            }
        }
        _layoutButtons.Clear();
        _buttonLayoutMap.Clear();
        _selectedLayoutButton = null;
        _selectedLayout = null;
    }

    /// <summary>
    /// 创建布局按钮
    /// </summary>
    /// <param name="layout">布局数据</param>
    /// <param name="selectByDefault">是否默认选中</param>
    private void CreateLayoutButton(TankInitialLayoutData layout, bool selectByDefault)
    {
        if (LayoutButtonTemplate == null || LayoutButtonContent == null)
        {
            Debug.LogError("[UI_Biotank_AddBiotank] LayoutButtonTemplate或LayoutButtonContent未设置");
            return;
        }

        // 实例化按钮
        var buttonObj = Instantiate(LayoutButtonTemplate.gameObject, LayoutButtonContent);
        buttonObj.SetActive(true);

        var button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[UI_Biotank_AddBiotank] 布局按钮模板缺少Button组件");
            Destroy(buttonObj);
            return;
        }

        // 获取所有TextMeshProUGUI组件
        var textComponents = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();

        if (textComponents.Length >= 2)
        {
            // 第一个文本显示布局名称（解析本地化ID）
            textComponents[0].text = GetLocalizedText(layout.layoutName);
            // 第二个文本显示布局描述（解析本地化ID）
            textComponents[1].text = GetLocalizedText(layout.description ?? "");
        }
        else if (textComponents.Length == 1)
        {
            // 只有一个文本组件时，显示名称
            textComponents[0].text = GetLocalizedText(layout.layoutName);
            Debug.LogWarning("[UI_Biotank_AddBiotank] 布局按钮模板只有一个文本组件，无法显示描述");
        }

        // 存储按钮与布局的映射
        _buttonLayoutMap.Add(button, layout);
        _layoutButtons.Add(button);

        // 添加点击事件
        button.AddDeskTopListener(() => OnLayoutButtonClicked(button, layout));

        // 默认选中第一个
        if (selectByDefault)
        {
            OnLayoutButtonClicked(button, layout);
        }
    }

    /// <summary>
    /// 布局按钮点击回调
    /// </summary>
    /// <param name="clickedButton">被点击的按钮</param>
    /// <param name="layout">对应的布局数据</param>
    private void OnLayoutButtonClicked(Button clickedButton, TankInitialLayoutData layout)
    {
        // 重置所有布局按钮颜色
        foreach (var btn in _buttonLayoutMap.Keys)
        {
            btn.image.color = Color.white;
        }

        // 设置选中按钮颜色
        clickedButton.image.color = Color.black;
        _selectedLayoutButton = clickedButton;
        _selectedLayout = layout;

        // 更新费用显示
        UpdateCostDisplay();

        // 更新预览
        UpdatePreview();
    }

    /// <summary>
    /// 更新费用显示
    /// 使用BioTankCreationService计算完整费用（包含生态箱基础价格、数量费用、布局内容费用）
    /// </summary>
    private void UpdateCostDisplay()
    {
        if (CostText == null)
        {
            return;
        }

        if (_selectedTankData == null)
        {
            CostText.text = Tools.MoneyString(0);
            return;
        }

        // 使用BioTankCreationService计算完整费用
        string layoutId = _selectedLayout?.layoutId;
        var costResult = BioTankCreationService.Instance.CalculateCreationCost(_selectedTankData.ID, layoutId);

        // 使用通用方法格式化金额显示
        CostText.text = Tools.MoneyString(costResult.TotalCost);
    }

    /// <summary>
    /// 获取当前选中的布局
    /// </summary>
    public TankInitialLayoutData GetSelectedLayout()
    {
        return _selectedLayout;
    }

    /// <summary>
    /// 获取当前新建生态箱的总费用
    /// </summary>
    public int GetTotalCost()
    {
        if (_selectedTankData == null)
        {
            return 0;
        }

        string layoutId = _selectedLayout?.layoutId;
        var costResult = BioTankCreationService.Instance.CalculateCreationCost(_selectedTankData.ID, layoutId);
        return costResult.TotalCost;
    }

    /// <summary>
    /// 新建生态箱按钮点击事件
    /// 使用BioTankCreationService创建生态箱
    /// </summary>
    private void OnCreateBioTankClicked()
    {
        // 防止重复点击
        if (_isCreating)
        {
            return;
        }

        // 检查是否选择了尺寸
        if (_selectedTankData == null)
        {
            Debug.LogWarning("[UI_Biotank_AddBiotank] 未选择生态箱尺寸");
            ToastManager.Show(LocalizationManager.GetCurrentLanguageByID(158));
            return;
        }

        _isCreating = true;

        // 使用BioTankCreationService创建生态箱（支持预览模式）
        var result = BioTankCreationService.Instance.CreateBioTankWithPreview(
            _selectedTankData.ID,
            _selectedLayout?.layoutId,
            _isPreviewMode,
            showToast: true);

        if (result.Success)
        {
            Debug.Log($"[UI_Biotank_AddBiotank] 成功创建生态箱 ID={result.BioTank?.stateData?.ID}");

            // 创建成功，预览模式已在CreateBioTankWithPreview中处理
            _isPreviewMode = false;

            // 创建成功后关闭界面
            CloseOrReturnUIForms();
        }
        else
        {
            Debug.LogWarning($"[UI_Biotank_AddBiotank] 创建生态箱失败: {result.ErrorMessage}");
            _isCreating = false;
        }
    }

    /// <summary>
    /// 解析文本，如果是"Id:xxxx"格式则返回本地化文本，否则返回原文本
    /// </summary>
    /// <param name="text">原始文本</param>
    /// <returns>本地化后的文本</returns>
    private string GetLocalizedText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        if (text.StartsWith("Id:"))
        {
            if (int.TryParse(text.Substring(3), out int textId))
            {
                return LocalizationManager.GetCurrentLanguageByID(textId);
            }
        }

        return text;
    }

    #endregion

    #region 预览模式相关方法

    /// <summary>
    /// 进入预览模式
    /// </summary>
    private void EnterPreviewMode()
    {
        // 检查预加载池是否可用
        if (PreloadClientPoolManager.Instance == null || !PreloadClientPoolManager.Instance.IsEnabled)
        {
            Debug.Log("[UI_Biotank_AddBiotank] 预加载池未启用，跳过预览模式");
            return;
        }

        if (!PreloadClientPoolManager.Instance.HasAvailableDormantClient)
        {
            Debug.Log("[UI_Biotank_AddBiotank] 没有可用的休眠客户端，跳过预览模式");
            return;
        }

        // 获取当前选中的尺寸和布局
        int sizeX = _selectedTankData != null ? _selectedTankData.X : 10;
        int sizeY = _selectedTankData != null ? _selectedTankData.Y : 10;
        int sizeZ = _selectedTankData != null ? _selectedTankData.Z : 10;
        string layoutId = _selectedLayout?.layoutId;

        // 尝试进入预览模式
        bool success = PreloadClientPoolManager.Instance.EnterPreviewMode(sizeX, sizeY, sizeZ, layoutId);
        if (success)
        {
            _isPreviewMode = true;
            Debug.Log($"[UI_Biotank_AddBiotank] 成功进入预览模式，尺寸: {sizeX}x{sizeY}x{sizeZ}，布局: {layoutId ?? "空"}");
        }
        else
        {
            Debug.LogWarning("[UI_Biotank_AddBiotank] 进入预览模式失败");
        }
    }

    /// <summary>
    /// 退出预览模式（不创建生态箱）
    /// </summary>
    private void ExitPreviewMode()
    {
        if (!_isPreviewMode)
        {
            return;
        }

        if (PreloadClientPoolManager.Instance != null && PreloadClientPoolManager.Instance.IsInPreviewMode)
        {
            PreloadClientPoolManager.Instance.ExitPreviewMode();
            Debug.Log("[UI_Biotank_AddBiotank] 已退出预览模式");
        }

        _isPreviewMode = false;
    }

    /// <summary>
    /// 更新预览
    /// 当尺寸或布局改变时调用
    /// </summary>
    private void UpdatePreview()
    {
        if (!_isPreviewMode)
        {
            return;
        }

        if (PreloadClientPoolManager.Instance == null || !PreloadClientPoolManager.Instance.IsInPreviewMode)
        {
            return;
        }

        // 获取当前选中的尺寸和布局
        int sizeX = _selectedTankData != null ? _selectedTankData.X : 10;
        int sizeY = _selectedTankData != null ? _selectedTankData.Y : 10;
        int sizeZ = _selectedTankData != null ? _selectedTankData.Z : 10;
        string layoutId = _selectedLayout?.layoutId;

        // 更新预览布局
        PreloadClientPoolManager.Instance.UpdatePreviewLayout(sizeX, sizeY, sizeZ, layoutId);
        Debug.Log($"[UI_Biotank_AddBiotank] 更新预览，尺寸: {sizeX}x{sizeY}x{sizeZ}，布局: {layoutId ?? "空"}");
    }

    #endregion

    /// <summary>
    /// 重写Hiding方法，在界面隐藏时恢复窗口布局
    /// </summary>
    public override void Hiding()
    {
        base.Hiding();

        // 通知窗口管理器，由它决定是否恢复窗口尺寸
        BiotankWindowManager.OnWindowClose();
    }
}