using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Common;

/// <summary>
/// 预加载客户端池管理器
/// 管理休眠状态的客户端，实现快速启动新生态箱
/// </summary>
public class PreloadClientPoolManager : MonoSingleton<PreloadClientPoolManager>
{
    #region 配置

    /// <summary>
    /// 是否启用预加载客户端功能（始终为true）
    /// </summary>
    private bool _enablePreloadPool = true;

    /// <summary>
    /// 池中保持的休眠客户端数量
    /// </summary>
    [Header("池配置")]
    [SerializeField]
    [Tooltip("池中保持的休眠客户端数量")]
    private int _poolSize = 1;

    /// <summary>
    /// 启动新休眠客户端的间隔时间
    /// </summary>
    [SerializeField]
    [Tooltip("启动新休眠客户端的间隔时间（秒）")]
    private float _launchInterval = 0.5f;

    /// <summary>
    /// 客户端连接超时时间（秒）
    /// </summary>
    [SerializeField]
    [Tooltip("客户端连接超时时间（秒）")]
    private float _connectionTimeout = 30f;

    #endregion

    #region 状态

    /// <summary>
    /// 休眠客户端队列（等待被分配的客户端ID）
    /// </summary>
    private Queue<int> _dormantClientQueue = new Queue<int>();

    /// <summary>
    /// 正在等待连接的休眠客户端数量
    /// </summary>
    private int _pendingDormantClients = 0;

    /// <summary>
    /// 下一个休眠客户端的临时ID（使用负数避免与正常ID冲突）
    /// </summary>
    private int _nextDormantId = -1000;

    /// <summary>
    /// 休眠客户端ID到ControlBehavior的映射
    /// </summary>
    private ConcurrentDictionary<int, ControlBehavior> _dormantClients = new ConcurrentDictionary<int, ControlBehavior>();

    /// <summary>
    /// 是否已初始化
    /// </summary>
    private bool _isInitialized = false;

    /// <summary>
    /// 池维护协程
    /// </summary>
    private Coroutine _maintainPoolCoroutine;

    /// <summary>
    /// 当前预览客户端（如果有）
    /// </summary>
    private ControlBehavior _previewClient = null;

    /// <summary>
    /// 预览客户端的原始休眠ID
    /// </summary>
    private int _previewClientOriginalId = 0;

    /// <summary>
    /// 是否处于预览模式
    /// </summary>
    private bool _isInPreviewMode = false;

    #endregion

    #region 公共属性

    /// <summary>
    /// 是否启用预加载客户端功能
    /// </summary>
    public bool IsEnabled => _enablePreloadPool;

    /// <summary>
    /// 当前池中可用的休眠客户端数量
    /// </summary>
    public int AvailableDormantClients => _enablePreloadPool ? _dormantClients.Count : 0;

    /// <summary>
    /// 池是否有可用的休眠客户端
    /// </summary>
    public bool HasAvailableDormantClient => _enablePreloadPool && _dormantClients.Count > 0;

    /// <summary>
    /// 是否处于预览模式
    /// </summary>
    public bool IsInPreviewMode => _isInPreviewMode;

    /// <summary>
    /// 当前预览客户端
    /// </summary>
    public ControlBehavior PreviewClient => _previewClient;

    #endregion

    #region 初始化

    /// <summary>
    /// 初始化预加载池
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 已经初始化过了");
            return;
        }

        _isInitialized = true;

        if (!_enablePreloadPool)
        {
            Debug.Log("[PreloadClientPoolManager] 预加载客户端池功能已禁用，跳过初始化");
            return;
        }

        Debug.Log($"[PreloadClientPoolManager] 初始化预加载客户端池，目标池大小: {_poolSize}");

        // 启动补充休眠客户端的协程
        _maintainPoolCoroutine = StartCoroutine(MaintainPoolCoroutine());
    }

    /// <summary>
    /// 启动初始的休眠客户端
    /// </summary>
    public IEnumerator LaunchInitialDormantClients()
    {
        if (!_enablePreloadPool)
        {
            Debug.Log("[PreloadClientPoolManager] 预加载池已禁用，跳过启动休眠客户端");
            yield break;
        }

        Debug.Log($"[PreloadClientPoolManager] 开始启动 {_poolSize} 个休眠客户端");

        for (int i = 0; i < _poolSize; i++)
        {
            LaunchDormantClient();
            yield return new WaitForSeconds(_launchInterval);
        }

        Debug.Log("[PreloadClientPoolManager] 初始休眠客户端启动完成");
    }

    #endregion

    #region 休眠客户端管理

    /// <summary>
    /// 启动一个新的休眠客户端
    /// 服务端会在客户端连接后分配休眠ID
    /// </summary>
    private void LaunchDormantClient()
    {
        if (!_enablePreloadPool)
        {
            return;
        }

        _pendingDormantClients++;

        Debug.Log($"[PreloadClientPoolManager] 启动休眠客户端进程");

        // 启动进程，服务端会在连接后分配ID
        HiddenProcessLauncher.Instance.LaunchHiddenProcess();
    }

    /// <summary>
    /// 注册一个休眠客户端（客户端连接时调用）
    /// </summary>
    /// <param name="client">WebSocket客户端行为</param>
    /// <param name="clientId">客户端ID（负数表示休眠客户端）</param>
    public void RegisterDormantClient(ControlBehavior client, int clientId)
    {
        if (!_enablePreloadPool)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 预加载池已禁用，拒绝注册休眠客户端");
            // 直接关闭这个客户端
            client?.SendCloseCommand();
            return;
        }

        if (client == null)
        {
            Debug.LogError("[PreloadClientPoolManager] 尝试注册null客户端");
            return;
        }

        _dormantClients[clientId] = client;
        _pendingDormantClients = Mathf.Max(0, _pendingDormantClients - 1);

        Debug.Log($"[PreloadClientPoolManager] 休眠客户端已注册，ID: {clientId}，当前池大小: {_dormantClients.Count}");
    }

    /// <summary>
    /// 从池中获取一个休眠客户端并绑定到指定的生态箱
    /// </summary>
    /// <param name="bioTankId">目标生态箱ID</param>
    /// <returns>是否成功获取并绑定</returns>
    public bool TryActivateDormantClient(int bioTankId)
    {
        if (!_enablePreloadPool)
        {
            return false;
        }

        if (_dormantClients.Count == 0)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 池中没有可用的休眠客户端");
            return false;
        }

        // 获取第一个休眠客户端
        foreach (var kvp in _dormantClients)
        {
            if (_dormantClients.TryRemove(kvp.Key, out ControlBehavior client))
            {
                Debug.Log($"[PreloadClientPoolManager] 激活休眠客户端，原ID: {kvp.Key}，新ID: {bioTankId}");

                // 发送绑定命令
                client.SendBindBioTankCommand(bioTankId, WallpaperManager.Instance.GetWorkerWPtr());

                // 将客户端重新注册到正常的客户端列表
                client.SetClientID(bioTankId);
                ControlBehavior.clients[bioTankId] = client;

                // 将新客户端窗口置于最顶层
                client.SetWindowZOrderCommand(WindowZOrder.Top);
                Debug.Log($"[PreloadClientPoolManager] 已将客户端 {bioTankId} 窗口置顶");

                // 触发补充休眠客户端
                StartCoroutine(ReplenishPoolDelayed());

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检查是否应该使用休眠客户端（而不是启动新进程）
    /// </summary>
    public bool ShouldUseDormantClient()
    {
        return _enablePreloadPool && _dormantClients.Count > 0;
    }

    #endregion

    #region 池维护

    /// <summary>
    /// 维护池大小的协程
    /// </summary>
    private IEnumerator MaintainPoolCoroutine()
    {
        while (_enablePreloadPool)
        {
            yield return new WaitForSeconds(5f);

            if (!_enablePreloadPool)
            {
                break;
            }

            // 如果处于预览模式，跳过池补充（预览客户端还在使用中，等预览结束后再补充）
            if (_isInPreviewMode)
            {
                Debug.Log("[PreloadClientPoolManager] 处于预览模式，跳过池补充");
                continue;
            }

            // 检查是否需要补充休眠客户端
            int currentTotal = _dormantClients.Count + _pendingDormantClients;
            if (currentTotal < _poolSize)
            {
                int needed = _poolSize - currentTotal;
                Debug.Log($"[PreloadClientPoolManager] 池需要补充 {needed} 个休眠客户端");

                for (int i = 0; i < needed; i++)
                {
                    if (!_enablePreloadPool) break;
                    // 再次检查预览模式，避免在补充过程中进入预览模式
                    if (_isInPreviewMode) break;
                    LaunchDormantClient();
                    yield return new WaitForSeconds(_launchInterval);
                }
            }
        }
    }

    /// <summary>
    /// 延迟补充池
    /// </summary>
    private IEnumerator ReplenishPoolDelayed()
    {
        yield return new WaitForSeconds(1f);

        if (!_enablePreloadPool)
        {
            yield break;
        }

        // 如果处于预览模式，跳过补充
        if (_isInPreviewMode)
        {
            Debug.Log("[PreloadClientPoolManager] 处于预览模式，跳过延迟补充");
            yield break;
        }

        int currentTotal = _dormantClients.Count + _pendingDormantClients;
        if (currentTotal < _poolSize)
        {
            Debug.Log("[PreloadClientPoolManager] 补充休眠客户端");
            LaunchDormantClient();
        }
    }

    #endregion

    #region 预览模式管理

    /// <summary>
    /// 进入预览模式
    /// 将一个休眠客户端转换为预览客户端
    /// </summary>
    /// <param name="sizeX">生态箱尺寸X</param>
    /// <param name="sizeY">生态箱尺寸Y</param>
    /// <param name="sizeZ">生态箱尺寸Z</param>
    /// <param name="layoutId">布局ID（可为null）</param>
    /// <returns>是否成功进入预览模式</returns>
    public bool EnterPreviewMode(int sizeX, int sizeY, int sizeZ, string layoutId = null)
    {
        if (_isInPreviewMode)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 已经处于预览模式");
            return false;
        }

        if (!_enablePreloadPool)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 预加载池未启用，无法进入预览模式");
            return false;
        }

        if (_dormantClients.Count == 0)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 没有可用的休眠客户端，无法进入预览模式");
            return false;
        }

        // 获取第一个休眠客户端
        foreach (var kvp in _dormantClients)
        {
            if (_dormantClients.TryRemove(kvp.Key, out ControlBehavior client))
            {
                _previewClient = client;
                _previewClientOriginalId = kvp.Key;
                _isInPreviewMode = true;

                Debug.Log($"[PreloadClientPoolManager] 进入预览模式，原休眠ID: {kvp.Key}，尺寸: {sizeX}x{sizeY}x{sizeZ}");

                // 获取布局数据和相机参数
                string terrainData = null;
                LayoutCameraData cameraData = null;
                if (!string.IsNullOrEmpty(layoutId))
                {
                    var layout = TankInitialLayoutManager.Instance.GetLayoutById(layoutId);
                    // 预览时使用处理后的terrainData（植物显示为最终阶段）
                    terrainData = TankInitialLayoutManager.Instance.GetPreviewTerrainData(layoutId);
                    cameraData = layout?.defaultCameraPos;
                }

                // 发送进入预览模式命令（包含相机参数）
                _previewClient.SendEnterPreviewModeCommand(sizeX, sizeY, sizeZ, layoutId, terrainData, WallpaperManager.Instance.GetWorkerWPtr(), cameraData);

                // 将预览窗口置于最顶层
                _previewClient.SetWindowZOrderCommand(WindowZOrder.Topmost);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 更新预览布局
    /// 当玩家切换尺寸或布局时调用
    /// </summary>
    /// <param name="sizeX">新的生态箱尺寸X</param>
    /// <param name="sizeY">新的生态箱尺寸Y</param>
    /// <param name="sizeZ">新的生态箱尺寸Z</param>
    /// <param name="layoutId">新的布局ID（可为null）</param>
    public void UpdatePreviewLayout(int sizeX, int sizeY, int sizeZ, string layoutId = null)
    {
        if (!_isInPreviewMode || _previewClient == null)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 不在预览模式，无法更新预览布局");
            return;
        }

        // 获取布局数据和相机参数
        string terrainData = null;
        LayoutCameraData cameraData = null;
        if (!string.IsNullOrEmpty(layoutId))
        {
            var layout = TankInitialLayoutManager.Instance.GetLayoutById(layoutId);
            // 预览时使用处理后的terrainData（植物显示为最终阶段）
            terrainData = TankInitialLayoutManager.Instance.GetPreviewTerrainData(layoutId);
            cameraData = layout?.defaultCameraPos;
            Debug.Log($"[PreloadClientPoolManager] 获取到布局 '{layoutId}'，terrainData长度: {terrainData?.Length ?? 0}，terrainData是否为空: {string.IsNullOrEmpty(terrainData)}");
            if (!string.IsNullOrEmpty(terrainData) && terrainData.Length > 100)
            {
                Debug.Log($"[PreloadClientPoolManager] terrainData前100字符: {terrainData.Substring(0, 100)}...");
            }
        }
        else
        {
            Debug.Log($"[PreloadClientPoolManager] layoutId为空，不获取布局数据");
        }

        Debug.Log($"[PreloadClientPoolManager] 更新预览布局，尺寸: {sizeX}x{sizeY}x{sizeZ}，布局: {layoutId ?? "空"}，terrainData长度: {terrainData?.Length ?? 0}");

        // 发送更新预览布局命令（包含相机参数）
        _previewClient.SendUpdatePreviewLayoutCommand(sizeX, sizeY, sizeZ, layoutId, terrainData, cameraData);
    }

    /// <summary>
    /// 退出预览模式（不创建生态箱）
    /// 预览客户端回到休眠状态
    /// </summary>
    public void ExitPreviewMode()
    {
        if (!_isInPreviewMode || _previewClient == null)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 不在预览模式，无法退出");
            return;
        }

        Debug.Log($"[PreloadClientPoolManager] 退出预览模式，预览客户端回到休眠状态");

        // 发送退出预览模式命令（不确认创建）
        _previewClient.SendExitPreviewModeCommand(false, 0);

        // 将客户端重新放回休眠池
        _dormantClients[_previewClientOriginalId] = _previewClient;

        // 清理预览状态
        _previewClient = null;
        _previewClientOriginalId = 0;
        _isInPreviewMode = false;
    }

    /// <summary>
    /// 确认创建生态箱
    /// 预览客户端变成正常客户端
    /// </summary>
    /// <param name="newBioTankId">新生态箱的ID</param>
    /// <returns>激活的客户端</returns>
    public ControlBehavior ConfirmPreviewCreation(int newBioTankId)
    {
        if (!_isInPreviewMode || _previewClient == null)
        {
            Debug.LogWarning("[PreloadClientPoolManager] 不在预览模式，无法确认创建");
            return null;
        }

        Debug.Log($"[PreloadClientPoolManager] 确认预览创建，新生态箱ID: {newBioTankId}");

        // 发送退出预览模式命令（确认创建）
        _previewClient.SendExitPreviewModeCommand(true, newBioTankId);

        // 将客户端重新注册到正常的客户端列表
        _previewClient.SetClientID(newBioTankId);
        ControlBehavior.clients[newBioTankId] = _previewClient;

        // 将窗口保持在顶层（但不是Topmost）
        _previewClient.SetWindowZOrderCommand(WindowZOrder.Top);

        var activatedClient = _previewClient;

        // 清理预览状态
        _previewClient = null;
        _previewClientOriginalId = 0;
        _isInPreviewMode = false;

        // 触发补充休眠客户端
        StartCoroutine(ReplenishPoolDelayed());

        return activatedClient;
    }

    #endregion

    #region 清理

    /// <summary>
    /// 关闭所有休眠客户端（包括预览客户端）
    /// </summary>
    public void CloseAllDormantClients()
    {
        Debug.Log($"[PreloadClientPoolManager] 关闭所有休眠客户端，数量: {_dormantClients.Count}，预览客户端: {(_previewClient != null ? "有" : "无")}");

        // 先关闭预览客户端（如果有）
        if (_previewClient != null)
        {
            try
            {
                _previewClient.SendCloseCommand();
                Debug.Log("[PreloadClientPoolManager] 已关闭预览客户端");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PreloadClientPoolManager] 关闭预览客户端失败: {ex.Message}");
            }
            _previewClient = null;
            _previewClientOriginalId = 0;
            _isInPreviewMode = false;
        }

        // 关闭所有休眠客户端
        foreach (var kvp in _dormantClients)
        {
            try
            {
                kvp.Value.SendCloseCommand();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PreloadClientPoolManager] 关闭休眠客户端失败: {ex.Message}");
            }
        }

        _dormantClients.Clear();
        _pendingDormantClients = 0;
    }

    private void OnDestroy()
    {
        CloseAllDormantClients();
    }

    #endregion
}
