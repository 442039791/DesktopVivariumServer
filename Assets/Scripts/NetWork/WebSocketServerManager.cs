using UnityEngine;
using WebSocketSharp.Server;
using System;
using System.Collections.Generic;
using Common;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;

/// <summary>
/// WebSocket服务器管理器 - 负责服务器的启动、停止和任务队列管理
/// 已重构：数据模型、枚举、命令处理器和ControlBehavior已分离到独立文件
/// </summary>
public class WebSocketServerManager : MonoSingleton<WebSocketServerManager>
{
    #region 内部类

    /// <summary>
    /// 任务类 - 用于队列化处理异步回调
    /// </summary>
    public class Task
    {
        public string Message;
        public Action<string> Callback;
    }

    #endregion

    #region 静态字段

    private static readonly Queue<Task> queue = new();
    private static readonly object queueLock = new object();
    public static int ActionEventID = 0;

    /// <summary>
    /// �̰߳�ȫ���ɺ�һ��ActionEventIDֵ
    /// </summary>
    public static int GetNextActionEventID()
    {
        // ʹ��Interlocked.Incrementȷ��++��������ԭ������
        return Interlocked.Increment(ref ActionEventID);
    }

    #endregion

    #region 私有字段

    private WebSocketServer wssv;

    #endregion

    #region 配置常量

    /// <summary>
    /// 单帧最大处理任务数 - 防止大量任务积压导致单帧卡顿
    /// </summary>
    private const int MAX_TASKS_PER_FRAME = 50;

    #endregion

    #region Unity生命周期

    void Start()
    {
        // 启动 WebSocket 服务器（带重试机制，避免端口竞争）
        int maxRetries = 3;
        bool serverStarted = false;

        for (int retry = 0; retry < maxRetries && !serverStarted; retry++)
        {
            int port = GetAvailablePort();

            if (retry > 0)
            {
                UnityEngine.Debug.Log($"[WebSocketServerManager] 第 {retry + 1} 次重试启动服务器...");
            }

            try
            {
                // 使用 IPAddress.Loopback (127.0.0.1) 绑定
                wssv = new WebSocketServer(System.Net.IPAddress.Loopback, port);
                wssv.AddWebSocketService<ControlBehavior>("/control");
                wssv.Start();

                // 验证服务器确实在监听
                if (wssv.IsListening)
                {
                    _currentPort = port; // 保存端口号用于退出时清理防火墙规则

                    // 写入端口文件（在确认监听后再写入）
                    var path = Application.streamingAssetsPath + "/port.txt";
                    UnityEngine.Debug.Log($"[WebSocketServerManager] 端口文件路径: {path}");
                    File.WriteAllText(path, port.ToString());

                    // 尝试添加防火墙规则（静默失败，不影响主流程）
                    TryAddFirewallRule(port);

                    UnityEngine.Debug.Log($"[WebSocketServerManager] WebSocket服务器已启动, IsListening={wssv.IsListening}, 地址=127.0.0.1:{port}");

                    // 诊断：检查端口是否真的在监听
                    LogListeningPorts(port);

                    serverStarted = true;
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[WebSocketServerManager] 服务器启动但IsListening=false, 端口={port}");
                    wssv.Stop();
                    wssv = null;
                }
            }
            catch (SocketException ex)
            {
                UnityEngine.Debug.LogWarning($"[WebSocketServerManager] Socket异常(可能端口被占用): 端口={port}, 错误={ex.SocketErrorCode}, {ex.Message}");
                if (wssv != null)
                {
                    try { wssv.Stop(); } catch { }
                    wssv = null;
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[WebSocketServerManager] WebSocket服务器启动失败: {ex.Message}\n{ex.StackTrace}");
                if (wssv != null)
                {
                    try { wssv.Stop(); } catch { }
                    wssv = null;
                }
            }
        }

        if (!serverStarted)
        {
            UnityEngine.Debug.LogError($"[WebSocketServerManager] 经过 {maxRetries} 次尝试后，WebSocket服务器仍无法启动");
        }
    }

    /// <summary>
    /// 诊断：记录当前端口监听状态
    /// </summary>
    private void LogListeningPorts(int expectedPort)
    {
        try
        {
            // 验证端口是否真的在监听
            var testClient = new TcpClient();
            var connectTask = testClient.BeginConnect("127.0.0.1", expectedPort, null, null);
            bool connected = connectTask.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

            if (connected)
            {
                try
                {
                    testClient.EndConnect(connectTask);
                    UnityEngine.Debug.Log($"[WebSocketServerManager] 端口验证成功: 可以连接到 127.0.0.1:{expectedPort}");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[WebSocketServerManager] 端口验证警告: {ex.Message}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[WebSocketServerManager] 端口验证失败: 无法连接到 127.0.0.1:{expectedPort}");
            }

            testClient.Close();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[WebSocketServerManager] 端口验证异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 尝试添加防火墙入站规则，允许本地连接
    /// 静默执行，失败不影响主流程
    /// </summary>
    private void TryAddFirewallRule(int port)
    {
#if !UNITY_EDITOR
        try
        {
            string appName = "DesktopVivarium_Server";
            string ruleName = $"{appName}_Port_{port}";

            // 使用 netsh 添加防火墙规则
            // 只允许本地回环地址的入站连接
            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=tcp localport={port} remoteip=127.0.0.1",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit(3000); // 最多等待3秒
                if (process.ExitCode == 0)
                {
                    UnityEngine.Debug.Log($"[WebSocketServerManager] 防火墙规则添加成功: {ruleName}");
                }
                else
                {
                    // 可能是权限不足或规则已存在，静默处理
                    string error = process.StandardError.ReadToEnd();
                    UnityEngine.Debug.Log($"[WebSocketServerManager] 防火墙规则添加跳过(可能已存在或权限不足): {error}");
                }
            }
        }
        catch (System.Exception ex)
        {
            // 静默失败，不影响主流程
            UnityEngine.Debug.Log($"[WebSocketServerManager] 防火墙规则添加失败(非致命): {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// 当前使用的端口号，用于退出时清理防火墙规则
    /// </summary>
    private int _currentPort = 0;

    void OnApplicationQuit()
    {
        if (wssv != null)
        {
            wssv.Stop();
        }

        // 清理防火墙规则
        if (_currentPort > 0)
        {
            TryRemoveFirewallRule(_currentPort);
        }
    }

    /// <summary>
    /// 尝试删除防火墙规则
    /// </summary>
    private void TryRemoveFirewallRule(int port)
    {
#if !UNITY_EDITOR
        try
        {
            string appName = "DesktopVivarium_Server";
            string ruleName = $"{appName}_Port_{port}";

            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"advfirewall firewall delete rule name=\"{ruleName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit(2000);
            }
        }
        catch
        {
            // 静默失败
        }
#endif
    }

    /// <summary>
    /// 处理任务队列 - 线程安全版本 + 批量处理
    /// </summary>
    private void Update()
    {
        int processedCount = 0;

        // 批量处理任务队列，限制单帧处理数量防止卡顿
        while (processedCount < MAX_TASKS_PER_FRAME)
        {
            Task task = null;

            lock (queueLock)
            {
                if (queue.Count > 0)
                {
                    task = queue.Dequeue();
                }
                else
                {
                    break;
                }
            }

            if (task != null && task.Callback != null)
            {
                try
                {
                    task.Callback(task.Message);
                    processedCount++;
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"任务回调执行失败: {ex.Message}\n{ex.StackTrace}");
                    processedCount++;
                }
            }
            else
            {
                processedCount++;
            }
        }

        // 如果队列中还有未处理的任务，在日志中提示
        if (queue.Count > 0 && processedCount >= MAX_TASKS_PER_FRAME)
        {
            UnityEngine.Debug.LogWarning($"[WebSocketServer] 单帧任务处理达到上限({MAX_TASKS_PER_FRAME}条)，剩余{queue.Count}条任务将在下一帧处理");
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 线程安全地将任务加入队列
    /// </summary>
    public static void EnqueueTask(Task task)
    {
        if (task == null)
        {
            UnityEngine.Debug.LogWarning("尝试加入null任务到队列");
            return;
        }

        lock (queueLock)
        {
            queue.Enqueue(task);
        }
    }

    /// <summary>
    /// 获取一个可用的端口号
    /// </summary>
    public static int GetAvailablePort()
    {
        // 使用 TcpListener 绑定到端口 0，系统自动分配可用端口
        TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>
    /// 向所有客户端广播命令
    /// </summary>
    public void SendCommandToClient(string command)
    {
        foreach (var path in wssv.WebSocketServices.Paths)
        {
            var sessions = wssv.WebSocketServices[path].Sessions;
            sessions.Broadcast(command);
        }
    }

    #endregion

    #region 静态客户端管理方法

    /// <summary>
    /// 尝试获取当前活动的客户端
    /// </summary>
    public static bool TryGetActiveClient(out ControlBehavior client)
    {
        client = GetActiveClient();
        return client != null;
    }

    /// <summary>
    /// 获取所有客户端
    /// </summary>
    public static System.Collections.Concurrent.ConcurrentDictionary<int, ControlBehavior> GetClients()
    {
        return ControlBehavior.clients;
    }

    /// <summary>
    /// 获取所有非活动的客户端
    /// </summary>
    public static List<ControlBehavior> GetUnActiveClient()
    {
        List<ControlBehavior> clients = new List<ControlBehavior>();
        foreach (var item in ControlBehavior.clients)
        {
            if (item.Value.GetClientID() != PlayerManager.ActiveBioTanksID)
            {
                clients.Add(item.Value);
            }
        }
        if (clients.Count == 0)
            return null;
        return clients;
    }

    /// <summary>
    /// 获取当前活动的客户端
    /// </summary>
    public static ControlBehavior GetActiveClient()
    {
        return GetClient(PlayerManager.ActiveBioTanksID);
    }

    /// <summary>
    /// [已弃用] 获取当前活动的客户端 - 请使用 GetActiveClient
    /// </summary>
    [System.Obsolete("方法名拼写错误,请使用 GetActiveClient")]
    public static ControlBehavior GetAcitveClient()
    {
        return GetActiveClient();
    }

    /// <summary>
    /// 根据客户端ID获取客户端
    /// </summary>
    public static ControlBehavior GetClient(int clientId)
    {
        ControlBehavior client;
        if (ControlBehavior.clients.TryGetValue(clientId, out client))
        {
            return client;
        }
        else
        {
            return null;
        }
    }

    #endregion
}
