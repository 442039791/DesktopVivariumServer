using UnityEngine;
using Common;
using System.Collections.Generic;
using System.Collections;
#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Steamworks;
#endif

/// <summary>
/// 游戏主管理器 - 简化后仅负责协调各子系统
/// </summary>
public class GameManager : MonoSingleton<GameManager>
{
    [Header("是否以第一次空场景启动游戏")]
    public bool first = false;

    // 子系统
    private SaveSystem _saveSystem;
    private TimerSystem _timerSystem;
    private GameInitializer _gameInitializer;

    // 公开属性
    public SaveSystem SaveSystem => _saveSystem;

    // 游戏状态
    public bool FirstStartGame = false;
    public List<IStop> StopList = new List<IStop>();
    public bool LoadComplete = false;
    public bool BlockLoadComplete = false;

    // 游戏数据
    public Gather gather;

    // UI引用
    public UI_Title UI_Title;
    public UI_BioTankInfo UI_BioTankInfo;

    public override void Init()
    {
        // 禁用 Unity-Logs-Viewer 的 Reporter（画圈手势触发的日志界面）
        var reporter = FindFirstObjectByType<Reporter>();
        if (reporter != null)
        {
            reporter.gameObject.SetActive(false);
        }

        // 初始化日志系统（最先执行，以便记录所有后续日志）
        InitializeLogger();

        InitializeCoreManagers();
        InitializeGameSettings();
        StartCoroutine(GameStart());
    }

    /// <summary>
    /// 初始化文件日志系统
    /// </summary>
    private void InitializeLogger()
    {
#if !UNITY_EDITOR
        try
        {
            // 日志保存到服务端的StreamingAssets/Logs目录
            string logDirectory = System.IO.Path.Combine(Application.streamingAssetsPath, "Logs");
            FileLogger.Initialize(logDirectory, "server");
            Debug.Log($"[GameManager] 服务端日志系统初始化完成");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 日志系统初始化失败: {e.Message}");
        }
#endif
    }

    private void InitializeCoreManagers()
    {
        // 初始化 Steamworks（必须在使用任何 Steam API 之前）
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        try
        {
            if (SteamAPI.Init())
            {
                // 设置 Steam 初始化成功标志，供 SteamCloudSaveManager 使用
                SteamCloudSaveManager.SetSteamInitialized(true);
                Debug.Log("[GameManager] Steamworks 初始化成功");
            }
            else
            {
                // 确保标志为 false，防止后续代码尝试使用未初始化的 Steam API
                SteamCloudSaveManager.SetSteamInitialized(false);
                Debug.LogWarning("[GameManager] Steamworks 初始化失败 - Steam 客户端可能未运行或 steam_appid.txt 配置错误");
            }
        }
        catch (System.Exception e)
        {
            SteamCloudSaveManager.SetSteamInitialized(false);
            Debug.LogError($"[GameManager] Steamworks 初始化异常: {e.Message}\n{e.StackTrace}");
        }
#endif

        // 初始化本地化
        try
        {
            LocalizationManager.Init();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] LocalizationManager初始化失败: {e.Message}\n{e.StackTrace}");
        }

        // 初始化 Mod 系统
        try
        {
            // 先初始化 ModScriptLoader
            GameObject modScriptLoaderObj = new GameObject("ModScriptLoader");
            modScriptLoaderObj.transform.SetParent(transform);
            modScriptLoaderObj.AddComponent<ModScriptLoader>();

            ModManager.Instance.Initialize(GameConfig.ModsDirectoryPath);
            ModManager.Instance.DiscoverMods();
            ModManager.Instance.LoadAllEnabledMods();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] ModManager初始化失败: {e.Message}\n{e.StackTrace}");
        }

        // 初始化显示器信息
        WallpaperManager.Instance.Test();

        // 注册服务到依赖注入容器（必须在创建SaveSystem之前）
        RegisterServices();

        // 初始化存档系统
        _saveSystem = new SaveSystem(
            GameConfig.SaveDirectoryPath,
            ServiceLocator.Get<IWarehouseService>(),
            ServiceLocator.Get<IBioTankService>(),
            ServiceLocator.Get<IPlayerStateService>());

        // Steam 云存档调试日志
        LogSteamCloudStatus();

        // 初始化定时器系统
        GameObject timerObject = new GameObject("TimerSystem");
        timerObject.transform.SetParent(transform);
        _timerSystem = timerObject.AddComponent<TimerSystem>();

        // 初始化游戏初始化器
        _gameInitializer = new GameInitializer(_saveSystem);

        // 设置定时任务
        _timerSystem.AddTimer(GameConfig.BioTankUpdateInterval, OnBioTankUpdate);

#if !UNITY_EDITOR
        _timerSystem.AddTimer(GameConfig.AutoSaveInterval, () => Save());
#endif
    }

    /// <summary>
    /// 注册服务到ServiceLocator
    /// </summary>
    private void RegisterServices()
    {
        // 注册仓库服务
        ServiceLocator.Register<IWarehouseService>(WarehouseManager.Instance);

        // 注册生物罐服务
        ServiceLocator.Register<IBioTankService>(BioTankRegistry.Instance);

        // 注册玩家状态服务
        ServiceLocator.Register<IPlayerStateService>(PlayerStateManager.Instance);

        // 注册实体工厂服务
        ServiceLocator.Register<IEntityFactoryService>(EntityFactory.Instance);
    }

    private void InitializeGameSettings()
    {
        var settingData = _saveSystem.LoadSettingData();
        ApplySettingData(settingData);
        Application.runInBackground = true;
    }
    /// <summary>
    /// 应用设置数据
    /// </summary>
    private void ApplySettingData(SettingSaveData settingData)
    {
        Debug.Log("[GameManager] ========== ApplySettingData 开始 ==========");

        // 获取 Steam 语言
        string steamLang = LocalizationManager.GetSteamLanguage();
        string steamMappedLang = LocalizationManager.MapSteamLanguageToProject(steamLang);
        Debug.Log($"[GameManager] Steam 返回的语言: '{steamLang}' -> 映射后: '{steamMappedLang}'");

        if (settingData == null)
        {
            Debug.Log("[GameManager] settingData 为 null，这是首次启动");

            // 创建默认设置
            BioTankSaveData defaultData = new BioTankSaveData
            {
                Id = 0,
                Position = WallpaperManager.Instance.GetStartPosition(),
                Size = WallpaperManager.Instance.GetStartSize()
            };
            WindowManager.Instance.saveData.TryAdd(0, defaultData);

            // 首次启动，使用 Steam 语言并保存
            Debug.Log($"[GameManager] 首次启动，使用 Steam 语言并保存: {steamMappedLang}");
            LocalizationManager.SetLanguageAndSave(steamMappedLang);
            Debug.Log("[GameManager] ========== ApplySettingData 结束 (首次启动) ==========");
            return;
        }

        Debug.Log($"[GameManager] 存档中的语言设置: '{settingData.language}'");
        Debug.Log($"[GameManager] 存档中的窗口位置: {settingData.WinowPos}");
        Debug.Log($"[GameManager] 存档中的窗口缩放: {settingData.WinowScale}");

        WindowManager.Instance.SetSaveData(settingData.BioTankSaveDatas);
        WallpaperManager.Instance.SetDisplayPosition(settingData.WinowPos.x, settingData.WinowPos.y);

        // 使用存档中的语言（已经在 SaveSystem.LoadSettingData 中处理过首次启动的情况）
        string finalLanguage = settingData.language;

        // 验证语言有效性
        if (string.IsNullOrEmpty(finalLanguage) || !LocalizationManager.IsLanguageAvailable(finalLanguage))
        {
            finalLanguage = steamMappedLang;
            Debug.Log($"[GameManager] 存档语言无效，使用 Steam 语言: {finalLanguage}");
            // 保存修正后的语言
            LocalizationManager.SetLanguageAndSave(finalLanguage);
        }
        else
        {
            Debug.Log($"[GameManager] 使用存档中的语言: {finalLanguage}");
            LocalizationManager.SetLanguage(finalLanguage);
        }

        // 重要：验证缩放比例，避免0或无效值导致窗口不可见
        float scale = settingData.WinowScale;
        if (scale <= 0.01f || scale > 10f)
        {
            scale = 1.0f;
            Debug.LogWarning($"[GameManager] 检测到无效的窗口缩放比例: {settingData.WinowScale}，使用默认值: {scale}");
        }
        WallpaperManager.Instance.SetDisplayScale(scale);

        Debug.Log("[GameManager] ========== ApplySettingData 结束 ==========");
    }

    /// <summary>
    /// 解析一个可用的语言编码，按顺序尝试：入参 -> 当前语言 -> Steam语言 -> English -> 语言列表首个
    /// </summary>
    private string ResolveLanguageCode(string langCode)
    {
        Debug.Log($"[GameManager] ========== ResolveLanguageCode 开始 ==========");
        Debug.Log($"[GameManager] 输入的语言代码: '{langCode}'");

        // 1. 优先使用传入的有效语言
        if (!string.IsNullOrEmpty(langCode) && LocalizationManager.IsLanguageAvailable(langCode))
        {
            Debug.Log($"[GameManager] 使用传入的有效语言: '{langCode}'");
            return langCode;
        }
        Debug.Log($"[GameManager] 传入的语言 '{langCode}' 无效或为空，继续查找...");

        // 2. 尝试使用当前已设置的语言（排除初始值 "null"）
        var current = LocalizationManager.GetCurrentLanguage();
        Debug.Log($"[GameManager] 当前语言: '{current}'");
        if (!string.IsNullOrEmpty(current) && current != "null" && LocalizationManager.IsLanguageAvailable(current))
        {
            Debug.Log($"[GameManager] 使用当前已设置的语言: '{current}'");
            return current;
        }
        Debug.Log($"[GameManager] 当前语言 '{current}' 不可用，继续查找...");

        // 3. 尝试从Steam获取语言（首次启动时使用）
        Debug.Log("[GameManager] 尝试从 Steam 获取语言...");
        string steamLang = LocalizationManager.GetSteamLanguage();
        Debug.Log($"[GameManager] 从 Steam 获取到的语言: '{steamLang}'");
        if (!string.IsNullOrEmpty(steamLang))
        {
            string mappedLang = LocalizationManager.MapSteamLanguageToProject(steamLang);
            Debug.Log($"[GameManager] Steam 语言映射结果: '{steamLang}' -> '{mappedLang}'");
            if (LocalizationManager.IsLanguageAvailable(mappedLang))
            {
                Debug.Log($"[GameManager] 首次启动，使用Steam语言: {steamLang} -> {mappedLang}");
                return mappedLang;
            }
            Debug.Log($"[GameManager] 映射后的语言 '{mappedLang}' 不可用");
        }

        // 4. 回退到默认英文
        Debug.Log("[GameManager] 尝试回退到默认语言 'English'");
        if (LocalizationManager.IsLanguageAvailable("English"))
        {
            Debug.Log("[GameManager] 使用默认语言: English");
            return "English";
        }

        // 5. 最后回退到语言列表第一个
        var langs = LocalizationManager.GetLanguageList();
        Debug.Log($"[GameManager] 可用语言列表: {(langs != null ? string.Join(", ", langs) : "null")}");
        if (langs != null && langs.Count > 0)
        {
            Debug.Log($"[GameManager] 使用语言列表第一个: '{langs[0]}'");
            return langs[0];
        }

        Debug.LogWarning("[GameManager] 无法找到任何可用语言，返回默认 'English'");
        return "English";
    }

    /// <summary>
    /// 游戏启动协程 - 委托给GameInitializer处理
    /// </summary>
    public IEnumerator GameStart()
    {
        yield return _gameInitializer.StartGame();
    }

    /// <summary>
    /// 保存游戏 - 委托给SaveSystem处理
    /// </summary>
    public void Save(bool sendCommandToClients = true, bool generateNewIds = false)
    {
        _saveSystem.Save(sendCommandToClients, generateNewIds);
    }
    /// <summary>
    /// 删除存档命令 - 删除后重新加载游戏
    /// </summary>
    public void DoDeleteCommand()
    {
        _saveSystem.DeleteAllSaveData();

        // 关闭非活动客户端
        var inactiveClients = WebSocketServerManager.GetUnActiveClient();
        if (inactiveClients != null)
        {
            foreach (var client in inactiveClients)
            {
                client.SendCloseCommand();
            }
        }

        // 重新加载游戏
        StartCoroutine(_gameInitializer.StartGame());
        Save(false);

        // 通知活动客户端
        var activeClient = WebSocketServerManager.GetActiveClient();
        activeClient?.SendSetBioTankIDCommand(PlayerManager.ActiveBioTanksID, WallpaperManager.Instance.GetWorkerWPtr());
        activeClient?.SendLoadCommand();
    }
    /// <summary>
    /// 生态缸更新回调 - 每秒调用一次
    /// </summary>
    private void OnBioTankUpdate()
    {
        // 使用快照避免在StateUpdate期间修改BioTanks集合导致的异常
        // 直接使用BioTankRegistry，避免每次创建字典副本
        var bioTankSnapshot = BioTankRegistry.Instance.GetAllBioTanks();
        foreach (var bioTank in bioTankSnapshot)
        {
            bioTank.StateUpdate(1);
        }
    }
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void Quit()
    {
        StartCoroutine(QuitCoroutine());
    }

    /// <summary>
    /// 退出游戏协程 - 确保关闭命令发送到客户端后再退出
    /// </summary>
    private IEnumerator QuitCoroutine()
    {
        // 发送关闭命令给所有客户端
        foreach (var client in WebSocketServerManager.GetClients().Values)
        {
            client.SendCloseCommand();
        }

        // 等待一小段时间确保消息发送到客户端
        yield return new WaitForSeconds(0.5f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 记录Steam云存档状态信息 - 用于调试
    /// </summary>
    private void LogSteamCloudStatus()
    {
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        if (_saveSystem?.SteamCloudSave != null)
        {
            var cloudMgr = _saveSystem.SteamCloudSave;
            Debug.Log("=== Steam 云存档初始化状态 ===");
            Debug.Log($"  IsCloudEnabled: {cloudMgr.IsCloudEnabled}");
            Debug.Log($"  IsCloudEnabledForAccount: {cloudMgr.IsCloudEnabledForAccount}");
            Debug.Log($"  IsCloudEnabledForApp: {cloudMgr.IsCloudEnabledForApp}");

            var info = cloudMgr.GetCloudSaveInfo();
            Debug.Log($"  ManifestExists: {info.ManifestExists}");
            Debug.Log($"  FileCount: {info.FileCount}");
            Debug.Log($"  TotalSize: {info.TotalSize} bytes");

            if (cloudMgr.GetQuota(out ulong total, out ulong avail))
            {
                Debug.Log($"  Cloud Quota: {avail}/{total} bytes available ({avail / 1024.0 / 1024.0:F2}/{total / 1024.0 / 1024.0:F2} MB)");
            }
            else
            {
                Debug.LogWarning("  无法获取 Steam 云存档配额信息");
            }
            Debug.Log("================================");
        }
        else
        {
            Debug.LogWarning("[GameManager] Steam 云存档管理器未初始化");
        }
#else
        Debug.Log("[GameManager] Steam 云存档功能未启用 (DISABLESTEAMWORKS 或 STEAMWORKSNET 宏未定义)");
#endif
    }

    /// <summary>
    /// 每帧更新 - 处理 Steamworks 回调
    /// </summary>
    private void Update()
    {
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        // 只有在 Steam 初始化成功后才调用 RunCallbacks
        if (SteamCloudSaveManager.GetSteamInitialized())
        {
            SteamAPI.RunCallbacks();
        }
#endif
    }

    /// <summary>
    /// 应用退出时清理
    /// </summary>
    private void OnApplicationQuit()
    {
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        // 关闭 Steamworks
        SteamAPI.Shutdown();
        Debug.Log("[GameManager] Steamworks 已关闭");
#endif

        // 关闭所有正常客户端
        var clients = WebSocketServerManager.GetClients();
        foreach (var client in clients)
        {
            client.Value.SendCloseCommand();
        }

        // 关闭所有休眠客户端
        if (PreloadClientPoolManager.Instance != null)
        {
            PreloadClientPoolManager.Instance.CloseAllDormantClients();
        }
    }
}
