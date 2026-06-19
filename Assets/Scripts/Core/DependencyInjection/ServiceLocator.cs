using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 服务定位器 - 提供全局服务注册和查找功能
/// 作为从单例模式迁移到依赖注入的过渡方案
///
/// 使用方法：
/// 1. 注册服务：ServiceLocator.Register&lt;IService&gt;(new ServiceImpl());
/// 2. 获取服务：var service = ServiceLocator.Get&lt;IService&gt;();
/// 3. 检查服务：if (ServiceLocator.TryGet&lt;IService&gt;(out var service)) { ... }
///
/// 注意：建议逐步迁移到构造函数注入，ServiceLocator仅用于过渡期
/// </summary>
public static class ServiceLocator
{
    /// <summary>
    /// 服务容器（接口类型 -> 服务实例）
    /// </summary>
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    /// <summary>
    /// 服务工厂容器（接口类型 -> 工厂函数）
    /// </summary>
    private static readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

    /// <summary>
    /// 是否启用调试日志
    /// </summary>
    public static bool EnableDebugLog { get; set; } = false;

    #region 注册服务

    /// <summary>
    /// 注册服务实例（单例）
    /// </summary>
    /// <typeparam name="TService">服务接口类型</typeparam>
    /// <param name="instance">服务实例</param>
    public static void Register<TService>(TService instance) where TService : class
    {
        var serviceType = typeof(TService);

        if (_services.ContainsKey(serviceType))
        {
            Debug.LogWarning($"[ServiceLocator] 服务已注册，将被覆盖: {serviceType.Name}");
            _services[serviceType] = instance;
        }
        else
        {
            _services.Add(serviceType, instance);
            if (EnableDebugLog)
            {
            }
        }
    }

    /// <summary>
    /// 注册服务工厂（延迟创建）
    /// </summary>
    /// <typeparam name="TService">服务接口类型</typeparam>
    /// <param name="factory">服务工厂函数</param>
    public static void RegisterFactory<TService>(Func<TService> factory) where TService : class
    {
        var serviceType = typeof(TService);

        if (_factories.ContainsKey(serviceType))
        {
            Debug.LogWarning($"[ServiceLocator] 服务工厂已注册，将被覆盖: {serviceType.Name}");
        }

        _factories[serviceType] = () => factory();

        if (EnableDebugLog)
        {
        }
    }

    /// <summary>
    /// 注册服务类型（自动创建实例）
    /// </summary>
    /// <typeparam name="TService">服务接口类型</typeparam>
    /// <typeparam name="TImplementation">服务实现类型</typeparam>
    public static void Register<TService, TImplementation>()
        where TService : class
        where TImplementation : TService, new()
    {
        RegisterFactory<TService>(() => new TImplementation());
    }

    #endregion

    #region 获取服务

    /// <summary>
    /// 获取服务实例（如果不存在会抛出异常）
    /// </summary>
    /// <typeparam name="TService">服务接口类型</typeparam>
    /// <returns>服务实例</returns>
    public static TService Get<TService>() where TService : class
    {
        var serviceType = typeof(TService);

        // 1. 优先从已创建的实例中获取
        if (_services.TryGetValue(serviceType, out var service))
        {
            return service as TService;
        }

        // 2. 尝试从工厂创建
        if (_factories.TryGetValue(serviceType, out var factory))
        {
            var instance = factory() as TService;
            _services[serviceType] = instance; // 缓存实例

            if (EnableDebugLog)
            {
            }

            return instance;
        }

        // 3. 服务未注册
        throw new InvalidOperationException(
            $"[ServiceLocator] 服务未注册: {serviceType.Name}\n" +
            $"请先调用 ServiceLocator.Register<{serviceType.Name}>(instance) 或 RegisterFactory");
    }

    /// <summary>
    /// 尝试获取服务实例
    /// </summary>
    /// <typeparam name="TService">服务接口类型</typeparam>
    /// <param name="service">输出的服务实例</param>
    /// <returns>是否成功获取</returns>
    public static bool TryGet<TService>(out TService service) where TService : class
    {
        try
        {
            service = Get<TService>();
            return service != null;
        }
        catch
        {
            service = null;
            return false;
        }
    }

    /// <summary>
    /// 检查服务是否已注册
    /// </summary>
    /// <typeparam name="TService">服务接口类型</typeparam>
    /// <returns>是否已注册</returns>
    public static bool IsRegistered<TService>() where TService : class
    {
        var serviceType = typeof(TService);
        return _services.ContainsKey(serviceType) || _factories.ContainsKey(serviceType);
    }

    #endregion

    #region 注销服务

    /// <summary>
    /// 注销服务
    /// </summary>
    /// <typeparam name="TService">服务接口类型</typeparam>
    public static void Unregister<TService>() where TService : class
    {
        var serviceType = typeof(TService);

        if (_services.Remove(serviceType))
        {
            if (EnableDebugLog)
            {
            }
        }

        _factories.Remove(serviceType);
    }

    /// <summary>
    /// 清空所有服务
    /// </summary>
    public static void Clear()
    {
        _services.Clear();
        _factories.Clear();

        if (EnableDebugLog)
        {
        }
    }

    #endregion

    #region 调试功能

    /// <summary>
    /// 获取所有已注册的服务类型
    /// </summary>
    public static IEnumerable<Type> GetRegisteredServices()
    {
        foreach (var key in _services.Keys)
        {
            yield return key;
        }

        foreach (var key in _factories.Keys)
        {
            if (!_services.ContainsKey(key))
            {
                yield return key;
            }
        }
    }

    /// <summary>
    /// 打印所有已注册的服务（用于调试）
    /// </summary>
    public static void PrintRegisteredServices()
    {
        foreach (var kvp in _services)
        {
        }

        foreach (var kvp in _factories)
        {
        }
    }

    #endregion
}
