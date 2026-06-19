using UnityEngine;
using SUIFW;
using static Gather;
public class UI_Gather : BaseUIForms
{
    public UI_Gather_Title uI_Gather_Title;
    public UI_Gather_Info uI_Gather_Info;
    /// <summary>
    /// 后景
    /// </summary>
    public GameObject Background;
    /// <summary>
    /// 中景
    /// </summary>
    public GameObject MediumShot;
    /// <summary>
    /// 前景
    /// </summary>
    public GameObject Prospect;
    /// <summary>
    /// 角色
    /// </summary>
    public GameObject Role;
    public Gather gather;
    public override bool Init()
    {
        gameObject.SetActive(true);

        // 扫描并注册所有本地化文本（自动处理所有"Id:xxx"格式的文本）
        LocalizationManager.ScanAndRegisterTextsStatic(transform);

        uI_Gather_Title.Init(this);
        uI_Gather_Info.Init();
        return true;
    }
    /// <summary>
    /// 打开界面时调用
    /// </summary>
    public void OpenUi()
    {
        if (gather.State== GatherState.rest)
        {
            //角色隐藏
            
            //所有物体都在初始位置
        }
        else if (gather.State== GatherState.complete)
        {
            //角色隐藏
            //卡池列表隐藏
            //卡池信息隐藏
            //展示宝箱
        }
        else
        {
            //先检测是在前往还是在返回的路上

            GetBackgroundPosition();
            GetRolePosition();
            //中景前景正常展示，需要填充满整个宽度，并且还有增加
            
        }
    }
    /// <summary>
    /// 更新UI，在界面开始时持续调用
    /// </summary>
    public void Update()
    {
        GetBackgroundPosition();
        GetRolePosition();
        //中景前景正常展示
    }
    /// <summary>
    /// 获取后景位置
    /// </summary>
    public float GetBackgroundPosition()
    {
        //经历的时间
        var time = Time.time - gather.StartTime;
        //野采所需时间
        var sustainTime = gather.gatherAddressData[gather.GatherID].SustainTime;
        //前往
        if (time <= sustainTime / 2)
        {
            return -(time / (sustainTime / 2) * 800);
        }
        //返回
        else
        {
            return -((time / (sustainTime / 2) - 1) * 800);
        }
    }
    /// <summary>
    /// 获取角色位置
    /// </summary>
    public float GetRolePosition()
    {
        //经历的时间
        var time = Time.time - gather.StartTime;
        //野采所需时间
        var sustainTime = gather.gatherAddressData[gather.GatherID].SustainTime;
        //前往
        if (time <= sustainTime / 2)
        {
            return (time / (sustainTime / 2) * 500);
        }
        //返回
        else
        {
            return ((time / (sustainTime / 2) - 1) * 500);
        }
    }
}
