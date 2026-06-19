using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// 路径管理器 - 统一管理所有游戏路径的获取、验证、探测和持久化
/// 服务器端版本，使用 UnitySingleton 基类
/// </summary>
public class PathManager : UnitySingleton<PathManager>
{
    /// <summary>
    /// 配置文件路径类型
    /// </summary>
    public enum PathType
    {
        SaveData = 0,   // 保存数据路径
        Config = 1,     // 配置路径
        BluePrint = 2,  // 蓝图路径
        SecondUnityExe = 3  // 另一个Unity可执行文件路径(服务器端使用)
    }

    /// <summary>
    /// 应用类型标识，用于区分路径探测逻辑
    /// </summary>
    public enum AppType
    {
        Client,  // 客户端 - 路径指向 Server 目录
        Server   // 服务器端 - 路径指向 Client 目录
    }

    private const string PATH_CONFIG_FILE = "UIWindowPath.txt";

    // 路径缓存,避免重复读取文件
    private string[] _cachedPaths = new string[4];

    // 标记是否已初始化
    private bool _isInitialized = false;

    /// <summary>
    /// 获取指定类型的路径,带完整错误处理
    /// </summary>
    /// <param name="type">路径类型</param>
    /// <returns>配置的路径,失败返回空字符串</returns>
    public string GetPath(PathType type)
    {
        int index = (int)type;

        // 如果已缓存,直接返回
        if (index < _cachedPaths.Length && !string.IsNullOrEmpty(_cachedPaths[index]))
            return _cachedPaths[index];

        // SecondUnityExe 不从文件读取，只能通过 SetPath 或 InitPaths 设置
        if (type == PathType.SecondUnityExe)
            return string.Empty;

        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, PATH_CONFIG_FILE);
            filePath = filePath.Replace(@"\", "/");

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[PathManager] 路径配置文件不存在: {filePath}");
                return string.Empty;
            }

            string[] lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);

            if (lines.Length <= index)
            {
                Debug.LogWarning($"[PathManager] 配置文件行数不足,需要至少{index + 1}行,实际{lines.Length}行");
                return string.Empty;
            }

            string path = lines[index].Trim();

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"[PathManager] 第{index}行路径为空");
                return string.Empty;
            }

            // 标准化路径分隔符
            _cachedPaths[index] = path.Replace(@"\", "/");

            return _cachedPaths[index];
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PathManager] 读取路径配置失败: {ex.Message}\n{ex.StackTrace}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 验证路径是否有效
    /// </summary>
    /// <param name="path">待验证的路径</param>
    /// <returns>路径是否有效</returns>
    public bool ValidatePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            string directory = Path.GetDirectoryName(path);
            return !string.IsNullOrEmpty(directory);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 清除路径缓存,下次调用GetPath时会重新读取
    /// </summary>
    public void ClearCache()
    {
        for (int i = 0; i < _cachedPaths.Length; i++)
        {
            _cachedPaths[i] = string.Empty;
        }
    }

    /// <summary>
    /// 手动设置路径(用于测试或运行时修改)
    /// </summary>
    /// <param name="type">路径类型</param>
    /// <param name="path">新路径</param>
    public void SetPath(PathType type, string path)
    {
        int index = (int)type;
        if (index < _cachedPaths.Length)
        {
            _cachedPaths[index] = path.Replace(@"\", "/");
        }
    }

    /// <summary>
    /// 初始化路径配置 - 在非编辑器模式下探测并设置路径
    /// 此方法替代原 GlobalSetting.OnInit() 中的路径逻辑
    /// </summary>
    /// <param name="appType">应用类型(客户端或服务器端)</param>
    /// <returns>是否初始化成功</returns>
    public bool InitPaths(AppType appType)
    {
#if UNITY_EDITOR
        return true;
#else
        if (_isInitialized)
        {
            Debug.LogWarning("[PathManager] 路径已初始化,跳过重复初始化");
            return true;
        }

        try
        {
            // 获取当前 EXE 路径信息
            string currentExePath = Process.GetCurrentProcess().MainModule.FileName;
            string currentDir = Path.GetDirectoryName(currentExePath);
            string parentDir = Directory.GetParent(currentDir).FullName;

            // 根据应用类型设置路径
            if (appType == AppType.Client)
            {
                InitClientPaths(parentDir);
            }
            else
            {
                InitServerPaths(parentDir);
            }

            _isInitialized = true;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PathManager] 路径初始化失败: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
#endif
    }

    /// <summary>
    /// 初始化客户端路径 - 指向 Server 目录
    /// </summary>
    private void InitClientPaths(string parentDir)
    {
        string serverDataPath = Path.Combine(parentDir, "Server", "DesktopVivarium_Data/StreamingAssets");

        SetPath(PathType.SaveData, Path.Combine(serverDataPath, "Save/SaveData.txt"));
        SetPath(PathType.Config, serverDataPath);
        SetPath(PathType.BluePrint, serverDataPath + "/");
    }

    /// <summary>
    /// 初始化服务器端路径 - 指向 Client 目录
    /// </summary>
    private void InitServerPaths(string parentDir)
    {
        string clientExePath = Path.Combine(parentDir, "Client", "DesktopVivarium(Wallpaper).exe");
        SetPath(PathType.SecondUnityExe, clientExePath);
    }

    /// <summary>
    /// 获取另一个Unity可执行文件路径(服务器端使用)
    /// </summary>
    public string GetSecondUnityExePath()
    {
        return GetPath(PathType.SecondUnityExe);
    }

    /// <summary>
    /// 检查路径是否已初始化
    /// </summary>
    public bool IsInitialized => _isInitialized;
}
