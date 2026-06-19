using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 引导配置管理器
/// 负责管理和加载所有引导配置
/// </summary>
public class TutorialConfigManager : MonoBehaviour
{
    private static TutorialConfigManager instance;
    public static TutorialConfigManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TutorialConfigManager");
                instance = go.AddComponent<TutorialConfigManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("引导配置列表")]
    [SerializeField] private List<TutorialConfig> tutorialConfigs = new List<TutorialConfig>();

    private Dictionary<string, TutorialConfig> configDict = new Dictionary<string, TutorialConfig>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialize()
    {
        configDict.Clear();

        foreach (var config in tutorialConfigs)
        {
            if (config != null && !string.IsNullOrEmpty(config.flowID))
            {
                configDict[config.flowID] = config;
            }
        }

        Debug.Log($"[Tutorial] 加载了 {configDict.Count} 个引导配置");
    }

    /// <summary>
    /// 获取引导配置
    /// </summary>
    public TutorialConfig GetConfig(string flowID)
    {
        if (configDict.TryGetValue(flowID, out TutorialConfig config))
        {
            return config;
        }

        Debug.LogWarning($"[Tutorial] 未找到引导配置: {flowID}");
        return null;
    }

    /// <summary>
    /// 根据配置ID启动引导
    /// </summary>
    public void StartTutorialByConfig(string flowID)
    {
        TutorialConfig config = GetConfig(flowID);
        if (config != null)
        {
            TutorialFlow flow = config.CreateFlow();
            TutorialManager.Instance.StartTutorial(flow);
        }
    }

    /// <summary>
    /// 获取所有配置
    /// </summary>
    public List<TutorialConfig> GetAllConfigs()
    {
        return new List<TutorialConfig>(tutorialConfigs);
    }

    /// <summary>
    /// 添加配置（运行时）
    /// </summary>
    public void AddConfig(TutorialConfig config)
    {
        if (config != null && !string.IsNullOrEmpty(config.flowID))
        {
            if (!configDict.ContainsKey(config.flowID))
            {
                tutorialConfigs.Add(config);
                configDict[config.flowID] = config;
                Debug.Log($"[Tutorial] 添加引导配置: {config.flowID}");
            }
        }
    }
}
