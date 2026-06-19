using UnityEngine;
using UnityEngine.EventSystems;
using SUIFW;

public class IconTipsTrigger : MonoBehaviour, IPointerEnterHandler
{
    public string Name;
    public string Type;
    public string Describe;
    public bool isEnabled = false;

    private RectTransform rectTransform;
    private static IconTipsManager cachedTipsManager;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEnabled)
        {
            return;
        }

        IconTipsManager tips = GetTipsManager();
        if (tips == null)
        {
            return;
        }
        tips.transform.SetParent(CoreSetting.Instance.LastPanel);
        tips.ShowTips(rectTransform, Name, Type, Describe);
    }

    private void OnDisable()
    {
        if (cachedTipsManager != null)
        {
            cachedTipsManager.ForceHide();
        }
    }

    private IconTipsManager GetTipsManager()
    {
        if (cachedTipsManager != null)
        {
            return cachedTipsManager;
        }
        cachedTipsManager = (IconTipsManager)UIManager.Instance.ShowUIForms("IconTips");
        return cachedTipsManager;
    }
}
