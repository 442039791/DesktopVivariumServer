using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 仓库服务接口 - 定义仓库管理的核心功能
/// 使用接口替代直接依赖WarehouseManager单例
/// </summary>
public interface IWarehouseService
{
    #region 数据访问（只读）

    /// <summary>
    /// 获取鱼数据列表（只读）
    /// </summary>
    IReadOnlyDictionary<int, WarehouseBaseData> FishDataList { get; }

    /// <summary>
    /// 获取所有鱼数据列表（只读）
    /// </summary>
    IReadOnlyList<WarehouseBaseData> FishDataListAll { get; }

    /// <summary>
    /// 获取植物数据列表（只读）
    /// </summary>
    IReadOnlyList<WarehouseBaseData> PlantDataList { get; }

    /// <summary>
    /// 获取植物数据字典（只读）
    /// </summary>
    IReadOnlyDictionary<int, WarehouseBaseData> PlantDataDicList { get; }

    /// <summary>
    /// 获取装饰物数据字典（只读）
    /// </summary>
    IReadOnlyDictionary<int, WarehouseBaseData> DecorationDataDicList { get; }

    /// <summary>
    /// 获取方块数据字典（只读）
    /// </summary>
    IReadOnlyDictionary<int, WarehouseBaseData> CubeDataDicList { get; }

    #endregion

    #region 仓库操作

    /// <summary>
    /// 保存动物到仓库
    /// </summary>
    /// <param name="animalStateData">动物状态数据</param>
    /// <param name="adult">是否成年</param>
    /// <param name="UIChange">是否触发UI更新</param>
    void SaveAnimal(AnimalStateData animalStateData, bool adult, bool UIChange = true);

    /// <summary>
    /// 从仓库取出鱼
    /// </summary>
    /// <param name="ID">仓库基础ID</param>
    /// <returns>鱼的仓库数据</returns>
    WarehouseAnimalStateData DislodgeFish(int ID);

    /// <summary>
    /// 保存植物到仓库
    /// </summary>
    /// <param name="data">植物状态数据</param>
    /// <param name="UIChange">是否触发UI更新</param>
    void SavePlant(PlantStateData data, bool UIChange = true);

    /// <summary>
    /// 从仓库取出植物
    /// </summary>
    /// <param name="id">仓库基础ID</param>
    /// <returns>植物仓库数据</returns>
    WarehousePlantStateData DislodgePlant(int id);

    /// <summary>
    /// 保存装饰物到仓库
    /// </summary>
    /// <param name="data">装饰物状态数据</param>
    /// <param name="UIChange">是否触发UI更新</param>
    void SaveDecoration(DecorationStateData data, bool UIChange = true);

    /// <summary>
    /// 从仓库取出装饰物
    /// </summary>
    /// <param name="decorationStateData">装饰物状态数据</param>
    /// <returns>装饰物仓库数据</returns>
    WarehouseDecorationStateData DislodgeDecoration(DecorationStateData decorationStateData);

    /// <summary>
    /// 保存方块到仓库
    /// </summary>
    /// <param name="data">方块状态数据</param>
    /// <param name="UIChange">是否触发UI更新</param>
    void SaveCube(CubeStateData data, bool UIChange = true);

    /// <summary>
    /// 从仓库取出方块
    /// </summary>
    /// <param name="id">方块状态数据</param>
    /// <returns>方块仓库数据</returns>
    WarehouseCubeStateData DislodgeCube(CubeStateData id);

    #endregion

    #region 查询和排序

    /// <summary>
    /// 获取指定类型的仓库数据列表
    /// </summary>
    /// <param name="type">物品类型</param>
    /// <returns>仓库数据列表</returns>
    List<WarehouseBaseData> GetWarehouseBaseDatas(ItemType type);

    /// <summary>
    /// 获取仓库基础数据
    /// </summary>
    /// <param name="type">物品类型</param>
    /// <param name="count">数量</param>
    /// <param name="ID">物品ID</param>
    /// <param name="quality">品质</param>
    /// <returns>仓库基础数据</returns>
    WarehouseBaseData GetWarehouseBaseData(ItemType type, int count, int ID, ObjectQuality quality = ObjectQuality.White);

    /// <summary>
    /// 对动物列表进行排序
    /// </summary>
    void SortAnimal();

    /// <summary>
    /// 对植物列表进行排序
    /// </summary>
    void SortPlant();

    #endregion

    #region 放置物品

    /// <summary>
    /// 放置物品（到生物罐或出售）
    /// </summary>
    /// <param name="baseData">仓库基础数据</param>
    /// <param name="count">数量</param>
    /// <param name="adult">是否成年（仅对鱼有效）</param>
    /// <param name="sell">是否出售</param>
    void PlaceItem(WarehouseBaseData baseData, int count = -1, bool adult = true, bool sell = false);

    #endregion
}
