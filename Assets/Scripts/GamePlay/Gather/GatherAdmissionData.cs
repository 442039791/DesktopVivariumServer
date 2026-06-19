using UnityEngine;
using System.Collections;

public partial class GatherAdmissionData : GameConfigDataBase
{
	public string ID; //ID
	public string GatherAddressID; //在哪个采集中
	public string AdmissionID; //卡池解锁道具ID
	public int Weight; //权重

    protected override string getFilePath ()
	{
		return "GatherAdmissionData.json";
	}
}
