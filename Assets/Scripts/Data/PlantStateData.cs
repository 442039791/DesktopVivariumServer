using UnityEngine;


[System.Serializable]
public class PlantStateData
{
    public Vector3 Pos;
    public int ID;
    public int ObjID;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type;
    /// <summary>
    /// 植物成年进度
    /// </summary>
    public float GrowthProgress;
    /// <summary>
    /// 植物死亡进度
    /// </summary>
    public float HP;
    /// <summary>
    /// 是否已经死亡，true为死亡
    /// </summary>
    public bool AsDeath;
    public int GetPrice()
    {
        var pd = GetPlantData();
        int price = pd.Price;

        return price;
    }
    public PlantData GetPlantData()
    {
        return GameConfigDataBase.GetConfigData<PlantData>(int.Parse(Type), SysDefine.PlantConfigData);
    }
    public PlantStateData DeepCopy()
    {
        return new PlantStateData
        {
            Pos = new Vector3(Pos.x, Pos.y, Pos.z),
            ID = this.ID,
            Type = this.Type != null ? string.Copy(this.Type) : null,
            ObjID = this.ObjID,
            GrowthProgress = this.GrowthProgress,
            HP = this.HP,
            AsDeath = this.AsDeath
        };
    }
    public static PlantStateData GetStateDataByPlantData(PlantData data)
    {
        PlantStateData p = new()
        {
            Pos = Vector3.zero,
            ObjID = IDFactory.GetUniqueID(),
            ID = 0,
            Type = data.ID,
            GrowthProgress = 0,
            HP = 100,
            AsDeath = false,
        };
        return p;
    }
    public static PlantStateData GetStateDataByInstance(Plant plant)
    {
        return new PlantStateData();
    }
    /// <summary>
    /// 得到成年进度
    /// </summary>
    /// <returns></returns>
    public float GetGrowthProgress()
    {
        return GrowthProgress / (GetPlantData().AdultTime * 60);
    }
    /// <summary>
    /// 得到成年时间
    /// </summary>
    /// <returns></returns>
    public int GetGrowthTime()
    {
        return (int)(GetPlantData().AdultTime * 60 - GrowthProgress);
    }
    /// <summary>
    /// 修改成年进度，输入的是时间
    /// </summary>
    /// <param name="time"></param>
    public void SetGrowthProgress(float time)
    {
        if (GetPlantData().AdultTime * 60 <= time + GrowthProgress)
        {
            GrowthProgress = GetPlantData().AdultTime * 60;
        }
        else
        {
            GrowthProgress += time;
        }
    }
}

/// <summary>
/// 库存植物数据
/// </summary>
public class WarehousePlantStateData : WarehouseBaseData
{
    /// <summary>
    /// 植物数据
    /// </summary>
    public PlantStateData data;
    public new WarehousePlantStateData DeepCopy()
    {
        WarehousePlantStateData warehouseBaseData = new()
        {
            DataType = DataType,
            Name = Name,
            Count = Count,
            Icon = Icon,
            New = New,
            data = data
        };
        return warehouseBaseData;
    }

}