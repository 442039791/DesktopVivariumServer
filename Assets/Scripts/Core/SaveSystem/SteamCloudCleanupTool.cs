#if !DISABLESTEAMWORKS && STEAMWORKSNET
using UnityEngine;
using Steamworks;

/// <summary>
/// Steam Cloud 清理工具 - 用于删除云端的垃圾文件
/// 使用方法：在 Unity Editor 菜单中选择 Tools > Steam > Clean Cloud Files
/// 或者在非编辑器模式下调用 CleanupCloudFiles() 方法
/// </summary>
public class SteamCloudCleanupTool
{
    /// <summary>
    /// 清理 Steam Cloud 上的所有 .meta 和 .backup 文件
    /// 注意：此方法只能在非编辑器模式下运行（需要真实的 Steam 环境）
    /// </summary>
    public static void CleanupCloudFiles()
    {
#if UNITY_EDITOR
        Debug.LogWarning("[SteamCloudCleanup] 此功能只能在打包后的游戏中运行，编辑器模式下无法访问 Steam Cloud");
        return;
#else
        if (!SteamAPI.IsSteamRunning())
        {
            Debug.LogError("[SteamCloudCleanup] Steam 未运行");
            return;
        }

        if (!SteamRemoteStorage.IsCloudEnabledForAccount())
        {
            Debug.LogWarning("[SteamCloudCleanup] Steam 云存档未为账户启用");
            return;
        }

        int fileCount = SteamRemoteStorage.GetFileCount();
        int deletedCount = 0;

        Debug.Log($"[SteamCloudCleanup] 开始扫描云端文件，共 {fileCount} 个文件");

        for (int i = fileCount - 1; i >= 0; i--)
        {
            string fileName;
            int fileSize;

            fileName = SteamRemoteStorage.GetFileNameAndSize(i, out fileSize);

            if (string.IsNullOrEmpty(fileName))
                continue;

            // 检查是否是需要删除的文件
            bool shouldDelete = false;

            // 删除 .meta 文件
            if (fileName.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                shouldDelete = true;
            }
            // 删除 .backup 文件
            else if (fileName.Contains(".backup", System.StringComparison.OrdinalIgnoreCase))
            {
                shouldDelete = true;
            }

            if (shouldDelete)
            {
                bool deleted = SteamRemoteStorage.FileDelete(fileName);
                if (deleted)
                {
                    Debug.Log($"[SteamCloudCleanup] 已删除: {fileName} ({fileSize} 字节)");
                    deletedCount++;
                }
                else
                {
                    Debug.LogWarning($"[SteamCloudCleanup] 删除失败: {fileName}");
                }
            }
        }

        Debug.Log($"[SteamCloudCleanup] 清理完成，已删除 {deletedCount} 个文件");
#endif
    }

    /// <summary>
    /// 列出所有云端文件（用于调试）
    /// </summary>
    public static void ListCloudFiles()
    {
#if UNITY_EDITOR
        Debug.LogWarning("[SteamCloudCleanup] 此功能只能在打包后的游戏中运行");
        return;
#else
        if (!SteamAPI.IsSteamRunning())
        {
            Debug.LogError("[SteamCloudCleanup] Steam 未运行");
            return;
        }

        int fileCount = SteamRemoteStorage.GetFileCount();
        Debug.Log($"[SteamCloudCleanup] 云端共有 {fileCount} 个文件:");

        for (int i = 0; i < fileCount; i++)
        {
            string fileName;
            int fileSize;

            fileName = SteamRemoteStorage.GetFileNameAndSize(i, out fileSize);

            if (!string.IsNullOrEmpty(fileName))
            {
                Debug.Log($"  [{i}] {fileName} ({fileSize} 字节)");
            }
        }
#endif
    }
}
#endif
