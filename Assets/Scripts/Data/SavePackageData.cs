using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 服务器存档包 - 包含所有服务器需要的数据
/// 文件名: ServerPackage.dat (GZip压缩)
///
/// 存档结构:
/// - ServerPackage.dat: 账号级别数据(SaveData, GatherSaveData, 语言设置)
/// - {ID}_BioTankPackage.gz: 每个生态箱的独立数据(地形 + 客户端设置)
/// </summary>
[System.Serializable]
public class ServerPackage
{
    /// <summary>
    /// 主存档数据 - 鱼、植物、装饰、设施、仓库等
    /// </summary>
    public SaveData mainSaveData;

    /// <summary>
    /// 采集系统数据 - 解锁状态和采集进度
    /// </summary>
    public GatherSaveData gatherSaveData;

    /// <summary>
    /// 全局设置数据 - 语言设置
    /// </summary>
    public ServerSettingData serverSettingData;
}

/// <summary>
/// 服务器设置数据 - 仅包含服务器需要的设置
/// </summary>
[System.Serializable]
public class ServerSettingData
{
    /// <summary>
    /// 语言设置
    /// </summary>
    public string language;

    /// <summary>
    /// 是否启用预加载客户端池功能
    /// </summary>
    public bool enablePreloadClientPool = false;
}

/// <summary>
/// 生态箱独立存档包 - 包含单个生态箱的所有数据
/// 文件名: {ID}_BioTankPackage.gz (GZip压缩)
///
/// 替代旧的结构:
/// - 旧: {ID}_LandSaveData.gz (仅地形) + ClientPackage.dat (所有客户端设置)
/// - 新: {ID}_BioTankPackage.gz (地形 + 客户端设置 合并)
/// </summary>
[System.Serializable]
public class BioTankSavePackage
{
    /// <summary>
    /// 生态箱ID
    /// </summary>
    public int id;

    /// <summary>
    /// 地形数据 - 原 LandSaveData 的内容 (JSON字符串)
    /// 保持为字符串格式以兼容现有地形加载逻辑
    /// </summary>
    public string landData;

    /// <summary>
    /// 摄像头位置数据
    /// </summary>
    public CameraPositionData cameraPos;

    /// <summary>
    /// 窗口位置数据
    /// </summary>
    public WindowPositionData windowPos;
}

/// <summary>
/// 客户端存档包 - 包含所有客户端需要的数据
/// 文件名: ClientPackage.dat (GZip压缩)
///
/// [已废弃] 客户端数据现在存储在 {ID}_BioTankPackage.gz 中
/// 保留此类仅用于迁移旧存档
/// </summary>
[System.Serializable]
public class ClientPackage
{
    /// <summary>
    /// 客户端设置数据 - 窗口位置、缩放等
    /// </summary>
    public ClientSettingData clientSettingData;

    /// <summary>
    /// 每个BioTank的客户端设置 - 摄像头位置、窗口位置
    /// </summary>
    public BioTankClientSetting[] bioTankSettings;
}

/// <summary>
/// 客户端设置数据 - 仅包含客户端需要的设置
/// </summary>
[System.Serializable]
public class ClientSettingData
{
    /// <summary>
    /// 主窗口位置
    /// </summary>
    public Vector2Int windowPos;

    /// <summary>
    /// 窗口缩放比例
    /// </summary>
    public float windowScale;

    /// <summary>
    /// 各BioTank窗口的位置和大小
    /// </summary>
    public BioTankWindowData[] bioTankWindows;
}

/// <summary>
/// BioTank窗口数据 - 用于SettingSaveData的BioTankSaveDatas兼容
/// </summary>
[System.Serializable]
public class BioTankWindowData
{
    public int id;
    public Vector2Int position;
    public Vector2Int size;
}

/// <summary>
/// BioTank客户端设置 - 摄像头和窗口位置
/// 对应旧的 {ID}_BioTankSettingSaveData.txt
/// </summary>
[System.Serializable]
public class BioTankClientSetting
{
    /// <summary>
    /// BioTank ID
    /// </summary>
    public int id;

    /// <summary>
    /// 摄像头位置数据
    /// </summary>
    public CameraPositionData cameraPos;

    /// <summary>
    /// 窗口位置数据
    /// </summary>
    public WindowPositionData windowPos;
}

/// <summary>
/// 摄像头位置数据
/// </summary>
[System.Serializable]
public class CameraPositionData
{
    public float angleX;
    public float angleY;
    public float radius;
    public Vector3 targetPos;
}

/// <summary>
/// 窗口位置数据
/// </summary>
[System.Serializable]
public class WindowPositionData
{
    public Vector2Int position;
    public int width;
    public int height;
}

/// <summary>
/// 旧版存档包 - 用于迁移工具读取旧格式
/// 已废弃,仅用于向后兼容
/// </summary>
[System.Serializable]
public class SavePackage
{
    public SaveData mainSaveData;
    public SettingSaveData settingSaveData;
    public GatherSaveData gatherSaveData;
}
