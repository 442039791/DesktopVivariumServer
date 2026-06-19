
using UnityEngine;

using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Collections;
using System.IO;
using SUIFW;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Security.Cryptography;
using AssetBundles;

public class Test_QC : MonoBehaviour, Test_QcInterface
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string fileID;
    public Image image;
    public AudioSource audioSource;
    public AudioClip musicClip;
    public AudioClip musicClip2;
    public Button button;
    public Test_QcInterface AsInterface { get; private set; }

    void Start()
    {
        button.onClick.AddListener(()=>audioSource.Play());
        //(this as Test_QcInterface).Test_interface(null, null);
        StartCoroutine(Test3());
        //Test2(fileID, "AnimalData.csv");
        //BaseInfo_info info = new BaseInfo_info();
        //info.SetHour("+99/h");
        //info.SetRatio("99/100");
        //event_manager.instance.dispatch_UIevent(SysDefine.UI_BioTankInfo_BaseInfo_info_O2, info);


    }
    IEnumerator Test3()
    {
        yield return new WaitUntil(() => GameLaunch.Instance.InitComplete);
        AssetBundleManager.Instance.SetAssetBundleResident("image.assetbundle", true);
        ResMgr.Instance.LoadAssetBundleAsync("image.assetbundle", () =>
        {

            Sprite[] sprites = ResMgr.Instance.GetAssetBundle("image.assetbundle").LoadAllAssets<Sprite>();
#if UNITY_EDITOR || DEBUG
            foreach (var sprite in sprites)
            {
            }
#endif

            Sprite texture = ResMgr.Instance.GetAssetCache("Image/Test1.png", null) as Sprite;

            if (texture == null)
            {
#if UNITY_EDITOR || DEBUG
                Debug.LogError("Failed to load Texture2D from AssetBundle!");
#endif
                return;
            }
            //Sprite sprite = Sprite.Create(
            //texture,
            //new Rect(0, 0, texture.width, texture.height),
            //new Vector2(0.5f, 0.5f)
            //);
            image.sprite = texture;


        });
        AssetBundleManager.Instance.SetAssetBundleResident("sound.assetbundle", true);
        ResMgr.Instance.LoadAssetBundleAsync("sound.assetbundle", () =>
        {
            //var ab = ResMgr.Instance.GetAssetBundle("sound.assetbundle");
            
            //var clip = ab.LoadAsset<AudioClip>("Assets/AssetsPackage/Sound/Test.mp3");
            // ���� AudioSource ���

            musicClip = ResMgr.Instance.GetAssetCache("Sound/Test.mp3", null) as AudioClip;
            //ab.Unload(false);


            if (musicClip == null)
            {
#if UNITY_EDITOR || DEBUG
                Debug.LogError("Failed to load AudioClip from AssetBundle!");
#endif
                return;
            }
            if (!(musicClip is AudioClip))
            {
#if UNITY_EDITOR || DEBUG
                Debug.LogError("Loaded object is not an AudioClip!");
#endif
                return;
            }
#if UNITY_EDITOR || DEBUG

            string audioClipHash = GetAudioClipHash(musicClip);
            string audioClipHash1 = GetAudioClipHash(musicClip2);
#endif
            // ��������
            audioSource.clip = musicClip;
            audioSource.Play();
        });
    }
    string GetAudioClipHash(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using (var md5 = MD5.Create())
        {
            byte[] bytes = new byte[samples.Length * sizeof(float)];
            Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);
            byte[] hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public AnimalData Test2(string fileID,string filename)
    {
        //var fishdata = GameConfigDataBase.GetConfigData<AnimalData>(fileID, filename);
        return null;
    }
    public void Test1()
    {
        string input = ResourceManager.GetConfigFile("test.csv");

        StringReader sr = new StringReader(input);

        var fileDataLine = sr.ReadLine();

        // �������ʽƥ������ŵ��ֶκ���ͨ�ֶ�
        var matches = Regex.Matches(fileDataLine, @"\""\[[^\]]*\]\""|[^,]+");
        string[] result = matches
            .Cast<Match>()
            .Select(m => m.Value.Trim('"').Trim('[', ']')) // ȥ��˫���źͷ�����
            .ToArray();

#if UNITY_EDITOR || DEBUG
        int i = 0;
        foreach (var item in result)
        {
            i++;
        }
        i = 0;
        foreach (var item in result)
        {
            string[] t = item.Split(",");
            foreach (var t2 in t)
            {
                i++;
            }

        }
#endif
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
public interface Test_QcInterface
{
    public Test_QcInterface AsInterface {  get;}
    public void Test_interface(string fileID, string filename)
    {
    }
    
}