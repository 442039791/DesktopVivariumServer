using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SUIFW;
using DG.Tweening;
using System.Collections;

public class IconTipsManager : BaseUIForms
{
    [SerializeField] private TextMeshProUGUI iconName;
    [SerializeField] private TextMeshProUGUI type;
    [SerializeField] private TextMeshProUGUI describe;

    [Header("UI Elements")]
    [SerializeField] private RectTransform tipsPanel;
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float padding = 10f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float horizontalThreshold = 0.5f;
    [SerializeField] private float verticalThreshold = 0.5f;

    private RectTransform currentTarget;
    private Tween fadeTween;

    public override bool Init()
    {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0;

        LocalizationManager.ScanAndRegisterTextsStatic(transform);

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
        TipsDetector.Instance?.RegisterIconTipsManager(this);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && gameObject.activeInHierarchy)
        {
            ForceHide();
        }
    }

    public void ShowTips(RectTransform target, string Name, string Type, string Describe)
    {
        if (target == null) return;

        if (currentTarget == target && Name == iconName.text && gameObject.activeInHierarchy)
        {
            UpdatePosition();
            return;
        }

        KillFadeTween();
        currentTarget = target;

        UpdatePosition();
        LocalizationManager.SetText(iconName, int.Parse(Name));
        LocalizationManager.SetText(type, int.Parse(Type));
        LocalizationManager.SetText(describe, int.Parse(Describe));
        gameObject.SetActive(true);

        StartCoroutine(UpdateLayoutAndPosition());
    }

    private IEnumerator UpdateLayoutAndPosition()
    {
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(iconName.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tipsPanel);

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

    private void OnRectTransformDimensionsChange()
    {
        if (gameObject.activeInHierarchy)
        {
            UpdatePosition();
        }
    }
}
