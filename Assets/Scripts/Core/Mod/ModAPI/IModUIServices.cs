using System;
using UnityEngine;
using SUIFW;

/// <summary>
/// 模组 UI 服务接口
/// 允许模组注册、显示和管理自定义 UI 界面
/// </summary>
public interface IModUIServices
{
    /// <summary>
    /// 从 AssetBundle 注册 UI 预制体
    /// </summary>
    /// <param name="uiName">UI 名称（用于后续显示/关闭）</param>
    /// <param name="bundlePath">AssetBundle 文件的相对路径（相对于模组根目录）</param>
    /// <param name="prefabName">预制体在 AssetBundle 中的名称</param>
    /// <returns>是否注册成功</returns>
    bool RegisterUIFromBundle(string uiName, string bundlePath, string prefabName);

    /// <summary>
    /// 从模组目录注册 UI 预制体（需要预先打包的 AssetBundle）
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <param name="prefabBundlePath">预制体 AssetBundle 的相对路径</param>
    /// <returns>是否注册成功</returns>
    bool RegisterUI(string uiName, string prefabBundlePath);

    /// <summary>
    /// 使用代码动态创建并注册 UI
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <param name="createFunc">创建 UI GameObject 的函数</param>
    /// <returns>是否注册成功</returns>
    bool RegisterUIWithFactory(string uiName, Func<Transform, GameObject> createFunc);

    /// <summary>
    /// 显示已注册的模组 UI
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <returns>UI 窗体实例，如果失败返回 null</returns>
    BaseUIForms ShowUI(string uiName);

    /// <summary>
    /// 关闭模组 UI
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    void CloseUI(string uiName);

    /// <summary>
    /// 检查 UI 是否已注册
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <returns>是否已注册</returns>
    bool IsUIRegistered(string uiName);

    /// <summary>
    /// 检查 UI 是否正在显示
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <returns>是否正在显示</returns>
    bool IsUIShowing(string uiName);

    /// <summary>
    /// 获取 UI 实例（如果已创建）
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    /// <returns>UI 窗体实例</returns>
    BaseUIForms GetUI(string uiName);

    /// <summary>
    /// 注销 UI
    /// </summary>
    /// <param name="uiName">UI 名称</param>
    void UnregisterUI(string uiName);

    /// <summary>
    /// 注销该模组的所有 UI
    /// </summary>
    void UnregisterAllUI();

    /// <summary>
    /// 获取 UI 根节点 Canvas 的 Transform
    /// </summary>
    Transform GetCanvasTransform();

    /// <summary>
    /// 获取 Normal 层级的 Transform（普通全屏界面）
    /// </summary>
    Transform GetNormalLayerTransform();

    /// <summary>
    /// 获取 PopUp 层级的 Transform（弹出窗口）
    /// </summary>
    Transform GetPopUpLayerTransform();

    /// <summary>
    /// 获取 Fixed 层级的 Transform（固定界面）
    /// </summary>
    Transform GetFixedLayerTransform();
}

/// <summary>
/// 模组 UI 注册信息
/// </summary>
public class ModUIRegistration
{
    /// <summary>
    /// UI 名称
    /// </summary>
    public string UIName { get; set; }

    /// <summary>
    /// 所属模组 ID
    /// </summary>
    public string ModId { get; set; }

    /// <summary>
    /// AssetBundle 路径（如果使用 AssetBundle 加载）
    /// </summary>
    public string BundlePath { get; set; }

    /// <summary>
    /// 预制体名称
    /// </summary>
    public string PrefabName { get; set; }

    /// <summary>
    /// 已加载的预制体
    /// </summary>
    public GameObject LoadedPrefab { get; set; }

    /// <summary>
    /// 动态创建函数
    /// </summary>
    public Func<Transform, GameObject> CreateFunc { get; set; }

    /// <summary>
    /// 已实例化的 UI
    /// </summary>
    public BaseUIForms Instance { get; set; }

    /// <summary>
    /// 是否正在显示
    /// </summary>
    public bool IsShowing { get; set; }
}
