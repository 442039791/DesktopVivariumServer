using UnityEngine;
using Common;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DesktopButtonManager : Singleton<DesktopButtonManager>
{
    public static DesktopButton Button;
    public HashSet<IClick> allClicks = new HashSet<IClick>();
    public Dictionary<string, UnityEngine.Events.UnityAction> Actions = new();
    //public List<IClick> saveActiveClick = new List<IClick>();
    //public void AllclicksActive()
    //{
    //    saveActiveClick.Clear();
    //    foreach (IClick click in allClicks)
    //    {
    //        if(click.CanClick)
    //        {
    //            saveActiveClick.Add(click);
    //        }
    //    }

    //}
    public void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
#if UNITY_EDITOR || MY_CUSTOM_MACRO
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { AudioSourceManager.Instance.PlayButtonClip(); }) ;
        button.onClick.AddListener(action);
#else
        var click = button.GetComponent<IClick>();
        if (click==null)
        {
            click = button.transform.AddComponent<DesktopButton>();
        }
        click.AddListener(action);
        allClicks.Add(click);
#endif
    }
    public void RemoveListener(Button button)
    {
#if UNITY_EDITOR || MY_CUSTOM_MACRO
        button.onClick.RemoveAllListeners();
#else
        var click = button.GetComponent<IClick>();
        if (click == null)
            return;
        click.RemoveListener();
        allClicks.Remove(click);
#endif
    }
    public void ButtonShowFalse(Button button)
    {
        button.gameObject.SetActive(false);
#if !UNITY_EDITOR && !MY_CUSTOM_MACRO
        var click = button.GetComponent<IClick>();
        if (click == null)
            return;
        click.SavePos = button.transform.position;
        button.gameObject.transform.position = new Vector3(-99999999, -9999999, 0);
        click.CanClick = false;
#endif
    }
    public void ButtonShowTrue(Button button)
    {
        button.gameObject.SetActive(true);
#if !UNITY_EDITOR && !MY_CUSTOM_MACRO
        var click = button.GetComponent<IClick>();
        if (click == null)
            return;
        button.gameObject.transform.position = click.SavePos;
        click.CanClick = true;
#endif
    }


}
public static class DesktopButtonExpand
{
    public static MyButtonTriggerEvent AddDeskTopStillClipListener(this Button button, UnityEngine.Events.UnityAction action, float activationDelay = 0.5f, float repeatInterval = 0.1f)
    {
        // �ֶ������¼����ͣ�����Inspector�����ӣ�
        if(!button.gameObject.TryGetComponent<MyButtonTriggerEvent>(out var trigger))
        {
            trigger = button.gameObject.AddComponent<MyButtonTriggerEvent>();
        }
        trigger.Init(action, activationDelay, repeatInterval);
        button.AddDeskTopListener(action);
        //trigger.triggers.Clear();

        //// ��ѹ��ʼ�¼�
        //EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        //pointerDownEntry.eventID = EventTriggerType.PointerDown;
        //pointerDownEntry.callback.AddListener((data) => { trigger._OnPointerClick = true; });
        //trigger.triggers.Add(pointerDownEntry);

        //// ��ѹ�����¼�
        //EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        //pointerUpEntry.eventID = EventTriggerType.PointerUp;
        //pointerUpEntry.callback.AddListener((data) => { trigger._OnPointerClick = false; });
        //trigger.triggers.Add(pointerUpEntry);

        //// ָ���Ƴ��¼�����ֹ��ס���Ƴ���ť�Ա���״̬��
        //EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        //pointerExitEntry.eventID = EventTriggerType.PointerExit;
        //pointerExitEntry.callback.AddListener((data) => { trigger._OnPointerClick = false; });
        //trigger.triggers.Add(pointerExitEntry);
        return trigger;
    }
    public static void AddDeskTopListener(this Button button, UnityEngine.Events.UnityAction action)
    {
        DesktopButtonManager.instance.AddListener(button, action);
    }
    public static void RemoveDeskTopListener(this Button button)
    {
        DesktopButtonManager.instance.RemoveListener(button);
    }
    public static void DestTopShowTrue(this Button button)
    {
        DesktopButtonManager.instance.ButtonShowTrue(button);
    }
    public static void DeskTopShowFalse(this Button button)
    {
        DesktopButtonManager.instance.ButtonShowFalse(button);
    }
}
public class DesktopButton : MonoBehaviour, IClick, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEngine.Events.UnityAction Action;

    public bool CanClick { get ; set ; }
    public Vector3 SavePos { get ; set ; }

    public void AddListener(UnityEngine.Events.UnityAction action)
    {
        Action = action;
        CanClick = true;
    }
    public void RemoveListener()
    {
        Action=null;
    }
    public void OnClick()
    {
        if(!CanClick) {
            return;
        }
        //TODO�����ֳ��������Ч��
        Action?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //throw new NotImplementedException();
        DesktopButtonManager.Button = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(DesktopButtonManager.Button==this)
        {
            DesktopButtonManager.Button = null;
        }
        //throw new NotImplementedException();
    }
}