using UnityEngine;
using UnityEngine.EventSystems;
using SUIFW;

public class TipsTrigger : MonoBehaviour, IPointerEnterHandler
{
    [Header("Tips Content")]
    [TextArea(3, 10)]
    public string tipsContent = "Id:0";

    private RectTransform rectTransform;
    private static TipsManager cachedTipsManager;
    private int textId = -1;
    private bool useTextId = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (tipsContent != null && tipsContent.StartsWith("Id:"))
        {
            if (int.TryParse(tipsContent.Substring(3), out int id))
            {
                textId = id;
                useTextId = true;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!TryGetTipsManager(out var tips))
        {
            return;
        }
        tips.transform.SetParent(CoreSetting.Instance.LastPanel);
        tips.transform.SetAsLastSibling();

        if (useTextId && textId > 0)
        {
            string content = LocalizationManager.GetCurrentLanguageByID(textId);
            if (!string.IsNullOrEmpty(content))
            {
                tips.ShowTips(rectTransform, content);
            }
            else
            {
                tips.ShowTips(rectTransform, tipsContent);
            }
        }
        else
        {
            tips.ShowTips(rectTransform, tipsContent);
        }
    }

    private void OnDisable()
    {
        if (cachedTipsManager != null)
        {
            cachedTipsManager.ForceHide();
        }
    }

    private bool TryGetTipsManager(out TipsManager tips)
    {
        if (cachedTipsManager != null)
        {
            tips = cachedTipsManager;
            return true;
        }

        if (ResMgr.Instance == null || !ResMgr.Instance.prefabInitComplete)
        {
            tips = null;
            return false;
        }

        var loadedTips = UIManager.Instance.ShowUIForms("TipsManager") as TipsManager;
        if (loadedTips == null)
        {
            tips = null;
            return false;
        }

        cachedTipsManager = loadedTips;
        tips = cachedTipsManager;
        return true;
    }
}
