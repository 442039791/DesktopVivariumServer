using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 批量替换场景中TextMeshProUGUI组件的字体工具
/// </summary>
public class BatchReplaceSceneFontTool
{
    private const string TARGET_FONT_PATH = "Assets/AssetsPackage/Fonts/SourceHanSansCN-Heavy SDF.asset";

    [MenuItem("工具/替换当前场景的所有字体")]
    public static void ReplaceCurrentSceneFonts()
    {
        // 检查当前场景
        Scene currentScene = EditorSceneManager.GetActiveScene();
        if (!currentScene.IsValid() || string.IsNullOrEmpty(currentScene.path))
        {
            EditorUtility.DisplayDialog("错误", "当前没有打开有效的场景，或场景未保存", "确定");
            return;
        }

        // 加载目标字体
        TMP_FontAsset targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TARGET_FONT_PATH);
        if (targetFont == null)
        {
            EditorUtility.DisplayDialog("错误", $"无法加载字体文件:\n{TARGET_FONT_PATH}", "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认",
            $"将要替换当前场景的所有TextMeshProUGUI字体为:\n{targetFont.name}\n\n" +
            $"场景: {currentScene.name}\n路径: {currentScene.path}\n\n是否继续？",
            "继续", "取消"))
        {
            return;
        }

        // 替换场景中的字体
        int modifiedCount = ReplaceSceneFonts(currentScene, targetFont);

        // 保存场景
        if (modifiedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);
            EditorUtility.DisplayDialog("完成",
                $"场景字体替换完成！\n\n已修改 {modifiedCount} 个TextMeshProUGUI组件",
                "确定");
            Debug.Log($"[BatchReplaceSceneFontTool] 场景 {currentScene.name} 字体替换完成，已修改 {modifiedCount} 个组件");
        }
        else
        {
            EditorUtility.DisplayDialog("提示",
                "当前场景中没有找到需要替换的TextMeshProUGUI组件",
                "确定");
        }
    }

    [MenuItem("工具/替换所有场景的字体")]
    public static void ReplaceAllScenesFonts()
    {
        // 加载目标字体
        TMP_FontAsset targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TARGET_FONT_PATH);
        if (targetFont == null)
        {
            EditorUtility.DisplayDialog("错误", $"无法加载字体文件:\n{TARGET_FONT_PATH}", "确定");
            return;
        }

        // 查找所有场景
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        if (sceneGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "未找到任何场景文件", "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog("确认",
            $"将要替换所有场景的TextMeshProUGUI字体为:\n{targetFont.name}\n\n" +
            $"共找到 {sceneGuids.Length} 个场景\n\n此操作会保存所有场景，建议先备份。\n\n是否继续？",
            "继续", "取消"))
        {
            return;
        }

        // 保存当前场景状态
        Scene currentScene = EditorSceneManager.GetActiveScene();
        string currentScenePath = currentScene.path;

        List<string> modifiedScenes = new List<string>();
        int totalModified = 0;
        int processedCount = 0;

        try
        {
            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                processedCount++;

                // 显示进度条
                if (EditorUtility.DisplayCancelableProgressBar(
                    "批量替换场景字体",
                    $"正在处理: {scenePath} ({processedCount}/{sceneGuids.Length})",
                    (float)processedCount / sceneGuids.Length))
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("取消", "操作已取消", "确定");
                    return;
                }

                // 打开场景
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                if (!scene.IsValid()) continue;

                // 替换字体
                int modifiedCount = ReplaceSceneFonts(scene, targetFont);

                if (modifiedCount > 0)
                {
                    // 保存场景
                    EditorSceneManager.SaveScene(scene);
                    modifiedScenes.Add(scenePath);
                    totalModified += modifiedCount;
                    Debug.Log($"[BatchReplaceSceneFontTool] 场景 {scene.name} 已修改 {modifiedCount} 个组件");
                }
            }

            EditorUtility.ClearProgressBar();

            // 恢复原始场景
            if (!string.IsNullOrEmpty(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }

            // 显示结果
            string resultMessage = $"批量替换完成！\n\n" +
                $"总共处理: {sceneGuids.Length} 个场景\n" +
                $"已修改: {modifiedScenes.Count} 个场景\n" +
                $"修改组件总数: {totalModified}\n\n" +
                $"详细信息已输出到Console";

            EditorUtility.DisplayDialog("完成", resultMessage, "确定");

            // 输出详细信息
            Debug.Log($"========== 批量替换场景字体完成 ==========");
            Debug.Log($"目标字体: {targetFont.name}");
            Debug.Log($"总共处理: {sceneGuids.Length} 个场景");
            Debug.Log($"已修改: {modifiedScenes.Count} 个场景");
            Debug.Log($"修改组件总数: {totalModified}");
            Debug.Log($"========== 修改的场景列表 ==========");
            foreach (string scenePath in modifiedScenes)
            {
                Debug.Log($"  - {scenePath}");
            }
            Debug.Log($"======================================");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("错误", $"批量替换过程中发生错误:\n{e.Message}", "确定");
            Debug.LogError($"[BatchReplaceSceneFontTool] 错误: {e}");

            // 尝试恢复原始场景
            if (!string.IsNullOrEmpty(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }
    }

    [MenuItem("工具/替换指定场景的字体")]
    public static void ReplaceSpecificSceneFonts()
    {
        // 打开场景选择对话框
        string scenePath = EditorUtility.OpenFilePanel(
            "选择场景文件",
            Application.dataPath,
            "unity");

        if (string.IsNullOrEmpty(scenePath))
        {
            return;
        }

        // 转换为相对路径
        if (scenePath.StartsWith(Application.dataPath))
        {
            scenePath = "Assets" + scenePath.Substring(Application.dataPath.Length);
        }

        // 加载目标字体
        TMP_FontAsset targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TARGET_FONT_PATH);
        if (targetFont == null)
        {
            EditorUtility.DisplayDialog("错误", $"无法加载字体文件:\n{TARGET_FONT_PATH}", "确定");
            return;
        }

        // 保存当前场景
        Scene currentScene = EditorSceneManager.GetActiveScene();
        string currentScenePath = currentScene.path;

        try
        {
            // 打开目标场景
            Scene targetScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!targetScene.IsValid())
            {
                EditorUtility.DisplayDialog("错误", $"无法打开场景:\n{scenePath}", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("确认",
                $"将要替换场景的所有TextMeshProUGUI字体为:\n{targetFont.name}\n\n" +
                $"场景: {targetScene.name}\n是否继续？",
                "继续", "取消"))
            {
                // 恢复原始场景
                if (!string.IsNullOrEmpty(currentScenePath))
                {
                    EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
                }
                return;
            }

            // 替换字体
            int modifiedCount = ReplaceSceneFonts(targetScene, targetFont);

            if (modifiedCount > 0)
            {
                EditorSceneManager.SaveScene(targetScene);
                EditorUtility.DisplayDialog("完成",
                    $"场景字体替换完成！\n\n已修改 {modifiedCount} 个TextMeshProUGUI组件",
                    "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("提示",
                    "场景中没有找到需要替换的TextMeshProUGUI组件",
                    "确定");
            }

            // 恢复原始场景
            if (!string.IsNullOrEmpty(currentScenePath) && currentScenePath != scenePath)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"替换过程中发生错误:\n{e.Message}", "确定");
            Debug.LogError($"[BatchReplaceSceneFontTool] 错误: {e}");

            // 恢复原始场景
            if (!string.IsNullOrEmpty(currentScenePath))
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }
    }

    /// <summary>
    /// 替换指定场景中的所有字体
    /// </summary>
    private static int ReplaceSceneFonts(Scene scene, TMP_FontAsset targetFont)
    {
        int modifiedCount = 0;

        // 获取场景中的所有根GameObject
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            // 查找所有TextMeshProUGUI组件（包括未激活的）
            TextMeshProUGUI[] textComponents = rootObject.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var textComponent in textComponents)
            {
                if (textComponent.font != targetFont)
                {
                    // 记录修改前的状态（用于Undo）
                    Undo.RecordObject(textComponent, "Replace Font");
                    textComponent.font = targetFont;
                    EditorUtility.SetDirty(textComponent);
                    modifiedCount++;
                }
            }
        }

        return modifiedCount;
    }

    [MenuItem("工具/统计当前场景的字体使用情况")]
    public static void StatisticsCurrentSceneFonts()
    {
        Scene currentScene = EditorSceneManager.GetActiveScene();
        if (!currentScene.IsValid())
        {
            EditorUtility.DisplayDialog("错误", "当前没有打开有效的场景", "确定");
            return;
        }

        Dictionary<string, int> fontUsageCount = new Dictionary<string, int>();
        int totalComponents = 0;

        // 获取场景中的所有根GameObject
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            // 查找所有TextMeshProUGUI组件
            TextMeshProUGUI[] textComponents = rootObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            totalComponents += textComponents.Length;

            foreach (var textComponent in textComponents)
            {
                string fontName = textComponent.font != null ? textComponent.font.name : "[无字体]";

                if (fontUsageCount.ContainsKey(fontName))
                {
                    fontUsageCount[fontName]++;
                }
                else
                {
                    fontUsageCount[fontName] = 1;
                }
            }
        }

        // 输出统计结果
        Debug.Log($"========== 场景字体使用统计 ==========");
        Debug.Log($"场景: {currentScene.name}");
        Debug.Log($"TextMeshProUGUI组件总数: {totalComponents}");
        Debug.Log($"使用的字体种类数: {fontUsageCount.Count}");
        Debug.Log($"====================================");
        foreach (var kvp in fontUsageCount)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} 个组件");
        }
        Debug.Log($"====================================");

        EditorUtility.DisplayDialog("统计完成",
            $"场景: {currentScene.name}\n\n" +
            $"TextMeshProUGUI组件总数: {totalComponents}\n" +
            $"使用的字体种类数: {fontUsageCount.Count}\n\n" +
            $"详细信息已输出到Console",
            "确定");
    }
}
