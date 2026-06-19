



using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceManager
{
    private static Dictionary<string, Dictionary<string, string>> Map = new Dictionary<string, Dictionary<string, string>>();
    private static string[] configFiles = { "/ConfigMap.txt", "/ConfigAssets.txt", "/ConfigCsv.txt" };
    private static Dictionary<string, UnityEngine.Object> cache = new Dictionary<string, UnityEngine.Object>();
    private string modsPath;
    // 静态构造函数
    static ResourceManager()
    {
        // 初始化配置映射
        foreach (string configFile in configFiles)
        {
            string fileContent = GetConfigFile(configFile);
            if (!string.IsNullOrEmpty(fileContent))
            {
                Dictionary<string, string> configMap = new Dictionary<string, string>();
                Reader(fileContent, line =>
                {
                    string[] keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        configMap[keyValue[0].Trim()] = keyValue[1].Trim();
                    }
                });
                Map[configFile] = configMap;
            }
            else
            {
                Debug.LogError($"[ResourceManager] 无法加载配置文件：{configFile}");
            }
        }
    }
    // 根据平台读取配置文件
    public static string GetConfigFile(string filename)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string filePath = Application.streamingAssetsPath + filename;
        UnityWebRequest request = UnityWebRequest.Get(filePath);
        request.SendWebRequest();

        while (!request.isDone)
        {
            // 等待请求完成
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[ResourceManager] 加载配置文件失败：{filename}\n错误信息：{request.error}");
            return null;
        }
        else
        {
            return request.downloadHandler.text;
        }
#else
        string configPath = Path.Combine(Application.streamingAssetsPath, "ConfigData");
        string filePath = Path.Combine(configPath, filename);
        filePath = filePath.Replace(@"\", "/");
        //string filePath = Application.streamingAssetsPath + filename;
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogError($"[ResourceManager] 配置文件不存在：{filePath}");
            return null;
        }
#endif
    }

    // 读取文件内容并处理每一行
    private static void Reader(string fileContent, Action<string> handler)
    {
        using (StringReader reader = new StringReader(fileContent))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                handler(line);
            }
        }
    }

    //#region 加载纹理

    public async Task<Texture2D> GetTextureAsync(string fileName)
    {
        string filePath = Path.Combine(modsPath, "Textures", fileName);
        var loder = TextureLoaderPool.Instance.GetLoader();
        return await loder.LoadTextureAsync(filePath);
    }

    //#endregion

    //#region 加载数据文件

    //public async Task<T> LoadJsonDataAsync<T>(string fileName)
    //{
    //    string filePath = PathName.Combine(modsPath, "Data", fileName);
    //    //return await DataLoader.LoadJsonDataAsync<T>(filePath);
    //}
    // 加载资源的方法
    public static T Load<T>(string prefabName) where T : UnityEngine.Object
    {
        if (cache.ContainsKey(prefabName))
        {
            return cache[prefabName] as T;
        }
        

        
        foreach (var mapEntry in Map)
        {
            if (mapEntry.Value.ContainsKey(prefabName))
            {
                string prefabPath = mapEntry.Value[prefabName];
                T asset = Resources.Load<T>(prefabPath);
                if (asset != null)
                {
                    cache[prefabName] = asset;
                    return asset;
                }
                else
                {
                    Debug.LogError($"[ResourceManager] 无法加载资源，路径：{prefabPath}");
                    return null;
                }
            }
        }

        Debug.LogError($"[ResourceManager] 未找到资源：{prefabName}");
        return null;
    }
}

/*
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    private string modsPath;
    private TextureLoader textureLoader;
    private DataLoader dataLoader;
    private ModelLoader modelLoader;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化 Mods 路径
            modsPath = PathName.Combine(Application.dataPath, "../Mods");
            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
            }

            // 初始化加载器
            textureLoader = new TextureLoader();
            dataLoader = new DataLoader();
            modelLoader = new ModelLoader();
        }
        else
        {
            Destroy(gameObject);
        }
    }



    #region 加载模型

    public IEnumerator LoadModelAsync(string fileName, System.Action<GameObject> callback)
    {
        string filePath = PathName.Combine(modsPath, "Models", fileName);
        yield return modelLoader.LoadModelAsync(filePath, callback);
    }

    #endregion
}

using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class ModTest : MonoBehaviour
{
    async void Start()
    {
        // 异步加载并应用用户纹理
        Texture2D userTexture = await ResourceManager.Instance.GetTextureAsync("UserTexture.png");
        if (userTexture != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = userTexture;
        }

        // 异步加载用户配置数据
        GameConfig config = await ResourceManager.Instance.LoadJsonDataAsync<GameConfig>("GameConfig.json");
        if (config != null)
        {
        }

        // 异步加载用户模型
        StartCoroutine(ResourceManager.Instance.LoadModelAsync("UserModel.obj", OnModelLoaded));
    }

    void OnModelLoaded(GameObject model)
    {
        if (model != null)
        {
            model.transform.position = new Vector3(0, 0, 0);
        }
    }
}


*/