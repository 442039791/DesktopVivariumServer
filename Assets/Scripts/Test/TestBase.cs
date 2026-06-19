using UnityEngine;

/// <summary>
/// 测试基类 - 提供通用的测试辅助方法
/// </summary>
public abstract class TestBase
{
    protected virtual void Setup()
    {
        // 测试前准备
        Log.Info("测试开始", GetType().Name);
    }

    protected virtual void TearDown()
    {
        // 测试后清理
        Log.Info("测试结束", GetType().Name);
    }

    #region 断言方法

    protected void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Log.Error($"断言失败: {message}", GetType().Name);
            throw new System.Exception($"断言失败: {message}");
        }
    }

    protected void AssertEqual<T>(T expected, T actual, string message = null)
    {
        if (!expected.Equals(actual))
        {
            string errorMsg = message ?? $"期望值: {expected}, 实际值: {actual}";
            Log.Error($"断言失败: {errorMsg}", GetType().Name);
            throw new System.Exception($"断言失败: {errorMsg}");
        }
    }

    protected void AssertNotNull(object obj, string message = "对象不应为null")
    {
        if (obj == null)
        {
            Log.Error($"断言失败: {message}", GetType().Name);
            throw new System.Exception($"断言失败: {message}");
        }
    }

    #endregion

    #region 性能测试

    protected void MeasurePerformance(System.Action action, string operationName, int iterations = 1000)
    {
        using (Log.BeginPerformance($"{operationName} x{iterations}", GetType().Name))
        {
            for (int i = 0; i < iterations; i++)
            {
                action?.Invoke();
            }
        }
    }

    #endregion
}
