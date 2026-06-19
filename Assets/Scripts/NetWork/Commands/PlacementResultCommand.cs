using System.Collections.Generic;
using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 放置结果命令 - 处理客户端发送的放置结果
    /// </summary>
    public class PlacementResultCommand : ServerCommandBase
    {
        private const int PlacementFailTemplateId = 140;
        private const int PlacementFailOccupiedId = 138;  // 修正：138="位置被占用"
        private const int PlacementFailExitBuildModeId = 141;  // 141="已退出建造模式"
        private const int PlacementFailMoneyId = 134;

        private static readonly Dictionary<string, int> FailReasonLocalizationIds = new()
        {
            { "位置被占用", PlacementFailOccupiedId },
            { "位置上已有物体", PlacementFailOccupiedId },  // 客户端发送的另一种表述，映射到同一个本地化ID
            { "已退出建造模式", PlacementFailExitBuildModeId },
            { "金币不足", PlacementFailMoneyId },
        };

        public override CommandType Type => CommandType.PlacementResult;

        /// <summary>
        /// 需要在主线程执行（访问Unity API和UI）
        /// </summary>
        protected override bool RequiresMainThread => true;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            var data = ParseJson<PlacementResultData>(jsonData);
            if (data == null)
            {
                Debug.LogError("[PlacementResult] 解析放置结果数据失败");
                return;
            }


            if (data.success)
            {
                // 放置成功，播放音效反馈
                AudioSourceManager.Instance.PlayButtonClip();

                // 放置成功，使用服务器端记录的价格扣费（不信任客户端发来的价格）
                int price = 0;
                if (UI_BioTank_Edit.CurrentInstance != null && UI_BioTank_Edit.CurrentInstance.currentBuildingItem != null)
                {
                    price = UI_BioTank_Edit.CurrentInstance.currentBuildingItem.price;
                }

                if (price > 0)
                {
                    // 扣除金币（仅服务器端处理，客户端无金币UI无需同步）
                    bool deducted = PlayerManager.Instance.OnMoneyChanged(-price);
                    if (deducted)
                    {
                    }
                    else
                    {
                        Debug.LogError($"[PlacementResult] 扣除金币失败: {price}");
                    }
                }
                else
                {
                }

                // 只有植物放置成功时才清空currentBuildingItem并退出编辑模式（植物不支持连续放置）
                // Cube和Decoration可以继续连续放置，保持Build模式
                // blockType: 0=Cube, 1=Decoration, 2=Plant
                if (data.blockType == 2 && UI_BioTank_Edit.CurrentInstance != null)
                {
                    UI_BioTank_Edit.CurrentInstance.ClearCurrentBuildingItem();
                    // 植物放置成功后退出编辑模式，这样退出删除模式时不会错误地回到放置模式
                    UI_BioTank_Edit.CurrentInstance.currentMode = BuildControlMode.Normal;
                    // 重置Build按钮UI状态
                    UI_BioTank_Edit.CurrentInstance.handle?.ResetBuildButtonState();
                    // 通知客户端退出Build模式
                    WebSocketServerManager.GetActiveClient()?.SendBuilControlCommand(BuildControlMode.Normal, false);
                }
            }
            else
            {
                // 放置失败，显示错误消息
                string reason = ResolveLocalizedReason(data.failReason);

                Debug.LogWarning($"[PlacementResult] 放置失败: {reason}");

                string template = LocalizationManager.GetCurrentLanguageByID(PlacementFailTemplateId);
                if (string.IsNullOrEmpty(template))
                {
                    template = "放置失败: {0}";
                }
                if (string.IsNullOrEmpty(reason))
                {
                    reason = LocalizationManager.GetCurrentLanguageByID(PlacementFailOccupiedId);
                    if (string.IsNullOrEmpty(reason))
                    {
                        reason = "未知原因";
                    }
                }

                // 显示错误提示给玩家
                ToastManager.Show(string.Format(template, reason));

                // 不扣除金币
            }
        }

        private static string ResolveLocalizedReason(string rawReason)
        {
            if (string.IsNullOrEmpty(rawReason))
            {
                return LocalizationManager.GetCurrentLanguageByID(PlacementFailOccupiedId);
            }

            if (FailReasonLocalizationIds.TryGetValue(rawReason, out int textId))
            {
                string localized = LocalizationManager.GetCurrentLanguageByID(textId);
                if (!string.IsNullOrEmpty(localized))
                {
                    return localized;
                }
            }

            return rawReason;
        }
    }
}
