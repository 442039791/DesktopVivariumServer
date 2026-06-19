using UnityEngine;

/// <summary>
/// 全局设置管理器
/// [重构] 路径管理已委托给 PathManager
/// </summary>
public class GlobalSetting : UnitySingleton<GlobalSetting>
{
    public Transform BioTankNode;
    public Transform FishNode;
    public string TestConfigPath;
    public Camera UIcamera;

    /// <summary>
    /// 获取客户端可执行文件路径 - 委托给 PathManager 处理
    /// </summary>
    public string GetSecondUnityExePath()
    {
        return PathManager.Instance.GetSecondUnityExePath();
    }

    public override void Awake()
    {
        base.Awake();
        // BioTankNode = GameObject.Find("BioTankNode").transform;
        // FishNode = GameObject.Find("FishNode").transform;
    }

    /// <summary>
    /// 初始化配置 - 委托给 PathManager 处理
    /// [重构] 路径探测逻辑已统一到 PathManager
    /// </summary>
    public void OnInit()
    {
        PathManager.Instance.InitPaths(PathManager.AppType.Server);
    }
}
