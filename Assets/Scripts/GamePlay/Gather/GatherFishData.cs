using UnityEngine;
using System.Collections;

public partial class GatherFishData : GameConfigDataBase
{
	public string ID; //ID
	public string GatherAddressID; //在哪个采集中
	public string FishID; //鱼的ID
	public int Weight; //权重
	protected override string getFilePath ()
	{
		return "GatherFishData.json";
	}
}
