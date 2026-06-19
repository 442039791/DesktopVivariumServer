using System.Collections.Generic;
using UnityEngine;

public class Plant : IStop, IShowAtUI
{
    public PlantData data;
    public Color32 deathColor = new Color32(207, 145, 145, 255);
    public Color32 liveColor = Color.white;
    public float BaseSize;
    public BioTankManager Parent
    {
        get { return BioTankRegistry.Instance.GetBioTank(stateData.ID); }

    }
    public PlantStateData stateData;
    public SpriteRenderer foregroundSprite;
    public SpriteRenderer midgroundSprite;
    public SpriteRenderer backgroundSprite;
    public List<Sprite> sprites = new ();
    public float lastGrowthProgress;
    public bool lastDeahState;
    public bool OnShow { get; set; }
    private int id;
    public int ID
    {
        get
        {
            return stateData.ObjID;
        }
        //set { id = value; }
    }
    public bool AsDeath
    {
        get { return stateData.AsDeath; }
        set { stateData.AsDeath = value; }
    }
    //public IDHolder idHolder;
    public float GrowthProgress
    {
        get { return stateData.GrowthProgress/ (data.AdultTime* 60); }
    }
    public float Hp
    {
        get { return stateData.HP; }
        set { stateData.HP = value; }
    }
    public void Init(PlantStateData stateData/*,SpriteRenderer foregroundSprite, SpriteRenderer midgroundSprite, SpriteRenderer backgroundSprite*/)
    {
        //ID = stateData.ObjID;
        this.stateData = stateData;
        data = GameConfigDataBase.GetConfigData<PlantData>(int.Parse(stateData.Type), SysDefine.PlantConfigData);

        var bioTank = BioTankRegistry.Instance.GetBioTank(stateData.ID);
        bioTank?.AddPlant(this);
        //this.foregroundSprite = foregroundSprite;
        //this.midgroundSprite = midgroundSprite;
        //this.backgroundSprite = backgroundSprite;
        //foregroundSprite.gameObject.transform.localPosition = new Vector3(0, -0.8f, 0);
        //midgroundSprite.gameObject.transform.localPosition = new Vector3(0, -0.8f, 0);
        //backgroundSprite.gameObject.transform.localPosition = new Vector3(0, -0.8f, 0);
        lastGrowthProgress = GrowthProgress;
        lastDeahState = stateData.AsDeath;
        lastGrowthProgressState = GetGrowthProgressState();
        GrowthProgressState = GetGrowthProgressState();
        wasMature = GrowthProgress >= 1f; // 初始化成熟状态

        //WebSocketServerManager.GetAcitveClient()?.SendPlantGrowthCommand(stateData.ObjID, GrowthProgressState);
        
        //sprites=data.GetAliveSprite();
        //SetSpriteRendererSize(GrowthProgress);


        //this.foregroundSprite.sprite = sprites[0];
        //this.midgroundSprite.sprite = sprites[1];
        //this.backgroundSprite.sprite = sprites[2];
        //BaseSize = Random.Range(0.9f, 1.1f);

        // 基于原始大小设置倍率
        // 获取精灵
        //Sprite sprite = foregroundSprite.sprite;
        //if (sprite == null)
        //{
        //    sprite = midgroundSprite.sprite;
        //    if(sprite == null)
        //        sprite = backgroundSprite.sprite;
        //}

        // 计算精灵的世界单位大小
        //Vector2 originalWorldSize = new Vector2(
        //    sprite.rect.width / sprite.pixelsPerUnit,
        //    sprite.rect.height / sprite.pixelsPerUnit
        //);

        //// 计算新的大小
        //Vector2 newSize = new Vector2(
        //    originalWorldSize.x * BaseSize,
        //    originalWorldSize.y * BaseSize
        //);

        //// 设置到所有 SpriteRenderer
        //foregroundSprite.size = newSize;
        //backgroundSprite.size = newSize;
        //midgroundSprite.size = newSize;
        //defaultcurrentSize = newSize;
        //SetSpriteRendererSize(GrowthProgress);
        //SetSpriteState(stateData.AsDeath);
        //DisplayUpdate();
    }
    public void SetSpriteState(bool Asdeath)
    {
        this.foregroundSprite.color = AsDeath ? deathColor : liveColor;
        this.midgroundSprite.color = AsDeath ? deathColor : liveColor;
        this.backgroundSprite.color = AsDeath ? deathColor : liveColor;
    }
    public void StateUpdate(float IntervalTime)
    {
        if (Parent.IsNutrientConsume())
        {
            //消耗二氧化碳
            ConsumeCO2(IntervalTime);
            //产生氧气
            ProduceO2(IntervalTime);
            //消耗垃圾
            ConsumeWaste(IntervalTime);
            //成长
            GrowUp(IntervalTime);
            //死亡,改成每秒更新了
            //Death(IntervalTime);
            //修复
            //Repair(IntervalTime);
        }
        // else
        // {
        //     //产生二氧化碳
        //     ProduceCO2(IntervalTime);
        //     //消耗氧气
        //     ConsumeO2(IntervalTime);
        //     //产生垃圾
        //     ProduceWaste(IntervalTime);
        // }
        //消耗养分
        ConsumeNutrient(IntervalTime);
        //DisplayUpdate();
    }
    /// <summary>
    /// 产生氧气
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ProduceO2(float IntervalTime)
    {
        Parent.OnO2Changed(data.O2Produce* IntervalTime* GrowthProgress);
    }
    /// <summary>
    /// 消耗氧气
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ConsumeO2(float IntervalTime)
    {
        Parent.OnO2Changed(-data.DeathO2Cost * IntervalTime);
    }
    /// <summary>
    /// 产生二氧化碳
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ProduceCO2(float IntervalTime)
    {
        Parent.OnCO2Changed(data.DeathCO2Produce * IntervalTime);
    }
    /// <summary>
    /// 消耗二氧化碳
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ConsumeCO2(float IntervalTime)
    {
        
        Parent.OnCO2Changed(-data.CO2Cost * IntervalTime * GrowthProgress);
    }
    /// <summary>
    /// 产生垃圾
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ProduceWaste(float IntervalTime)
    {
        Parent.OnWasteChanged(data.DeathWasteProduce * IntervalTime);
    }
    /// <summary>
    /// 消耗垃圾
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ConsumeWaste(float IntervalTime)
    {
        Parent.OnWasteChanged(-data.WasteCost * IntervalTime * GrowthProgress);
    }
    /// <summary>
    /// 消耗养分
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void ConsumeNutrient(float IntervalTime)
    {
        Parent.OnNutrientChanged(-data.NutrientCost * IntervalTime);
    }
    /// <summary>
    /// 植物成长
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    private int lastGrowthProgressState;
    private int GrowthProgressState;
    private bool wasMature = false; // 记录之前是否已成熟
    public void GrowUp(float IntervalTime)
    {
        lastGrowthProgressState = GrowthProgressState;
        GrowthProgressState = GetGrowthProgressState();
        if(lastGrowthProgressState!= GrowthProgressState)
        {
            // 使用植物所属的生态箱ID（stateData.ID）而不是活动生态箱
            // 修复：多生态箱时消息错误发送给活动生态箱导致第一个客户端卡顿的问题
            WebSocketServerManager.GetClient(stateData.ID)?.SendPlantGrowthCommand(stateData.ObjID, GrowthProgressState);
        }
        if (Parent.NutrientProportion() < 0.3)
        {
            return;
        }

        // 记录成长前的成熟状态
        bool wasMaturedBefore = wasMature;

        stateData.SetGrowthProgress(IntervalTime);

        // 检查是否刚刚达到100%成熟
        bool isMatureNow = GrowthProgress >= 1f;
        if (isMatureNow && !wasMaturedBefore)
        {
            wasMature = true;
            // 触发排序（UI会在主动拉取时刷新）
            Parent?.SortPlant();
        }
    }
    public int GetGrowthProgressState()
    {
        if (GrowthProgress >= 0.75)
            return 3;
        else if(GrowthProgress >= 0.5)
            return 2;
        else if(GrowthProgress >= 0.25)
            return 1;
        else
            return 0;
    }
    /// <summary>
    /// 植物死亡
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void Death(float IntervalTime)
    {
        //植物死亡
        if (AsDeath)
        {
            return;
        }
        //营养不足时会增加死亡率
        if (Parent.NutrientProportion() < 0.3)
        {
            float proportion = 1 - Parent.NutrientProportion() / 0.3f;
            Hp -= proportion * IntervalTime / data.NutrientDeath * 100;
        }
        //垃圾超过数量时也会增加死亡率
        if (Parent.WasteProportion() > data.WasteMaxProportion)
        {
            float proportion = 1 - Parent.WasteProportion() / data.WasteMaxProportion;
            Hp -= proportion* IntervalTime/data.WasteDeath*100;
        }
        if (Hp <= 0)
        {
            AsDeath = true;
            // 使用植物所属的生态箱ID（stateData.ID）而不是活动生态箱
            // 修复：多生态箱时消息错误发送给活动生态箱导致第一个客户端卡顿的问题
            WebSocketServerManager.GetClient(stateData.ID)?.SendPlantDeathCommand(stateData.ObjID);
        }
    }
    /// <summary>
    /// 修改植物的外显，主要是两部分
    /// 1.根据植物的成长进度修改植物的大小。
    /// 2.根据植物是否死亡，修改植物死亡相关的外显
    /// </summary>
    public void DisplayUpdate()
    {
        if(lastGrowthProgress!= GrowthProgress)
        {
            SetSpriteRendererSize(GrowthProgress);
            lastGrowthProgress = GrowthProgress;
        }
        if(lastDeahState != AsDeath)
        {
            SetSpriteState(AsDeath);
            lastDeahState = AsDeath;
        }
    }

    private Vector2 defaultcurrentSize;
    public void SetSpriteRendererSize(float  size)
    {
        //if(size>=1)
        //{
        //    return;
        //}

        
        //if(size<1)
        //{
        //    xsize = size  + 1;
        //    ysize = size + 1;
        //}
        //else
        //{
        //    xsize = size;
        //    ysize = size;
        //}

        var v = new Vector2(defaultcurrentSize.x * size, defaultcurrentSize.y * size);
        // 修改宽度和高度
        this.foregroundSprite.size = v;
        this.backgroundSprite.size = v ;
        this.midgroundSprite.size = v ;
    }
    public void Destroy()
    {
        Parent.RemovePlant(this);
        // 使用植物所属的生态箱ID（stateData.ID）而不是活动生态箱
        // 修复：多生态箱时消息错误发送给活动生态箱导致第一个客户端卡顿的问题
        WebSocketServerManager.GetClient(stateData.ID)?.SendRemovePlantCubeCommand(stateData.ObjID,(int)BlockType.Plant);
        //PlayerManager.Instance.RemoveCube(stateData.Pos);
    }
    /// <summary>
    /// 植物修复
    /// </summary>
    /// <param name="IntervalTime">距离上次计算过去的分钟数。</param>
    public void Repair(float IntervalTime)
    {
        if (Parent.WasteProportion() < data.WasteMaxProportion && Parent.NutrientProportion() > 0.3)
        {
            Hp += IntervalTime;
        }

    }
    public void StopChange(bool stop)
    {
        
    }
    /// <summary>
    /// 获取氧气1小时数据的变化量
    /// </summary>
    public float GetO2HourChange()
    {
        if (AsDeath)
        {
            return -data.DeathO2Cost * 60;
        }
        else
        {
            return data.O2Produce * GrowthProgress * 60;
        }
    }
    /// <summary>
    /// 获取二氧化碳1小时数据的变化量
    /// </summary>
    public float GetCO2HourChange()
    {
        if (AsDeath)
        {
            return data.DeathCO2Produce * 60;
        }
        else
        {
            return -data.CO2Cost  * GrowthProgress * 60;
        }
    }
    /// <summary>
    /// 获取垃圾1小时数据的变化量
    /// </summary>
    public float GetWasteHourChange()
    {
        if (AsDeath)
        {
            return data.DeathWasteProduce * 60;
        }
        else
        {
            return -data.WasteCost * GrowthProgress * 60;
        }
    }
    /// <summary>
    /// 获取养分1小时数据的变化量
    /// </summary>
    public float GetNutrientHourChange()
    {
        if (!AsDeath)
        {
            return -data.NutrientCost * 60;
        }
        return 0;
    }
}
