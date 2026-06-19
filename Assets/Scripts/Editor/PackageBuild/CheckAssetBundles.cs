using UnityEngine;
using System.Collections;
using AssetBundles;
using UnityEditor;

/// <summary>
/// added by wsh @ 2018.01.03
/// 功能：打包前的AB检测工作
/// </summary>

public static class CheckAssetBundles
{
    public static void SwitchChannel(string channelName)
    {
        var channelFolderPath = AssetBundleUtility.PackagePathToAssetsPath(AssetBundleConfig.ChannelFolderName);
        var guids = AssetDatabase.FindAssets("t:textAsset", new string[] { channelFolderPath });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            GameUtility.SafeWriteAllText(path, channelName); // 将渠道的名字保存到我们的每隔ab包通用的文本(test);
        }
        AssetDatabase.Refresh();
    }

    public static void ClearAllAssetBundles()
    {
        var assebundleNames = AssetDatabase.GetAllAssetBundleNames();
        var length = assebundleNames.Length;
        var count = 0;
        foreach (var assetbundleName in assebundleNames)
        {
            count++;
            EditorUtility.DisplayProgressBar("Remove assetbundle name :", assetbundleName, (float)count / length);
            AssetDatabase.RemoveAssetBundleName(assetbundleName, true);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        assebundleNames = AssetDatabase.GetAllAssetBundleNames();
        if (assebundleNames.Length != 0)
        {
            Debug.LogError("Something wrong!!!");
        }
    }

    public static void RunAllCheckers()
    {
        // 读取数据库下面的所有的.asset文件内容;
        // Assets/Editor/AssetBundle/Database";
        var guids = AssetDatabase.FindAssets("t:AssetBundleDispatcherConfig", new string[] { AssetBundleInspectorUtils.DatabaseRoot });
        var length = guids.Length;
        var count = 0;
        foreach (var guid in guids) // 遍历每隔 数据库路径下的.asset文件;
        {
            count++;
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 从这个数据库文件里面读取了config对象;  AssetBundleDispatcherConfig
            var config = AssetDatabase.LoadAssetAtPath<AssetBundleDispatcherConfig>(assetPath);
            config.Load();
            EditorUtility.DisplayProgressBar("Run checker :", config.PackagePath, (float)count / length);

            // 检查对应的资源包所在的文件夹是否存在;
            AssetBundleDispatcher.Run(config);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    public static void Run()
    {
        // XLuaMenu: 拷贝Lua代码到 我们的资源包里面;
        //XLuaMenu.CopyLuaFilesToAssetsPackage();

        // 清理掉所有的不用的过时的asseetbunle的名字；
        ClearAllAssetBundles();
        RunAllCheckers();
    }
}
