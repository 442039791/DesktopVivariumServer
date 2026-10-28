using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 打包后清理 StreamingAssets 中的运行时数据（方案 B：只清产物，不动工程文件）。
///
/// 背景：编辑器测试期间，存档/蓝图等运行时数据会直接写进工程
/// Assets/StreamingAssets 下（见 GameConfig.SaveDirectoryPath），
/// 而 Unity 打包会把 StreamingAssets 原样全量拷入产物，导致测试数据混进发布包。
///
/// 清理安全性依据（运行时均按需自建，无需预置）：
///   - Save/                 : SaveSystem.EnsureSaveDirectoryExists() 自建目录；
///                              首次启动走 GameInitializer 初始化默认存档流程。
///   - BluePrint/BluePrintData、BluePrintIcon、BluePrintSaveDate.txt
///                           : 蓝图索引缺失时 UI_BluePrint.Init() 以空列表起步，
///                              玩家首次保存蓝图时自建。
///   - BluePrintBioTank/     : 全工程仅 SysDefine 中定义常量、无任何调用，死数据。
///   !! BluePrint/DefaultBioTank 必须保留：首次启动模板
///      （GameConfig.FirstStartSaveDataPath）。
/// </summary>
public class StreamingAssetsBuildCleaner : IPostprocessBuildWithReport
{
    /// <summary>
    /// 排除清单：相对于打包产物 StreamingAssets 目录的路径（目录或文件均可）。
    /// 新增排除项时必须先确认运行时缺失自愈，参考类头注释的核查结论。
    /// </summary>
    private static readonly string[] ExcludedPaths =
    {
        "Save",                            // 测试存档：ServerPackage.dat / *_BioTankPackage.gz / ModSettings.json
        "BluePrint/BluePrintData",         // 玩家蓝图数据（运行时自建）
        "BluePrint/BluePrintIcon",         // 玩家蓝图图标（运行时自建）
        "BluePrint/BluePrintSaveDate.txt", // 蓝图索引（缺失 = 空列表起步）
        "BluePrintBioTank",                // 无代码引用的历史数据
    };

    /// <summary>排在其他后处理之后执行，避免被后续拷贝动作覆盖</summary>
    public int callbackOrder => 100;

    public void OnPostprocessBuild(BuildReport report)
    {
        try
        {
            // report.summary.outputPath 为 exe 完整路径，数据目录约定为 <exe名>_Data
            string dataPath = Path.ChangeExtension(report.summary.outputPath, null) + "_Data";
            string streamingAssets = Path.Combine(dataPath, "StreamingAssets");

            if (!Directory.Exists(streamingAssets))
            {
                Debug.Log($"[StreamingAssetsBuildCleaner] 产物无 StreamingAssets 目录（非 Windows 桌面布局），跳过清理: {streamingAssets}");
                return;
            }

            int removed = 0;
            foreach (string rel in ExcludedPaths)
            {
                string target = Path.Combine(streamingAssets, rel);
                if (File.Exists(target))
                {
                    File.Delete(target);
                    removed++;
                    Debug.Log($"[StreamingAssetsBuildCleaner] 已删除文件: {rel}");
                }
                else if (Directory.Exists(target))
                {
                    Directory.Delete(target, true);
                    removed++;
                    Debug.Log($"[StreamingAssetsBuildCleaner] 已删除目录: {rel}");
                }
            }

            Debug.Log($"[StreamingAssetsBuildCleaner] 清理完成，处理 {removed}/{ExcludedPaths.Length} 项，产物: {streamingAssets}");
        }
        catch (Exception e)
        {
            // 清理失败必须让构建失败，避免带测试存档的包被误认为可用
            Debug.LogError($"[StreamingAssetsBuildCleaner] 清理失败: {e.Message}\n{e.StackTrace}");
            throw;
        }
    }
}
