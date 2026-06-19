using UnityEngine;
using System.Collections;

public partial class GatherFacilitiesData : GameConfigDataBase
{
	public string ID; //ID
	public string GatherAddressID; //在哪个采集中
	public string FacilitiesID; //植物的ID
	public int Weight; //权重
	protected override string getFilePath ()
	{
		return "GatherFacilitiesData.json";
	}
}
