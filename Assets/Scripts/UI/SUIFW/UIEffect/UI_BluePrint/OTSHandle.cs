using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Common;
[Serializable]
public class BlockData
{
    // 对应JSON中的"blocks"字段
    public Dictionary<int, string> blocks;

    // 对应JSON中的"xyzi"字段（三维坐标+索引）
    public List<List<int>> xyzi;

}

public class OTSHandle : MonoSingleton<OTSHandle>
{
    Dictionary<int, string> cubeType/* = new()*/;
    public List<BlockModifyNewData> BlueprintData = new();
    Dictionary<string, string> newCubeType = new()
        {
            {"minecraft:andesite","1001"},
            {"minecraft:black_concrete_powder","1002"},
            {"minecraft:blue_concrete_powder","1003"},
            {"minecraft:brain_coral_block","1004"},
            {"minecraft:brown_concrete_powder","1005"},
            {"minecraft:brown_mushroom_block","1006"},
            {"minecraft:bubble_coral_block","1007"},
            {"minecraft:calcite","1008"},
            {"minecraft:clay","1009"},
            {"minecraft:coal_block","1010"},
            {"minecraft:cyan_concrete_powder","1011"},
            {"minecraft:fire_coral_block","1012"},
            {"minecraft:granite","1013"},
            {"minecraft:gray_concrete_powder","1014"},
            {"minecraft:green_concrete_powder","1015"},
            {"minecraft:light_gray_concrete_powder","1016"},
            {"minecraft:lime_concrete_powder","1017"},
            {"minecraft:magenta_concrete_powder","1018"},
            {"minecraft:moss_block","1019"},
            {"minecraft:mud","1020"},
            {"minecraft:mushroom_stem","1021"},
            {"minecraft:nether_wart_block","1022"},
            {"minecraft:orange_concrete_powder","1023"},
            {"minecraft:packed_ice","1024"},
            {"minecraft:packed_mud","1025"},
            {"minecraft:pink_concrete_powder","1026"},
            {"minecraft:purple_concrete_powder","1027"},
            {"minecraft:quartz_block","1028"},
            {"minecraft:red_concrete_powder","1029"},
            {"minecraft:red_sand","1030"},
            {"minecraft:sand","1031"},
            {"minecraft:stone","1032"},
            {"minecraft:white_concrete_powder","1033"},
            {"minecraft:yellow_concrete_powder","1034"},
        };
    //public List<BlockInstance> SaveBlock = new();
    void Start()
    {
        
    }
    private  void FindBoundingCorners(
   List<List<int>> points,
   out Vector3 minCorner,
   out Vector3 maxCorner)
    {
        // 异常处理
        if (points == null || points.Count == 0)
        {
            minCorner = maxCorner = Vector3.zero;
            Debug.LogWarning("点集为空，返回默认坐标(0,0,0)");
            return;
        }

        // 初始化极值
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        // 遍历计算
        foreach (var point in points)
        {
            // 更新X轴
            if (point[0] < minX) minX = point[0];
            if (point[0] > maxX) maxX = point[0];

            // 更新Y轴
            if (point[1] < minY) minY = point[1];
            if (point[1] > maxY) maxY = point[1];

            // 更新Z轴
            if (point[2] < minZ) minZ = point[2];
            if (point[2] > maxZ) maxZ = point[2];
        }

        // 组合结果
        minCorner = new Vector3(minX, minY, minZ);
        maxCorner = new Vector3(maxX, maxY, maxZ);
    }
    /// <summary>
    /// 初始化cube的对应关系
    /// </summary>
    public  BluePrintBlockSaveData InitializationCube(/*Dictionary<int, string> blocks*/)
    {
        cubeType = new();
        BlueprintData.Clear();
        BluePrintBlockSaveData saveData = new BluePrintBlockSaveData();
        // 读取JSON文件路径
        string path = Path.Combine(Application.streamingAssetsPath, "data.json");
        string jsonContent = File.ReadAllText(path);

        // 解析JSON数据
        BlockData data = JsonConvert.DeserializeObject<BlockData>(jsonContent); ;
        foreach (var item in data.blocks)
        {
            if (newCubeType.ContainsKey(item.Value))
            {
                cubeType.Add(item.Key, newCubeType[item.Value]);
            }
            else
            {
                cubeType.Add(item.Key, "1001");
            }
        }
        FindBoundingCorners(data.xyzi, out Vector3 min, out Vector3 max);
        var size = max - min;
        //cube转换
        //InitializationCube(data.blocks);
        foreach (var block in data.xyzi)
        {
            if (block.Count>3)
            {
                BlueprintData.Add(new()
                {
                    _posX =(int)size.x- (block[0]-(int)((min.x<0)? min.x :0)),
                    _posY = block[1]-(int)((min.y<0)? min.y :0),
                    _posZ = (int)size.z-(block[2]-(int)((min.z<0)? min.z :0)),
                    _y = block[1]-(int)((min.y<0)? min.y :0),
                    _mode = 0,
                    _addUID = 0,
                    _addType = 4,
                    _name = cubeType[block[3]],
                    _addAngle = new() { x = 0, y = 0, z = 0 },
                    _localScale = new() { x = 1, y = 1, z = 1 },
                    _prop = false,
                });
            }
        }

        
        saveData.Size = size;
        saveData._blockModifyNewDatas = BlueprintData.ToArray();
        return saveData;
    }
}
[System.Serializable]
public class BluePrintBlockSaveData
{
    public int ID;
    public string IconPath;
    public Vector3 Size;
    public BlockModifyNewData[] _blockModifyNewDatas;
}
[System.Serializable]
public class BlockModifyNewData
{
    public int _posX;
    public int _posY;
    public int _posZ;
    public int _y;

    public int _mode = 0;//0-add 1-del
    public int _addUID;
    public int _addType;
    public string _name;
    public Vector3 _addAngle = Vector3.zero;
    public Vector3 _localScale = Vector3.one;
    public bool _prop;
    //public int _style;
    //public int _rnd;
}