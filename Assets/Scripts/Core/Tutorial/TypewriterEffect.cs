using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 打字机效果组件
/// 用于逐字显示文本内容
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterEffect : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private float typeSpeed = 0.05f; // 每个字符的显示间隔（秒）

    private TextMeshProUGUI textComponent;
    private Coroutine typeCoroutine;
    private string fullText;
    private bool isTyping;
    private bool skipRequested;

    /// <summary>
    /// 是否正在打字
    /// </summary>
    public bool IsTyping => isTyping;

    /// <summary>
    /// 打字完成回调
    /// </summary>
    public System.Action OnTypingComplete;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// 开始打字机效果
    /// </summary>
    /// <param name="text">要显示的完整文本</param>
    /// <param name="speed">每个字符的显示间隔（可选）</param>
    public void StartTyping(string text, float? speed = null)
    {
        // 停止之前的打字
        StopTyping();

        fullText = text;
        if (speed.HasValue)
        {
            typeSpeed = speed.Value;
        }

        skipRequested = false;
        typeCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// 停止打字机效果
    /// </summary>
    public void StopTyping()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }
        isTyping = false;
    }

    /// <summary>
    /// 跳过打字动画，立即显示完整文本
    /// </summary>
    public void SkipTyping()
    {
        if (isTyping)
        {
            skipRequested = true;
        }
    }

    /// <summary>
    /// 打字协程
    /// </summary>
    private IEnumerator TypeText()
    {
        isTyping = true;
        textComponent.text = "";

        // 逐字符显示
        for (int i = 0; i < fullText.Length; i++)
        {
            // 检查是否请求跳过
            if (skipRequested)
            {
                textComponent.text = fullText;
                break;
            }

            textComponent.text += fullText[i];

            // 等待指定时间后显示下一个字符
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        skipRequested = false;
        typeCoroutine = null;

        // 触发完成回调
        OnTypingComplete?.Invoke();
    }

    /// <summary>
    /// 设置打字速度
    /// </summary>
    public void SetTypeSpeed(float speed)
    {
        typeSpeed = Mathf.Max(0.01f, speed);
    }

    /// <summary>
    /// 获取当前打字速度
    /// </summary>
    public float GetTypeSpeed()
    {
        return typeSpeed;
    }

    private void OnDestroy()
    {
        StopTyping();
        OnTypingComplete = null;
    }
}
