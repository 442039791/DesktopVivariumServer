using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 实体工厂 - 负责创建动物、植物、装饰物和生物罐
/// 从PlayerManager中拆分出来,符合单一职责原则
/// 已实现IEntityFactoryService接口，可通过ServiceLocator访问
/// </summary>
public class EntityFactory : IEntityFactoryService
{
    private static EntityFactory _instance;
    public static EntityFactory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EntityFactory();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 临时保存的植物数据 (客户端ID -> 数据)
    /// </summary>
    public Dictionary<int, PlayerManager.WareHouseSavePlantData> SavedPlantData { get; private set; }

    // 依赖注入字段
    private IWarehouseService _warehouseService;

    private EntityFactory()
    {
        SavedPlantData = new Dictionary<int, PlayerManager.WareHouseSavePlantData>();

        // 初始化依赖注入
        _warehouseService = ServiceLocator.Get<IWarehouseService>();
    }

    #region 生物罐创建

    /// <summary>
    /// 创建生物罐
    /// </summary>
    /// <param name="stateData">生态箱状态数据</param>
    /// <param name="createIsland">是否创建岛屿（旧参数，保留兼容性）</param>
    /// <param name="launchClient">是否启动新客户端（默认false，游戏初始化时由GameInitializer统一启动）</param>
    /// <returns>生物罐管理器</returns>
    public BioTankManager CreateBioTank(BioTankStateData stateData, bool createIsland = false, bool launchClient = false)
    {
        if (stateData == null)
        {
            Debug.LogError("[EntityFactory] 生物罐状态数据为null");
            return null;
        }

        var bioTank = new BioTankManager();
        bioTank.Init(stateData, createIsland);

        // 注册到注册表
        BioTankRegistry.Instance.RegisterBioTank(bioTank);

        // 检查是否需要启动新客户端
        if (launchClient)
        {
            CheckAndLaunchNewClient();
        }

        event_manager.instance.dispatch_UIevent(SysDefine.UI_Biotank_List_Init, null);

        return bioTank;
    }

    /// <summary>
    /// 从蓝图实例化生物罐
    /// </summary>
    public BioTankManager CreateBioTankFromBlueprint(BioTankStateData stateData, string blueprintPath)
    {
        if (stateData == null || string.IsNullOrEmpty(blueprintPath))
        {
            Debug.LogError("[EntityFactory] 参数无效");
            return null;
        }

        var bioTank = new BioTankManager();
        bioTank.Init(stateData);

        // 注册到注册表
        BioTankRegistry.Instance.RegisterBioTank(bioTank);

        // 复制蓝图数据
        SaveBlueprintData(stateData.ID, blueprintPath);

        // 检查是否需要启动新客户端
        CheckAndLaunchNewClient();

        event_manager.instance.dispatch_UIevent(SysDefine.UI_Biotank_List_Init, null);

        return bioTank;
    }

    /// <summary>
    /// 使用初始布局创建生物罐（根据尺寸自动匹配初始布局）
    /// </summary>
    /// <param name="stateData">生态箱状态数据</param>
    /// <returns>生物罐管理器</returns>
    public BioTankManager CreateBioTankWithInitialLayout(BioTankStateData stateData)
    {
        if (stateData == null)
        {
            Debug.LogError("[EntityFactory] 生物罐状态数据为null");
            return null;
        }

        var bioTank = new BioTankManager();
        bioTank.Init(stateData);

        // 注册到注册表
        BioTankRegistry.Instance.RegisterBioTank(bioTank);

        // 尝试应用初始布局
        bool layoutApplied = TankInitialLayoutManager.Instance.ApplyInitialLayout(
            stateData.ID,
            stateData.area_x,
            stateData.area_y,
            stateData.area_z
        );

        if (!layoutApplied)
        {
            Debug.Log($"[EntityFactory] 尺寸 {stateData.area_x}x{stateData.area_y}x{stateData.area_z} 没有初始布局，使用空白生态箱");
        }

        // 检查是否需要启动新客户端
        CheckAndLaunchNewClient();

        event_manager.instance.dispatch_UIevent(SysDefine.UI_Biotank_List_Init, null);

        return bioTank;
    }

    #endregion

    #region 动物创建

    /// <summary>
    /// 创建动物
    /// </summary>
    public Animal CreateAnimal(AnimalStateData stateData, bool display = true)
    {
        if (stateData == null)
        {
            Debug.LogError("[EntityFactory] 动物状态数据为null");
            return null;
        }

        // 检查生物罐是否存在
        if (!BioTankRegistry.Instance.TryGetBioTank(stateData.ID, out var bioTank))
        {
            Debug.LogWarning($"[EntityFactory] 未找到生物罐: ID={stateData.ID}");
            return null;
        }

        // 检查空间是否足够
        var animalData = stateData.GetAnimalData();
        if (bioTank.GetUseVolume() + animalData.activitySpace > bioTank.Volume)
        {
            Debug.LogWarning($"[EntityFactory] 生物罐 {stateData.ID} 空间不足");
            return null;
        }

        var animal = new Animal();
        animal.Init(stateData, display);

        return animal;
    }

    #endregion

    #region 植物创建

    /// <summary>
    /// 创建植物
    /// </summary>
    public Plant CreatePlant(PlantStateData stateData)
    {
        if (stateData == null)
        {
            Debug.LogError("[EntityFactory] 植物状态数据为null");
            return null;
        }

        var plant = new Plant();
        plant.Init(stateData);

        return plant;
    }

    /// <summary>
    /// 创建保存的植物 (从客户端)
    /// </summary>
    public Plant CreateSavedPlant(int clientId, int objID, Vector3 position)
    {
        if (!SavedPlantData.TryGetValue(clientId, out var savedData))
        {
            Debug.LogError($"[EntityFactory] 未找到客户端 {clientId} 的保存植物数据");
            Debug.LogError($"[EntityFactory] 当前SavedPlantData中的clientId: {string.Join(", ", SavedPlantData.Keys)}");
            Debug.LogError($"[EntityFactory] 这通常是因为clientId不匹配。请检查WarehouseManager.PlaceItem是否使用了正确的clientId");
            return null;
        }

        if (savedData == null)
        {
            Debug.LogWarning($"[EntityFactory] 客户端 {clientId} 的保存植物数据为null");
            return null;
        }

        if (objID != savedData.plantStateData.ObjID)
        {
            Debug.LogWarning($"[EntityFactory] 植物ID不匹配: 期望={savedData.plantStateData.ObjID}, 实际={objID}");
            SavedPlantData[clientId] = null;
            return null;
        }

        // 更新位置
        savedData.plantStateData.Pos = position;

        // 创建植物
        var plant = CreatePlant(savedData.plantStateData);

        if (plant != null)
        {
        }

        // 从仓库移除
        _warehouseService.DislodgePlant(savedData.WarehouseBaseID);

        // 清除临时数据
        SavedPlantData[clientId] = null;

        return plant;
    }

    #endregion

    #region 装饰物创建

    /// <summary>
    /// 创建装饰物
    /// </summary>
    public Decoration CreateDecoration(DecorationStateData stateData)
    {
        if (stateData == null)
        {
            Debug.LogError("[EntityFactory] 装饰物状态数据为null");
            return null;
        }

        var decoration = new Decoration();
        decoration.Init(stateData);

        return decoration;
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 保存蓝图数据到文件（支持压缩格式）
    /// </summary>
    private void SaveBlueprintData(int bioTankId, string sourcePath)
    {
        string targetPath = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            SysDefine.DefaultSavePath,
            $"{bioTankId}_{SysDefine.DefaultSaveBioTankDataName}");

        try
        {
            // 使用智能加载读取源文件（支持压缩和非压缩格式）
            string content = SaveCompression.LoadSmart(sourcePath);
            // 保存为压缩格式
            SaveCompression.SaveCompressed(targetPath, content);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EntityFactory] 保存蓝图数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查并启动新客户端
    /// </summary>
    private void CheckAndLaunchNewClient()
    {
        // 直接从 clients 字典获取活跃客户端数量
        int activeClientCount = ControlBehavior.clients.Count;

        if (BioTankRegistry.Instance.Count > activeClientCount)
        {
            // 启动新客户端，服务端会分配ID
            HiddenProcessLauncher.Instance.LaunchHiddenProcess();
        }
    }

    #endregion
}
