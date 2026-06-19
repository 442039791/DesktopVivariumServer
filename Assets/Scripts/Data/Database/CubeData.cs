using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class CubeData : GameConfigDataBase
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
    public List<string> artResources; //美术资源
    public int Price; //购买价格


    public Sprite GetIconSprite()
    {
        if (!(iconResource.Contains(".png")))
            iconResource += ".png";
        Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle + SysDefine.CubeMaterialPath +  SysDefine.AssetbundleIconSpritePath + iconResource, null) as Sprite;
        return sprite;
    }


    private Material GetMaterial(string type)
    {
        Material material = ResMgr.Instance.GetAssetCache(SysDefine.AssetbundleMaterialPath+SysDefine.CubeMaterialPath + type, null) as Material;
        return material;
    }
    protected override string getFilePath()
    {
        return "CubeData.json";
    }
}
