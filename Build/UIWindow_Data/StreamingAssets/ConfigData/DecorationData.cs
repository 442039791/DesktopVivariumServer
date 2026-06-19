using UnityEngine;
using System.Collections;

public partial class DecorationData : GameConfigDataBase
{
	public string ID; //ID

	public string name; //����
	public string[] artResources; //������Դ
	public int Price; //����۸�
	protected override string getFilePath ()
	{
		return "DecorationData";
	}
}
