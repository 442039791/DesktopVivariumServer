using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生态缸管理器 - 重构后使用EntityCollection
/// </summary>
public partial class BioTankManager
{
    // 使用EntityCollection替代双重列表设计
    public EntityCollection<Animal> AnimalCollection { get; private set; }
    public EntityCollection<Plant> PlantCollection { get; private set; }
    public EntityCollection<Decoration> DecorationCollection { get; private set; }

    // 保留字典属性以兼容现有代码（将逐步移除）
    [System.Obsolete("请使用AnimalCollection代替")]
    public Dictionary<int,Animal> animalList => ConvertToDictionary(AnimalCollection);

    [System.Obsolete("请使用PlantCollection代替")]
    public Dictionary<int, Plant> plantList => ConvertToDictionary(PlantCollection);

    [System.Obsolete("请使用DecorationCollection代替")]
    public Dictionary<int,Decoration> decorationList => ConvertToDictionary(DecorationCollection);

    public BioTankStateData stateData;
    public static UI_BioTankInfo uIInfo;

    private Dictionary<int, T> ConvertToDictionary<T>(EntityCollection<T> collection) where T : class
    {
        var dict = new Dictionary<int, T>();
        foreach (var entity in collection.Entities)
        {
            var animal = entity as Animal;
            var plant = entity as Plant;
            var decoration = entity as Decoration;

            int id = animal?.ID ?? plant?.ID ?? decoration?.ID ?? 0;
            dict[id] = entity;
        }
        return dict;
    }
    public delegate void SortFunc();
    private static Dictionary<AnimalSortType, SortFunc>  animalSortFuncDict ;
    public AnimalSortType CurrentAnimalSortType = AnimalSortType.Default;
    private static Dictionary<PlantSortType, SortFunc> plantSortFuncDict;
    public PlantSortType CurrentPlantSortType = PlantSortType.Default;
    private static CustomClassSorter customClassSorter = new CustomClassSorter();
    /// <summary>
    /// 销毁生态缸，清理所有实体
    /// </summary>
    public void Destroy()
    {
        // 销毁所有动物
        AnimalCollection.ForEach(animal => animal.Destroy());
        AnimalCollection.Clear();

        // 销毁所有植物
        PlantCollection.ForEach(plant => plant.Destroy());
        PlantCollection.Clear();

        // 销毁所有装饰物
        DecorationCollection.ForEach(decoration => decoration.Destroy());
        DecorationCollection.Clear();

        stateData = null;
    }
    public float GetMaxVolume()
    {
        return stateData.area_x* stateData.area_y* stateData.area_z /1000;
    }
    //public int ID
    //{
    //    get
    //    {
    //        return idHolder.UniqueID;
    //    }
    //    private set { idHolder.UniqueID = value; }
    //}
    //public IDHolder idHolder;


    public void RefreshBitTankInfo()
    {
        // 改为UI主动拉取模式，此方法保留但不再主动推送
    }

    /// <summary>
    /// 强制刷新所有UI数据 - 已改为UI主动拉取模式
    /// </summary>
    public void ForceRefreshAllUIData()
    {
        // 改为UI主动拉取模式，此方法不再需要
    }


    public void FoodVegetableChange()
    {
        // 改为UI主动拉取模式，此方法不再需要
    }
    public void FoodMeatChange()
    {
        // 改为UI主动拉取模式，此方法不再需要
    }
    public BaseInfo_info GetBaseInfo_info(string ratio,string ratio2, string hour=null)
    {
        return new BaseInfo_info {
            Ratio = ratio + "/" + ratio2,
            Hour = hour == null ? "" : hour + "/h" 
        };
    }
    /// <summary>
    /// 初始化生态缸
    /// </summary>
    public void Init(BioTankStateData Data, bool Hasland = true)
    {
        // 初始化EntityCollection
        AnimalCollection = new EntityCollection<Animal>(animal => animal.ID);
        PlantCollection = new EntityCollection<Plant>(plant => plant.ID);
        DecorationCollection = new EntityCollection<Decoration>(decoration => decoration.ID);

        // 初始化排序函数字典
        animalSortFuncDict ??= new Dictionary<AnimalSortType, SortFunc>()
        {
            { AnimalSortType.Default, SortAnimalDefault },
        };

        plantSortFuncDict ??= new Dictionary<PlantSortType, SortFunc>()
        {
            { PlantSortType.Default, SortPlantDefault },
        };

        stateData = Data;
    }
    //public void InitLand()
    //{
    //    stateData.Volume = land.xMax * land.yMax * land.zMax / 1000;
    //}
    public void CreateAnimal(string data)
    {
        //ResMgr.Instance.LoadAssetBundleAsync("AnimalData", () =>
        //{
        //    var animaldata = (AnimalData)ResMgr.Instance.GetAssetCache(data+"Data",null);
        //    var animalprefab=(GameObject)ResMgr.Instance.GetAssetCache(data , null);
        //    var animal = GameObjectPool.Instance.CreateObject(data, animalprefab);
        //    var f = animal.GetComponent<Animal>();
        //    f.Init(new()
        //    {
        //        Data = animaldata,
        //        Parent=this,
        //    });
        //    animalList.Add(f);
        //});
    }
    public BaseInfo_info GetBaseInfo_infoVal(string name)
    {
        switch (name)
        {
            case "O2":
                return GetBaseInfo_info(((int)stateData.O2).ToString(), ((int)GetO2Max()).ToString(), ((int)GetO2HourChange()).ToString());
            case "CO2":
                return GetBaseInfo_info(((int)stateData.CO2).ToString(), ((int)GetCO2Max()).ToString(), ((int)GetCO2HourChange()).ToString());
            case "Waste":
                return GetBaseInfo_info(((int)stateData.Waste).ToString(), ((int)GetWasteMax()).ToString(), ((int)GetWasteHourChange()).ToString());
            case "Vegetable":
                GetFoodData(out var whiteNum, out var whiteMaxNum, out var meatNum, out var meatMaxNum, out var specificWhiteDIC, out var specificWhiteMaxDIC, out var specificMeatDIC, out var specificMeatMaxDIC);
                GetFoodHourChange(out float whiteNum_, out float meatNum_, out Dictionary<string, float> specificWhiteDIC_, out Dictionary<string, float> specificMeatDIC_);
                return GetBaseInfo_info(((int)whiteNum).ToString(), ((int)whiteMaxNum).ToString(), ((int)whiteNum_).ToString());

            case "Meat":
                GetFoodData(out var whiteNum1, out var whiteMaxNum1, out var meatNum1, out var meatMaxNum1, out var specificWhiteDIC1, out var specificWhiteMaxDIC1, out var specificMeatDIC1, out var specificMeatMaxDIC1);
                GetFoodHourChange(out float whiteNum_1, out float meatNum_1, out Dictionary<string, float> specificWhiteDIC_1, out Dictionary<string, float> specificMeatDIC_1);
                return GetBaseInfo_info(((int)meatNum1).ToString(), ((int)meatMaxNum1).ToString(), ((int)meatNum_1).ToString());
            case "PlantNutrition":
                return GetBaseInfo_info(((int)stateData.Nutrient).ToString(), ((int)GetNutrientMax()).ToString(), ((int)GetNutrientHourChange()).ToString());

            //case "Size":
            //    return GetBaseInfo_info(((int)stateData.Volume).ToString(), ((int)(land.xMax * land.yMax * land.zMax / 1000)).ToString(), null);

        }
        return null;
    }
    public void ChangeValue(string v)
    {
        switch (v)
        {
            case "O2":
                AddO2Behavior();
                //PlayerManager.Instance.OnMoneyChanged();
                break;
            case "CO2":
                RemoveCO2Behavior();
                break;
            case "Waste":
                RemoveWasteBehavior();
                break;
            case "Vegetable":
                FeedingWhiteBehavior(10);
                FoodVegetableChange();
                break;
            case "Meat":
                FeedingMeatBehavior(10);
                FoodMeatChange();
                break;
            case "PlantNutrition":
                AddNutrientBehavior();
                break;
            case "Size":
                break;

        }
    }
    //public void TryAddAnimal(AnimalStateData animalState,int adultNum,int JuvenileNum)
    //{
    //    if (animalList.TryGetValue(int.Parse(animalState.Type), out var _animal))
    //    {
    //        _animal.AdultNum += adultNum;

    //        _animal.JuvenileNum += JuvenileNum;

    //        _animal.JuvenileDeath += animalState.JuvenileDeath;
    //        _animal.AdultDeath += animalState.AdultDeath;
    //    }
    //    else
    //    {
    //        animalState.AdultNum = adultNum;
    //        animalState.JuvenileNum = JuvenileNum;
    //        PlayerManager.Instance.CreateAnimal(animalState);
    //    }
    //}

    public void TryAddAnimal(Animal animal)
    {
       //if( animalList.TryGetValue(int.Parse(animal.stateData.Type), out var _animal))
       // {
       //     _animal.AdultNum += animal.AdultNum;

       //     _animal.JuvenileNum += animal.JuvenileNum;

       //     _animal.JuvenileDeath += animal.JuvenileDeath;
       //     _animal.AdultDeath += animal.AdultDeath;
       // }
       //else
       // {
            AddAnimal(animal);
        //}
    }
    /// <summary>
    /// 添加动物
    /// </summary>
    public void AddAnimal(Animal animal)
    {
        AnimalCollection.Add(animal);
        SortAnimal();

        if (uIInfo == null || !uIInfo.OnShow)
        {
            return;
        }

        uIInfo._CreatureInfo.listController.AddItem(1);
        uIInfo.Refresh();
        ReFreshFood();
        event_manager.instance.dispatch_UIevent(SysDefine.UI_BioTankInfo_BaseInfo_info_Size, null);
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

    private void SortAnimalDefault()
    {
        AnimalCollection.Sort((a, b) =>
        {
            int ageSort = customClassSorter.Compare(a.State, b.State);
            if (ageSort != 0) return ageSort;
            int qualitySort = b.stateData.GetObjectQuality().CompareTo(a.stateData.GetObjectQuality());
            if (qualitySort != 0) return qualitySort;
            return b.data.Sort.CompareTo(a.data.Sort);
        });
    }

    private void SortPlantDefault()
    {
        PlantCollection.Sort((a, b) =>
        {
            float growthA = a.GrowthProgress;
            float growthB = b.GrowthProgress;
            bool matureA = growthA >= 1f;
            bool matureB = growthB >= 1f;

            // 大于等于100%的植物，优先按Sort参数升序排序（数字越小越靠前）
            if (matureA && matureB)
            {
                int sortCompare = a.data.Sort.CompareTo(b.data.Sort);
                if (sortCompare != 0) return sortCompare;

                // Sort相同再按成长值降序，保证稳定
                return growthB.CompareTo(growthA);
            }

            // 只有一方达到100%，直接将已满成长的排前面
            if (matureA != matureB) return matureA ? -1 : 1;

            // 其余情况按成长度排序，越高越靠前
            int growthCompare = growthB.CompareTo(growthA);
            if (growthCompare != 0) return growthCompare;

            // 兜底按配置Sort升序排序保证稳定性（数字越小越靠前）
            return a.data.Sort.CompareTo(b.data.Sort);
        });
    }

    public void ReFreshFood()
    {
        GetFoodData(out var whiteNum, out var whiteMaxNum, out var meatNum, out var meatMaxNum, out var specificWhiteDIC, out var specificWhiteMaxDIC, out var specificMeatDIC, out var specificMeatMaxDIC);
        GetFoodHourChange(out float whiteNum_, out float meatNum_, out Dictionary<string, float> specificWhiteDIC_, out Dictionary<string, float> specificMeatDIC_);
        event_manager.instance.dispatch_UIevent(stateData.ID + SysDefine.UI_BioTankInfo_BaseInfo_info_Vegetable, GetBaseInfo_info(((int)whiteNum).ToString(), ((int)whiteMaxNum).ToString(), ((int)whiteNum_).ToString()));
    }
    /// <summary>
    /// 添加植物
    /// </summary>
    public void AddPlant(Plant plant)
    {
        PlantCollection.Add(plant);

        if (uIInfo == null || !uIInfo.OnShow)
        {
            return;
        }

        uIInfo._PlantInfo.listController.AddItem(1);
        uIInfo.Refresh();
    }

    /// <summary>
    /// 添加装饰物
    /// </summary>
    public void AddDecoration(Decoration decoration)
    {
        DecorationCollection.Add(decoration);

        if (uIInfo == null || !uIInfo.OnShow)
        {
            return;
        }

        uIInfo.Refresh();
    }

    /// <summary>
    /// 移除装饰物
    /// </summary>
    public void RemoveDecoration(Decoration decoration)
    {
        DecorationCollection.Remove(decoration);

        if (uIInfo == null || !uIInfo.OnShow)
        {
            return;
        }

        uIInfo.Refresh();
    }

    /// <summary>
    /// 移除动物
    /// </summary>
    public void RemoveAnimal(Animal animal)
    {
        AnimalCollection.Remove(animal);
        SortAnimal();

        if (uIInfo == null || !uIInfo.OnShow)
        {
            return;
        }

        uIInfo._CreatureInfo.listController.RemoveItem(1);
        uIInfo.Refresh();
        ReFreshFood();
        event_manager.instance.dispatch_UIevent(SysDefine.UI_BioTankInfo_BaseInfo_info_Size, null);
    }

    /// <summary>
    /// 移除植物
    /// </summary>
    public void RemovePlant(Plant plant)
    {
        PlantCollection.Remove(plant);

        if (uIInfo == null || !uIInfo.OnShow)
        {
            return;
        }

        uIInfo._PlantInfo.listController.RemoveItem(1);
        uIInfo.Refresh();
    }
    public void SortAndRefreshAnimal()
    {
        SortAnimal();
        if (uIInfo == null)
            return;
        if (!uIInfo.OnShow)
            return;

        uIInfo._CreatureInfo.listController.Refresh();
        uIInfo.Refresh();
    }
    /// <summary>
    /// 上次更新的间隔时间
    /// </summary>
    int currentTime = 0;

    /// <summary>
    /// 更新生态缸，包括生态缸下的各个动物
    /// </summary>
    public void StateUpdate(float IntervalTime)
    {
        // 如果生态箱暂停了，则跳过所有更新
        if (stateData.IsPaused)
            return;

        currentTime+= (int)IntervalTime;
        if (currentTime >= 60)
        {
            // 动物消耗资源
            AnimalCollection.ForEach(animal => animal.StateUpdate(IntervalTime));

            // 植物更新
            PlantCollection.ForEach(plant => plant.StateUpdate(IntervalTime));

            // 素食动物吃饭
            WhiteEat(IntervalTime);

            // 肉食动物吃饭
            MeatEat();

            // 动物受孕
            AnimalConception(IntervalTime);

            // 设备处理
            FacilityManager.Instance.FacilitiesUpdate(stateData.ID, this);

            currentTime = 0;
        }

        // 更新动植物的成长进度
        AnimalCollection.ForEach(animal => animal.GrowUp(IntervalTime));
        PlantCollection.ForEach(plant => plant.GrowUp(IntervalTime));
    }

    /// <summary>
    /// 设置生态箱暂停状态
    /// </summary>
    /// <param name="paused">是否暂停</param>
    public void SetPaused(bool paused)
    {
        stateData.IsPaused = paused;
    }

    /// <summary>
    /// 获取生态箱暂停状态
    /// </summary>
    public bool IsPaused => stateData.IsPaused;
}

