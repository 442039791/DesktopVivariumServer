using System;

/// <summary>
/// 服务端本地枚举定义
/// 注意: 这些枚举仅在服务端使用，不通过网络传输
/// </summary>

/// <summary>
/// 方块类型（服务端本地定义）
/// 注意: 网络传输时会转换为 int 类型
/// 枚举值必须与客户端的 SoftKitty.PCW.BlockPool.BlockType 保持一致：
/// 0 = Cube, 1 = Decoration, 2 = Plant
/// </summary>
[Serializable]
public enum BlockType
{
    Cube = 0,
    Decoration = 1,
    Plant = 2
}
