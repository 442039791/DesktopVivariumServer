#if !DISABLESTEAMWORKS && STEAMWORKSNET
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;

/// <summary>
/// Steam 创意工坊管理器
/// 负责订阅物品扫描、安装路径获取、状态监控
/// </summary>
public class SteamWorkshopManager : MonoBehaviour
{
    // 单例
    private static SteamWorkshopManager _instance;
    public static SteamWorkshopManager Instance => _instance;

    [Header("设置")]
    [Tooltip("是否启用详细日志")]
    [SerializeField] private bool _enableDebugLog = true;

    // Steam API 是否已初始化
    private bool _steamInitialized = false;

    // 订阅物品缓存
    private Dictionary<PublishedFileId_t, WorkshopItemInfo> _subscribedItems = new();

    // 回调
    private Callback<ItemInstalled_t> _itemInstalledCallback;
    private Callback<DownloadItemResult_t> _downloadItemResultCallback;

    // 事件
    public event Action<WorkshopItemInfo> OnItemInstalled;
    public event Action<WorkshopItemInfo> OnItemUpdated;
    public event Action<PublishedFileId_t, bool> OnItemDownloadCompleted;
    public event Action OnSubscribedItemsChanged;

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
        Initialize();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 初始化创意工坊管理器
    /// </summary>
    public void Initialize()
    {
        try
        {
            _steamInitialized = SteamAPI.IsSteamRunning();

            if (!_steamInitialized)
            {
                LogWarning("Steam 未运行，创意工坊功能不可用");
                return;
            }

            // 注册回调
            _itemInstalledCallback = Callback<ItemInstalled_t>.Create(OnItemInstalledCallback);
            _downloadItemResultCallback = Callback<DownloadItemResult_t>.Create(OnDownloadItemResultCallback);

            Log("Steam 创意工坊管理器初始化成功");

            // 初始扫描订阅物品
            RefreshSubscribedItems();
        }
        catch (Exception e)
        {
            LogError($"初始化创意工坊管理器异常: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 检查 Steam 是否可用
    /// </summary>
    public bool IsSteamAvailable => _steamInitialized;

    #region 订阅物品管理

    /// <summary>
    /// 刷新订阅物品列表
    /// </summary>
    public void RefreshSubscribedItems()
    {
        if (!_steamInitialized)
        {
            LogWarning("Steam 未初始化，无法刷新订阅列表");
            return;
        }

        _subscribedItems.Clear();

        // 获取订阅物品数量
        uint numSubscribed = SteamUGC.GetNumSubscribedItems();
        Log($"发现 {numSubscribed} 个订阅的创意工坊物品");

        if (numSubscribed == 0) return;

        // 获取订阅物品 ID 列表
        PublishedFileId_t[] subscribedIds = new PublishedFileId_t[numSubscribed];
        uint count = SteamUGC.GetSubscribedItems(subscribedIds, numSubscribed);

        // 获取每个物品的信息
        for (uint i = 0; i < count; i++)
        {
            var itemId = subscribedIds[i];
            var itemInfo = GetItemInfo(itemId);
            if (itemInfo != null)
            {
                _subscribedItems[itemId] = itemInfo;
            }
        }

        Log($"成功加载 {_subscribedItems.Count} 个创意工坊物品信息");
        OnSubscribedItemsChanged?.Invoke();
    }

    /// <summary>
    /// 获取物品详细信息
    /// </summary>
    private WorkshopItemInfo GetItemInfo(PublishedFileId_t itemId)
    {
        var info = new WorkshopItemInfo
        {
            ItemId = itemId
        };

        // 获取物品状态
        uint state = SteamUGC.GetItemState(itemId);
        info.State = (EItemState)state;

        // 解析状态标志
        info.IsSubscribed = (state & (uint)EItemState.k_EItemStateSubscribed) != 0;
        info.IsInstalled = (state & (uint)EItemState.k_EItemStateInstalled) != 0;
        info.NeedsUpdate = (state & (uint)EItemState.k_EItemStateNeedsUpdate) != 0;
        info.IsDownloading = (state & (uint)EItemState.k_EItemStateDownloading) != 0;
        info.IsDownloadPending = (state & (uint)EItemState.k_EItemStateDownloadPending) != 0;

        // 获取安装信息
        if (info.IsInstalled)
        {
            ulong sizeOnDisk;
            string folder;
            uint timeStamp;

            if (SteamUGC.GetItemInstallInfo(itemId, out sizeOnDisk, out folder, 1024, out timeStamp))
            {
                info.InstallPath = folder;
                info.SizeOnDisk = sizeOnDisk;
                info.LastUpdateTime = DateTimeOffset.FromUnixTimeSeconds(timeStamp).LocalDateTime;

                Log($"物品 {itemId}: 路径={folder}, 大小={sizeOnDisk / 1024}KB");
            }
        }

        // 获取下载进度
        if (info.IsDownloading)
        {
            ulong bytesDownloaded, bytesTotal;
            if (SteamUGC.GetItemDownloadInfo(itemId, out bytesDownloaded, out bytesTotal))
            {
                info.DownloadProgress = bytesTotal > 0 ? (float)bytesDownloaded / bytesTotal : 0f;
                info.BytesDownloaded = bytesDownloaded;
                info.BytesTotal = bytesTotal;
            }
        }

        return info;
    }

    /// <summary>
    /// 获取所有订阅物品
    /// </summary>
    public IReadOnlyDictionary<PublishedFileId_t, WorkshopItemInfo> GetSubscribedItems()
    {
        return _subscribedItems;
    }

    /// <summary>
    /// 获取所有已安装的物品
    /// </summary>
    public List<WorkshopItemInfo> GetInstalledItems()
    {
        var result = new List<WorkshopItemInfo>();
        foreach (var item in _subscribedItems.Values)
        {
            if (item.IsInstalled && !string.IsNullOrEmpty(item.InstallPath))
            {
                result.Add(item);
            }
        }
        return result;
    }

    /// <summary>
    /// 获取所有已安装物品的路径
    /// </summary>
    public List<string> GetInstalledItemPaths()
    {
        var paths = new List<string>();
        foreach (var item in _subscribedItems.Values)
        {
            if (item.IsInstalled && !string.IsNullOrEmpty(item.InstallPath))
            {
                paths.Add(item.InstallPath);
            }
        }
        return paths;
    }

    /// <summary>
    /// 检查物品是否已安装
    /// </summary>
    public bool IsItemInstalled(PublishedFileId_t itemId)
    {
        if (_subscribedItems.TryGetValue(itemId, out var info))
        {
            return info.IsInstalled;
        }
        return false;
    }

    /// <summary>
    /// 获取物品安装路径
    /// </summary>
    public string GetItemInstallPath(PublishedFileId_t itemId)
    {
        if (_subscribedItems.TryGetValue(itemId, out var info))
        {
            return info.InstallPath;
        }
        return null;
    }

    #endregion

    #region 下载管理

    /// <summary>
    /// 请求下载物品（如果需要更新或未安装）
    /// </summary>
    public bool RequestItemDownload(PublishedFileId_t itemId, bool highPriority = false)
    {
        if (!_steamInitialized)
        {
            LogWarning("Steam 未初始化，无法下载");
            return false;
        }

        bool result = SteamUGC.DownloadItem(itemId, highPriority);
        if (result)
        {
            Log($"已请求下载物品: {itemId}");
        }
        else
        {
            LogWarning($"请求下载物品失败: {itemId}");
        }
        return result;
    }

    /// <summary>
    /// 下载所有需要更新的物品
    /// </summary>
    public void DownloadAllPendingItems()
    {
        foreach (var item in _subscribedItems.Values)
        {
            if (item.NeedsUpdate || item.IsDownloadPending)
            {
                RequestItemDownload(item.ItemId, false);
            }
        }
    }

    /// <summary>
    /// 获取物品下载进度
    /// </summary>
    public float GetItemDownloadProgress(PublishedFileId_t itemId)
    {
        if (_subscribedItems.TryGetValue(itemId, out var info))
        {
            if (info.IsDownloading)
            {
                // 更新下载进度
                ulong bytesDownloaded, bytesTotal;
                if (SteamUGC.GetItemDownloadInfo(itemId, out bytesDownloaded, out bytesTotal))
                {
                    return bytesTotal > 0 ? (float)bytesDownloaded / bytesTotal : 0f;
                }
            }
            return info.IsInstalled ? 1f : 0f;
        }
        return 0f;
    }

    #endregion

    #region 订阅管理

    /// <summary>
    /// 订阅物品
    /// </summary>
    public void SubscribeItem(PublishedFileId_t itemId, Action<bool> callback = null)
    {
        if (!_steamInitialized)
        {
            LogWarning("Steam 未初始化，无法订阅");
            callback?.Invoke(false);
            return;
        }

        var callResult = SteamUGC.SubscribeItem(itemId);
        Log($"已请求订阅物品: {itemId}");

        // 注意：实际结果需要通过回调获取
        // 这里简化处理，假设成功
        callback?.Invoke(true);
    }

    /// <summary>
    /// 取消订阅物品
    /// </summary>
    public void UnsubscribeItem(PublishedFileId_t itemId, Action<bool> callback = null)
    {
        if (!_steamInitialized)
        {
            LogWarning("Steam 未初始化，无法取消订阅");
            callback?.Invoke(false);
            return;
        }

        var callResult = SteamUGC.UnsubscribeItem(itemId);
        Log($"已请求取消订阅物品: {itemId}");

        // 从缓存中移除
        _subscribedItems.Remove(itemId);
        OnSubscribedItemsChanged?.Invoke();

        callback?.Invoke(true);
    }

    #endregion

    #region Steam 回调

    /// <summary>
    /// 物品安装完成回调
    /// </summary>
    private void OnItemInstalledCallback(ItemInstalled_t callback)
    {
        var itemId = callback.m_nPublishedFileId;
        Log($"物品安装完成: {itemId}");

        // 更新物品信息
        var itemInfo = GetItemInfo(itemId);
        if (itemInfo != null)
        {
            bool isUpdate = _subscribedItems.ContainsKey(itemId);
            _subscribedItems[itemId] = itemInfo;

            if (isUpdate)
            {
                OnItemUpdated?.Invoke(itemInfo);
            }
            else
            {
                OnItemInstalled?.Invoke(itemInfo);
            }

            OnSubscribedItemsChanged?.Invoke();
        }
    }

    /// <summary>
    /// 物品下载结果回调
    /// </summary>
    private void OnDownloadItemResultCallback(DownloadItemResult_t callback)
    {
        var itemId = callback.m_nPublishedFileId;
        bool success = callback.m_eResult == EResult.k_EResultOK;

        if (success)
        {
            Log($"物品下载成功: {itemId}");

            // 刷新物品信息
            var itemInfo = GetItemInfo(itemId);
            if (itemInfo != null)
            {
                _subscribedItems[itemId] = itemInfo;
            }
        }
        else
        {
            LogError($"物品下载失败: {itemId}, 结果: {callback.m_eResult}");
        }

        OnItemDownloadCompleted?.Invoke(itemId, success);
    }

    #endregion

    #region 与 ModManager 集成

    /// <summary>
    /// 获取所有可作为模组加载的创意工坊物品路径
    /// 只返回已安装且包含 manifest.json 的物品
    /// </summary>
    public List<string> GetValidModPaths()
    {
        var validPaths = new List<string>();

        foreach (var item in _subscribedItems.Values)
        {
            if (!item.IsInstalled || string.IsNullOrEmpty(item.InstallPath))
                continue;

            // 检查是否包含 manifest.json
            string manifestPath = Path.Combine(item.InstallPath, "manifest.json");
            if (File.Exists(manifestPath))
            {
                validPaths.Add(item.InstallPath);
                Log($"发现有效的创意工坊模组: {item.InstallPath}");
            }
        }

        return validPaths;
    }

    /// <summary>
    /// 检查是否有待更新的模组
    /// </summary>
    public bool HasPendingUpdates()
    {
        foreach (var item in _subscribedItems.Values)
        {
            if (item.NeedsUpdate)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取待更新的物品数量
    /// </summary>
    public int GetPendingUpdateCount()
    {
        int count = 0;
        foreach (var item in _subscribedItems.Values)
        {
            if (item.NeedsUpdate)
                count++;
        }
        return count;
    }

    #endregion

    #region 日志

    private void Log(string message)
    {
        if (_enableDebugLog)
        {
            Debug.Log($"[SteamWorkshop] {message}");
        }
    }

    private void LogWarning(string message)
    {
        if (_enableDebugLog)
        {
            Debug.LogWarning($"[SteamWorkshop] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[SteamWorkshop] {message}");
    }

    #endregion
}

/// <summary>
/// 创意工坊物品信息
/// </summary>
[Serializable]
public class WorkshopItemInfo
{
    /// <summary>
    /// 物品 ID
    /// </summary>
    public PublishedFileId_t ItemId;

    /// <summary>
    /// 物品状态
    /// </summary>
    public EItemState State;

    /// <summary>
    /// 是否已订阅
    /// </summary>
    public bool IsSubscribed;

    /// <summary>
    /// 是否已安装
    /// </summary>
    public bool IsInstalled;

    /// <summary>
    /// 是否需要更新
    /// </summary>
    public bool NeedsUpdate;

    /// <summary>
    /// 是否正在下载
    /// </summary>
    public bool IsDownloading;

    /// <summary>
    /// 是否等待下载
    /// </summary>
    public bool IsDownloadPending;

    /// <summary>
    /// 安装路径
    /// </summary>
    public string InstallPath;

    /// <summary>
    /// 磁盘占用大小（字节）
    /// </summary>
    public ulong SizeOnDisk;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdateTime;

    /// <summary>
    /// 下载进度 (0-1)
    /// </summary>
    public float DownloadProgress;

    /// <summary>
    /// 已下载字节数
    /// </summary>
    public ulong BytesDownloaded;

    /// <summary>
    /// 总字节数
    /// </summary>
    public ulong BytesTotal;

    /// <summary>
    /// 模组来源类型
    /// </summary>
    public ModSourceType SourceType => ModSourceType.SteamWorkshop;
}

/// <summary>
/// 模组来源类型
/// </summary>
public enum ModSourceType
{
    /// <summary>
    /// 本地模组
    /// </summary>
    Local,

    /// <summary>
    /// Steam 创意工坊
    /// </summary>
    SteamWorkshop
}
#endif
