using System;
using System.Reflection;

/// <summary>
/// Harmony 补丁管理器接口
/// 允许模组使用 Harmony 对游戏方法进行补丁
/// </summary>
public interface IModHarmonyManager
{
    /// <summary>
    /// 创建一个新的 Harmony 实例
    /// </summary>
    /// <param name="harmonyId">Harmony 实例的唯一ID（通常使用模组ID）</param>
    /// <returns>Harmony 包装器</returns>
    IHarmonyInstance CreateHarmony(string harmonyId);

    /// <summary>
    /// 卸载指定 Harmony 实例的所有补丁
    /// </summary>
    /// <param name="harmonyId">Harmony 实例ID</param>
    void UnpatchAll(string harmonyId);

    /// <summary>
    /// 检查 Harmony 是否可用
    /// </summary>
    bool IsHarmonyAvailable { get; }
}

/// <summary>
/// Harmony 实例包装器接口
/// </summary>
public interface IHarmonyInstance
{
    /// <summary>
    /// Harmony 实例ID
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 对方法添加前缀补丁
    /// </summary>
    /// <param name="original">原始方法</param>
    /// <param name="prefix">前缀方法（返回 false 可阻止原方法执行）</param>
    void Prefix(MethodInfo original, MethodInfo prefix);

    /// <summary>
    /// 对方法添加后缀补丁
    /// </summary>
    /// <param name="original">原始方法</param>
    /// <param name="postfix">后缀方法</param>
    void Postfix(MethodInfo original, MethodInfo postfix);

    /// <summary>
    /// 对方法添加转译器补丁（修改IL代码）
    /// </summary>
    /// <param name="original">原始方法</param>
    /// <param name="transpiler">转译器方法</param>
    void Transpiler(MethodInfo original, MethodInfo transpiler);

    /// <summary>
    /// 对类型的所有带有 HarmonyPatch 特性的方法应用补丁
    /// </summary>
    /// <param name="patchType">包含补丁方法的类型</param>
    void PatchAll(Type patchType);

    /// <summary>
    /// 从程序集中自动发现并应用所有补丁
    /// </summary>
    /// <param name="assembly">要扫描的程序集</param>
    void PatchAll(Assembly assembly);

    /// <summary>
    /// 移除此实例的所有补丁
    /// </summary>
    void UnpatchAll();
}

/// <summary>
/// 补丁优先级
/// </summary>
public static class ModPatchPriority
{
    public const int First = 0;
    public const int VeryHigh = 100;
    public const int High = 200;
    public const int Higher = 300;
    public const int Normal = 400;
    public const int Lower = 500;
    public const int Low = 600;
    public const int VeryLow = 700;
    public const int Last = 800;
}
