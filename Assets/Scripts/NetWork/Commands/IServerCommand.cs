using System;

namespace Network.Commands
{
    /// <summary>
    /// 服务器命令接口 - 所有命令必须实现此接口
    /// </summary>
    public interface IServerCommand
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        CommandType Type { get; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="jsonData">JSON格式的命令数据</param>
        /// <param name="clientId">发送命令的客户端ID</param>
        /// <returns>任务对象,将被加入执行队列</returns>
        WebSocketServerManager.Task Execute(string jsonData, int clientId);
    }
}
