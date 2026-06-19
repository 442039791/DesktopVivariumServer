using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class DecorationData : GameConfigDataBase
{
    public string ID; //ID

    public string name; //名称
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
    public string iconResource;
    //public List<string> artResources; //美术资源
    public int Price; //购买价格
    public string UniqueType; //类型
    public string DecorationModel;
    public string ModelDataName;
    public string ModName ;
    public Sprite GetIconSprite()
    {
        if(!iconResource.Contains(".png"))
            iconResource += ".png";
        Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle + SysDefine.DecorationSpritePath + SysDefine.AssetbundleIconSpritePath + iconResource, null) as Sprite;
        return sprite;
    }
    //public List<Sprite> GetGroundSprites()
    //{
    //    return new List<Sprite> { GetSprite(SysDefine.AssetbundleSpritePath + artResources[0]), GetSprite(SysDefine.AssetbundleSpritePath + artResources[1]),
    //    GetSprite(SysDefine.AssetbundleSpritePath+ artResources[2])};
    //}
    //public Sprite GetSprite()
    //{
    //  return GetSprite(SysDefine.PlantSpritePath + SysDefine.AssetbundleSpritePath + artResources[0]);
    //}
    private Sprite GetSprite(string type)
    {
        Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle + SysDefine.DecorationSpritePath + type, null) as Sprite;
        return sprite;
    }

    protected override string getFilePath ()
	{
		return "DecorationData.json";
	}
}
