using UnityEngine;
using System.Collections;

/// <summary>
/// 语言文本数据库基类
/// 注意：这个类的文件路径由调用方通过参数指定，getFilePath 不使用
/// </summary>
public partial class LaunguageDataBase : GameConfigDataBase
{
	public int ID; //ID
	public string Text; //文本

	/// <summary>
	/// 获取文件路径（此类不使用，路径由外部指定）
	/// </summary>
	protected override string getFilePath()
	{
		// 返回空字符串而不是 "null"，避免路径解析问题
		// 实际使用时路径由 GameConfigDataBase.GetConfigDatas<T>(string fileName) 的参数指定
		return string.Empty;
	}
}
