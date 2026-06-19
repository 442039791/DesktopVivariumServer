using System;
using UnityEngine;
using SUIFW;

/// <summary>
/// 模组 UI 服务实现
/// 为每个模组提供独立的 UI 服务接口
/// </summary>
public class ModUIServices : IModUIServices
{
    private readonly string _modId;
    private readonly ModInfo _modInfo;

    public ModUIServices(string modId, ModInfo modInfo)
    {
        _modId = modId;
        _modInfo = modInfo;
    }

    /// <summary>
    /// 从 AssetBundle 注册 UI 预制体
    /// </summary>
    public bool RegisterUIFromBundle(string uiName, string bundlePath, string prefabName)
    {
        if (ModUIManager.Instance == null)
        {
            Debug.LogError("[ModUIServices] ModUIManager 未初始化");
            return false;
        }

        return ModUIManager.Instance.RegisterUIFromBundle(_modId, uiName, bundlePath, prefabName);
    }

    /// <summary>
    /// 从模组目录注册 UI 预制体
    /// 简化版本：bundlePath 即为 AssetBundle 路径，prefabName 默认使用 uiName
    /// </summary>
    public bool RegisterUI(string uiName, string prefabBundlePath)
    {
        return RegisterUIFromBundle(uiName, prefabBundlePath, uiName);
    }

    /// <summary>
    /// 使用代码动态创建并注册 UI
    /// </summary>
    public bool RegisterUIWithFactory(string uiName, Func<Transform, GameObject> createFunc)
    {
        if (ModUIManager.Instance == null)
        {
            Debug.LogError("[ModUIServices] ModUIManager 未初始化");
            return false;
        }

        return ModUIManager.Instance.RegisterUIWithFactory(_modId, uiName, createFunc);
    }

    /// <summary>
    /// 直接使用预制体注册 UI
    /// </summary>
    public bool RegisterUIWithPrefab(string uiName, GameObject prefab)
    {
        if (ModUIManager.Instance == null)
        {
            Debug.LogError("[ModUIServices] ModUIManager 未初始化");
            return false;
        }

        return ModUIManager.Instance.RegisterUIWithPrefab(_modId, uiName, prefab);
    }

    /// <summary>
    /// 显示已注册的模组 UI
    /// </summary>
    public BaseUIForms ShowUI(string uiName)
    {
        if (ModUIManager.Instance == null)
        {
            Debug.LogError("[ModUIServices] ModUIManager 未初始化");
            return null;
        }

        return ModUIManager.Instance.ShowUI(_modId, uiName);
    }

    /// <summary>
    /// 关闭模组 UI
    /// </summary>
    public void CloseUI(string uiName)
    {
        if (ModUIManager.Instance == null) return;
        ModUIManager.Instance.CloseUI(_modId, uiName);
    }

    /// <summary>
    /// 销毁模组 UI 实例
    /// </summary>
    public void DestroyUI(string uiName)
    {
        if (ModUIManager.Instance == null) return;
        ModUIManager.Instance.DestroyUI(_modId, uiName);
    }

    /// <summary>
    /// 检查 UI 是否已注册
    /// </summary>
    public bool IsUIRegistered(string uiName)
    {
        if (ModUIManager.Instance == null) return false;
        return ModUIManager.Instance.IsUIRegistered(_modId, uiName);
    }

    /// <summary>
    /// 检查 UI 是否正在显示
    /// </summary>
    public bool IsUIShowing(string uiName)
    {
        if (ModUIManager.Instance == null) return false;
        return ModUIManager.Instance.IsUIShowing(_modId, uiName);
    }

    /// <summary>
    /// 获取 UI 实例（如果已创建）
    /// </summary>
    public BaseUIForms GetUI(string uiName)
    {
        if (ModUIManager.Instance == null) return null;
        return ModUIManager.Instance.GetUI(_modId, uiName);
    }

    /// <summary>
    /// 注销 UI
    /// </summary>
    public void UnregisterUI(string uiName)
    {
        if (ModUIManager.Instance == null) return;
        ModUIManager.Instance.UnregisterUI(_modId, uiName);
    }

    /// <summary>
    /// 注销该模组的所有 UI
    /// </summary>
    public void UnregisterAllUI()
    {
        if (ModUIManager.Instance == null) return;
        ModUIManager.Instance.UnregisterAllUI(_modId);
        ModUIManager.Instance.UnloadModBundles(_modId);
    }

    /// <summary>
    /// 获取 UI 根节点 Canvas 的 Transform
    /// </summary>
    public Transform GetCanvasTransform()
    {
        if (ModUIManager.Instance == null) return null;
        return ModUIManager.Instance.GetCanvasTransform();
    }

    /// <summary>
    /// 获取 Normal 层级的 Transform（普通全屏界面）
    /// </summary>
    public Transform GetNormalLayerTransform()
    {
        if (ModUIManager.Instance == null) return null;
        return ModUIManager.Instance.GetNormalLayerTransform();
    }

    /// <summary>
    /// 获取 PopUp 层级的 Transform（弹出窗口）
    /// </summary>
    public Transform GetPopUpLayerTransform()
    {
        if (ModUIManager.Instance == null) return null;
        return ModUIManager.Instance.GetPopUpLayerTransform();
    }

    /// <summary>
    /// 获取 Fixed 层级的 Transform（固定界面）
    /// </summary>
    public Transform GetFixedLayerTransform()
    {
        if (ModUIManager.Instance == null) return null;
        return ModUIManager.Instance.GetFixedLayerTransform();
    }

    /// <summary>
    /// 获取模组 UI 专用容器
    /// </summary>
    public Transform GetModUIContainer()
    {
        if (ModUIManager.Instance == null) return null;
        return ModUIManager.Instance.GetModUIContainer();
    }
}
