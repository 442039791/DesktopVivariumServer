using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// 字体加载器 - 负责加载和管理TextMeshPro SDF字体资源
/// 支持从AssetsPackage、AssetBundle、TTF/OTF文件或模组加载字体
/// </summary>
public class FontLoader
{
    // 字体缓存：<字体资源名, TMP_FontAsset>
    private static Dictionary<string, TMP_FontAsset> fontCache = new Dictionary<string, TMP_FontAsset>();

    // 模组字体注册表：<字体名称, 文件路径> (支持 AssetBundle 或 TTF/OTF)
    private static Dictionary<string, string> modFontRegistry = new Dictionary<string, string>();

    // 动态创建的 Unity Font 缓存（用于 TTF/OTF）
    private static Dictionary<string, Font> dynamicFontCache = new Dictionary<string, Font>();

    // 支持的字体文件扩展名
    private static readonly string[] FONT_EXTENSIONS = { ".ttf", ".otf", ".ttc" };

    /// <summary>
    /// 注册模组字体（在模组加载时调用）
    /// 支持 AssetBundle 文件或 TTF/OTF 字体文件
    /// </summary>
    /// <param name="fontName">字体名称</param>
    /// <param name="fontPath">字体文件路径 (AssetBundle 或 TTF/OTF)</param>
    public static void RegisterModFont(string fontName, string fontPath)
    {
        if (string.IsNullOrEmpty(fontName) || string.IsNullOrEmpty(fontPath))
            return;

        modFontRegistry[fontName] = fontPath;
        var extension = Path.GetExtension(fontPath).ToLowerInvariant();
        var fontType = IsTrueTypeFont(extension) ? "TTF/OTF" : "AssetBundle";
        Debug.Log($"[FontLoader] 注册模组字体: {fontName} -> {Path.GetFileName(fontPath)} ({fontType})");
    }

    /// <summary>
    /// 检查是否是 TrueType/OpenType 字体文件
    /// </summary>
    private static bool IsTrueTypeFont(string extension)
    {
        foreach (var ext in FONT_EXTENSIONS)
        {
            if (extension.Equals(ext, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 取消注册模组字体
    /// </summary>
    public static void UnregisterModFont(string fontName)
    {
        if (modFontRegistry.ContainsKey(fontName))
        {
            modFontRegistry.Remove(fontName);
            // 同时清除缓存
            if (fontCache.ContainsKey(fontName))
            {
                fontCache.Remove(fontName);
            }
        }
    }

    /// <summary>
    /// 清除所有模组字体注册
    /// </summary>
    public static void ClearModFonts()
    {
        foreach (var fontName in modFontRegistry.Keys)
        {
            if (fontCache.ContainsKey(fontName))
            {
                fontCache.Remove(fontName);
            }
        }
        modFontRegistry.Clear();

        // 清理动态创建的 Unity Font
        foreach (var font in dynamicFontCache.Values)
        {
            if (font != null)
            {
                Object.Destroy(font);
            }
        }
        dynamicFontCache.Clear();

        Debug.Log("[FontLoader] 模组字体注册已清除");
    }

    /// <summary>
    /// 加载SDF字体资源
    /// </summary>
    /// <param name="fontName">字体资源名（不含扩展名），如 "AlimamaShuHeiTi-Bold SDF"</param>
    /// <returns>TMP_FontAsset字体资源，失败返回null</returns>
    public static TMP_FontAsset LoadFont(string fontName)
    {
        if (string.IsNullOrEmpty(fontName))
        {
            Debug.LogError("[FontLoader] 字体名称为空");
            return null;
        }

        // 检查缓存
        if (fontCache.TryGetValue(fontName, out var cachedFont))
        {
            if (cachedFont != null)
                return cachedFont;
            else
                fontCache.Remove(fontName); // 清理无效缓存
        }

        TMP_FontAsset font = null;

        // 方式1：尝试从模组加载（支持 AssetBundle 或 TTF/OTF）
        if (modFontRegistry.TryGetValue(fontName, out var fontPath))
        {
            var extension = Path.GetExtension(fontPath).ToLowerInvariant();

            if (IsTrueTypeFont(extension))
            {
                // 从 TTF/OTF 文件动态创建字体
                font = LoadFontFromTTF(fontPath, fontName);
            }
            else
            {
                // 从 AssetBundle 加载
                font = LoadFontFromBundle(fontPath, fontName);
            }

            if (font != null)
            {
                fontCache[fontName] = font;
                Debug.Log($"[FontLoader] 从模组加载字体: {fontName}");
                return font;
            }
        }

        // 方式2：尝试从Resources/Fonts加载（推荐方式）
        font = Resources.Load<TMP_FontAsset>($"Fonts/{fontName}");

        if (font == null)
        {
            // 方式3：尝试从AssetsPackage直接路径加载（Editor模式）
#if UNITY_EDITOR
            string assetPath = $"Assets/AssetsPackage/Fonts/{fontName}.asset";
            font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);

            if (font == null)
            {
                // 如果是模组注册的字体但加载失败，给出更详细的提示
                if (modFontRegistry.ContainsKey(fontName))
                {
                    Debug.LogWarning($"[FontLoader] 模组字体加载失败: {fontName}\n" +
                        $"请确保字体 AssetBundle 文件存在且包含正确的 TMP_FontAsset");
                }
                else
                {
                    Debug.LogWarning($"[FontLoader] 未找到字体资源: {fontName}\n" +
                        $"请确保字体放置在以下任一位置:\n" +
                        $"1. Assets/Resources/Fonts/{fontName}.asset\n" +
                        $"2. Assets/AssetsPackage/Fonts/{fontName}.asset\n" +
                        $"3. 模组的 Fonts/ 目录（需打包为 AssetBundle）");
                }
            }
#else
            // 运行时如果是模组字体，给出模组相关提示
            if (modFontRegistry.ContainsKey(fontName))
            {
                Debug.LogError($"[FontLoader] 模组字体加载失败: {fontName}");
            }
            else
            {
                Debug.LogError($"[FontLoader] 运行时未找到字体: {fontName}，请将字体放入 Resources/Fonts/ 目录");
            }
#endif
        }

        // 缓存字体
        if (font != null)
        {
            fontCache[fontName] = font;
        }

        return font;
    }

    /// <summary>
    /// 从 TTF/OTF 文件动态创建 TMP 字体
    /// </summary>
    private static TMP_FontAsset LoadFontFromTTF(string ttfPath, string fontName)
    {
        if (!File.Exists(ttfPath))
        {
            Debug.LogWarning($"[FontLoader] 字体文件不存在: {ttfPath}");
            return null;
        }

        try
        {
            // 检查缓存的 Unity Font
            Font unityFont;
            if (!dynamicFontCache.TryGetValue(ttfPath, out unityFont) || unityFont == null)
            {
                // 从文件加载字体数据
                byte[] fontData = File.ReadAllBytes(ttfPath);

                // 创建 Unity Font
                unityFont = new Font();

                // 使用 Font.CreateDynamicFontFromOSFont 需要字体已安装在系统中
                // 对于模组字体，我们需要使用其他方式

                // Unity 2018.3+ 支持从字体数据创建
                // 但需要通过 Resources 或 AssetBundle
                // 这里我们尝试使用临时安装方式

                // 方法: 将字体文件路径传递给 Font 构造函数（Unity 内部会处理）
                // 注意: 这在某些平台可能不工作，需要测试

                // 尝试直接从路径加载
                unityFont = new Font(ttfPath);

                if (unityFont == null || string.IsNullOrEmpty(unityFont.name))
                {
                    Debug.LogWarning($"[FontLoader] 无法从路径创建字体，尝试从系统字体加载: {fontName}");

                    // 备选方案：尝试从系统字体加载（如果字体已安装）
                    string[] systemFontNames = Font.GetOSInstalledFontNames();
                    string matchedFont = null;

                    foreach (var sysFontName in systemFontNames)
                    {
                        if (sysFontName.Contains(fontName) || fontName.Contains(sysFontName))
                        {
                            matchedFont = sysFontName;
                            break;
                        }
                    }

                    if (matchedFont != null)
                    {
                        unityFont = Font.CreateDynamicFontFromOSFont(matchedFont, 32);
                    }
                }

                if (unityFont != null)
                {
                    dynamicFontCache[ttfPath] = unityFont;
                }
            }

            if (unityFont == null)
            {
                Debug.LogError($"[FontLoader] 无法加载字体文件: {ttfPath}");
                return null;
            }

            // 创建动态 TMP_FontAsset
            // 使用 Dynamic 模式，字符按需生成
            TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(
                unityFont,
                90,                              // 采样点大小
                9,                               // 填充
                GlyphRenderMode.SDFAA,           // SDF 抗锯齿渲染
                1024,                            // 图集宽度
                1024,                            // 图集高度
                AtlasPopulationMode.Dynamic      // 动态填充模式
            );

            if (tmpFont != null)
            {
                tmpFont.name = fontName;
                Debug.Log($"[FontLoader] 从 TTF 创建动态字体: {fontName}");
            }

            return tmpFont;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FontLoader] 从 TTF 创建字体失败 {ttfPath}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 从 AssetBundle 加载字体
    /// </summary>
    private static TMP_FontAsset LoadFontFromBundle(string bundlePath, string fontName)
    {
        if (!File.Exists(bundlePath))
        {
            Debug.LogWarning($"[FontLoader] 字体 Bundle 文件不存在: {bundlePath}");
            return null;
        }

        // 使用 RuntimeAssetLoader 加载
        var font = RuntimeAssetLoader.LoadAssetFromBundle<TMP_FontAsset>(bundlePath, fontName);

        // 如果按名称找不到，尝试加载 Bundle 中的第一个字体
        if (font == null)
        {
            var fonts = RuntimeAssetLoader.LoadAllAssetsFromBundle<TMP_FontAsset>(bundlePath);
            if (fonts != null && fonts.Length > 0)
            {
                font = fonts[0];
                Debug.Log($"[FontLoader] 从 Bundle 加载首个字体: {font.name}");
            }
        }

        return font;
    }

    /// <summary>
    /// 清空字体缓存
    /// </summary>
    public static void ClearCache()
    {
        fontCache.Clear();
    }

    /// <summary>
    /// 预加载所有配置的字体
    /// </summary>
    public static void PreloadFonts(List<LanguageConfigData> languageConfigs)
    {
        if (languageConfigs == null)
            return;

        foreach (var config in languageConfigs)
        {
            if (!string.IsNullOrEmpty(config.Fonts))
            {
                LoadFont(config.Fonts);
            }
        }
    }
}
