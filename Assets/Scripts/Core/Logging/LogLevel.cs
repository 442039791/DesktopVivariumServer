/// <summary>
/// 日志级别
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// 调试信息 - 仅在开发环境显示
    /// </summary>
    Debug = 0,

    /// <summary>
    /// 一般信息 - 记录正常操作
    /// </summary>
    Info = 1,

    /// <summary>
    /// 警告 - 可能的问题，但不影响运行
    /// </summary>
    Warning = 2,

    /// <summary>
    /// 错误 - 影响功能的错误
    /// </summary>
    Error = 3,

    /// <summary>
    /// 致命错误 - 导致系统崩溃的错误
    /// </summary>
    Fatal = 4
}
