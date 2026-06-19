using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Main_Biotank_Information : ChangeLanugeBase
{

    public Transform InfoParent;
    public Dictionary<string, UI_BioTankInfo_BaseInfo_info> dic = new();
    public int BioTankID = 0;
    public TextMeshProUGUI SizeText;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI SpaceText;

    // 数据刷新间隔
    private float refreshInterval = 1f;
    private float lastRefreshTime;

    public void Init(int BID)
    {
        var biotank = PlayerManager.Instance.GetBioTank(BID);
        if (biotank == null)
        {
            return;
        }
        // 先更新 BioTankID，确保后续代码使用正确的 ID
        BioTankID = BID;
        // 更新当前选中生态箱的名称
        UpdateBiotankName(biotank);
        SizeText.text = biotank.stateData.area_x.ToString() + "x" + biotank.stateData.area_y.ToString() + "x" + biotank.stateData.area_z.ToString();
        SpaceText.text = biotank.GetUseVolume().ToString()+"/"+((int)biotank.GetMaxVolume()).ToString();
        foreach (var t in dic.Values)
        {
            if (t != null)
                t.Init(BioTankID);
        }
        if (InitCompelete)
        {
            return;
        }
        base.Init(transform);

        if (InfoParent == null)
        {
            Debug.LogWarning("UI_Main_Biotank_Information: InfoParent 未赋值，请在 Inspector 中设置");
            return;
        }

            foreach (Transform t in InfoParent.transform)
            {
                var info = t.GetComponent<UI_BioTankInfo_BaseInfo_info>();

                dic.TryAdd(t.gameObject.name, info);
            }

        foreach(var t in dic.Values)
        {
            if(t!= null)
            t.Init(BioTankID);
        }

    }

    private void Update()
    {
        // 定时刷新空间数据
        if (InitCompelete && gameObject.activeInHierarchy)
        {
            if (Time.time - lastRefreshTime >= refreshInterval)
            {
                lastRefreshTime = Time.time;
                RefreshSpaceData();
            }
        }
    }

    /// <summary>
    /// 主动拉取并刷新空间数据
    /// </summary>
    private void RefreshSpaceData()
    {
        var biotank = PlayerManager.Instance.GetBioTank(BioTankID);
        if (biotank == null)
        {
            return;
        }
        SpaceText.text = biotank.GetUseVolume().ToString() + "/" + ((int)biotank.GetMaxVolume()).ToString();
    }

    /// <summary>
    /// 更新生态箱名称显示
    /// </summary>
    private void UpdateBiotankName(BioTankManager biotank)
    {
        if (biotank == null || biotank.stateData == null)
        {
            NameText.text = "";
            return;
        }

        // 如果已有名称则直接显示
        if (!string.IsNullOrEmpty(biotank.stateData.Name))
        {
            NameText.text = biotank.stateData.Name;
            return;
        }

        // 兼容旧存档：从配置获取默认名称
        if (int.TryParse(biotank.stateData.Type, out int typeId))
        {
            var tankData = GameConfigDataBase.GetConfigData<TankData>(typeId, SysDefine.TankConfigData);
            if (tankData != null && !string.IsNullOrEmpty(tankData.name) && int.TryParse(tankData.name, out int nameId))
            {
                NameText.text = LocalizationManager.GetCurrentLanguageByID(nameId);
                return;
            }
        }
        NameText.text = biotank.stateData.Type;
    }
}
