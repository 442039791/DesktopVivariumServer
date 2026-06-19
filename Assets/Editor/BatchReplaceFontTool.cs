using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 批量替换预制体中TextMeshProUGUI组件的字体工具
/// </summary>
public class BatchReplaceFontTool : EditorWindow
{
    private TMP_FontAsset targetFont;
    private string searchPath = "Assets";
    private bool includeScenes = false;
    private List<string> modifiedFiles = new List<string>();
    private Vector2 scrollPosition;
    private bool showResults = false;

    [MenuItem("工具/批量替换字体")]
    public static void ShowWindow()
    {
        GetWindow<BatchReplaceFontTool>("批量替换字体工具");
    }

    private void OnEnable()
    {
        // 自动加载目标字体
        string fontPath = "Assets/AssetsPackage/Fonts/SourceHanSansCN-Heavy SDF.asset";
        targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);

        if (targetFont == null)
        {
            Debug.LogWarning($"未能自动加载字体: {fontPath}");
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("批量替换预制体字体工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 目标字体选择
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("目标字体:", GUILayout.Width(80));
        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField(targetFont, typeof(TMP_FontAsset), false);
        EditorGUILayout.EndHorizontal();

        if (targetFont != null)
        {
            EditorGUILayout.HelpBox($"已选择字体: {targetFont.name}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("请选择目标字体", MessageType.Warning);
        }

        EditorGUILayout.Space();

        // 搜索路径
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("搜索路径:", GUILayout.Width(80));
        searchPath = EditorGUILayout.TextField(searchPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("选择搜索路径", searchPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                // 转换为相对路径
                if (path.StartsWith(Application.dataPath))
                {
                    searchPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // 是否包含场景
        includeScenes = EditorGUILayout.Toggle("同时替换场景文件", includeScenes);

        EditorGUILayout.Space();

        // 操作按钮
        EditorGUI.BeginDisabledGroup(targetFont == null);

        if (GUILayout.Button("开始批量替换", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("确认",
                $"将要替换路径 {searchPath} 下所有预制体的TextMeshProUGUI字体为:\n{targetFont.name}\n\n此操作不可撤销，是否继续？",
                "继续", "取消"))
            {
                BatchReplaceFonts();
            }
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        // 显示结果
        if (showResults)
        {
            EditorGUILayout.LabelField($"已修改 {modifiedFiles.Count} 个文件:", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            foreach (string file in modifiedFiles)
            {
                EditorGUILayout.LabelField(file);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("清除结果"))
            {
                modifiedFiles.Clear();
                showResults = false;
            }
        }
    }

    private void BatchReplaceFonts()
    {
        modifiedFiles.Clear();
        showResults = false;

        try
        {
            // 查找所有预制体
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { searchPath });
            int totalPrefabs = prefabGuids.Length;
            int processedCount = 0;
            int modifiedCount = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                processedCount++;

                // 显示进度条
                if (EditorUtility.DisplayCancelableProgressBar(
                    "批量替换字体",
                    $"正在处理: {path} ({processedCount}/{totalPrefabs})",
                    (float)processedCount / totalPrefabs))
                {
                    break;
                }

                // 加载预制体
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                // 查找所有TextMeshProUGUI组件
                TextMeshProUGUI[] textComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (textComponents.Length == 0) continue;

                bool modified = false;

                // 替换字体
                foreach (var textComponent in textComponents)
                {
                    if (textComponent.font != targetFont)
                    {
                        // 使用PrefabUtility来修改预制体
                        GameObject instance = PrefabUtility.LoadPrefabContents(path);
                        TextMeshProUGUI[] instanceTexts = instance.GetComponentsInChildren<TextMeshProUGUI>(true);

                        foreach (var instanceText in instanceTexts)
                        {
                            if (instanceText.font != targetFont)
                            {
                                instanceText.font = targetFont;
                                modified = true;
                            }
                        }

                        if (modified)
                        {
                            PrefabUtility.SaveAsPrefabAsset(instance, path);
                        }

                        PrefabUtility.UnloadPrefabContents(instance);
                        break;
                    }
                }

                if (modified)
                {
                    modifiedFiles.Add(path);
                    modifiedCount++;
                }
            }

            // 如果需要处理场景
            if (includeScenes)
            {
                string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { searchPath });
                // 注意：场景文件需要打开后才能修改，这里暂时跳过
                Debug.LogWarning("场景文件替换功能暂未实现，请手动处理或在场景打开时运行脚本");
            }

            EditorUtility.ClearProgressBar();

            // 刷新资源数据库
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            showResults = true;

            // 显示完成消息
            EditorUtility.DisplayDialog("完成",
                $"批量替换完成！\n\n总共处理: {totalPrefabs} 个预制体\n已修改: {modifiedCount} 个预制体",
                "确定");

            Debug.Log($"[BatchReplaceFontTool] 批量替换完成！已修改 {modifiedCount} 个预制体");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("错误", $"批量替换过程中发生错误:\n{e.Message}", "确定");
            Debug.LogError($"[BatchReplaceFontTool] 错误: {e}");
        }
    }
}
