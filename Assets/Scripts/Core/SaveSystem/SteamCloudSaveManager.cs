#if !DISABLESTEAMWORKS && STEAMWORKSNET
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Steamworks;

/// <summary>
/// Steam 云存档管理器
/// 负责将整个 Save 目录同步到 Steam Cloud
/// </summary>
public class SteamCloudSaveManager
{
    // Steam Cloud 文件名定义
    private const string CLOUD_MANIFEST = "SaveFileManifest.json";
    private const string CLOUD_MANIFEST_BACKUP = "SaveFileManifest.backup.json";

    // 本地存档目录
    private readonly string _localSaveDirectory;

    // 静态标志：追踪 Steam 是否真正初始化成功
    private static bool _steamInitialized = false;

    /// <summary>
    /// 设置 Steam 初始化状态（应在 SteamAPI.Init() 成功后调用）
    /// </summary>
    public static void SetSteamInitialized(bool initialized)
    {
        _steamInitialized = initialized;
    }

    /// <summary>
    /// 获取 Steam 初始化状态
    /// </summary>
    public static bool GetSteamInitialized()
    {
        return _steamInitialized;
    }

    // Steam 是否已初始化（使用静态标志而非 SteamAPI.IsSteamRunning()）
    private bool IsSteamInitialized => _steamInitialized;

    // 是否启用 Steam 云存档
    public bool IsCloudEnabled
    {
        get
        {
#if UNITY_EDITOR
            // 编辑器模式下禁用 Steam Cloud，避免污染云端存档
            return false;
#else
            return IsSteamInitialized && IsCloudEnabledForAccount && IsCloudEnabledForApp;
#endif
        }
    }

    // 是否为账户启用云存档
    public bool IsCloudEnabledForAccount
    {
        get
        {
            if (!IsSteamInitialized) return false;
            return SteamRemoteStorage.IsCloudEnabledForAccount();
        }
    }

    // 是否为应用启用云存档
    public bool IsCloudEnabledForApp
    {
        get
        {
            if (!IsSteamInitialized) return false;
            return SteamRemoteStorage.IsCloudEnabledForApp();
        }
        set
        {
            if (!IsSteamInitialized) return;
            SteamRemoteStorage.SetCloudEnabledForApp(value);
        }
    }

    public SteamCloudSaveManager(string localSaveDirectory)
    {
        _localSaveDirectory = localSaveDirectory;

#if !UNITY_EDITOR
        // 非编辑器模式下，启动时自动清理云端的垃圾文件
        if (IsSteamInitialized && IsCloudEnabledForAccount)
        {
            CleanupCloudGarbageFiles();
        }
#endif
    }

    #region 上传存档到云端

    /// <summary>
    /// 上传整个 Save 目录的所有文件到 Steam Cloud
    /// </summary>
    /// <param name="createBackup">是否在上传前创建云端备份</param>
    /// <returns>是否上传成功</returns>
    public bool UploadAllSavesToCloud(bool createBackup = true)
    {
        if (!IsCloudEnabled)
        {
            Debug.LogWarning("[SteamCloudSave] Steam 云存档未启用");
            return false;
        }

        try
        {
            // 创建 manifest 备份
            if (createBackup && SteamRemoteStorage.FileExists(CLOUD_MANIFEST))
            {
                CreateCloudBackup(CLOUD_MANIFEST, CLOUD_MANIFEST_BACKUP);
            }

            // 获取本地所有存档文件
            if (!Directory.Exists(_localSaveDirectory))
            {
                Debug.LogWarning($"[SteamCloudSave] 本地存档目录不存在: {_localSaveDirectory}");
                return false;
            }

            string[] allFiles = Directory.GetFiles(_localSaveDirectory);

            // 过滤掉不需要上传的文件(.meta, .backup, .tmp 等)
            var localFiles = allFiles.Where(f => ShouldUploadFile(Path.GetFileName(f))).ToArray();

            if (localFiles.Length == 0)
            {
                Debug.LogWarning("[SteamCloudSave] 本地存档目录为空或没有需要上传的文件");
                return false;
            }

            Debug.Log($"[SteamCloudSave] 准备上传 {localFiles.Length} 个文件 (已过滤 {allFiles.Length - localFiles.Length} 个不需要的文件)");

            // 创建文件清单
            var manifest = new SaveFileManifest
            {
                files = new SaveFileEntry[localFiles.Length],
                lastModified = DateTime.UtcNow
            };

            int successCount = 0;

            // 上传每个文件
            for (int i = 0; i < localFiles.Length; i++)
            {
                string localPath = localFiles[i];
                string fileName = Path.GetFileName(localPath);

                try
                {
                    byte[] data = File.ReadAllBytes(localPath);
                    bool success = SteamRemoteStorage.FileWrite(fileName, data, data.Length);

                    if (success)
                    {
                        successCount++;
                        manifest.files[i] = new SaveFileEntry
                        {
                            fileName = fileName,
                            fileSize = data.Length,
                            timestamp = File.GetLastWriteTimeUtc(localPath)
                        };
                        Debug.Log($"[SteamCloudSave] {fileName} 已上传 ({data.Length} 字节)");
                    }
                    else
                    {
                        Debug.LogError($"[SteamCloudSave] {fileName} 上传失败");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SteamCloudSave] 上传 {fileName} 异常: {e.Message}");
                }
            }

            // 上传清单文件
            string manifestJson = JsonUtility.ToJson(manifest, true);
            byte[] manifestData = System.Text.Encoding.UTF8.GetBytes(manifestJson);
            bool manifestSuccess = SteamRemoteStorage.FileWrite(CLOUD_MANIFEST, manifestData, manifestData.Length);

            if (manifestSuccess)
            {
                Debug.Log($"[SteamCloudSave] 清单文件已上传，共 {successCount}/{localFiles.Length} 个文件成功");
            }
            else
            {
                Debug.LogError("[SteamCloudSave] 清单文件上传失败");
            }

            return successCount > 0 && manifestSuccess;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 上传存档到 Steam Cloud 异常: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 异步上传整个 Save 目录的所有文件到 Steam Cloud
    /// </summary>
    public void UploadAllSavesToCloudAsync(bool createBackup, Action<bool> onComplete)
    {
        if (!IsCloudEnabled)
        {
            Debug.LogWarning("[SteamCloudSave] Steam 云存档未启用");
            onComplete?.Invoke(false);
            return;
        }

        try
        {
            // 创建 manifest 备份
            if (createBackup && SteamRemoteStorage.FileExists(CLOUD_MANIFEST))
            {
                CreateCloudBackup(CLOUD_MANIFEST, CLOUD_MANIFEST_BACKUP);
            }

            // 获取本地所有存档文件
            if (!Directory.Exists(_localSaveDirectory))
            {
                Debug.LogWarning($"[SteamCloudSave] 本地存档目录不存在: {_localSaveDirectory}");
                onComplete?.Invoke(false);
                return;
            }

            string[] allFiles = Directory.GetFiles(_localSaveDirectory);

            // 过滤掉不需要上传的文件(.meta, .backup, .tmp 等)
            var localFiles = allFiles.Where(f => ShouldUploadFile(Path.GetFileName(f))).ToArray();

            if (localFiles.Length == 0)
            {
                Debug.LogWarning("[SteamCloudSave] 本地存档目录为空或没有需要上传的文件");
                onComplete?.Invoke(false);
                return;
            }

            Debug.Log($"[SteamCloudSave] 准备异步上传 {localFiles.Length} 个文件 (已过滤 {allFiles.Length - localFiles.Length} 个不需要的文件)");

            int uploadCount = 0;
            int successCount = 0;
            int totalFiles = localFiles.Length;
            var manifest = new SaveFileManifest
            {
                files = new SaveFileEntry[localFiles.Length],
                lastModified = DateTime.UtcNow
            };

            // 异步上传每个文件
            for (int i = 0; i < localFiles.Length; i++)
            {
                string localPath = localFiles[i];
                string fileName = Path.GetFileName(localPath);
                int index = i;

                try
                {
                    byte[] data = File.ReadAllBytes(localPath);
                    var callHandle = SteamRemoteStorage.FileWriteAsync(fileName, data, (uint)data.Length);
                    var fileTimestamp = File.GetLastWriteTimeUtc(localPath);

                    var callResult = new CallResult<RemoteStorageFileWriteAsyncComplete_t>((result, error) =>
                    {
                        uploadCount++;

                        if (!error && result.m_eResult == EResult.k_EResultOK)
                        {
                            successCount++;
                            manifest.files[index] = new SaveFileEntry
                            {
                                fileName = fileName,
                                fileSize = data.Length,
                                timestamp = fileTimestamp
                            };
                            Debug.Log($"[SteamCloudSave] {fileName} 已异步上传 ({data.Length} 字节)");
                        }
                        else
                        {
                            Debug.LogError($"[SteamCloudSave] {fileName} 异步上传失败: {result.m_eResult}");
                        }

                        // 所有文件上传完成后，上传清单
                        if (uploadCount == totalFiles)
                        {
                            UploadManifestAsync(manifest, successCount, totalFiles, onComplete);
                        }
                    });

                    callResult.Set(callHandle);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SteamCloudSave] 上传 {fileName} 异常: {e.Message}");
                    uploadCount++;

                    if (uploadCount == totalFiles)
                    {
                        UploadManifestAsync(manifest, successCount, totalFiles, onComplete);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 异步上传异常: {e.Message}");
            onComplete?.Invoke(false);
        }
    }

    /// <summary>
    /// 异步上传清单文件
    /// </summary>
    private void UploadManifestAsync(SaveFileManifest manifest, int successCount, int totalFiles, Action<bool> onComplete)
    {
        try
        {
            string manifestJson = JsonUtility.ToJson(manifest, true);
            byte[] manifestData = System.Text.Encoding.UTF8.GetBytes(manifestJson);
            var callHandle = SteamRemoteStorage.FileWriteAsync(CLOUD_MANIFEST, manifestData, (uint)manifestData.Length);

            var callResult = new CallResult<RemoteStorageFileWriteAsyncComplete_t>((result, error) =>
            {
                if (!error && result.m_eResult == EResult.k_EResultOK)
                {
                    Debug.Log($"[SteamCloudSave] 清单文件已上传，共 {successCount}/{totalFiles} 个文件成功");
                    onComplete?.Invoke(successCount > 0);
                }
                else
                {
                    Debug.LogError($"[SteamCloudSave] 清单文件上传失败: {result.m_eResult}");
                    onComplete?.Invoke(false);
                }
            });

            callResult.Set(callHandle);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 上传清单文件异常: {e.Message}");
            onComplete?.Invoke(false);
        }
    }

    #endregion

    #region 从云端下载存档

    /// <summary>
    /// 从 Steam Cloud 下载整个 Save 目录的所有文件到本地
    /// </summary>
    /// <param name="createLocalBackup">是否在下载前创建本地备份</param>
    /// <returns>是否下载成功</returns>
    public bool DownloadAllSavesFromCloud(bool createLocalBackup = true)
    {
        if (!IsCloudEnabled)
        {
            Debug.LogWarning("[SteamCloudSave] Steam 云存档未启用");
            return false;
        }

        try
        {
            // 检查云端清单文件是否存在
            if (!SteamRemoteStorage.FileExists(CLOUD_MANIFEST))
            {
                Debug.LogWarning("[SteamCloudSave] 云端清单文件不存在");
                return false;
            }

            // 下载并解析清单文件
            int manifestSize = SteamRemoteStorage.GetFileSize(CLOUD_MANIFEST);
            byte[] manifestData = new byte[manifestSize];
            int bytesRead = SteamRemoteStorage.FileRead(CLOUD_MANIFEST, manifestData, manifestSize);

            if (bytesRead != manifestSize)
            {
                Debug.LogError("[SteamCloudSave] 清单文件读取失败");
                return false;
            }

            string manifestJson = System.Text.Encoding.UTF8.GetString(manifestData);
            SaveFileManifest manifest = JsonUtility.FromJson<SaveFileManifest>(manifestJson);

            if (manifest == null || manifest.files == null || manifest.files.Length == 0)
            {
                Debug.LogError("[SteamCloudSave] 清单文件解析失败或为空");
                return false;
            }

            // 确保本地目录存在
            if (!Directory.Exists(_localSaveDirectory))
            {
                Directory.CreateDirectory(_localSaveDirectory);
            }

            int successCount = 0;

            // 下载清单中列出的每个文件
            foreach (var fileEntry in manifest.files)
            {
                if (fileEntry == null || string.IsNullOrEmpty(fileEntry.fileName))
                    continue;

                // 跳过不应该下载的文件（.meta, .backup 等）
                if (!ShouldUploadFile(fileEntry.fileName))
                {
                    Debug.Log($"[SteamCloudSave] 跳过不需要的文件: {fileEntry.fileName}");
                    continue;
                }

                try
                {
                    if (!SteamRemoteStorage.FileExists(fileEntry.fileName))
                    {
                        Debug.LogWarning($"[SteamCloudSave] 云端文件不存在: {fileEntry.fileName}");
                        continue;
                    }

                    int fileSize = SteamRemoteStorage.GetFileSize(fileEntry.fileName);
                    byte[] fileData = new byte[fileSize];
                    int fileBytesRead = SteamRemoteStorage.FileRead(fileEntry.fileName, fileData, fileSize);

                    if (fileBytesRead != fileSize)
                    {
                        Debug.LogError($"[SteamCloudSave] {fileEntry.fileName} 读取字节数不匹配");
                        continue;
                    }

                    // 创建本地备份
                    string localPath = Path.Combine(_localSaveDirectory, fileEntry.fileName);
                    if (createLocalBackup && File.Exists(localPath))
                    {
                        CreateLocalBackup(localPath);
                    }

                    // 写入本地文件
                    File.WriteAllBytes(localPath, fileData);
                    successCount++;
                    Debug.Log($"[SteamCloudSave] {fileEntry.fileName} 已下载到本地 ({fileData.Length} 字节)");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SteamCloudSave] 下载 {fileEntry.fileName} 异常: {e.Message}");
                }
            }

            Debug.Log($"[SteamCloudSave] 存档下载完成: {successCount}/{manifest.files.Length} 个文件成功");
            return successCount > 0;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 从 Steam Cloud 下载存档异常: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 异步从 Steam Cloud 下载整个 Save 目录的所有文件
    /// </summary>
    public void DownloadAllSavesFromCloudAsync(bool createLocalBackup, Action<bool> onComplete)
    {
        if (!IsCloudEnabled)
        {
            Debug.LogWarning("[SteamCloudSave] Steam 云存档未启用");
            onComplete?.Invoke(false);
            return;
        }

        try
        {
            // 检查云端清单文件是否存在
            if (!SteamRemoteStorage.FileExists(CLOUD_MANIFEST))
            {
                Debug.LogWarning("[SteamCloudSave] 云端清单文件不存在");
                onComplete?.Invoke(false);
                return;
            }

            // 异步下载清单文件
            int manifestSize = SteamRemoteStorage.GetFileSize(CLOUD_MANIFEST);
            var callHandle = SteamRemoteStorage.FileReadAsync(CLOUD_MANIFEST, 0, (uint)manifestSize);

            var callResult = new CallResult<RemoteStorageFileReadAsyncComplete_t>((result, error) =>
            {
                if (error || result.m_eResult != EResult.k_EResultOK)
                {
                    Debug.LogError($"[SteamCloudSave] 清单文件异步下载失败: {result.m_eResult}");
                    onComplete?.Invoke(false);
                    return;
                }

                try
                {
                    // 读取清单数据
                    byte[] manifestData = new byte[result.m_cubRead];
                    bool success = SteamRemoteStorage.FileReadAsyncComplete(result.m_hFileReadAsync, manifestData, (uint)result.m_cubRead);

                    if (!success)
                    {
                        Debug.LogError("[SteamCloudSave] 清单文件读取失败");
                        onComplete?.Invoke(false);
                        return;
                    }

                    string manifestJson = System.Text.Encoding.UTF8.GetString(manifestData);
                    SaveFileManifest manifest = JsonUtility.FromJson<SaveFileManifest>(manifestJson);

                    if (manifest == null || manifest.files == null || manifest.files.Length == 0)
                    {
                        Debug.LogError("[SteamCloudSave] 清单文件解析失败或为空");
                        onComplete?.Invoke(false);
                        return;
                    }

                    // 确保本地目录存在
                    if (!Directory.Exists(_localSaveDirectory))
                    {
                        Directory.CreateDirectory(_localSaveDirectory);
                    }

                    // 异步下载所有文件
                    DownloadFilesAsync(manifest, createLocalBackup, onComplete);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SteamCloudSave] 处理清单文件异常: {e.Message}");
                    onComplete?.Invoke(false);
                }
            });

            callResult.Set(callHandle);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 异步下载异常: {e.Message}");
            onComplete?.Invoke(false);
        }
    }

    /// <summary>
    /// 异步下载清单中的所有文件
    /// </summary>
    private void DownloadFilesAsync(SaveFileManifest manifest, bool createLocalBackup, Action<bool> onComplete)
    {
        int downloadCount = 0;
        int successCount = 0;
        int totalFiles = manifest.files.Length;

        foreach (var fileEntry in manifest.files)
        {
            if (fileEntry == null || string.IsNullOrEmpty(fileEntry.fileName))
            {
                downloadCount++;
                if (downloadCount == totalFiles)
                {
                    Debug.Log($"[SteamCloudSave] 存档下载完成: {successCount}/{totalFiles} 个文件成功");
                    onComplete?.Invoke(successCount > 0);
                }
                continue;
            }

            // 跳过不应该下载的文件（.meta, .backup 等）
            if (!ShouldUploadFile(fileEntry.fileName))
            {
                Debug.Log($"[SteamCloudSave] 跳过不需要的文件: {fileEntry.fileName}");
                downloadCount++;
                if (downloadCount == totalFiles)
                {
                    Debug.Log($"[SteamCloudSave] 存档下载完成: {successCount}/{totalFiles} 个文件成功");
                    onComplete?.Invoke(successCount > 0);
                }
                continue;
            }

            try
            {
                if (!SteamRemoteStorage.FileExists(fileEntry.fileName))
                {
                    Debug.LogWarning($"[SteamCloudSave] 云端文件不存在: {fileEntry.fileName}");
                    downloadCount++;
                    if (downloadCount == totalFiles)
                    {
                        Debug.Log($"[SteamCloudSave] 存档下载完成: {successCount}/{totalFiles} 个文件成功");
                        onComplete?.Invoke(successCount > 0);
                    }
                    continue;
                }

                int fileSize = SteamRemoteStorage.GetFileSize(fileEntry.fileName);
                var callHandle = SteamRemoteStorage.FileReadAsync(fileEntry.fileName, 0, (uint)fileSize);
                string fileName = fileEntry.fileName;

                var callResult = new CallResult<RemoteStorageFileReadAsyncComplete_t>((result, error) =>
                {
                    downloadCount++;

                    if (error || result.m_eResult != EResult.k_EResultOK)
                    {
                        Debug.LogError($"[SteamCloudSave] {fileName} 异步下载失败: {result.m_eResult}");
                    }
                    else
                    {
                        try
                        {
                            byte[] fileData = new byte[result.m_cubRead];
                            bool fileSuccess = SteamRemoteStorage.FileReadAsyncComplete(result.m_hFileReadAsync, fileData, (uint)result.m_cubRead);

                            if (!fileSuccess)
                            {
                                Debug.LogError($"[SteamCloudSave] {fileName} 读取失败");
                            }
                            else
                            {
                                // 创建本地备份
                                string localPath = Path.Combine(_localSaveDirectory, fileName);
                                if (createLocalBackup && File.Exists(localPath))
                                {
                                    CreateLocalBackup(localPath);
                                }

                                // 写入本地文件
                                File.WriteAllBytes(localPath, fileData);
                                successCount++;
                                Debug.Log($"[SteamCloudSave] {fileName} 已异步下载到本地 ({fileData.Length} 字节)");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[SteamCloudSave] 写入 {fileName} 到本地失败: {e.Message}");
                        }
                    }

                    if (downloadCount == totalFiles)
                    {
                        Debug.Log($"[SteamCloudSave] 存档下载完成: {successCount}/{totalFiles} 个文件成功");
                        onComplete?.Invoke(successCount > 0);
                    }
                });

                callResult.Set(callHandle);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SteamCloudSave] 下载 {fileEntry.fileName} 异常: {e.Message}");
                downloadCount++;

                if (downloadCount == totalFiles)
                {
                    Debug.Log($"[SteamCloudSave] 存档下载完成: {successCount}/{totalFiles} 个文件成功");
                    onComplete?.Invoke(successCount > 0);
                }
            }
        }
    }

    #endregion

    #region 同步策略

    /// <summary>
    /// 智能同步：比较本地和云端清单的时间戳，使用最新的存档
    /// </summary>
    /// <returns>同步结果：1=上传到云端, -1=从云端下载, 0=无需同步或失败</returns>
    public int SyncWithCloud()
    {
        if (!IsCloudEnabled)
        {
            Debug.LogWarning("[SteamCloudSave] Steam 云存档未启用");
            return 0;
        }

        try
        {
            // 检查本地是否有有效的存档文件(排除 .meta 和 .backup 文件)
            bool localExists = false;
            if (Directory.Exists(_localSaveDirectory))
            {
                var validFiles = Directory.GetFiles(_localSaveDirectory)
                    .Where(f => ShouldUploadFile(Path.GetFileName(f)))
                    .ToArray();
                localExists = validFiles.Length > 0;
            }

            bool cloudExists = SteamRemoteStorage.FileExists(CLOUD_MANIFEST);

            // 情况1: 本地和云端都没有 -> 无需同步
            if (!localExists && !cloudExists)
            {
                Debug.Log("[SteamCloudSave] 本地和云端都不存在存档");
                return 0;
            }

            // 情况2: 只有本地存档 -> 上传
            if (localExists && !cloudExists)
            {
                Debug.Log("[SteamCloudSave] 仅本地存在存档，上传到云端");
                return UploadAllSavesToCloud(false) ? 1 : 0;
            }

            // 情况3: 只有云端存档 -> 下载
            if (!localExists && cloudExists)
            {
                Debug.Log("[SteamCloudSave] 仅云端存在存档，下载到本地");
                return DownloadAllSavesFromCloud(false) ? -1 : 0;
            }

            // 情况4: 两边都有 -> 比较清单的时间戳
            long cloudTimestamp = SteamRemoteStorage.GetFileTimestamp(CLOUD_MANIFEST);
            DateTime cloudTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(cloudTimestamp);

            // 获取本地最新文件的时间戳(排除 .meta 和 .backup 文件)
            DateTime localTime = DateTime.MinValue;
            var localFiles = Directory.GetFiles(_localSaveDirectory)
                .Where(f => ShouldUploadFile(Path.GetFileName(f)))
                .ToArray();

            foreach (string filePath in localFiles)
            {
                DateTime fileTime = File.GetLastWriteTimeUtc(filePath);
                if (fileTime > localTime)
                {
                    localTime = fileTime;
                }
            }

            Debug.Log($"[SteamCloudSave] 本地最新文件: {localTime:yyyy-MM-dd HH:mm:ss}, 云端清单: {cloudTime:yyyy-MM-dd HH:mm:ss}");

            if (localTime > cloudTime.AddSeconds(1)) // 容忍1秒误差
            {
                Debug.Log("[SteamCloudSave] 本地存档较新，上传到云端");
                return UploadAllSavesToCloud(true) ? 1 : 0;
            }
            else if (cloudTime > localTime.AddSeconds(1))
            {
                Debug.Log("[SteamCloudSave] 云端存档较新，下载到本地");
                return DownloadAllSavesFromCloud(true) ? -1 : 0;
            }
            else
            {
                Debug.Log("[SteamCloudSave] 本地和云端存档时间相近，无需同步");
                return 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 同步异常: {e.Message}\n{e.StackTrace}");
            return 0;
        }
    }

    #endregion

    #region 备份管理

    /// <summary>
    /// 创建云端备份
    /// </summary>
    private bool CreateCloudBackup(string sourceFileName, string backupFileName)
    {
        try
        {
            if (!SteamRemoteStorage.FileExists(sourceFileName))
            {
                return false;
            }

            int fileSize = SteamRemoteStorage.GetFileSize(sourceFileName);
            byte[] currentData = new byte[fileSize];
            SteamRemoteStorage.FileRead(sourceFileName, currentData, fileSize);

            bool success = SteamRemoteStorage.FileWrite(backupFileName, currentData, currentData.Length);

            if (success)
            {
                Debug.Log($"[SteamCloudSave] 已创建云端备份: {backupFileName}");
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SteamCloudSave] 创建云端备份失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 创建本地备份
    /// </summary>
    private bool CreateLocalBackup(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            // 跳过 .meta 文件和已存在的 .backup 文件，避免创建嵌套备份
            string fileName = Path.GetFileName(filePath);
            if (fileName.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) ||
                fileName.Contains(".backup", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string backupPath = filePath + ".backup";
            File.Copy(filePath, backupPath, true);
            Debug.Log($"[SteamCloudSave] 已创建本地备份: {backupPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SteamCloudSave] 创建本地备份失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从云端备份恢复所有存档
    /// </summary>
    public bool RestoreAllFromCloudBackup()
    {
        // Steam 未初始化时直接返回
        if (!IsSteamInitialized)
        {
            Debug.LogWarning("[SteamCloudSave] Steam 未初始化，无法从云端备份恢复");
            return false;
        }

        if (!SteamRemoteStorage.FileExists(CLOUD_MANIFEST_BACKUP))
        {
            Debug.LogWarning("[SteamCloudSave] 云端备份清单不存在");
            return false;
        }

        bool success = RestoreFromCloudBackup(CLOUD_MANIFEST_BACKUP, CLOUD_MANIFEST);

        if (success)
        {
            Debug.Log("[SteamCloudSave] 清单已从云端备份恢复，现在下载所有文件");
            return DownloadAllSavesFromCloud(false);
        }

        return false;
    }

    /// <summary>
    /// 从云端备份恢复单个文件
    /// </summary>
    private bool RestoreFromCloudBackup(string backupFileName, string targetFileName)
    {
        try
        {
            if (!SteamRemoteStorage.FileExists(backupFileName))
            {
                Debug.LogWarning($"[SteamCloudSave] 云端备份不存在: {backupFileName}");
                return false;
            }

            int fileSize = SteamRemoteStorage.GetFileSize(backupFileName);
            byte[] backupData = new byte[fileSize];
            SteamRemoteStorage.FileRead(backupFileName, backupData, fileSize);

            bool success = SteamRemoteStorage.FileWrite(targetFileName, backupData, backupData.Length);

            if (success)
            {
                Debug.Log($"[SteamCloudSave] 已从云端备份恢复: {targetFileName}");
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 从云端备份恢复失败: {e.Message}");
            return false;
        }
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 判断文件是否应该上传到 Steam Cloud
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>是否应该上传</returns>
    private bool ShouldUploadFile(string fileName)
    {
        // 排除 Unity 的 .meta 文件
        if (fileName.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 排除备份文件
        if (fileName.Contains(".backup", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 排除临时文件和其他不需要同步的文件
        if (fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".temp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 清理云端的垃圾文件（.meta 和 .backup 文件）
    /// </summary>
    private void CleanupCloudGarbageFiles()
    {
        try
        {
            int fileCount = SteamRemoteStorage.GetFileCount();
            int deletedCount = 0;

            Debug.Log($"[SteamCloudSave] 开始清理云端垃圾文件，共 {fileCount} 个文件");

            // 从后往前遍历，避免删除后索引变化
            for (int i = fileCount - 1; i >= 0; i--)
            {
                string fileName = SteamRemoteStorage.GetFileNameAndSize(i, out int fileSize);

                if (string.IsNullOrEmpty(fileName))
                    continue;

                // 检查是否是垃圾文件
                bool shouldDelete = fileName.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) ||
                                   fileName.Contains(".backup", StringComparison.OrdinalIgnoreCase);

                if (shouldDelete)
                {
                    bool deleted = SteamRemoteStorage.FileDelete(fileName);
                    if (deleted)
                    {
                        Debug.Log($"[SteamCloudSave] 已从云端删除垃圾文件: {fileName} ({fileSize} 字节)");
                        deletedCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[SteamCloudSave] 删除云端文件失败: {fileName}");
                    }
                }
            }

            if (deletedCount > 0)
            {
                Debug.Log($"[SteamCloudSave] 云端垃圾文件清理完成，已删除 {deletedCount} 个文件");
            }
            else
            {
                Debug.Log("[SteamCloudSave] 云端没有垃圾文件需要清理");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SteamCloudSave] 清理云端垃圾文件时出错: {e.Message}");
        }
    }

    /// <summary>
    /// 获取云端存档信息
    /// </summary>
    public CloudSaveInfo GetCloudSaveInfo()
    {
        var info = new CloudSaveInfo
        {
            IsCloudEnabled = IsCloudEnabled,
            ManifestExists = false,
            BackupExists = false,
            FileCount = 0,
            TotalSize = 0
        };

        // Steam 未初始化时直接返回默认值
        if (!IsSteamInitialized)
        {
            return info;
        }

        info.ManifestExists = SteamRemoteStorage.FileExists(CLOUD_MANIFEST);
        info.BackupExists = SteamRemoteStorage.FileExists(CLOUD_MANIFEST_BACKUP);

        if (info.ManifestExists)
        {
            try
            {
                // 读取清单文件
                int manifestSize = SteamRemoteStorage.GetFileSize(CLOUD_MANIFEST);
                byte[] manifestData = new byte[manifestSize];
                SteamRemoteStorage.FileRead(CLOUD_MANIFEST, manifestData, manifestSize);

                string manifestJson = System.Text.Encoding.UTF8.GetString(manifestData);
                SaveFileManifest manifest = JsonUtility.FromJson<SaveFileManifest>(manifestJson);

                if (manifest != null && manifest.files != null)
                {
                    info.FileCount = manifest.files.Length;
                    info.LastModified = manifest.lastModified;

                    // 计算总大小
                    foreach (var fileEntry in manifest.files)
                    {
                        if (fileEntry != null && !string.IsNullOrEmpty(fileEntry.fileName))
                        {
                            info.TotalSize += fileEntry.fileSize;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SteamCloudSave] 读取云端清单信息失败: {e.Message}");
            }
        }

        return info;
    }

    /// <summary>
    /// 获取 Steam Cloud 配额信息
    /// </summary>
    public bool GetQuota(out ulong totalBytes, out ulong availableBytes)
    {
        // Steam 未初始化时返回 false
        if (!IsSteamInitialized)
        {
            totalBytes = 0;
            availableBytes = 0;
            return false;
        }
        return SteamRemoteStorage.GetQuota(out totalBytes, out availableBytes);
    }

    /// <summary>
    /// 删除所有云端存档（包括清单和所有文件）
    /// </summary>
    public bool DeleteAllCloudSaves()
    {
        // Steam 未初始化时直接返回
        if (!IsSteamInitialized)
        {
            Debug.LogWarning("[SteamCloudSave] Steam 未初始化，无法删除云端存档");
            return false;
        }

        try
        {
            int deletedCount = 0;

            // 尝试读取清单并删除列出的所有文件
            if (SteamRemoteStorage.FileExists(CLOUD_MANIFEST))
            {
                try
                {
                    int manifestSize = SteamRemoteStorage.GetFileSize(CLOUD_MANIFEST);
                    byte[] manifestData = new byte[manifestSize];
                    SteamRemoteStorage.FileRead(CLOUD_MANIFEST, manifestData, manifestSize);

                    string manifestJson = System.Text.Encoding.UTF8.GetString(manifestData);
                    SaveFileManifest manifest = JsonUtility.FromJson<SaveFileManifest>(manifestJson);

                    if (manifest != null && manifest.files != null)
                    {
                        foreach (var fileEntry in manifest.files)
                        {
                            if (fileEntry != null && !string.IsNullOrEmpty(fileEntry.fileName))
                            {
                                if (DeleteCloudFile(fileEntry.fileName))
                                    deletedCount++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SteamCloudSave] 读取清单失败，将尝试删除清单文件: {e.Message}");
                }
            }

            // 删除清单和备份文件
            bool manifestDeleted = DeleteCloudFile(CLOUD_MANIFEST);
            bool backupDeleted = DeleteCloudFile(CLOUD_MANIFEST_BACKUP);

            if (manifestDeleted) deletedCount++;
            if (backupDeleted) deletedCount++;

            Debug.Log($"[SteamCloudSave] 已删除 {deletedCount} 个云端文件");
            return deletedCount > 0;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 删除云端存档失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 删除云端单个文件
    /// </summary>
    private bool DeleteCloudFile(string fileName)
    {
        try
        {
            if (!SteamRemoteStorage.FileExists(fileName))
            {
                return true; // 文件不存在，视为成功
            }

            bool success = SteamRemoteStorage.FileDelete(fileName);
            if (success)
            {
                Debug.Log($"[SteamCloudSave] 已删除云端文件: {fileName}");
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SteamCloudSave] 删除云端文件失败: {fileName}, {e.Message}");
            return false;
        }
    }

    #endregion
}

/// <summary>
/// 云端存档信息
/// </summary>
[Serializable]
public struct CloudSaveInfo
{
    public bool IsCloudEnabled;
    public bool ManifestExists;
    public bool BackupExists;
    public int FileCount;
    public long TotalSize;
    public DateTime LastModified;

    public override string ToString()
    {
        var result = $"云存档: {(IsCloudEnabled ? "已启用" : "未启用")}\n";
        result += $"清单文件: {(ManifestExists ? "存在" : "不存在")}\n";
        result += $"备份文件: {(BackupExists ? "存在" : "不存在")}\n";
        result += $"文件数量: {FileCount}\n";
        result += $"总大小: {TotalSize / 1024.0:F2} KB\n";
        result += $"最后修改: {LastModified:yyyy-MM-dd HH:mm:ss}";
        return result;
    }
}

/// <summary>
/// 存档文件清单（用于追踪 Save 目录中的所有文件）
/// </summary>
[Serializable]
public class SaveFileManifest
{
    public SaveFileEntry[] files;
    public DateTime lastModified;
}

/// <summary>
/// 单个存档文件的条目信息
/// </summary>
[Serializable]
public class SaveFileEntry
{
    public string fileName;
    public long fileSize;
    public DateTime timestamp;
}
#endif
