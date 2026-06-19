using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ForwardScrollDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    public ScrollRect externalScrollRect; // �ⲿ ScrollRect����Ҫת������Ŀ�꣩
    private ScrollRect selfScrollRect;    // ������ ScrollRect
    private bool isDragging = false;      // �Ƿ�������ק
    [Tooltip("���������ȣ���ֵ��ʾ�����ٶ�")]
    public float scrollSensitivity = 0.001f; // ����������

    void Awake()
    {
        selfScrollRect = GetComponent<ScrollRect>();
        // ��ʼ��ʱ��ȫ���������Ĺ�����Ӧ
        selfScrollRect.scrollSensitivity = 0;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        if (externalScrollRect != null)
        {
            externalScrollRect.OnBeginDrag(eventData);
        }
        selfScrollRect.enabled = false; // ��ʱ������������
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (externalScrollRect != null)
        {
            externalScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (externalScrollRect != null)
        {
            externalScrollRect.OnEndDrag(eventData);
        }
        selfScrollRect.enabled = true; // �ָ���������
    }

    // ʵ�ֹ��ֽӿ� - ���ǹؼ��޸ĵ�
    public void OnScroll(PointerEventData eventData)
    {
        // 1. ��ֹ�¼�����������ScrollRect
        eventData.Use();

        // 2. ֻ�е������ͣ�ڴ�UI��ʱ�Ŵ������֣�����û����ק��
        if (/*IsPointerOverThisUI() && */!isDragging && externalScrollRect != null)
        {
            HandleExternalScroll(eventData.scrollDelta);
        }
    }

    // ת�������¼��ľ���ʵ��
    private void HandleExternalScroll(Vector2 scrollDelta)
    {
        // ע�⣺scrollDelta.y�����Ϲ���ʱΪ�������¹���Ϊ��
        // ��ScrollRect��verticalNormalizedPosition���Ϲ���ʱ���ӣ���0��1��
        if (externalScrollRect.vertical)
        {
            // ���㲢�����µĴ�ֱλ��
            float newVerticalPos = externalScrollRect.verticalNormalizedPosition +
                                  scrollDelta.y * scrollSensitivity;
            newVerticalPos = Mathf.Clamp01(newVerticalPos);
            externalScrollRect.verticalNormalizedPosition = newVerticalPos;
        }

        if (externalScrollRect.horizontal)
        {
            // ���㲢�����µ�ˮƽλ��
            float newHorizontalPos = externalScrollRect.horizontalNormalizedPosition +
                                    scrollDelta.x * scrollSensitivity;
            newHorizontalPos = Mathf.Clamp01(newHorizontalPos);
            externalScrollRect.horizontalNormalizedPosition = newHorizontalPos;
        }
    }

    // �������Ƿ���ͣ�ڴ�UIԪ����
    private bool IsPointerOverThisUI()
    {
        // ʹ��EventSystem������ȷ
        var eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        // �������Ƿ��ڴ�UI��
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
