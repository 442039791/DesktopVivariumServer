using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Windows64 桌面自动打包入口（供命令行 batchmode 调用）
///
/// 用法：
///   Unity.exe -batchmode -quit -projectPath <本工程> ^
///     -executeMethod BuildWindowsPlayer.Build ^
///     -outputRoot "D:\DesktopVivarium" -channel Test [-buildBundles 1] -logFile <日志>
///
/// 输出：<outputRoot>/Server/DesktopVivarium.exe
/// 注意：客户端工程的同名脚本输出到 <outputRoot>/Client/DesktopVivarium(Wallpaper).exe，
/// Client 与 Server 两个目录必须同级摆放——运行时双方按此相对路径互相寻找
/// （服务器到 ../Client/ 找客户端 exe，客户端到 ../Server/ 找服务器 StreamingAssets）。
///
/// 默认复用工程里已有的 AssetBundles/win64/<channel> 产物；
/// 传 -buildBundles 1 可在打包前先重建 AssetBundle（资源有改动时用）。
/// </summary>
public static class BuildWindowsPlayer
{
    private const string ExeName = "DesktopVivarium";
    private const string OutputSubFolder = "Server";

    /// <summary>默认输出根目录（与运行时 Client/Server 同级约定配套）</summary>
    private const string DefaultOutputRoot = "D:\\DesktopVivarium";

    /// <summary>编辑器内一键打包入口（batchmode 死锁时的手动替代路径）</summary>
    [MenuItem("Tools/AutoBuild/Windows 服务器 (输出 D:/DesktopVivarium/Server)")]
    public static void BuildFromMenu()
    {
        Run(DefaultOutputRoot, "Test", false);
    }

    /// <summary>batchmode 命令行入口</summary>
    public static void Build()
    {
        string[] args = Environment.GetCommandLineArgs();
        string outputRoot = GetArg(args, "-outputRoot") ?? DefaultOutputRoot;
        string channel = GetArg(args, "-channel") ?? "Test";
        bool buildBundles = (GetArg(args, "-buildBundles") ?? "0") == "1";
        Run(outputRoot, channel, buildBundles);
    }

    private static void Run(string outputRoot, string channel, bool buildBundles)
    {
        int exitCode = 0;
        try
        {
            var target = BuildTarget.StandaloneWindows64;

            // 1. AssetBundle：默认复用已有产物，可选重建
            if (buildBundles)
            {
                Debug.Log($"[BuildWindowsPlayer] 重建 AssetBundles (channel={channel}) ...");
                BuildPlayer.BuildAssetBundles(target, channel);
            }

            // 2. 把 Bundle 拷进 StreamingAssets（随后打进包内）
            Debug.Log($"[BuildWindowsPlayer] 拷贝 AssetBundles 到 StreamingAssets (channel={channel}) ...");
            PackageUtils.CopyAssetBundlesToStreamingAssets(target, channel);
            AssetDatabase.Refresh();

            // 3. 收集启用场景
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildWindowsPlayer] Build Settings 里没有启用的场景，无法打包！");
                EditorApplication.Exit(1);
                return;
            }

            // 4. 出包
            string outputDir = Path.Combine(Path.GetFullPath(outputRoot), OutputSubFolder);
            string exePath = Path.Combine(outputDir, ExeName + ".exe");
            Directory.CreateDirectory(outputDir);

            Debug.Log($"[BuildWindowsPlayer] 开始打包: {exePath}, 场景: {string.Join(", ", scenes)}");
            BuildReport report = BuildPipeline.BuildPlayer(scenes, exePath, target, BuildOptions.None);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildWindowsPlayer] 打包失败: {report.summary.result}, " +
                               $"错误数: {report.summary.totalErrors}");
                exitCode = 1;
            }
            else
            {
                Debug.Log($"[BuildWindowsPlayer] 打包成功: {exePath}, " +
                          $"总大小: {report.summary.totalSize / (1024 * 1024)} MB, " +
                          $"耗时: {report.summary.totalTime.TotalSeconds:F0}s");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BuildWindowsPlayer] 打包异常: {ex.Message}\n{ex.StackTrace}");
            exitCode = 1;
        }

        // batchmode 下用显式退出码，命令行才能感知成败；
        // 编辑器菜单触发时不退出，避免关掉用户的编辑器
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(exitCode);
        }
    }

    private static string GetArg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
