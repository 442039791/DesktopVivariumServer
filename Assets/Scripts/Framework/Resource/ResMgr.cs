using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using AssetBundles;
using Common;
using Newtonsoft.Json.Linq;

public class ResMgr : MonoSingleton<ResMgr> {

    public bool prefabInitComplete = false;
    private Dictionary<string, Sprite> spriteIoCache = new Dictionary<string, Sprite>();
    public override void Init() {
        base.Init();
    }
    public void InitPrefab()
    {
        AssetBundleManager.Instance.SetAssetBundleResident("gameprefab.assetbundle", true);
        LoadAssetBundleAsync("gameprefab.assetbundle", () =>
        {
            prefabInitComplete = true;
        });
        AssetBundleManager.Instance.SetAssetBundleResident("image.assetbundle", true);
        LoadAssetBundleAsync("image.assetbundle", null);
    }
    public UnityEngine.Object GetAssetCache(string name, string type_name) {
        // 1. 优先从模组加载（如果有覆盖资源）
        if (ModManager.Instance != null && ModManager.Instance.IsInitialized && ModManager.Instance.HasLoadedMods)
        {
            // 根据资源类型选择加载方式
            if (name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            {
                Sprite modSprite = ModManager.Instance.GetSprite(name);
                if (modSprite != null)
                {
                    return modSprite;
                }
            }
            else if (name.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
            {
                Mesh modMesh = ModManager.Instance.GetMesh(name);
                if (modMesh != null)
                {
                    return modMesh;
                }
            }
            else if (name.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
            {
                Material modMaterial = ModManager.Instance.GetMaterial(name);
                if (modMaterial != null)
                {
                    return modMaterial;
                }
            }
            else if (name.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                // UI 预制体覆盖检查
                // 提取预制体名称: "gameprefab/UI_Main.prefab" -> "UI_Main"
                string prefabName = Path.GetFileNameWithoutExtension(name);
                GameObject modPrefab = ModManager.Instance.GetUIPrefab(prefabName);
                if (modPrefab != null)
                {
                    return modPrefab;
                }
            }
        }

        // 2. 从游戏内置资源加载
#if UNITY_EDITOR
        if (AssetBundleConfig.IsEditorMode) {
            string path = AssetBundleUtility.PackagePathToAssetsPath(name);
            UnityEngine.Object target = null;
            if (type_name == null) {
                // 如果是png文件，尝试加载为Sprite
                if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    target = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
                if (target == null)
                {
                    target = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                }
            }
            else if (type_name.CompareTo("GameObject") == 0) {
                target = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            else if (type_name.CompareTo("AudioClip") == 0) {
                target = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            else {
                target = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }
            return target;
        }
#endif
        return AssetBundleManager.Instance.GetAssetCache(name);
    }
    public AssetBundle GetAssetBundle(string name)
    {
        return AssetBundleManager.Instance.GetAssetBundleCache(name);
    }
    public string GetConfigFile(string filename)
    {
        // 1. 先加载游戏默认配置
        string configPath = Path.Combine(Application.streamingAssetsPath, "ConfigData");
        string filePath = Path.Combine(configPath, filename);
        filePath = filePath.Replace(@"\", "/");

        string baseConfig = null;
        if (File.Exists(filePath))
        {
            baseConfig = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        }

        // 2. 检查 Mod 配置并合并
        if (ModManager.Instance != null && ModManager.Instance.IsInitialized)
        {
            string modConfig = ModManager.Instance.GetConfigText($"ConfigData/{filename}");
            if (!string.IsNullOrEmpty(modConfig))
            {
                if (string.IsNullOrEmpty(baseConfig))
                {
                    // 没有基础配置，直接使用Mod配置
                    Debug.Log($"[ResMgr] 使用Mod配置: {filename}");
                    return modConfig;
                }

                // 合并配置：Mod的data数组合并到基础配置
                string mergedConfig = MergeConfigData(baseConfig, modConfig, filename);
                if (!string.IsNullOrEmpty(mergedConfig))
                {
                    Debug.Log($"[ResMgr] 合并Mod配置: {filename}");
                    return mergedConfig;
                }
            }
        }

        if (string.IsNullOrEmpty(baseConfig))
        {
            Debug.LogError($"[ResMgr] 配置文件不存在：{filePath}");
        }
        return baseConfig;
    }

    /// <summary>
    /// 合并配置数据：将Mod配置的data数组合并到基础配置
    /// 同ID的数据会被Mod覆盖，新ID的数据会被添加
    /// </summary>
    private string MergeConfigData(string baseJson, string modJson, string filename)
    {
        try
        {
            JObject baseObj = JObject.Parse(baseJson);
            JObject modObj = JObject.Parse(modJson);

            JArray baseData = baseObj["data"] as JArray;
            JArray modData = modObj["data"] as JArray;

            if (baseData == null || modData == null)
            {
                Debug.LogWarning($"[ResMgr] 配置合并失败，缺少data数组: {filename}");
                return baseJson;
            }

            // 构建基础配置的ID索引
            Dictionary<string, int> idIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < baseData.Count; i++)
            {
                JObject item = baseData[i] as JObject;
                if (item != null && item["ID"] != null)
                {
                    idIndexMap[item["ID"].ToString()] = i;
                }
            }

            // 合并Mod数据
            int addedCount = 0;
            int overriddenCount = 0;
            foreach (JObject modItem in modData)
            {
                if (modItem["ID"] == null) continue;

                string id = modItem["ID"].ToString();
                if (idIndexMap.TryGetValue(id, out int index))
                {
                    // 覆盖现有数据
                    baseData[index] = modItem;
                    overriddenCount++;
                }
                else
                {
                    // 添加新数据
                    baseData.Add(modItem);
                    addedCount++;
                }
            }

            Debug.Log($"[ResMgr] 配置合并完成 {filename}: 新增 {addedCount}, 覆盖 {overriddenCount}");
            return baseObj.ToString();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ResMgr] 配置合并异常 {filename}: {ex.Message}");
            return baseJson;
        }
    }
    public Sprite Getprite(string name)
    {
        Sprite sprite = null;
        string[] parts = name.Split('_');
        sprite = GetAssetCache(SysDefine.ImageAssetbundle + parts[1], null) as Sprite;
        return sprite;
    }
    /// <summary>
    /// [已废弃] 获取语言列表
    /// 新架构：语言列表现在从 languageData.csv 配置文件读取，不再通过文件系统扫描
    /// </summary>
    [System.Obsolete("此方法已废弃，请使用 LocalizationManager.GetLanguageList() 替代")]
    public List<string> GetLaunguageList()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "ConfigData/Lauguage");
        var langList = GetAllFileNames(path);
        return langList;
    }
    /// <summary>
    /// 获取文件夹下所有文件名（包括后缀）
    /// </summary>
    /// <param name="folderPath">目标文件夹绝对路径</param>
    /// <returns>文件名列表（带后缀）</returns>
    public  List<string> GetAllFileNames(string folderPath)
    {
        List<string> fileNames = new List<string>();

        // 1. 检查文件夹是否存在
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"目录不存在: {folderPath}");
            return fileNames;
        }

        try
        {
            // 2. 获取所有文件路径
            string[] allFilePaths = Directory.GetFiles(
                folderPath,
                "*.*",
                SearchOption.AllDirectories  // 包含子文件夹
            );

            // 3. 提取文件名（带后缀）
            foreach (string filePath in allFilePaths)
            {
                // 跳过Unity生成的.meta文件
                if (Path.GetExtension(filePath) == ".meta") continue;

                fileNames.Add(Path.GetFileName(filePath));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"扫描文件时出错: {e.Message}");
        }

        return fileNames;
    }
    public Sprite GetBluePrintIconByIO(string name,bool reload=false)
    {
        //name += ".jpg";
        string iconPath = Path.Combine(Application.streamingAssetsPath, SysDefine.BluePrintIconPath);

        //string filePath = Path.Combine(iconPath, SysDefine.BluePrintIconPath);
        string fullPath = iconPath + name;
        return GetSpriteByIO(fullPath);
    }
    public Sprite GetSpriteByIO(string filePath, bool reload = false)
    {
        filePath = filePath.Replace(@"\", "/");
        if (spriteIoCache.ContainsKey(filePath)&&!reload)
        {
            return spriteIoCache[filePath];
        }
        else
        {
            if (File.Exists(filePath))
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                spriteIoCache[filePath] = sprite;
                return sprite;
            }
            else
            {
                return null;
            }
        }
        
    }
    public bool DeleteFileByIO(string filePath)
    {
        filePath = filePath.Replace(@"\", "/");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        else
        {
            return false;
        }
    }

    public string GetDataByIO(string filePath)
    {
        filePath = filePath.Replace(@"\", "/");
        //string filePath = Application.streamingAssetsPath + filename;
        if (File.Exists(filePath))
        {
            // 使用智能加载，自动识别压缩和非压缩格式
            return SaveCompression.LoadSmart(filePath);
        }
        else
        {
            return null;
        }
    }
    public void LoadAssetBundleAsync(string assetbundleName, Action end_func)
    {
        this.StartCoroutine(this.IE_LoadAssetBundleAsync(assetbundleName, end_func));
    }

    IEnumerator IE_LoadAssetBundleAsync(string assetbundleName, Action end_func) {
        var loader = AssetBundleManager.Instance.LoadAssetBundleAsync(assetbundleName);
        yield return loader;
        loader.Dispose();
        if (end_func != null)
            end_func();
    }
}
