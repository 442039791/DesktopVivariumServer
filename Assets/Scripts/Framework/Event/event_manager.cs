using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class event_manager : Singleton<event_manager> {
    public struct Timer
    {
        public float interval;
        public string eventName;    // 事件名称
        public object eventData;    // 事件数据
        public bool isRepeated;
        public float nextRunTime;
    }
    private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
    public delegate void on_event_handler(string name, object udata);
    
    private Dictionary<string, on_event_handler> event_listeners = new Dictionary<string,on_event_handler>();
    private Dictionary<string , on_event_handler> event_UI_listenrs = new Dictionary<string ,on_event_handler>();
    public void init() { 
    }
    public void add_UIevent_listener(string name, on_event_handler handler)
    {
        if (this.event_UI_listenrs.ContainsKey(name))
        {
            this.event_UI_listenrs[name] = handler;
        }
        else
        {
            this.event_UI_listenrs.Add(name, handler);
        }
    }
    public void remove_UIevent_listener(string name)
    {
        if (!this.event_UI_listenrs.ContainsKey(name))
        {
            return;
        }

        this.event_UI_listenrs[name] = null;

        this.event_UI_listenrs.Remove(name);
        
    }
    public void dispatch_UIevent(string name, object udata)
    {
        if (!this.event_UI_listenrs.ContainsKey(name))
        {
            return;
        }

        if (this.event_UI_listenrs[name] != null)
        {
            this.event_UI_listenrs[name](name, udata);
        }
    }
    // 订阅者而言
    public void add_event_listener(string name, on_event_handler handler) {
        if (this.event_listeners.ContainsKey(name)) {
            this.event_listeners[name] += handler;
        }
        else {
            this.event_listeners.Add(name, handler);
        }
    }

    public void remove_event_listener(string name, on_event_handler handler) {
        if (!this.event_listeners.ContainsKey(name)) {
            return;
        }

        this.event_listeners[name] -= handler;
        if (this.event_listeners[name] == null) {
            this.event_listeners.Remove(name);
        }
    }
    // end 

    // 发布
    public void dispatch_event(string name, object udata) {
        if (!this.event_listeners.ContainsKey(name)) {

            return;
        }

        if (this.event_listeners[name] != null) {
            this.event_listeners[name].Invoke(name, udata);
        }
    }
    // end 
    public void AddTimer(string id, float interval, string eventName, object eventData, bool isRepeated)
    {
        Timer newTimer = new Timer
        {
            interval = interval,
            eventName = eventName,
            eventData = eventData,
            isRepeated = isRepeated,
            //TODO:LogicTime
            //nextRunTime = Testserver.Instance.time + interval
        };
        timers[id] = newTimer;
    }
    public void RemoveTimer(string id)
    {
        if (timers.ContainsKey(id))
        {
            timers.Remove(id);
        }
    }
    //public void LogicUpdate()
    //{
    //    if(timers.Count == 0) {
    //        return;
    //    }
    //    List<string> toRemove = new List<string>();
    //    float currentTime = Testserver.Instance.time;

    //    foreach (var pair in timers)
    //    { // 创建副本以避免修改集合
    //        var id = pair.Key;
    //        var timer = pair.Value;
    //        if (currentTime >= timer.nextRunTime)
    //        {
    //            dispatch_event(timer.eventName, timer.eventData); // 使用事件系统触发

    //            if (timer.isRepeated)
    //            {
    //                var t = timers[id];
    //                t.nextRunTime = currentTime + timer.interval;
    //            }
    //            else
    //            {
    //                toRemove.Add(id);
    //            }
    //        }
    //    }

    //    foreach (var id in toRemove)
    //    {
    //        timers.Remove(id);
    //    }
    //}

}
