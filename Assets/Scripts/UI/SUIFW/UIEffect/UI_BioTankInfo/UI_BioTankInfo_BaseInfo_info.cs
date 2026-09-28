using SUIFW;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_BioTankInfo_BaseInfo_info : MonoBehaviour
{
    public bool RatioImageType;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Ratio;
    public TextMeshProUGUI Hour;
    public Image RatioImage;
    //public Button ShowInfo;
    public Button Add;
    public bool InitCompelte;
    private int BioTankID=-1;
    public string Type;
    public Image Scale;

    // 数据刷新间隔
    private float refreshInterval = 1f;
    private float lastRefreshTime;

    public void Init(int BId)
    {

        if (!InitCompelte)
        {
            // 自动应用当前语言的字体到所有文本组件
            LocalizationManager.ApplyCurrentFontToUI(transform);

            //Name ??= transform.Find("Name").GetComponent<TextMeshProUGUI>();
            //Ratio ??= transform.Find("Ratio").GetComponent<TextMeshProUGUI>();
            //Hour ??= transform.Find("Hour").GetComponent<TextMeshProUGUI>();
            //ShowInfo ??= transform.Find("ShowInfo").GetComponent<Button>();
            //Add ??= transform.Find("Add").GetComponent<Button>();
            //event_manager.instance.add_event_listener(SysDefine.ChangeLanugeEventName, ChangeLanuge);
            string[] v = gameObject.name.Split("_");
            Type = v[v.Length - 1];

        }
        BioTankID = BId;
        InitCompelte = true;

        // 主动拉取数据刷新UI
        RefreshData();

        // 长按支持：单击仍为一次操作；长按0.5秒后连续触发（每0.15秒一次）
        // 复用项目既有的 AddDeskTopStillClipListener（MyButtonTriggerEvent）模式，
        // 与 UI_Biotank_Move 的窗口/相机调整按钮行为保持一致
        Add.AddDeskTopStillClipListener(() => {
            var bioTank = BioTankRegistry.Instance.GetBioTank(BioTankID);
            bioTank?.ChangeValue(Type);
            // 操作后立即刷新显示
            RefreshData();
        }, 0.5f, 0.15f);
        //DesktopButtonManager.instance.AddListener(Add,()=> { PlayerManager.Instance.BioTanks[BioTankID].ChangeValue(Type); });
    }

    /// <summary>
    /// 主动拉取数据刷新UI
    /// </summary>
    public void RefreshData()
    {
        var bioTank = BioTankRegistry.Instance.GetBioTank(BioTankID);
        if (bioTank != null)
        {
            var info = bioTank.GetBaseInfo_infoVal(Type);
            if (info != null)
            {
                UpdateDisplay(info);
            }
        }
    }

    private void Update()
    {
        // 定时刷新数据
        if (InitCompelte && gameObject.activeInHierarchy)
        {
            if (Time.time - lastRefreshTime >= refreshInterval)
            {
                lastRefreshTime = Time.time;
                RefreshData();
            }
        }
    }

    /// <summary>
    /// 更新显示（原ChangeInfo方法重命名）
    /// </summary>
    private void UpdateDisplay(BaseInfo_info info)
    {
        if(info.Ratio!=null)
        {
            string[] c = info.Ratio.Split('/');
            float a = float.Parse(c[0]);
            float b = float.Parse(c[1]);
            //string[] n = Name.text.Split("(");
            if (b == 0)
            {
                Ratio.text = info.Ratio + "(0%)";
            }
            else
            {
                Ratio.text = info.Ratio + "(" + ((int)((a / b)*100)).ToString() + "%)";
            }
            RatioImage.fillAmount = a / b+0.02f >1?1: a / b + 0.02f;
            //float val = a / b + 0.02f;
            if(RatioImageType)
            {
                if (a / b >= 0.5f)
                {
                    Scale.color = Color.green;
                }
                else if (a / b >= 0.3f)
                {
                    Scale.color = Color.yellow;
                }
                else
                {
                    Scale.color = Color.red;
                }
            }else
            {
                if (a / b >= 0.7f)
                {
                    Scale.color = Color.red;
                }
                else if (a / b >= 0.5f)
                {
                    Scale.color = Color.yellow;
                }
                else
                {
                    Scale.color = Color.green;
                }
            }

            //Ratio.text = info.Ratio;
        }
        if (info.Hour!="")
        {
            //string[] c = info.Ratio.Split('/');
            bool b = info.Hour.Contains("-");

            if(!b)
            {
                info.Hour = "+" + info.Hour;
            }
            Hour.text = info.Hour;
        }
        else
        {
            Hour.text = "";
        }
    }

    // 保留旧方法签名以兼容可能的外部调用
    public void ChangeInfo(string name, object udata)
    {
        BaseInfo_info info = (BaseInfo_info)udata;
        UpdateDisplay(info);
    }

}
public class BaseInfo_info
{
    public string Ratio;
    public string Hour;
    public BaseInfo_info()
    {

        Ratio=null;
        Hour = null;
    }
    public void SetRatio(string name)
    {
        Ratio=name;
    }
    public void SetHour(string name)
    {
        Hour=name;
    }
}