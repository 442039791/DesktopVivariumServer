using Common;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;

/// <summary>
/// 物品类型
/// </summary>
[System.Serializable]
public enum ItemType
{
    /// <summary>
    /// 成年鱼
    /// </summary>
    AdultFish,
    /// <summary>
    /// 幼年鱼
    /// </summary>
    JuvenileFish,
    /// <summary>
    /// 植物
    /// </summary>
    Plant,
    /// <summary>
    /// 装饰物
    /// </summary>
    Decoration,
    /// <summary>
    /// 方块
    /// </summary>
    Cube
}
/// <summary>
/// 仓库管理器 - 负责管理玩家仓库中的所有物品
///
/// 功能：
/// - 动物（鱼类）的存储和管理
/// - 植物的存储和管理
/// - 装饰物的存储和管理
/// - 方块的存储和管理
/// - 物品放置和取出
/// - 排序和查询功能
///
/// 注意：此类职责较重，后续考虑按物品类型拆分为独立的仓库管理器
/// 已实现IWarehouseService接口，可通过ServiceLocator访问
/// </summary>
public class WarehouseManager : MonoSingleton<WarehouseManager>, IWarehouseService
{
    #region 内部字段

    // 新的存储类（重构后）
    private AnimalWarehouseStorage _animalStorage;
    private PlantWarehouseStorage _plantStorage;
    private DecorationWarehouseStorage _decorationStorage;
    private CubeWarehouseStorage _cubeStorage;

    /// <summary>
    /// 唯一ID生成器
    /// </summary>
    private int _UniqueId;

    /// <summary>
    /// 临时缓存字典
    /// </summary>
    private Dictionary<string, WarehouseBaseData> _cache = new();

    // 公共只读属性（委托到新的存储类）
    public IReadOnlyDictionary<int, WarehouseBaseData> FishDataList
    {
        get
        {
            if (_animalStorage == null) return new Dictionary<int, WarehouseBaseData>();
            return _animalStorage.DataDictionary.ToDictionary(kvp => kvp.Key, kvp => (WarehouseBaseData)kvp.Value);
        }
    }
    public IReadOnlyList<WarehouseBaseData> FishDataListAll => _animalStorage?.DataList ?? new List<WarehouseBaseData>();
    // 修复: 正确转换泛型字典,避免类型转换失败导致返回空字典
    public IReadOnlyDictionary<int, WarehouseBaseData> PlantDataDicList
    {
        get
        {
            if (_plantStorage == null) return new Dictionary<int, WarehouseBaseData>();
            return _plantStorage.DataDictionary.ToDictionary(kvp => kvp.Key, kvp => (WarehouseBaseData)kvp.Value);
        }
    }
    public IReadOnlyList<WarehouseBaseData> PlantDataList => _plantStorage?.DataList ?? new List<WarehouseBaseData>();
    public IReadOnlyDictionary<int, WarehouseBaseData> DecorationDataDicList
    {
        get
        {
            if (_decorationStorage == null) return new Dictionary<int, WarehouseBaseData>();
            return _decorationStorage.DataDictionary.ToDictionary(kvp => kvp.Key, kvp => (WarehouseBaseData)kvp.Value);
        }
    }
    public IReadOnlyDictionary<int, WarehouseBaseData> CubeDataDicList
    {
        get
        {
            if (_cubeStorage == null) return new Dictionary<int, WarehouseBaseData>();
            return _cubeStorage.DataDictionary.ToDictionary(kvp => kvp.Key, kvp => (WarehouseBaseData)kvp.Value);
        }
    }

    /// <summary>
    /// 仓库UI引用
    /// </summary>
    public UI_Warehouse uI_Warehouse;

    #endregion

    #region 排序相关

    /// <summary>
    /// 排序函数委托
    /// </summary>
    public delegate void SortFunc();

    /// <summary>
    /// 动物排序函数字典
    /// </summary>
    private static Dictionary<AnimalSortType, SortFunc> animalSortFuncDict;

    /// <summary>
    /// 当前动物排序类型
    /// </summary>
    public AnimalSortType CurrentAnimalSortType = AnimalSortType.Default;

    /// <summary>
    /// 植物排序函数字典
    /// </summary>
    private static Dictionary<PlantSortType, SortFunc> plantSortFuncDict;

    /// <summary>
    /// 当前植物排序类型
    /// </summary>
    public PlantSortType CurrentPlantSortType = PlantSortType.Default;

    /// <summary>
    /// 自定义类排序器
    /// </summary>
    private CustomClassSorter customClassSorter = new CustomClassSorter();

    #endregion
    public void Destroy()
    {
        if (uI_Warehouse != null)
        {
            uI_Warehouse.OnShow = false;
            uI_Warehouse = null;
        }

        // 清空存储类
        _animalStorage?.Clear();
        _plantStorage?.Clear();
        _decorationStorage?.Clear();
        _cubeStorage?.Clear();
    }

    public override void Init()
    {
        // 初始化新的存储类（只在第一次初始化）
        if (_animalStorage == null)
        {
            _animalStorage = new AnimalWarehouseStorage();
            _plantStorage = new PlantWarehouseStorage();
            _decorationStorage = new DecorationWarehouseStorage();
            _cubeStorage = new CubeWarehouseStorage();
        }

        if (animalSortFuncDict == null)
        {
            animalSortFuncDict = new Dictionary<AnimalSortType, SortFunc>()
            {
                { AnimalSortType.Default,SortAnimalDefault},
            };
        }
        if (plantSortFuncDict == null)
        {
            plantSortFuncDict = new Dictionary<PlantSortType, SortFunc>()
            {
                { PlantSortType.Default,SortPlantDefault},
            };
        }
        _UniqueId = 0;
    }
    public void SortPlantDefault()
    {
        _plantStorage.Sort((a, b) => {
            var plantA = (WarehousePlantStateData)a;
            var plantB = (WarehousePlantStateData)b;
            float growthA = plantA.data.GetGrowthProgress();
            float growthB = plantB.data.GetGrowthProgress();
            bool matureA = growthA >= 1f;
            bool matureB = growthB >= 1f;

            // 大于等于100%的植物，优先按Sort参数升序排序（数字越小越靠前）
            if (matureA && matureB)
            {
                int sortCompare = plantA.data.GetPlantData().Sort.CompareTo(plantB.data.GetPlantData().Sort);
                if (sortCompare != 0) return sortCompare;

                // Sort相同再按价格降序
                int priceCompare = plantB.data.GetPrice().CompareTo(plantA.data.GetPrice());
                if (priceCompare != 0) return priceCompare;

                // 价格相同再按成长值降序，保证稳定
                return growthB.CompareTo(growthA);
            }

            // 只有一方达到100%，直接将已满成长的排前面
            if (matureA != matureB) return matureA ? -1 : 1;

            // 其余情况按成长度排序，越高越靠前
            int growthCompare = growthB.CompareTo(growthA);
            if (growthCompare != 0) return growthCompare;

            // 兜底按配置Sort升序排序保证稳定性（数字越小越靠前）
            return plantA.data.GetPlantData().Sort.CompareTo(plantB.data.GetPlantData().Sort);
        });
    }
    public void SortAnimal()
    {
        SortAnimal(CurrentAnimalSortType);
    }
    public void SortAnimal(AnimalSortType sortType)
    {
        animalSortFuncDict[sortType].Invoke();
    }
    public void SortPlant()
    {
        SortPlant(CurrentPlantSortType);
    }
    public void SortPlant(PlantSortType sortType)
    {
        plantSortFuncDict[sortType].Invoke();
    }
    public void SortAnimalDefault()
    {
        _animalStorage.Sort((a, b) => {
            int ageSort = customClassSorter.Compare(((WarehouseAnimalStateData)a).stateData.State, ((WarehouseAnimalStateData)b).stateData.State);
            if (ageSort != 0) return ageSort;
            int qualitySort = ((WarehouseAnimalStateData)b).stateData.GetObjectQuality().CompareTo(((WarehouseAnimalStateData)a).stateData.GetObjectQuality());
            if (qualitySort != 0) return qualitySort;
            return ((WarehouseAnimalStateData)b).stateData.GetAnimalData().Sort.CompareTo(((WarehouseAnimalStateData)a).stateData.GetAnimalData().Sort);
        });
    }
    public int GetUniqueId()
    {
        return _UniqueId++;
    }
    public List<WarehouseBaseData> GetWarehouseBaseDatas(ItemType type)
    {
        switch (type)
        {
            case ItemType.AdultFish:
            case ItemType.JuvenileFish:
                // 在返回数据前先排序
                SortAnimal();
                return _animalStorage.DataList.ToList();

            case ItemType.Plant:
                // 在返回数据前先排序
                SortPlant();
                return _plantStorage.DataList.ToList();

            case ItemType.Decoration:
                return _decorationStorage.DataList.ToList();

            case ItemType.Cube:
                return _cubeStorage.DataList.ToList();
        }
        // 返回空列表而不是null，防止NullReferenceException
        return new List<WarehouseBaseData>();
    }
    public WarehouseBaseData GetWarehouseBaseData(ItemType type,int count,int ID,ObjectQuality quality = ObjectQuality.White)
    {


        switch(type)
        {
            case ItemType.AdultFish:
            case ItemType.JuvenileFish:
                string key = "Fish"+"_"+ID ;
                if (_cache.TryGetValue(key,out var data))
                {
                    WarehouseAnimalStateData _d = (data as WarehouseAnimalStateData).DeepCopy();
                    var _da = GameConfigDataBase.GetConfigData<AnimalData>(ID, SysDefine.AnimalConfigData);
                    _d.Icon = _da.GetSprite(type == ItemType.AdultFish, quality);
                    _d.Count = count;
                    return _d;
                }

                WarehouseAnimalStateData d = new WarehouseAnimalStateData();
                var da = GameConfigDataBase.GetConfigData<AnimalData>(ID, SysDefine.AnimalConfigData);
                d.DataType = WarehouseBaseDataType.Fish;
                //d.Adult = true;
                bool isAdult = type == ItemType.AdultFish;
                d.Count = count;
                d.Name = da.name;
                d.Icon = da.GetSprite(isAdult, quality);
                _cache.Add(key, d);
                return d;
                
            //case ItemType.JuvenileFish:
            //    string key1 = ID + "_JuvenileFish";
            //    if (_cache.TryGetValue(key1, out var data1))
            //    {
            //        data1.Count = count;
            //        return data1;
            //    }

            //    WarehouseFishStateData d1 = new WarehouseFishStateData();
            //    var da1 = GameConfigDataBase.GetConfigData<AnimalData>(ID, SysDefine.AnimalConfigData);
            //    d1.Adult = false;
            //    d1.Count = count;
            //    d1.Name = da1.name;
            //    d1.Icon = da1.GetJuvenileSprite();
            //    _cache.Add(key1, d1);
            //    return d1;

            case ItemType.Plant:
                string key2 = "Plant"+"_"+ID.ToString() ;
                if (_cache.TryGetValue(key2, out var data2))
                {
                    var _d2 = (data2 as WarehousePlantStateData).DeepCopy();
                    _d2.Count = count;
                    return _d2.DeepCopy();
                }

                WarehousePlantStateData d2 = new WarehousePlantStateData();
                var da2 = GameConfigDataBase.GetConfigData<PlantData>(ID, SysDefine.PlantConfigData);
                d2.DataType = WarehouseBaseDataType.Plant;
                d2.Count = count;
                d2.Name = da2.name;
                d2.Icon = da2.GetIconSprite();
                _cache.Add(key2, d2);
                return d2;

            case ItemType.Decoration:
                string key3 = "Decoration"+"_"+ID.ToString();
                if (_cache.TryGetValue(key3, out var data3))
                {
                    var _d3 = data3.DeepCopy();
                    _d3.Count = count;
                    return _d3;
                }

                WarehouseDecorationStateData d3 = new WarehouseDecorationStateData();
                var da3 = GameConfigDataBase.GetConfigData<DecorationData>(ID, SysDefine.DecorationConfigData);
                d3.DataType = WarehouseBaseDataType.Decoration;
                d3.Count = count;
                d3.Name = da3.name;
                d3.Icon = da3.GetIconSprite();
                _cache.Add(key3, d3);
                return d3;
            case ItemType.Cube:
                string key4 = "Cube"+"_"+ID.ToString();
                if (_cache.TryGetValue(key4, out var data4))
                {
                    var _d4 = data4.DeepCopy();
                    _d4.Count = count;
                    return _d4;
                }

                WarehouseCubeStateData d4 = new WarehouseCubeStateData();
                var da4 = GameConfigDataBase.GetConfigData<CubeData>(ID, SysDefine.CubeConfigData);
                d4.DataType = WarehouseBaseDataType.Cube;
                d4.Count = count;
                d4.Name = da4.name;
                d4.Icon = da4.GetIconSprite();
                _cache.Add(key4, d4);
                return d4;
                
        }
        return null;
    }

    public void SaveAnimal(AnimalStateData animalStateData, bool adult, bool UIChange = true)
    {
        // 委托到新的存储类
        _animalStorage.SaveAnimal(animalStateData, adult, UIChange);

        // 触发事件
        event_manager.instance.dispatch_event(SysDefine.UI_WareHouse_List, new UI_WareHouse_List_EventData()
        {
            type = ItemType.AdultFish,
            num = -1,
        });

        // 更新UI（如果需要）
        if (UIChange && uI_Warehouse && uI_Warehouse.OnShow)
        {
            uI_Warehouse._WareInfo_Animal.listController.AddItem(1);
        }
    }
    public WarehouseAnimalStateData DislodgeFish(int ID)
    {
        // 委托到新的存储类
        var fish = _animalStorage.DislodgeFish(ID);

        if (fish == null)
        {
            Debug.LogError("DislodgeFish NoID:" + ID);
            return null;
        }

        // 触发事件
        event_manager.instance.dispatch_event(SysDefine.UI_WareHouse_List, new UI_WareHouse_List_EventData()
        {
            type = ItemType.AdultFish,
            num = -1,
        });

        return fish;
    }

    public void SaveDecoration(DecorationStateData data, bool UIChange = true)
    {
        // 委托到新的存储类
        _decorationStorage.SaveDecoration(data, UIChange);

        // 触发事件
        event_manager.instance.dispatch_event(SysDefine.UI_WareHouse_List, new UI_WareHouse_List_EventData()
        {
            type = ItemType.Decoration,
            num = 1,
        });

        // 更新UI（如果需要）
        if (UIChange && uI_Warehouse && uI_Warehouse.OnShow)
        {
            uI_Warehouse._WareInfo_Decoration.listController.AddItem(1);
        }
    }
    public WarehouseDecorationStateData DislodgeDecoration(DecorationStateData decorationStateData)
    {
        // 委托到新的存储类
        var decoration = _decorationStorage.DislodgeDecoration(decorationStateData);

        if (decoration == null)
        {
            Debug.LogWarning($"[WarehouseManager] 未找到装饰物: 类型={decorationStateData.Type}");
            return null;
        }

        // 触发事件
        event_manager.instance.dispatch_event(SysDefine.UI_WareHouse_List, new UI_WareHouse_List_EventData()
        {
            type = ItemType.Decoration,
            num = -1,
        });

        return decoration;
    }
    
    public void SaveCube(CubeStateData data, bool UIChange = true)
    {
        // 委托到新的存储类
        _cubeStorage.SaveCube(data, UIChange);

        // 触发事件
        event_manager.instance.dispatch_event(SysDefine.UI_WareHouse_List, new UI_WareHouse_List_EventData()
        {
            type = ItemType.Cube,
            num = 1,
        });

        // 更新UI（如果需要）
        if (UIChange && uI_Warehouse && uI_Warehouse.OnShow &&
            uI_Warehouse._WareInfo_Cube.type == ItemType.Cube)
        {
            uI_Warehouse._WareInfo_Cube.listController.AddItem(1);
        }
    }
    public WarehouseCubeStateData DislodgeCube(CubeStateData cubeStateData)
    {
        // 委托到新的存储类
        var cube = _cubeStorage.DislodgeCube(cubeStateData);

        if (cube == null)
        {
            Debug.LogWarning($"[WarehouseManager] 未找到方块: 类型={cubeStateData.Type}");
            return null;
        }

        // 触发事件
        event_manager.instance.dispatch_event(SysDefine.UI_WareHouse_List, new UI_WareHouse_List_EventData()
        {
            type = ItemType.Cube,
            num = -1,
        });

        // 更新UI（如果需要）
        if (uI_Warehouse && uI_Warehouse.OnShow &&
            uI_Warehouse._WareInfo_Cube.type == ItemType.Cube)
        {
            uI_Warehouse._WareInfo_Cube.listController.RemoveItem(1);
        }

        return cube;
    }
    public void SavePlant(PlantStateData data, bool UIChange = true)
    {
        // 委托到新的存储类
        _plantStorage.SavePlant(data, UIChange);

        // 触发事件
        event_manager.instance.dispatch_event(SysDefine.UI_WareHouse_List, new UI_WareHouse_List_EventData()
        {
            type = ItemType.Plant,
            num = 1,
        });

        // 更新UI（如果需要）
        if (UIChange && uI_Warehouse && uI_Warehouse.OnShow &&
            uI_Warehouse._WareInfo_Plant.type == ItemType.Plant)
        {
            uI_Warehouse._WareInfo_Plant.listController.AddItem(1);
        }
    }

    public WarehousePlantStateData DislodgePlant(int id)
    {
        // 委托到新的存储类
        var plant = _plantStorage.DislodgePlant(id);

        if (plant == null)
        {
            Debug.LogWarning($"[WarehouseManager] 未找到植物: ID={id}");
            return null;
        }

        // 触发事件
        event_manager.instance.dispatch_event(SysDefine.UI_WareHouse_List, new UI_WareHouse_List_EventData()
        {
            type = ItemType.Plant,
            num = -1,
        });

        return plant;
    }

    public void PlaceItem(WarehouseBaseData baseData, int count=-1,bool adult=true,bool sell = false)
    {
        if(count == 0 )
        { return; }
        switch(baseData.DataType)
        {
            case WarehouseBaseDataType.Plant:
                if (_plantStorage.TryGetItem(baseData.WarehouseBaseID, out var Plant))
                {


                    if(sell)
                    {

                        PlayerManager.Instance.OnMoneyChanged(Plant.data.GetPrice());
                        DislodgePlant(baseData.WarehouseBaseID);
                    }
                    else
                    {
                        //发送消息baseData.WarehouseBaseID发送消息 存id Plant.data.name GrowthProgress
                        //uI_Warehouse.CloseOrReturnUIForms();
                        var activeBioTank = BioTankRegistry.Instance.ActiveBioTank;
                        if (activeBioTank == null)
                        {
                            Debug.LogError("[WarehouseManager] 没有活动的生物罐，无法放置植物");
                            return;
                        }

                        // 设置当前建造物品信息（用于金币检查和退出删除模式后恢复预览）
                        if (UI_BioTank_Edit.CurrentInstance != null)
                        {
                            UI_BioTank_Edit.CurrentInstance.currentBuildingItem = new UI_BioTank_Edit.CurrentBuildingItem
                            {
                                itemID = Plant.data.Type,  // Type是string类型
                                blockType = 2,  // Plant类型 (0=Cube, 1=Decoration, 2=Plant)
                                price = 0,  // 植物从仓库取出不收费（已拥有），放置后会从仓库删除
                                objID = Plant.data.ObjID,  // 保存objID用于重新发送CreatePlantCubeCommand
                                growthProgress = Plant.data.GetGrowthProgress(),  // 保存成长进度
                                baseData = baseData
                            };
                            // 设置为Build模式，以便服务器端能响应空格键
                            UI_BioTank_Edit.CurrentInstance.currentMode = BuildControlMode.Build;
                        }
                        else
                        {
                            Debug.LogWarning("[WarehouseManager] UI_BioTank_Edit未打开，无法设置currentBuildingItem");
                            Debug.LogWarning("[WarehouseManager] 这可能导致空格键监听失效。请确保在放置植物前打开了生态箱编辑界面。");
                        }

                        // 修复: 使用activeBioTank的ID作为key,确保与后续CreatePlantCubeCommand使用的clientId一致
                        int clientId = activeBioTank.stateData.ID;
                        Plant.data.ID = clientId;
                        EntityFactory.Instance.SavedPlantData[clientId] = new PlayerManager.WareHouseSavePlantData
                        {
                            plantStateData = Plant.data,
                            WarehouseBaseID = baseData.WarehouseBaseID,
                        };


                        var client = WebSocketServerManager.GetClient(clientId);
                        if (client != null)
                        {
                            client.SendCreateBlockCommand(Plant.data.Type, Plant.data.ObjID, Plant.data.GetGrowthProgress(), 2);
                        }
                        else
                        {
                            Debug.LogError($"[WarehouseManager] 找不到clientId={clientId}的客户端连接！");
                        }
                    }
                }
                break;
            case WarehouseBaseDataType.Fish:
                if (_animalStorage.TryGetValue(baseData.WarehouseBaseID, out var f))
                {
                    var fish = (WarehouseAnimalStateData)f;
                    if (sell)
                    {
                        var fdata = fish.stateData.GetAnimalData();
                        PlayerManager.Instance.OnMoneyChanged(fish.stateData.GetPrice());
                        DislodgeFish(baseData.WarehouseBaseID);
                    }
                    else
                    {
                        fish.stateData.ID = PlayerManager.ActiveBioTanksID;
                        var _f = PlayerManager.Instance.CreateAnimal(fish.stateData);
                        if (_f != null)
                            DislodgeFish(baseData.WarehouseBaseID);
                    }
                }
                break;
            case WarehouseBaseDataType.Cube:
                if (_cubeStorage.TryGetItem(baseData.WarehouseBaseID, out var cube))
                {
                    if (sell)
                    {
                        // Cube是解锁型物品，不支持出售
                        Debug.LogWarning("[WarehouseManager] Cube不支持出售功能");
                        return;
                    }
                    else
                    {
                        // 设置当前建造物品信息（用于金币检查）
                        if (UI_BioTank_Edit.CurrentInstance != null)
                        {
                            UI_BioTank_Edit.CurrentInstance.currentBuildingItem = new UI_BioTank_Edit.CurrentBuildingItem
                            {
                                itemID = cube.stateData.Type,  // Type是string类型
                                blockType = 0,  // Cube类型
                                price = cube.stateData.GetPrice(),
                                baseData = baseData
                            };
                            // 设置为Build模式，以便服务器端能响应空格键
                            UI_BioTank_Edit.CurrentInstance.currentMode = BuildControlMode.Build;
                        }

                        // 发送创建命令到客户端（Cube/Decoration/Plant统一使用SendCreateBlockCommand）
                        // 注意：扣费现在在服务器端检查金币后由客户端执行
                        WebSocketServerManager.GetActiveClient()?.SendCreateBlockCommand(
                            cube.stateData.Type,
                            int.Parse(cube.stateData.Type),
                            0f,
                            0  // BlockType.Cube = 0
                        );

                        // 注意：不删除仓库中的cube，可以重复放置
                    }
                }
                break;
            case WarehouseBaseDataType.Decoration:
                if (_decorationStorage.TryGetItem(baseData.WarehouseBaseID, out var Decoration))
                {
                    if (sell)
                    {
                        // Decoration是解锁型物品，不支持出售
                        Debug.LogWarning("[WarehouseManager] Decoration不支持出售功能");
                        return;
                    }
                    else
                    {
                        // Decoration与Cube处理逻辑完全一致（都是解锁型物品）
                        // 设置当前建造物品信息（用于金币检查）
                        if (UI_BioTank_Edit.CurrentInstance != null)
                        {
                            UI_BioTank_Edit.CurrentInstance.currentBuildingItem = new UI_BioTank_Edit.CurrentBuildingItem
                            {
                                itemID = Decoration.stateData.Type,
                                blockType = 1,  // Decoration类型 (0=Cube, 1=Decoration, 2=Plant)
                                price = Decoration.stateData.GetPrice(),
                                baseData = baseData
                            };
                            // 设置为Build模式，以便服务器端能响应空格键
                            UI_BioTank_Edit.CurrentInstance.currentMode = BuildControlMode.Build;
                        }

                        // 发送创建命令到客户端（使用SendCreateBlockCommand，blockType=1表示Decoration）
                        // 注意：扣费现在在服务器端检查金币后由客户端执行
                        WebSocketServerManager.GetActiveClient()?.SendCreateBlockCommand(
                            Decoration.stateData.Type,
                            int.Parse(Decoration.stateData.Type),
                            0f,
                            1  // BlockType.Decoration = 1
                        );

                        // 注意：不删除仓库中的decoration，可以重复放置（与Cube相同）
                    }
                }
                break;
        }


        
    }

}

