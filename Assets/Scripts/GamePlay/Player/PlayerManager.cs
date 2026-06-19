using System.Collections.Generic;
using UnityEngine;
using Common;
using Material = UnityEngine.Material;
using static Gather;
using System.IO;

/// <summary>
/// 玩家管理器 - 重构为轻量级门面类
/// 委托具体职责到: PlayerStateManager, BioTankRegistry, EntityFactory, GatherRewardProcessor
/// </summary>
public class PlayerManager : MonoSingleton<PlayerManager>
{
    #region 显示列表 (保留,用于UI)

    public List<string> AnimalDisplayList = new();
    public List<string> PlantDisplayList = new();
    public List<string> FacilitiesDisplayList = new();
    public List<string> CubeDisplayList = new();
    public List<string> DecorationDisplayList = new();
    public List<string> TankDisplayList = new();
    public List<string> AdmissionDisplayList = new();

    #endregion

    #region 兼容性属性 - 委托到新管理器

    /// <summary>
    /// [已弃用] 使用 BioTankRegistry.Instance.BioTanks
    /// </summary>
    [System.Obsolete("使用 BioTankRegistry.Instance.BioTanks")]
    public Dictionary<int, BioTankManager> BioTanks =>
        new Dictionary<int, BioTankManager>(BioTankRegistry.Instance.BioTanks);

    /// <summary>
    /// [已弃用] 使用 PlayerStateManager.Instance.StateData
    /// </summary>
    [System.Obsolete("使用 PlayerStateManager.Instance.StateData")]
    public PlayerStateData stateData
    {
        get => PlayerStateManager.Instance.StateData;
        set => PlayerStateManager.Instance.Initialize(value);
    }

    /// <summary>
    /// [已弃用] 使用 BioTankRegistry.Instance.IsInTank
    /// </summary>
    [System.Obsolete("使用 BioTankRegistry.Instance.IsInTank")]
    public bool OnTankEnter
    {
        get => BioTankRegistry.Instance.IsInTank;
        set => BioTankRegistry.Instance.IsInTank = value;
    }

    /// <summary>
    /// [已弃用] 使用 BioTankRegistry.Instance.ActiveBioTank
    /// </summary>
    [System.Obsolete("使用 BioTankRegistry.Instance.ActiveBioTank")]
    public BioTankManager actionBioTankManager
    {
        get => BioTankRegistry.Instance.ActiveBioTank;
        set
        {
            if (value != null)
            {
                BioTankRegistry.Instance.SetActiveBioTank(value);
            }
            else
            {
                BioTankRegistry.Instance.ClearActiveBioTank();
            }
        }
    }

    /// <summary>
    /// [已弃用] 使用 EntityFactory.Instance.SavedPlantData
    /// </summary>
    [System.Obsolete("使用 EntityFactory.Instance.SavedPlantData")]
    public Dictionary<int, WareHouseSavePlantData> SavePlantData =>
        EntityFactory.Instance.SavedPlantData;

    /// <summary>
    /// 活动生物罐ID
    /// </summary>
    public static int ActiveBioTanksID
    {
        get
        {
            // 静态属性暂时仍使用BioTankRegistry.Instance
            // TODO: 考虑移除此静态属性，改用实例方法
            return BioTankRegistry.Instance.ActiveBioTankID;
        }
    }

    #endregion

    #region 遗留字段 (暂时保留)

    public Material SpriteMat;

    #endregion

    #region 依赖注入字段

    private IBioTankService _bioTankServiceCache;
    private IPlayerStateService _playerStateServiceCache;
    private IWarehouseService _warehouseServiceCache;

    /// <summary>
    /// 生物罐服务（懒加载）
    /// </summary>
    private IBioTankService _bioTankService
    {
        get
        {
            if (_bioTankServiceCache == null)
            {
                ServiceLocator.TryGet(out _bioTankServiceCache);
            }
            return _bioTankServiceCache;
        }
        set => _bioTankServiceCache = value;
    }

    /// <summary>
    /// 玩家状态服务（懒加载）
    /// </summary>
    private IPlayerStateService _playerStateService
    {
        get
        {
            if (_playerStateServiceCache == null)
            {
                ServiceLocator.TryGet(out _playerStateServiceCache);
            }
            return _playerStateServiceCache;
        }
        set => _playerStateServiceCache = value;
    }

    /// <summary>
    /// 仓库服务（懒加载）
    /// </summary>
    private IWarehouseService _warehouseService
    {
        get
        {
            if (_warehouseServiceCache == null)
            {
                ServiceLocator.TryGet(out _warehouseServiceCache);
            }
            return _warehouseServiceCache;
        }
        set => _warehouseServiceCache = value;
    }

    #endregion

    #region 生物罐管理方法 - 委托到BioTankService

    /// <summary>
    /// 获取生物罐
    /// </summary>
    public BioTankManager GetBioTank(int id)
    {
        return _bioTankService.GetBioTank(id);
    }

    /// <summary>
    /// 通过ID设置活动生物罐
    /// </summary>
    public void SetActionBioTankManager(int id)
    {
        _bioTankService.SetActiveBioTankById(id);
    }

    /// <summary>
    /// 设置活动生物罐
    /// </summary>
    public void SetActionBioTankManager(BioTankManager bioTank)
    {
        _bioTankService.SetActiveBioTank(bioTank);
    }

    /// <summary>
    /// 检查是否是活动生物罐
    /// </summary>
    public bool CheckIsActionBiotankID(int id)
    {
        return _bioTankService.IsActiveBioTank(id);
    }

    /// <summary>
    /// 删除生物罐
    /// </summary>
    public void DeleteBiotank(int id)
    {
        // 1. 先关闭对应的客户端
        var client = WebSocketServerManager.GetClient(id);
        if (client != null)
        {
            Debug.Log($"[PlayerManager] 关闭生态箱 {id} 的客户端");
            client.SendCloseCommand();
            // 等待客户端关闭（可以添加延迟或者等待确认）
        }

        // 2. 从注册表中注销生物罐
        if (_bioTankService.UnregisterBioTank(id))
        {
            Debug.Log($"[PlayerManager] 已从注册表注销生态箱 {id}");
        }

        // 3. 删除存档（删除生态箱时必须保存）
        Debug.Log($"[PlayerManager] 删除生态箱 {id} 的存档");
        GameManager.Instance.Save(false);
    }

    /// <summary>
    /// 清空所有生物罐
    /// </summary>
    public void ClearAllBiotanks()
    {
        _bioTankService.ClearAll();
        _playerStateService.Reset();
    }

    /// <summary>
    /// 设置活动管理器为null
    /// </summary>
    public void SetActionManagerNull()
    {
        _bioTankService.ClearActiveBioTank();
    }

    #endregion
    #region 客户端实体移除方法

    /// <summary>
    /// 客户端移除装饰物
    /// </summary>
    public void ClientRemoveDecoration(int objID)
    {
        var activeTank = _bioTankService.ActiveBioTank;
        if (activeTank == null)
        {
            Debug.LogWarning("[PlayerManager] 没有活动生物罐");
            return;
        }

        if (activeTank.DecorationCollection.TryGetById(objID, out var decoration))
        {
            decoration.Destroy();
        }
    }

    /// <summary>
    /// 客户端移除植物 (自动保存到仓库)
    /// </summary>
    public void ClientRemovePlant(int objID)
    {
        var activeTank = _bioTankService.ActiveBioTank;
        if (activeTank == null)
        {
            Debug.LogWarning("[PlayerManager] 没有活动生物罐");
            return;
        }

        if (activeTank.PlantCollection.TryGetById(objID, out var plant))
        {
            // 保存到仓库
            PlantStateData plantData = plant.stateData.DeepCopy();
            plantData.Pos = Vector3.zero;
            plantData.ID = 0;
            _warehouseService.SavePlant(plantData, true);

            // 删除场景中的植物
            plant.Destroy();
        }
    }

    #endregion

    #region 内部数据类

    public class WareHouseSavePlantData
    {
        public PlantStateData plantStateData;
        public int WarehouseBaseID;
    }

    #endregion

    #region 初始化和设置

    public override void Init()
    {
        // 服务依赖使用懒加载，在首次访问时自动获取
        // 如果PlayerStateService的StateData已存在，执行初始化
        if (_playerStateService != null && _playerStateService.StateData != null)
        {
            Init(_playerStateService.StateData);
        }
    }

    public void Init(PlayerStateData playerStateData)
    {
        _playerStateService.Initialize(playerStateData);
        _playerStateService.ChangeMoney(0); // 触发初始化事件
    }

    /// <summary>
    /// 改变建造模式
    /// </summary>
    public void ChangeBuildMode(bool mode)
    {
        var client = WebSocketServerManager.GetClient(ActiveBioTanksID);
        client?.SendChangeBuildMode(mode);
    }

    #endregion

    #region 金钱管理 - 委托到PlayerStateManager

    /// <summary>
    /// 金钱变化
    /// </summary>
    public bool OnMoneyChanged(int amount)
    {
        return _playerStateService.ChangeMoney(amount);
    }

    /// <summary>
    /// 检查金币是否足够
    /// </summary>
    public bool CheckMoneyEnough(int costAmount)
    {
        return _playerStateService.HasEnoughMoney(costAmount);
    }

    #endregion

    #region 采集奖励处理 - 委托到GatherRewardProcessor

    /// <summary>
    /// 处理采集奖励到仓库
    /// </summary>
    public void GatherToWareHouse()
    {
        // 播放点击音效
        AudioSourceManager.Instance.PlayButtonClip();

        GatherRewardProcessor.Instance.ProcessGatherById(null);
    }

    /// <summary>
    /// 测试：处理采集奖励（支持自定义奖励列表）
    /// </summary>
    public void TestGatherToWareHouse(List<Reward> rewards)
    {
        GatherRewardProcessor.Instance.ProcessGatherRewards(rewards);
    }

    /// <summary>
    /// 测试：采集所有类型到仓库
    /// </summary>
    public void TestGatherAllToWareHouse()
    {
        var gatherList = Gather.Instance.TestGetAllGather();
        GatherRewardProcessor.Instance.ProcessGatherRewards(gatherList);
    }

    /// <summary>
    /// 测试：采集部分动物到仓库
    /// </summary>
    public void TestGatherSomeAnimalToWareHouse(string gatherID)
    {
        var gatherList = Gather.Instance.TestGetSomeAnimalGather(gatherID);
        GatherRewardProcessor.Instance.ProcessGatherRewards(gatherList);
    }

    /// <summary>
    /// 测试：采集部分植物到仓库
    /// </summary>
    public void TestGatherSomePlantToWareHouse(string gatherID)
    {
        var gatherList = Gather.Instance.TestGetSomePlantGather(gatherID);
        GatherRewardProcessor.Instance.ProcessGatherRewards(gatherList);
    }

    /// <summary>
    /// 测试：采集所有动物到仓库
    /// </summary>
    public void TestGatherAnimalToWareHouse()
    {
        var gatherList = Gather.Instance.TestGetAnimalGather();
        GatherRewardProcessor.Instance.ProcessGatherRewards(gatherList);
    }

    /// <summary>
    /// 测试：采集所有植物到仓库
    /// </summary>
    public void TestGatherPlantToWareHouse()
    {
        var gatherList = Gather.Instance.TestGetPlantGather();
        GatherRewardProcessor.Instance.ProcessGatherRewards(gatherList);
    }

    #endregion
    #region 生物罐创建 - 委托到EntityFactory

    /// <summary>
    /// 创建生物罐
    /// </summary>
    /// <param name="bioTankStateData">生态箱状态数据</param>
    /// <param name="createIsland">是否创建岛屿</param>
    /// <param name="launchClient">是否启动新客户端（加载存档时应传false）</param>
    public BioTankManager CreateBioTank(BioTankStateData bioTankStateData, bool createIsland, bool launchClient = false)
    {
        var bioTank = EntityFactory.Instance.CreateBioTank(bioTankStateData, createIsland, launchClient);

        // 如果没有活动生物罐，设置为活动
        if (_bioTankService.ActiveBioTank == null && bioTank != null)
        {
            SetActionBioTankManager(bioTank);
        }

        return bioTank;
    }

    /// <summary>
    /// 从蓝图创建生物罐
    /// </summary>
    public BioTankManager CreateBioTankInstance(BioTankStateData bioTankStateData, string path)
    {
        var bioTank = EntityFactory.Instance.CreateBioTankFromBlueprint(bioTankStateData, path);

        // 如果没有活动生物罐，设置为活动
        if (_bioTankService.ActiveBioTank == null && bioTank != null)
        {
            SetActionBioTankManager(bioTank);
        }

        return bioTank;
    }

    /// <summary>
    /// 使用初始布局创建生物罐（根据尺寸自动匹配初始布局）
    /// </summary>
    public BioTankManager CreateBioTankWithInitialLayout(BioTankStateData bioTankStateData)
    {
        var bioTank = EntityFactory.Instance.CreateBioTankWithInitialLayout(bioTankStateData);

        // 如果没有活动生物罐，设置为活动
        if (_bioTankService.ActiveBioTank == null && bioTank != null)
        {
            SetActionBioTankManager(bioTank);
        }

        return bioTank;
    }

    #endregion


    #region 实体创建 - 委托到EntityFactory

    /// <summary>
    /// 创建植物
    /// </summary>
    public Plant CreatePlant(PlantStateData plantStateData)
    {
        return EntityFactory.Instance.CreatePlant(plantStateData);
    }

    /// <summary>
    /// 创建保存的植物（从客户端）
    /// </summary>
    public void CreateSavePlant(int clientId, int objID, Vector3 position)
    {
        EntityFactory.Instance.CreateSavedPlant(clientId, objID, position);
    }

    /// <summary>
    /// 创建装饰物
    /// </summary>
    public void CreateDecoration(DecorationStateData decorationStateData)
    {
        EntityFactory.Instance.CreateDecoration(decorationStateData);
    }

    /// <summary>
    /// 创建动物
    /// </summary>
    public Animal CreateAnimal(AnimalStateData animalStateData, bool display = true)
    {
        return EntityFactory.Instance.CreateAnimal(animalStateData, display);
    }

    #endregion

    #region 遗留方法（已废弃）

    /// <summary>
    /// [已弃用] 修改生物罐体积
    /// </summary>
    [System.Obsolete("该方法已废弃")]
    public void ChangeBioTanksVolume(int id, int vol)
    {
        return;
    }

    #endregion
}
