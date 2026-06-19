using System;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 游戏日志系统 - 提供统一的日志记录接口
/// </summary>
public class GameLogger
{
    private static GameLogger _instance;
    public static GameLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameLogger();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 当前日志级别（低于此级别的日志将被过滤）
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; }

    /// <summary>
    /// 是否启用堆栈跟踪
    /// </summary>
    public bool EnableStackTrace { get; set; }

    /// <summary>
    /// 日志前缀格式
    /// </summary>
    private const string LOG_PREFIX = "[{0}] [{1}] {2}";

    private GameLogger()
    {
#if UNITY_EDITOR
        MinimumLogLevel = LogLevel.Debug;
        EnableStackTrace = true;
#else
        MinimumLogLevel = LogLevel.Info;
        EnableStackTrace = false;
#endif
    }

    #region 公共日志方法

    /// <summary>
    /// 记录调试信息
    /// </summary>
    public void Debug(string message, string context = null)
    {
        Log(LogLevel.Debug, message, context);
    }

    /// <summary>
    /// 记录一般信息
    /// </summary>
    public void Info(string message, string context = null)
    {
        Log(LogLevel.Info, message, context);
    }

    /// <summary>
    /// 记录警告
    /// </summary>
    public void Warning(string message, string context = null)
    {
        Log(LogLevel.Warning, message, context);
    }

    /// <summary>
    /// 记录错误
    /// </summary>
    public void Error(string message, string context = null)
    {
        Log(LogLevel.Error, message, context);
    }

    /// <summary>
    /// 记录错误（带异常信息）
    /// </summary>
    public void Error(string message, Exception exception, string context = null)
    {
        string fullMessage = $"{message}\n异常: {exception.Message}\n堆栈: {exception.StackTrace}";
        Log(LogLevel.Error, fullMessage, context);
    }

    /// <summary>
    /// 记录致命错误
    /// </summary>
    public void Fatal(string message, string context = null)
    {
        Log(LogLevel.Fatal, message, context);
    }

    /// <summary>
    /// 记录致命错误（带异常信息）
    /// </summary>
    public void Fatal(string message, Exception exception, string context = null)
    {
        string fullMessage = $"{message}\n异常: {exception.Message}\n堆栈: {exception.StackTrace}";
        Log(LogLevel.Fatal, fullMessage, context);
    }

    #endregion

    #region 核心日志方法

    /// <summary>
    /// 核心日志记录方法
    /// </summary>
    private void Log(LogLevel level, string message, string context)
    {
        // 过滤低于最小级别的日志
        if (level < MinimumLogLevel)
        {
            return;
        }

        // 格式化日志消息
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string formattedMessage = string.Format(LOG_PREFIX, timestamp, context ?? "Game", message);

        // 添加堆栈跟踪（仅在启用时）
        if (EnableStackTrace && level >= LogLevel.Error)
        {
            StackTrace stackTrace = new StackTrace(2, true);
            formattedMessage += $"\n调用堆栈:\n{stackTrace}";
        }

        // 根据日志级别输出
        switch (level)
        {
            case LogLevel.Debug:
            case LogLevel.Info:
                break;

            case LogLevel.Warning:
                UnityEngine.Debug.LogWarning(formattedMessage);
                break;

            case LogLevel.Error:
            case LogLevel.Fatal:
                UnityEngine.Debug.LogError(formattedMessage);
                break;
        }

        // 写入文件（可选）
        // WriteToFile(formattedMessage);
    }

    #endregion

    #region 条件日志（性能优化）

    /// <summary>
    /// 条件日志 - 仅在条件为true时记录
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public void DebugIf(bool condition, string message, string context = null)
    {
        if (condition)
        {
            Debug(message, context);
        }
    }

    /// <summary>
    /// 仅在编辑器模式下记录调试信息
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public void EditorDebug(string message, string context = null)
    {
        Debug(message, context);
    }

    #endregion

    #region 性能分析

    /// <summary>
    /// 创建性能计时器
    /// </summary>
    public IDisposable BeginPerformanceLog(string operationName, string context = null)
    {
        return new PerformanceLogger(operationName, context ?? "Performance");
    }

    /// <summary>
    /// 性能日志记录器
    /// </summary>
    private class PerformanceLogger : IDisposable
    {
        private readonly string _operationName;
        private readonly string _context;
        private readonly Stopwatch _stopwatch;

        public PerformanceLogger(string operationName, string context)
        {
            _operationName = operationName;
            _context = context;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Instance.Debug($"{_operationName} 耗时: {_stopwatch.ElapsedMilliseconds}ms", _context);
        }
    }

    #endregion

    #region 配置

    /// <summary>
    /// 设置日志级别
    /// </summary>
    public void SetLogLevel(LogLevel level)
    {
        MinimumLogLevel = level;
        Info($"日志级别已设置为: {level}", "GameLogger");
    }

    /// <summary>
    /// 启用/禁用堆栈跟踪
    /// </summary>
    public void SetStackTraceEnabled(bool enabled)
    {
        EnableStackTrace = enabled;
        Info($"堆栈跟踪已{(enabled ? "启用" : "禁用")}", "GameLogger");
    }

    #endregion
}
