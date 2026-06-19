using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SUIFW;
using Common;

/// <summary>
/// 模组 UI 管理器
/// 负责加载、注册和管理模组的自定义 UI 界面
///
/// 支持两种 UI 加载方式：
/// 1. AssetBundle 方式：模组开发者在 Unity 编辑器中设计 UI，打包成 AssetBundle
/// 2. 代码创建方式：模组开发者通过代码动态创建 UI
/// </summary>
public class ModUIManager : MonoSingleton<ModUIManager>
{
    /// <summary>
    /// 所有已注册的模组 UI
    /// Key: 完整 UI 名称 (ModId:UIName)
    /// </summary>
    private readonly Dictionary<string, ModUIRegistration> _registeredUIs = new();

    /// <summary>
    /// 已加载的 AssetBundle 缓存
    /// Key: Bundle 路径
    /// </summary>
    private readonly Dictionary<string, AssetBundle> _loadedBundles = new();

    /// <summary>
    /// UI 根节点缓存
    /// </summary>
    private Transform _canvasTransform;
    private Transform _normalLayer;
    private Transform _fixedLayer;
    private Transform _popUpLayer;

    /// <summary>
    /// 模组 UI 的默认父节点（在 Normal 层下创建一个专门的容器）
    /// </summary>
    private Transform _modUIContainer;

    public override void Init()
    {
        base.Init();
        CacheCanvasTransforms();
        Debug.Log("[ModUIManager] 初始化完成");
    }

    /// <summary>
    /// 缓存 Canvas 节点引用
    /// </summary>
    private void CacheCanvasTransforms()
    {
        var canvasGO = GameObject.FindGameObjectWithTag(SysDefine.SYS_TAG_CANVAS);
        if (canvasGO != null)
        {
            _canvasTransform = canvasGO.transform;
            _normalLayer = UnityHelper.FindTheChild(canvasGO, SysDefine.SYS_CANVAS_NORMAL_NODE_NAME);
            _fixedLayer = UnityHelper.FindTheChild(canvasGO, SysDefine.SYS_CANVAS_FIXED_NODE_NAME);
            _popUpLayer = UnityHelper.FindTheChild(canvasGO, SysDefine.SYS_CANVAS_POPUP_NODE_NAME);

            // 创建模组 UI 专用容器
            CreateModUIContainer();
        }
        else
        {
            Debug.LogWarning("[ModUIManager] 未找到 Canvas，UI 功能可能不可用");
        }
    }

    /// <summary>
    /// 创建模组 UI 专用容器
    /// </summary>
    private void CreateModUIContainer()
    {
        if (_normalLayer == null) return;

        // 检查是否已存在
        var existing = _normalLayer.Find("ModUIContainer");
        if (existing != null)
        {
            _modUIContainer = existing;
            return;
        }

        // 创建容器
        var containerGO = new GameObject("ModUIContainer");
        containerGO.transform.SetParent(_normalLayer, false);

        // 设置 RectTransform 使其铺满父节点
        var rectTransform = containerGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        _modUIContainer = containerGO.transform;
        Debug.Log("[ModUIManager] 创建模组 UI 容器");
    }

    #region 注册 UI

    /// <summary>
    /// 从 AssetBundle 注册 UI
    /// </summary>
    public bool RegisterUIFromBundle(string modId, string uiName, string bundlePath, string prefabName)
    {
        string fullUIName = GetFullUIName(modId, uiName);

        if (_registeredUIs.ContainsKey(fullUIName))
        {
            Debug.LogWarning($"[ModUIManager] UI 已注册: {fullUIName}");
            return false;
        }

        var registration = new ModUIRegistration
        {
            UIName = uiName,
            ModId = modId,
            BundlePath = bundlePath,
            PrefabName = prefabName
        };

        _registeredUIs[fullUIName] = registration;
        Debug.Log($"[ModUIManager] 注册 UI: {fullUIName} (Bundle: {bundlePath}, Prefab: {prefabName})");
        return true;
    }

    /// <summary>
    /// 使用工厂函数注册 UI
    /// </summary>
    public bool RegisterUIWithFactory(string modId, string uiName, Func<Transform, GameObject> createFunc)
    {
        string fullUIName = GetFullUIName(modId, uiName);

        if (_registeredUIs.ContainsKey(fullUIName))
        {
            Debug.LogWarning($"[ModUIManager] UI 已注册: {fullUIName}");
            return false;
        }

        var registration = new ModUIRegistration
        {
            UIName = uiName,
            ModId = modId,
            CreateFunc = createFunc
        };

        _registeredUIs[fullUIName] = registration;
        Debug.Log($"[ModUIManager] 注册 UI (工厂模式): {fullUIName}");
        return true;
    }

    /// <summary>
    /// 直接使用预制体注册 UI
    /// </summary>
    public bool RegisterUIWithPrefab(string modId, string uiName, GameObject prefab)
    {
        string fullUIName = GetFullUIName(modId, uiName);

        if (_registeredUIs.ContainsKey(fullUIName))
        {
            Debug.LogWarning($"[ModUIManager] UI 已注册: {fullUIName}");
            return false;
        }

        if (prefab == null)
        {
            Debug.LogError($"[ModUIManager] 预制体为空: {fullUIName}");
            return false;
        }

        var registration = new ModUIRegistration
        {
            UIName = uiName,
            ModId = modId,
            LoadedPrefab = prefab
        };

        _registeredUIs[fullUIName] = registration;
        Debug.Log($"[ModUIManager] 注册 UI (预制体): {fullUIName}");
        return true;
    }

    #endregion

    #region 显示/关闭 UI

    /// <summary>
    /// 显示模组 UI
    /// </summary>
    public BaseUIForms ShowUI(string modId, string uiName)
    {
        string fullUIName = GetFullUIName(modId, uiName);

        if (!_registeredUIs.TryGetValue(fullUIName, out var registration))
        {
            Debug.LogError($"[ModUIManager] UI 未注册: {fullUIName}");
            return null;
        }

        // 如果已经有实例且正在显示
        if (registration.Instance != null && registration.IsShowing)
        {
            return registration.Instance;
        }

        // 如果有实例但隐藏了，重新显示
        if (registration.Instance != null)
        {
            registration.Instance.Display();
            registration.IsShowing = true;
            return registration.Instance;
        }

        // 需要创建新实例
        GameObject uiGO = CreateUIInstance(registration);
        if (uiGO == null)
        {
            Debug.LogError($"[ModUIManager] 创建 UI 实例失败: {fullUIName}");
            return null;
        }

        // 获取或添加 BaseUIForms 组件
        var uiForm = uiGO.GetComponent<BaseUIForms>();
        if (uiForm == null)
        {
            // 如果没有 BaseUIForms，添加一个默认的 ModUIForm
            uiForm = uiGO.AddComponent<ModUIForm>();
            Debug.Log($"[ModUIManager] 为 {fullUIName} 添加默认 ModUIForm 组件");
        }

        // 设置父节点
        SetUIParent(uiGO, uiForm);

        // 初始化并显示
        uiForm.Init();
        uiForm.Display();

        registration.Instance = uiForm;
        registration.IsShowing = true;

        Debug.Log($"[ModUIManager] 显示 UI: {fullUIName}");
        return uiForm;
    }

    /// <summary>
    /// 关闭模组 UI
    /// </summary>
    public void CloseUI(string modId, string uiName)
    {
        string fullUIName = GetFullUIName(modId, uiName);

        if (!_registeredUIs.TryGetValue(fullUIName, out var registration))
        {
            return;
        }

        if (registration.Instance != null && registration.IsShowing)
        {
            registration.Instance.Hiding();
            registration.IsShowing = false;
            Debug.Log($"[ModUIManager] 关闭 UI: {fullUIName}");
        }
    }

    /// <summary>
    /// 销毁模组 UI 实例
    /// </summary>
    public void DestroyUI(string modId, string uiName)
    {
        string fullUIName = GetFullUIName(modId, uiName);

        if (!_registeredUIs.TryGetValue(fullUIName, out var registration))
        {
            return;
        }

        if (registration.Instance != null)
        {
            Destroy(registration.Instance.gameObject);
            registration.Instance = null;
            registration.IsShowing = false;
            Debug.Log($"[ModUIManager] 销毁 UI: {fullUIName}");
        }
    }

    #endregion

    #region 创建 UI 实例

    /// <summary>
    /// 创建 UI 实例
    /// </summary>
    private GameObject CreateUIInstance(ModUIRegistration registration)
    {
        GameObject prefab = null;

        // 1. 如果已有加载的预制体
        if (registration.LoadedPrefab != null)
        {
            prefab = registration.LoadedPrefab;
        }
        // 2. 使用工厂函数创建
        else if (registration.CreateFunc != null)
        {
            try
            {
                return registration.CreateFunc(_modUIContainer ?? _normalLayer);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ModUIManager] 工厂函数创建 UI 失败: {e.Message}");
                return null;
            }
        }
        // 3. 从 AssetBundle 加载
        else if (!string.IsNullOrEmpty(registration.BundlePath))
        {
            prefab = LoadPrefabFromBundle(registration);
            if (prefab != null)
            {
                registration.LoadedPrefab = prefab;
            }
        }

        if (prefab == null)
        {
            Debug.LogError($"[ModUIManager] 无法获取预制体: {registration.UIName}");
            return null;
        }

        // 实例化预制体
        var instance = Instantiate(prefab);
        instance.name = registration.UIName;
        return instance;
    }

    /// <summary>
    /// 从 AssetBundle 加载预制体
    /// </summary>
    private GameObject LoadPrefabFromBundle(ModUIRegistration registration)
    {
        // 获取模组根路径
        string modRootPath = GetModRootPath(registration.ModId);
        if (string.IsNullOrEmpty(modRootPath))
        {
            Debug.LogError($"[ModUIManager] 找不到模组路径: {registration.ModId}");
            return null;
        }

        string fullBundlePath = Path.Combine(modRootPath, registration.BundlePath);

        // 检查缓存
        if (!_loadedBundles.TryGetValue(fullBundlePath, out var bundle))
        {
            // 加载 AssetBundle
            bundle = RuntimeAssetLoader.LoadAssetBundle(fullBundlePath);
            if (bundle == null)
            {
                Debug.LogError($"[ModUIManager] 加载 AssetBundle 失败: {fullBundlePath}");
                return null;
            }
            _loadedBundles[fullBundlePath] = bundle;
        }

        // 从 Bundle 加载预制体
        string prefabName = registration.PrefabName;
        if (string.IsNullOrEmpty(prefabName))
        {
            prefabName = registration.UIName;
        }

        var prefab = bundle.LoadAsset<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogError($"[ModUIManager] 从 Bundle 加载预制体失败: {prefabName}");
            return null;
        }

        return prefab;
    }

    /// <summary>
    /// 设置 UI 的父节点
    /// </summary>
    private void SetUIParent(GameObject uiGO, BaseUIForms uiForm)
    {
        Transform parent = _modUIContainer ?? _normalLayer;

        // 根据 UI 类型选择父节点
        if (uiForm.CurrentUIType != null)
        {
            switch (uiForm.CurrentUIType.UIForms_Type)
            {
                case UIFormsType.Normal:
                    parent = _modUIContainer ?? _normalLayer;
                    break;
                case UIFormsType.Fixed:
                    parent = _fixedLayer ?? _normalLayer;
                    break;
                case UIFormsType.PopUp:
                    parent = _popUpLayer ?? _normalLayer;
                    break;
            }
        }

        if (parent != null)
        {
            uiGO.transform.SetParent(parent, false);
            uiGO.transform.localPosition = Vector3.zero;
            uiGO.transform.localScale = Vector3.one;

            var rectTransform = uiGO.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
    }

    #endregion

    #region 注销 UI

    /// <summary>
    /// 注销 UI
    /// </summary>
    public void UnregisterUI(string modId, string uiName)
    {
        string fullUIName = GetFullUIName(modId, uiName);

        if (_registeredUIs.TryGetValue(fullUIName, out var registration))
        {
            // 销毁实例
            if (registration.Instance != null)
            {
                Destroy(registration.Instance.gameObject);
            }

            _registeredUIs.Remove(fullUIName);
            Debug.Log($"[ModUIManager] 注销 UI: {fullUIName}");
        }
    }

    /// <summary>
    /// 注销模组的所有 UI
    /// </summary>
    public void UnregisterAllUI(string modId)
    {
        var toRemove = new List<string>();

        foreach (var kvp in _registeredUIs)
        {
            if (kvp.Value.ModId == modId)
            {
                toRemove.Add(kvp.Key);

                // 销毁实例
                if (kvp.Value.Instance != null)
                {
                    Destroy(kvp.Value.Instance.gameObject);
                }
            }
        }

        foreach (var key in toRemove)
        {
            _registeredUIs.Remove(key);
        }

        if (toRemove.Count > 0)
        {
            Debug.Log($"[ModUIManager] 注销模组 {modId} 的所有 UI ({toRemove.Count} 个)");
        }
    }

    /// <summary>
    /// 卸载模组的 AssetBundle
    /// </summary>
    public void UnloadModBundles(string modId)
    {
        string modRootPath = GetModRootPath(modId);
        if (string.IsNullOrEmpty(modRootPath)) return;

        var toRemove = new List<string>();

        foreach (var kvp in _loadedBundles)
        {
            if (kvp.Key.StartsWith(modRootPath))
            {
                kvp.Value?.Unload(true);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            _loadedBundles.Remove(key);
        }
    }

    #endregion

    #region 查询方法

    /// <summary>
    /// 检查 UI 是否已注册
    /// </summary>
    public bool IsUIRegistered(string modId, string uiName)
    {
        return _registeredUIs.ContainsKey(GetFullUIName(modId, uiName));
    }

    /// <summary>
    /// 检查 UI 是否正在显示
    /// </summary>
    public bool IsUIShowing(string modId, string uiName)
    {
        if (_registeredUIs.TryGetValue(GetFullUIName(modId, uiName), out var registration))
        {
            return registration.IsShowing;
        }
        return false;
    }

    /// <summary>
    /// 获取 UI 实例
    /// </summary>
    public BaseUIForms GetUI(string modId, string uiName)
    {
        if (_registeredUIs.TryGetValue(GetFullUIName(modId, uiName), out var registration))
        {
            return registration.Instance;
        }
        return null;
    }

    #endregion

    #region 层级访问

    public Transform GetCanvasTransform()
    {
        if (_canvasTransform == null) CacheCanvasTransforms();
        return _canvasTransform;
    }

    public Transform GetNormalLayerTransform()
    {
        if (_normalLayer == null) CacheCanvasTransforms();
        return _normalLayer;
    }

    public Transform GetPopUpLayerTransform()
    {
        if (_popUpLayer == null) CacheCanvasTransforms();
        return _popUpLayer;
    }

    public Transform GetFixedLayerTransform()
    {
        if (_fixedLayer == null) CacheCanvasTransforms();
        return _fixedLayer;
    }

    public Transform GetModUIContainer()
    {
        if (_modUIContainer == null) CacheCanvasTransforms();
        return _modUIContainer;
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 获取完整 UI 名称
    /// </summary>
    private string GetFullUIName(string modId, string uiName)
    {
        return $"{modId}:{uiName}";
    }

    /// <summary>
    /// 获取模组根路径
    /// </summary>
    private string GetModRootPath(string modId)
    {
        if (ModManager.Instance == null) return null;

        var allMods = ModManager.Instance.GetAllMods();
        if (allMods.TryGetValue(modId, out var modInfo))
        {
            return modInfo.RootPath;
        }

        return null;
    }

    #endregion

    #region 清理

    /// <summary>
    /// 清理所有模组 UI
    /// </summary>
    public void ClearAll()
    {
        // 销毁所有 UI 实例
        foreach (var registration in _registeredUIs.Values)
        {
            if (registration.Instance != null)
            {
                Destroy(registration.Instance.gameObject);
            }
        }
        _registeredUIs.Clear();

        // 卸载所有 AssetBundle
        foreach (var bundle in _loadedBundles.Values)
        {
            bundle?.Unload(true);
        }
        _loadedBundles.Clear();

        Debug.Log("[ModUIManager] 已清理所有模组 UI");
    }

    private void OnDestroy()
    {
        ClearAll();
    }

    #endregion
}

/// <summary>
/// 默认的模组 UI 窗体基类
/// 当模组 UI 预制体没有继承 BaseUIForms 的脚本时使用
/// </summary>
public class ModUIForm : BaseUIForms
{
    public override bool Init()
    {
        // 设置默认 UI 类型
        CurrentUIType = new UIType
        {
            UIForms_Type = UIFormsType.Normal,
            UIForms_ShowMode = UIFormsShowMode.Normal,
            UIForms_LucencyType = UIFormsLucencyType.Lucency
        };

        return base.Init();
    }
}
