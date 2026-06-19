using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 生态箱初始布局数据 - 包含布局元信息和地形数据
/// 文件格式: StreamingAssets/TankInitialLayouts/{LayoutName}.json
/// 注意：所有内容（Cube、植物、装饰物）都存储在terrainData中
/// </summary>
[Serializable]
public class TankInitialLayoutData
{
    /// <summary>
    /// 布局ID（唯一标识）
    /// </summary>
    public string layoutId;

    /// <summary>
    /// 布局名称（用于显示）
    /// </summary>
    public string layoutName;

    /// <summary>
    /// 布局描述
    /// </summary>
    public string description;

    /// <summary>
    /// 适用的生态箱尺寸X
    /// </summary>
    public int sizeX;

    /// <summary>
    /// 适用的生态箱尺寸Y
    /// </summary>
    public int sizeY;

    /// <summary>
    /// 适用的生态箱尺寸Z
    /// </summary>
    public int sizeZ;

    /// <summary>
    /// 排序顺序（数值越小越靠前）
    /// </summary>
    public int sortOrder;

    /// <summary>
    /// 布局预览图标名称（可选）
    /// </summary>
    public string iconName;

    /// <summary>
    /// 地形数据（原始JSON字符串，包含所有Cube、植物、装饰物数据）
    /// - Cube: _name为3xxx格式（如3001）
    /// - 植物: _name为植物ID_生长阶段格式（如2001_3）
    /// - 装饰物: _name为4xxx格式（如4001）
    /// </summary>
    public string terrainData;

    /// <summary>
    /// [已废弃] 植物配置列表 - 现在植物数据存储在terrainData中，此字段仅用于兼容旧JSON文件
    /// </summary>
    [Obsolete("植物数据现在存储在terrainData中")]
    public List<InitialPlantData> plants = new List<InitialPlantData>();

    /// <summary>
    /// [已废弃] 装饰物配置列表 - 现在装饰物数据存储在terrainData中，此字段仅用于兼容旧JSON文件
    /// </summary>
    [Obsolete("装饰物数据现在存储在terrainData中")]
    public List<InitialDecorationData> decorations = new List<InitialDecorationData>();

    /// <summary>
    /// 版本号
    /// </summary>
    public string version = "1.0";

    /// <summary>
    /// 默认摄像机参数（可选，用于新建生态箱时设置初始摄像机位置）
    /// </summary>
    public LayoutCameraData defaultCameraPos;
}

/// <summary>
/// 初始植物配置数据
/// </summary>
[Serializable]
public class InitialPlantData
{
    /// <summary>
    /// 植物类型ID（对应PlantData.csv中的ID）
    /// </summary>
    public string type;

    /// <summary>
    /// 位置
    /// </summary>
    public Vector3 position;
}

/// <summary>
/// 初始装饰物配置数据
/// </summary>
[Serializable]
public class InitialDecorationData
{
    /// <summary>
    /// 装饰物类型ID（对应DecorationData.csv中的ID）
    /// </summary>
    public string type;

    /// <summary>
    /// 位置
    /// </summary>
    public Vector3 position;
}

/// <summary>
/// 布局摄像机参数数据
/// </summary>
[Serializable]
public class LayoutCameraData
{
    /// <summary>
    /// 摄像机水平角度
    /// </summary>
    public float angleX;

    /// <summary>
    /// 摄像机垂直角度
    /// </summary>
    public float angleY;

    /// <summary>
    /// 摄像机到目标点的距离
    /// </summary>
    public float radius;

    /// <summary>
    /// 摄像机目标点位置
    /// </summary>
    public LayoutVector3 targetPos;

    /// <summary>
    /// 转换为CameraPosOutput
    /// </summary>
    public CameraPosOutput ToCameraPosOutput()
    {
        return new CameraPosOutput(
            angleX,
            angleY,
            radius,
            targetPos != null ? targetPos.ToVector3() : UnityEngine.Vector3.zero
        );
    }
}

/// <summary>
/// 布局用的Vector3数据（用于JSON序列化）
/// </summary>
[Serializable]
public class LayoutVector3
{
    public float x;
    public float y;
    public float z;

    public UnityEngine.Vector3 ToVector3()
    {
        return new UnityEngine.Vector3(x, y, z);
    }
}
