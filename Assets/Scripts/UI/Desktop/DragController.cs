// ���ص�������Ϸ����
using UnityEngine.EventSystems;
using UnityEngine;

public class DragController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private DirectWindowDrag windowDrag;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            windowDrag.StartDrag();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        windowDrag.EndDrag();
    }
}
