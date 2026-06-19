/// <summary>
/// 玩家状态服务接口 - 定义玩家数据和金钱管理的核心功能
/// 使用接口替代直接依赖PlayerStateManager单例
/// </summary>
public interface IPlayerStateService
{
    #region 属性

    /// <summary>
    /// 玩家状态数据
    /// </summary>
    PlayerStateData StateData { get; }

    /// <summary>
    /// 当前金钱
    /// </summary>
    int Money { get; }

    #endregion

    #region 初始化

    /// <summary>
    /// 初始化玩家状态
    /// </summary>
    /// <param name="stateData">玩家状态数据</param>
    void Initialize(PlayerStateData stateData);

    /// <summary>
    /// 重置玩家状态
    /// </summary>
    void Reset();

    #endregion

    #region 金钱管理

    /// <summary>
    /// 金钱变化 - 返回是否成功
    /// </summary>
    /// <param name="amount">变化金额（正数为增加，负数为减少）</param>
    /// <returns>是否变化成功</returns>
    bool ChangeMoney(int amount);

    /// <summary>
    /// 检查金钱是否足够
    /// </summary>
    /// <param name="requiredAmount">需要的金额</param>
    /// <returns>是否足够</returns>
    bool HasEnoughMoney(int requiredAmount);

    /// <summary>
    /// 设置金钱（直接设置，不检查）
    /// </summary>
    /// <param name="amount">金额</param>
    void SetMoney(int amount);

    #endregion

    #region 调试

    /// <summary>
    /// 获取状态快照（用于调试）
    /// </summary>
    /// <returns>状态信息字符串</returns>
    string GetStateSnapshot();

    #endregion
}
