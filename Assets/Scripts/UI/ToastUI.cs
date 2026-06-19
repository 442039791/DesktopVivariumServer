using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// 单个飘字UI控制脚本
/// 负责飘字的显示、淡入淡出动画和自动销毁
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ToastUI : MonoBehaviour
{
    [Header("UI组件引用")]
    [Tooltip("飘字文本组件（TextMeshProUGUI）")]
    public TextMeshProUGUI toastText;

    [Header("可选背景")]
    [Tooltip("飘字背景图片（可选）")]
    public Image background;

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;

    private void Awake()
    {
        // 获取或添加CanvasGroup组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初始时完全透明
        canvasGroup.alpha = 0f;

        // 禁用交互，飘字不需要接收鼠标事件
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // 自动查找TextMeshProUGUI组件（如果未设置）
        if (toastText == null)
        {
            toastText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // 自动查找Image组件（如果未设置）
        if (background == null)
        {
            background = GetComponent<Image>();
        }

        // 禁用所有 Image 的 raycastTarget
        DisableRaycastTargets();
    }

    /// <summary>
    /// 禁用所有图像的 Raycast Target，避免 Input System 追踪
    /// </summary>
    private void DisableRaycastTargets()
    {
        var images = GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img != null)
            {
                img.raycastTarget = false;
            }
        }

        var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in texts)
        {
            if (text != null)
            {
                text.raycastTarget = false;
            }
        }
    }

    /// <summary>
    /// 显示飘字消息
    /// </summary>
    /// <param name="message">飘字文本</param>
    /// <param name="duration">显示持续时间</param>
    /// <param name="fadeInTime">淡入时间</param>
    /// <param name="fadeOutTime">淡出时间</param>
    /// <param name="onComplete">完成后的回调</param>
    public void Show(string message, float duration, float fadeInTime, float fadeOutTime, Action onComplete = null)
    {
        if (toastText != null)
        {
            toastText.text = message;
        }
        else
        {
            Debug.LogError("[ToastUI] 未找到TextMeshProUGUI组件！");
        }

        // 停止之前的协程
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        // 开始显示协程
        displayCoroutine = StartCoroutine(DisplayCoroutine(duration, fadeInTime, fadeOutTime, onComplete));
    }

    /// <summary>
    /// 显示协程：淡入 -> 等待 -> 淡出 -> 销毁
    /// </summary>
    private IEnumerator DisplayCoroutine(float duration, float fadeInTime, float fadeOutTime, Action onComplete)
    {
        // 淡入阶段
        yield return StartCoroutine(FadeIn(fadeInTime));

        // 等待显示
        yield return new WaitForSeconds(duration);

        // 淡出阶段
        yield return StartCoroutine(FadeOut(fadeOutTime));

        // 执行回调（在销毁前）
        onComplete?.Invoke();

        // 销毁对象（不再使用 SetActive，因为会停止协程）
        // 我们已经通过 CanvasGroup 和 raycastTarget 禁用了交互
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 淡入动画
    /// </summary>
    private IEnumerator FadeIn(float fadeTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 淡出动画
    /// </summary>
    private IEnumerator FadeOut(float fadeTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 立即隐藏飘字
    /// </summary>
    public void Hide()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        // 直接销毁对象
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
