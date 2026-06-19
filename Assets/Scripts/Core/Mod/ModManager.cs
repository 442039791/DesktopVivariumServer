using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Common;

/// <summary>
/// Mod 管理器 - 基于路径覆盖模式
///
/// Mod 资源按项目目录结构放置，自动覆盖同名资源
/// 加载逻辑：先加载游戏资源，再从 Mod 加载，冲突时用 Mod 覆盖
///
/// Mod 目录结构示例：
/// Mods/
/// └── MyMod/
///     ├── manifest.json              # 只包含基本信息(Id, Name, Version, Author, Description, MinGameVersion, Dependencies)
///     ├── Image/Fish/Fish100101.png  # 覆盖游戏的 Image/Fish/Fish100101.png
///     ├── Mesh/Fish/Fish100101.obj   # 覆盖游戏的 Mesh/Fish/Fish100101.obj
///     ├── ConfigData/FishData.json   # 覆盖游戏配置文件
///     └── Plugins/MyPlugin.dll       # DLL 扩展
/// </summary>
public class ModManager : MonoSingleton<ModManager>
{
    public string ModsPath { get; private set; }
    private Dictionary<string, ModInfo> _allMods;
    private List<ModInfo> _loadedMods;
    private ModSettingsSaveData _userSettings;
    private bool _isInitialized;

    // 资源缓存
    private Dictionary<string, UnityEngine.Object> _assetCache;

    // 资源路径映射: 相对路径 -> Mod绝对路径
    private Dictionary<string, string> _resourceOverrideMap;

    public event Action<ModInfo> OnModLoaded;
    public event Action OnAllModsLoaded;

    private const string MANIFEST_FILE = "manifest.json";
    private const string SETTINGS_FILE = "ModSettings.json";

    // 支持的资源目录（镜像项目结构）
    private static readonly string[] RESOURCE_DIRECTORIES = {
        "Image",       // 图片资源 (对应 AssetsPackage/Image)
        "Mesh",        // 模型资源 (对应 AssetsPackage/Mesh)
        "Material",    // 材质资源 (对应 AssetsPackage/Material)
        "Animation",   // 动画资源 (对应 AssetsPackage/Animation)
        "Sound",       // 音效资源 (对应 AssetsPackage/Sound)
        "ConfigData",  // 配置文件 (对应 StreamingAssets/ConfigData)
        "UI",          // UI 预制体 AssetBundle
        "Plugins"      // DLL 扩展
    };

    // UI AssetBundle 缓存: UI名称 -> AssetBundle
    private Dictionary<string, AssetBundle> _uiBundleCache;

    public override void Init()
    {
        _allMods = new Dictionary<string, ModInfo>();
        _loadedMods = new List<ModInfo>();
        _assetCache = new Dictionary<string, UnityEngine.Object>();
        _resourceOverrideMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _uiBundleCache = new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);
        _isInitialized = false;
    }

    /// <summary>
    /// 初始化 Mod 系统
    /// </summary>
    public void Initialize(string modsPath)
    {
        if (_isInitialized) return;

        ModsPath = modsPath;
        if (!Directory.Exists(ModsPath))
        {
            Directory.CreateDirectory(ModsPath);
        }

        LoadUserSettings();
        RegisterWorkshopEvents();
        _isInitialized = true;
        Debug.Log($"[ModManager] 初始化完成. 路径: {ModsPath}");
    }

    /// <summary>
    /// 注册创意工坊事件监听
    /// </summary>
    private void RegisterWorkshopEvents()
    {
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        if (SteamWorkshopManager.Instance != null)
        {
            SteamWorkshopManager.Instance.OnItemInstalled += OnWorkshopItemInstalled;
            SteamWorkshopManager.Instance.OnItemUpdated += OnWorkshopItemUpdated;
            Debug.Log("[ModManager] 已注册创意工坊事件监听");
        }
#endif
    }

#if !DISABLESTEAMWORKS && STEAMWORKSNET
    /// <summary>
    /// 创意工坊物品安装完成回调
    /// </summary>
    private void OnWorkshopItemInstalled(WorkshopItemInfo itemInfo)
    {
        Debug.Log($"[ModManager] 创意工坊物品安装完成: {itemInfo.ItemId}");
        // 刷新 Mod 列表以发现新安装的 Mod
        DiscoverMods();
    }

    /// <summary>
    /// 创意工坊物品更新完成回调
    /// </summary>
    private void OnWorkshopItemUpdated(WorkshopItemInfo itemInfo)
    {
        Debug.Log($"[ModManager] 创意工坊物品更新完成: {itemInfo.ItemId}");
        // 检查是否有对应的已加载 Mod 需要重新加载
        foreach (var mod in _loadedMods)
        {
            if (mod.Type == ModType.Workshop && mod.WorkshopItemId == itemInfo.ItemId.m_PublishedFileId)
            {
                Debug.LogWarning($"[ModManager] Mod '{mod.DisplayName}' 已更新，需要重启游戏以应用更改");
                // TODO: 可以在此触发 UI 提示用户重启
                break;
            }
        }
    }
#endif

    /// <summary>
    /// 发现所有 Mod
    /// </summary>
    public void DiscoverMods()
    {
        if (!_isInitialized) return;

        _allMods.Clear();

        // 1. 扫描本地 Mod 目录
        ScanLocalMods();

        // 2. 扫描 Steam 创意工坊订阅
        ScanWorkshopMods();

        ApplyUserSettings();
    }

    /// <summary>
    /// 扫描本地 Mod 目录
    /// </summary>
    private void ScanLocalMods()
    {
        if (!Directory.Exists(ModsPath)) return;

        foreach (var dir in Directory.GetDirectories(ModsPath))
        {
            var manifestPath = Path.Combine(dir, MANIFEST_FILE);
            if (File.Exists(manifestPath))
            {
                var modInfo = LoadModManifest(dir, ModType.Local);
                if (modInfo != null && !_allMods.ContainsKey(modInfo.ModId))
                {
                    _allMods[modInfo.ModId] = modInfo;
                    Debug.Log($"[ModManager] 发现本地 Mod: {modInfo.DisplayName} ({modInfo.ModId})");
                }
            }
        }
    }

    /// <summary>
    /// 扫描 Steam 创意工坊订阅的 Mod
    /// </summary>
    private void ScanWorkshopMods()
    {
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        // 检查 SteamWorkshopManager 是否可用
        if (SteamWorkshopManager.Instance == null || !SteamWorkshopManager.Instance.IsSteamAvailable)
        {
            Debug.Log("[ModManager] Steam 创意工坊不可用，跳过扫描");
            return;
        }

        // 获取所有已安装的创意工坊物品
        var installedItems = SteamWorkshopManager.Instance.GetInstalledItems();
        Debug.Log($"[ModManager] 发现 {installedItems.Count} 个创意工坊订阅");

        foreach (var workshopItem in installedItems)
        {
            // 检查是否有 manifest.json
            string manifestPath = Path.Combine(workshopItem.InstallPath, MANIFEST_FILE);
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            var modInfo = LoadModManifest(workshopItem.InstallPath, ModType.Workshop);
            if (modInfo != null)
            {
                // 设置 Workshop Item ID
                modInfo.WorkshopItemId = workshopItem.ItemId.m_PublishedFileId;

                // 如果已存在同 ID 的本地 Mod，本地优先
                if (_allMods.ContainsKey(modInfo.ModId))
                {
                    Debug.Log($"[ModManager] 创意工坊 Mod 已被本地版本覆盖: {modInfo.ModId}");
                    continue;
                }

                _allMods[modInfo.ModId] = modInfo;
                Debug.Log($"[ModManager] 发现创意工坊 Mod: {modInfo.DisplayName} ({modInfo.ModId}) [Workshop ID: {modInfo.WorkshopItemId}]");
            }
        }
#endif
    }

    private ModInfo LoadModManifest(string modPath, ModType modType = ModType.Local)
    {
        try
        {
            var json = File.ReadAllText(Path.Combine(modPath, MANIFEST_FILE));
            var manifest = JsonConvert.DeserializeObject<ModManifest>(json);

            if (manifest == null)
            {
                Debug.LogWarning($"[ModManager] Manifest 解析失败: {modPath}");
                return null;
            }

            if (!manifest.Validate(out string errorMsg))
            {
                Debug.LogWarning($"[ModManager] Manifest 验证失败: {errorMsg}");
                return null;
            }

            return new ModInfo
            {
                ModId = manifest.Id,
                Manifest = manifest,
                Type = modType,
                RootPath = modPath,
                LoadState = ModLoadState.Unloaded,
                IsEnabled = true
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ModManager] 加载 manifest 失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 加载所有启用的 Mod
    /// </summary>
    public void LoadAllEnabledMods()
    {
        if (!_isInitialized) return;

        UpdateRuntimeModAccessFlag();

        if (_userSettings != null && !_userSettings.RuntimeModAccessAllowed)
        {
            Debug.Log("[ModManager] 当前用户未拥有完整版，已禁止激活所有模组");
            OnAllModsLoaded?.Invoke();
            return;
        }

        if (_userSettings != null && !_userSettings.ModSystemEnabled)
        {
            OnAllModsLoaded?.Invoke();
            return;
        }

        // 清空覆盖映射
        _resourceOverrideMap.Clear();
        _assetCache.Clear();

        // 初始化本地化管理器
        ModLocalizationManager.Instance.Initialize();

        // 按加载顺序处理所有启用的 Mod
        var enabledMods = _allMods.Values
            .Where(m => m.IsEnabled && m.CanLoad())
            .OrderBy(m => m.LoadOrder)
            .ToList();

        foreach (var mod in enabledMods)
        {
            LoadMod(mod);
        }

        // 应用模组本地化到游戏系统
        ModLocalizationManager.Instance.ApplyToGameLocalization();

        Debug.Log($"[ModManager] 资源覆盖映射总数: {_resourceOverrideMap.Count}");
        OnAllModsLoaded?.Invoke();
    }

    private void LoadMod(ModInfo modInfo)
    {
        modInfo.LoadState = ModLoadState.Loading;

        try
        {
            // 扫描 Mod 目录，建立资源覆盖映射
            int overrideCount = ScanModResources(modInfo);

            // 加载本地化文件
            ModLocalizationManager.Instance.LoadModLocalization(modInfo);

            // 加载DLL脚本（如果有）
            LoadModScripts(modInfo);

            modInfo.LoadState = ModLoadState.Loaded;
            modInfo.LoadedTime = DateTime.Now;
            _loadedMods.Add(modInfo);

            Debug.Log($"[ModManager] 加载 Mod: {modInfo.DisplayName} (覆盖资源: {overrideCount})");
            OnModLoaded?.Invoke(modInfo);
        }
        catch (Exception ex)
        {
            modInfo.LoadState = ModLoadState.Failed;
            modInfo.ErrorMessage = ex.Message;
            Debug.LogError($"[ModManager] 加载失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 加载模组的DLL脚本
    /// </summary>
    private void LoadModScripts(ModInfo modInfo)
    {
        var pluginsPath = Path.Combine(modInfo.RootPath, "Plugins");
        if (!Directory.Exists(pluginsPath))
        {
            return;
        }

        // 确保ModScriptLoader已初始化
        if (ModScriptLoader.Instance != null)
        {
            ModScriptLoader.Instance.LoadModScripts(modInfo);
        }
        else
        {
            Debug.LogWarning("[ModManager] ModScriptLoader未初始化，跳过DLL加载");
        }
    }

    /// <summary>
    /// 扫描 Mod 资源目录，建立覆盖映射
    /// </summary>
    private int ScanModResources(ModInfo modInfo)
    {
        int count = 0;

        foreach (var resourceDir in RESOURCE_DIRECTORIES)
        {
            var modResourcePath = Path.Combine(modInfo.RootPath, resourceDir);
            if (!Directory.Exists(modResourcePath)) continue;

            // 递归扫描所有文件
            var files = Directory.GetFiles(modResourcePath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                // 跳过 meta 文件
                if (file.EndsWith(".meta")) continue;

                // 获取相对路径 (如 "Image/Fish/Fish100101.png")
                var relativePath = file.Substring(modInfo.RootPath.Length + 1).Replace('\\', '/');

                // 添加到覆盖映射（后加载的 Mod 覆盖先加载的）
                _resourceOverrideMap[relativePath] = file;
                count++;
            }
        }

        return count;
    }

    #region 资源覆盖接口

    /// <summary>
    /// 检查指定相对路径是否有 Mod 覆盖
    /// </summary>
    public bool HasOverride(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return false;
        relativePath = NormalizePath(relativePath);
        return _resourceOverrideMap.ContainsKey(relativePath);
    }

    /// <summary>
    /// 获取覆盖资源的完整路径
    /// </summary>
    public string GetOverridePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        relativePath = NormalizePath(relativePath);
        return _resourceOverrideMap.TryGetValue(relativePath, out var path) ? path : null;
    }

    /// <summary>
    /// 加载贴图资源（从 Mod 覆盖加载）
    /// </summary>
    public Texture2D GetTexture(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        relativePath = NormalizePath(relativePath);

        var cacheKey = $"tex:{relativePath}";
        if (_assetCache.TryGetValue(cacheKey, out var cached))
            return cached as Texture2D;

        if (_resourceOverrideMap.TryGetValue(relativePath, out var modPath))
        {
            var texture = RuntimeAssetLoader.LoadTexture(modPath);
            if (texture != null)
            {
                _assetCache[cacheKey] = texture;
                Debug.Log($"[ModManager] 贴图覆盖: {relativePath}");
                return texture;
            }
        }

        return null;
    }

    /// <summary>
    /// 加载 Sprite 资源（从 Mod 覆盖加载）
    /// </summary>
    public Sprite GetSprite(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        relativePath = NormalizePath(relativePath);

        var cacheKey = $"spr:{relativePath}";
        if (_assetCache.TryGetValue(cacheKey, out var cached))
            return cached as Sprite;

        if (_resourceOverrideMap.TryGetValue(relativePath, out var modPath))
        {
            var sprite = RuntimeAssetLoader.LoadSprite(modPath);
            if (sprite != null)
            {
                _assetCache[cacheKey] = sprite;
                return sprite;
            }
        }

        return null;
    }

    /// <summary>
    /// 加载 Mesh 资源（从 Mod 覆盖加载）
    /// </summary>
    public Mesh GetMesh(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        relativePath = NormalizePath(relativePath);

        var cacheKey = $"mesh:{relativePath}";
        if (_assetCache.TryGetValue(cacheKey, out var cached))
            return cached as Mesh;

        if (_resourceOverrideMap.TryGetValue(relativePath, out var modPath))
        {
            var mesh = RuntimeAssetLoader.LoadOBJ(modPath);
            if (mesh != null)
            {
                _assetCache[cacheKey] = mesh;
                Debug.Log($"[ModManager] Mesh覆盖: {relativePath}");
                return mesh;
            }
        }

        return null;
    }

    /// <summary>
    /// 加载 Material（从 Mod 的贴图自动创建）
    /// </summary>
    public Material GetMaterial(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        relativePath = NormalizePath(relativePath);

        var cacheKey = $"mat:{relativePath}";
        if (_assetCache.TryGetValue(cacheKey, out var cached))
            return cached as Material;

        // 尝试找对应的贴图文件
        var texturePath = relativePath;
        if (!texturePath.StartsWith("Image/"))
        {
            texturePath = texturePath.Replace("Material/", "Image/");
            var ext = Path.GetExtension(texturePath);
            if (ext == ".mat")
            {
                texturePath = texturePath.Replace(".mat", ".png");
            }
        }

        if (_resourceOverrideMap.TryGetValue(texturePath, out var modPath))
        {
            var texture = RuntimeAssetLoader.LoadTexture(modPath);
            if (texture != null)
            {
                var material = RuntimeAssetLoader.CreateMaterial(Path.GetFileNameWithoutExtension(relativePath), texture);
                if (material != null)
                {
                    _assetCache[cacheKey] = material;
                    Debug.Log($"[ModManager] Material覆盖: {relativePath}");
                    return material;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 加载配置文件内容（从 Mod 覆盖加载）
    /// </summary>
    public string GetConfigText(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        relativePath = NormalizePath(relativePath);

        if (_resourceOverrideMap.TryGetValue(relativePath, out var modPath))
        {
            try
            {
                var content = File.ReadAllText(modPath, System.Text.Encoding.UTF8);
                Debug.Log($"[ModManager] 配置覆盖: {relativePath}");
                return content;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModManager] 读取配置失败: {ex.Message}");
            }
        }

        return null;
    }

    /// <summary>
    /// 获取所有覆盖的资源路径
    /// </summary>
    public IEnumerable<string> GetAllOverridePaths()
    {
        return _resourceOverrideMap.Keys;
    }

    /// <summary>
    /// 获取指定目录下的所有覆盖资源
    /// </summary>
    public IEnumerable<string> GetOverridesInDirectory(string directory)
    {
        directory = NormalizePath(directory);
        if (!directory.EndsWith("/")) directory += "/";

        return _resourceOverrideMap.Keys
            .Where(k => k.StartsWith(directory, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 加载 UI 预制体（从 Mod 的 AssetBundle 加载）
    /// </summary>
    /// <param name="uiName">UI 名称（如 "UI_Main"）</param>
    /// <returns>预制体 GameObject，如果没有模组覆盖则返回 null</returns>
    public GameObject GetUIPrefab(string uiName)
    {
        if (string.IsNullOrEmpty(uiName)) return null;

        // 检查缓存
        var cacheKey = $"uiprefab:{uiName}";
        if (_assetCache.TryGetValue(cacheKey, out var cached))
            return cached as GameObject;

        // 查找对应的 UI bundle 文件
        // 文件名格式: UI/{uiName}.prefab.bundle
        string bundleRelativePath = $"UI/{uiName}.prefab.bundle";

        if (_resourceOverrideMap.TryGetValue(bundleRelativePath, out var bundlePath))
        {
            try
            {
                // 检查是否已加载此 bundle
                if (!_uiBundleCache.TryGetValue(uiName, out var bundle) || bundle == null)
                {
                    bundle = AssetBundle.LoadFromFile(bundlePath);
                    if (bundle != null)
                    {
                        _uiBundleCache[uiName] = bundle;
                    }
                }

                if (bundle != null)
                {
                    // 加载预制体
                    var prefab = bundle.LoadAsset<GameObject>(uiName);
                    if (prefab != null)
                    {
                        _assetCache[cacheKey] = prefab;
                        Debug.Log($"[ModManager] UI预制体覆盖: {uiName}");
                        return prefab;
                    }
                    else
                    {
                        // 尝试加载 bundle 中的第一个资源
                        var allAssets = bundle.GetAllAssetNames();
                        if (allAssets.Length > 0)
                        {
                            prefab = bundle.LoadAsset<GameObject>(allAssets[0]);
                            if (prefab != null)
                            {
                                _assetCache[cacheKey] = prefab;
                                Debug.Log($"[ModManager] UI预制体覆盖: {uiName} (从 {allAssets[0]})");
                                return prefab;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModManager] 加载UI预制体失败 {uiName}: {ex.Message}");
            }
        }

        return null;
    }

    /// <summary>
    /// 检查是否有 UI 预制体覆盖
    /// </summary>
    public bool HasUIPrefabOverride(string uiName)
    {
        if (string.IsNullOrEmpty(uiName)) return false;
        string bundleRelativePath = $"UI/{uiName}.prefab.bundle";
        return _resourceOverrideMap.ContainsKey(bundleRelativePath);
    }

    /// <summary>
    /// 获取所有被覆盖的 UI 名称列表
    /// </summary>
    public List<string> GetAllUIOverrides()
    {
        var result = new List<string>();
        foreach (var path in _resourceOverrideMap.Keys)
        {
            if (path.StartsWith("UI/", StringComparison.OrdinalIgnoreCase) &&
                path.EndsWith(".prefab.bundle", StringComparison.OrdinalIgnoreCase))
            {
                // 提取 UI 名称: "UI/UI_Main.prefab.bundle" -> "UI_Main"
                var fileName = Path.GetFileName(path);
                var uiName = fileName.Replace(".prefab.bundle", "");
                result.Add(uiName);
            }
        }
        return result;
    }

    // 兼容旧接口（已废弃，返回默认值）
    public bool IsModAsset(string typeId) => false;
    public Vector3 GetOffset(string typeId) => Vector3.zero;
    public float GetScale(string typeId) => 1.0f;
    public GameObject GetGameObject(string typeId) => null;

    #endregion

    #region 工具方法

    private string NormalizePath(string path)
    {
        return path?.Replace('\\', '/').TrimStart('/');
    }

    #endregion

    #region 清理

    public void UnloadAllMods()
    {
        // 先卸载所有DLL脚本
        if (ModScriptLoader.Instance != null)
        {
            ModScriptLoader.Instance.UnloadAllModScripts();
        }

        // 清理本地化数据
        ModLocalizationManager.Instance.Clear();

        // 卸载 UI AssetBundles
        foreach (var bundle in _uiBundleCache.Values)
        {
            if (bundle != null)
            {
                bundle.Unload(true);
            }
        }
        _uiBundleCache.Clear();

        _resourceOverrideMap.Clear();
        RuntimeAssetLoader.ClearCache();

        foreach (var asset in _assetCache.Values)
        {
            if (asset != null) Destroy(asset);
        }
        _assetCache.Clear();

        foreach (var mod in _loadedMods)
        {
            mod.LoadState = ModLoadState.Unloaded;
            mod.LoadedBundles.Clear();
        }
        _loadedMods.Clear();

        Debug.Log("[ModManager] 所有 Mod 已卸载");
    }

    public void ClearAssetCache()
    {
        foreach (var asset in _assetCache.Values)
        {
            if (asset != null) Destroy(asset);
        }
        _assetCache.Clear();
        RuntimeAssetLoader.ClearCache();
    }

    #endregion

    #region 用户设置

    private void LoadUserSettings()
    {
        var path = GameConfig.ModSettingsPath;
        try
        {
            if (File.Exists(path))
            {
                _userSettings = JsonConvert.DeserializeObject<ModSettingsSaveData>(File.ReadAllText(path));
                Debug.Log($"[ModManager] 加载用户设置: {path}");
            }
        }
        catch { }

        _userSettings ??= new ModSettingsSaveData();
        UpdateRuntimeModAccessFlag();
    }

    public void SaveUserSettings()
    {
        if (_userSettings == null) return;

        UpdateRuntimeModAccessFlag();

        _userSettings.ModSettings = _allMods.Values.Select(m => new ModUserSetting
        {
            ModId = m.ModId,
            IsEnabled = m.IsEnabled,
            LoadOrder = m.LoadOrder
        }).ToList();

        var path = GameConfig.ModSettingsPath;
        try
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(_userSettings, Formatting.Indented));
            Debug.Log($"[ModManager] 保存用户设置: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ModManager] 保存设置失败: {ex.Message}");
        }
    }

    private void ApplyUserSettings()
    {
        if (_userSettings?.ModSettings == null) return;

        foreach (var setting in _userSettings.ModSettings)
        {
            if (_allMods.TryGetValue(setting.ModId, out var mod))
            {
                mod.IsEnabled = setting.IsEnabled;
                mod.LoadOrder = setting.LoadOrder;
            }
        }
    }

    private void UpdateRuntimeModAccessFlag()
    {
        if (_userSettings == null)
        {
            return;
        }

#if !DISABLESTEAMWORKS && STEAMWORKSNET
        if (SteamOwnershipManager.Instance != null)
        {
            if (!SteamOwnershipManager.Instance.IsOwnershipChecked)
            {
                SteamOwnershipManager.Instance.CheckOwnership();
            }
            _userSettings.RuntimeModAccessAllowed = SteamOwnershipManager.Instance.IsFullVersionOwned;
            return;
        }
#endif
        _userSettings.RuntimeModAccessAllowed = true;
    }

    #endregion

    #region 公开属性

    public IReadOnlyDictionary<string, ModInfo> GetAllMods() => _allMods;
    public IReadOnlyList<ModInfo> GetLoadedMods() => _loadedMods.AsReadOnly();
    public bool IsModLoaded(string modId) => _allMods.TryGetValue(modId, out var m) && m.LoadState == ModLoadState.Loaded;
    public bool IsInitialized => _isInitialized;
    public bool HasLoadedMods => _loadedMods.Count > 0;
    public int OverrideCount => _resourceOverrideMap.Count;

    #endregion
}
