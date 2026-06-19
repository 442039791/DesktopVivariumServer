using UnityEngine;
using Common;

/// <summary>
/// 玩家状态管理器 - 负责玩家数据和金钱管理
/// 从PlayerManager中拆分出来,符合单一职责原则
/// 已实现IPlayerStateService接口，可通过ServiceLocator访问
/// </summary>
public class PlayerStateManager : IPlayerStateService
{
    private static PlayerStateManager _instance;
    public static PlayerStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PlayerStateManager();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 玩家状态数据
    /// </summary>
    public PlayerStateData StateData { get; private set; }

    /// <summary>
    /// 当前金钱
    /// </summary>
    public int Money => StateData?.Money ?? 0;

    private PlayerStateManager()
    {
        // 私有构造函数
    }

    /// <summary>
    /// 初始化玩家状态
    /// </summary>
    public void Initialize(PlayerStateData stateData)
    {
        StateData = stateData ?? new PlayerStateData { Money = 0 };
    }

    /// <summary>
    /// 金钱变化 - 返回是否成功
    /// </summary>
    public bool ChangeMoney(int amount)
    {
        if (StateData == null)
        {
            Debug.LogError("[PlayerStateManager] 状态数据未初始化");
            return false;
        }

        // 检查是否会导致负数
        if (StateData.Money + amount < 0)
        {
            Debug.LogWarning($"[PlayerStateManager] 金钱不足 - 当前: {StateData.Money}, 尝试变化: {amount}");
            StateData.Money = 0;
            DispatchMoneyChangeEvent();
            return false;
        }

        // 应用变化
        StateData.Money += amount;

        DispatchMoneyChangeEvent();
        return true;
    }

    /// <summary>
    /// 检查金钱是否足够
    /// </summary>
    public bool HasEnoughMoney(int requiredAmount)
    {
        return StateData != null && StateData.Money >= requiredAmount;
    }

    /// <summary>
    /// 设置金钱 (直接设置,不检查)
    /// </summary>
    public void SetMoney(int amount)
    {
        if (StateData == null)
        {
            Debug.LogError("[PlayerStateManager] 状态数据未初始化");
            return;
        }

        StateData.Money = Mathf.Max(0, amount);
        DispatchMoneyChangeEvent();
    }

    /// <summary>
    /// 重置玩家状态
    /// </summary>
    public void Reset()
    {
        StateData = null;
    }

    /// <summary>
    /// 分发金钱变化事件
    /// </summary>
    private void DispatchMoneyChangeEvent()
    {
        if (StateData != null)
        {
            event_manager.instance.dispatch_UIevent(
                SysDefine.UI_MoneyChangeEvent,
                StateData.Money.ToString());
        }
    }

    /// <summary>
    /// 获取状态快照 (用于调试)
    /// </summary>
    public string GetStateSnapshot()
    {
        if (StateData == null)
        {
            return "[未初始化]";
        }

        return $"金钱: {StateData.Money:F2}";
    }
}
