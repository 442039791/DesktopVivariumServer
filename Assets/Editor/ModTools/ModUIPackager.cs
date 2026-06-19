using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 模组 UI 打包工具
/// 用于将 UI 预制体打包成 AssetBundle，供模组使用
/// </summary>
public class ModUIPackager : EditorWindow
{
    private string modName = "MyMod";
    private string outputPath = "";
    private List<GameObject> prefabsToPackage = new List<GameObject>();
    private Vector2 scrollPosition;
    private bool showHelp = true;

    [MenuItem("Tools/Mod Tools/UI 打包工具", false, 100)]
    public static void ShowWindow()
    {
        var window = GetWindow<ModUIPackager>("Mod UI 打包工具");
        window.minSize = new Vector2(450, 500);
    }

    private void OnEnable()
    {
        // 默认输出到 Mods 目录
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Mods"));
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Mod UI 打包工具", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // 帮助信息
        showHelp = EditorGUILayout.Foldout(showHelp, "使用说明", true);
        if (showHelp)
        {
            EditorGUILayout.HelpBox(
                "此工具用于将 UI 预制体打包成 AssetBundle，供模组替换或新增游戏 UI。\n\n" +
                "使用步骤：\n" +
                "1. 输入模组名称\n" +
                "2. 将 UI 预制体拖入下方列表\n" +
                "3. 点击「打包」按钮\n\n" +
                "替换现有 UI：预制体名称需与游戏原版 UI 名称相同\n" +
                "新增 UI：使用任意名称，然后在模组脚本中调用显示",
                MessageType.Info);
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("基本设置", EditorStyles.boldLabel);

        // 模组名称
        modName = EditorGUILayout.TextField("模组名称", modName);

        // 输出路径
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("输出目录", outputPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("选择输出目录", outputPath, "");
            if (!string.IsNullOrEmpty(selected))
            {
                outputPath = selected;
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);
        EditorGUILayout.LabelField("UI 预制体列表", EditorStyles.boldLabel);

        // 拖放区域
        var dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "将预制体拖放到此处添加", EditorStyles.helpBox);
        HandleDragAndDrop(dropArea);

        // 预制体列表
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        for (int i = prefabsToPackage.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();

            prefabsToPackage[i] = (GameObject)EditorGUILayout.ObjectField(
                prefabsToPackage[i], typeof(GameObject), false);

            if (prefabsToPackage[i] != null)
            {
                EditorGUILayout.LabelField($"→ {prefabsToPackage[i].name}.prefab.bundle",
                    GUILayout.Width(200));
            }

            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                prefabsToPackage.RemoveAt(i);
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        // 添加/清空按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加预制体"))
        {
            prefabsToPackage.Add(null);
        }
        if (GUILayout.Button("清空列表"))
        {
            prefabsToPackage.Clear();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(15);

        // 打包按钮
        GUI.enabled = prefabsToPackage.Count > 0 && !string.IsNullOrEmpty(modName);
        if (GUILayout.Button("打包 UI", GUILayout.Height(40)))
        {
            PackageUI();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        // 快速操作
        EditorGUILayout.LabelField("快速操作", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("打开输出目录"))
        {
            string fullPath = Path.Combine(outputPath, modName, "UI");
            if (Directory.Exists(fullPath))
            {
                EditorUtility.RevealInFinder(fullPath);
            }
            else if (Directory.Exists(outputPath))
            {
                EditorUtility.RevealInFinder(outputPath);
            }
        }
        if (GUILayout.Button("创建模组模板"))
        {
            CreateModTemplate();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void HandleDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is GameObject go)
                        {
                            string path = AssetDatabase.GetAssetPath(go);
                            if (path.EndsWith(".prefab") && !prefabsToPackage.Contains(go))
                            {
                                prefabsToPackage.Add(go);
                            }
                        }
                    }
                }
                evt.Use();
                break;
        }
    }

    private void PackageUI()
    {
        // 清理空项
        prefabsToPackage.RemoveAll(p => p == null);

        if (prefabsToPackage.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "请添加至少一个预制体", "确定");
            return;
        }

        // 创建输出目录
        string modUIPath = Path.Combine(outputPath, modName, "UI");
        if (!Directory.Exists(modUIPath))
        {
            Directory.CreateDirectory(modUIPath);
        }

        int successCount = 0;
        List<string> failedItems = new List<string>();

        foreach (var prefab in prefabsToPackage)
        {
            if (prefab == null) continue;

            try
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefab);
                string bundleName = $"{prefab.name}.prefab.bundle";
                string outputFile = Path.Combine(modUIPath, bundleName);

                // 设置 AssetBundle 名称
                AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
                string originalBundleName = importer.assetBundleName;
                importer.assetBundleName = bundleName;
                importer.SaveAndReimport();

                // 构建 AssetBundle
                AssetBundleBuild[] builds = new AssetBundleBuild[1];
                builds[0].assetBundleName = bundleName;
                builds[0].assetNames = new string[] { prefabPath };

                BuildPipeline.BuildAssetBundles(
                    modUIPath,
                    builds,
                    BuildAssetBundleOptions.None,
                    EditorUserBuildSettings.activeBuildTarget);

                // 恢复原来的 AssetBundle 名称
                importer.assetBundleName = originalBundleName;
                importer.SaveAndReimport();

                // 清理多余文件
                CleanupBundleFiles(modUIPath, bundleName);

                successCount++;
                Debug.Log($"[ModUIPackager] 打包成功: {bundleName}");
            }
            catch (System.Exception ex)
            {
                failedItems.Add(prefab.name);
                Debug.LogError($"[ModUIPackager] 打包失败 {prefab.name}: {ex.Message}");
            }
        }

        // 刷新资源
        AssetDatabase.Refresh();

        // 显示结果
        string message = $"打包完成！\n成功: {successCount} 个\n失败: {failedItems.Count} 个";
        if (failedItems.Count > 0)
        {
            message += $"\n\n失败项目:\n{string.Join("\n", failedItems)}";
        }
        message += $"\n\n输出目录:\n{modUIPath}";

        EditorUtility.DisplayDialog("打包结果", message, "确定");
        EditorUtility.RevealInFinder(modUIPath);
    }

    private void CleanupBundleFiles(string directory, string keepBundleName)
    {
        // 删除 manifest 文件和主 bundle
        string[] filesToDelete = new string[]
        {
            Path.Combine(directory, "UI"),
            Path.Combine(directory, "UI.manifest"),
            Path.Combine(directory, keepBundleName + ".manifest")
        };

        foreach (var file in filesToDelete)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    private void CreateModTemplate()
    {
        if (string.IsNullOrEmpty(modName))
        {
            EditorUtility.DisplayDialog("提示", "请先输入模组名称", "确定");
            return;
        }

        string modPath = Path.Combine(outputPath, modName);

        // 创建目录结构
        Directory.CreateDirectory(Path.Combine(modPath, "UI"));
        Directory.CreateDirectory(Path.Combine(modPath, "Plugins", "Server"));
        Directory.CreateDirectory(Path.Combine(modPath, "Source"));

        // 创建 manifest.json
        string manifest = $@"{{
    ""ModId"": ""com.yourname.{modName.ToLower()}"",
    ""Name"": ""{modName}"",
    ""Version"": ""1.0.0"",
    ""Author"": ""YourName"",
    ""Description"": ""模组描述"",
    ""GameVersion"": ""1.0.0"",
    ""Dependencies"": [],
    ""Scripts"": {{
        ""Server"": [
            ""Plugins/Server/{modName}Mod.dll""
        ]
    }}
}}";
        File.WriteAllText(Path.Combine(modPath, "manifest.json"), manifest);

        // 创建示例脚本
        string script = $@"using System;
using UnityEngine;
using SUIFW;

/// <summary>
/// {modName} 模组
/// </summary>
public class {modName}Mod : IModEntry
{{
    private IModContext _context;
    private bool _isEnabled = false;

    public string ModId => ""com.yourname.{modName.ToLower()}"";
    public string ModName => ""{modName}"";
    public string Version => ""1.0.0"";

    public void Initialize(IModContext context)
    {{
        _context = context;
        _context.Logger.Log(""{modName} 初始化中..."");
    }}

    public void OnEnable()
    {{
        if (_isEnabled) return;

        // 在这里注册你的 UI
        // 如果 UI 名称与游戏原版相同，会自动替换
        // 例如: 注册名为 ""UI_Main"" 的预制体会替换主界面

        _isEnabled = true;
        _context.Logger.Log(""{modName} 已启用"");
    }}

    public void OnDisable()
    {{
        if (!_isEnabled) return;
        _isEnabled = false;
        _context.Logger.Log(""{modName} 已禁用"");
    }}

    public void OnUnload()
    {{
        if (_isEnabled) OnDisable();
        _context.Logger.Log(""{modName} 已卸载"");
    }}

    public void OnUpdate()
    {{
        // 每帧更新逻辑
    }}
}}
";
        File.WriteAllText(Path.Combine(modPath, "Source", $"{modName}Mod.cs"), script);

        // 创建 README
        string readme = $@"# {modName}

## 目录结构

```
{modName}/
├── manifest.json       # 模组配置
├── UI/                 # UI 预制体 AssetBundle
│   └── *.prefab.bundle
├── Plugins/
│   └── Server/
│       └── {modName}Mod.dll
└── Source/
    └── {modName}Mod.cs
```

## UI 替换说明

- 将 UI 预制体打包后放入 UI/ 目录
- 预制体名称与游戏原版 UI 相同时会自动替换
- 新增 UI 需要在脚本中手动调用显示

## 编译

```bash
cd Source
dotnet build -c Release
```
";
        File.WriteAllText(Path.Combine(modPath, "README.txt"), readme);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", $"模组模板已创建:\n{modPath}", "确定");
        EditorUtility.RevealInFinder(modPath);
    }
}
