using UnityEngine;
using System.Collections;

public partial class TankData : GameConfigDataBase
{
	public string ID; //ID

	public string name; //名称
	public string iconResource; //图标资源
    public string Describe; //道具描述
    /// <summary>
    /// 排序优先级
    /// </summary>
    public int Sort;
    /// <summary>
    /// 是否在图鉴中展示：0不展示、1展示
    /// </summary>
    public int Display;
    public int Price; //购买价格
	public int X; //尺寸
	public int Y; //尺寸
	public int Z; //尺寸
	
	public Sprite GetIconSprite()
	{
		if (!(iconResource.Contains(".png")))
			iconResource += ".png";
		Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle + SysDefine.TankSpritePath +  SysDefine.AssetbundleIconSpritePath + iconResource, null) as Sprite;
		return sprite;
	}
	
	protected override string getFilePath ()
	{
		return "TankData.json";
	}
}
