using UnityEngine;
using SUIFW;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class UI_Help : UIBase
{
    public Transform spawnParent;
    bool asInit;
    public override  bool Init()
    {
        if (asInit)
        {
            return true;
        }
        foreach (var data in GameConfigDataBase.GetConfigDatas<HelpData>(SysDefine.HelpConfigData))
        {
            if (data.Display >= 1)
            {
                var gamePrefab=(GameObject)ResMgr.Instance.GetAssetCache(SysDefine.PrefabAssetbundle + SysDefine.UI_HelpInfoPrefab, null);
                if (gamePrefab.TryGetComponent<UI_HelpInfo>(out var helpPrefab))
                {
                    // 使用SetText注册，支持语言切换时自动更新
                    LocalizationManager.SetText(helpPrefab.title, int.Parse(data.Title));
                    LocalizationManager.SetText(helpPrefab.describe, int.Parse(data.Describe));
                    var instantiatedPrefab = Instantiate(gamePrefab, spawnParent);
                }
            }
        }

        asInit=true;
        return true;
    }
}
