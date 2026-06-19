#if UNITY_EDITOR && !DISABLESTEAMWORKS && STEAMWORKSNET
using UnityEditor;
using UnityEngine;

/// <summary>
/// Steam Cloud 清理工具的 Editor 菜单
/// </summary>
public class SteamCloudCleanupMenu
{
    [MenuItem("Tools/Steam/Clean Cloud Files (需要打包后运行)")]
    public static void CleanCloudFiles()
    {
        EditorUtility.DisplayDialog(
            "Steam Cloud 清理工具",
            "此功能需要在打包后的游戏中运行，编辑器模式下无法访问 Steam Cloud。\n\n" +
            "请按照以下步骤操作：\n" +
            "1. 打包游戏\n" +
            "2. 运行打包后的游戏\n" +
            "3. 在游戏中打开控制台（按 ~ 键）\n" +
            "4. 输入命令调用 SteamCloudCleanupTool.CleanupCloudFiles()\n\n" +
            "或者在代码中添加调用，例如在启动时检测并清理。",
            "确定"
        );
    }

    [MenuItem("Tools/Steam/清理本地 Save 目录中的 .meta 和 .backup 文件")]
    public static void CleanLocalSaveDirectory()
    {
        string savePath = Application.streamingAssetsPath + "/Save";

        if (!System.IO.Directory.Exists(savePath))
        {
            EditorUtility.DisplayDialog("清理工具", $"Save 目录不存在：{savePath}", "确定");
            return;
        }

        int deletedCount = 0;
        string[] files = System.IO.Directory.GetFiles(savePath);

        foreach (string file in files)
        {
            string fileName = System.IO.Path.GetFileName(file);

            if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase) ||
                fileName.Contains(".backup", System.StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    System.IO.File.Delete(file);
                    Debug.Log($"已删除: {fileName}");
                    deletedCount++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"删除失败 {fileName}: {e.Message}");
                }
            }
        }

        EditorUtility.DisplayDialog(
            "清理完成",
            $"已删除 {deletedCount} 个文件\n路径：{savePath}",
            "确定"
        );

        // 刷新 Asset Database
        AssetDatabase.Refresh();
    }
}
#endif
