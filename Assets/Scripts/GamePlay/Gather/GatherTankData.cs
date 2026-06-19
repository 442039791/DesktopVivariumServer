using UnityEngine;
using System.Collections;

public partial class GatherTankData : GameConfigDataBase
{
	public string ID; //ID
	public string GatherAddressID; //在哪个采集中
	public string TankID; //鱼的ID
	public int Weight; //权重
	public int Sort;//排序优先级
    protected override string getFilePath ()
	{
		return "GatherTankData.json";
	}
}
