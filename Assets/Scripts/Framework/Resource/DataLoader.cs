using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 游戏配置数据类 - 用于JSON序列化
/// 注意：这是数据类，不要与静态GameConfig配置管理类混淆
/// </summary>
[System.Serializable]
public class GameConfigData
{
    public string playerName;
    public int maxScore;
    // 添加更多字段
}


public class DataLoader
{
    public async Task<T> LoadJsonDataAsync<T>(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                string jsonData = await ReadTextFileAsync(filePath);
                T data = JsonUtility.FromJson<T>(jsonData);
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Exception while loading data: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Data file not found: " + filePath);
        }
        return default(T);
    }

    private Task<string> ReadTextFileAsync(string filePath)
    {
        return Task.Run(() => File.ReadAllText(filePath));
    }
}
/*
 * 使用示例:
 * string configPath = Path.Combine(modsPath, "Data", "GameConfigData.json");
 * DataLoader dataLoader = new DataLoader();
 * GameConfigData config = await dataLoader.LoadJsonDataAsync<GameConfigData>(configPath);
 * if (config != null)
 * {
 * }
 */
