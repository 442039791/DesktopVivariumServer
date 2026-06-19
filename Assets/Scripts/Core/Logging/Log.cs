using System;

/// <summary>
/// 日志助手 - 提供便捷的静态日志方法
/// 使用方式: Log.Info("消息"), Log.Error("错误")
/// </summary>
public static class Log
{
    #region 基础日志方法

    /// <summary>
    /// 调试信息
    /// </summary>
    public static void Debug(string message, string context = null)
    {
        GameLogger.Instance.Debug(message, context);
    }

    /// <summary>
    /// 一般信息
    /// </summary>
    public static void Info(string message, string context = null)
    {
        GameLogger.Instance.Info(message, context);
    }

    /// <summary>
    /// 警告
    /// </summary>
    public static void Warning(string message, string context = null)
    {
        GameLogger.Instance.Warning(message, context);
    }

    /// <summary>
    /// 错误
    /// </summary>
    public static void Error(string message, string context = null)
    {
        GameLogger.Instance.Error(message, context);
    }

    /// <summary>
    /// 错误（带异常）
    /// </summary>
    public static void Error(string message, Exception exception, string context = null)
    {
        GameLogger.Instance.Error(message, exception, context);
    }

    /// <summary>
    /// 致命错误
    /// </summary>
    public static void Fatal(string message, string context = null)
    {
        GameLogger.Instance.Fatal(message, context);
    }

    /// <summary>
    /// 致命错误（带异常）
    /// </summary>
    public static void Fatal(string message, Exception exception, string context = null)
    {
        GameLogger.Instance.Fatal(message, exception, context);
    }

    #endregion

    #region 格式化日志

    /// <summary>
    /// 格式化调试信息
    /// </summary>
    public static void DebugFormat(string format, params object[] args)
    {
        GameLogger.Instance.Debug(string.Format(format, args));
    }

    /// <summary>
    /// 格式化一般信息
    /// </summary>
    public static void InfoFormat(string format, params object[] args)
    {
        GameLogger.Instance.Info(string.Format(format, args));
    }

    /// <summary>
    /// 格式化警告
    /// </summary>
    public static void WarningFormat(string format, params object[] args)
    {
        GameLogger.Instance.Warning(string.Format(format, args));
    }

    /// <summary>
    /// 格式化错误
    /// </summary>
    public static void ErrorFormat(string format, params object[] args)
    {
        GameLogger.Instance.Error(string.Format(format, args));
    }

    #endregion

    #region 性能日志

    /// <summary>
    /// 开始性能计时
    /// </summary>
    public static IDisposable BeginPerformance(string operationName, string context = null)
    {
        return GameLogger.Instance.BeginPerformanceLog(operationName, context);
    }

    #endregion

    #region 条件日志

    /// <summary>
    /// 条件调试
    /// </summary>
    public static void DebugIf(bool condition, string message, string context = null)
    {
        if (condition)
        {
            GameLogger.Instance.Debug(message, context);
        }
    }

    /// <summary>
    /// 仅编辑器调试
    /// </summary>
    public static void EditorDebug(string message, string context = null)
    {
        GameLogger.Instance.EditorDebug(message, context);
    }

    #endregion

    #region 特殊日志

    /// <summary>
    /// 记录网络日志
    /// </summary>
    public static void Network(string message)
    {
        Info(message, "Network");
    }

    /// <summary>
    /// 记录保存系统日志
    /// </summary>
    public static void Save(string message)
    {
        Info(message, "SaveSystem");
    }

    /// <summary>
    /// 记录命令日志
    /// </summary>
    public static void Command(string commandType, string message)
    {
        Debug($"[{commandType}] {message}", "Command");
    }

    /// <summary>
    /// 记录实体日志
    /// </summary>
    public static void Entity(string entityType, string message)
    {
        Debug($"[{entityType}] {message}", "Entity");
    }

    #endregion
}
