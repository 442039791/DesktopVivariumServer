using UnityEngine;
using System.Collections;

public partial class GatherPlantData : GameConfigDataBase
{
	public string ID; //ID
	public string GatherAddressID; //在哪个采集中
	public string PlantID; //植物的ID
	public int Weight; //权重
	protected override string getFilePath ()
	{
		return "GatherPlantData.json";
	}
}
