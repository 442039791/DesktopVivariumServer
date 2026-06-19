using UnityEngine;
using System.Collections;

public partial class GatherCubeData : GameConfigDataBase
{
	public string ID; //ID
	public string GatherAddressID; //在哪个采集中
	public string CubeID; //植物的ID
	public int Weight; //权重
	protected override string getFilePath ()
	{
		return "GatherCubeData.json";
	}
}
