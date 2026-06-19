using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 布局费用计算结果
/// </summary>
public class LayoutCostResult
{
    /// <summary>
    /// Cube总费用
    /// </summary>
    public int CubeCost { get; set; }

    /// <summary>
    /// 装饰物总费用
    /// </summary>
    public int DecorationCost { get; set; }

    /// <summary>
    /// 植物总费用（出售价格）
    /// </summary>
    public int PlantCost { get; set; }

    /// <summary>
    /// 布局内容总费用
    /// </summary>
    public int TotalCost => CubeCost + DecorationCost + PlantCost;

    /// <summary>
    /// Cube数量
    /// </summary>
    public int CubeCount { get; set; }

    /// <summary>
    /// 装饰物数量
    /// </summary>
    public int DecorationCount { get; set; }

    /// <summary>
    /// 植物数量
    /// </summary>
    public int PlantCount { get; set; }
}

/// <summary>
/// 用于解析terrainData中的地形JSON结构
/// </summary>
[System.Serializable]
public class TerrainDataWrapper
{
    public TerrainIslandData Terrain;
}

[System.Serializable]
public class TerrainIslandData
{
    public Vector3 Pos;
    public int ID;
    public int xMax;
    public int yMax;
    public int zMax;
    public List<TerrainModification> Modifications;
}

[System.Serializable]
public class TerrainModification
{
    public int _posX;
    public int _posY;
    public int _posZ;
    public int _addType;
    public string _name;
    public Vector3 _addAngle = Vector3.zero;
    public Vector3 _localScale = Vector3.one;
    public TerrainModificationOtherData _otherData;
}

[System.Serializable]
public class TerrainModificationOtherData
{
    // 空类，用于保持JSON结构完整
}

/// <summary>
/// 生态箱初始布局管理器 - 负责根据尺寸加载对应的初始布局配置
/// 目录结构: StreamingAssets/TankInitialLayouts/*.json
/// 通过读取文件内的sizeX/Y/Z字段来匹配对应尺寸的布局
/// </summary>
public class TankInitialLayoutManager
{
    private static TankInitialLayoutManager _instance;
    public static TankInitialLayoutManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TankInitialLayoutManager();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 初始布局配置根目录
    /// </summary>
    private const string LAYOUT_ROOT_PATH = "TankInitialLayouts";

    /// <summary>
    /// 所有布局缓存
    /// </summary>
    private List<TankInitialLayoutData> _allLayouts = new List<TankInitialLayoutData>();

    /// <summary>
    /// 按尺寸分组的布局缓存 (key: "X_Y_Z")
    /// </summary>
    private Dictionary<string, List<TankInitialLayoutData>> _layoutsBySize = new Dictionary<string, List<TankInitialLayoutData>>();

    private bool _initialized = false;

    private TankInitialLayoutManager() { }

    /// <summary>
    /// 初始化并加载所有布局配置
    /// </summary>
    public void Initialize()
    {
        if (_initialized) return;

        LoadAllLayouts();
        _initialized = true;
    }

    /// <summary>
    /// 加载所有初始布局配置
    /// </summary>
    private void LoadAllLayouts()
    {
        _allLayouts.Clear();
        _layoutsBySize.Clear();

        string rootPath = Path.Combine(Application.streamingAssetsPath, LAYOUT_ROOT_PATH);

        if (!Directory.Exists(rootPath))
        {
            Debug.Log($"[TankInitialLayoutManager] 初始布局配置目录不存在: {rootPath}");
            return;
        }

        // 遍历目录下所有JSON文件
        var layoutFiles = Directory.GetFiles(rootPath, "*.json");
        foreach (var layoutFile in layoutFiles)
        {
            try
            {
                string json = File.ReadAllText(layoutFile);
                var layout = JsonUtility.FromJson<TankInitialLayoutData>(json);

                if (layout != null && !string.IsNullOrEmpty(layout.terrainData))
                {
                    _allLayouts.Add(layout);

                    // 按尺寸分组
                    string sizeKey = GetSizeKey(layout.sizeX, layout.sizeY, layout.sizeZ);
                    if (!_layoutsBySize.ContainsKey(sizeKey))
                    {
                        _layoutsBySize[sizeKey] = new List<TankInitialLayoutData>();
                    }
                    _layoutsBySize[sizeKey].Add(layout);

                    // 解析terrainData统计各类型数量
                    int cubeCount = 0;
                    int plantCount = 0;
                    int decoCount = 0;
                    try
                    {
                        var terrainWrapper = JsonUtility.FromJson<TerrainDataWrapper>(layout.terrainData);
                        if (terrainWrapper?.Terrain?.Modifications != null)
                        {
                            foreach (var mod in terrainWrapper.Terrain.Modifications)
                            {
                                if (string.IsNullOrEmpty(mod._name))
                                    continue;

                                if (int.TryParse(mod._name, out int typeId))
                                {
                                    if (typeId >= 2000 && typeId < 3000)
                                        plantCount++;
                                    else if (typeId >= 3000 && typeId < 4000)
                                        cubeCount++;
                                    else if (typeId >= 4000 && typeId < 5000)
                                        decoCount++;
                                }
                            }
                        }
                    }
                    catch { }

                    Debug.Log($"[TankInitialLayoutManager] 加载布局: {layout.layoutId} - {layout.layoutName} (尺寸: {layout.sizeX}x{layout.sizeY}x{layout.sizeZ}, Cube: {cubeCount}, 植物: {plantCount}, 装饰物: {decoCount})");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TankInitialLayoutManager] 加载布局文件失败: {layoutFile}, {e.Message}");
            }
        }

        // 对每个尺寸的布局列表按sortOrder排序（数值越小越靠前）
        foreach (var kvp in _layoutsBySize)
        {
            kvp.Value.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
        }

        Debug.Log($"[TankInitialLayoutManager] 共加载 {_allLayouts.Count} 个初始布局配置");
    }

    /// <summary>
    /// 获取指定尺寸的所有布局
    /// </summary>
    public List<TankInitialLayoutData> GetLayoutsForSize(int sizeX, int sizeY, int sizeZ)
    {
        Initialize();

        string sizeKey = GetSizeKey(sizeX, sizeY, sizeZ);
        if (_layoutsBySize.TryGetValue(sizeKey, out var layouts))
        {
            return layouts;
        }
        return new List<TankInitialLayoutData>();
    }

    /// <summary>
    /// 获取指定尺寸的第一个布局
    /// </summary>
    public TankInitialLayoutData GetFirstLayout(int sizeX, int sizeY, int sizeZ)
    {
        var layouts = GetLayoutsForSize(sizeX, sizeY, sizeZ);
        return layouts.FirstOrDefault();
    }

    /// <summary>
    /// 根据布局ID获取布局
    /// </summary>
    public TankInitialLayoutData GetLayoutById(string layoutId)
    {
        Initialize();
        return _allLayouts.FirstOrDefault(l => l.layoutId == layoutId);
    }

    /// <summary>
    /// 应用初始布局到新生态箱（使用第一个布局）
    /// </summary>
    public bool ApplyInitialLayout(int bioTankId, int sizeX, int sizeY, int sizeZ)
    {
        var layout = GetFirstLayout(sizeX, sizeY, sizeZ);
        return ApplyLayout(bioTankId, layout);
    }

    /// <summary>
    /// 应用指定布局到新生态箱
    /// </summary>
    public bool ApplyLayoutById(int bioTankId, string layoutId)
    {
        var layout = GetLayoutById(layoutId);
        return ApplyLayout(bioTankId, layout);
    }

    /// <summary>
    /// 应用布局数据到生态箱
    /// 所有内容（地形、植物、装饰物）都存储在terrainData中
    /// 新建时植物会被转换为初始生长阶段（阶段0），并创建对应的PlantStateData（成长度为0）
    /// </summary>
    private bool ApplyLayout(int bioTankId, TankInitialLayoutData layout)
    {
        if (layout == null || string.IsNullOrEmpty(layout.terrainData))
        {
            Debug.Log($"[TankInitialLayoutManager] 没有可用的初始布局");
            return false;
        }

        try
        {
            // 将植物转换为初始生长阶段后保存
            string processedTerrainData = ConvertPlantsToStage(layout.terrainData, 0);
            ApplyTerrainData(bioTankId, processedTerrainData);

            // 从terrainData中解析植物数据并创建Plant实体
            CreatePlantsFromTerrainData(bioTankId, processedTerrainData);

            Debug.Log($"[TankInitialLayoutManager] 成功应用布局 '{layout.layoutName}' 到生态箱 {bioTankId}（植物已转换为初始阶段）");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TankInitialLayoutManager] 应用初始布局失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从terrainData中解析植物并创建Plant实体
    /// 植物格式：纯植物ID（如2001）
    /// 植物的成长度(GrowthProgress)设置为0
    /// </summary>
    private void CreatePlantsFromTerrainData(int bioTankId, string terrainData)
    {
        if (string.IsNullOrEmpty(terrainData))
            return;

        try
        {
            var terrainWrapper = JsonUtility.FromJson<TerrainDataWrapper>(terrainData);
            if (terrainWrapper?.Terrain?.Modifications == null)
                return;

            int plantCount = 0;
            foreach (var mod in terrainWrapper.Terrain.Modifications)
            {
                if (string.IsNullOrEmpty(mod._name))
                    continue;

                // 解析植物ID（纯数字格式，如2001）
                if (!int.TryParse(mod._name, out int plantTypeId))
                    continue;

                // 检查是否是有效的植物ID（2xxx范围）
                if (plantTypeId >= 2000 && plantTypeId < 3000)
                {
                    // 计算植物在世界中的位置（使用与客户端相同的坐标转换）
                    float scale = 1f / 20f;
                    Vector3 position = new Vector3(
                        mod._posX * scale,
                        mod._posZ * scale,  // Y和Z交换
                        mod._posY * scale
                    );

                    // 创建PlantStateData，成长度设置为0
                    var plantStateData = new PlantStateData
                    {
                        ID = bioTankId,
                        ObjID = IDFactory.GetUniqueID(),
                        Type = plantTypeId.ToString(),
                        Pos = position,
                        GrowthProgress = 0f,  // 成长度为0
                        HP = 100f,
                        AsDeath = false
                    };

                    // 创建植物实体
                    var plant = PlayerManager.Instance.CreatePlant(plantStateData);
                    if (plant != null)
                    {
                        plantCount++;
                    }
                }
            }

            if (plantCount > 0)
            {
                Debug.Log($"[TankInitialLayoutManager] 从布局中创建了 {plantCount} 个植物（成长度为0）");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[TankInitialLayoutManager] 从terrainData创建植物失败: {e.Message}");
        }
    }

    /// <summary>
    /// 应用地形数据
    /// </summary>
    private void ApplyTerrainData(int bioTankId, string terrainData)
    {
        // 目标路径：Save目录下的地形数据文件
        string targetPath = Path.Combine(
            Application.streamingAssetsPath,
            SysDefine.DefaultSavePath,
            $"{bioTankId}_{SysDefine.DefaultSaveBioTankLandDataName}");

        // 确保目标目录存在
        string targetDir = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // 保存地形数据为压缩格式
        SaveCompression.SaveCompressed(targetPath, terrainData);

        Debug.Log($"[TankInitialLayoutManager] 已应用地形数据到生态箱 {bioTankId}");
    }

    /// <summary>
    /// 生成尺寸键值
    /// </summary>
    private string GetSizeKey(int x, int y, int z)
    {
        return $"{x}_{y}_{z}";
    }

    #region 费用计算

    /// <summary>
    /// 计算布局的内容费用（cube + 装饰物 + 植物）
    /// 所有内容都从terrainData的Modifications中读取
    /// - Cube: _name为3xxx（如3001, 3002）
    /// - 植物: _name为2xxx（如2001, 2002）
    /// - 装饰物: _name为4xxx（如4001, 4002）
    /// </summary>
    /// <param name="layout">布局数据，如果为null则返回零费用</param>
    /// <returns>费用计算结果</returns>
    public LayoutCostResult CalculateLayoutCost(TankInitialLayoutData layout)
    {
        var result = new LayoutCostResult();

        if (layout == null)
        {
            return result;
        }

        // 从terrainData中读取所有内容（Cube、植物、装饰物）
        if (!string.IsNullOrEmpty(layout.terrainData))
        {
            try
            {
                var terrainWrapper = JsonUtility.FromJson<TerrainDataWrapper>(layout.terrainData);
                if (terrainWrapper?.Terrain?.Modifications != null)
                {
                    foreach (var mod in terrainWrapper.Terrain.Modifications)
                    {
                        if (string.IsNullOrEmpty(mod._name))
                            continue;

                        // 解析_name获取类型ID（纯数字格式）
                        if (!int.TryParse(mod._name, out int typeId))
                            continue;

                        // 根据ID范围判断类型
                        if (typeId >= 2000 && typeId < 3000)
                        {
                            // 植物
                            var plantData = GameConfigDataBase.GetConfigData<PlantData>(typeId, SysDefine.PlantConfigData);
                            if (plantData != null)
                            {
                                result.PlantCost += plantData.Price;
                                result.PlantCount++;
                            }
                        }
                        else if (typeId >= 3000 && typeId < 4000)
                        {
                            // Cube
                            var cubeData = GameConfigDataBase.GetConfigData<CubeData>(typeId, SysDefine.CubeConfigData);
                            if (cubeData != null)
                            {
                                result.CubeCost += cubeData.Price;
                                result.CubeCount++;
                            }
                        }
                        else if (typeId >= 4000 && typeId < 5000)
                        {
                            // 装饰物
                            var decoData = GameConfigDataBase.GetConfigData<DecorationData>(typeId, SysDefine.DecorationConfigData);
                            if (decoData != null)
                            {
                                result.DecorationCost += decoData.Price;
                                result.DecorationCount++;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TankInitialLayoutManager] 解析地形数据失败: {e.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// 根据布局ID计算费用
    /// </summary>
    public LayoutCostResult CalculateLayoutCostById(string layoutId)
    {
        var layout = GetLayoutById(layoutId);
        return CalculateLayoutCost(layout);
    }

    #endregion

    #region 预览数据处理

    /// <summary>
    /// 植物的最终生长阶段（用于预览）
    /// </summary>
    private const int PLANT_FINAL_GROWTH_STAGE = 3;

    /// <summary>
    /// 植物的初始生长阶段（用于新建生态箱）
    /// </summary>
    private const int PLANT_INITIAL_GROWTH_STAGE = 0;

    /// <summary>
    /// 获取预览用的布局数据
    /// 与原始数据的区别：所有植物都显示为最终生长阶段
    /// </summary>
    /// <param name="layoutId">布局ID</param>
    /// <returns>处理后的terrainData，如果布局不存在则返回null</returns>
    public string GetPreviewTerrainData(string layoutId)
    {
        var layout = GetLayoutById(layoutId);
        if (layout == null || string.IsNullOrEmpty(layout.terrainData))
        {
            return null;
        }

        return ConvertPlantsToStage(layout.terrainData, PLANT_FINAL_GROWTH_STAGE);
    }

    /// <summary>
    /// 获取新建生态箱用的布局数据
    /// 与原始数据的区别：所有植物都设置为初始生长阶段
    /// </summary>
    /// <param name="layoutId">布局ID</param>
    /// <returns>处理后的terrainData，如果布局不存在则返回null</returns>
    public string GetNewTankTerrainData(string layoutId)
    {
        var layout = GetLayoutById(layoutId);
        if (layout == null || string.IsNullOrEmpty(layout.terrainData))
        {
            return null;
        }

        return ConvertPlantsToStage(layout.terrainData, PLANT_INITIAL_GROWTH_STAGE);
    }

    /// <summary>
    /// 将terrainData中的所有植物转换为最终生长阶段
    /// </summary>
    [System.Obsolete("使用 ConvertPlantsToStage 代替")]
    public string ConvertPlantsToFinalStage(string terrainData)
    {
        return ConvertPlantsToStage(terrainData, PLANT_FINAL_GROWTH_STAGE);
    }

    /// <summary>
    /// 将terrainData中的所有植物添加生长阶段后缀
    /// 输入格式：纯植物ID（如2001）
    /// 输出格式：植物ID_阶段（如2001_3）
    /// </summary>
    /// <param name="terrainData">原始地形数据</param>
    /// <param name="targetStage">目标生长阶段（0-3）</param>
    /// <returns>处理后的地形数据</returns>
    public string ConvertPlantsToStage(string terrainData, int targetStage)
    {
        if (string.IsNullOrEmpty(terrainData))
        {
            return terrainData;
        }

        try
        {
            var terrainWrapper = JsonUtility.FromJson<TerrainDataWrapper>(terrainData);
            if (terrainWrapper?.Terrain?.Modifications == null)
            {
                return terrainData;
            }

            bool hasChanges = false;
            foreach (var mod in terrainWrapper.Terrain.Modifications)
            {
                if (string.IsNullOrEmpty(mod._name))
                    continue;

                // 解析植物ID（纯数字格式）
                if (int.TryParse(mod._name, out int typeId))
                {
                    // 检查是否是植物ID（2xxx范围）
                    if (typeId >= 2000 && typeId < 3000)
                    {
                        mod._name = $"{typeId}_{targetStage}";
                        hasChanges = true;
                    }
                }
            }

            if (hasChanges)
            {
                // 重新序列化
                return JsonUtility.ToJson(terrainWrapper);
            }

            return terrainData;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[TankInitialLayoutManager] 转换植物生长阶段失败: {e.Message}");
            return terrainData;
        }
    }

    #endregion
}
