using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 存档系统 - 负责游戏数据的保存和加载
/// 使用依赖注入获取服务
/// 使用ServerPackage格式保存服务器数据
/// </summary>
public class SaveSystem
{
        private readonly string _saveDirectory;
        private readonly IWarehouseService _warehouseService;
        private readonly IBioTankService _bioTankService;
        private readonly IPlayerStateService _playerStateService;
    private bool _migrationChecked = false;

#if !DISABLESTEAMWORKS && STEAMWORKSNET
    private SteamCloudSaveManager _steamCloudSave;

    /// <summary>
    /// Steam 云存档管理器（仅在 Steam 版本可用）
    /// </summary>
    public SteamCloudSaveManager SteamCloudSave => _steamCloudSave;
#endif

    public SaveSystem(string saveDirectory, IWarehouseService warehouseService, IBioTankService bioTankService, IPlayerStateService playerStateService)
    {
        _saveDirectory = saveDirectory;
        _warehouseService = warehouseService;
        _bioTankService = bioTankService;
        _playerStateService = playerStateService;
        EnsureSaveDirectoryExists();

        // 存档迁移：将旧格式存档迁移到新的ServerPackage/ClientPackage格式
        if (SaveMigrationTool.NeedsMigration(_saveDirectory))
        {
            SaveMigrationTool.MigrateToNewFormat(_saveDirectory);
        }

#if !DISABLESTEAMWORKS && STEAMWORKSNET
        // 初始化 Steam 云存档管理器
        _steamCloudSave = new SteamCloudSaveManager(_saveDirectory);
#endif
    }

        #region 保存逻辑

    /// <summary>
    /// 保存游戏数据
    /// </summary>
    public void Save(bool sendCommandToClients = true, bool generateNewIds = false)
    {
        SaveData saveData = CollectSaveData(generateNewIds);
        GatherSaveData gatherSaveData = CollectGatherSaveData();
        SettingSaveData settingSaveData = SettingSaveData.GetNewSaveData();

        WriteSaveDataToDisk(saveData, gatherSaveData, settingSaveData);

        if (sendCommandToClients)
        {
            NotifyClientsToSave();
        }

#if !DISABLESTEAMWORKS && STEAMWORKSNET
        // 保存完成后自动上传到 Steam Cloud (Server + Client 存档)
        if (_steamCloudSave != null && _steamCloudSave.IsCloudEnabled)
        {
            _steamCloudSave.UploadAllSavesToCloudAsync(true, (success) =>
            {
                if (!success)
                {
                    Debug.LogWarning("[SaveSystem] 上传到 Steam Cloud 失败，存档仅保存在本地");
                }
            });
        }
#endif
        }

    /// <summary>
    /// 保存初始存档数据（首次启动时使用）
    /// 直接将模板数据写入磁盘，不从游戏对象收集数据
    /// </summary>
    public void SaveInitialData(SaveData saveData, GatherSaveData gatherSaveData)
    {
        SettingSaveData settingSaveData = SettingSaveData.GetNewSaveData();
        WriteInitialSaveDataToDisk(saveData, gatherSaveData, settingSaveData);
        Debug.Log("[SaveSystem] 初始存档数据已保存");
    }

        /// <summary>
        /// 收集所有需要保存的数据
        /// </summary>
        private SaveData CollectSaveData(bool generateNewIds)
        {
            // 收集BioTank数据和对应的世界数据文件路径
            var bioTankDatas = CollectBioTankData(generateNewIds);
            var blockSaveDatas = CollectBlockSaveDatas(bioTankDatas);

            SaveData saveData = new SaveData
            {
                PlayerStateData = _playerStateService.StateData,
                BioTankStateDatas = bioTankDatas,
                fishStates = CollectFishData(generateNewIds),
                plantStateDatas = CollectPlantData(generateNewIds),
                decorationStateDatas = CollectDecorationData(),
                facilitiesStateDatas = FacilityManager.Instance.GetAllFacilitiesStateData().ToArray(),
                unLockFacilitiesData = CollectUnlockFacilitiesData(),
                warehouseSaveData = CollectWarehouseData(generateNewIds),
                BlockSaveDatas = blockSaveDatas,
                activeID = IDFactory.Count,
                tutorialSaveData = CollectTutorialData()
            };

            return saveData;
        }

        /// <summary>
        /// 收集世界数据文件路径
        /// </summary>
        private string[] CollectBlockSaveDatas(BioTankStateData[] bioTankDatas)
        {
            if (bioTankDatas == null || bioTankDatas.Length == 0)
            {
                return new string[0];
            }

            var blockSaveDatas = new string[bioTankDatas.Length];
            for (int i = 0; i < bioTankDatas.Length; i++)
            {
                // 生成世界数据文件名，格式：{ID}_LandSaveData.txt
                // 注意：地形数据使用专门的文件名，与BioTankSaveData分开
                blockSaveDatas[i] = $"{bioTankDatas[i].ID}_{SysDefine.DefaultSaveBioTankLandDataName}";
            }

            return blockSaveDatas;
        }

        /// <summary>
        /// 收集生物罐数据 - 使用泛型方法简化
        /// </summary>
        private BioTankStateData[] CollectBioTankData(bool generateNewIds)
        {
            // 使用依赖注入的BioTankService
            return _bioTankService.BioTanks.Values.CollectStateData(
                bioTank =>
                {
                    var data = bioTank.stateData;

                    // 收集该生态箱中的动物数据
                    data.Animals = bioTank.AnimalCollection.Entities
                        .Select(animal =>
                        {
                            var animalData = animal.stateData.DeepCopy();
                            animalData.ID = bioTank.stateData.ID;
                            if (generateNewIds)
                            {
                                animalData.ObjID = IDFactory.GetUniqueID();
                            }
                            return animalData;
                        }).ToArray();

                    // 收集该生态箱中的植物数据
                    data.Plants = bioTank.PlantCollection.Entities
                        .Select(plant =>
                        {
                            var plantData = plant.stateData.DeepCopy();
                            plantData.ID = bioTank.stateData.ID;
                            if (generateNewIds)
                            {
                                plantData.ObjID = IDFactory.GetUniqueID();
                            }
                            return plantData;
                        }).ToArray();

                    // 收集该生态箱中的设备数据
                    data.Facilities = FacilityManager.Instance.GetFacilitiesStateDataByBioTankId(bioTank.stateData.ID).ToArray();

                    return data;
                },
                generateNewIds,
                data =>
                {
                    if (generateNewIds)
                    {
                        data.ID = IDFactory.GetUniqueID();
                    }
                });
        }

        /// <summary>
        /// 收集鱼类数据 - 使用LINQ简化
        /// </summary>
        private AnimalStateData[] CollectFishData(bool generateNewIds)
        {
            // 使用依赖注入的BioTankService
            var allFish = _bioTankService.BioTanks.Values
                .SelectMany(bioTank => bioTank.AnimalCollection.Entities
                    .Select(fish =>
                    {
                        var data = fish.stateData.DeepCopy();
                        data.ID = bioTank.stateData.ID;
                        if (generateNewIds)
                        {
                            data.ObjID = IDFactory.GetUniqueID();
                        }
                        return data;
                    }));

            return allFish.ToArray();
        }

        /// <summary>
        /// 收集植物数据 - 使用LINQ简化
        /// </summary>
        private PlantStateData[] CollectPlantData(bool generateNewIds)
        {
            // 使用依赖注入的BioTankService
            var allPlants = _bioTankService.BioTanks.Values
                .SelectMany(bioTank => bioTank.PlantCollection.Entities
                    .Select(plant =>
                    {
                        var data = plant.stateData.DeepCopy();
                        data.ID = bioTank.stateData.ID;
                        if (generateNewIds)
                        {
                            data.ObjID = IDFactory.GetUniqueID();
                        }
                        return data;
                    }));

            return allPlants.ToArray();
        }

        /// <summary>
        /// 收集装饰物数据 - 使用LINQ简化
        /// </summary>
        private DecorationStateData[] CollectDecorationData()
        {
            // 使用依赖注入的BioTankService
            var allDecorations = _bioTankService.BioTanks.Values
                .SelectMany(bioTank => bioTank.DecorationCollection.Entities
                    .Select(decoration => decoration.stateData));

            return allDecorations.ToArray();
        }

        private UnLockFacilitiesData CollectUnlockFacilitiesData()
        {
            return new UnLockFacilitiesData
            {
                UnLockFacilityList = FacilityManager.Instance.GetUnlockFacilityList()
            };
        }

        private WarehouseSaveData CollectWarehouseData(bool generateNewIds)
        {
            return new WarehouseSaveData
            {
                fishStates = CollectWarehouseFishData(generateNewIds),
                plantStateDatas = CollectWarehousePlantData(generateNewIds),
                decorationStateDatas = CollectWarehouseDecorationData(),
                cubeStateDatas = CollectWarehouseCubeData()
            };
        }

        /// <summary>
        /// 收集仓库鱼类数据 - 使用泛型方法简化
        /// </summary>
        private AnimalStateData[] CollectWarehouseFishData(bool generateNewIds)
        {
            return _warehouseService.FishDataList.CollectFromDictionary(
                item => ((WarehouseAnimalStateData)item).stateData,
                generateNewIds,
                data =>
                {
                    if (generateNewIds)
                    {
                        data.ObjID = IDFactory.GetUniqueID();
                    }
                });
        }

        /// <summary>
        /// 收集仓库植物数据 - 使用泛型方法简化
        /// </summary>
        private PlantStateData[] CollectWarehousePlantData(bool generateNewIds)
        {
            return _warehouseService.PlantDataDicList.CollectFromDictionary(
                item => ((WarehousePlantStateData)item).data,
                generateNewIds,
                data =>
                {
                    if (generateNewIds)
                    {
                        data.ObjID = IDFactory.GetUniqueID();
                    }
                });
        }

        /// <summary>
        /// 收集仓库装饰物数据 - 使用LINQ简化
        /// </summary>
        private DecorationStateData[] CollectWarehouseDecorationData()
        {
            return _warehouseService.DecorationDataDicList.Values
                .Select(item => ((WarehouseDecorationStateData)item).stateData)
                .ToArray();
        }

        /// <summary>
        /// 收集仓库方块数据 - 使用LINQ简化
        /// </summary>
        private CubeStateData[] CollectWarehouseCubeData()
        {
            return _warehouseService.CubeDataDicList.Values
                .Select(item => ((WarehouseCubeStateData)item).stateData)
                .ToArray();
        }

        private GatherSaveData CollectGatherSaveData()
        {
            if (GameManager.Instance.gather == null)
            {
                return null;
            }

            return new GatherSaveData
            {
                gatherStateData = GameManager.Instance.gather.GetStateData(),
                gatherUnlockSaveData = GameManager.Instance.gather.GetGatherUnlockSaveData(),
                isFirstGather10001Completed = GameManager.Instance.gather.isFirstGather10001Completed
            };
        }

        /// <summary>
        /// 收集引导系统数据
        /// </summary>
        private TutorialSaveData CollectTutorialData()
        {
            if (TutorialSaveManager.Instance == null)
            {
                return new TutorialSaveData();
            }

            return TutorialSaveManager.Instance.GetSaveData();
        }

        #endregion

        #region 写入磁盘

        private void WriteSaveDataToDisk(SaveData saveData, GatherSaveData gatherSaveData, SettingSaveData settingSaveData)
        {
            // 删除旧格式存档，避免并存
            DeleteLegacyPlainSaves();

            // 打包写入服务器存档格式
            string serverPackagePath = Path.Combine(_saveDirectory, SaveMigrationTool.SERVER_PACKAGE_NAME);
            var serverPackage = new ServerPackage
            {
                mainSaveData = saveData,
                gatherSaveData = gatherSaveData,
                serverSettingData = new ServerSettingData
                {
                    language = settingSaveData?.language ?? "English"
                }
            };
            SaveServerPackage(serverPackage, serverPackagePath);

            // 保存每个BioTank的独立存档包 (地形 + 客户端设置)
            SaveAllBioTankPackages();
        }

        /// <summary>
        /// 写入初始存档数据到磁盘（首次启动时使用）
        /// 与 WriteSaveDataToDisk 不同，这里直接使用传入的 SaveData，不从游戏对象收集
        /// </summary>
        private void WriteInitialSaveDataToDisk(SaveData saveData, GatherSaveData gatherSaveData, SettingSaveData settingSaveData)
        {
            // 删除旧格式存档，避免并存
            DeleteLegacyPlainSaves();

            // 打包写入服务器存档格式
            string serverPackagePath = Path.Combine(_saveDirectory, SaveMigrationTool.SERVER_PACKAGE_NAME);
            var serverPackage = new ServerPackage
            {
                mainSaveData = saveData,
                gatherSaveData = gatherSaveData,
                serverSettingData = new ServerSettingData
                {
                    language = settingSaveData?.language ?? "English"
                }
            };
            SaveServerPackage(serverPackage, serverPackagePath);

            // 为每个BioTank创建初始独立存档包
            SaveInitialBioTankPackages(saveData);
        }

        private void NotifyClientsToSave()
        {
            foreach (var client in WebSocketServerManager.GetClients().Values)
            {
                client.SendSaveCommand();
            }
        }

        #endregion

        #region 加载逻辑

    /// <summary>
    /// 加载保存的游戏数据
    /// </summary>
    public SaveData LoadSaveData()
    {
#if !DISABLESTEAMWORKS && STEAMWORKSNET
        // 启动时从 Steam Cloud 同步最新存档 (Server + Client)
        if (_steamCloudSave != null && _steamCloudSave.IsCloudEnabled)
        {
            int syncResult = _steamCloudSave.SyncWithCloud();
            if (syncResult == -1)
            {
                // 云端存档较新，已下载
                // 通知客户端重新加载
                NotifyClientsToReloadSave();
            }
        }
#endif

        EnsurePackageReady();
        var serverPackage = LoadServerPackage();

        // 加载成功后，确保每个BioTank的独立存档包存在
        if (serverPackage?.mainSaveData != null)
        {
            EnsureBioTankPackagesExist(serverPackage.mainSaveData);
        }

        return serverPackage?.mainSaveData;
    }

    /// <summary>
    /// 确保每个BioTank的独立存档包存在
    /// 如果从云端下载的存档缺少某些BioTankPackage，则根据 ServerPackage 创建默认的
    /// </summary>
    private void EnsureBioTankPackagesExist(SaveData saveData)
    {
        if (saveData?.BioTankStateDatas == null) return;

        foreach (var bioTankState in saveData.BioTankStateDatas)
        {
            string packagePath = GetBioTankPackagePath(bioTankState.ID);

            // 如果存档包已存在，跳过
            if (File.Exists(packagePath))
            {
                continue;
            }

            Debug.Log($"[SaveSystem] BioTank {bioTankState.ID} 存档包不存在，正在创建...");

            // 读取地形数据（可能来自旧格式文件）
            string landData = LoadLandDataForBioTank(bioTankState.ID);

            // 从 DefaultCameraPos 获取摄像机位置
            CameraPositionData cameraPos = null;
            if (bioTankState.DefaultCameraPos != null)
            {
                cameraPos = new CameraPositionData
                {
                    angleX = bioTankState.DefaultCameraPos.angleX,
                    angleY = bioTankState.DefaultCameraPos.angleY,
                    radius = bioTankState.DefaultCameraPos.radius,
                    targetPos = bioTankState.DefaultCameraPos.TargetPos
                };
                Debug.Log($"[SaveSystem] BioTank {bioTankState.ID} 使用 DefaultCameraPos: angleX={cameraPos.angleX}, angleY={cameraPos.angleY}, radius={cameraPos.radius}");
            }

            // 创建并保存BioTankPackage
            var bioTankPackage = new BioTankSavePackage
            {
                id = bioTankState.ID,
                landData = landData,
                cameraPos = cameraPos,
                windowPos = null
            };

            SaveBioTankPackageToFile(bioTankPackage);
        }
    }

    /// <summary>
    /// 通知所有客户端重新加载存档
    /// </summary>
    private void NotifyClientsToReloadSave()
    {
        foreach (var client in WebSocketServerManager.GetClients().Values)
        {
            // 使用 SendLoadCommand 通知客户端重新加载 ClientPackage.dat
            client.SendLoadCommand();
        }
    }

    /// <summary>
    /// 从指定路径加载存档
    /// 兼容新格式(ServerPackage)和旧格式(SaveData)
    /// </summary>
    public SaveData LoadSaveDataFromPath(string path)
    {
        EnsurePackageReady();

        // 首先尝试加载新格式 ServerPackage
        var serverPackage = LoadServerPackage(path);
        if (serverPackage != null && serverPackage.mainSaveData != null)
        {
            return serverPackage.mainSaveData;
        }

        // 如果新格式加载失败，尝试加载旧格式 SaveData（用于蓝图模板等）
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            string json = SaveCompression.LoadSmart(path);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[SaveSystem] 文件内容为空");
                return null;
            }

            var saveData = JsonUtility.FromJson<SaveData>(json);
            if (saveData != null)
            {
                return saveData;
            }
            else
            {
                Debug.LogWarning($"[SaveSystem] JSON解析结果为null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveSystem] 旧格式加载失败: {path}, {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// 加载设置数据
    /// </summary>
    public SettingSaveData LoadSettingData()
    {
        EnsurePackageReady();
        var serverPackage = LoadServerPackage();

        SettingSaveData data = null;
        if (serverPackage != null && serverPackage.serverSettingData != null)
        {
            // 从ServerPackage的serverSettingData构造SettingSaveData
            data = new SettingSaveData
            {
                language = serverPackage.serverSettingData.language
            };
            Debug.Log($"[SaveSystem] 从存档加载语言设置: '{data.language}'");
        }
        else
        {
            // 兼容旧格式：单独文件
            string path = Path.Combine(_saveDirectory, SysDefine.DefaultSettingDataName);
            data = LoadAndMigrate<SettingSaveData>(path);
            if (data != null)
            {
                Debug.Log($"[SaveSystem] 从旧格式存档加载语言设置: '{data.language}'");
            }
        }

        // 兜底：保证语言字段有效，避免空语言导致初始化报错
        if (data == null)
        {
            Debug.Log("[SaveSystem] 没有找到存档，创建新的设置数据");
            // 首次启动，从 Steam 获取语言
            string steamLang = LocalizationManager.GetSteamLanguage();
            string steamMappedLang = LocalizationManager.MapSteamLanguageToProject(steamLang);
            Debug.Log($"[SaveSystem] 首次启动，从 Steam 获取语言: '{steamLang}' -> '{steamMappedLang}'");

            data = new SettingSaveData();
            data.language = steamMappedLang;
            data.WinowScale = 1.0f;
        }
        else
        {
            data = EnsureValidLanguage(data);
        }

        Debug.Log($"[SaveSystem] 最终返回的语言设置: '{data.language}'");
        return data;
    }

    /// <summary>
    /// 加载采集数据
    /// </summary>
    public GatherSaveData LoadGatherSaveData()
    {
        EnsurePackageReady();
        var serverPackage = LoadServerPackage();

        // 如果打包中缺失或字段为空，尝试旧格式，否则回退为空让 Gather 自行初始化
        if (serverPackage != null && serverPackage.gatherSaveData != null &&
            serverPackage.gatherSaveData.gatherStateData != null &&
            !string.IsNullOrEmpty(serverPackage.gatherSaveData.gatherStateData.GatherID))
        {
            return serverPackage.gatherSaveData;
        }

        // 兼容旧格式：单独文件
        string path = Path.Combine(_saveDirectory, SysDefine.DefaultGatherSaveDataName);
        var legacy = LoadAndMigrate<GatherSaveData>(path);
        if (legacy != null && legacy.gatherStateData != null && !string.IsNullOrEmpty(legacy.gatherStateData.GatherID))
        {
            return legacy;
        }

        return null;
    }

    #endregion

        #region 删除存档

        /// <summary>
        /// 删除所有存档文件
        /// </summary>
        public void DeleteAllSaveData()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Debug.LogWarning($"存档目录不存在: {_saveDirectory}");
                return;
            }

            try
            {
                DeleteAllFilesInFolder(_saveDirectory);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"删除存档文件时发生错误: {e.Message}");
            }
        }

        private void DeleteAllFilesInFolder(string folderPath)
        {
            // 删除所有文件
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            // 递归删除子文件夹中的文件
            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string subFolder in subFolders)
            {
                DeleteAllFilesInFolder(subFolder);
            }
        }

        #endregion

        #region 工具方法

        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

    /// <summary>
    /// 确保在读取任何存档前完成旧存档迁移
    /// </summary>
    private void EnsurePackageReady()
    {
        if (_migrationChecked)
        {
            return;
        }
        _migrationChecked = true;

        // 新格式使用ServerPackage.dat
        string serverPackagePath = Path.Combine(_saveDirectory, SaveMigrationTool.SERVER_PACKAGE_NAME);
        if (!File.Exists(serverPackagePath))
        {
            // 如果有旧存档，使用SaveMigrationTool迁移
            if (SaveMigrationTool.NeedsMigration(_saveDirectory))
            {
                SaveMigrationTool.MigrateToNewFormat(_saveDirectory);
            }
        }
    }

    /// <summary>
    /// 通用加载方法，兼容旧的未压缩存档并在加载后自动转换为压缩格式
    /// </summary>
    private T LoadAndMigrate<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            return null;
        }

        bool isCompressed = SaveCompression.IsGZipCompressed(path);
        string json = SaveCompression.LoadSmart(path);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning($"[SaveSystem] 加载内容为空: {path}");
            return null;
        }

        T data = JsonUtility.FromJson<T>(json);

        // 如果是旧的未压缩存档，加载成功后写回为压缩格式，保证后续读写都走gzip
        if (!isCompressed && data != null)
        {
            try
            {
                SaveCompression.SaveCompressed(path, json);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] 压缩迁移失败: {path}, {e.Message}");
            }
        }

        return data;
    }

    /// <summary>
    /// 保存服务器存档包
    /// </summary>
    private void SaveServerPackage(ServerPackage package, string path)
    {
        string json = JsonUtility.ToJson(package);
        SaveCompression.SaveCompressed(path, json);
    }

    /// <summary>
    /// 加载服务器存档包
    /// </summary>
    private ServerPackage LoadServerPackage(string overridePath = null)
    {
        EnsurePackageReady();

        // 读取ServerPackage.dat
        string serverPackagePath = Path.Combine(_saveDirectory, SaveMigrationTool.SERVER_PACKAGE_NAME);
        string path = overridePath ?? serverPackagePath;

        if (!File.Exists(path))
        {
            return null;
        }

        string json = SaveCompression.LoadSmart(path);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning($"[SaveSystem] 服务器存档内容为空: {path}");
            return null;
        }

        // 快速检查：ServerPackage 格式必须包含 "mainSaveData" 字段
        // 如果不包含，说明这是旧格式的 SaveData，不应该用 ServerPackage 解析
        if (!json.Contains("\"mainSaveData\""))
        {
            return null;
        }

        try
        {
            var serverPackage = JsonUtility.FromJson<ServerPackage>(json);
            if (serverPackage != null && serverPackage.mainSaveData != null)
            {
                // 额外检查：确保这真的是一个有效的ServerPackage
                bool hasValidData = serverPackage.mainSaveData.BioTankStateDatas != null ||
                                   serverPackage.mainSaveData.fishStates != null ||
                                   serverPackage.mainSaveData.plantStateDatas != null ||
                                   serverPackage.mainSaveData.warehouseSaveData != null;

                if (hasValidData)
                {
                    return serverPackage;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] 解析服务器存档失败: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// 确保设置数据中的语言代码有效，不在列表时回退到可用语言
    /// </summary>
    private SettingSaveData EnsureValidLanguage(SettingSaveData data)
    {
        if (data == null)
        {
            return SettingSaveData.GetNewSaveData();
        }

        data.language = ResolveLanguageCode(data.language);
        return data;
    }

    /// <summary>
    /// 根据可用语言列表选择有效的语言代码，提供多层回退
    /// </summary>
    private string ResolveLanguageCode(string langCode)
    {
        // 优先使用传入且可用的语言
        if (!string.IsNullOrEmpty(langCode) && LocalizationManager.IsLanguageAvailable(langCode))
        {
            return langCode;
        }

        // 尝试当前语言
        string current = LocalizationManager.GetCurrentLanguage();
        if (!string.IsNullOrEmpty(current) && LocalizationManager.IsLanguageAvailable(current))
        {
            return current;
        }

        // 尝试默认英文
        if (LocalizationManager.IsLanguageAvailable("English"))
        {
            return "English";
        }

        // 回退到语言列表的第一个可用项
        var langs = LocalizationManager.GetLanguageList();
        if (langs != null && langs.Count > 0)
        {
            return langs[0];
        }

        // 最终兜底
        return "English";
    }

    /// <summary>
    /// 删除旧的未压缩存档，避免新格式启用后仍保留老文件
    /// </summary>
    private void DeleteLegacyPlainSaves()
    {
        string[] legacyTargets =
        {
            Path.Combine(_saveDirectory, "SavePackage.dat"),
            Path.Combine(_saveDirectory, SysDefine.DefaultSaveDataName),
            Path.Combine(_saveDirectory, SysDefine.DefaultSettingDataName),
            Path.Combine(_saveDirectory, SysDefine.DefaultGatherSaveDataName)
        };

        foreach (var file in legacyTargets)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] 删除旧存档失败: {file}, {e.Message}");
            }
        }

        // 删除旧的 ClientPackage.dat (已废弃)
        string clientPackagePath = Path.Combine(_saveDirectory, SaveMigrationTool.CLIENT_PACKAGE_NAME);
        try
        {
            if (File.Exists(clientPackagePath))
            {
                File.Delete(clientPackagePath);
                Debug.Log("[SaveSystem] 已删除旧的 ClientPackage.dat");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SaveSystem] 删除 ClientPackage.dat 失败: {e.Message}");
        }
    }

    #endregion

    #region BioTankPackage 存档方法

    /// <summary>
    /// 保存所有BioTank的独立存档包
    /// </summary>
    private void SaveAllBioTankPackages()
    {
        foreach (var bioTank in _bioTankService.BioTanks.Values)
        {
            SaveBioTankPackage(bioTank.stateData.ID);
        }
    }

    /// <summary>
    /// 为初始存档创建BioTank独立存档包
    /// </summary>
    private void SaveInitialBioTankPackages(SaveData saveData)
    {
        if (saveData?.BioTankStateDatas == null) return;

        foreach (var bioTankState in saveData.BioTankStateDatas)
        {
            // 读取地形数据
            string landData = LoadLandDataForBioTank(bioTankState.ID);

            // 从 DefaultCameraPos 获取摄像机位置
            CameraPositionData cameraPos = null;
            if (bioTankState.DefaultCameraPos != null)
            {
                cameraPos = new CameraPositionData
                {
                    angleX = bioTankState.DefaultCameraPos.angleX,
                    angleY = bioTankState.DefaultCameraPos.angleY,
                    radius = bioTankState.DefaultCameraPos.radius,
                    targetPos = bioTankState.DefaultCameraPos.TargetPos
                };
                Debug.Log($"[SaveSystem] 初始化 BioTank {bioTankState.ID} 摄像机位置: angleX={cameraPos.angleX}, angleY={cameraPos.angleY}, radius={cameraPos.radius}");
            }

            // 创建并保存BioTankPackage
            var bioTankPackage = new BioTankSavePackage
            {
                id = bioTankState.ID,
                landData = landData,
                cameraPos = cameraPos,
                windowPos = null // 初始时窗口位置为空
            };

            SaveBioTankPackageToFile(bioTankPackage);
        }
    }

    /// <summary>
    /// 保存单个BioTank的独立存档包
    /// 【修复】保留现有的地形数据，不覆盖客户端已保存的修改
    /// 地形数据由客户端负责保存，服务端只更新摄像头和窗口位置
    /// </summary>
    public void SaveBioTankPackage(int bioTankId)
    {
        try
        {
            // 读取现有存档包（如果存在）
            var existingPackage = LoadBioTankPackage(bioTankId);

            // 【修复】优先使用现有存档包中的地形数据，避免覆盖客户端的修改
            // 只有当现有存档包不存在或地形数据为空时，才从旧格式文件读取
            string landData = existingPackage?.landData;
            if (string.IsNullOrEmpty(landData))
            {
                landData = LoadLandDataForBioTank(bioTankId);
            }

            // 【修复】如果地形数据仍然为空，为新建的空布局生态箱创建空白地形数据
            if (string.IsNullOrEmpty(landData) && _bioTankService.BioTanks.TryGetValue(bioTankId, out var bioTankForTerrain))
            {
                var stateData = bioTankForTerrain.stateData;
                landData = CreateEmptyTerrainData(stateData.area_x, stateData.area_y, stateData.area_z, bioTankId);
                Debug.Log($"[SaveSystem] 为空布局生态箱 {bioTankId} 创建空白地形数据，尺寸: {stateData.area_x}x{stateData.area_y}x{stateData.area_z}");
            }

            // 创建新的BioTankPackage，保留现有的客户端设置
            var bioTankPackage = new BioTankSavePackage
            {
                id = bioTankId,
                landData = landData,
                cameraPos = existingPackage?.cameraPos,
                windowPos = existingPackage?.windowPos
            };

            // 如果没有现有设置，尝试从 BioTank 的 DefaultCameraPos 获取
            if (bioTankPackage.cameraPos == null)
            {
                if (_bioTankService.BioTanks.TryGetValue(bioTankId, out var bioTank) &&
                    bioTank.stateData?.DefaultCameraPos != null)
                {
                    bioTankPackage.cameraPos = new CameraPositionData
                    {
                        angleX = bioTank.stateData.DefaultCameraPos.angleX,
                        angleY = bioTank.stateData.DefaultCameraPos.angleY,
                        radius = bioTank.stateData.DefaultCameraPos.radius,
                        targetPos = bioTank.stateData.DefaultCameraPos.TargetPos
                    };
                }
            }

            SaveBioTankPackageToFile(bioTankPackage);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] 保存 BioTank {bioTankId} 存档包失败: {e.Message}");
        }
    }

    /// <summary>
    /// 将BioTankPackage保存到文件
    /// </summary>
    private void SaveBioTankPackageToFile(BioTankSavePackage package)
    {
        string packagePath = GetBioTankPackagePath(package.id);
        string json = JsonUtility.ToJson(package);
        SaveCompression.SaveCompressed(packagePath, json);
        Debug.Log($"[SaveSystem] 已保存 BioTank {package.id} 独立存档包: {packagePath}");

        // 删除旧格式的地形文件（已迁移到新格式）
        DeleteLegacyLandDataFiles(package.id);
    }

    /// <summary>
    /// 删除旧格式的地形数据文件
    /// </summary>
    private void DeleteLegacyLandDataFiles(int bioTankId)
    {
        string[] legacyLandFiles = new string[]
        {
            Path.Combine(_saveDirectory, $"{bioTankId}_{SysDefine.DefaultSaveBioTankLandDataName}.gz"),
            Path.Combine(_saveDirectory, $"{bioTankId}_{SysDefine.DefaultSaveBioTankLandDataName}"),
            Path.Combine(_saveDirectory, $"{bioTankId}_{SysDefine.DefaultSaveBioTankLandDataName}.txt.gz")
        };

        foreach (var file in legacyLandFiles)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    Debug.Log($"[SaveSystem] 已删除旧格式地形文件: {file}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] 删除旧格式地形文件失败: {file}, {e.Message}");
            }
        }
    }

    /// <summary>
    /// 加载BioTank的独立存档包
    /// </summary>
    public BioTankSavePackage LoadBioTankPackage(int bioTankId)
    {
        string packagePath = GetBioTankPackagePath(bioTankId);

        // 首先尝试加载新格式
        if (File.Exists(packagePath))
        {
            try
            {
                string json = SaveCompression.LoadSmart(packagePath);
                if (!string.IsNullOrEmpty(json))
                {
                    var package = JsonUtility.FromJson<BioTankSavePackage>(json);
                    if (package != null)
                    {
                        return package;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] 加载 BioTank {bioTankId} 存档包失败: {e.Message}");
            }
        }

        // 如果新格式不存在，尝试从旧格式迁移
        return MigrateLegacyBioTankData(bioTankId);
    }

    /// <summary>
    /// 从旧格式迁移BioTank数据
    /// </summary>
    private BioTankSavePackage MigrateLegacyBioTankData(int bioTankId)
    {
        // 读取旧的地形数据
        string landData = LoadLandDataForBioTank(bioTankId);
        if (string.IsNullOrEmpty(landData))
        {
            return null;
        }

        // 读取旧的客户端设置
        CameraPositionData cameraPos = null;
        WindowPositionData windowPos = null;

        string clientPackagePath = Path.Combine(_saveDirectory, SaveMigrationTool.CLIENT_PACKAGE_NAME);
        if (File.Exists(clientPackagePath))
        {
            try
            {
                string json = SaveCompression.LoadSmart(clientPackagePath);
                if (!string.IsNullOrEmpty(json))
                {
                    var clientPackage = JsonUtility.FromJson<ClientPackage>(json);
                    if (clientPackage?.bioTankSettings != null)
                    {
                        foreach (var setting in clientPackage.bioTankSettings)
                        {
                            if (setting.id == bioTankId)
                            {
                                cameraPos = setting.cameraPos;
                                windowPos = setting.windowPos;
                                break;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] 从旧 ClientPackage 读取设置失败: {e.Message}");
            }
        }

        // 创建新格式的存档包
        var bioTankPackage = new BioTankSavePackage
        {
            id = bioTankId,
            landData = landData,
            cameraPos = cameraPos,
            windowPos = windowPos
        };

        // 保存为新格式
        SaveBioTankPackageToFile(bioTankPackage);

        Debug.Log($"[SaveSystem] 已从旧格式迁移 BioTank {bioTankId} 存档");
        return bioTankPackage;
    }

    /// <summary>
    /// 读取BioTank的地形数据
    /// </summary>
    private string LoadLandDataForBioTank(int bioTankId)
    {
        // 首先检查新格式存档包
        string packagePath = GetBioTankPackagePath(bioTankId);
        if (File.Exists(packagePath))
        {
            try
            {
                string json = SaveCompression.LoadSmart(packagePath);
                if (!string.IsNullOrEmpty(json))
                {
                    var package = JsonUtility.FromJson<BioTankSavePackage>(json);
                    if (package != null && !string.IsNullOrEmpty(package.landData))
                    {
                        return package.landData;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] 从新格式读取地形数据失败: {e.Message}");
            }
        }

        // 尝试读取旧格式地形文件
        string[] possiblePaths = new string[]
        {
            Path.Combine(_saveDirectory, $"{bioTankId}_{SysDefine.DefaultSaveBioTankLandDataName}.gz"),
            Path.Combine(_saveDirectory, $"{bioTankId}_{SysDefine.DefaultSaveBioTankLandDataName}")
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                try
                {
                    string landData = SaveCompression.LoadSmart(path);
                    if (!string.IsNullOrEmpty(landData))
                    {
                        return landData;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SaveSystem] 读取旧地形文件失败: {path}, {e.Message}");
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 创建空白地形数据JSON
    /// 用于新建空布局生态箱时初始化地形
    /// </summary>
    private string CreateEmptyTerrainData(int sizeX, int sizeY, int sizeZ, int bioTankId)
    {
        // 创建与客户端PreviewModeManager.CreateEmptyTerrain相同格式的空白地形JSON
        return $"{{\"Terrain\": {{\"Pos\": {{\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}}, \"ID\": {bioTankId}, \"xMax\": {sizeX}, \"yMax\": {sizeY}, \"zMax\": {sizeZ}, \"Modifications\": []}}}}";
    }

    /// <summary>
    /// 获取BioTank存档包文件路径
    /// </summary>
    public string GetBioTankPackagePath(int bioTankId)
    {
        return Path.Combine(_saveDirectory, $"{bioTankId}{SaveMigrationTool.BIOTANK_PACKAGE_SUFFIX}");
    }

    /// <summary>
    /// 更新BioTank的客户端设置（由客户端调用）
    /// </summary>
    public void UpdateBioTankClientSettings(int bioTankId, CameraPositionData cameraPos, WindowPositionData windowPos)
    {
        try
        {
            var package = LoadBioTankPackage(bioTankId);
            if (package == null)
            {
                // 如果不存在，创建新的
                package = new BioTankSavePackage
                {
                    id = bioTankId,
                    landData = LoadLandDataForBioTank(bioTankId)
                };
            }

            // 更新客户端设置
            if (cameraPos != null)
            {
                package.cameraPos = cameraPos;
            }
            if (windowPos != null)
            {
                package.windowPos = windowPos;
            }

            SaveBioTankPackageToFile(package);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] 更新 BioTank {bioTankId} 客户端设置失败: {e.Message}");
        }
    }

    #endregion
}
