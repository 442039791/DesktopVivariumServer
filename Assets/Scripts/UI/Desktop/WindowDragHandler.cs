using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragHandler : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (WallpaperManager.Instance != null)
            {
                WallpaperManager.Instance.StartWindowDrag();
            }
        }
    }
}