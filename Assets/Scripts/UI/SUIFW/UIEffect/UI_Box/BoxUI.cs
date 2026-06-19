using UnityEngine;
using UnityEngine.UIElements;

public class BoxUI : MonoBehaviour
{
    public Animator animator;
    public void StartPlay()
    {
        // 播放点击音效
        AudioSourceManager.Instance.PlayButtonClip();
        // 播放开箱音效
        AudioSourceManager.Instance.PlayBoxOpenClip();

        animator.Play("box-001");
    }
    public void PlayNormal()
    {
        animator.Play("normal");
    }
}
