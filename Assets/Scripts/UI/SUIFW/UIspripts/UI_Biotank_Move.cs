using UnityEngine;

using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class UI_Biotank_Move : ChangeLanugeBaseSUIFW
{
    [Header("窗口调整位置")]
    public Button Window_adjustPosition_Up_Btn;
    public Button Window_adjustPosition_Down_Btn;
    public Button Window_adjustPosition_Left_Btn;
    public Button Window_adjustPosition_Right_Btn;
    public Button Window_adjustPosition_Reset_Btn;

    [Header("窗口调整大小")]
    public Button Window_adjustSize_Increase_Btn;
    public Button Window_adjustSize_Decrease_Btn;
    public Button Window_adjustSize_Reset_Btn;

    [Header("镜头调整位置")]
    public Button Camera_adjustPosition_Up_Btn;
    public Button Camera_adjustPosition_Down_Btn;
    public Button Camera_adjustPosition_Left_Btn;
    public Button Camera_adjustPosition_Right_Btn;
    public Button Camera_adjustPosition_Reset_Btn;

    [Header("镜头调整角度")]
    public Button Camera_adjustAngle_Up_Btn;
    public Button Camera_adjustAngle_Down_Btn;
    public Button Camera_adjustAngle_Left_Btn;
    public Button Camera_adjustAngle_Right_Btn;
    public Button Camera_adjustAngle_Reset_Btn;

    [Header("窗口调整深度")]
    public Button Window_adjustDepth_Increase_Btn;
    public Button Window_adjustDepth_Decrease_Btn;
    public Button Window_adjustDepth_Reset_Btn;

    private Dictionary<MyButtonTriggerEvent, TriggerConfig> triggerConfigs = new Dictionary<MyButtonTriggerEvent, TriggerConfig>();

    public Button isVisible_Btn;
    public Button topMost;
    public Button ExitBtn;
    public TextMeshProUGUI canSee;
    public const string _lanugeCanSee = "可见";
    public const string _lanugeCantSee = "不可见";

    [Header("窗口层级与暂停")]
    public Button Window_TopMost_Btn;        // 窗口置顶按钮
    public Button Window_PauseAndHide_Btn;   // 隐藏并暂停按钮
    public TextMeshProUGUI pauseStateText;   // 暂停状态文本
    public const string _langRunning = "运行中";
    public const string _langPaused = "已暂停";

    [System.Serializable]
    public class TriggerConfig
    {
        public MyButtonTriggerEvent trigger;
        public Vector2Int moveDirection;
        public float activationDelay = 0.5f;
        public float repeatInterval = 0.1f;
        public UnityEngine.Events.UnityAction action;
        [HideInInspector] public float holdDuration;
        [HideInInspector] public float lastTriggerTime;
    }

    public override bool Init()
    {
        if (InitCompelete)
            return true;

        if(WebSocketServerManager.TryGetActiveClient(out var client))
        {
            SetCanSeeText(client.GetShowWindow());
        }

        ExitBtn.AddDeskTopListener(() =>
        {
            CloseOrReturnUIForms();
        });

        Window_adjustPosition_Up_Btn.AddDeskTopStillClipListener(() =>
        {
            WindowManager.Instance.MoveAddWindow(new Vector2Int(0, -SysDefine.WindowMoveY));
        });
        Window_adjustPosition_Down_Btn.AddDeskTopStillClipListener(() =>
        {
            WindowManager.Instance.MoveAddWindow(new Vector2Int(0, SysDefine.WindowMoveY));
        });
        Window_adjustPosition_Left_Btn.AddDeskTopStillClipListener(() =>
        {
            WindowManager.Instance.MoveAddWindow(new Vector2Int(-SysDefine.WindowMoveX, 0));
        });
        Window_adjustPosition_Right_Btn.AddDeskTopStillClipListener(() =>
        {
            WindowManager.Instance.MoveAddWindow(new Vector2Int(SysDefine.WindowMoveX, 0));
        });
        Window_adjustPosition_Reset_Btn.AddDeskTopListener(() =>
        {
            WindowManager.Instance.ResetWindowPosition();
        });
        Window_adjustSize_Increase_Btn.AddDeskTopListener(() =>
        {
            WindowManager.Instance.SetWindowSizeScaleProportionally(1.05f);
        });
        Window_adjustSize_Decrease_Btn.AddDeskTopListener(() =>
        {
            WindowManager.Instance.SetWindowSizeScaleProportionally(0.95f);
        });
        Window_adjustSize_Reset_Btn.AddDeskTopListener(() =>
        {
            WindowManager.Instance.ResetWindowSize();
        });

        Camera_adjustAngle_Down_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(0, -3);
        });
        Camera_adjustAngle_Up_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(0, 3);
        });
        Camera_adjustAngle_Left_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(-3, 0);
        });
        Camera_adjustAngle_Right_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(3, 0);
        });
        Camera_adjustAngle_Reset_Btn.AddDeskTopListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(99,99);
        });
        Camera_adjustPosition_Down_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlPosMoveCommand(0, -5);
        });
        Camera_adjustPosition_Up_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlPosMoveCommand(0, 5);
        });
        Camera_adjustPosition_Left_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlPosMoveCommand(-5, 0);
        });
        Camera_adjustPosition_Right_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlPosMoveCommand(5, 0);
        });
        Camera_adjustPosition_Reset_Btn.AddDeskTopListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlPosMoveCommand(99, 99);
        });

        Window_adjustDepth_Increase_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlScrollCommand(0.05f);
        });
        Window_adjustDepth_Decrease_Btn.AddDeskTopStillClipListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlScrollCommand(-0.05f);
        });
        Window_adjustDepth_Reset_Btn.AddDeskTopListener(() =>
        {
            WebSocketServerManager.GetActiveClient()?.SendCameraControlScrollCommand(99f);
        });

        isVisible_Btn.AddDeskTopListener(() =>
        {
            if(WebSocketServerManager.TryGetActiveClient(out var client))
            {
                WindowManager.Instance.SetIsVisibleWindow(!client.GetShowWindow());
                SetCanSeeText(client.GetShowWindow());
            }
        });
        topMost.AddDeskTopListener(() =>
        {
            WindowManager.Instance.TopMostWindow();
        });

        // 窗口置顶按钮（新增）
        if (Window_TopMost_Btn != null)
        {
            Window_TopMost_Btn.AddDeskTopListener(() =>
            {
                WindowManager.Instance.TopMostWindow();
            });
        }

        // 隐藏并暂停按钮（新增）
        if (Window_PauseAndHide_Btn != null)
        {
            // 初始化暂停状态文本
            UpdatePauseStateText();

            Window_PauseAndHide_Btn.AddDeskTopListener(() =>
            {
                // 切换暂停状态
                bool currentPaused = WindowManager.Instance.GetCurrentBioTankPausedState();
                WindowManager.Instance.SetPausedAndHideWindow(!currentPaused);
                UpdatePauseStateText();
            });
        }

        return base.Init();
    }
    private void SetCanSeeText(bool isShow)
    {
        if (isShow)
        {
            canSee.text = _lanugeCanSee;
        }
        else
        {
            canSee.text = _lanugeCantSee;
        }
    }

    /// <summary>
    /// 更新暂停状态文本
    /// </summary>
    private void UpdatePauseStateText()
    {
        if (pauseStateText == null) return;

        bool isPaused = WindowManager.Instance.GetCurrentBioTankPausedState();
        pauseStateText.text = isPaused ? _langPaused : _langRunning;
    }

}
