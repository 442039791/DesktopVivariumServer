using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 引导存档管理器
/// 负责保存和加载引导进度（通过服务器的通用存档系统）
/// </summary>
public class TutorialSaveManager : MonoBehaviour
{
    private static TutorialSaveManager instance;
    public static TutorialSaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TutorialSaveManager");
                instance = go.AddComponent<TutorialSaveManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private TutorialSaveData saveData;
    private GameManager gameManager;

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
        // 获取GameManager引用
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogWarning("[Tutorial] GameManager未找到，引导存档功能将在GameManager加载后可用");
            saveData = new TutorialSaveData();
        }
        else
        {
            LoadProgress();
        }
    }

    /// <summary>
    /// 从通用存档系统加载引导进度
    /// </summary>
    private void LoadProgress()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager != null && gameManager.SaveSystem != null)
        {
            // 引导数据会在GameManager加载游戏时一起加载
            // 这里只是初始化，实际数据会在LoadFromSaveData中设置
            if (saveData == null)
            {
                saveData = new TutorialSaveData();
            }
            Debug.Log("[Tutorial] 引导存档管理器已初始化，等待从通用存档加载");
        }
        else
        {
            // 如果SaveSystem还未初始化，创建新的空数据
            saveData = new TutorialSaveData();
            Debug.Log("[Tutorial] 创建新的引导进度数据");
        }
    }

    /// <summary>
    /// 从SaveData中加载引导数据（由SaveSystem调用）
    /// </summary>
    public void LoadFromSaveData(TutorialSaveData data)
    {
        if (data != null)
        {
            saveData = data;
            Debug.Log($"[Tutorial] 从通用存档加载引导进度成功 - 已完成流程: {saveData.completedFlows.Count}");
        }
        else
        {
            saveData = new TutorialSaveData();
            Debug.Log("[Tutorial] 引导数据为空，创建新数据");
        }
    }

    /// <summary>
    /// 获取当前引导存档数据（供SaveSystem保存）
    /// </summary>
    public TutorialSaveData GetSaveData()
    {
        if (saveData == null)
        {
            saveData = new TutorialSaveData();
        }
        return saveData;
    }

    /// <summary>
    /// 触发存档保存（通过通用存档系统）
    /// </summary>
    private void SaveProgress()
    {
        // 引导进度会在下次自动保存时一起保存，不单独触发保存
        // 保存只在以下情况触发：首次启动、玩家手动保存、每分钟自动保存
    }

    /// <summary>
    /// 检查流程是否完成
    /// </summary>
    public bool IsFlowCompleted(string flowID)
    {
        if (saveData == null)
        {
            LoadProgress();
        }
        return saveData != null && saveData.completedFlows.Contains(flowID);
    }

    /// <summary>
    /// 检查步骤是否完成
    /// </summary>
    public bool IsStepCompleted(string flowID, string stepID)
    {
        if (saveData == null)
        {
            LoadProgress();
        }
        string key = $"{flowID}_{stepID}";
        return saveData != null && saveData.completedSteps.Contains(key);
    }

    /// <summary>
    /// 标记流程完成
    /// </summary>
    public void MarkFlowCompleted(string flowID)
    {
        if (saveData == null)
        {
            LoadProgress();
        }

        if (saveData != null && !saveData.completedFlows.Contains(flowID))
        {
            saveData.completedFlows.Add(flowID);
            SaveProgress();
        }
    }

    /// <summary>
    /// 标记步骤完成
    /// </summary>
    public void MarkStepCompleted(string flowID, string stepID)
    {
        if (saveData == null)
        {
            LoadProgress();
        }

        string key = $"{flowID}_{stepID}";
        if (saveData != null && !saveData.completedSteps.Contains(key))
        {
            saveData.completedSteps.Add(key);
            SaveProgress();
        }
    }

    /// <summary>
    /// 重置流程进度
    /// </summary>
    public void ResetFlowProgress(string flowID)
    {
        if (saveData == null)
        {
            LoadProgress();
        }

        if (saveData != null)
        {
            saveData.completedFlows.Remove(flowID);

            // 移除该流程的所有步骤
            saveData.completedSteps.RemoveAll(step => step.StartsWith(flowID + "_"));

            SaveProgress();
            Debug.Log($"[Tutorial] 重置流程进度: {flowID}");
        }
    }

    /// <summary>
    /// 重置所有进度
    /// </summary>
    public void ResetAllProgress()
    {
        saveData = new TutorialSaveData();
        SaveProgress();
        Debug.Log("[Tutorial] 重置所有引导进度");
    }

    /// <summary>
    /// 获取已完成的流程列表
    /// </summary>
    public List<string> GetCompletedFlows()
    {
        if (saveData == null)
        {
            LoadProgress();
        }
        return saveData != null ? new List<string>(saveData.completedFlows) : new List<string>();
    }

    /// <summary>
    /// 获取流程完成进度（返回0-1的百分比）
    /// </summary>
    public float GetFlowProgress(string flowID, int totalSteps)
    {
        if (saveData == null)
        {
            LoadProgress();
        }

        if (saveData == null) return 0f;

        if (IsFlowCompleted(flowID)) return 1f;

        int completedSteps = 0;
        for (int i = 0; i < totalSteps; i++)
        {
            if (saveData.completedSteps.Exists(step => step.StartsWith(flowID + "_")))
            {
                completedSteps++;
            }
        }

        return totalSteps > 0 ? (float)completedSteps / totalSteps : 0f;
    }

    /// <summary>
    /// 设置引导系统启用状态
    /// </summary>
    public void SetTutorialEnabled(bool enabled)
    {
        if (saveData == null)
        {
            LoadProgress();
        }

        if (saveData != null)
        {
            saveData.isTutorialDisabled = !enabled;
            SaveProgress();
            Debug.Log($"[Tutorial] 引导系统 {(enabled ? "启用" : "禁用")}");
        }
    }

    /// <summary>
    /// 检查引导系统是否启用
    /// </summary>
    public bool IsTutorialEnabled()
    {
        if (saveData == null)
        {
            LoadProgress();
        }
        return saveData == null || !saveData.isTutorialDisabled;
    }
}
