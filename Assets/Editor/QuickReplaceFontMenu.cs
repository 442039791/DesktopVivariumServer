using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 快速批量替换字体的菜单脚本
/// </summary>
public class QuickReplaceFontMenu
{
    private const string TARGET_FONT_PATH = "Assets/AssetsPackage/Fonts/SourceHanSansCN-Heavy SDF.asset";

    [MenuItem("工具/一键替换所有UI字体为 SourceHanSansCN-Heavy SDF")]
    public static void ReplaceAllUIFonts()
    {
        // 加载目标字体
        TMP_FontAsset targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TARGET_FONT_PATH);

        if (targetFont == null)
        {
            EditorUtility.DisplayDialog("错误", $"无法加载字体文件:\n{TARGET_FONT_PATH}", "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认",
            $"将要替换所有预制体的TextMeshProUGUI字体为:\n{targetFont.name}\n\n" +
            "此操作会修改所有预制体文件，建议先备份或提交版本控制。\n\n是否继续？",
            "继续", "取消"))
        {
            return;
        }

        List<string> modifiedFiles = new List<string>();
        int totalModified = 0;

        try
        {
            // 查找所有预制体（排除第三方插件）
            string[] searchPaths = new[]
            {
                "Assets/AssetsPackage/GamePrefab",
                "Assets/Scripts"
            };

            string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab", searchPaths);
            int totalPrefabs = allPrefabGuids.Length;
            int processedCount = 0;

            foreach (string guid in allPrefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                processedCount++;

                // 显示进度条
                if (EditorUtility.DisplayCancelableProgressBar(
                    "批量替换字体",
                    $"正在处理: {path} ({processedCount}/{totalPrefabs})",
                    (float)processedCount / totalPrefabs))
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("取消", "操作已取消", "确定");
                    return;
                }

                // 替换预制体中的字体
                if (ReplaceFontInPrefab(path, targetFont))
                {
                    modifiedFiles.Add(path);
                    totalModified++;
                }
            }

            EditorUtility.ClearProgressBar();

            // 保存并刷新
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 显示结果
            string resultMessage = $"批量替换完成！\n\n" +
                $"总共处理: {totalPrefabs} 个预制体\n" +
                $"已修改: {totalModified} 个预制体\n\n" +
                $"修改的文件列表已输出到Console";

            EditorUtility.DisplayDialog("完成", resultMessage, "确定");

            // 输出详细信息到控制台
            Debug.Log($"========== 批量替换字体完成 ==========");
            Debug.Log($"目标字体: {targetFont.name}");
            Debug.Log($"总共处理: {totalPrefabs} 个预制体");
            Debug.Log($"已修改: {totalModified} 个预制体");
            Debug.Log($"========== 修改的文件列表 ==========");
            foreach (string file in modifiedFiles)
            {
                Debug.Log($"  - {file}");
            }
            Debug.Log($"======================================");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("错误", $"批量替换过程中发生错误:\n{e.Message}", "确定");
            Debug.LogError($"[QuickReplaceFontMenu] 错误: {e}");
        }
    }

    /// <summary>
    /// 替换单个预制体中的字体
    /// </summary>
    private static bool ReplaceFontInPrefab(string prefabPath, TMP_FontAsset targetFont)
    {
        try
        {
            // 加载预制体内容
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabContents == null) return false;

            // 查找所有TextMeshProUGUI组件（包括未激活的）
            TextMeshProUGUI[] textComponents = prefabContents.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (textComponents.Length == 0)
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return false;
            }

            bool modified = false;

            // 替换字体
            foreach (var textComponent in textComponents)
            {
                if (textComponent.font != targetFont)
                {
                    textComponent.font = targetFont;
                    modified = true;
                }
            }

            // 如果有修改，保存预制体
            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            }

            // 卸载预制体内容
            PrefabUtility.UnloadPrefabContents(prefabContents);

            return modified;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuickReplaceFontMenu] 处理预制体 {prefabPath} 时出错: {e.Message}");
            return false;
        }
    }

    [MenuItem("工具/统计当前使用的所有字体")]
    public static void ListAllUsedFonts()
    {
        Dictionary<string, int> fontUsageCount = new Dictionary<string, int>();

        // 查找所有预制体
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int totalPrefabs = allPrefabGuids.Length;
        int processedCount = 0;

        foreach (string guid in allPrefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            processedCount++;

            // 显示进度条
            EditorUtility.DisplayProgressBar(
                "统计字体使用情况",
                $"正在分析: {path} ({processedCount}/{totalPrefabs})",
                (float)processedCount / totalPrefabs);

            // 加载预制体
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // 查找所有TextMeshProUGUI组件
            TextMeshProUGUI[] textComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var textComponent in textComponents)
            {
                if (textComponent.font != null)
                {
                    string fontName = textComponent.font.name;
                    if (fontUsageCount.ContainsKey(fontName))
                    {
                        fontUsageCount[fontName]++;
                    }
                    else
                    {
                        fontUsageCount[fontName] = 1;
                    }
                }
                else
                {
                    // 没有字体的组件
                    if (fontUsageCount.ContainsKey("[无字体]"))
                    {
                        fontUsageCount["[无字体]"]++;
                    }
                    else
                    {
                        fontUsageCount["[无字体]"] = 1;
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();

        // 输出统计结果
        Debug.Log("========== 字体使用统计 ==========");
        foreach (var kvp in fontUsageCount)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} 个组件");
        }
        Debug.Log("==================================");

        EditorUtility.DisplayDialog("统计完成",
            $"字体使用统计已输出到Console\n共找到 {fontUsageCount.Count} 种不同的字体",
            "确定");
    }
}
