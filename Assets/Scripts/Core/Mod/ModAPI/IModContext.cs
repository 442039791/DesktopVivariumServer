/// <summary>
/// 模组上下文接口 - 提供游戏API访问
/// </summary>
public interface IModContext
{
    /// <summary>
    /// 模组路径
    /// </summary>
    string ModPath { get; }

    /// <summary>
    /// 模组ID
    /// </summary>
    string ModId { get; }

    /// <summary>
    /// 模组日志记录器
    /// </summary>
    IModLogger Logger { get; }

    /// <summary>
    /// 模组资源加载器
    /// </summary>
    IModAssetLoader AssetLoader { get; }

    /// <summary>
    /// 模组配置加载器
    /// </summary>
    IModConfigLoader ConfigLoader { get; }

    /// <summary>
    /// 模组事件总线（模组内部事件）
    /// </summary>
    IModEventBus EventBus { get; }

    /// <summary>
    /// 游戏服务访问
    /// </summary>
    IModGameServices GameServices { get; }

    /// <summary>
    /// 游戏事件订阅
    /// </summary>
    IModGameEvents GameEvents { get; }

    /// <summary>
    /// 游戏钩子系统
    /// </summary>
    IModHooks Hooks { get; }

    /// <summary>
    /// Harmony 补丁管理器
    /// </summary>
    IModHarmonyManager HarmonyManager { get; }

    /// <summary>
    /// UI 服务 - 注册和管理模组自定义 UI
    /// </summary>
    IModUIServices UIServices { get; }
}

/// <summary>
/// 模组日志记录器接口
/// </summary>
public interface IModLogger
{
    void Log(string message);
    void LogWarning(string message);
    void LogError(string message);
}

/// <summary>
/// 模组资源加载器接口
/// </summary>
public interface IModAssetLoader
{
    UnityEngine.Texture2D LoadTexture(string relativePath);
    UnityEngine.Mesh LoadMesh(string relativePath);
    UnityEngine.Material LoadMaterial(string relativePath);
    string LoadText(string relativePath);
}

/// <summary>
/// 模组配置加载器接口
/// </summary>
public interface IModConfigLoader
{
    string GetConfigText(string configName);
    T GetConfig<T>(string configName) where T : class;
}

/// <summary>
/// 模组事件总线接口
/// </summary>
public interface IModEventBus
{
    void Subscribe(string eventName, System.Action<object> handler);
    void Unsubscribe(string eventName, System.Action<object> handler);
    void Publish(string eventName, object data);
}

/// <summary>
/// 游戏服务访问接口 - 提供对游戏核心功能的访问
/// </summary>
public interface IModGameServices
{
    /// <summary>
    /// 获取所有生态箱
    /// </summary>
    System.Collections.Generic.IReadOnlyList<BioTankManager> GetAllBioTanks();

    /// <summary>
    /// 获取当前活动的生态箱
    /// </summary>
    BioTankManager GetActiveBioTank();

    /// <summary>
    /// 设置生态箱更新间隔倍率
    /// </summary>
    /// <param name="multiplier">倍率（1.0为正常速度）</param>
    void SetBioTankSpeedMultiplier(float multiplier);

    /// <summary>
    /// 获取当前生态箱更新间隔倍率
    /// </summary>
    float GetBioTankSpeedMultiplier();
}
