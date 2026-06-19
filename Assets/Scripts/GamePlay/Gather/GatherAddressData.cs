using UnityEngine;
using System.Collections;

public partial class GatherAddressData : GameConfigDataBase
{
	public string ID; //卡池ID
	public string Name; //
    public int Sort; //排序
    public string Icon; //图标
    public int Price; //价格
	public string Unlock;//解锁条件
    public float SustainTime; //持续时间
    public float FishProbability; //鱼概率
    public int FishNum; //鱼数量
    public float PlantProbability; //植物概率
    public int PlantNum; //植物数量
    public float CubeProbability; //结构概率
    public int CubeNum; //结构数量
    public float FacilitiesProbability; //设备概率
    public int FacilitiesNum; //设备数量
    public float DecorationProbability; //装饰物概率
    public int DecorationNum; //装饰物数量
    public float AdmissionProbability; //通行证概率
    public int AdmissionNum; //通行证数量
    public float TankProbability; //鱼缸概率
    public int TankNum; //鱼缸数量
    protected override string getFilePath ()
	{
		return "GatherAddressData.json";
	}
}
