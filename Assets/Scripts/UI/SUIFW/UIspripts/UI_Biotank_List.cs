using UnityEngine;
using SUIFW;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UI_Biotank_List : BaseUIForms
{
    [Header("按钮")]
    public Button ExitBtn;
    public Button AddBioTankBtn;

    [Header("列表模板")]
    public GameObject BiotankInfoTemplate;  // Biotank_Info模板物体
    public Transform ListParent;            // 列表项的父物体(ListControl)

    [Header("选中效果")]
    public Color normalColor = Color.white;         // 普通状态颜色
    public Color selectedColor = Color.yellow;      // 选中状态颜色

    private bool InitComplete = false;
    private bool OnShow = false;
    private List<GameObject> biotankItems = new List<GameObject>();  // 存储生成的列表项

    public override bool Init()
    {
        this.gameObject.SetActive(true);
        OnShow = true;

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

        // 刷新生态箱列表
        RefreshBiotankList();

        if (InitComplete)
            return true;

        // 监听列表刷新事件
        event_manager.instance.add_UIevent_listener(SysDefine.UI_Biotank_List_Init, on_event_handler);

        // 新建生态箱按钮
        AddBioTankBtn.AddDeskTopListener(() =>
        {
            // 先打开新界面，再关闭当前界面，确保窗口计数正确
            var addBiotank = UIManager.Instance.ShowUIForms(SysDefine.UI_Biotank_AddBiotank);

            // 关闭生态箱列表界面
            CloseOrReturnUIForms();
        });

        // 关闭按钮（Hiding方法会处理窗口恢复）
        ExitBtn.AddDeskTopListener(() =>
        {
            CloseOrReturnUIForms();
        });

        InitComplete = true;
        return base.Init();
    }

    public void on_event_handler(string name, object udata)
    {
        if (!OnShow)
            return;
        RefreshBiotankList();
    }

    /// <summary>
    /// 刷新生态箱列表
    /// </summary>
    private void RefreshBiotankList()
    {
        // 清除旧的列表项(除了模板)
        ClearBiotankItems();

        // 隐藏模板
        if (BiotankInfoTemplate != null)
        {
            BiotankInfoTemplate.SetActive(false);
        }

        // 获取所有生态箱
        var allBioTanks = BioTankRegistry.Instance.GetAllBioTanks();

        // 为每个生态箱创建列表项
        for (int i = 0; i < allBioTanks.Count; i++)
        {
            var bioTank = allBioTanks[i];
            CreateBiotankItem(bioTank.stateData.ID, i);
        }
    }

    /// <summary>
    /// 创建一个生态箱列表项
    /// </summary>
    private void CreateBiotankItem(int biotankID, int index)
    {
        if (BiotankInfoTemplate == null || ListParent == null)
        {
            Debug.LogError("[UI_Biotank_List] BiotankInfoTemplate或ListParent未设置");
            return;
        }

        // 复制模板
        GameObject item = Instantiate(BiotankInfoTemplate, ListParent);
        item.SetActive(true);
        biotankItems.Add(item);

        // 设置生态箱名称
        var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            var biotank = PlayerManager.Instance.GetBioTank(biotankID);
            if (biotank != null)
            {
                string displayName = GetBiotankDisplayName(biotank);
                nameText.text = displayName;
            }
        }

        // 设置按钮点击事件和选中效果
        var button = item.GetComponent<Button>();
        if (button != null)
        {
            int id = biotankID;  // 捕获变量
            button.onClick.RemoveAllListeners();
            button.AddDeskTopListener(() =>
            {
                OnBiotankItemClicked(id);
            });

            // 设置选中效果
            var image = item.GetComponent<Image>();
            if (image != null)
            {
                bool isSelected = (biotankID == BioTankRegistry.Instance.ActiveBioTankID);
                image.color = isSelected ? selectedColor : normalColor;
            }
        }
    }

    /// <summary>
    /// 获取生态箱显示名称
    /// </summary>
    private string GetBiotankDisplayName(BioTankManager biotank)
    {
        if (biotank == null || biotank.stateData == null)
            return LocalizationManager.GetCurrentLanguageByID(157);

        // 优先使用存储的名称
        if (!string.IsNullOrEmpty(biotank.stateData.Name))
        {
            return biotank.stateData.Name;
        }

        // 兼容旧存档：从配置获取默认名称
        if (int.TryParse(biotank.stateData.Type, out int typeId))
        {
            var tankData = GameConfigDataBase.GetConfigData<TankData>(typeId, SysDefine.TankConfigData);
            if (tankData != null && !string.IsNullOrEmpty(tankData.name) && int.TryParse(tankData.name, out int nameId))
            {
                return LocalizationManager.GetCurrentLanguageByID(nameId);
            }
        }
        return biotank.stateData.Type;
    }

    /// <summary>
    /// 点击生态箱列表项
    /// </summary>
    private void OnBiotankItemClicked(int biotankID)
    {
        // 切换当前活动的生态箱
        PlayerManager.Instance.SetActionBioTankManager(biotankID);

        // 将切换后的生态箱客户端窗口置顶
        WindowManager.Instance.TopMostWindow();

        // 关闭列表界面（Hiding方法会处理窗口恢复）
        CloseOrReturnUIForms();
    }

    /// <summary>
    /// 清除所有生成的列表项
    /// </summary>
    private void ClearBiotankItems()
    {
        foreach (var item in biotankItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        biotankItems.Clear();
    }

    /// <summary>
    /// 重写Hiding方法，在界面隐藏时恢复窗口布局
    /// </summary>
    public override void Hiding()
    {
        base.Hiding();
        OnShow = false;

        // 通知窗口管理器，由它决定是否恢复窗口尺寸
        BiotankWindowManager.OnWindowClose();
    }
}
