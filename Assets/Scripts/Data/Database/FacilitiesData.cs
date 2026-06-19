using UnityEngine;

public partial class FacilitiesData : GameConfigDataBase
{
    public string ID; //ID

    public string Name; //����
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
    public string Icon;
    public int Price; //����۸�


    public Sprite GetIconSprite()
    {
        if (!(Icon.Contains(".png")))
            Icon += ".png";
        Sprite sprite = ResMgr.Instance.GetAssetCache(SysDefine.ImageAssetbundle + SysDefine.FacilitiesSpritePath +  SysDefine.AssetbundleIconSpritePath + Icon, null) as Sprite;
        return sprite;
    }


    private Material GetMaterial(string type)
    {
        Material material = ResMgr.Instance.GetAssetCache(SysDefine.AssetbundleMaterialPath+SysDefine.FacilitiesSpritePath + type, null) as Material;
        return material;
    }
    protected override string getFilePath()
    {
        return "FacilitiesData.json";
    }
}
