using UnityEngine;
using System.IO;

/// <summary>
/// 游戏配置管理类 - 统一管理所有路径和配置
/// </summary>
public static class GameConfig
{
        #region 路径配置

        /// <summary>
        /// 获取存档目录完整路径
        /// </summary>
        public static string SaveDirectoryPath => Path.Combine(Application.streamingAssetsPath, SysDefine.DefaultSavePath);

        /// <summary>
        /// 获取主存档文件完整路径
        /// </summary>
        public static string SaveDataPath => Path.Combine(SaveDirectoryPath, SysDefine.DefaultSaveDataName);

        /// <summary>
        /// 获取设置存档文件完整路径
        /// </summary>
        public static string SettingDataPath => Path.Combine(SaveDirectoryPath, SysDefine.DefaultSettingDataName);

        /// <summary>
        /// 获取采集数据文件完整路径
        /// </summary>
        public static string GatherSaveDataPath => Path.Combine(SaveDirectoryPath, SysDefine.DefaultGatherSaveDataName);

        /// <summary>
        /// 获取蓝图目录路径
        /// </summary>
        public static string BluePrintDirectoryPath => Path.Combine(Application.streamingAssetsPath, SysDefine.BluePrintMainPath);

        /// <summary>
        /// 获取蓝图图标目录路径
        /// </summary>
        public static string BluePrintIconPath => Path.Combine(Application.streamingAssetsPath, SysDefine.BluePrintIconPath);

        /// <summary>
        /// 获取游戏根目录路径（服务器和客户端的父目录）
        /// 打包后结构: GameRoot/Server/, GameRoot/Client/, GameRoot/Mods/
        /// </summary>
        public static string GameRootPath
        {
            get
            {
                // Application.dataPath 在打包后指向 Server_Data 或 Client_Data 目录
                // 需要向上两级获取游戏根目录
                // 例如: C:/Game/Server/Server_Data -> C:/Game/
                string dataPath = Application.dataPath;
                string appFolder = Directory.GetParent(dataPath)?.FullName;  // Server 或 Client 目录
                string gameRoot = Directory.GetParent(appFolder)?.FullName;   // 游戏根目录
                return gameRoot ?? appFolder ?? dataPath;
            }
        }

        /// <summary>
        /// 获取 Mod 目录路径（位于游戏根目录下，与服务器/客户端同级）
        /// </summary>
        public static string ModsDirectoryPath => Path.Combine(GameRootPath, "Mods");

        /// <summary>
        /// 获取 Mod 用户设置文件路径（保存在存档目录下，支持云存档同步）
        /// </summary>
        public static string ModSettingsPath => Path.Combine(SaveDirectoryPath, "ModSettings.json");

        /// <summary>
        /// 获取首次启动默认存档路径
        /// </summary>
        public static string FirstStartSaveDataPath => Path.Combine(
            Application.streamingAssetsPath,
            SysDefine.DefaultFirstSaveData,
            SysDefine.DefaultSaveDataName
        );

        #endregion

        #region 游戏设置

        /// <summary>
        /// 自动存档间隔（秒）
        /// </summary>
        public const float AutoSaveInterval = 60f;

        /// <summary>
        /// 生态缸更新间隔（秒）
        /// </summary>
        public const float BioTankUpdateInterval = 1f;

        /// <summary>
        /// 默认窗口大小
        /// </summary>
        public static Vector2Int DefaultWindowSize => SysDefine.DefaultWindowSize;

        #endregion

        #region 价格系数

        /// <summary>
        /// 获取品质对应的价格系数
        /// </summary>
        public static float GetQualityPriceMagnification(ObjectQuality quality)
        {
            return quality switch
            {
                ObjectQuality.White => SysDefine.Price_Magnification_White,
                ObjectQuality.Blue => SysDefine.Price_Magnification_Blue,
                ObjectQuality.Purple => SysDefine.Price_Magnification_Purple,
                ObjectQuality.Gold => SysDefine.Price_Magnification_Gold,
                _ => 1f
            };
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 构建生态缸存档路径
        /// </summary>
        public static string GetBioTankSavePath(int bioTankId)
        {
            string fileName = $"{bioTankId}_{SysDefine.DefaultSaveBioTankDataName}";
            return Path.Combine(SaveDirectoryPath, fileName);
        }

        /// <summary>
        /// 构建蓝图存档路径
        /// </summary>
        public static string GetBluePrintDataPath(string bluePrintName)
        {
            return Path.Combine(BluePrintDirectoryPath, SysDefine.BluePrintDataPath, $"{bluePrintName}.txt");
        }

        /// <summary>
        /// 构建蓝图图标路径
        /// </summary>
        public static string GetBluePrintIconPath(string iconName)
        {
            return Path.Combine(BluePrintIconPath, iconName);
        }

        #endregion
}
