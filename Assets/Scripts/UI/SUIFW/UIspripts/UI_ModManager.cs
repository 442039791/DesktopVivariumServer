using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SUIFW;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 模组管理界面（简化版）
/// 功能：
/// 1. 模组列表（详情直接显示在列表项上）
/// 2. 应用更改按钮（带重启确认）
/// 3. 自动排序按钮（根据依赖关系排序）
/// 4. 关闭按钮
/// </summary>
public class UI_ModManager : BaseUIForms
{
    [Header("按钮")]
    public Button CloseBtn;           // 关闭按钮
    public Button ApplyBtn;           // 应用更改按钮
    public Button AutoSortBtn;        // 自动排序按钮

    [Header("模组列表")]
    public Transform ListParent;           // 列表项父节点
    public GameObject ModItemTemplate;     // 模组项模板

    [Header("确认对话框")]
    public GameObject ConfirmDialog;       // 确认对话框面板
    public TextMeshProUGUI ConfirmText;    // 确认文本
    public Button ConfirmYesBtn;           // 确认按钮
    public Button ConfirmNoBtn;            // 取消按钮

    private List<UI_ModManager_Item> _modItems = new List<UI_ModManager_Item>();
    private bool _hasChanges = false;
    private bool _isInitialized = false;

    // 确认对话框类型
    private enum ConfirmDialogType
    {
        None,
        Restart,    // 应用更改并重启
        Discard     // 放弃更改并关闭
    }
    private ConfirmDialogType _currentDialogType = ConfirmDialogType.None;

    // 原始模组状态备份（用于还原）
    private Dictionary<string, bool> _originalEnabledStates = new Dictionary<string, bool>();
    private Dictionary<string, int> _originalLoadOrders = new Dictionary<string, int>();

    public override bool Init()
    {
        if (_isInitialized)
        {
            // 重新打开时也需要设置位置和处理窗口
            SetupWindowPosition();
            RefreshModList();
            return true;
        }

        // 窗口扩展后，将界面放到右侧扩展区域（与UI_BioTank_Edit相同）
        SetupWindowPosition();

        // 先注册窗口打开，再关闭其他界面，确保窗口计数正确不会闪烁
        BiotankWindowManager.OnWindowOpen();

        // 关闭编辑界面（互斥逻辑）
        if (UI_BioTank_Edit.CurrentInstance != null && UI_BioTank_Edit.CurrentInstance.gameObject.activeSelf)
        {
            UI_BioTank_Edit.CurrentInstance.Close();
        }

        // 关闭生态箱列表界面（互斥逻辑）
        UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_Biotank_List);

        // 关闭新建生态箱界面（互斥逻辑）
        UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_Biotank_AddBiotank);

        // 关闭按钮
        if (CloseBtn != null)
        {
            CloseBtn.AddDeskTopListener(() =>
            {
                OnCloseButtonClicked();
            });
        }

        // 应用更改按钮
        if (ApplyBtn != null)
        {
            ApplyBtn.AddDeskTopListener(() =>
            {
                ShowRestartConfirmDialog();
            });
        }

        // 自动排序按钮
        if (AutoSortBtn != null)
        {
            AutoSortBtn.AddDeskTopListener(() =>
            {
                AutoSortMods();
            });
        }

        // 确认对话框按钮
        if (ConfirmYesBtn != null)
        {
            ConfirmYesBtn.AddDeskTopListener(() =>
            {
                OnConfirmYes();
            });
        }

        if (ConfirmNoBtn != null)
        {
            ConfirmNoBtn.AddDeskTopListener(() =>
            {
                OnConfirmNo();
            });
        }

        // 隐藏模板和对话框
        if (ModItemTemplate != null)
        {
            ModItemTemplate.SetActive(false);
        }

        if (ConfirmDialog != null)
        {
            ConfirmDialog.SetActive(false);
        }

        // 监听模组刷新事件
        event_manager.instance.add_UIevent_listener(SysDefine.UI_ModManager_Refresh, OnRefreshEvent);

        // 初始化模组列表
        RefreshModList();

        _isInitialized = true;
        return base.Init();
    }

    private void OnRefreshEvent(string name, object data)
    {
        RefreshModList();
    }

    /// <summary>
    /// 刷新模组列表
    /// </summary>
    public void RefreshModList()
    {
        // 清除旧列表项
        ClearModItems();

        if (ModManager.Instance == null || !ModManager.Instance.IsInitialized)
        {
            return;
        }

        // 重新发现模组
        ModManager.Instance.DiscoverMods();

        var allMods = ModManager.Instance.GetAllMods();

        // 首次加载时备份原始状态
        if (_originalEnabledStates.Count == 0)
        {
            BackupOriginalStates(allMods);
        }

        // 按加载顺序排序
        var sortedMods = allMods.Values.OrderBy(m => m.LoadOrder).ToList();

        foreach (var modInfo in sortedMods)
        {
            CreateModItem(modInfo);
        }
    }

    /// <summary>
    /// 备份原始模组状态
    /// </summary>
    private void BackupOriginalStates(IReadOnlyDictionary<string, ModInfo> allMods)
    {
        _originalEnabledStates.Clear();
        _originalLoadOrders.Clear();

        foreach (var kvp in allMods)
        {
            _originalEnabledStates[kvp.Key] = kvp.Value.IsEnabled;
            _originalLoadOrders[kvp.Key] = kvp.Value.LoadOrder;
        }
    }

    /// <summary>
    /// 创建模组列表项
    /// </summary>
    private void CreateModItem(ModInfo modInfo)
    {
        if (ModItemTemplate == null || ListParent == null)
        {
            Debug.LogError("[UI_ModManager] ModItemTemplate或ListParent未设置");
            return;
        }

        GameObject itemObj = Instantiate(ModItemTemplate, ListParent);
        itemObj.SetActive(true);

        var item = itemObj.GetComponent<UI_ModManager_Item>();
        if (item == null)
        {
            item = itemObj.AddComponent<UI_ModManager_Item>();
        }

        item.Initialize(modInfo, this);
        _modItems.Add(item);
    }

    /// <summary>
    /// 清除所有模组列表项
    /// </summary>
    private void ClearModItems()
    {
        foreach (var item in _modItems)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        _modItems.Clear();
    }

    /// <summary>
    /// 切换模组启用状态
    /// </summary>
    public void ToggleModEnabled(ModInfo modInfo)
    {
        modInfo.IsEnabled = !modInfo.IsEnabled;
        MarkAsChanged();
    }

    /// <summary>
    /// 模组项拖拽放置完成回调
    /// </summary>
    public void OnModItemDropped(UI_ModManager_Item droppedItem, int newIndex)
    {
        if (droppedItem == null || droppedItem.ModInfo == null) return;

        // 根据UI中的实际顺序重新排列_modItems列表
        _modItems.Clear();
        for (int i = 0; i < ListParent.childCount; i++)
        {
            var child = ListParent.GetChild(i);
            if (child == null || !child.gameObject.activeSelf) continue;

            var item = child.GetComponent<UI_ModManager_Item>();
            if (item != null && item.ModInfo != null)
            {
                _modItems.Add(item);
                item.ModInfo.LoadOrder = i * 10;
            }
        }

        MarkAsChanged();

        // 不立即刷新列表，避免在拖拽结束时销毁对象导致错误
    }

    /// <summary>
    /// 自动排序模组（根据依赖关系）
    /// </summary>
    private void AutoSortMods()
    {
        if (ModManager.Instance == null) return;

        var allMods = ModManager.Instance.GetAllMods().Values.ToList();
        var sortedMods = TopologicalSort(allMods);

        // 重新分配加载顺序
        for (int i = 0; i < sortedMods.Count; i++)
        {
            sortedMods[i].LoadOrder = i * 10; // 间隔10，方便后续插入
        }

        MarkAsChanged();
        RefreshModList();

        // 显示提示
        if (ToastManager.Instance != null)
        {
            ToastManager.Instance.ShowToast("模组已按依赖关系自动排序");
        }

        Debug.Log("[UI_ModManager] 模组已自动排序");
    }

    /// <summary>
    /// 拓扑排序（根据依赖关系）
    /// 被依赖的模组排在前面
    /// </summary>
    private List<ModInfo> TopologicalSort(List<ModInfo> mods)
    {
        var result = new List<ModInfo>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>(); // 用于检测循环依赖

        void Visit(ModInfo mod)
        {
            if (visited.Contains(mod.ModId)) return;

            if (visiting.Contains(mod.ModId))
            {
                // 检测到循环依赖，跳过
                Debug.LogWarning($"[UI_ModManager] 检测到循环依赖: {mod.ModId}");
                return;
            }

            visiting.Add(mod.ModId);

            // 先访问依赖
            var deps = mod.Manifest?.Dependencies;
            if (deps != null)
            {
                foreach (var depId in deps)
                {
                    var depMod = mods.Find(m => m.ModId == depId);
                    if (depMod != null)
                    {
                        Visit(depMod);
                    }
                }
            }

            visiting.Remove(mod.ModId);
            visited.Add(mod.ModId);
            result.Add(mod);
        }

        foreach (var mod in mods)
        {
            Visit(mod);
        }

        return result;
    }

    /// <summary>
    /// 标记有未保存的更改
    /// </summary>
    public void MarkAsChanged()
    {
        _hasChanges = true;

        if (ApplyBtn != null)
        {
            ApplyBtn.interactable = true;
        }
    }

    /// <summary>
    /// 关闭按钮点击处理
    /// </summary>
    private void OnCloseButtonClicked()
    {
        if (_hasChanges)
        {
            // 有未保存的更改，显示放弃确认对话框
            ShowDiscardConfirmDialog();
        }
        else
        {
            // 没有更改，直接关闭
            CloseAndCleanup();
        }
    }

    /// <summary>
    /// 显示重启确认对话框
    /// </summary>
    private void ShowRestartConfirmDialog()
    {
        _currentDialogType = ConfirmDialogType.Restart;

        if (ConfirmDialog != null)
        {
            ConfirmDialog.SetActive(true);
        }

        if (ConfirmText != null)
        {
            ConfirmText.text = "是否保存更改并立即重启游戏？\n模组更改需要重启后才能生效。";
        }
    }

    /// <summary>
    /// 显示放弃更改确认对话框
    /// </summary>
    private void ShowDiscardConfirmDialog()
    {
        _currentDialogType = ConfirmDialogType.Discard;

        if (ConfirmDialog != null)
        {
            ConfirmDialog.SetActive(true);
        }

        if (ConfirmText != null)
        {
            ConfirmText.text = "您有未保存的更改。\n关闭界面后所有修改将会被还原，是否确认？";
        }
    }

    /// <summary>
    /// 隐藏确认对话框
    /// </summary>
    private void HideConfirmDialog()
    {
        _currentDialogType = ConfirmDialogType.None;

        if (ConfirmDialog != null)
        {
            ConfirmDialog.SetActive(false);
        }
    }

    /// <summary>
    /// 确认按钮点击处理
    /// </summary>
    private void OnConfirmYes()
    {
        switch (_currentDialogType)
        {
            case ConfirmDialogType.Restart:
                OnConfirmRestart();
                break;
            case ConfirmDialogType.Discard:
                OnConfirmDiscard();
                break;
        }
    }

    /// <summary>
    /// 取消按钮点击处理
    /// </summary>
    private void OnConfirmNo()
    {
        HideConfirmDialog();
    }

    /// <summary>
    /// 确认重启
    /// </summary>
    private void OnConfirmRestart()
    {
        // 保存设置
        if (ModManager.Instance != null)
        {
            ModManager.Instance.SaveUserSettings();
        }

        _hasChanges = false;

        HideConfirmDialog();

        Debug.Log("[UI_ModManager] 保存模组设置并重启游戏");

        // 重启游戏
        RestartGame();
    }

    /// <summary>
    /// 确认放弃更改
    /// </summary>
    private void OnConfirmDiscard()
    {
        // 还原所有更改
        RevertChanges();

        _hasChanges = false;

        HideConfirmDialog();

        Debug.Log("[UI_ModManager] 放弃更改并关闭界面");

        // 关闭界面
        CloseAndCleanup();
    }

    /// <summary>
    /// 还原模组更改到原始状态
    /// </summary>
    private void RevertChanges()
    {
        if (ModManager.Instance == null) return;

        var allMods = ModManager.Instance.GetAllMods();

        foreach (var kvp in allMods)
        {
            string modId = kvp.Key;
            ModInfo modInfo = kvp.Value;

            // 还原启用状态
            if (_originalEnabledStates.TryGetValue(modId, out bool originalEnabled))
            {
                modInfo.IsEnabled = originalEnabled;
            }

            // 还原加载顺序
            if (_originalLoadOrders.TryGetValue(modId, out int originalOrder))
            {
                modInfo.LoadOrder = originalOrder;
            }
        }

        Debug.Log("[UI_ModManager] 已还原所有模组更改");
    }

    /// <summary>
    /// 关闭界面并清理状态
    /// </summary>
    private void CloseAndCleanup()
    {
        // 清除备份状态
        _originalEnabledStates.Clear();
        _originalLoadOrders.Clear();
        _hasChanges = false;

        // 关闭界面
        CloseOrReturnUIForms();
    }

    /// <summary>
    /// 重启游戏
    /// </summary>
    private void RestartGame()
    {
        // 先保存游戏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Save();
        }

        // 获取当前可执行文件路径并重启
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        // 启动新进程
        System.Diagnostics.Process.Start(exePath);

        // 退出当前进程
        Application.Quit();

#if UNITY_EDITOR
        // 编辑器模式下停止播放
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public override void Hiding()
    {
        base.Hiding();

        // 通知窗口管理器，由它决定是否恢复窗口尺寸
        BiotankWindowManager.OnWindowClose();
    }

    /// <summary>
    /// 设置界面位置到右侧扩展区域（与UI_BioTank_Edit相同）
    /// </summary>
    private void SetupWindowPosition()
    {
        float offsetX = 445f;
        transform.localPosition = new Vector3(offsetX, 0f, 0f);
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(offsetX, 0f);
        }
    }

    private void OnDestroy()
    {
        event_manager.instance.remove_UIevent_listener(SysDefine.UI_ModManager_Refresh);
    }
}
