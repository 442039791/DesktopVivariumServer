using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 实体工厂服务接口 - 定义实体创建的核心功能
/// 使用接口替代直接依赖EntityFactory单例
/// </summary>
public interface IEntityFactoryService
{
    #region 属性

    /// <summary>
    /// 临时保存的植物数据（客户端ID -> 数据）
    /// </summary>
    Dictionary<int, PlayerManager.WareHouseSavePlantData> SavedPlantData { get; }

    #endregion

    #region 生物罐创建

    /// <summary>
    /// 创建生物罐
    /// </summary>
    /// <param name="stateData">生物罐状态数据</param>
    /// <param name="createIsland">是否创建岛屿</param>
    /// <param name="launchClient">是否启动新客户端</param>
    /// <returns>生物罐管理器</returns>
    BioTankManager CreateBioTank(BioTankStateData stateData, bool createIsland = false, bool launchClient = false);

    /// <summary>
    /// 从蓝图实例化生物罐
    /// </summary>
    /// <param name="stateData">生物罐状态数据</param>
    /// <param name="blueprintPath">蓝图路径</param>
    /// <returns>生物罐管理器</returns>
    BioTankManager CreateBioTankFromBlueprint(BioTankStateData stateData, string blueprintPath);

    /// <summary>
    /// 使用初始布局创建生物罐（根据尺寸自动匹配初始布局）
    /// </summary>
    /// <param name="stateData">生态箱状态数据</param>
    /// <returns>生物罐管理器</returns>
    BioTankManager CreateBioTankWithInitialLayout(BioTankStateData stateData);

    #endregion

    #region 动物创建

    /// <summary>
    /// 创建动物
    /// </summary>
    /// <param name="stateData">动物状态数据</param>
    /// <param name="display">是否显示</param>
    /// <returns>动物对象</returns>
    Animal CreateAnimal(AnimalStateData stateData, bool display = true);

    #endregion

    #region 植物创建

    /// <summary>
    /// 创建植物
    /// </summary>
    /// <param name="stateData">植物状态数据</param>
    /// <returns>植物对象</returns>
    Plant CreatePlant(PlantStateData stateData);

    /// <summary>
    /// 创建保存的植物（从客户端）
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="objID">对象ID</param>
    /// <param name="position">位置</param>
    /// <returns>植物对象</returns>
    Plant CreateSavedPlant(int clientId, int objID, Vector3 position);

    #endregion

    #region 装饰物创建

    /// <summary>
    /// 创建装饰物
    /// </summary>
    /// <param name="stateData">装饰物状态数据</param>
    /// <returns>装饰物对象</returns>
    Decoration CreateDecoration(DecorationStateData stateData);

    #endregion
}
