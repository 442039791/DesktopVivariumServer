using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SUIFW;

public class UI_Main_Settings_Information : UIBase
{
    public Button Save;
    public Button DeleteSave;
    public Button SaveAndExit;
    [Header("本地化相关")]
    public Button _LanguageBnt;
    [Header("自身窗口尺寸")]
    public Button Self_adjustSize_Increase_Btn;
    public Button Self_adjustSize_Decrease_Btn;
    public Button Self_adjustSize_Reset_Btn;
    [Header("置于桌面上或下")]
    public Button Self_adjustPosition_Up_Btn;
    public Button Self_adjustPosition_Down_Btn;
    [Header("切换生态箱")]
    public Button SwitchBioTankBtn;
    [Header("测试功能")]
    public Button AddMoneyBtn;
    [Header("GM工具")]
    public Button UnlockAllBtn;

    [Header("模组管理")]
    public Button ModManagerBtn;

    private float _Selfvalue = 1.0f;
    private float maxValue = 1.5f;
    private float minValue = 0.5f;
    private float changeValue = 0.25f;



    [Header("Version")]
    public TextMeshProUGUI VersionText;
    private float Selfvalue
    {
        get => _Selfvalue;
        set
        {
            if (value >= maxValue)
                value = maxValue;
            if (value <= minValue)
                value = minValue;
            if (value == _Selfvalue)
                return;
            _Selfvalue = value;

            WallpaperManager.Instance.SetDisplayScale(_Selfvalue);
            WallpaperManager.Instance.SetWindowSize();
        }
    }
    public override bool Init()
    {
        VersionText.text = "Version: " + Application.version;
        Self_adjustPosition_Up_Btn.AddDeskTopListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendSetWindowUporDownCommand(true);
        });
        Self_adjustPosition_Down_Btn.AddDeskTopListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendSetWindowUporDownCommand(false);
        });
        Save.AddDeskTopListener(() =>
        {
            GameManager.Instance.Save();
            
        });
        DeleteSave.AddDeskTopListener(() =>
        {
            GameManager.Instance.DoDeleteCommand();
            
        });
        SaveAndExit.AddDeskTopListener(() =>
        {
            GameManager.Instance.Save();

            GameManager.Instance.Quit();
        });
        _LanguageBnt.AddDeskTopListener (() =>
        {
            SetLanguage();
        });
        Self_adjustSize_Decrease_Btn.AddDeskTopListener(() =>
        {
            Selfvalue -= changeValue;
        });
        Self_adjustSize_Increase_Btn.AddDeskTopListener(() =>
        {
            Selfvalue += changeValue;
        });
        Self_adjustSize_Reset_Btn.AddDeskTopListener(() =>
        {
            Selfvalue = 1.0f;
        });

        // 切换生态箱按钮
        if (SwitchBioTankBtn != null)
        {
            SwitchBioTankBtn.AddDeskTopListener(() =>
            {
                SwitchToNextBioTank();
            });
        }

        // 添加金币按钮
        if (AddMoneyBtn != null)
        {
            AddMoneyBtn.AddDeskTopListener(() =>
            {
                AddPlayerMoney();
            });
        }

        // GM工具 - 解锁所有Cube、装饰物和生态箱
        if (UnlockAllBtn != null)
        {
            UnlockAllBtn.AddDeskTopListener(() =>
            {
                UnlockAllItems();
            });
        }

        // 模组管理按钮
        if (ModManagerBtn != null)
        {
            ModManagerBtn.AddDeskTopListener(() =>
            {
                OpenModManager();
            });
        }

        return base.Init();
    }

    /// <summary>
    /// 切换到下一个生态箱
    /// 循环切换，到末尾后回到第一个
    /// 注意：切换时不改变窗口层级
    /// </summary>
    private void SwitchToNextBioTank()
    {
        var allBioTanks = BioTankRegistry.Instance.GetAllBioTanks();

        // 如果没有生态箱或只有一个，无需切换
        if (allBioTanks.Count <= 1)
        {
            Debug.Log("[UI_Main_Settings_Information] 只有一个或没有生态箱，无需切换");
            return;
        }

        // 获取当前活动生态箱ID
        int currentID = BioTankRegistry.Instance.ActiveBioTankID;

        // 找到当前生态箱在列表中的索引
        int currentIndex = -1;
        for (int i = 0; i < allBioTanks.Count; i++)
        {
            if (allBioTanks[i].stateData.ID == currentID)
            {
                currentIndex = i;
                break;
            }
        }

        // 计算下一个生态箱的索引（循环）
        int nextIndex = (currentIndex + 1) % allBioTanks.Count;
        int nextBioTankID = allBioTanks[nextIndex].stateData.ID;

        // 切换生态箱（不会改变窗口层级）
        BioTankRegistry.Instance.SetActiveBioTankById(nextBioTankID);

        Debug.Log($"[UI_Main_Settings_Information] 切换生态箱: {currentID} -> {nextBioTankID}");
    }

    /// <summary>
    /// 修改选择的语言
    /// 点击按钮后切换到列表中的下一个语言，如果到末尾则从头开始
    /// 刷新所有界面文本并保存当前语言选择
    /// </summary>
    void SetLanguage()
    {
        // 每次都从 langDatabase 获取最新的语言列表（包含模组添加的语言）
        var languageList = new List<string>(LocalizationManager.langDatabase.Keys);

        // 如果没有可用语言，直接返回
        if (languageList.Count == 0)
            return;

        string currentLang = LocalizationManager.GetCurrentLanguage();
        int currentIndex = languageList.IndexOf(currentLang);

        // 计算下一个语言的索引（循环）
        int nextIndex = (currentIndex + 1) % languageList.Count;
        string nextLang = languageList[nextIndex];

        // 使用 SetLanguageAndSave 切换语言并保存
        // 该方法会自动刷新所有已注册的文本组件
        LocalizationManager.SetLanguageAndSave(nextLang);
    }

    /// <summary>
    /// 添加金币
    /// 给玩家增加1000万金币
    /// </summary>
    private void AddPlayerMoney()
    {
        int addAmount = 10000000; // 1000万
        bool success = PlayerStateManager.Instance.ChangeMoney(addAmount);

        if (success)
        {
            Debug.Log($"[UI_Main_Settings_Information] 成功添加金币: {addAmount}");
            ToastManager.Instance.ShowToast($"获得 {addAmount} 金币!");
        }
        else
        {
            Debug.LogWarning("[UI_Main_Settings_Information] 添加金币失败");
        }
    }

    /// <summary>
    /// GM工具 - 解锁所有Cube、装饰物和生态箱
    /// </summary>
    private void UnlockAllItems()
    {
        Debug.Log("========== GM工具: 开始解锁所有物品 ==========");

        int cubeUnlocked = 0;
        int decorationUnlocked = 0;
        int tankUnlocked = 0;

        // 解锁所有Cube
        if (Gather.Instance != null && Gather.Instance.CubeUnlockDic != null)
        {
            var keys = new List<string>(Gather.Instance.CubeUnlockDic.Keys);
            foreach (var key in keys)
            {
                if (!Gather.Instance.CubeUnlockDic[key])
                {
                    Gather.Instance.CubeUnlockDic[key] = true;
                    cubeUnlocked++;
                }
            }
            Debug.Log($"[GM工具] 解锁Cube数量: {cubeUnlocked}");
        }

        // 解锁所有装饰物
        if (Gather.Instance != null && Gather.Instance.DecorationUnlockDic != null)
        {
            var keys = new List<string>(Gather.Instance.DecorationUnlockDic.Keys);
            foreach (var key in keys)
            {
                if (!Gather.Instance.DecorationUnlockDic[key])
                {
                    Gather.Instance.DecorationUnlockDic[key] = true;
                    decorationUnlocked++;
                }
            }
            Debug.Log($"[GM工具] 解锁装饰物数量: {decorationUnlocked}");
        }

        // 解锁所有生态箱
        if (Gather.Instance != null && Gather.Instance.TankUnlockDic != null)
        {
            var keys = new List<string>(Gather.Instance.TankUnlockDic.Keys);
            foreach (var key in keys)
            {
                if (!Gather.Instance.TankUnlockDic[key])
                {
                    Gather.Instance.TankUnlockDic[key] = true;
                    tankUnlocked++;
                }
            }
            Debug.Log($"[GM工具] 解锁生态箱数量: {tankUnlocked}");
        }

        // 同步到图鉴系统
        if (IlluGuideMgr.Instance != null)
        {
            // 同步Cube解锁状态到图鉴
            foreach (var entry in IlluGuideMgr.Instance.cubeEntries.Values)
            {
                if (!entry.isUnlocked)
                {
                    entry.isUnlocked = true;
                    entry.collectCount++;
                }
            }

            // 同步装饰物解锁状态到图鉴
            foreach (var entry in IlluGuideMgr.Instance.decorationEntries.Values)
            {
                if (!entry.isUnlocked)
                {
                    entry.isUnlocked = true;
                    entry.collectCount++;
                }
            }
        }

        // 同步到UnlockableItemManager并更新仓库
        if (UnlockableItemManager.Instance != null)
        {
            UnlockableItemManager.Instance.UnlockAllCubes();
            UnlockableItemManager.Instance.UnlockAllDecorations();
        }

        int totalUnlocked = cubeUnlocked + decorationUnlocked + tankUnlocked;
        Debug.Log($"========== GM工具: 解锁完成，共解锁 {totalUnlocked} 个物品 ==========");

        ToastManager.Instance.ShowToast($"解锁完成! Cube:{cubeUnlocked} 装饰物:{decorationUnlocked} 生态箱:{tankUnlocked}");
    }

    /// <summary>
    /// 打开模组管理界面
    /// </summary>
    private void OpenModManager()
    {
        Debug.Log("[UI_Main_Settings_Information] 打开模组管理界面");
        UIManager.Instance.ShowUIForms(SysDefine.UI_ModManager);
    }
}
