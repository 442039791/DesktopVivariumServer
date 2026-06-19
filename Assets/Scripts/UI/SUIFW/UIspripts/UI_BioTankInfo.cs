using UnityEngine;
using SUIFW;
using UnityEngine.UI;


public class UI_BioTankInfo :UIBase
{
    public int BioTankID;
    //public UI_BioTankInfo_BaseInfo _BaseInfo;
    //public UI_BioTankInfo_ButtonGroup _ButtonGroup;
    public UI_BioTankInfo_CreatureInfo _CreatureInfo;
    public UI_BioTankInfo_CreatureInfo _PlantInfo;
    // public UI_BioTankInfo_CreatureInfo _DecorationInfo;
    public UI_BioTankInfo_CreatureInfo _FacilityInfo;
    public GameObject _facilityTitleObj;
    public GameObject _facilityListObj;
    
    public UI_Main_Biotank_Information _MainBiotankInfo;
    public Button Change_btn;
    public Button AdjustWinow_btn;
    public Button Build_btn;
    private bool NextFrameRefresh = false;
    //public UI_BioTankInfo_FunctionArea _FunctionArea;
    //public UI_BioTankInfo_Title _Title;
    private bool InitCompleted = false;
    private UI_BioTank_Edit uI_BIoTank_Edit;
    private BaseUIForms um;
    public void Refresh()
    {
        // 确保使用最新的活动生物罐ID
        BioTankID = PlayerManager.ActiveBioTanksID;

        // 检查生物罐是否有效
        if (!BioTankRegistry.Instance.HasBioTank(BioTankID))
        {
            return;
        }

        //NextFrameRefresh = true;
        _CreatureInfo.Init(BioTankID);
        _PlantInfo.Init(BioTankID);
        // _DecorationInfo.Init(BioTankID);
        _FacilityInfo.Init(BioTankID);
        if (FacilityManager.Instance.GetUnlockFacilityList().Count > 0)
        {
            _facilityTitleObj.SetActive(true);
            _facilityListObj.SetActive(true);
        }
        Show();
    }
    private bool first = true;
    public override  bool Init()
    {
        OnShow = true;
        //first = true;
        BioTankID = PlayerManager.ActiveBioTanksID;
        if (!BioTankRegistry.Instance.HasBioTank(BioTankID))
        {

            return false;
        }
        _MainBiotankInfo.Init(BioTankID);
        gameObject.SetActive(true);
        //_BaseInfo.Init(BioTankID);
        //_ButtonGroup.Init(BioTankID,this);
        base.InitButtonClickEvent((obj) =>
        {
            NextFrameRefresh = true;
        }, null);
        base.Init();

        NextFrameRefresh = true;
        if (InitCompleted)
        {
            return true;
        }
        Change_btn.AddDeskTopListener(() =>
        {
            // 先打开新界面，再关闭旧界面，确保窗口计数正确
            var biotankList = UIManager.Instance.ShowUIForms(SysDefine.UI_Biotank_List);

            // 关闭编辑界面（如果打开的话）
            if (uI_BIoTank_Edit != null && uI_BIoTank_Edit.gameObject.activeSelf)
            {
                uI_BIoTank_Edit.Close();
            }
            // 关闭新建生态箱界面
            UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_Biotank_AddBiotank);
        });
        AdjustWinow_btn.AddDeskTopListener(() =>
        {
            um = UIManager.Instance.ShowUIForms(SysDefine.UI_Biotank_Move);
            um.transform.SetParent(CoreSetting.Instance.MovePos, false);
            // SetParent后直接设置本地坐标
            um.transform.localPosition = new Vector3(260f, -270f, 0f);
        });
        event_manager.instance.add_UIevent_listener(SysDefine.On_CloseUI_UMMove, On_CloseUI_UMMove);
        Build_btn.AddDeskTopListener(() =>
        {
            // 如果编辑界面已打开，点击则关闭
            if (uI_BIoTank_Edit != null && uI_BIoTank_Edit.transform.gameObject.activeSelf)
            {
                uI_BIoTank_Edit.Close();
                return;
            }

            // 先打开编辑界面，再关闭其他界面，确保窗口计数正确
            uI_BIoTank_Edit = (UI_BioTank_Edit)UIManager.Instance.ShowUIForms(SysDefine.UI_BioTank_Edit);
            uI_BIoTank_Edit.Init();

            // 关闭生态箱列表界面和新建生态箱界面
            UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_Biotank_List);
            UIManager.Instance.CloseOrReturnUIForms(SysDefine.UI_Biotank_AddBiotank);
        });
        InitCompleted = true;
        event_manager.instance.add_UIevent_listener(SysDefine.InitUI_BioTankInfo, on_event_handler);
        //_FunctionArea.Init(this);
        //_Title.Init(this);
        return true;
    }
    public void On_CloseUI_UMMove(string name, object udata)
    {
        if (um!= null)
        {
            um.CloseOrReturnUIForms();
        }
        if (uI_BIoTank_Edit != null)
        {
            if (uI_BIoTank_Edit.transform.gameObject.activeSelf)
            {
                uI_BIoTank_Edit.Close();
            }
        }
    }
    public  void on_event_handler(string name, object udata)
    {
        int id = (int)udata;

        if (!BioTankRegistry.Instance.HasBioTank(BioTankID))
        {
            return ;
        }
        OnShow = true;
        BioTankID = id;
        PlayerManager.Instance.SetActionBioTankManager(id);
        _MainBiotankInfo.Init(BioTankID);
        gameObject.SetActive(true);
        //_BaseInfo.Init(BioTankID);
        //_ButtonGroup.Init(BioTankID,this);
        base.InitButtonClickEvent((obj) =>
        {
            _CreatureInfo.Init(BioTankID);
            _PlantInfo.Init(BioTankID);
            // _DecorationInfo.Init(BioTankID);
            _FacilityInfo.Init(BioTankID);
        }, null);
        base.Init();

        _CreatureInfo.Init(BioTankID);
        _PlantInfo.Init(BioTankID);
        // _DecorationInfo.Init(BioTankID);
        _FacilityInfo.Init(BioTankID);
    }
    private int frame = 0;
    private void Update()
    {
        if (NextFrameRefresh)
        {
            ++frame;
            if (frame > 1)
            {
                frame = 0;
                if (first)
                {
                    first = false;
                }
                else
                {
                    NextFrameRefresh = false;
                }
                Refresh();

            }
        }

    }

    private void OnDestroy()
    {
        // 清理UI引用，防止持有已销毁对象
        uI_BIoTank_Edit = null;
        um = null;
    }
}
