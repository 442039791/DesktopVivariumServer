using UnityEngine;
using WebSocketSharp;
using Common;
using System;
using Network.Commands;

/// <summary>
/// 服务器命令处理器 - 使用命令模式处理所有来自客户端的命令
/// 重构版本: 通过命令工厂分发命令,遵循开闭原则
/// </summary>
public class ServerCommandProcessor : MonoSingleton<ServerCommandProcessor>
{
    private ServerCommandFactory _commandFactory;

    /// <summary>
    /// 初始化命令工厂
    /// </summary>
    public override void Init()
    {
        base.Init();
        _commandFactory = ServerCommandFactory.Instance;
    }

    /// <summary>
    /// 处理接收到的命令消息 - 重构版本
    /// </summary>
    /// <param name="jsonData">JSON格式的命令数据</param>
    /// <param name="clientId">客户端ID</param>
    /// <param name="rawMessage">原始消息（用于回调）</param>
    public void ProcessCommand(string jsonData, int clientId, string rawMessage)
    {

        try
        {
            // 解析命令类型
            var receiveData = JsonUtility.FromJson<ReceiveData>(jsonData);
            if (receiveData == null)
            {
                Debug.LogError($"[ServerCommandProcessor] JSON解析失败: {jsonData}");
                return;
            }

            var commandType = GetCommandTypeFromInt(receiveData.type);

            // 确保命令工厂已初始化
            if (_commandFactory == null)
            {
                Debug.LogWarning("[ServerCommandProcessor] 命令工厂未初始化,正在初始化...");
                _commandFactory = ServerCommandFactory.Instance;
            }

            // 从工厂获取命令
            var command = _commandFactory.GetCommand(commandType);
            if (command == null)
            {
                Debug.LogWarning($"[ServerCommandProcessor] 未注册的命令类型: {commandType}");
                return;
            }

            // 执行命令
            var task = command.Execute(jsonData, clientId);

            // 如果命令返回任务,加入队列
            if (task != null)
            {
                WebSocketServerManager.EnqueueTask(task);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ServerCommandProcessor] 处理命令时发生异常\n" +
                          $"数据: {jsonData}\n" +
                          $"错误: {ex.Message}\n" +
                          $"堆栈: {ex.StackTrace}");
        }
    }

    #region 旧版命令处理方法 - 已弃用,保留用于向后兼容
    // 注意: 这些方法已被命令模式取代,位于 Network.Commands 命名空间
    // 如需添加新命令,请在 Commands 文件夹中创建新的命令类,并在 ServerCommandFactory 中注册
    // 这些方法将在后续版本中完全移除

    #endregion

    #region 辅助方法

    /// <summary>
    /// 将整数转换为命令类型枚举
    /// </summary>
    private CommandType GetCommandTypeFromInt(int typeValue)
    {
        if (System.Enum.IsDefined(typeof(CommandType), typeValue))
        {
            return (CommandType)typeValue;
        }
        else
        {
            Debug.LogWarning("Invalid type value");
            return CommandType.ShowWindow; // 默认值
        }
    }

    #endregion
}
