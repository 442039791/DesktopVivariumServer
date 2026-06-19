using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Steamworks;
#endif

public class LocalizationManager : MonoBehaviour
{
    /// <summary>
    /// 语言字典结构：<语言文件名称, <文本ID, 本地化文本>>
    /// </summary>
    public static Dictionary<string, Dictionary<int, string>> langDatabase ;

    /// <summary>
    /// 语言配置字典：<语言文件名称, 语言配置数据>
    /// 模组系统可以通过 RegisterLanguageConfig 方法添加新语言配置
    /// </summary>
    private static Dictionary<string, LanguageConfigData> languageConfigs = new();

    /// <summary>
    /// 注册语言配置（供模组系统使用）
    /// </summary>
    public static void RegisterLanguageConfig(string langCode, LanguageConfigData config)
    {
        if (string.IsNullOrEmpty(langCode) || config == null) return;
        languageConfigs[langCode] = config;
    }

    /// <summary>
    /// 当前选择的语言
    /// </summary>
    private static string currentLanguage = "null";

    /// <summary>
    /// 当前使用的字体资源
    /// </summary>
    private static TMP_FontAsset currentFont = null;

    /// <summary>
    /// PlayerPrefs 存储语言的键名
    /// </summary>
    private const string LANGUAGE_PREFS_KEY = "SelectedLanguage";

    /// <summary>
    /// 默认语言（当Steam语言不支持时使用）
    /// </summary>
    private const string DEFAULT_LANGUAGE = "English";

    /// <summary>
    /// Steam语言代码到项目语言代码的映射
    /// </summary>
    private static readonly Dictionary<string, string> steamToProjectLanguageMap = new()
    {
        { "schinese", "CN" },           // 简体中文
        { "tchinese", "CN_TW" },         // 繁体中文
        { "english", "English" },        // 英语
        // 以下语言如果项目支持可以添加映射，目前默认回退到英语
        { "japanese", "English" },
        { "koreana", "English" },
        { "french", "English" },
        { "german", "English" },
        { "spanish", "English" },
        { "russian", "English" },
        { "portuguese", "English" },
        { "brazilian", "English" },
    };

    // 存储已注册的TextMeshPro组件及其ID
    private static Dictionary<TMP_Text, int> registeredTexts = new();
    private static Dictionary<TipsTrigger, int> registeredTips = new();
    public static bool langDatabaseisInit = false;
    private bool isInitialized = false;
    // 新增：存储格式字符串的字典
    private static Dictionary<TMP_Text, string> originalFormats = new();
    private static Dictionary<TMP_Text, object[]> formatArgs = new();

    // 修改后的UpdateText方法（支持格式化）
    private static void UpdateText(TMP_Text textComponent, int textId)
    {
        if (langDatabase.TryGetValue(currentLanguage, out var langDict) &&
            langDict.TryGetValue(textId, out var localizedText))
        {
            // 更新格式字符串（每次语言切换都需要更新）
            originalFormats[textComponent] = localizedText;

            // 应用格式化参数（如果有）
            if (formatArgs.TryGetValue(textComponent, out var args))
            {
                try
                {
                    textComponent.text = string.Format(localizedText, args);
                }
                catch (FormatException)
                {
                    Debug.LogError($"格式错误: ID {textId} | 模板: {localizedText}");
                    textComponent.text = localizedText;
                }
            }
            else
            {
                textComponent.text = localizedText;
            }
        }
        else
        {
            Debug.LogWarning($"本地化缺失: ID {textId} 语言 {currentLanguage}");
        }
    }

    // 新增：设置文本格式化参数
    public static void SetTextFormatParams(TMP_Text textComponent, params object[] args)
    {
        if (!registeredTexts.ContainsKey(textComponent))
        {
            Debug.LogWarning("未注册的文本组件");
            return;
        }

        formatArgs[textComponent] = args;
        UpdateText(textComponent, registeredTexts[textComponent]);
    }

    // 修改：组件销毁时清理数据
    public void UnregisterTextComponent(TMP_Text textComponent)
    {
        UnregisterText(textComponent);
    }

    /// <summary>
    /// 静态方法：取消注册文本组件，使其不再受本地化系统管理
    /// </summary>
    public static void UnregisterText(TMP_Text textComponent)
    {
        if (registeredTexts.ContainsKey(textComponent))
            registeredTexts.Remove(textComponent);

        if (originalFormats.ContainsKey(textComponent))
            originalFormats.Remove(textComponent);

        if (formatArgs.ContainsKey(textComponent))
            formatArgs.Remove(textComponent);
    }

    /// <summary>
    /// 获取当前语言代码
    /// </summary>
    public static string GetCurrentLanguage()
    {
        return currentLanguage;
    }

    #region Steam语言集成和存储功能

    /// <summary>
    /// 检查本地是否已存储语言选择
    /// </summary>
    public static bool HasSavedLanguage()
    {
        return PlayerPrefs.HasKey(LANGUAGE_PREFS_KEY);
    }

    /// <summary>
    /// 获取本地存储的语言代码
    /// </summary>
    public static string GetSavedLanguage()
    {
        return PlayerPrefs.GetString(LANGUAGE_PREFS_KEY, string.Empty);
    }

    /// <summary>
    /// 保存语言选择到本地存储
    /// </summary>
    public static void SaveLanguage(string langCode)
    {
        PlayerPrefs.SetString(LANGUAGE_PREFS_KEY, langCode);
        PlayerPrefs.Save();
        Debug.Log($"[LocalizationManager] 语言已保存: {langCode}");
    }

    /// <summary>
    /// 从Steam获取玩家的语言设置
    /// </summary>
    /// <returns>Steam语言代码（小写），如果获取失败返回空字符串</returns>
    public static string GetSteamLanguage()
    {
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        try
        {
            // 获取 Steam UI 语言（Steam 客户端界面语言）
            string steamUILang = SteamUtils.GetSteamUILanguage();
            Debug.Log($"[LocalizationManager] Steam UI 语言 (SteamUtils.GetSteamUILanguage): {steamUILang}");

            // 获取游戏在 Steam 中设置的语言（用户在 Steam 游戏属性中选择的语言）
            string steamGameLang = SteamApps.GetCurrentGameLanguage();
            Debug.Log($"[LocalizationManager] Steam 游戏语言 (SteamApps.GetCurrentGameLanguage): {steamGameLang}");

            // 优先使用游戏语言设置，如果为空则回退到 UI 语言
            string resultLang = !string.IsNullOrEmpty(steamGameLang) ? steamGameLang : steamUILang;
            Debug.Log($"[LocalizationManager] 最终选择的 Steam 语言: {resultLang}");

            return resultLang?.ToLower() ?? string.Empty;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LocalizationManager] 获取Steam语言失败: {e.Message}\n{e.StackTrace}");
            return string.Empty;
        }
#else
        Debug.Log("[LocalizationManager] Steam未启用，无法获取Steam语言");
        return string.Empty;
#endif
    }

    /// <summary>
    /// 将Steam语言代码转换为项目语言代码
    /// </summary>
    /// <param name="steamLangCode">Steam语言代码（如 schinese, english）</param>
    /// <returns>项目语言代码（如 CN, English），如果不支持则返回默认语言</returns>
    public static string MapSteamLanguageToProject(string steamLangCode)
    {
        Debug.Log($"[LocalizationManager] MapSteamLanguageToProject 输入: '{steamLangCode}'");

        if (string.IsNullOrEmpty(steamLangCode))
        {
            Debug.Log($"[LocalizationManager] Steam语言代码为空，返回默认语言: {DEFAULT_LANGUAGE}");
            return DEFAULT_LANGUAGE;
        }

        string lowerCode = steamLangCode.ToLower();
        Debug.Log($"[LocalizationManager] 转换后的小写代码: '{lowerCode}'");

        // 先查映射表
        if (steamToProjectLanguageMap.TryGetValue(lowerCode, out string projectLang))
        {
            Debug.Log($"[LocalizationManager] 在映射表中找到: {lowerCode} -> {projectLang}");

            // 检查项目中是否实际存在该语言
            if (langDatabase != null && langDatabase.ContainsKey(projectLang))
            {
                Debug.Log($"[LocalizationManager] 语言 '{projectLang}' 在项目中存在，返回此语言");
                return projectLang;
            }
            else
            {
                Debug.LogWarning($"[LocalizationManager] 语言 '{projectLang}' 不在项目数据库中! 可用语言: {(langDatabase != null ? string.Join(", ", langDatabase.Keys) : "null")}");
            }
        }
        else
        {
            Debug.LogWarning($"[LocalizationManager] Steam语言 '{lowerCode}' 不在映射表中! 映射表包含: {string.Join(", ", steamToProjectLanguageMap.Keys)}");
        }

        // 如果映射的语言在项目中不存在，返回默认语言
        Debug.Log($"[LocalizationManager] Steam语言 '{steamLangCode}' 映射到默认语言: {DEFAULT_LANGUAGE}");
        return DEFAULT_LANGUAGE;
    }

    /// <summary>
    /// 初始化语言设置（在系统初始化完成后调用）
    /// 优先使用本地存储的语言，否则从Steam获取
    /// </summary>
    public static void InitializeLanguageFromSteamOrStorage()
    {
        Debug.Log("[LocalizationManager] ========== 开始语言初始化 ==========");
        Debug.Log($"[LocalizationManager] PlayerPrefs 是否有保存的语言: {HasSavedLanguage()}");
        Debug.Log($"[LocalizationManager] 当前语言状态: '{currentLanguage}'");
        Debug.Log($"[LocalizationManager] 语言数据库是否初始化: {langDatabaseisInit}");
        Debug.Log($"[LocalizationManager] 可用语言: {(langDatabase != null ? string.Join(", ", langDatabase.Keys) : "null")}");

        string targetLanguage;

        // 1. 检查本地是否已有存储的语言
        if (HasSavedLanguage())
        {
            string savedLang = GetSavedLanguage();
            Debug.Log($"[LocalizationManager] 从 PlayerPrefs 读取到保存的语言: '{savedLang}'");

            // 验证存储的语言是否仍然有效
            if (langDatabase != null && langDatabase.ContainsKey(savedLang))
            {
                targetLanguage = savedLang;
                Debug.Log($"[LocalizationManager] 使用本地存储的语言: {targetLanguage}");
            }
            else
            {
                // 存储的语言无效，从Steam获取
                Debug.LogWarning($"[LocalizationManager] 存储的语言 '{savedLang}' 无效，将从Steam获取");
                targetLanguage = GetLanguageFromSteam();
            }
        }
        else
        {
            // 2. 本地没有存储，从Steam获取
            Debug.Log("[LocalizationManager] PlayerPrefs 中没有保存的语言，将从 Steam 获取");
            targetLanguage = GetLanguageFromSteam();
        }

        Debug.Log($"[LocalizationManager] 最终决定使用的语言: '{targetLanguage}'");

        // 3. 设置并保存语言
        SetLanguageAndSave(targetLanguage);
        Debug.Log("[LocalizationManager] ========== 语言初始化完成 ==========");
    }

    /// <summary>
    /// 从Steam获取语言并映射到项目语言
    /// </summary>
    private static string GetLanguageFromSteam()
    {
        string steamLang = GetSteamLanguage();
        string projectLang = MapSteamLanguageToProject(steamLang);
        Debug.Log($"[LocalizationManager] 从Steam获取语言: {steamLang} -> 项目语言: {projectLang}");
        return projectLang;
    }

    /// <summary>
    /// 设置语言并保存到本地存储（供UI调用的手动切换方法）
    /// </summary>
    /// <param name="langCode">项目语言代码</param>
    /// <returns>切换是否成功</returns>
    public static bool SetLanguageAndSave(string langCode)
    {
        if (string.IsNullOrEmpty(langCode))
        {
            Debug.LogError("[LocalizationManager] 语言代码为空");
            return false;
        }

        // 验证语言是否存在
        if (langDatabase == null || !langDatabase.ContainsKey(langCode))
        {
            string available = langDatabase != null ? string.Join(", ", langDatabase.Keys) : "无";
            Debug.LogError($"[LocalizationManager] 语言不存在: {langCode}\n可用语言: {available}");
            return false;
        }

        // 切换语言（会刷新所有文本）
        SetLanguage(langCode);

        // 保存到本地存储
        SaveLanguage(langCode);

        Debug.Log($"[LocalizationManager] 语言已切换并保存: {langCode}");
        return true;
    }

    /// <summary>
    /// 检查指定的语言是否在项目中可用
    /// </summary>
    public static bool IsLanguageAvailable(string langCode)
    {
        return langDatabase != null && langDatabase.ContainsKey(langCode);
    }

    #endregion

    /// <summary>
    /// 获取当前使用的字体资源
    /// </summary>
    public static TMP_FontAsset GetCurrentFont()
    {
        return currentFont;
    }

    /// <summary>
    /// 获取指定语言的字体名称
    /// </summary>
    public static string GetFontNameForLanguage(string langCode)
    {
        if (languageConfigs.TryGetValue(langCode, out var config))
        {
            return config.Fonts;
        }
        return null;
    }

    /// <summary>
    /// 根据ID获取当前语言的文本
    /// </summary>
    public static string GetCurrentLanguageByID(int id)
    {
        if (langDatabase.TryGetValue(currentLanguage, out var langDict) )
        {
            if (langDict.TryGetValue(id, out var text))
                return text;
            return "";
        }
            
        else
            return "";
    }
    /// <summary>
    /// 获取所有可用语言列表
    /// </summary>
    public static List<string> GetLanguageList()
    {
        return new List<string>(langDatabase.Keys);
    }

    /// <summary>
    /// 初始化本地化系统（从配置文件加载所有语言数据）
    /// 新架构：从languageData.csv读取语言配置，自动关联字体
    /// </summary>
    public static void Init()
    {
        // 防止重复初始化
        if (langDatabaseisInit)
        {
            Debug.LogWarning("[LocalizationManager] 已经初始化过，跳过重复初始化");
            return;
        }

        if (langDatabase == null)
            langDatabase = new Dictionary<string, Dictionary<int, string>>();

        if (languageConfigs == null)
            languageConfigs = new Dictionary<string, LanguageConfigData>();

        // 1. 读取语言配置（languageData.json）
        List<LanguageConfigData> configs = null;
        try
        {
            configs = GameConfigDataBase.GetConfigDatas<LanguageConfigData>("languageData.json");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LocalizationManager] 读取languageData.json失败: {e.Message}");
            langDatabaseisInit = true;
            return;
        }

        if (configs == null || configs.Count == 0)
        {
            Debug.LogError("[LocalizationManager] 未找到语言配置 languageData.json");
            langDatabaseisInit = true;
            return;
        }

        // 2. 加载每种语言的文本数据和字体配置
        foreach (var config in configs)
        {
            if (config == null)
            {
                Debug.LogWarning("[LocalizationManager] 跳过null配置项");
                continue;
            }

            if (string.IsNullOrEmpty(config.Lauguage))
            {
                Debug.LogWarning($"[LocalizationManager] 跳过空语言名配置 ID:{config.ID}");
                continue;
            }

            try
            {
                // 2.1 加载语言文本数据（添加 .json 扩展名）
                var dic = new Dictionary<int, string>();
                var languageData = GameConfigDataBase.GetConfigDatas<LaunguageDataBase>("Lauguage/" + config.Lauguage + ".json");

                if (languageData != null)
                {
                    foreach (var data in languageData)
                    {
                        if (data != null)
                        {
                            dic.TryAdd(data.ID, data.Text ?? string.Empty);
                        }
                    }
                }

                // 检查是否已存在，避免重复添加
                if (!langDatabase.ContainsKey(config.Lauguage))
                {
                    langDatabase.Add(config.Lauguage, dic);
                }

                // 2.2 保存语言配置（包含字体映射）
                if (!languageConfigs.ContainsKey(config.Lauguage))
                {
                    languageConfigs.Add(config.Lauguage, config);
                }

            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LocalizationManager] 加载语言 {config.Lauguage} 失败: {e.Message}");
            }
        }

        langDatabaseisInit = true;
    }

    /// <summary>
    /// 语言变化事件回调
    /// </summary>
    public void OnLanguageChanged(string name, object udata)
    {
        string langCode = (string)udata;
        SetLanguage(langCode);
    }

    void Awake()
    {
        if (isInitialized)
            return;
        isInitialized = true;

        event_manager.instance.add_event_listener(SysDefine.ChangeLauguageEvent, OnLanguageChanged);
        StartCoroutine(InitCoroutine());
    }

    private IEnumerator InitCoroutine()
    {
        if(langDatabaseisInit)
        {
            if(currentLanguage=="null")
            {
                // 优先从本地存储或Steam获取语言
                InitializeLanguageFromSteamOrStorage();
            }
            ScanAndRegisterTexts(transform);
            ApplyCurrentLanguage();
            goto end;
        }
        yield return new WaitUntil(() => langDatabaseisInit);
        if (currentLanguage == "null")
        {
            // 优先从本地存储或Steam获取语言
            InitializeLanguageFromSteamOrStorage();
        }
        ScanAndRegisterTexts(transform);
        ApplyCurrentLanguage();
        end:
        yield break;
    }

    /// <summary>
    /// 切换语言并应用到所有已注册的文本组件（静态方法，可从任何地方调用）
    /// 新功能：自动切换对应的字体
    /// </summary>
    public static void SetLanguage(string langCode)
    {
        if (string.IsNullOrEmpty(langCode))
        {
            Debug.LogError("[LocalizationManager] 语言代码为空");
            return;
        }

        // 兼容性处理：移除 .csv 后缀（如果存在）
        if (langCode.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
        {
            langCode = langCode.Substring(0, langCode.Length - 4);
            Debug.LogWarning($"[LocalizationManager] 语言代码包含.csv后缀，已自动移除: {langCode}");
        }

        if (langDatabase == null || !langDatabase.ContainsKey(langCode))
        {
            string availableLanguages = langDatabase != null ? string.Join(", ", langDatabase.Keys) : "无";
            Debug.LogError($"[LocalizationManager] 语言不存在: {langCode}\n可用语言: {availableLanguages}");
            return;
        }

        // 如果语言相同，不重复处理（避免循环调用）
        if (currentLanguage == langCode)
        {
            return;
        }

        currentLanguage = langCode;

        // 触发语言切换事件，通知其他需要手动更新的组件
        event_manager.instance.dispatch_event(SysDefine.ChangeLauguageEvent, langCode);

        // 加载并切换字体（统一从 languageConfigs 获取，模组语言在加载时已注册）
        if (languageConfigs.TryGetValue(langCode, out var config))
        {
            if (!string.IsNullOrEmpty(config.Fonts))
            {
                var newFont = FontLoader.LoadFont(config.Fonts);
                if (newFont != null)
                {
                    currentFont = newFont;
                }
                else
                {
                    Debug.LogWarning($"[LocalizationManager] 未能加载字体: {config.Fonts}，将使用默认字体");
                }
            }
        }

        ApplyCurrentLanguage();
    }

    /// <summary>
    /// 应用当前语言到所有已注册文本
    /// 新功能：同时应用当前字体
    /// </summary>
    private static void ApplyCurrentLanguage()
    {
        // 1. 更新所有文本内容
        foreach (var pair in registeredTexts)
        {
            UpdateText(pair.Key, pair.Value);
        }

        // 2. 应用字体到所有TMP_Text组件
        if (currentFont != null)
        {
            ApplyFontToAllTexts(currentFont);
        }

        // 3. 更新所有Tips内容
        foreach (var pair in registeredTips)
        {
            UpdateTips(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// 应用字体到所有已注册的TMP_Text组件
    /// </summary>
    private static void ApplyFontToAllTexts(TMP_FontAsset font)
    {
        if (font == null)
            return;

        int count = 0;
        foreach (var textComponent in registeredTexts.Keys)
        {
            if (textComponent != null)
            {
                textComponent.font = font;
                count++;
            }
        }

    }

    /// <summary>
    /// 应用当前字体到指定GameObject及其所有子对象的TextMeshProUGUI组件
    /// 推荐在UI组件的Init方法开始时调用此方法
    /// </summary>
    /// <param name="root">要应用字体的根对象</param>
    /// <param name="includeInactive">是否包含未激活的对象（默认true）</param>
    public static void ApplyCurrentFontToUI(Transform root, bool includeInactive = true)
    {
        if (currentFont == null || root == null)
            return;

        var allTexts = root.GetComponentsInChildren<TMP_Text>(includeInactive);
        int count = 0;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.font = currentFont;
                count++;
            }
        }

        if (count > 0)
        {
        }
    }

    /// <summary>
    /// 应用当前字体到单个TextMeshProUGUI组件
    /// </summary>
    /// <param name="textComponent">要应用字体的文本组件</param>
    public static void ApplyCurrentFont(TMP_Text textComponent)
    {
        if (currentFont != null && textComponent != null)
        {
            textComponent.font = currentFont;
        }
    }

    /// <summary>
    /// 更新 Tips 组件的本地化文本
    /// </summary>
    private static void UpdateTips(TipsTrigger tipsComponent, int textId)
    {
        if (langDatabase.TryGetValue(currentLanguage, out var langDict) &&
            langDict.TryGetValue(textId, out var localizedText))
        {
            tipsComponent.tipsContent = localizedText;
        }
        else
            Debug.LogWarning($"Localization missing: ID {textId} in {currentLanguage}");
    }
    /// <summary>
    /// 核心遍历方法：扫描并自动注册所有带 "Id:xxxx" 格式的文本组件（实例方法）
    /// </summary>
    public void ScanAndRegisterTexts(Transform root)
    {
        ScanAndRegisterTextsStatic(root);
    }

    /// <summary>
    /// 静态方法：扫描并自动注册所有带 "Id:xxxx" 格式的文本组件
    /// 可以从任何UI界面的Init方法中直接调用，无需LocalizationManager实例
    /// </summary>
    public static void ScanAndRegisterTextsStatic(Transform root)
    {
        if (root == null)
            return;

        TMP_Text[] allTexts = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in allTexts)
        {
            // 检测ID格式 "Id:1001"
            string rawText = text.text;
            if (rawText == null)
            {
                continue;
            }
            if (rawText.StartsWith("Id:"))
            {
                if (int.TryParse(rawText.Substring(3), out int textId))
                {
                    SetText(text, textId);
                }
                else
                    Debug.LogWarning($"Invalid ID format: {rawText}", text.gameObject);
            }
        }

        TipsTrigger[] allTips = root.GetComponentsInChildren<TipsTrigger>(true);
        foreach (TipsTrigger tips in allTips)
        {
            // 检测ID格式 "Id:1001"
            string rawText = tips.tipsContent;
            if (string.IsNullOrEmpty(rawText))
                continue;
            if (rawText.StartsWith("Id:"))
            {
                if (int.TryParse(rawText.Substring(3), out int textId))
                {
                    if (registeredTips.ContainsKey(tips))
                        registeredTips[tips] = textId;
                    else
                        registeredTips.Add(tips, textId);
                    UpdateTips(tips, textId);
                }
                else
                    Debug.LogWarning($"Invalid ID format: {rawText}", tips.gameObject);
            }
        }
    }

    /// <summary>
    /// 设置文本并注册，支持语言切换时自动更新
    /// </summary>
    /// <param name="textComponent">文本组件</param>
    /// <param name="textId">文本ID</param>
    public static void SetText(TMP_Text textComponent, int textId)
    {
        if (textComponent == null)
        {
            Debug.LogWarning("文本组件为空，无法设置文本");
            return;
        }

        // 注册文本组件
        if (registeredTexts.ContainsKey(textComponent))
            registeredTexts[textComponent] = textId;
        else
            registeredTexts.Add(textComponent, textId);

        // 立即应用当前字体（如果有）
        if (currentFont != null)
        {
            textComponent.font = currentFont;
        }

        // 立即更新文本
        UpdateText(textComponent, textId);
    }

    /// <summary>
    /// 设置带格式化参数的文本并注册，支持语言切换时自动更新
    /// 例如：SetTextWithFormat(textComponent, 100, fishUnlock, totalFish)
    /// 文本ID 100对应 "动物 ({0}/{1})"
    /// </summary>
    /// <param name="textComponent">文本组件</param>
    /// <param name="textId">文本ID</param>
    /// <param name="formatArgs">格式化参数</param>
    public static void SetTextWithFormat(TMP_Text textComponent, int textId, params object[] args)
    {
        if (textComponent == null)
        {
            Debug.LogWarning("文本组件为空，无法设置文本");
            return;
        }

        // 注册文本组件
        if (registeredTexts.ContainsKey(textComponent))
            registeredTexts[textComponent] = textId;
        else
            registeredTexts.Add(textComponent, textId);

        // 保存格式化参数
        formatArgs[textComponent] = args;

        // 立即应用当前字体（如果有）
        if (currentFont != null)
        {
            textComponent.font = currentFont;
        }

        // 立即更新文本
        UpdateText(textComponent, textId);
    }

    /// <summary>
    /// 注册 Tips 组件（用于自动扫描）
    /// </summary>
    public void RegisterTipsComponent(TipsTrigger tipsTrigger, int textId)
    {
        if(registeredTips.ContainsKey(tipsTrigger))
            registeredTips[tipsTrigger] = textId;
        else
            registeredTips.Add(tipsTrigger, textId);
    }
}
