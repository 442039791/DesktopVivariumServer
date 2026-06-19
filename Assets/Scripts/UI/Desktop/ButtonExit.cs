using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonExit : MonoBehaviour ,IClick, IPointerEnterHandler, IPointerExitHandler
{
    public Button buttonp;

    public Vector3 SavePos { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool CanClick { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Start()
    {
        buttonp.onClick.AddListener(OnClick);
        //buttonp.AddDeskTopListener(OnClick);
    }
    public void OnClick()
    {
#if UNITY_EDITOR
        // 在编辑器中停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 其他平台使用通用的 Application.Quit()
            Application.Quit();
#endif
    }

    public void AddListener(UnityAction action)
    {
        
    }

    public void RemoveListener()
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ((IPointerEnterHandler)buttonp).OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ((IPointerExitHandler)buttonp).OnPointerExit(eventData);
    }
}
