using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// SaveSystem扩展方法 - 提供泛型方法以减少代码重复
/// </summary>
public static class SaveSystemExtensions
{
    /// <summary>
    /// 泛型收集方法 - 从集合中收集状态数据
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TStateData">状态数据类型</typeparam>
    /// <param name="entities">实体集合</param>
    /// <param name="stateDataSelector">状态数据选择器</param>
    /// <param name="generateNewIds">是否生成新ID</param>
    /// <param name="idAssigner">ID分配器 (可选)</param>
    /// <returns>状态数据数组</returns>
    public static TStateData[] CollectStateData<TEntity, TStateData>(
        this IEnumerable<TEntity> entities,
        Func<TEntity, TStateData> stateDataSelector,
        bool generateNewIds = false,
        Action<TStateData> idAssigner = null)
        where TStateData : class
    {
        if (entities == null)
        {
            return Array.Empty<TStateData>();
        }

        var dataList = new List<TStateData>();

        foreach (var entity in entities)
        {
            if (entity == null)
            {
                continue;
            }

            var stateData = stateDataSelector(entity);
            if (stateData == null)
            {
                continue;
            }

            // 深拷贝 - 使用反射调用DeepCopy方法
            var deepCopyMethod = stateData.GetType().GetMethod("DeepCopy", BindingFlags.Public | BindingFlags.Instance);
            if (deepCopyMethod == null)
            {
                UnityEngine.Debug.LogError($"[SaveSystemExtensions] 类型 {stateData.GetType().Name} 没有DeepCopy方法");
                continue;
            }

            var dataCopy = deepCopyMethod.Invoke(stateData, null) as TStateData;
            if (dataCopy == null)
            {
                UnityEngine.Debug.LogError($"[SaveSystemExtensions] DeepCopy返回null: {stateData.GetType().Name}");
                continue;
            }

            // 如果需要生成新ID,执行ID分配
            if (generateNewIds && idAssigner != null)
            {
                idAssigner(dataCopy);
            }

            dataList.Add(dataCopy);
        }

        return dataList.ToArray();
    }

    /// <summary>
    /// 从字典值集合收集状态数据
    /// </summary>
    public static TStateData[] CollectFromDictionary<TKey, TValue, TStateData>(
        this Dictionary<TKey, TValue> dictionary,
        Func<TValue, TStateData> stateDataSelector,
        bool generateNewIds = false,
        Action<TStateData> idAssigner = null)
        where TStateData : class
    {
        return dictionary?.Values.CollectStateData(stateDataSelector, generateNewIds, idAssigner)
               ?? Array.Empty<TStateData>();
    }

    /// <summary>
    /// 从只读字典值集合收集状态数据
    /// </summary>
    public static TStateData[] CollectFromDictionary<TKey, TValue, TStateData>(
        this IReadOnlyDictionary<TKey, TValue> dictionary,
        Func<TValue, TStateData> stateDataSelector,
        bool generateNewIds = false,
        Action<TStateData> idAssigner = null)
        where TStateData : class
    {
        return dictionary?.Values.CollectStateData(stateDataSelector, generateNewIds, idAssigner)
               ?? Array.Empty<TStateData>();
    }
}

/// <summary>
/// 深拷贝接口 - 所有需要深拷贝的状态数据应实现此接口
/// </summary>
public interface IDeepCopyable<T>
{
    T DeepCopy();
}
