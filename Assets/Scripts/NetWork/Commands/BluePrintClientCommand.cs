using UnityEngine;
using Common;

namespace Network.Commands
{
    /// <summary>
    /// 蓝图客户端命令
    /// </summary>
    public class BluePrintClientCommand : ServerCommandBase
    {
        public override CommandType Type => CommandType.BluePrintClientCommand;

        /// <summary>
        /// 需要在主线程执行（访问事件系统）
        /// </summary>
        protected override bool RequiresMainThread => true;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            var data = ParseJson<BluePrintClientCommandData>(jsonData);
            if (data == null)
            {
                return;
            }

            switch (data.commandType)
            {
                case BluePrintCommandType.Add:
                    event_manager.instance.dispatch_UIevent(SysDefine.UI_BluePrint_Add, data);
                    break;

                case BluePrintCommandType.Save:
                    event_manager.instance.dispatch_UIevent(SysDefine.UI_BluePrint_Save, data.id);
                    break;

                default:
                    Debug.LogWarning($"[BluePrintClient] 未知命令类型: {data.commandType}");
                    break;
            }
        }
    }
}
