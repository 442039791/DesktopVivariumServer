using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 重置窗口位置命令
    /// </summary>
    public class ResetWindowPositionCommand : ServerCommandBase
    {
        public override CommandType Type => CommandType.ResetWindowPosition;

        /// <summary>
        /// 需要在主线程执行（访问Unity API）
        /// </summary>
        protected override bool RequiresMainThread => true;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            WindowManager.Instance.ResetWindow();
        }
    }
}
