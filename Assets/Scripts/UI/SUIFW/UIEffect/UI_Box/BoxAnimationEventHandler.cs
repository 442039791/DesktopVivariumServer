using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BoxAnimationEventHandler : MonoBehaviour
{

    // 基本事件处理（无参数）
    public void OnAnimationEvent()
    {
        StartCoroutine(AnimationEvent());
        // 这里添加自定义逻辑
    }
    private IEnumerator AnimationEvent()
    {
        yield return new WaitForSeconds(0.1f);
        Gather.Instance.ShowGather();
    }
    // 带字符串参数的事件
    public void OnAnimationEventWithMessage(string message)
    {
    }

    // 带浮点数参数的事件
    public void OnAnimationEventWithFloat(float value)
    {
        // 示例：根据值调整透明度
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, value);
    }

    // 带对象引用的事件
    public void OnAnimationEventWithObject(Object targetObject)
    {
        if (targetObject is GameObject obj)
        {
            obj.SetActive(true);
        }
    }
}
