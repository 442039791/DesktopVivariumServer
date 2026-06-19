using UnityEngine;
using Common;
using AssetBundles;
using System.Collections;
public class AudioSourceManager : MonoSingleton<AudioSourceManager>
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    [System.NonSerialized]
    public AudioClip buttonClip;
    [System.NonSerialized]
    public AudioClip boxOpenClip;
    private int currentIndex = 0;
    private bool isButtonClipLoaded = false;
    void Start()
    {
        audioSource.loop = true;
        currentIndex = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[currentIndex];
        audioSource.Play();
        StartCoroutine(WaitAndLoadButtonClip());
    }

    private IEnumerator WaitAndLoadButtonClip()
    {
        // 等待 GameLaunch 初始化完成
        while (GameLaunch.Instance == null || !GameLaunch.Instance.InitComplete)
        {
            yield return null;
        }
        LoadButtonClipFromAB();
    }

    private void LoadButtonClipFromAB()
    {
        AssetBundleManager.Instance.SetAssetBundleResident("sound.assetbundle", true);
        ResMgr.Instance.LoadAssetBundleAsync("sound.assetbundle", () =>
        {
            var clip = ResMgr.Instance.GetAssetCache("Sound/ButtonClick.mp3", "AudioClip") as AudioClip;
            if (clip != null)
            {
                buttonClip = clip;
                isButtonClipLoaded = true;
            }
            var boxClip = ResMgr.Instance.GetAssetCache("Sound/BoxOpen.mp3", "AudioClip") as AudioClip;
            if (boxClip != null)
            {
                boxOpenClip = boxClip;
            }
        });
    }
    public void PlayRandomClip()
    {
        currentIndex = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[currentIndex];
        audioSource.Play();
    }
    public void PlayNextClip()
    {
        currentIndex = (currentIndex + 1) % audioClips.Length;
        audioSource.clip = audioClips[currentIndex];
        audioSource.Play();
    }
    public void PlayPreviousClip()
    {
        currentIndex = (currentIndex - 1 + audioClips.Length) % audioClips.Length;
        audioSource.clip = audioClips[currentIndex];
        audioSource.Play();
    }
    public void PlayButtonClip()
    {
        if (buttonClip != null)
        {
            audioSource.PlayOneShot(buttonClip);
        }
    }
    public void PlayBoxOpenClip()
    {
        if (boxOpenClip != null)
        {
            audioSource.PlayOneShot(boxOpenClip);
        }
    }
    private bool isPlaying = true;
    public void StopOrPlay()
    {
        if (isPlaying)
        {
            audioSource.Stop();
            isPlaying = false;
        }
        else
        {
            audioSource.Play();
            isPlaying = true;
        }
        
    }
    public void PlayClip(int index)
    {
        currentIndex = index;
        audioSource.clip = audioClips[currentIndex];
        audioSource.Play();
    }
}
