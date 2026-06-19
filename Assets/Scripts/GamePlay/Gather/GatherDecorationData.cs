using UnityEngine;
using System.Collections;

public partial class GatherDecorationData : GameConfigDataBase
{
	public string ID; //ID
	public string GatherAddressID; //在哪个采集中
	public string DecorationID; //装饰物的ID
	public int Weight; //权重
	protected override string getFilePath ()
	{
		return "GatherDecorationData.json";
	}
}
