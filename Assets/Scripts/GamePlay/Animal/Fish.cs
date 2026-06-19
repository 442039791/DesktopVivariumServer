using System.Collections.Generic;
using UnityEngine;

// AnimalAgeStatus 已移至共享定义
// 位置: Assets/Scripts/Shared/Network/NetworkEnums.cs

/// <summary>
/// 食物类型
/// </summary>
public enum FoodType
{
    /// <summary>
    /// 素食
    /// </summary>
    White,
    /// <summary>
    /// 肉食
    /// </summary>
    Meat,
    /// <summary>
    /// 指定的植物
    /// </summary>
    SpecificWhite,
    /// <summary>
    /// 指定的肉
    /// </summary>
    SpecificMeat,
}


public partial class Animal :  IStop,IShowAtUI
{



    public AnimalData data;
    public BioTankManager Parent
    {
        get { return BioTankRegistry.Instance.GetBioTank(stateData.ID); }

    }
    public AnimalStateData stateData;
    public Dictionary<string, AnimalStateData> states;
    //public Dictionary<AnimalAgeStatus, List<AnimalDisplay>> animalDisplays= new();
    public Transform AnimalDisplay;
    public bool OnShow { get; set; }
    public int ID{get
        {
            return stateData.ObjID;
        }
        private set { stateData.ObjID = value; }
    }

    //public float AdultNum
    //{
    //    get { return stateData.AdultNum; }
    //    set {
    //        //SetDisplay(AnimalAgeStatus.Adult, (int)(value - stateData.AdultNum));
    //        stateData.AdultNum = value;
    //        if (BioTankManager.uIInfo == null)
    //            return;
    //        if (BioTankManager.uIInfo.OnShow)
    //        {
    //            event_manager.instance.dispatch_UIevent("Animal" + ID, "AdultNum");
    //        }
    //    }
    //}
    /// <summary>
    /// 储备的食物
    /// </summary>
    public float FoodReserve
    {
        get { return stateData.FoodReserve; }
        set { stateData.FoodReserve = value; }
    }
    //public float JuvenileNum
    //{
    //    get
    //    {
    //        return stateData.JuvenileNum;
    //    }
    //    set
    //    {

    //        //SetDisplay(AnimalAgeStatus.Juvenile, (int)(value - stateData.JuvenileNum));
    //        stateData.JuvenileNum = value;
    //        if (BioTankManager.uIInfo == null)
    //            return;
    //        if (BioTankManager.uIInfo.OnShow)
    //        {
    //            event_manager.instance.dispatch_UIevent("Animal" + ID, "JuvenileNum");
    //        }
    //    }
    //}
    //public float AdultDeath
    //{
    //    get
    //    {
    //        return stateData.AdultDeath;
    //    }
    //    set
    //    {
    //        stateData.AdultDeath = value;
    //        if (BioTankManager.uIInfo == null)
    //            return;
    //        if (BioTankManager.uIInfo.OnShow)
    //        {
    //            event_manager.instance.dispatch_UIevent("Animal" + ID, "DeadFish");
    //        }
    //    }
    //}
    //public float JuvenileDeath
    //{
    //    get
    //    {
    //        return stateData.JuvenileDeath;
    //    }
    //    set
    //    {
    //        stateData.JuvenileDeath = value;
    //        if (BioTankManager.uIInfo == null)
    //            return;
    //        if (BioTankManager.uIInfo.OnShow)
    //        {
    //            event_manager.instance.dispatch_UIevent("Animal" + ID, "DeadFish");
    //        }
    //    }
    //}
    ///<summary>
    ///鱼的状态
    /// </summary>
    public AnimalAgeStatus State
    {
        get { return stateData.State; }
        set { stateData.State = value; }
    }

    ///<summary>
    ///品质,小于等于1为白色、小于等于2为蓝色，以此类推
    /// </summary>
    public float Quality
    {
        get { return stateData.Quality; }
        set { stateData.Quality = value; }
    }
    
    public ObjectQuality FishObjectQuality
    {
        get
        {
            return stateData.GetObjectQuality();
        }
    }

    

    ///<summary>
    ///拥有的特质列表
    /// </summary>
    public List<Feature> FeatureList
    {
        get { return stateData.FeatureList; }
        set { stateData.FeatureList = value; }
    }

    ///<summary>
    /// 怀孕进度，当达到这种鱼的剩余时间后将会生出鱼
    /// </summary>
    public float ReproductionProgress
    {
        get { return stateData.ReproductionProgress; }
        set { stateData.ReproductionProgress = value; }
    }

    ///<summary>
    ///鱼的血量
    /// </summary>
    public float HP
    {
        get { return stateData.HP; }
        set { stateData.HP = value; }
    }
    ///<summary>
    ///检测这条鱼能不能回血
    /// </summary>
    bool Restore;

    ///<summary>
    ///成长的进度
    /// </summary>
    public float GrowUpNum
    {
        get { return stateData.GrowUpNum; }
        set { stateData.GrowUpNum = value; }
    }



    /// <summary>
    /// 更新鱼，主要更新以下信息：1、生产与消耗；2、繁衍
    /// </summary>
    /// <param name="IntervalTime">距离上次间隔的时间，单位为分钟</param>
    public void StateUpdate(float IntervalTime)
    {
        //氧气消耗
        ConsumeO2(IntervalTime);
        //二氧化碳产生
        ProduceCO2(IntervalTime);
        //废物产生
        ProduceWaste(IntervalTime);
        //食物消耗
        ConsumeFood(IntervalTime);
        //成长，改成每秒更新了
        //GrowUp(IntervalTime);

        //鱼死亡
        // 保存之前的状态,用于检测是否需要通知客户端
        AnimalAgeStatus previousState = State;

        if (State == AnimalAgeStatus.Adult && HP <= 0)
        {
            State = AnimalAgeStatus.AdultDeath;
            HP = 0;
            FoodReserve = 0;
        }
        else if (State == AnimalAgeStatus.Juvenile && HP <= 0)
        {
            State= AnimalAgeStatus.JuvenileDeath;
            HP = 0;
            FoodReserve = 0;
        }

        // 如果鱼从活鱼变成死鱼,通知客户端执行死亡效果
        bool wasPreviouslyAlive = (int)previousState >= 2; // Adult=2, Juvenile=3
        bool isNowDead = (int)State < 2; // AdultDeath=0, JuvenileDeath=1
        if (wasPreviouslyAlive && isNowDead)
        {
            // 使用动物所属的生态箱ID（stateData.ID）而不是活动生态箱
            // 修复：多生态箱时消息错误发送给活动生态箱导致第一个客户端卡顿的问题
            WebSocketServerManager.GetClient(stateData.ID)?.SendFishDeathCommand(ID);
        }
        //空间不足时该怎么办？后面说

        //回血
        if (Restore && HP < 100)
        {
            HP += IntervalTime;
            if (HP > 100)
            {
                HP = 100;
            }
        }

        Restore = true;
    }
    public void Init(AnimalStateData stateData,bool display=true)
    {
        this.stateData = stateData;
        //transform.parent = GlobalSetting.Instance.AnimalNode;
        //transform.localPosition = stateData.Position;
        //idHolder??=transform.AddComponent<IDHolder>();
        data = GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(stateData.Type), SysDefine.AnimalConfigData);

        Parent?.TryAddAnimal(this);
        if(display)
        SetDisplay(State, stateData.ObjID/*,stateData.Quality*/);
        //switch (stateData.State)
        //{

        //}
        //if(stateData.State==AnimalAgeStatus.Adult)
        //{
        //    SetDisplay(AnimalAgeStatus.Adult, stateData.ObjID);
        //}
        //if((int)stateData.AdultNum>0)
        //{
        //    SetDisplay(AnimalAgeStatus.Adult, stateData.ObjID);
        //}
        //if ((int)stateData.JuvenileNum > 0)
        //{
        //    SetDisplay(AnimalAgeStatus.Juvenile, stateData.ObjID);
        //}
        
    }

    /// <summary>
    /// 消耗氧气
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ConsumeO2(float IntervalTime)
    {
        //活鱼的氧气消耗
        if (State == AnimalAgeStatus.Adult)
        {
            Parent.OnO2Changed(-data.adultO2Cost  * IntervalTime);
        }
        else if (State == AnimalAgeStatus.Juvenile)
        {
            Parent.OnO2Changed(-data.juvenileO2Cost * IntervalTime);
        }
        //死鱼的氧气消耗
        else if (State == AnimalAgeStatus.AdultDeath)
        {
            Parent.OnO2Changed(-data.adultDeathO2Cost * IntervalTime);
        }
        else
        {
            Parent.OnO2Changed(-data.juvenileDeathO2Cost * IntervalTime);
        }

        //如果当前氧气含量低于鱼所需的含量，则开始死鱼
        if (State== AnimalAgeStatus.Adult&& Parent.O2Proportion() < data.o2MinProportion)
        {
            float proportion = 1 - Parent.O2Proportion() / data.o2MinProportion;
            HP -= proportion * IntervalTime / data.adultO2Death * 100;
            Restore=false;
        }
        else if (State == AnimalAgeStatus.Juvenile && Parent.O2Proportion() < data.o2MinProportion)
        {
            float proportion = 1 - Parent.O2Proportion() / data.o2MinProportion;
            HP -= proportion * IntervalTime / data.juvenileO2Death * 100;
            Restore = false;
        }
    }
    /// <summary>
    /// 消耗食物
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ConsumeFood(float IntervalTime)
    {
        if (State == AnimalAgeStatus.Adult)
        {
            float num= data.adultFoodCost * IntervalTime;
            if (num> FoodReserve)
            {
                float proportion=1- FoodReserve /num;
                HP -= proportion * IntervalTime / data.adultFoodDeath*100;
                FoodReserve = 0;
                Restore = false;
            }
            else
            {
                FoodReserve -= num;
            }
        }
        if (State == AnimalAgeStatus.Juvenile)
        {
            float num =  data.juvenileFoodCost * IntervalTime;
            if (num > FoodReserve)
            {
                float proportion = 1 - FoodReserve / num;
                HP -= proportion * IntervalTime / data.juvenileFoodDeath * 100;
                FoodReserve = 0;
                Restore = false;
            }
            else
            {
                FoodReserve -= num;
            }
        }
        PlayerManager.Instance.GetBioTank(stateData.ID)?.ReFreshFood();
    }
    /// <summary>
    /// 产生二氧化碳
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ProduceCO2(float IntervalTime)
    {
        if (State == AnimalAgeStatus.Adult)
        {
            Parent.OnCO2Changed(data.adultCO2Produce  * IntervalTime);
        }
        else if (State == AnimalAgeStatus.Juvenile)
        {
            Parent.OnCO2Changed(data.juvenileCO2Produce  * IntervalTime);
        }
        else if (State == AnimalAgeStatus.AdultDeath)
        {
            Parent.OnCO2Changed(data.adultDeathCO2Produce * IntervalTime);
        }
        else
        {
            Parent.OnCO2Changed(data.juvenileDeathCO2Produce * IntervalTime);
        }
        //如果当前二氧化碳高于鱼所需的含量，则开始死鱼
        if (State == AnimalAgeStatus.Adult && Parent.CO2Proportion() > data.co2MaxProportion)
        {
            float proportion = 1 - Parent.CO2Proportion() / data.co2MaxProportion;
            HP -= proportion * IntervalTime / data.adultCO2Death * 100;
            Restore = false;
        }
        else if (State == AnimalAgeStatus.Juvenile && Parent.CO2Proportion() > data.co2MaxProportion)
        {
            float proportion = 1 - Parent.CO2Proportion() / data.co2MaxProportion;
            HP -= proportion * IntervalTime / data.juvenileCO2Death * 100;
            Restore = false;
        }
    }
    /// <summary>
    /// 产生垃圾
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ProduceWaste(float IntervalTime)
    {
        //活鱼产生的垃圾
        if (State == AnimalAgeStatus.Adult)
        {
            Parent.OnWasteChanged(data.adultWasteProduce  * IntervalTime);
        }
        else if (State == AnimalAgeStatus.Juvenile)
        {
            Parent.OnWasteChanged(data.juvenileWasteProduce  * IntervalTime);
        }
        //死鱼产生的垃圾
        else if (State == AnimalAgeStatus.AdultDeath)
        {
            Parent.OnWasteChanged(data.adultDeathWasteProduce * IntervalTime);
        }
        else
        {
            Parent.OnWasteChanged(data.juvenileDeathWasteProduce * IntervalTime);
        }

        //如果当前垃圾高于鱼所需的含量，则开始死鱼
        if (State == AnimalAgeStatus.Adult && Parent.WasteProportion() > data.wasteMaxProportion)
        {
            float proportion = 1 - Parent.WasteProportion() / data.wasteMaxProportion;
            HP -= proportion * IntervalTime / data.adultWasteDeath * 100;
            Restore = false;
        }
        else if (State == AnimalAgeStatus.Juvenile && Parent.WasteProportion() > data.wasteMaxProportion)
        {
            float proportion = 1 - Parent.WasteProportion() / data.wasteMaxProportion;
            HP -= proportion * IntervalTime / data.juvenileWasteDeath * 100;
            Restore = false;
        }

    }
    /// <summary>
    /// 获取氧气1小时数据的变化量
    /// </summary>
    public float GetO2HourChange()
    {
        float num = 0;
        //活鱼的氧气消耗
        if (State == AnimalAgeStatus.Adult)
        {
            num-=data.adultO2Cost  * 60;
        }
        else if (State == AnimalAgeStatus.Juvenile)
        {
            num -= data.juvenileO2Cost * 60;
        }
        //死鱼的氧气消耗
        else if (State == AnimalAgeStatus.AdultDeath)
        {
            num -= data.adultDeathO2Cost * 60;
        }
        else
        {
            num -= data.juvenileDeathO2Cost * 60;
        }
        return num;
    }
    /// <summary>
    /// 获取二氧化碳1小时数据的变化量
    /// </summary>
    public float GetCO2HourChange()
    {
        float num = 0;
        //活鱼产生二氧化碳
        if (State == AnimalAgeStatus.Adult)
        {
            num+=data.adultCO2Produce * 60;
        }
        else if (State == AnimalAgeStatus.Juvenile)
        {
            num += data.juvenileCO2Produce * 60;
        }
        //死鱼的氧气消耗
        else if (State == AnimalAgeStatus.AdultDeath)
        {
            num += data.adultDeathCO2Produce * 60;
        }
        else
        {
            num += data.juvenileDeathCO2Produce * 60;
        }
        return num;
    }
    /// <summary>
    /// 获取垃圾1小时数据的变化量
    /// </summary>
    public float GetWasteHourChange()
    {
        float num = 0;
        //活鱼产生的垃圾
        if (State == AnimalAgeStatus.Adult)
        {
            num+=data.adultWasteProduce * 60;
        }
        else if (State == AnimalAgeStatus.Juvenile)
        {
            num+=data.juvenileWasteProduce * 60;
        }
        //死鱼产生的垃圾
        else if (State == AnimalAgeStatus.AdultDeath)
        {
            num += data.adultDeathWasteProduce * 60;
        }
        else
        {
            num += data.juvenileDeathWasteProduce * 60;
        }
        return num;
    }
    /// <summary>
    /// 获取食物1小时数据的变化量
    /// </summary>
    public float GetFoodHourChange()
    {
        if (State == AnimalAgeStatus.Adult)
        {
            return data.adultFoodCost * 60;
        }
        if (State == AnimalAgeStatus.Juvenile)
        {
            return data.juvenileFoodCost * 60;
        }
        return 0;
    }
    public Vector3 GetRandomPointInCuboid(Vector3 startPoint, float width, float height, float depth)
    {
        // 生成随机 x, y, z 值
        float randomX = UnityEngine.Random.Range(startPoint.x, startPoint.x + width);
        float randomY = UnityEngine.Random.Range(startPoint.y, startPoint.y + height);
        float randomZ = UnityEngine.Random.Range(startPoint.z, startPoint.z + depth);

        // 返回随机坐标
        return new Vector3(randomX, randomY, randomZ);
    }

    /// <summary>
    /// 幼年成长为成年
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void GrowUp(float IntervalTime)
    {
        if (State == AnimalAgeStatus.Juvenile)
        {
            // 健康值不满时暂停成长
            if (HP < 100)
            {
                return;
            }
            GrowUpNum += IntervalTime;
            if (GrowUpNum>= data.adultTime*60)
            {
                State= AnimalAgeStatus.Adult;
                // 使用动物所属的生态箱ID（stateData.ID）而不是活动生态箱
                // 修复：多生态箱时消息错误发送给活动生态箱导致第一个客户端卡顿的问题
                WebSocketServerManager.GetClient(stateData.ID)?.SendAnimalGrowthCommand(ID, 1);
                Parent.SortAndRefreshAnimal();
            }
        }
    }

    public void Destroy()
    {
        Parent.RemoveAnimal(this);
        //foreach (var item in animalDisplays.Values)
        //{
            RemoveAnimalDisPlay();
        //}
        //TODO:
        //GameObjectPool.Instance.CollectObject(this.gameObject);
        //
        event_manager.instance.remove_UIevent_listener("Animal" + ID);
    }
    //public List<FishDisplay> GetFishDisplays(AnimalAgeStatus fishAgeStatus)
    //{
    //    if (!fishDisplays.ContainsKey(fishAgeStatus))
    //    {
    //        fishDisplays[fishAgeStatus] =new List<FishDisplay>();
    //    }
    //    return fishDisplays[fishAgeStatus];
    //}
    //public void AddFishDisplay(List<FishDisplay> list, int num, AnimalAgeStatus fishAgeStatus,FishData data)
    //{
    //    for(int i = 0; i < num; i++)
    //    {
    //        var g = GameObjectPool.Instance.CreateObject(SysDefine.FishDisPlayPrefabName, ResMgr.Instance.FishDisPlayPrefab);
    //        g.transform.parent = this.FishDisplay;
    //        var v= GetRandomPointInCuboid(Parent.land._info.transform.position, Parent.land.xMax, Parent.land.yMax, Parent.land.zMax);
    //        g.transform.position = v;
    //        var dis = g.GetComponent<FishDisplay>();
    //        dis.Init(data, fishAgeStatus);
    //        list.Add(dis);
    //    }
    //}
    public void RemoveAnimalDisPlay()
    {
        // 使用动物所属的生态箱ID（stateData.ID）而不是活动生态箱ID
        // 修复：多生态箱时消息错误发送给活动生态箱导致第一个客户端卡顿的问题
        switch (State)
        {
            case AnimalAgeStatus.Adult:
                WebSocketServerManager.GetClient(stateData.ID)?.SendRemoveAnimalDisplay(ID, AnimalAgeStatus.Adult, stateData.Type);
                break;
            case AnimalAgeStatus.Juvenile:
                WebSocketServerManager.GetClient(stateData.ID)?.SendRemoveAnimalDisplay(ID, AnimalAgeStatus.Juvenile, stateData.Type);
                break;
            case AnimalAgeStatus.AdultDeath:
                WebSocketServerManager.GetClient(stateData.ID)?.SendRemoveAnimalDisplay(ID, AnimalAgeStatus.Adult, stateData.Type);
                break;
            case AnimalAgeStatus.JuvenileDeath:
                WebSocketServerManager.GetClient(stateData.ID)?.SendRemoveAnimalDisplay(ID, AnimalAgeStatus.Juvenile, stateData.Type);
                break;
        }

        //if (AdultNum > 0)
        //    WebSocketServerManager.GetClient(PlayerManager.ActiveBioTanksID).SendRemoveAnimalDisplay(ID, AnimalAgeStatus.Adult, stateData.Type);
        //if (JuvenileNum > 0)
        //{
        //    WebSocketServerManager.GetClient(PlayerManager.ActiveBioTanksID).SendRemoveAnimalDisplay(ID, AnimalAgeStatus.Juvenile, stateData.Type);
        //}
        //if(list.Count == 0)
        //{ return; }
        //if(list.Count-num<0)
        //{
        //    num = list.Count - 0;
        //}
        //List<FishDisplay> l = new List<FishDisplay>();
        //foreach (FishDisplay dis in list)
        //{
        //    if(dis.gameObject.activeInHierarchy)
        //    {
        //        l.Add(dis);
        //        --num;
        //        if(num<=0)
        //        {
        //            break;
        //        }
        //    }
        //}
        //foreach (FishDisplay dis in l)
        //{
        //    GameObjectPool.Instance.CollectObject(dis.gameObject);
        //    list.Remove(dis);
        //}
    }
    /// <summary>
    /// 修改显示的动物群
    /// </summary>
    /// <param name="animalAgeStatus">对哪种类型的动物进行操作。</param>
    ///     /// <param name="num">操作的数量，正数是增加，负数是减少，增加或者尖减少指定的数量。</param>
    public void SetDisplay(AnimalAgeStatus animalAgeStatus,int objid)
    {
        // 使用动物所属的生态箱ID（stateData.ID）而不是活动生态箱ID
        // 修复：多生态箱时消息错误发送给活动生态箱导致第一个客户端卡顿的问题
        switch (animalAgeStatus)
        {
            case AnimalAgeStatus.Adult:
                WebSocketServerManager.GetClient(stateData.ID)?.SendAddAnimalDisplay(objid, animalAgeStatus, stateData.Type,stateData.GetObjectQuality());
                break;
            case AnimalAgeStatus.Juvenile:
                WebSocketServerManager.GetClient(stateData.ID)?.SendAddAnimalDisplay(objid, animalAgeStatus, stateData.Type, stateData.GetObjectQuality());
                break;
        }
    }
    public void StopChange(bool s)
    {
        
    }
}
