using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MyButtonTriggerEvent : EventTrigger
{
    public UnityEngine.Events.UnityAction action;
    public bool _OnPointerClick = false;
    public bool _OnPointerExit = false;
    private float holdDuration = 0;
    private float lastTriggerTime = 0;
    public float activationDelay = 0.5f;
    public float repeatInterval = 0.1f;
    public void Init(UnityAction action,float activationDelay=0.5f,float repeatInterval=0.1f)
    {
        this.action = action;
        this.activationDelay = activationDelay;
        this.repeatInterval = repeatInterval;
        triggers.Clear();

        // 按压开始事件
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { _OnPointerClick = true; });
        triggers.Add(pointerDownEntry);

        // 按压结束事件
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { _OnPointerClick = false; });
        triggers.Add(pointerUpEntry);

        // 指针移出事件（防止按住后移出按钮仍保持状态）
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => { _OnPointerClick = false; });
        triggers.Add(pointerExitEntry);
    }
    private void Update()
    {
        if (_OnPointerClick)
        {
            if (holdDuration >= activationDelay)
            {
                if (lastTriggerTime >= repeatInterval)
                {
                    action.Invoke();
                    lastTriggerTime = 0;
                }
                lastTriggerTime += Time.deltaTime;
            }
            holdDuration += Time.deltaTime;
        }
        else
        {
            holdDuration = 0;
            lastTriggerTime = 0;
        }
    }
}
