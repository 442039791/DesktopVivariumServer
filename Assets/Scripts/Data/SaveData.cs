using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Random = UnityEngine.Random;

[System.Serializable]
public class DecorationStateData
{
    public Vector3 Pos;
    public int ID;
    public int ObjID;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type;
    //public int Count;
    public int GetPrice()
    {
        var dd = GetDecorationData();
        int price = dd.Price;

        return price;
    }
    public DecorationData GetDecorationData()
    {
        return GameConfigDataBase.GetConfigData<DecorationData>(int.Parse(Type),SysDefine.DecorationConfigData);
    }
    public DecorationStateData DeepCopy()
    {
        return new DecorationStateData
        {
            Pos = new Vector3(Pos.x, Pos.y, Pos.z),
            ID = this.ID,
            ObjID = this.ObjID,
            Type = this.Type != null ? string.Copy(this.Type) : null
        };
    }
    public static DecorationStateData GetStateDataByDecorationData( DecorationData data)
    {
        return new DecorationStateData
        {
            Pos = Vector3.zero,
            ID = 0,
            ObjID = IDFactory.GetUniqueID(),
            Type = data.ID
        };
    }
}
[System.Serializable]
public class CubeStateData
{
    public string Type;
    public static CubeStateData GetStateDataByCubeData(CubeData data)
    {
        return new CubeStateData
        {
            Type = data.ID
        };
    }
    public int GetPrice()
    {
        var cd = GetCubeData();
        int price = cd.Price;

        return price;
    }
    public CubeData GetCubeData()
    {
        return GameConfigDataBase.GetConfigData<CubeData>(int.Parse(Type),SysDefine.CubeConfigData);
    }
    public CubeStateData DeepCopy()
    {
        return new CubeStateData
        {
            Type = this.Type != null ? string.Copy(this.Type) : null
        };
    }
}
// ObjectQuality 已移至共享定义
// 位置: Assets/Scripts/Shared/Network/NetworkEnums.cs

[System.Serializable]
public class SaveData
{
	public AnimalStateData[] fishStates;
	public PlantStateData[] plantStateDatas;
	public BioTankStateData[] BioTankStateDatas;
    public DecorationStateData[] decorationStateDatas;
    public FacilitiesStateData[] facilitiesStateDatas;
    public UnLockFacilitiesData unLockFacilitiesData;
    public WarehouseSaveData warehouseSaveData;
    public PlayerStateData PlayerStateData;
	public string[] BlockSaveDatas;
    public int activeID;
    public TutorialSaveData tutorialSaveData;
}
[System.Serializable]
public class BluePrintInfomation
{
    public int ID;
    public string Name;
    public Vector3 Size;
    public string IconName;
    public string BlockModifyNewDataName;
}
[System.Serializable]
public class BluePrintSaveData
{
    public int ActiveID;
    public BluePrintInfomation[] _blueprints;
}
[System.Serializable]
public class BluePrintBiotankInfomation
{
    public int ID;
    public string Name;
    public Vector3 Size;
    public string IconName;
    public string BluePrintBiotankName;
}
[System.Serializable]
public class BluePrintBiotankSaveData
{
    public int ActiveID;
    public Vector3[] Sizes;
    public BluePrintBiotankInfomation[] _blueprints;
}
[System.Serializable]
public class WarehouseSaveData
{
    public AnimalStateData[] fishStates;
    public PlantStateData[] plantStateDatas;
    public DecorationStateData[] decorationStateDatas;
    public CubeStateData[] cubeStateDatas;
}


[System.Serializable]
public class UnLockFacilitiesData
{
    public List<int> UnLockFacilityList;
}


[System.Serializable]
public class AutoSellFishInfo
{
    /// <summary>
    /// 鱼的ID
    /// </summary>
    public string ID;

    /// <summary>
    /// 鱼的状态
    /// </summary>
    public AnimalAgeStatus State;

    ///<summary>
    ///品质
    /// </summary>
    public ObjectQuality Quality;
}

[System.Serializable]
public class FacilitiesStateData
{
    public int ObjID;
    public int Id;
    public FacilityState State;
    public List<AutoSellFishInfo> AutoSellFishList= new List<AutoSellFishInfo>();
    
    public int GetPrice()
    {
        var pd = GetFacilityData();
        int price = pd.Price;
        return price;
    }
    
    public FacilitiesData GetFacilityData()
    {
        return GameConfigDataBase.GetConfigData<FacilitiesData>(Id,SysDefine.FacilitiesConfigData);
    }
    public FacilitiesStateData DeepCopy()
    {
        return new FacilitiesStateData
        {
            ObjID = this.ObjID,
            Id = this.Id,
            State = this.State,
            AutoSellFishList = this.AutoSellFishList
        };
    }
}

[System.Serializable]
public class PlayerStateData
{
    public int Money;
    public static PlayerStateData GetStateDataByInstance(PlayerManager player)
    {
        return new PlayerStateData();
    }
}
[System.Serializable]
public class BioTankStateData
{
    // 面积的宽度（x方向）
    public int area_x;
    // 面积的高度（y方向）
    public int area_y;
    // 面积的深度（z方向）
    public int area_z;
    public Vector3 Pos;
    public int ID;
    /// <summary>
    /// 生态箱名称
    /// </summary>
    public string Name;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type;
    /// <summary>
    /// 水体的体积
    /// </summary>
    public float Volume;
    /// <summary>
    /// 氧气含量
    /// </summary>
    public float O2;
    /// <summary>
    /// 二氧化碳
    /// </summary>
    public float CO2;
    /// <summary>
    /// 废物数量
    /// </summary>
    public float Waste;
    /// <summary>
    /// 拥有的养分
    /// </summary>
    public float Nutrient;
    /// <summary>
    /// 开启氧气的自动供给
    /// </summary>
    public bool AsO2Auto;
    /// <summary>
    /// 开启二氧化碳的自动供给
    /// </summary>
    public bool AsCO2Auto;
    /// <summary>
    /// 开启垃圾的自动供给
    /// </summary>
    public bool AsWasteAuto;
    /// <summary>
    /// 开启养分的自动供给
    /// </summary>
    public bool AsNutrientAuto;
    /// <summary>
    /// 开启素食的自动供给
    /// </summary>
    public bool AsWhiteAuto;
    /// <summary>
    /// 开启肉食的自动供给
    /// </summary>
    public bool AsMeatAuto;
    /// <summary>
    /// 生态箱是否暂停（隐藏窗口时暂停所有运算）
    /// </summary>
    public bool IsPaused;
    /// <summary>
    /// 生态箱的默认摄像头参数（可选，用于首次加载时设置摄像头位置）
    /// </summary>
    public CameraPosOutput DefaultCameraPos;
    /// <summary>
    /// 该生态箱中的动物数据
    /// </summary>
    public AnimalStateData[] Animals;
    /// <summary>
    /// 该生态箱中的植物数据
    /// </summary>
    public PlantStateData[] Plants;
    /// <summary>
    /// 该生态箱中的设备数据
    /// </summary>
    public FacilitiesStateData[] Facilities;
    public BioTankStateData DeepCopy()
    {
        return new BioTankStateData
        {
            area_x = area_x,
            area_y = area_y,
            area_z = area_z,
            Pos = new Vector3(Pos.x, Pos.y, Pos.z),
            ID = this.ID,
            Name = this.Name != null ? string.Copy(this.Name) : null,
            Type = this.Type != null ? string.Copy(this.Type) : null,
            Volume = this.Volume,
            O2 = this.O2,
            CO2 = this.CO2,
            Waste = this.Waste,
            Nutrient = this.Nutrient,
            AsO2Auto = this.AsO2Auto,
            AsCO2Auto = this.AsCO2Auto,
            AsWasteAuto = this.AsWasteAuto,
            AsNutrientAuto = this.AsNutrientAuto,
            AsWhiteAuto = this.AsWhiteAuto,
            AsMeatAuto = this.AsMeatAuto,
            IsPaused = this.IsPaused,
            DefaultCameraPos = this.DefaultCameraPos != null ? new CameraPosOutput(
                this.DefaultCameraPos.angleX,
                this.DefaultCameraPos.angleY,
                this.DefaultCameraPos.radius,
                this.DefaultCameraPos.TargetPos
            ) : null,
            Animals = this.Animals != null ? System.Array.ConvertAll(this.Animals, a => a.DeepCopy()) : null,
            Plants = this.Plants != null ? System.Array.ConvertAll(this.Plants, p => p.DeepCopy()) : null,
            Facilities = this.Facilities != null ? System.Array.ConvertAll(this.Facilities, f => f.DeepCopy()) : null
        };
    }
    public static BioTankStateData GetStateDataByInstance(BioTankManager bioTankManager)
    {
        return new BioTankStateData();
    }
	public static string GetBlockDataByInstance(BioTankManager bioTankManager)
	{
		return Application.streamingAssetsPath+SysDefine.BlockSaveDataPath+"_"+ bioTankManager.stateData.ID+".txt";
	}
}

/// <summary>
/// 摄像机位置输出数据
/// </summary>
[System.Serializable]
public class CameraPosOutput
{
    public CameraPosOutput() { }

    public CameraPosOutput(float _angleX, float _angleY, float _radius, Vector3 _TargetPos)
    {
        angleX = _angleX;
        angleY = _angleY;
        radius = _radius;
        TargetPos = _TargetPos;
    }
    public float angleX;
    public float angleY;
    public float radius;
    public Vector3 TargetPos;
}

public enum WarehouseBaseDataType
{
    Fish,
    Plant,
    Decoration,
    Cube
}
public class WarehouseBaseData
{
    public int WarehouseBaseID;
    public WarehouseBaseDataType DataType;
    //public bool Adult;
    public string Name;
    public int Count;
    public Sprite Icon;
    public bool New;
    public virtual WarehouseBaseData DeepCopy()
    {
        WarehouseBaseData warehouseBaseData = new()
        {
            DataType = DataType,
            Name = Name,
            Count = Count,
            Icon = Icon,
            New = New
        };
        return warehouseBaseData;
    }
}
public class WarehouseCubeStateData: WarehouseBaseData
{
    public CubeStateData stateData;
    public new WarehouseCubeStateData DeepCopy()
    {
        WarehouseCubeStateData warehouseBaseData = new WarehouseCubeStateData();
        warehouseBaseData.DataType = DataType;
        warehouseBaseData.Name = Name;
        warehouseBaseData.Count = Count;
        warehouseBaseData.Icon = Icon;
        stateData = stateData.DeepCopy();
        warehouseBaseData.stateData = stateData;
        warehouseBaseData.New = New;
        return warehouseBaseData;
    }
}

/// <summary>
/// 库存装饰物
/// </summary>
public class WarehouseDecorationStateData : WarehouseBaseData
{


    public DecorationStateData stateData;
}

/// <summary>
/// 引导存档数据
/// </summary>
[System.Serializable]
public class TutorialSaveData
{
    /// <summary>
    /// 已完成的引导流程ID列表
    /// </summary>
    public List<string> completedFlows = new List<string>();

    /// <summary>
    /// 已完成的引导步骤列表（格式：flowID_stepID）
    /// </summary>
    public List<string> completedSteps = new List<string>();

    /// <summary>
    /// 是否禁用引导系统
    /// </summary>
    public bool isTutorialDisabled = false;
}