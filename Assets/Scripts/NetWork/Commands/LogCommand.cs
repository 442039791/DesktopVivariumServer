using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 日志命令 - 接收客户端的日志消息
    /// </summary>
    public class LogCommand : ServerCommandBase
    {
        public override CommandType Type => CommandType.Log;

        /// <summary>
        /// 日志命令不需要主线程，可以直接执行（默认行为）
        /// 注意：已移除重写的 Execute 方法，使用基类的优化实现
        /// </summary>
        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            var data = ParseJson<LogDescribe>(jsonData);
            if (data == null)
            {
                return;
            }

            Debug.LogError($"[客户端日志] 客户端ID: {clientId}, 消息: {data.describe}");
        }
    }
}
