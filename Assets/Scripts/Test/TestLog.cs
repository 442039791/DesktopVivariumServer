using TMPro;
using UnityEngine;

public class TestLog : MonoBehaviour
{
    public TextMeshProUGUI log;
    private bool Clear=false;
    private void Awake()
    {
        event_manager.instance.add_UIevent_listener(SysDefine.TestLog, on_event_handler);
        log??= GetComponent<TextMeshProUGUI>();
    }
    private float timeElapsed = 0f;
    public  void on_event_handler(string name, object udata)
    {
        var message = (string)udata;
        log.text = message;
        Clear = true;
        timeElapsed = 0;
    }
    void Update()
    {
        if(Clear)
        {
            // 累加时间
            timeElapsed += Time.deltaTime;

            // 每秒执行一次
            if (timeElapsed >= 0.5f)
            {
                // 执行你想要的操作
                log.text = null;

                // 重置时间计数器
                timeElapsed = 0f;
                Clear = false;
            }
            
        }

    }
}
