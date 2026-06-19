#if !DISABLESTEAMWORKS && STEAMWORKSNET
using UnityEngine;
using System;
using Steamworks;

/// <summary>
/// Steam所有权管理器
/// 用于检测用户是否拥有完整版游戏（用于Demo解锁功能）
/// </summary>
public class SteamOwnershipManager : MonoBehaviour
{
    [Header("App ID 配置")]
    [Tooltip("完整版游戏的Steam App ID（在Steamworks后台获取）")]
    [SerializeField] private uint _fullGameAppId = 0;

    [Header("设置")]
    [Tooltip("是否启用详细日志")]
    [SerializeField] private bool _enableDebugLog = true;

    // 单例
    private static SteamOwnershipManager _instance;
    public static SteamOwnershipManager Instance => _instance;

    // 所有权状态
    private bool _isFullVersionOwned = false;
    private bool _isOwnershipChecked = false;

    // 事件：当所有权状态改变时触发
    public event Action<bool> OnOwnershipChanged;

    /// <summary>
    /// 是否拥有完整版游戏
    /// </summary>
    public bool IsFullVersionOwned => _isFullVersionOwned;

    /// <summary>
    /// 是否已完成所有权检测
    /// </summary>
    public bool IsOwnershipChecked => _isOwnershipChecked;

    /// <summary>
    /// 完整版游戏的App ID
    /// </summary>
    public uint FullGameAppId => _fullGameAppId;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        CheckOwnership();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 检测完整版游戏所有权
    /// </summary>
    public void CheckOwnership()
    {
        if (_fullGameAppId == 0)
        {
            LogWarning("完整版游戏App ID未配置，跳过所有权检测");
            _isOwnershipChecked = true;
            _isFullVersionOwned = false;
            return;
        }

        try
        {
            if (!SteamAPI.IsSteamRunning())
            {
                LogWarning("Steam未运行，无法检测所有权");
                _isOwnershipChecked = true;
                _isFullVersionOwned = false;
                return;
            }

            // 使用 BIsSubscribedApp 检测用户是否拥有完整版游戏
            // 注意：使用 BIsSubscribedApp 而不是 BIsSubscribed，因为后者会受 steam_appid.txt 影响
            _isFullVersionOwned = SteamApps.BIsSubscribedApp((AppId_t)_fullGameAppId);
            _isOwnershipChecked = true;

            Log($"所有权检测完成 - 完整版AppId: {_fullGameAppId}, 拥有完整版: {_isFullVersionOwned}");

            // 触发事件
            OnOwnershipChanged?.Invoke(_isFullVersionOwned);
        }
        catch (Exception e)
        {
            LogError($"检测所有权异常: {e.Message}\n{e.StackTrace}");
            _isOwnershipChecked = true;
            _isFullVersionOwned = false;
        }
    }

    /// <summary>
    /// 检测用户是否拥有指定的App（可用于检测DLC等）
    /// </summary>
    /// <param name="appId">要检测的App ID</param>
    /// <returns>是否拥有该App</returns>
    public bool IsAppOwned(uint appId)
    {
        try
        {
            if (!SteamAPI.IsSteamRunning())
            {
                return false;
            }

            return SteamApps.BIsSubscribedApp((AppId_t)appId);
        }
        catch (Exception e)
        {
            LogError($"检测App所有权异常 [AppId: {appId}]: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 检测DLC是否已安装
    /// </summary>
    /// <param name="dlcAppId">DLC的App ID</param>
    /// <returns>DLC是否已拥有且安装</returns>
    public bool IsDlcInstalled(uint dlcAppId)
    {
        try
        {
            if (!SteamAPI.IsSteamRunning())
            {
                return false;
            }

            return SteamApps.BIsDlcInstalled((AppId_t)dlcAppId);
        }
        catch (Exception e)
        {
            LogError($"检测DLC安装状态异常 [DlcAppId: {dlcAppId}]: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 打开Steam商店页面购买完整版
    /// </summary>
    public void OpenStorePage()
    {
        if (_fullGameAppId == 0)
        {
            LogWarning("完整版游戏App ID未配置，无法打开商店页面");
            return;
        }

        OpenStorePage(_fullGameAppId);
    }

    /// <summary>
    /// 打开指定App的Steam商店页面
    /// </summary>
    /// <param name="appId">App ID</param>
    public void OpenStorePage(uint appId)
    {
        try
        {
            if (!SteamAPI.IsSteamRunning())
            {
                LogWarning("Steam未运行，无法打开商店页面");
                return;
            }

            SteamFriends.ActivateGameOverlayToStore(
                (AppId_t)appId,
                EOverlayToStoreFlag.k_EOverlayToStoreFlag_None
            );

            Log($"已打开Steam商店页面 [AppId: {appId}]");
        }
        catch (Exception e)
        {
            LogError($"打开商店页面异常 [AppId: {appId}]: {e.Message}");
        }
    }

    /// <summary>
    /// 设置完整版游戏App ID（运行时配置）
    /// </summary>
    /// <param name="appId">完整版游戏的App ID</param>
    public void SetFullGameAppId(uint appId)
    {
        _fullGameAppId = appId;
        Log($"完整版游戏App ID已设置为: {appId}");

        // 重新检测所有权
        _isOwnershipChecked = false;
        CheckOwnership();
    }

    /// <summary>
    /// 获取当前运行的App ID
    /// </summary>
    /// <returns>当前App ID</returns>
    public uint GetCurrentAppId()
    {
        try
        {
            if (!SteamAPI.IsSteamRunning())
            {
                return 0;
            }

            return SteamUtils.GetAppID().m_AppId;
        }
        catch (Exception e)
        {
            LogError($"获取当前App ID异常: {e.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 检查功能是否可用（根据所有权状态）
    /// </summary>
    /// <param name="featureName">功能名称（用于日志）</param>
    /// <param name="showPurchasePrompt">如果功能不可用，是否显示购买提示</param>
    /// <returns>功能是否可用</returns>
    public bool CheckFeatureAccess(string featureName, bool showPurchasePrompt = true)
    {
        if (_isFullVersionOwned)
        {
            return true;
        }

        Log($"功能 [{featureName}] 需要购买完整版才能使用");

        if (showPurchasePrompt)
        {
            OpenStorePage();
        }

        return false;
    }

    #region 日志方法

    private void Log(string message)
    {
        if (_enableDebugLog)
        {
            Debug.Log($"[SteamOwnership] {message}");
        }
    }

    private void LogWarning(string message)
    {
        if (_enableDebugLog)
        {
            Debug.LogWarning($"[SteamOwnership] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[SteamOwnership] {message}");
    }

    #endregion
}
#endif
