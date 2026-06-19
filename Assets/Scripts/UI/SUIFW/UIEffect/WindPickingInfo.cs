using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Steamworks;
#endif

public class WindPickingInfo : MonoBehaviour
{
    [Header("野采信息ID")]
    public string windPickID;
    [Header("是否解锁")]
    public bool unLocked;
    [Header("正式购买后")]
    public bool check;
    public Transform checkg;
    [Header("名称")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI moneyText;
    public Transform IsUnlocked;
    [Header("未解锁原因文本")]
    public TextMeshProUGUI unlockReasonText;

    /// <summary>
    /// Demo版本中需要购买完整版才能解锁的卡池排序阈值（Sort >= 此值的卡池需要购买）
    /// </summary>
    private const int DEMO_LOCKED_SORT_THRESHOLD = 2;

    public Transform Selected;
    public Button CanSelect;
    public Image icon;
    private bool IsInit = false;
    private static int m_selectID = -1;

    /// <summary>
    /// 是否因为Demo限制而被锁定
    /// </summary>
    private bool isDemoLocked = false;
    private void Awake()
    {
        StartCoroutine(Init());

    }

    private IEnumerator Init()
    {
        // 等待 GameManager 和 Gather 初始化完成
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            GameManager.Instance.gather != null &&
            GameManager.Instance.gather.IsInited());

        // 等待本地化系统初始化完成（数据库加载且语言已设置）
        yield return new WaitUntil(() =>
            LocalizationManager.langDatabaseisInit &&
            LocalizationManager.GetCurrentLanguage() != "null");

        // 等待windPickID被设置（支持动态生成的物体）
        yield return new WaitUntil(() => !string.IsNullOrEmpty(windPickID));

        var gather = GameManager.Instance.gather;

        // 检查配置是否存在
        if (gather.gatherAddressData == null ||
            !gather.gatherAddressData.ContainsKey(windPickID))
        {
            yield break;
        }

        var admissionData = gather.gatherAddressData[windPickID];

        // 检查是否因为Demo限制而锁定（Sort >= 2的卡池需要购买完整版）
        isDemoLocked = CheckDemoLocked(admissionData.Sort);

        // 如果被Demo锁定，则视为未解锁
        if (isDemoLocked)
        {
            unLocked = false;
        }
        else
        {
            unLocked = gather.GetAdmissionIsUnlock(windPickID);
        }

        on_event_handler("", gather.GatherID);
        // 使用SetText注册，支持语言切换时自动更新
        LocalizationManager.SetText(nameText, int.Parse(admissionData.Name));
        // 价格文本：如果有价格显示价格，否则显示"免费"
        if (admissionData.Price != 0)
        {
            moneyText.text = admissionData.Price.ToString();
        }
        else
        {
            LocalizationManager.SetText(moneyText, SysDefine.Language_Free);
        }
        if (checkg != null)
            checkg.gameObject.SetActive(check);
        // 根据解锁状态设置黑色遮罩显示
        // check为true表示正式购买后，此时不显示遮罩
        // 否则根据unLocked状态：未解锁时显示遮罩，已解锁时隐藏遮罩
        if (check)
        {
            if (IsUnlocked != null)
                IsUnlocked.gameObject.SetActive(false);
        }
        else
        {
            // 确保未解锁时显示黑色遮罩
            if (IsUnlocked != null)
                IsUnlocked.gameObject.SetActive(!unLocked);
        }
        // 同时设置Selected状态，确保初始状态正确
        if (Selected != null)
            Selected.gameObject.SetActive(false);


        if (IsInit)
            yield break;
        event_manager.instance.add_event_listener(SysDefine.UI_WindPickingInfo_change, WindPickingInfo_change);
        IsInit = true;
        CanSelect.AddDeskTopListener(() =>
        {
            if(!unLocked)
            {
                // 如果是Demo锁定，显示提示并打开商店
                if (isDemoLocked)
                {
                    // 使用通用的未解锁提示，同时打开购买页面
                    ToastManager.Show(86);
                    OpenPurchasePage();
                }
                else
                {
                    ToastManager.Show(86);
                }
            }
            else
            {
                event_manager.instance.dispatch_event(SysDefine.UI_GatherSelect, windPickID);
            }
        });
        event_manager.instance.add_event_listener(SysDefine.UI_GatherSelect, on_event_handler);
    }
    public void WindPickingInfo_change(string name, object udata)
    {
        StartCoroutine(Init());
    }
    public  void on_event_handler(string name, object udata)
    {
        string id = (string)udata;
        if(unLocked&&id==windPickID)
        {
            if (Selected != null)
                Selected.gameObject.SetActive(true);
            m_selectID = int.Parse(id);
            Gather.Instance.SetAddress(id);
        }
        else
        {
            if (Selected != null)
                Selected.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 检查卡池是否因Demo限制而锁定
    /// </summary>
    /// <param name="sort">卡池的排序值</param>
    /// <returns>是否被Demo锁定</returns>
    private bool CheckDemoLocked(int sort)
    {
        // 如果排序值小于阈值，不需要购买完整版
        if (sort < DEMO_LOCKED_SORT_THRESHOLD)
        {
            return false;
        }

#if !DISABLESTEAMWORKS && STEAMWORKSNET
        // 检查是否拥有完整版
        if (SteamOwnershipManager.Instance != null)
        {
            // 如果拥有完整版，则不锁定
            return !SteamOwnershipManager.Instance.IsFullVersionOwned;
        }
#endif
        // 默认锁定（如果Steam未初始化）
        return true;
    }

    /// <summary>
    /// 打开Steam商店购买页面
    /// </summary>
    private void OpenPurchasePage()
    {
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        if (SteamOwnershipManager.Instance != null)
        {
            SteamOwnershipManager.Instance.OpenStorePage();
        }
#endif
    }
}
