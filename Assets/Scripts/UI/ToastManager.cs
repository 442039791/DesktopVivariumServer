using UnityEngine;
using Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 飘字通知管理器 - 统一管理游戏中的所有飘字通知（Toast）
/// 使用方法：ToastManager.Show("金币不足！");
/// </summary>
public class ToastManager : MonoSingleton<ToastManager>
{
    [Header("飘字UI预制体")]
    [Tooltip("在Inspector中拖入飘字UI预制体")]
    public GameObject toastPrefab;

    [Header("飘字显示设置")]
    [Tooltip("飘字显示的持续时间（秒）")]
    public float displayDuration = 2f;

    [Tooltip("淡入时间（秒）")]
    public float fadeInDuration = 0.3f;

    [Tooltip("淡出时间（秒）")]
    public float fadeOutDuration = 0.3f;

    [Tooltip("多个飘字之间的垂直间距")]
    public float verticalSpacing = 80f;

    [Header("飘字位置")]
    [Tooltip("飘字在屏幕上的锚点位置")]
    public Vector2 anchorPosition = new(0.5f, 0.8f); // 默认屏幕上方中间

    private Canvas toastCanvas;
    private List<ToastUI> displayingToasts = new();
    private Dictionary<ToastUI, Coroutine> moveCoroutines = new();

    public override void Init()
    {
        // 创建或查找Canvas
        CreateToastCanvas();
    }

    /// <summary>
    /// 创建飘字专用的Canvas
    /// </summary>
    private void CreateToastCanvas()
    {
        // 查找现有的Canvas
        toastCanvas = FindAnyObjectByType<Canvas>();

        // 如果没有Canvas，创建一个新的
        if (toastCanvas == null)
        {
            GameObject canvasObj = new("ToastCanvas");
            toastCanvas = canvasObj.AddComponent<Canvas>();
            toastCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            toastCanvas.sortingOrder = 9999; // 确保飘字显示在最上层

            // 添加CanvasScaler以适配不同分辨率
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasObj.transform.SetParent(transform);

        }
    }

    /// <summary>
    /// 显示飘字消息（静态方法，方便外部调用）
    /// </summary>
    /// <param name="message">要显示的飘字文本</param>
    public static void Show(string message)
    {
        Instance.ShowToast(message);
    }

    /// <summary>
    /// 显示飘字消息，通过文本ID从字典获取内容（静态方法，方便外部调用）
    /// </summary>
    /// <param name="textId">文本ID</param>
    public static void Show(int textId)
    {
        Instance.ShowToast(textId);
    }

    /// <summary>
    /// 显示飘字消息（实例方法）
    /// </summary>
    /// <param name="message">要显示的飘字文本</param>
    public void ShowToast(string message)
    {
        if (toastPrefab == null)
        {
            Debug.LogError("[ToastManager] 飘字预制体未设置！请在Inspector中拖入ToastPrefab。");
            return;
        }

        if (toastCanvas == null)
        {
            CreateToastCanvas();
        }

        // 实例化飘字UI
        GameObject toastObj = Instantiate(toastPrefab, toastCanvas.transform);
        ToastUI toastUI = toastObj.GetComponent<ToastUI>();

        if (toastUI == null)
        {
            Debug.LogError("[ToastManager] 飘字预制体上没有ToastUI组件！");
            Destroy(toastObj);
            return;
        }

        // 设置飘字位置
        RectTransform rectTransform = toastObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorPosition;
        rectTransform.anchorMax = anchorPosition;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        // 如果有其他正在显示的飘字，向下偏移（新飘字在下方）
        if (displayingToasts.Count > 0)
        {
            float offset = displayingToasts.Count * verticalSpacing;
            rectTransform.anchoredPosition = new Vector2(0, -offset);
        }

        // 添加到显示列表
        displayingToasts.Add(toastUI);

        // 显示飘字
        toastUI.Show(message, displayDuration, fadeInDuration, fadeOutDuration, () =>
        {
            // 飘字消失后的回调
            displayingToasts.Remove(toastUI);

            // 清理协程引用
            if (moveCoroutines.ContainsKey(toastUI))
            {
                moveCoroutines.Remove(toastUI);
            }

            UpdateToastPositions();
        });
    }

    /// <summary>
    /// 显示飘字消息，通过文本ID从字典获取内容（实例方法）
    /// </summary>
    /// <param name="textId">文本ID</param>
    public void ShowToast(int textId)
    {
        // 从本地化管理器获取文本
        string message = LocalizationManager.GetCurrentLanguageByID(textId);

        // 如果获取失败，使用默认文本
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning($"[ToastManager] 未找到文本ID: {textId}，使用默认文本");
            message = $"[文本ID:{textId}]";
        }

        // 调用原有的ShowToast方法显示飘字
        ShowToast(message);
    }

    /// <summary>
    /// 更新所有飘字的位置（当有飘字消失时）
    /// </summary>
    private void UpdateToastPositions()
    {
        // 清理已销毁的 toast
        displayingToasts.RemoveAll(toast => toast == null);

        for (int i = 0; i < displayingToasts.Count; i++)
        {
            ToastUI toast = displayingToasts[i];
            if (toast == null || toast.gameObject == null) continue;

            RectTransform rectTransform = toast.GetComponent<RectTransform>();
            if (rectTransform == null) continue;

            // 第一个飘字在Y=0位置，后续飘字向下偏移
            float targetY = -i * verticalSpacing;

            // 停止之前的移动协程（如果存在）
            if (moveCoroutines.ContainsKey(toast) && moveCoroutines[toast] != null)
            {
                StopCoroutine(moveCoroutines[toast]);
                moveCoroutines.Remove(toast);
            }

            // 启动新的移动协程
            Coroutine moveCoroutine = StartCoroutine(SmoothMoveY(toast, rectTransform, targetY, 0.3f));
            moveCoroutines[toast] = moveCoroutine;
        }
    }

    /// <summary>
    /// 平滑移动Y坐标
    /// </summary>
    private IEnumerator SmoothMoveY(ToastUI toast, RectTransform rectTransform, float targetY, float duration)
    {
        if (toast == null || rectTransform == null)
        {
            if (toast != null && moveCoroutines.ContainsKey(toast))
            {
                moveCoroutines.Remove(toast);
            }
            yield break;
        }

        float startY = rectTransform.anchoredPosition.y;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 检查对象是否仍然存在
            if (toast == null || toast.gameObject == null || rectTransform == null)
            {
                if (toast != null && moveCoroutines.ContainsKey(toast))
                {
                    moveCoroutines.Remove(toast);
                }
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            // 使用EaseOutCubic缓动函数
            t = 1f - Mathf.Pow(1f - t, 3f);

            float newY = Mathf.Lerp(startY, targetY, t);

            // 再次检查以防在赋值前对象被销毁
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
            }

            yield return null;
        }

        // 最终位置设置
        if (toast != null && rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, targetY);
        }

        // 清理协程引用
        if (toast != null && moveCoroutines.ContainsKey(toast))
        {
            moveCoroutines.Remove(toast);
        }
    }

    /// <summary>
    /// 清除所有飘字
    /// </summary>
    public static void ClearAll()
    {
        Instance.ClearAllToasts();
    }

    /// <summary>
    /// 清除所有飘字（实例方法）
    /// </summary>
    public void ClearAllToasts()
    {
        // 停止所有移动协程
        foreach (var kvp in moveCoroutines)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        moveCoroutines.Clear();

        // 销毁所有飘字
        foreach (var toast in displayingToasts)
        {
            if (toast != null && toast.gameObject != null)
            {
                Destroy(toast.gameObject);
            }
        }
        displayingToasts.Clear();
    }
}
