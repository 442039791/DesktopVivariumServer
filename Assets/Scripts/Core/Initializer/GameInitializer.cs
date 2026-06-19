using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 游戏初始化器 - 负责游戏启动流程
/// </summary>
public class GameInitializer
{
    private readonly SaveSystem _saveSystem;

    public GameInitializer(SaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

        /// <summary>
        /// 执行游戏启动流程
        /// </summary>
        public IEnumerator StartGame()
        {
            // 1. 等待GameLaunch初始化完成
            yield return new WaitUntil(() => GameLaunch.Instance.InitComplete);

            // 2. 初始化金钱事件
            GameManager.Instance.UI_Title.InitMoneyEvent();

            // 3. 加载全局设置
            GlobalSetting.Instance.OnInit();

            // 4. 初始化预制体资源
            ResMgr.Instance.InitPrefab();
            yield return new WaitUntil(() => ResMgr.Instance.prefabInitComplete);

            // 5. 加载存档数据
            LoadGameData();
            yield return new WaitUntil(() => GameManager.Instance.LoadComplete && GameManager.Instance.BlockLoadComplete);

            // 6. 初始化UI
            GameManager.Instance.UI_Title.Init();

            // 设置生物罐信息UI引用（保留以兼容部分功能）
            BioTankManager.uIInfo = GameManager.Instance.UI_BioTankInfo;

            // 7. 初始化并激活生物罐信息UI（在生物罐创建完成后）
            int bioTankCount = BioTankRegistry.Instance.Count;

            if (BioTankManager.uIInfo != null && bioTankCount > 0)
            {
                // 确保有活动生物罐
                if (BioTankRegistry.Instance.ActiveBioTank == null)
                {
                    var firstBioTank = BioTankRegistry.Instance.GetAllBioTanks().First();
                    BioTankRegistry.Instance.SetActiveBioTank(firstBioTank);
                }

                BioTankManager.uIInfo.gameObject.SetActive(true);
                BioTankManager.uIInfo.Init();

                // UI现在使用主动拉取模式，在Init中会自动刷新数据
                if (BioTankRegistry.Instance.ActiveBioTank != null)
                {
                    BioTankManager.uIInfo.Refresh();
                }
            }
            else
            {
                if (BioTankManager.uIInfo == null)
                {
                    Debug.LogWarning("[GameInitializer] UI_BioTankInfo 对象为 null，无法初始化!");
                }
                if (bioTankCount == 0)
                {
                    Debug.LogWarning("[GameInitializer] 没有生物罐，无法初始化UI!");
                }
            }

            // 8. 恢复所有暂停的对象
            foreach (var obj in GameManager.Instance.StopList)
            {
                if (obj != null)
                {
                    obj.StopChange(false);
                }
            }

            // 9. 等待壁纸管理器初始化
            yield return new WaitUntil(() => WallpaperManager.Instance.InitWorkerW);
            WallpaperManager.Instance.StartCoroutine(WallpaperManager.Instance.SetWindowSizeCoroutine(0.1f));

            // 10. 启动隐藏进程（仅在非编辑器模式）
#if !UNITY_EDITOR
            yield return LaunchHiddenProcesses();
#endif

            // 注意：游戏开场引导已在 UI_Title.Init() 中启动
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        private void LoadGameData()
        {
            ClearAllData();

            // 【存档迁移】在加载前检查并迁移旧格式存档
            var migrationService = new SaveMigrationService(_saveSystem);
            migrationService.CheckAndMigrateSaveData();

            SaveData saveData = _saveSystem.LoadSaveData();
            GatherSaveData gatherSaveData = _saveSystem.LoadGatherSaveData();

            if (saveData != null)
            {
                Debug.Log("[GameInitializer] 加载存档成功");

                // 【孤儿实体清理】清理模组卸载后遗留的无效实体
                var entityCleaner = new OrphanedEntityCleaner();
                var cleanupResult = entityCleaner.CleanupOrphanedEntities(saveData);
                if (cleanupResult.HasRemovedEntities)
                {
                    Debug.Log($"[GameInitializer] 已清理 {cleanupResult.TotalRemoved} 个无效实体（可能来自已卸载的模组）");
                }

                IDFactory.Count = saveData.activeID;
                GameManager.Instance.FirstStartGame = false;

                // 加载引导系统数据
                if (saveData.tutorialSaveData != null)
                {
                    TutorialSaveManager.Instance.LoadFromSaveData(saveData.tutorialSaveData);
                }
                else
                {
                    TutorialSaveManager.Instance.LoadFromSaveData(new TutorialSaveData());
                }

                GameManager.Instance.StartCoroutine(CreateFromSaveData(saveData, gatherSaveData));
            }
            else
            {
                // 首次启动游戏，初始化存档
                Debug.Log("[GameInitializer] 首次启动，初始化存档");
                GameManager.Instance.FirstStartGame = true;

                // 1. 从 InitialData + 模板文件创建默认存档数据
                SaveData defaultSaveData = CreateDefaultSaveData();
                if (defaultSaveData == null)
                {
                    Debug.LogError("[GameInitializer] 创建默认存档失败！");
                    return;
                }

                // 2. 从 InitialData 创建解锁数据
                GatherSaveData defaultGatherSaveData = CreateDefaultGatherSaveData();

                // 3. 写入磁盘，确保数据持久化
                _saveSystem.SaveInitialData(defaultSaveData, defaultGatherSaveData);
                Debug.Log("[GameInitializer] 初始存档已写入磁盘");

                // 4. 从存档加载（确保前后端数据一致）
                saveData = _saveSystem.LoadSaveData();
                gatherSaveData = _saveSystem.LoadGatherSaveData();

                if (saveData == null)
                {
                    Debug.LogError("[GameInitializer] 重新加载初始存档失败！");
                    return;
                }

                IDFactory.Count = saveData.activeID;
                GameManager.Instance.StartCoroutine(CreateFromSaveData(saveData, gatherSaveData));
            }
        }

        /// <summary>
        /// 创建默认的采集存档数据
        /// 从 InitialData 获取解锁配置
        /// </summary>
        private GatherSaveData CreateDefaultGatherSaveData()
        {
            try
            {
                var initialData = GameConfigDataBase.GetConfigData<InitialData>(1);
                if (initialData == null)
                {
                    Debug.LogError("[GameInitializer] 无法加载 InitialData 配置！");
                    return null;
                }

                var gatherSaveData = new GatherSaveData
                {
                    gatherUnlockSaveData = new UnlockSaveData(),
                    gatherStateData = null
                };

                // 从 InitialData 创建解锁列表
                gatherSaveData.gatherUnlockSaveData.FishUnlockDic = CreateUnlockArray(initialData.Fish);
                gatherSaveData.gatherUnlockSaveData.PlantUnlockDic = CreateUnlockArray(initialData.Plant);
                gatherSaveData.gatherUnlockSaveData.CubeUnlockDic = CreateUnlockArray(initialData.Cube);
                gatherSaveData.gatherUnlockSaveData.DecorationUnlockDic = CreateUnlockArray(initialData.Decoration);
                gatherSaveData.gatherUnlockSaveData.FacilitiesUnlockDic = CreateUnlockArray(initialData.Facilities);
                gatherSaveData.gatherUnlockSaveData.AdmissionUnlockDic = CreateUnlockArray(initialData.Admission);
                gatherSaveData.gatherUnlockSaveData.TankUnlockDic = CreateUnlockArray(initialData.Tank);

                return gatherSaveData;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameInitializer] 创建默认采集存档失败: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 从ID列表创建解锁数据数组
        /// </summary>
        private UnlockSaveData.GatherUnlockData[] CreateUnlockArray(List<string> idList)
        {
            if (idList == null || idList.Count == 0)
            {
                return new UnlockSaveData.GatherUnlockData[0];
            }

            var unlockList = new List<UnlockSaveData.GatherUnlockData>();
            foreach (var id in idList)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    unlockList.Add(new UnlockSaveData.GatherUnlockData
                    {
                        ItemID = id,
                        Quality = 0,
                        isUnlocked = true
                    });
                }
            }
            return unlockList.ToArray();
        }

        /// <summary>
        /// 从存档数据创建游戏对象
        /// </summary>
        private IEnumerator CreateFromSaveData(SaveData saveData, GatherSaveData gatherSaveData = null)
        {
            // 等待本地化系统语言设置完成
            yield return new WaitUntil(() => LocalizationManager.GetCurrentLanguage() != "null");

            int frameCounter = 0;
            PlayerManager player = PlayerManager.Instance;

            // 1. 创建生态缸并加载每个生态缸的动植物数据
            if (saveData.BioTankStateDatas != null)
            {
                int createdCount = 0;
                // 用于统计生态箱序号（生成默认名称时使用）
                int biotankIndex = 0;

                foreach (var data in saveData.BioTankStateDatas)
                {
                    biotankIndex++;

                    if (string.IsNullOrEmpty(data.Type))
                    {
                        data.Type = LocalizationManager.GetCurrentLanguageByID(3);
                    }

                    // 如果Name为空，生成默认名称（文本ID 3 + 序号）
                    if (string.IsNullOrEmpty(data.Name))
                    {
                        data.Name = BioTankCreationService.GenerateDefaultName(biotankIndex);
                    }

                    // 检查是否需要创建地形数据
                    string landFilePath = System.IO.Path.Combine(
                        GameConfig.SaveDirectoryPath,
                        $"{data.ID}_{SysDefine.DefaultSaveBioTankLandDataName}.gz"
                    );
                    bool needCreateIsland = !System.IO.File.Exists(landFilePath);

                    var bioTank = player.CreateBioTank(data, needCreateIsland);
                    if (bioTank != null)
                    {
                        createdCount++;

                        // 加载该生态箱的动物数据
                        if (data.Animals != null && data.Animals.Length > 0)
                        {
                            Debug.Log($"[GameInitializer] 加载生态箱 {data.ID} 的动物，数量: {data.Animals.Length}");
                            foreach (var animalData in data.Animals)
                            {
                                var animal = player.CreateAnimal(animalData, false);
                                if (animal != null)
                                {
                                    GameManager.Instance.StopList.Add(animal);
                                }
                                frameCounter++;
                                if (frameCounter >= 3)
                                {
                                    frameCounter = 0;
                                    yield return null;
                                }
                            }
                        }

                        // 加载该生态箱的植物数据
                        if (data.Plants != null && data.Plants.Length > 0)
                        {
                            Debug.Log($"[GameInitializer] 加载生态箱 {data.ID} 的植物，数量: {data.Plants.Length}");
                            foreach (var plantData in data.Plants)
                            {
                                var plant = player.CreatePlant(plantData);
                                if (plant != null)
                                {
                                    GameManager.Instance.StopList.Add(plant);
                                }
                                frameCounter++;
                                if (frameCounter >= 10)
                                {
                                    frameCounter = 0;
                                    yield return null;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"[GameInitializer] 创建生态缸失败!");
                    }
                }

                if (createdCount < saveData.BioTankStateDatas.Length)
                {
                    Debug.LogWarning($"[GameInitializer] 部分生态缸创建失败: {createdCount}/{saveData.BioTankStateDatas.Length}");
                }
            }
            else
            {
                Debug.LogWarning("[GameInitializer] 存档中没有生态缸数据!");
            }

            // 2. 初始化玩家数据
            if (saveData.PlayerStateData != null)
            {
                player.Init(saveData.PlayerStateData);
            }
            else
            {
                player.Init(new PlayerStateData { Money = 0 });
            }

            // 3. 标记地块加载完成
            GameManager.Instance.BlockLoadComplete = true;

            // 4. 加载仓库数据
            LoadWarehouseData(saveData.warehouseSaveData);

            // 5. 初始化设施数据
            if (saveData.unLockFacilitiesData != null)
            {
                FacilityManager.Instance.InitUnLockFacilityList(saveData.unLockFacilitiesData.UnLockFacilityList);
            }

            // 从生态箱内部加载设备数据
            if (saveData.BioTankStateDatas != null)
            {
                var allFacilities = new List<FacilitiesStateData>();
                foreach (var bioTankData in saveData.BioTankStateDatas)
                {
                    if (bioTankData.Facilities != null && bioTankData.Facilities.Length > 0)
                    {
                        allFacilities.AddRange(bioTankData.Facilities);
                    }
                }
                if (allFacilities.Count > 0)
                {
                    Debug.Log($"[GameInitializer] 加载设备数据，总数量: {allFacilities.Count}");
                    FacilityManager.Instance.InitBioTankFacilityList(allFacilities.ToArray());
                }
            }

            // 6. 初始化采集数据
            GameManager.Instance.gather.InitStateData(gatherSaveData);

            GameManager.Instance.LoadComplete = true;
        }

        /// <summary>
        /// 加载仓库数据
        /// </summary>
        private void LoadWarehouseData(WarehouseSaveData data)
        {
            if (data == null)
            {
                return;
            }

            // 加载鱼类
            if (data.fishStates != null)
            {
                foreach (var fish in data.fishStates)
                {
                    WarehouseManager.Instance.SaveAnimal(fish, fish.State == AnimalAgeStatus.Adult);
                }
            }

            // 加载植物
            if (data.plantStateDatas != null)
            {
                foreach (var plant in data.plantStateDatas)
                {
                    WarehouseManager.Instance.SavePlant(plant);
                }
            }

            // 加载装饰物
            if (data.decorationStateDatas != null)
            {
                foreach (var decoration in data.decorationStateDatas)
                {
                    WarehouseManager.Instance.SaveDecoration(decoration);
                }
            }

            // 加载方块
            if (data.cubeStateDatas != null)
            {
                foreach (var cube in data.cubeStateDatas)
                {
                    WarehouseManager.Instance.SaveCube(cube);
                }
            }
        }

        /// <summary>
        /// 创建默认存档数据（首次启动游戏）
        /// 流程：先从 InitialData 获取初始配置，然后从模板文件加载游戏数据
        /// </summary>
        private SaveData CreateDefaultSaveData()
        {
            try
            {
                // 1. 从 InitialData 获取初始配置（解锁、金钱等）
                var initialData = GameConfigDataBase.GetConfigData<InitialData>(1);
                if (initialData == null)
                {
                    Debug.LogError("[GameInitializer] 无法加载 InitialData 配置！");
                    return null;
                }

                // 2. 从模板文件加载游戏数据（动植物、生态箱等）
                SaveData saveData = _saveSystem.LoadSaveDataFromPath(GameConfig.FirstStartSaveDataPath);
                if (saveData == null)
                {
                    Debug.LogError("[GameInitializer] 无法加载模板存档文件！");
                    return null;
                }

                // 2.5 检查模板是否是旧格式，如果是则迁移到新格式
                if (saveData.BioTankStateDatas != null && saveData.BioTankStateDatas.Length > 0)
                {
                    bool hasGlobalAnimals = saveData.fishStates != null && saveData.fishStates.Length > 0;
                    bool hasGlobalPlants = saveData.plantStateDatas != null && saveData.plantStateDatas.Length > 0;

                    if (hasGlobalAnimals || hasGlobalPlants)
                    {
                        Debug.Log("[GameInitializer] 模板存档是旧格式，开始迁移到第一个生态箱...");

                        // 将所有全局数据迁移到第一个生态箱
                        var firstBioTank = saveData.BioTankStateDatas[0];

                        // 迁移动物数据到第一个生态箱
                        if (saveData.fishStates != null && saveData.fishStates.Length > 0)
                        {
                            // 更新所有动物的 ID 为第一个生态箱的 ID
                            foreach (var animal in saveData.fishStates)
                            {
                                animal.ID = firstBioTank.ID;
                            }
                            firstBioTank.Animals = saveData.fishStates;
                            Debug.Log($"[GameInitializer] 模板迁移 - 第一个生态箱获得 {saveData.fishStates.Length} 条动物");
                        }

                        // 迁移植物数据到第一个生态箱
                        if (saveData.plantStateDatas != null && saveData.plantStateDatas.Length > 0)
                        {
                            // 更新所有植物的 ID 为第一个生态箱的 ID
                            foreach (var plant in saveData.plantStateDatas)
                            {
                                plant.ID = firstBioTank.ID;
                            }
                            firstBioTank.Plants = saveData.plantStateDatas;
                            Debug.Log($"[GameInitializer] 模板迁移 - 第一个生态箱获得 {saveData.plantStateDatas.Length} 个植物");
                        }

                        // 迁移设备数据到第一个生态箱
                        if (saveData.facilitiesStateDatas != null && saveData.facilitiesStateDatas.Length > 0)
                        {
                            // 更新所有设备的 ObjID 为第一个生态箱的 ID
                            foreach (var facility in saveData.facilitiesStateDatas)
                            {
                                facility.ObjID = firstBioTank.ID;
                            }
                            firstBioTank.Facilities = saveData.facilitiesStateDatas;
                        }

                        // 清空全局数组
                        saveData.fishStates = new AnimalStateData[0];
                        saveData.plantStateDatas = new PlantStateData[0];
                        Debug.Log("[GameInitializer] 模板存档迁移完成");
                    }
                }

                // 3. 复制模板的地形文件
                if (saveData.BioTankStateDatas != null && saveData.BioTankStateDatas.Length > 0)
                {
                    foreach (var bioTank in saveData.BioTankStateDatas)
                    {
                        TryCopyTemplateLandData(bioTank.ID);
                    }
                }

                // 4. 应用 InitialData 的配置覆盖模板中的部分数据
                // 玩家金钱使用 InitialData 配置
                saveData.PlayerStateData = new PlayerStateData { Money = initialData.Money };

                // 设施解锁使用 InitialData 配置
                saveData.unLockFacilitiesData = new UnLockFacilitiesData
                {
                    UnLockFacilityList = new List<int>()
                };
                if (initialData.Facilities != null)
                {
                    foreach (var facilityIdStr in initialData.Facilities)
                    {
                        if (!string.IsNullOrEmpty(facilityIdStr) && int.TryParse(facilityIdStr, out int facilityId))
                        {
                            saveData.unLockFacilitiesData.UnLockFacilityList.Add(facilityId);
                        }
                    }
                }

                // 仓库数据从模板文件读取，如果模板中没有则初始化为空
                if (saveData.warehouseSaveData == null)
                {
                    saveData.warehouseSaveData = new WarehouseSaveData
                    {
                        fishStates = new AnimalStateData[0],
                        plantStateDatas = new PlantStateData[0],
                        decorationStateDatas = new DecorationStateData[0],
                        cubeStateDatas = new CubeStateData[0]
                    };
                }

                // 装饰物数据，如果模板中没有则初始化为空
                if (saveData.decorationStateDatas == null)
                {
                    saveData.decorationStateDatas = new DecorationStateData[0];
                }

                // 设施状态数据，首次启动时为空
                if (saveData.facilitiesStateDatas == null)
                {
                    saveData.facilitiesStateDatas = new FacilitiesStateData[0];
                }

                // 引导数据，首次启动时为空
                saveData.tutorialSaveData = new TutorialSaveData();

                return saveData;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameInitializer] 创建默认存档失败: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 启动隐藏进程
        /// </summary>
        private IEnumerator LaunchHiddenProcesses()
        {
            int bioTankCount = BioTankRegistry.Instance.Count;

            if (bioTankCount == 0)
            {
                Debug.LogWarning("[GameInitializer] 没有生态缸，无法启动客户端进程！");
                yield break;
            }

            // 为每个生态箱启动一个客户端进程，服务端会按顺序分配ID
            for (int i = 0; i < bioTankCount; i++)
            {
                HiddenProcessLauncher.Instance.LaunchHiddenProcess();
                yield return new WaitForSeconds(0.1f);
            }

            // 初始化预加载客户端池并启动休眠客户端
            Debug.Log("[GameInitializer] 初始化预加载客户端池...");
            PreloadClientPoolManager.Instance.Initialize();
            yield return PreloadClientPoolManager.Instance.LaunchInitialDormantClients();

            Debug.Log("[GameInitializer] 所有客户端进程启动完成");
        }

        /// <summary>
        /// 尝试从模板目录复制地形数据文件
        /// </summary>
        private void TryCopyTemplateLandData(int bioTankId)
        {
            try
            {
                // 可能的模板地形文件路径（尝试多种格式）
                string templateDir = System.IO.Path.Combine(
                    Application.streamingAssetsPath,
                    SysDefine.DefaultFirstSaveData
                );

                string[] possibleTemplateFiles = new string[]
                {
                    System.IO.Path.Combine(templateDir, "BioTank.txt"),
                    System.IO.Path.Combine(templateDir, "0_LandSaveData.txt"),
                    System.IO.Path.Combine(templateDir, "0_LandSaveData.gz"),
                    System.IO.Path.Combine(templateDir, "LandSaveData.txt"),
                    System.IO.Path.Combine(templateDir, "LandSaveData.gz")
                };

                // 目标地形文件路径
                string targetPath = System.IO.Path.Combine(
                    GameConfig.SaveDirectoryPath,
                    $"{bioTankId}_{SysDefine.DefaultSaveBioTankLandDataName}.gz"
                );

                // 确保目标目录存在
                System.IO.Directory.CreateDirectory(GameConfig.SaveDirectoryPath);

                // 尝试复制任何存在的模板文件
                foreach (var templatePath in possibleTemplateFiles)
                {
                    if (System.IO.File.Exists(templatePath))
                    {
                        System.IO.File.Copy(templatePath, targetPath, true);
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameInitializer] 复制模板地形文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清除所有游戏数据
        /// </summary>
        private void ClearAllData()
        {
            WarehouseManager.Instance.Destroy();
            GameManager.Instance.BlockLoadComplete = false;
            PlayerManager.Instance.ClearAllBiotanks();
        }
}
