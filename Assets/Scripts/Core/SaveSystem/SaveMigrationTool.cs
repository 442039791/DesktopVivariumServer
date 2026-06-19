using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 存档迁移工具 - 用于将旧格式存档迁移到新的ServerPackage格式
///
/// 新存档格式(v2):
/// - ServerPackage.dat: 账号级别数据(SaveData, GatherSaveData, 语言设置)
/// - {ID}_BioTankPackage.gz: 每个生态箱的独立数据(地形 + 客户端设置)
///
/// 旧存档格式(v1):
/// - ServerPackage.dat + ClientPackage.dat + {ID}_LandSaveData.gz
///
/// 更旧的格式:
/// - SavePackage.dat 或 SaveData.txt, SettingData.txt, GatherSaveData.txt
/// - {ID}_BioTankSaveData.txt
/// - {ID}_BioTankSettingSaveData.txt
/// - {ID}_LandSaveData.txt
/// </summary>
public static class SaveMigrationTool
{
    public const string SERVER_PACKAGE_NAME = "ServerPackage.dat";
    public const string BIOTANK_PACKAGE_SUFFIX = "_BioTankPackage.gz"; // 新格式: {ID}_BioTankPackage.gz

    // 以下常量仅用于旧存档迁移，迁移完成后可删除
    public const string CLIENT_PACKAGE_NAME = "ClientPackage.dat";
    private const string LEGACY_PACKAGE_NAME = "SavePackage.dat";

    /// <summary>
    /// 执行存档迁移 - 将当前Save目录下的旧存档迁移到新格式
    /// </summary>
    /// <param name="saveDirectory">存档目录路径</param>
    /// <returns>是否成功迁移</returns>
    public static bool MigrateToNewFormat(string saveDirectory)
    {
        if (!Directory.Exists(saveDirectory))
        {
            Debug.LogWarning($"[SaveMigrationTool] 存档目录不存在: {saveDirectory}");
            return false;
        }

        string serverPackagePath = Path.Combine(saveDirectory, SERVER_PACKAGE_NAME);
        string clientPackagePath = Path.Combine(saveDirectory, CLIENT_PACKAGE_NAME);

        // 如果新格式已存在,跳过迁移
        if (File.Exists(serverPackagePath) && File.Exists(clientPackagePath))
        {
            Debug.Log("[SaveMigrationTool] 新格式存档已存在,跳过迁移");
            return true;
        }

        Debug.Log($"[SaveMigrationTool] 开始迁移存档: {saveDirectory}");

        try
        {
            // 1. 收集旧数据
            SaveData saveData = null;
            GatherSaveData gatherSaveData = null;
            SettingSaveData settingSaveData = null;

            // 尝试从旧的SavePackage.dat加载
            string legacyPackagePath = Path.Combine(saveDirectory, LEGACY_PACKAGE_NAME);
            if (File.Exists(legacyPackagePath))
            {
                var legacyPackage = LoadLegacyPackage(legacyPackagePath);
                if (legacyPackage != null)
                {
                    saveData = legacyPackage.mainSaveData;
                    gatherSaveData = legacyPackage.gatherSaveData;
                    settingSaveData = legacyPackage.settingSaveData;
                }
            }

            // 如果SavePackage.dat不存在或加载失败,尝试从单独文件加载
            if (saveData == null)
            {
                saveData = LoadFromFile<SaveData>(saveDirectory, SysDefine.DefaultSaveDataName);
            }
            if (gatherSaveData == null)
            {
                gatherSaveData = LoadFromFile<GatherSaveData>(saveDirectory, SysDefine.DefaultGatherSaveDataName);
            }
            if (settingSaveData == null)
            {
                settingSaveData = LoadFromFile<SettingSaveData>(saveDirectory, SysDefine.DefaultSettingDataName);
            }

            // 从单独的BioTank文件加载BioTank状态
            if (saveData == null)
            {
                saveData = new SaveData();
            }
            if (saveData.BioTankStateDatas == null || saveData.BioTankStateDatas.Length == 0)
            {
                var bioTankStates = LoadLegacyBioTankStates(saveDirectory);
                if (bioTankStates.Count > 0)
                {
                    saveData.BioTankStateDatas = bioTankStates.ToArray();
                }
            }

            // 2. 创建ServerPackage
            var serverPackage = new ServerPackage
            {
                mainSaveData = saveData,
                gatherSaveData = gatherSaveData,
                serverSettingData = new ServerSettingData
                {
                    language = settingSaveData?.language ?? "English"
                }
            };

            // 3. 创建ClientPackage
            var bioTankWindows = new List<BioTankWindowData>();
            if (settingSaveData?.BioTankSaveDatas != null)
            {
                foreach (var btSave in settingSaveData.BioTankSaveDatas)
                {
                    bioTankWindows.Add(new BioTankWindowData
                    {
                        id = btSave.Id,
                        position = btSave.Position,
                        size = btSave.Size
                    });
                }
            }

            var bioTankSettings = LoadLegacyBioTankSettings(saveDirectory);

            var clientPackage = new ClientPackage
            {
                clientSettingData = new ClientSettingData
                {
                    windowPos = settingSaveData?.WinowPos ?? Vector2Int.zero,
                    windowScale = settingSaveData?.WinowScale ?? 1.0f,
                    bioTankWindows = bioTankWindows.ToArray()
                },
                bioTankSettings = bioTankSettings.ToArray()
            };

            // 4. 保存新格式
            SaveCompression.SaveCompressed(serverPackagePath, JsonUtility.ToJson(serverPackage));
            SaveCompression.SaveCompressed(clientPackagePath, JsonUtility.ToJson(clientPackage));

            // 5. 压缩地形数据
            CompressLandSaveData(saveDirectory);

            // 6. 删除旧文件
            DeleteLegacyFiles(saveDirectory);

            Debug.Log($"[SaveMigrationTool] 存档迁移完成");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveMigrationTool] 迁移失败: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 加载旧的SavePackage
    /// </summary>
    private static SavePackage LoadLegacyPackage(string path)
    {
        try
        {
            string json = SaveCompression.LoadSmart(path);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonUtility.FromJson<SavePackage>(json);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveMigrationTool] 加载旧SavePackage失败: {e.Message}");
        }
        return null;
    }

    /// <summary>
    /// 从文件加载数据
    /// </summary>
    private static T LoadFromFile<T>(string directory, string fileName) where T : class
    {
        string path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            string json = SaveCompression.LoadSmart(path);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonUtility.FromJson<T>(json);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveMigrationTool] 加载文件失败: {path}, {e.Message}");
        }
        return null;
    }

    /// <summary>
    /// 加载旧的BioTank状态数据
    /// </summary>
    private static List<BioTankStateData> LoadLegacyBioTankStates(string directory)
    {
        var result = new List<BioTankStateData>();

        try
        {
            var files = Directory.GetFiles(directory, "*_BioTankSaveData.txt", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var data = JsonUtility.FromJson<BioTankStateData>(json);
                    if (data != null)
                    {
                        result.Add(data);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SaveMigrationTool] 读取BioTank状态失败: {file}, {e.Message}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveMigrationTool] 扫描BioTank状态文件失败: {e.Message}");
        }

        return result;
    }

    /// <summary>
    /// 加载旧的BioTank设置数据
    /// </summary>
    private static List<BioTankClientSetting> LoadLegacyBioTankSettings(string directory)
    {
        var result = new List<BioTankClientSetting>();

        try
        {
            var files = Directory.GetFiles(directory, "*_BioTankSettingSaveData.txt", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var legacyData = JsonUtility.FromJson<LegacyBioTankSettingData>(json);
                    if (legacyData != null)
                    {
                        result.Add(new BioTankClientSetting
                        {
                            id = legacyData.ID,
                            cameraPos = legacyData.CameraPos != null ? new CameraPositionData
                            {
                                angleX = legacyData.CameraPos.angleX,
                                angleY = legacyData.CameraPos.angleY,
                                radius = legacyData.CameraPos.radius,
                                targetPos = legacyData.CameraPos.TargetPos
                            } : null,
                            windowPos = legacyData.WindowPos != null ? new WindowPositionData
                            {
                                position = legacyData.WindowPos.Position,
                                width = legacyData.WindowPos.Width,
                                height = legacyData.WindowPos.High
                            } : null
                        });
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SaveMigrationTool] 读取BioTank设置失败: {file}, {e.Message}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveMigrationTool] 扫描BioTank设置文件失败: {e.Message}");
        }

        return result;
    }

    /// <summary>
    /// 压缩地形数据文件
    /// </summary>
    private static void CompressLandSaveData(string directory)
    {
        try
        {
            var landFiles = Directory.GetFiles(directory, "*_LandSaveData.txt", SearchOption.TopDirectoryOnly);
            foreach (var file in landFiles)
            {
                try
                {
                    string content = File.ReadAllText(file);
                    string compressedPath = file.Replace(".txt", ".gz");

                    SaveCompression.SaveCompressed(compressedPath, content);
                    File.Delete(file);
                    Debug.Log($"[SaveMigrationTool] 已压缩地形数据: {Path.GetFileName(file)} -> {Path.GetFileName(compressedPath)}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SaveMigrationTool] 压缩地形数据失败: {file}, {e.Message}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveMigrationTool] 扫描地形文件失败: {e.Message}");
        }
    }

    /// <summary>
    /// 删除旧格式文件
    /// </summary>
    private static void DeleteLegacyFiles(string directory)
    {
        string[] legacyTargets =
        {
            Path.Combine(directory, LEGACY_PACKAGE_NAME),
            Path.Combine(directory, SysDefine.DefaultSaveDataName),
            Path.Combine(directory, SysDefine.DefaultSettingDataName),
            Path.Combine(directory, SysDefine.DefaultGatherSaveDataName)
        };

        foreach (var file in legacyTargets)
        {
            TryDeleteFile(file);
        }

        // 删除旧的BioTank存档文件
        TryDeleteFilesByPattern(directory, "*_BioTankSaveData.txt");
        TryDeleteFilesByPattern(directory, "*_BioTankSettingSaveData.txt");
    }

    private static void TryDeleteFilesByPattern(string directory, string pattern)
    {
        try
        {
            var files = Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                TryDeleteFile(file);
            }
        }
        catch { }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveMigrationTool] 删除旧文件: {Path.GetFileName(path)}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveMigrationTool] 删除文件失败: {path}, {e.Message}");
        }
    }

    /// <summary>
    /// 检查是否需要迁移
    /// </summary>
    public static bool NeedsMigration(string saveDirectory)
    {
        if (!Directory.Exists(saveDirectory))
        {
            return false;
        }

        string serverPackagePath = Path.Combine(saveDirectory, SERVER_PACKAGE_NAME);
        string clientPackagePath = Path.Combine(saveDirectory, CLIENT_PACKAGE_NAME);

        // 如果新格式已存在,不需要迁移
        if (File.Exists(serverPackagePath) && File.Exists(clientPackagePath))
        {
            return false;
        }

        // 检查是否存在旧格式文件
        if (File.Exists(Path.Combine(saveDirectory, LEGACY_PACKAGE_NAME)) ||
            File.Exists(Path.Combine(saveDirectory, SysDefine.DefaultSaveDataName)) ||
            File.Exists(Path.Combine(saveDirectory, SysDefine.DefaultSettingDataName)))
        {
            return true;
        }

        // 检查是否存在旧的BioTank文件
        try
        {
            var files = Directory.GetFiles(saveDirectory, "*_BioTankSaveData.txt", SearchOption.TopDirectoryOnly);
            if (files != null && files.Length > 0)
            {
                return true;
            }
        }
        catch { }

        return false;
    }
}

/// <summary>
/// 旧版BioTank设置数据 - 用于迁移
/// </summary>
[System.Serializable]
public class LegacyBioTankSettingData
{
    public int ID;
    public LegacyCameraPosOutput CameraPos;
    public LegacyWindowPosOutput WindowPos;
}

[System.Serializable]
public class LegacyCameraPosOutput
{
    public float angleX;
    public float angleY;
    public float radius;
    public Vector3 TargetPos;
}

[System.Serializable]
public class LegacyWindowPosOutput
{
    public Vector2Int Position;
    public int Width;
    public int High;
}
