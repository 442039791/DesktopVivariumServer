using UnityEngine;
using System;

namespace Network.Commands
{
    /// <summary>
    /// 服务器命令抽象基类 - 提供通用功能
    /// </summary>
    public abstract class ServerCommandBase : IServerCommand
    {
        public abstract CommandType Type { get; }

        /// <summary>
        /// 是否需要在主线程执行（默认为 false）
        /// 子类可以重写此属性以指定是否需要在主线程执行
        /// 如果为 false，命令将在接收线程直接执行，避免额外的排队延迟
        /// </summary>
        protected virtual bool RequiresMainThread => false;

        /// <summary>
        /// 执行命令 - 模板方法（可被子类重写）
        /// 优化版本：默认直接执行，只有 RequiresMainThread = true 时才创建 Task
        /// </summary>
        public virtual WebSocketServerManager.Task Execute(string jsonData, int clientId)
        {
            // 优化：如果不需要主线程，直接执行，返回 null
            if (!RequiresMainThread)
            {
                try
                {
                    ExecuteInternal(jsonData, clientId);
                }
                catch (Exception ex)
                {
                    HandleError(ex, jsonData, clientId);
                }
                return null; // 不需要创建 Task
            }

            // 需要主线程时才创建 Task
            var task = new WebSocketServerManager.Task
            {
                Message = Type.ToString(),
                Callback = (message) =>
                {
                    try
                    {
                        ExecuteInternal(jsonData, clientId);
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex, jsonData, clientId);
                    }
                }
            };

            return task;
        }

        /// <summary>
        /// 子类实现具体的命令逻辑
        /// </summary>
        protected abstract void ExecuteInternal(string jsonData, int clientId);

        /// <summary>
        /// 统一的错误处理
        /// </summary>
        protected virtual void HandleError(Exception ex, string jsonData, int clientId)
        {
            Debug.LogError($"[命令执行失败] 类型: {Type}, 客户端ID: {clientId}\n" +
                          $"错误: {ex.Message}\n" +
                          $"数据: {jsonData}\n" +
                          $"堆栈: {ex.StackTrace}");
        }

        /// <summary>
        /// 安全地解析JSON数据
        /// </summary>
        protected T ParseJson<T>(string jsonData) where T : class
        {
            try
            {
                var data = JsonUtility.FromJson<T>(jsonData);
                if (data == null)
                {
                    Debug.LogError($"JSON解析返回null: {typeof(T).Name}");
                }
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON解析失败: {typeof(T).Name}\n错误: {ex.Message}\n数据: {jsonData}");
                return null;
            }
        }

        /// <summary>
        /// 获取活动客户端
        /// </summary>
        protected ControlBehavior GetActiveClient()
        {
            return WebSocketServerManager.GetActiveClient();
        }

        /// <summary>
        /// 根据ID获取客户端
        /// </summary>
        protected ControlBehavior GetClient(int clientId)
        {
            return WebSocketServerManager.GetClient(clientId);
        }
    }
}
