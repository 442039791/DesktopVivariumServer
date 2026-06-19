using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class InitialData : GameConfigDataBase
{
	public int ID; //ID

	public int Money; //钱
	public List<string>Admission; //卡池
	public List<string> Cube; //结构
    public List<string> Facilities; //设备
	public List<string> Grid; //鱼缸位置
	public List<string> Tank; //鱼缸
	public List<string> Decoration; //装饰
	public List<string> Fish; //鱼解锁类
	public List<string> Plant; //植物解锁类
	protected override string getFilePath ()
	{
		return "InitialData.json";
	}
}
