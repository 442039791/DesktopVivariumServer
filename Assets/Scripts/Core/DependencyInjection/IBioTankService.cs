using System.Collections.Generic;

/// <summary>
/// 生物罐服务接口 - 定义生物罐管理的核心功能
/// 使用接口替代直接依赖BioTankRegistry单例
/// </summary>
public interface IBioTankService
{
    #region 属性

    /// <summary>
    /// 获取所有生物罐（只读）
    /// </summary>
    IReadOnlyDictionary<int, BioTankManager> BioTanks { get; }

    /// <summary>
    /// 获取活动生物罐
    /// </summary>
    BioTankManager ActiveBioTank { get; }

    /// <summary>
    /// 获取活动生物罐ID
    /// </summary>
    int ActiveBioTankID { get; }

    /// <summary>
    /// 是否在生物罐内
    /// </summary>
    bool IsInTank { get; set; }

    /// <summary>
    /// 获取生物罐数量
    /// </summary>
    int Count { get; }

    #endregion

    #region 注册和注销

    /// <summary>
    /// 注册生物罐
    /// </summary>
    /// <param name="bioTank">生物罐管理器</param>
    /// <returns>是否注册成功</returns>
    bool RegisterBioTank(BioTankManager bioTank);

    /// <summary>
    /// 注销生物罐
    /// </summary>
    /// <param name="bioTankId">生物罐ID</param>
    /// <returns>是否注销成功</returns>
    bool UnregisterBioTank(int bioTankId);

    /// <summary>
    /// 清空所有生物罐
    /// </summary>
    void ClearAll();

    #endregion

    #region 查询

    /// <summary>
    /// 获取生物罐
    /// </summary>
    /// <param name="bioTankId">生物罐ID</param>
    /// <returns>生物罐管理器，未找到返回null</returns>
    BioTankManager GetBioTank(int bioTankId);

    /// <summary>
    /// 尝试获取生物罐
    /// </summary>
    /// <param name="bioTankId">生物罐ID</param>
    /// <param name="bioTank">输出的生物罐管理器</param>
    /// <returns>是否找到</returns>
    bool TryGetBioTank(int bioTankId, out BioTankManager bioTank);

    /// <summary>
    /// 检查生物罐是否存在
    /// </summary>
    /// <param name="bioTankId">生物罐ID</param>
    /// <returns>是否存在</returns>
    bool HasBioTank(int bioTankId);

    /// <summary>
    /// 获取所有生物罐列表
    /// </summary>
    /// <returns>生物罐列表</returns>
    List<BioTankManager> GetAllBioTanks();

    #endregion

    #region 活动生物罐管理

    /// <summary>
    /// 设置活动生物罐
    /// </summary>
    /// <param name="bioTank">生物罐管理器</param>
    void SetActiveBioTank(BioTankManager bioTank);

    /// <summary>
    /// 通过ID设置活动生物罐
    /// </summary>
    /// <param name="bioTankId">生物罐ID</param>
    /// <returns>是否设置成功</returns>
    bool SetActiveBioTankById(int bioTankId);

    /// <summary>
    /// 清除活动生物罐
    /// </summary>
    void ClearActiveBioTank();

    /// <summary>
    /// 检查是否是活动生物罐
    /// </summary>
    /// <param name="bioTankId">生物罐ID</param>
    /// <returns>是否是活动罐</returns>
    bool IsActiveBioTank(int bioTankId);

    #endregion
}
