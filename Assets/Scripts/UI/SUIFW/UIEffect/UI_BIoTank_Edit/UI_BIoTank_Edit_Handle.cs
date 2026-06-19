using UnityEngine;
using UnityEngine.UI;

public class UI_BIoTank_Edit_Handle : MonoBehaviour
{
    public Button Btn_Move;
    public Button Btn_Build;
    public Button Btn_Delete;
    public Toggle Tgl_Move_Range_On;
    public Toggle Tgl_Move_Range_Off;
    public Toggle Tgl_Build_Range_On;
    public Toggle Tgl_Build_Range_Off;
    public Toggle Tgl_Delete_Range_On;
    public Toggle Tgl_Delete_Range_Off;
    private bool isMoveOn = false;
    private bool isBuildOn = false;
    private bool isDeleteOn = false;
    private bool isMoveRangeOn = false;
    private bool isBuildRangeOn = false;
    private bool isDeleteRangeOn = false;

    private bool InitComplete = false;
    public void Init()
    {
        if (InitComplete) return;
        InitComplete = true;
        Btn_Move.AddDeskTopListener(() => {
            SetBuildMode(BuildControlMode.Move, isMoveRangeOn,Btn_Move,ref isMoveOn);
        });
        Btn_Build.AddDeskTopListener(() => {
            SetBuildMode(BuildControlMode.Build, isBuildRangeOn,Btn_Build,ref isBuildOn);
        });
        Btn_Delete.AddDeskTopListener(() => {
            SetBuildMode(BuildControlMode.Delete, isDeleteRangeOn,Btn_Delete,ref isDeleteOn);
        });

        Tgl_Move_Range_On.isOn = false;
        Tgl_Move_Range_Off.isOn = true;
        Tgl_Build_Range_On.isOn = false;
        Tgl_Build_Range_Off.isOn = true;
        Tgl_Delete_Range_On.isOn = false;
        Tgl_Delete_Range_Off.isOn = true;
        Tgl_Move_Range_On.onValueChanged.AddListener((bool isOn) => {
            Tgl_Move_Range_On.isOn = isOn;
            Tgl_Move_Range_Off.isOn =!isOn;
            isMoveRangeOn = isOn;
            if (isMoveOn && isOn)
                WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Move, isMoveRangeOn);
        });
        Tgl_Move_Range_Off.onValueChanged.AddListener((bool isOn) => {
            Tgl_Move_Range_Off.isOn = isOn;
            Tgl_Move_Range_On.isOn = !isOn;
            isMoveRangeOn = !isOn;
            if (isMoveOn && isOn)
                WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Move, isMoveRangeOn);
        });
        Tgl_Build_Range_On.onValueChanged.AddListener((bool isOn) => {
             Tgl_Build_Range_On.isOn = isOn;
             Tgl_Build_Range_Off.isOn = !isOn;
             isBuildRangeOn = isOn;
             if (isBuildOn && isOn)
                 WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Build, isBuildRangeOn);
        });
        Tgl_Build_Range_Off.onValueChanged.AddListener((bool isOn) => {
             Tgl_Build_Range_Off.isOn = isOn;
             Tgl_Build_Range_On.isOn = !isOn;
             isBuildRangeOn = !isOn;
             if (isBuildOn && isOn)
                 WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Build, isBuildRangeOn);
            });
        Tgl_Delete_Range_On.onValueChanged.AddListener((bool isOn) => {
             Tgl_Delete_Range_On.isOn = isOn;
             Tgl_Delete_Range_Off.isOn = !isOn;
             isDeleteRangeOn = isOn;
             if (isDeleteOn && isOn)
                 WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Delete, isDeleteRangeOn);
             });
        Tgl_Delete_Range_Off.onValueChanged.AddListener((bool isOn) => {
             Tgl_Delete_Range_Off.isOn = isOn;
             Tgl_Delete_Range_On.isOn = !isOn;
             isDeleteRangeOn = !isOn;
             if (isDeleteOn && isOn)
                 WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Delete, isDeleteRangeOn);
             });
    }
    private void SetAllButtonOff()
    {
        Btn_Move.image.color = Color.white;
        Btn_Build.image.color = Color.white;
        Btn_Delete.image.color = Color.white;
        isMoveOn = false;
        isBuildOn = false;
        isDeleteOn = false;
    }
    public void ExitBuildMode()
    {
        SetAllButtonOff();
        isMoveRangeOn = false;
        isBuildRangeOn = false;
        isDeleteRangeOn = false;

        // 更新当前模式为Normal
        if (UI_BioTank_Edit.CurrentInstance != null)
        {
            UI_BioTank_Edit.CurrentInstance.currentMode = BuildControlMode.Normal;
        }

        WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Normal, false);
    }

    /// <summary>
    /// 重置Build按钮状态（植物放置成功后调用）
    /// 植物放置成功后退出编辑模式，但不发送命令（由PlacementResultCommand发送）
    /// </summary>
    public void ResetBuildButtonState()
    {
        isBuildOn = false;
        Btn_Build.image.color = Color.white;
    }
    public void ExitDeleteMode()
    {
        if (isDeleteOn)
        {
            isDeleteOn = false;
            Btn_Delete.image.color = Color.white;

            // 检查是否有选中的物品，且只有Cube和Decoration才恢复编辑模式
            // 植物放置成功后不应该恢复到放置模式（植物不支持连续放置）
            // blockType: 0=Cube, 1=Decoration, 2=Plant
            var instance = UI_BioTank_Edit.CurrentInstance;
            var item = instance?.currentBuildingItem;

            // 只有Cube(0)和Decoration(1)才恢复编辑模式，植物(2)不恢复
            if (item != null && item.blockType != 2)
            {
                // 有选中的Cube或Decoration物品，回到Build模式并恢复预览
                // 更新UI状态
                isBuildOn = true;
                Btn_Build.image.color = Color.black;
                instance.currentMode = BuildControlMode.Build;

                // 发送Build模式命令
                WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Build, isBuildRangeOn);

                // 发送创建命令恢复预览
                WebSocketServerManager.GetActiveClient()?.SendCreateBlockCommand(
                    item.itemID,
                    item.objID,
                    item.growthProgress,
                    item.blockType
                );
            }
            else
            {
                // 没有选中物品或选中的是植物，回到Normal模式
                if (instance != null)
                {
                    instance.currentMode = BuildControlMode.Normal;
                    // 如果是植物，清除currentBuildingItem
                    if (item != null && item.blockType == 2)
                    {
                        instance.ClearCurrentBuildingItem();
                    }
                }
                WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Normal, false);
            }
        }
    }
    private void SetBuildMode(BuildControlMode defalutMode,  bool RangeOn,Button btn,ref bool isOn)
    {
        // 特殊处理：从Delete模式退出时（toggle off），调用ExitDeleteMode恢复预览
        // 必须在修改isOn之前判断，因为ExitDeleteMode内部会检查isDeleteOn状态
        if (defalutMode == BuildControlMode.Delete && isOn)
        {
            // 当前处于Delete模式且按钮已打开，用户点击按钮要退出删除模式
            ExitDeleteMode();
            return;
        }

        if(!isOn)
            SetAllButtonOff();
        isOn = !isOn;
        defalutMode = isOn ? defalutMode : BuildControlMode.Normal;
        btn.image.color = isOn ? Color.black : Color.white;

        // 只在切换到Normal模式时清除建造物品信息（真正退出编辑）
        // Delete模式不清除，这样退出删除模式后可以继续放置
        if (defalutMode == BuildControlMode.Normal)
        {
            UI_BioTank_Edit.CurrentInstance?.ClearCurrentBuildingItem();
        }

        // 更新当前模式
        if (UI_BioTank_Edit.CurrentInstance != null)
        {
            UI_BioTank_Edit.CurrentInstance.currentMode = defalutMode;
        }

        WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(defalutMode, RangeOn);
    }
}
