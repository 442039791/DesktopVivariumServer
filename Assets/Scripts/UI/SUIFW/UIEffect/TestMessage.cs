using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class TestMessage : MonoBehaviour
{
    public Text textPrefab; // 带有Text组件的预制体
    public float moveDistance = 20f; // 上升距离
    public float duration = 0.5f;      // 动画时长
    public Camera UIcamera;
    public void ShowFloatingText(Vector3 worldPosition, string message, int fontSize)
    {
        // 实例化文本对象
        Text floatingText = Instantiate(textPrefab, worldPosition, Quaternion.identity, transform);
        floatingText.gameObject.SetActive(true);
        // 设置文本属性
        floatingText.text = message;
        floatingText.fontSize = fontSize;
        floatingText.color = new Color(floatingText.color.r, floatingText.color.g, floatingText.color.b, 0);

        // 获取RectTransform组件
        RectTransform rectTransform = floatingText.GetComponent<RectTransform>();

        // 设置初始位置（将世界坐标转换为屏幕坐标）
        Vector2 screenPosition = /*UIcamera.WorldToScreenPoint(*/worldPosition/*)*/;
        rectTransform.position = screenPosition;

        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        // 上升动画
        sequence.Append(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + moveDistance, duration));

        // 淡入淡出动画
        sequence.Join(floatingText.DOFade(1, duration * 0.2f));      // 前20%时间淡入
        sequence.Append(floatingText.DOFade(0, duration * 0.8f));    // 后80%时间淡出

        // 动画完成后销毁对象
        sequence.OnComplete(() => Destroy(floatingText.gameObject));

        // 设置缓动函数
        sequence.SetEase(Ease.OutQuad);
    }
    public void TestShow(string message)
    {
        ShowFloatingText(transform.position, message, 14);
    }
    public void on_event_handler(string name, object udata)
    {
        string message = (string)udata;
        ShowFloatingText(transform.position, message, 14);
    }
    void Start()
    {
        event_manager.instance.add_UIevent_listener(SysDefine.TestMessage, on_event_handler);
    }


}
