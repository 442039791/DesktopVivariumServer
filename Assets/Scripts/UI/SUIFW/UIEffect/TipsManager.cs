using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SUIFW;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(ContentSizeFitter))]
public class TipsManager : BaseUIForms
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform tipsPanel;
    [SerializeField] private TextMeshProUGUI tipsText;
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ContentSizeFitter sizeFitter;

    [Header("Settings")]
    [SerializeField] private float padding = 10f;
    [SerializeField] private Vector2 minMaxWidth = new(150f, 500f);
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float horizontalThreshold = 0.5f;
    [SerializeField] private float verticalThreshold = 0.5f;
    [SerializeField] private float maxHeightRatio = 0.7f;

    private RectTransform currentTarget;
    private string currentContent;
    private Tween fadeTween;

    public override bool Init()
    {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0;

        LocalizationManager.ScanAndRegisterTextsStatic(transform);

        if (sizeFitter == null)
            sizeFitter = transform.GetComponent<ContentSizeFitter>();

        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 确保TipsDetector存在
        EnsureTipsDetector();

        return base.Init();
    }

    private void EnsureTipsDetector()
    {
        if (TipsDetector.Instance == null)
        {
            var go = new GameObject("TipsDetector");
            go.AddComponent<TipsDetector>();
        }
        TipsDetector.Instance?.RegisterTipsManager(this);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && gameObject.activeInHierarchy)
        {
            ForceHide();
        }
    }

    public void ShowTips(RectTransform target, string content)
    {
        if (target == null) return;

        if (currentTarget == target && content == currentContent && gameObject.activeInHierarchy)
        {
            UpdatePosition();
            return;
        }

        KillFadeTween();
        currentTarget = target;
        currentContent = content;

        UpdatePosition();
        tipsText.text = content;

        var font = LocalizationManager.GetCurrentFont();
        if (font != null)
        {
            tipsText.font = font;
        }

        gameObject.SetActive(true);
        StartCoroutine(UpdateLayoutAndPosition());
    }

    public void ShowTips(RectTransform target, int textId)
    {
        string content = LocalizationManager.GetCurrentLanguageByID(textId);
        if (string.IsNullOrEmpty(content))
        {
            content = $"[ID:{textId}]";
        }
        ShowTips(target, content);
    }

    private IEnumerator UpdateLayoutAndPosition()
    {
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(tipsText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tipsPanel);

        float clampedWidth = Mathf.Clamp(tipsPanel.rect.width, minMaxWidth.x, minMaxWidth.y);
        tipsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedWidth);

        LayoutRebuilder.ForceRebuildLayoutImmediate(tipsPanel);
        float actualHeight = tipsText.preferredHeight + padding * 2;
        float maxHeight = Screen.height * maxHeightRatio;
        if (actualHeight > maxHeight)
        {
            actualHeight = maxHeight;
        }
        tipsPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, actualHeight);

        UpdatePosition();

        KillFadeTween();
        canvasGroup.alpha = 0;
        fadeTween = canvasGroup.DOFade(1f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    private void KillFadeTween()
    {
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }
        fadeTween = null;
    }

    public void HideTips()
    {
        if (!gameObject.activeInHierarchy) return;

        currentTarget = null;
        currentContent = null;

        KillFadeTween();

        fadeTween = canvasGroup.DOFade(0f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() => {
                gameObject.SetActive(false);
                gameObject.transform.position = new Vector3(-9999, -9999, 0);
            });
    }

    /// <summary>
    /// 强制立即隐藏，由TipsDetector调用
    /// </summary>
    public void ForceHide()
    {
        currentTarget = null;
        currentContent = null;

        KillFadeTween();
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
        gameObject.transform.position = new Vector3(-9999, -9999, 0);
    }

    private void UpdatePosition()
    {
        if (currentTarget == null) return;

        Vector3[] targetCorners = new Vector3[4];
        currentTarget.GetWorldCorners(targetCorners);

        Vector3 targetCenter = (targetCorners[0] + targetCorners[2]) / 2f;
        Vector3 targetScreenPos = targetCenter;

        float tipWidth = tipsPanel.rect.width;
        float tipHeight = tipsPanel.rect.height;

        bool shouldShowOnLeft = currentTarget.position.x > horizontalThreshold;
        bool shouldShowAbove = currentTarget.position.y < verticalThreshold;

        Vector2 tipsPosition = Vector2.zero;

        if (shouldShowOnLeft)
        {
            tipsPosition.x = targetScreenPos.x - (currentTarget.rect.width / 2 + tipWidth / 2 + padding);
        }
        else
        {
            tipsPosition.x = targetScreenPos.x + (currentTarget.rect.width / 2 + tipWidth / 2 + padding);
        }

        if (shouldShowAbove)
        {
            tipsPosition.y = targetScreenPos.y + (currentTarget.rect.height / 2 + tipHeight / 2 + padding);
        }
        else
        {
            tipsPosition.y = targetScreenPos.y - (currentTarget.rect.height / 2 + tipHeight / 2 + padding);
        }

        transform.position = tipsPosition;
    }
}
