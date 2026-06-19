using UnityEngine;
using System.Collections;

public partial class AdmissionData : GameConfigDataBase
{
	public string ID; //ID
	public string Name; //道具名称
    /// <summary>
    /// 描述
    /// </summary>
    public string Describe;
    /// <summary>
    /// 排序优先级
    /// </summary>
    public int Sort;
    /// <summary>
    /// 是否在图鉴中展示：0不展示、1展示
    /// </summary>
    public int Display;
    public string IconSprite; //图标路径
    public Sprite GetIconSprite()
    {
        if (!(IconSprite.Contains(".png")))
            IconSprite += ".png";
        Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle + SysDefine.AdmissionSpritePath +  SysDefine.AssetbundleIconSpritePath + IconSprite, null) as Sprite;
        return sprite;
    }
    private Sprite GetSprite(string type)
    {
        Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle + SysDefine.AdmissionSpritePath + type, null) as Sprite;
        return sprite;
    }
    protected override string getFilePath ()
	{
		return "AdmissionData.json";
	}
}
