using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 模组本地化管理器
///
/// 功能：
/// 1. 加载模组的本地化文本（覆盖/扩展游戏文本）
/// 2. 支持模组添加全新的语言
/// 3. 与游戏LocalizationManager集成
///
/// 模组本地化目录结构：
/// MyMod/
/// └── Localization/
///     ├── CN.json              # 覆盖/扩展简体中文
///     ├── English.json         # 覆盖/扩展英文
///     └── French.json          # 添加新语言（法语）
///
/// 语言文件格式（与游戏格式一致）：
/// {
///   "fields": ["ID", "Text"],
///   "types": ["int", "string"],
///   "data": [
///     { "ID": 90001, "Text": "模组添加的文本" },
///     { "ID": 1, "Text": "覆盖游戏原有文本" }
///   ]
/// }
///
/// 或简化格式（仅包含data）：
/// {
///   "data": [
///     { "ID": 90001, "Text": "模组添加的文本" }
///   ]
/// }
/// </summary>
public class ModLocalizationManager
{
    private static ModLocalizationManager _instance;
    public static ModLocalizationManager Instance => _instance ??= new ModLocalizationManager();

    // 模组添加的新语言配置: 语言代码 -> 语言配置
    private Dictionary<string, ModLanguageConfig> _modLanguages = new();

    // 模组本地化数据: 语言代码 -> (文本ID -> 文本内容)
    private Dictionary<string, Dictionary<int, string>> _modTexts = new();

    // 已加载的模组本地化信息
    private List<ModLocalizationInfo> _loadedLocalizations = new();

    // 本地化目录名
    private const string LOCALIZATION_DIR = "Localization";

    // 语言配置文件名（可选，用于添加新语言）
    private const string LANGUAGE_CONFIG_FILE = "languageConfig.json";

    /// <summary>
    /// 语言变更事件（当模组添加新语言时触发）
    /// </summary>
    public event Action<List<string>> OnNewLanguagesAdded;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        _modLanguages.Clear();
        _modTexts.Clear();
        _loadedLocalizations.Clear();
        Debug.Log("[ModLocalization] 初始化完成");
    }

    // 字体目录名
    private const string FONTS_DIR = "Fonts";

    /// <summary>
    /// 加载模组的本地化文件
    /// </summary>
    public void LoadModLocalization(ModInfo modInfo)
    {
        var locPath = Path.Combine(modInfo.RootPath, LOCALIZATION_DIR);
        if (!Directory.Exists(locPath))
        {
            return;
        }

        var locInfo = new ModLocalizationInfo
        {
            ModId = modInfo.ModId,
            ModName = modInfo.DisplayName,
            Languages = new List<string>(),
            TextCount = 0
        };

        // 1. 加载模组字体（从 Fonts 目录）
        LoadModFonts(modInfo);

        // 2. 检查是否有语言配置文件（用于添加新语言）
        var configPath = Path.Combine(locPath, LANGUAGE_CONFIG_FILE);
        if (File.Exists(configPath))
        {
            LoadLanguageConfig(modInfo.ModId, modInfo.RootPath, configPath);
        }

        // 3. 加载所有语言文件
        var jsonFiles = Directory.GetFiles(locPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (var jsonFile in jsonFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(jsonFile);

            // 跳过语言配置文件
            if (fileName.Equals("languageConfig", StringComparison.OrdinalIgnoreCase))
                continue;

            var langCode = fileName; // 文件名就是语言代码，如 CN, English
            int loadedCount = LoadLanguageFile(modInfo.ModId, langCode, jsonFile);

            if (loadedCount > 0)
            {
                locInfo.Languages.Add(langCode);
                locInfo.TextCount += loadedCount;
            }
        }

        if (locInfo.Languages.Count > 0)
        {
            _loadedLocalizations.Add(locInfo);
            Debug.Log($"[ModLocalization] 加载 {modInfo.DisplayName} 本地化: {locInfo.Languages.Count} 种语言, {locInfo.TextCount} 条文本");
        }
    }

    /// <summary>
    /// 加载模组字体（支持 TTF/OTF 文件或 AssetBundle）
    /// </summary>
    private void LoadModFonts(ModInfo modInfo)
    {
        var fontsPath = Path.Combine(modInfo.RootPath, FONTS_DIR);
        if (!Directory.Exists(fontsPath))
        {
            return;
        }

        // 支持的字体文件类型
        var fontExtensions = new[] { ".ttf", ".otf", ".ttc", ".bundle", "" };

        // 查找所有字体文件（TTF/OTF 或 AssetBundle）
        var fontFiles = Directory.GetFiles(fontsPath)
            .Where(f => !f.EndsWith(".meta") && !f.EndsWith(".manifest"))
            .ToArray();

        foreach (var fontFile in fontFiles)
        {
            var fontName = Path.GetFileNameWithoutExtension(fontFile);
            var extension = Path.GetExtension(fontFile).ToLowerInvariant();

            // 注册字体（FontLoader 会根据扩展名自动选择加载方式）
            FontLoader.RegisterModFont(fontName, fontFile);
        }
    }

    /// <summary>
    /// 加载语言配置文件（用于添加新语言）
    /// </summary>
    private void LoadLanguageConfig(string modId, string modRootPath, string configPath)
    {
        try
        {
            var json = File.ReadAllText(configPath);
            var configs = JsonConvert.DeserializeObject<ModLanguageConfigFile>(json);

            if (configs?.Languages == null) return;

            foreach (var lang in configs.Languages)
            {
                if (string.IsNullOrEmpty(lang.Code)) continue;

                // 检查是否是新语言（游戏中不存在）
                if (!LocalizationManager.IsLanguageAvailable(lang.Code))
                {
                    _modLanguages[lang.Code] = lang;
                    Debug.Log($"[ModLocalization] Mod {modId} 添加新语言: {lang.DisplayName} ({lang.Code})");

                    // 如果指定了字体，尝试注册字体
                    if (!string.IsNullOrEmpty(lang.FontName))
                    {
                        // 查找字体 Bundle 文件
                        var fontBundlePath = Path.Combine(modRootPath, FONTS_DIR, lang.FontName);
                        if (File.Exists(fontBundlePath))
                        {
                            FontLoader.RegisterModFont(lang.FontName, fontBundlePath);
                        }
                        // 也尝试带 .bundle 扩展名
                        else if (File.Exists(fontBundlePath + ".bundle"))
                        {
                            FontLoader.RegisterModFont(lang.FontName, fontBundlePath + ".bundle");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ModLocalization] 加载语言配置失败 {configPath}: {e.Message}");
        }
    }

    /// <summary>
    /// 加载单个语言文件
    /// </summary>
    private int LoadLanguageFile(string modId, string langCode, string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var jObject = JObject.Parse(json);

            // 获取或创建该语言的文本字典
            if (!_modTexts.TryGetValue(langCode, out var langDict))
            {
                langDict = new Dictionary<int, string>();
                _modTexts[langCode] = langDict;
            }

            // 解析 data 数组
            var dataArray = jObject["data"] as JArray;
            if (dataArray == null)
            {
                Debug.LogWarning($"[ModLocalization] 语言文件缺少 data 字段: {filePath}");
                return 0;
            }

            int count = 0;
            foreach (var item in dataArray)
            {
                var id = item["ID"]?.Value<int>() ?? 0;
                var text = item["Text"]?.Value<string>() ?? "";

                if (id > 0)
                {
                    langDict[id] = text; // 后加载的覆盖先加载的
                    count++;
                }
            }

            return count;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ModLocalization] 加载语言文件失败 {filePath}: {e.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 应用模组本地化到游戏系统
    /// 在所有模组加载完成后调用
    /// </summary>
    public void ApplyToGameLocalization()
    {
        if (LocalizationManager.langDatabase == null)
        {
            Debug.LogWarning("[ModLocalization] 游戏本地化系统未初始化");
            return;
        }

        // 1. 添加新语言到游戏系统（同时注册语言配置，使运行时逻辑统一）
        var newLanguages = new List<string>();
        foreach (var kvp in _modLanguages)
        {
            var langCode = kvp.Key;
            var modConfig = kvp.Value;

            if (!LocalizationManager.langDatabase.ContainsKey(langCode))
            {
                // 创建新语言的空字典
                LocalizationManager.langDatabase[langCode] = new Dictionary<int, string>();
                newLanguages.Add(langCode);

                // 注册语言配置到 LocalizationManager（统一字体加载逻辑）
                var langConfigData = new LanguageConfigData
                {
                    ID = 1000 + newLanguages.Count, // 模组语言使用 1000+ 的ID
                    Lauguage = langCode,
                    Fonts = modConfig.FontName
                };
                LocalizationManager.RegisterLanguageConfig(langCode, langConfigData);

                Debug.Log($"[ModLocalization] 注册新语言: {modConfig.DisplayName} ({langCode}), 字体: {modConfig.FontName}");
            }
        }

        // 2. 合并模组文本到游戏系统
        int mergedCount = 0;
        foreach (var kvp in _modTexts)
        {
            var langCode = kvp.Key;
            var modLangDict = kvp.Value;

            // 获取游戏的语言字典
            if (!LocalizationManager.langDatabase.TryGetValue(langCode, out var gameLangDict))
            {
                // 如果游戏没有这个语言，创建它
                gameLangDict = new Dictionary<int, string>();
                LocalizationManager.langDatabase[langCode] = gameLangDict;
            }

            // 合并文本（模组覆盖游戏）
            foreach (var textKvp in modLangDict)
            {
                gameLangDict[textKvp.Key] = textKvp.Value;
                mergedCount++;
            }
        }

        Debug.Log($"[ModLocalization] 应用完成: {newLanguages.Count} 种新语言, {mergedCount} 条文本合并");

        // 3. 触发新语言添加事件
        if (newLanguages.Count > 0)
        {
            OnNewLanguagesAdded?.Invoke(newLanguages);
        }

        // 4. 刷新当前语言显示（如果当前语言有模组文本）
        var currentLang = LocalizationManager.GetCurrentLanguage();
        if (_modTexts.ContainsKey(currentLang))
        {
            // 触发语言刷新事件
            event_manager.instance.dispatch_event(SysDefine.ChangeLauguageEvent, currentLang);
        }
    }

    /// <summary>
    /// 获取模组添加的新语言列表
    /// </summary>
    public List<ModLanguageConfig> GetModLanguages()
    {
        return _modLanguages.Values.ToList();
    }

    /// <summary>
    /// 获取所有可用语言（游戏 + 模组）
    /// </summary>
    public List<string> GetAllAvailableLanguages()
    {
        var languages = new HashSet<string>();

        // 添加游戏语言
        if (LocalizationManager.langDatabase != null)
        {
            foreach (var lang in LocalizationManager.langDatabase.Keys)
            {
                languages.Add(lang);
            }
        }

        // 添加模组新语言
        foreach (var lang in _modLanguages.Keys)
        {
            languages.Add(lang);
        }

        return languages.ToList();
    }

    /// <summary>
    /// 检查指定语言是否由模组添加
    /// </summary>
    public bool IsModLanguage(string langCode)
    {
        return _modLanguages.ContainsKey(langCode);
    }

    /// <summary>
    /// 获取语言显示名称
    /// </summary>
    public string GetLanguageDisplayName(string langCode)
    {
        if (_modLanguages.TryGetValue(langCode, out var config))
        {
            return config.DisplayName;
        }

        // 游戏内置语言的显示名称
        return langCode switch
        {
            "CN" => "简体中文",
            "CN_TW" => "繁體中文",
            "English" => "English",
            "Japanese" => "日本語",
            "Korean" => "한국어",
            _ => langCode
        };
    }

    /// <summary>
    /// 获取已加载的本地化信息
    /// </summary>
    public IReadOnlyList<ModLocalizationInfo> GetLoadedLocalizations()
    {
        return _loadedLocalizations.AsReadOnly();
    }

    /// <summary>
    /// 检查是否有模组本地化
    /// </summary>
    public bool HasModLocalizations => _loadedLocalizations.Count > 0;

    /// <summary>
    /// 获取模组文本总数
    /// </summary>
    public int TotalModTextCount => _loadedLocalizations.Sum(l => l.TextCount);

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        _modLanguages.Clear();
        _modTexts.Clear();
        _loadedLocalizations.Clear();

        // 清理模组字体注册
        FontLoader.ClearModFonts();
    }
}

/// <summary>
/// 模组语言配置
/// </summary>
[Serializable]
public class ModLanguageConfig
{
    /// <summary>
    /// 语言代码（如 French, German, Spanish）
    /// </summary>
    public string Code;

    /// <summary>
    /// 语言显示名称（如 Français, Deutsch, Español）
    /// </summary>
    public string DisplayName;

    /// <summary>
    /// 字体名称（可选，如果需要特殊字体）
    /// </summary>
    public string FontName;

    /// <summary>
    /// 是否从右到左书写（如阿拉伯语、希伯来语）
    /// </summary>
    public bool RightToLeft;
}

/// <summary>
/// 语言配置文件结构
/// </summary>
[Serializable]
public class ModLanguageConfigFile
{
    public List<ModLanguageConfig> Languages;
}

/// <summary>
/// 模组本地化信息
/// </summary>
public class ModLocalizationInfo
{
    public string ModId;
    public string ModName;
    public List<string> Languages;
    public int TextCount;
}
