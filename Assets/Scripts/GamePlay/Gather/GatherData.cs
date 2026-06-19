using System.Collections.Generic;

/// <summary>
/// 野采数据
/// </summary>
public class GatherData : GameConfigDataBase
{
    /// <summary>
    /// 奖池中拥有哪些鱼
    /// </summary>
    public List<string> FishIDList;
    /// <summary>
    /// 奖池中每种鱼的权重
    /// </summary>
    public List<int> FishWeightList;
    /// <summary>
    /// 奖池中拥有哪些植物
    /// </summary>
    public List<string> PlantIDList;
    /// <summary>
    /// 奖池中每种植物的权重
    /// </summary>
    public List<int> PlantWeightList;
    /// <summary>
    /// 奖池中拥有哪些装饰物
    /// </summary>
    public List<string> DecorationIDList;
    /// <summary>
    /// 奖池中每种装饰物的权重
    /// </summary>
    public List<int> DecorationWeightList;
    /// <summary>
    /// 奖池中拥有立方体
    /// </summary>
    public List<string> CubeIDList;
    /// <summary>
    /// 奖池中每种立方体的权重
    /// </summary>
    public List<int> CubeWeightList;
    /// <summary>
    /// 奖池中拥有设备
    /// </summary>
    public List<string> FacilitiesIDList;
    /// <summary>
    /// 奖池中每种设备的权重
    /// </summary>
    public List<int> FacilitiesWeightList;
    /// <summary>
    /// 奖池中拥有通行证
    /// </summary>
    public List<string> AdmissionIDList;
    /// <summary>
    /// 奖池中每种通行证的权重
    /// </summary>
    public List<int> AdmissionWeightList;
    /// <summary>
    /// 奖池中拥有鱼缸
    /// </summary>
    public List<string> TankIDList;
    /// <summary>
    /// 奖池中鱼缸的权重
    /// </summary>
    public List<int> TankWeightList;
}
