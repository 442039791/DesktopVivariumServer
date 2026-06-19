using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 生物罐注册表 - 负责生物罐的注册、查找和活动罐管理
/// 从PlayerManager中拆分出来,符合单一职责原则
/// 已实现IBioTankService接口，可通过ServiceLocator访问
/// </summary>
public class BioTankRegistry : IBioTankService
{
    private static BioTankRegistry _instance;
    public static BioTankRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BioTankRegistry();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 所有生物罐字典 (ID -> BioTankManager)
    /// </summary>
    private readonly Dictionary<int, BioTankManager> _bioTanks = new Dictionary<int, BioTankManager>();

    /// <summary>
    /// 当前活动的生物罐
    /// </summary>
    private BioTankManager _activeBioTank;

    /// <summary>
    /// 是否在生物罐内
    /// </summary>
    public bool IsInTank { get; set; }

    /// <summary>
    /// 重入保护标志 - 防止SetActiveBioTank无限递归
    /// </summary>
    private bool _isSettingActiveBioTank = false;

    /// <summary>
    /// 获取所有生物罐 (只读)
    /// </summary>
    public IReadOnlyDictionary<int, BioTankManager> BioTanks => _bioTanks;

    /// <summary>
    /// 获取活动生物罐
    /// </summary>
    public BioTankManager ActiveBioTank => _activeBioTank;

    /// <summary>
    /// 获取活动生物罐ID
    /// </summary>
    public int ActiveBioTankID
    {
        get
        {
            var state = _activeBioTank?.stateData;
            if (state == null)
            {
                return -999;
            }
            return state.ID;
        }
    }

    private BioTankRegistry()
    {
        // 私有构造函数
    }

    /// <summary>
    /// 注册生物罐
    /// </summary>
    public bool RegisterBioTank(BioTankManager bioTank)
    {
        if (bioTank == null)
        {
            Debug.LogError("[BioTankRegistry] 尝试注册null生物罐");
            return false;
        }

        int id = bioTank.stateData.ID;

        if (_bioTanks.ContainsKey(id))
        {
            Debug.LogWarning($"[BioTankRegistry] 生物罐ID {id} 已存在,将被覆盖");
        }

        _bioTanks[id] = bioTank;

        // 如果是第一个生物罐,自动设为活动罐
        if (_activeBioTank == null)
        {
            SetActiveBioTank(bioTank);
        }

        return true;
    }

    /// <summary>
    /// 注销生物罐
    /// </summary>
    public bool UnregisterBioTank(int bioTankId)
    {
        if (!_bioTanks.TryGetValue(bioTankId, out var bioTank))
        {
            Debug.LogWarning($"[BioTankRegistry] 未找到生物罐: ID={bioTankId}");
            return false;
        }

        // 如果是活动罐,先清除
        if (_activeBioTank == bioTank)
        {
            ClearActiveBioTank();
        }

        // 销毁生物罐
        bioTank.Destroy();
        _bioTanks.Remove(bioTankId);

        return true;
    }

    /// <summary>
    /// 获取生物罐
    /// </summary>
    public BioTankManager GetBioTank(int bioTankId)
    {
        _bioTanks.TryGetValue(bioTankId, out var bioTank);
        return bioTank;
    }

    /// <summary>
    /// 尝试获取生物罐
    /// </summary>
    public bool TryGetBioTank(int bioTankId, out BioTankManager bioTank)
    {
        return _bioTanks.TryGetValue(bioTankId, out bioTank);
    }

    /// <summary>
    /// 检查生物罐是否存在
    /// </summary>
    public bool HasBioTank(int bioTankId)
    {
        return _bioTanks.ContainsKey(bioTankId);
    }

    /// <summary>
    /// 设置活动生物罐
    /// </summary>
    /// <param name="bioTank">目标生物罐</param>
    public void SetActiveBioTank(BioTankManager bioTank)
    {
        // 重入保护：如果正在设置活动生物罐，直接返回
        if (_isSettingActiveBioTank)
        {
            Debug.LogWarning($"[BioTankRegistry] 检测到重入调用 SetActiveBioTank，已阻止，目标ID={(bioTank != null ? bioTank.stateData.ID.ToString() : "null")}");
            return;
        }

        try
        {
            _isSettingActiveBioTank = true;

            if (bioTank == null)
            {
                Debug.LogWarning("[BioTankRegistry] 尝试设置null为活动生物罐");
                ClearActiveBioTank();
                return;
            }

            var targetState = bioTank.stateData;
            if (targetState == null)
            {
                Debug.LogError("[BioTankRegistry] 目标生物罐的stateData为null,无法设置为活动罐");
                return;
            }

            // 如果已经是活动罐，直接返回
            if (_activeBioTank == bioTank)
            {
                return;
            }

            // 如果之前有活动罐,通知其失活
            if (_activeBioTank != null && _activeBioTank != bioTank)
            {
                var previousState = _activeBioTank.stateData;
                if (previousState != null)
                {
                    var previousClient = WebSocketServerManager.GetClient(previousState.ID);
                    if (previousClient != null)
                    {
                        previousClient.SendActionBioTankCommand(false);
                    }
                }
                else
                {
                    Debug.LogWarning("[BioTankRegistry] 旧活动生物罐stateData为null,无法发送失活命令");
                }
            }

            _activeBioTank = bioTank;
            IsInTank = true;

            // 通知新活动罐
            var activeClient = WebSocketServerManager.GetClient(targetState.ID);
            if (activeClient != null)
            {
                activeClient.SendActionBioTankCommand(true);
            }
            else
            {
                Debug.LogWarning($"[BioTankRegistry] 找不到客户端连接 ID={targetState.ID}");
            }

            // 触发UI刷新事件
            event_manager.instance.dispatch_UIevent(SysDefine.InitUI_BioTankInfo, targetState.ID);

        }
        finally
        {
            _isSettingActiveBioTank = false;
        }
    }

    /// <summary>
    /// 通过ID设置活动生物罐
    /// </summary>
    /// <param name="bioTankId">生物罐ID</param>
    public bool SetActiveBioTankById(int bioTankId)
    {
        if (TryGetBioTank(bioTankId, out var bioTank))
        {
            SetActiveBioTank(bioTank);
            return true;
        }

        Debug.LogWarning($"[BioTankRegistry] 未找到生物罐: ID={bioTankId}");
        return false;
    }

    /// <summary>
    /// 清除活动生物罐
    /// </summary>
    public void ClearActiveBioTank()
    {
        if (_activeBioTank == null)
        {
            IsInTank = false;
            return;
        }

        var state = _activeBioTank.stateData;
        if (state != null)
        {
            var client = WebSocketServerManager.GetClient(state.ID);
            client?.SendActionBioTankCommand(false);
        }
        else
        {
            Debug.LogWarning("[BioTankRegistry] 活动生物罐stateData为null,跳过客户端通知");
        }

        _activeBioTank = null;
        IsInTank = false;
    }

    /// <summary>
    /// 检查是否是活动生物罐
    /// </summary>
    public bool IsActiveBioTank(int bioTankId)
    {
        var state = _activeBioTank?.stateData;
        return state != null && state.ID == bioTankId;
    }

    /// <summary>
    /// 清空所有生物罐
    /// </summary>
    public void ClearAll()
    {

        // 先清除活动罐，避免销毁后stateData为null导致空引用
        ClearActiveBioTank();

        // 销毁所有生物罐
        foreach (var bioTank in _bioTanks.Values)
        {
            bioTank.Destroy();
        }

        _bioTanks.Clear();
    }

    /// <summary>
    /// 获取所有生物罐列表
    /// </summary>
    public List<BioTankManager> GetAllBioTanks()
    {
        return _bioTanks.Values.ToList();
    }

    /// <summary>
    /// 获取生物罐数量
    /// </summary>
    public int Count => _bioTanks.Count;
}
