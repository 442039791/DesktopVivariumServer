using System;

/// <summary>
/// 模组入口接口
/// 所有DLL模组必须实现此接口
/// </summary>
public interface IModEntry
{
    /// <summary>
    /// 模组ID
    /// </summary>
    string ModId { get; }

    /// <summary>
    /// 模组名称
    /// </summary>
    string ModName { get; }

    /// <summary>
    /// 模组版本
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 初始化模组
    /// </summary>
    /// <param name="context">模组上下文</param>
    void Initialize(IModContext context);

    /// <summary>
    /// 启用模组时调用
    /// </summary>
    void OnEnable();

    /// <summary>
    /// 禁用模组时调用
    /// </summary>
    void OnDisable();

    /// <summary>
    /// 卸载模组时调用
    /// </summary>
    void OnUnload();

    /// <summary>
    /// 每帧更新（可选实现）
    /// </summary>
    void OnUpdate();
}
