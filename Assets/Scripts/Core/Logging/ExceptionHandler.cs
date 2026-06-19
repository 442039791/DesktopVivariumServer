using System;
using UnityEngine;
using Common;

/// <summary>
/// 全局异常处理器 - 提供统一的异常处理和报告
/// </summary>
public class ExceptionHandler : MonoSingleton<ExceptionHandler>
{
    /// <summary>
    /// 异常计数器
    /// </summary>
    private int _exceptionCount = 0;

    /// <summary>
    /// 最大异常阈值（超过后可能触发保护措施）
    /// </summary>
    private const int MAX_EXCEPTION_THRESHOLD = 100;

    public override void Init()
    {
        base.Init();

        // 注册Unity全局异常处理
        Application.logMessageReceived += HandleUnityLog;

        GameLogger.Instance.Info("异常处理器已初始化", "ExceptionHandler");
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    #region Unity日志处理

    /// <summary>
    /// 处理Unity日志消息
    /// </summary>
    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            _exceptionCount++;

            // 检查异常阈值
            if (_exceptionCount > MAX_EXCEPTION_THRESHOLD)
            {
                GameLogger.Instance.Fatal(
                    $"异常数量超过阈值 ({MAX_EXCEPTION_THRESHOLD})，系统可能不稳定",
                    "ExceptionHandler");
            }
        }
    }

    #endregion

    #region 异常捕获包装器

    /// <summary>
    /// 安全执行操作（自动捕获异常）
    /// </summary>
    public bool TryExecute(Action action, string operationName, string context = "Unknown")
    {
        try
        {
            action?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            HandleException(ex, operationName, context);
            return false;
        }
    }

    /// <summary>
    /// 安全执行操作并返回结果
    /// </summary>
    public bool TryExecute<T>(Func<T> func, out T result, string operationName, string context = "Unknown")
    {
        try
        {
            result = func != null ? func() : default(T);
            return true;
        }
        catch (Exception ex)
        {
            result = default(T);
            HandleException(ex, operationName, context);
            return false;
        }
    }

    /// <summary>
    /// 安全执行操作（带默认值）
    /// </summary>
    public T ExecuteWithDefault<T>(Func<T> func, T defaultValue, string operationName, string context = "Unknown")
    {
        try
        {
            return func != null ? func() : defaultValue;
        }
        catch (Exception ex)
        {
            HandleException(ex, operationName, context);
            return defaultValue;
        }
    }

    #endregion

    #region 异常处理

    /// <summary>
    /// 处理异常
    /// </summary>
    public void HandleException(Exception exception, string operationName, string context = "Unknown")
    {
        if (exception == null)
        {
            return;
        }

        _exceptionCount++;

        // 分类异常
        var exceptionType = ClassifyException(exception);

        // 记录异常
        string message = $"操作失败: {operationName}\n" +
                        $"异常类型: {exceptionType}\n" +
                        $"异常信息: {exception.Message}";

        if (exceptionType == ExceptionSeverity.Critical)
        {
            GameLogger.Instance.Fatal(message, exception, context);
        }
        else
        {
            GameLogger.Instance.Error(message, exception, context);
        }

        // 可选：上报到分析服务
        // ReportToAnalytics(exception, operationName, context);
    }

    /// <summary>
    /// 分类异常严重性
    /// </summary>
    private ExceptionSeverity ClassifyException(Exception exception)
    {
        // 致命异常
        if (exception is OutOfMemoryException ||
            exception is StackOverflowException ||
            exception is AccessViolationException)
        {
            return ExceptionSeverity.Critical;
        }

        // 一般异常
        if (exception is ArgumentException ||
            exception is InvalidOperationException ||
            exception is NullReferenceException)
        {
            return ExceptionSeverity.Normal;
        }

        return ExceptionSeverity.Low;
    }

    #endregion

    #region 统计

    /// <summary>
    /// 获取异常统计
    /// </summary>
    public string GetExceptionStats()
    {
        return $"异常总数: {_exceptionCount}";
    }

    /// <summary>
    /// 重置异常计数
    /// </summary>
    public void ResetExceptionCount()
    {
        _exceptionCount = 0;
        GameLogger.Instance.Info("异常计数已重置", "ExceptionHandler");
    }

    #endregion

    #region 内部类型

    /// <summary>
    /// 异常严重性
    /// </summary>
    private enum ExceptionSeverity
    {
        Low,      // 低 - 可忽略
        Normal,   // 正常 - 需要注意
        Critical  // 严重 - 可能导致崩溃
    }

    #endregion
}
