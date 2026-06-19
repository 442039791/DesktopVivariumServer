using UnityEngine;
using Common;

namespace Network.Commands
{
    /// <summary>
    /// 蓝图生物罐命令处理器
    /// </summary>
    public class BluePrintBioTankCommandHandler : ServerCommandBase
    {
        public override CommandType Type => CommandType.BluePrintBioTankCommand;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            // 注意：这里解析的是数据模型BluePrintBioTankCommand（来自NetworkDataModels.cs）
            var data = ParseJson<global::BluePrintBioTankCommand>(jsonData);
            if (data == null)
            {
                return;
            }

            switch (data.commandType)
            {
                case BluePrintBioTankCommandType.Save:
                    event_manager.instance.dispatch_UIevent(SysDefine.UI_BluePrint_Save_BioTank, data.objID);
                    break;

                default:
                    Debug.LogWarning($"[BluePrintBioTank] 未知命令类型: {data.commandType}");
                    break;
            }
        }
    }
}
