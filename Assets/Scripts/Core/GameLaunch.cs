using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AssetBundles;
using GameChannel;
using Common;
public class GameLaunch : MonoSingleton<GameLaunch> {

    private bool _InitComplete = false;
    public bool InitComplete { get { return _InitComplete;
        } }
    new void Awake() {
        // 调用基类 Awake 确保单例初始化
        base.Awake();

        // 初始化框架
        this.gameObject.AddComponent<AssetBundleManager>(); // 实例化一个AssetBudnleManager;
        //this.gameObject.AddComponent<show_fps>();
        Application.targetFrameRate = 60;
        this.gameObject.AddComponent<ResMgr>();

        // 初始化全局输入管理器
        this.gameObject.AddComponent<GlobalInputManager>();
        // end
    }

    IEnumerator InitPackageName()
    {
#if UNITY_EDITOR
        if (AssetBundleConfig.IsEditorMode)
        {
            yield break;
        }
#endif
        var packageNameRequest = AssetBundleManager.Instance.RequestAssetFileAsync(BuildUtils.PackageNameFileName);
        yield return packageNameRequest; // 中断当前协程，直到请求结束;
        var packageName = packageNameRequest.text;
        packageNameRequest.Dispose(); // 释放请求;

        AssetBundleManager.ManifestBundleName = packageName; // 包名字;
        ChannelManager.instance.Init(packageName);
        yield break;
    }

    IEnumerator CheckAndDownload() {
        // 如果已经是最新的，直接返回就可以了;
        // end 

        // 更新的资源包的下载; 直接下载, 最新的ab包;
        // 根据版本，拉取要下载文件列表，然后来一个个下载, 下载完成后直接进入游戏，即可;
        // 检车更新;
        var downloadRequest = AssetBundleManager.Instance.DownloadAssetBundleAsync("lua.assetbundle");
        yield return downloadRequest;
        GameUtility.SafeWriteAllBytes(AssetBundleUtility.GetPersistentDataPath() + "/lua.assetbundle", downloadRequest.bytes);
        downloadRequest.Dispose();
        // end 
        yield break;
    }

    IEnumerator GameStart()
    {

        var start = DateTime.Now;
        yield return InitPackageName();

        // 启动资源管理模块
        start = DateTime.Now;
        yield return AssetBundleManager.Instance.Initialize();

        // 启动检测更新
        //yield return CheckAndDownload();
        // end


        _InitComplete = true;

        // GameManager 已挂载在场景中，其 Awake 会自动调用 Init()
        // Init() 中的协程会等待 InitComplete 为 true 后再继续执行
        // 因此这里不需要手动调用任何初始化方法

        yield break;
    }

	void Start () {
        this.StartCoroutine(this.GameStart());
        

    }
	
	void Update () {
		
	}
}
