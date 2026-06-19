using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;

namespace AssetBundles
{
	internal class LaunchAssetBundleServer : ScriptableSingleton<LaunchAssetBundleServer>
	{

		[SerializeField]
		int mServerPID = 0;

		private static double lastUrlWriteTime = 0;
		private const double MIN_URL_WRITE_INTERVAL = 3.0; // 最少3秒间隔
        
        public static void CheckAndDoRunning()
        {
            bool needRunning = AssetBundleConfig.IsSimulateMode;
            bool isRunning = IsRunning();
            if (needRunning != isRunning)
            {
                if (needRunning)
                {
                    Run();
                }
                else
                {
                    KillRunningAssetBundleServer();
                }
            }
        }
        
		static bool IsRunning ()
		{
            if (instance.mServerPID == 0)
            {
                return false;
            }

            try
            {
                var process = Process.GetProcessById(instance.mServerPID);
                if (process == null)
                {
                    return false;
                }

                return !process.HasExited;
            }
            catch
            {
                return false;
            }
		}
        
		static void KillRunningAssetBundleServer ()
		{
			try
			{
				if (instance.mServerPID == 0)
					return;

				var lastProcess = Process.GetProcessById (instance.mServerPID);
				lastProcess.Kill();
				instance.mServerPID = 0;
            }
			catch
			{
			}
		}

		static void Run ()
		{
			KillRunningAssetBundleServer();
			WriteAssetBundleServerURL();

			string args = string.Format("\"{0}\" {1}", AssetBundleConfig.LocalSvrAppWorkPath, Process.GetCurrentProcess().Id);
            ProcessStartInfo startInfo = ExecuteInternalMono.GetProfileStartInfoForMono(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), GetMonoProfileVersion(), AssetBundleConfig.LocalSvrAppPath, args, true);
            startInfo.WorkingDirectory = AssetBundleConfig.LocalSvrAppWorkPath;
			startInfo.UseShellExecute = false;
			Process launchProcess = Process.Start(startInfo);
			if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
			{
                UnityEngine.Debug.LogError ("无法启动AssetBundle服务器进程!");
			}
			else
			{
				instance.mServerPID = launchProcess.Id;
            }
        }

        static string GetMonoProfileVersion()
        {
            string path = Path.Combine(Path.Combine(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), "lib"), "mono");

            string[] folders = Directory.GetDirectories(path);
            string[] foldersWithApi = folders.Where(f => f.Contains("-api")).ToArray();
            string profileVersion = "0";

            for (int i = 0; i < foldersWithApi.Length; i++)
            {
                foldersWithApi[i] = foldersWithApi[i].Split(Path.DirectorySeparatorChar).Last();
                foldersWithApi[i] = foldersWithApi[i].Split('-').First();
                
                if (string.Compare(foldersWithApi[i], profileVersion) > 0)
                {
                    profileVersion = foldersWithApi[i];
                }
            }

            return profileVersion;
        }

        public static string GetStreamingAssetBundleServerUrl()
        {
            string assetBundleServerUrl = Path.Combine(Application.streamingAssetsPath, AssetBundleConfig.AssetBundlesFolderName);
            assetBundleServerUrl = Path.Combine(assetBundleServerUrl, AssetBundleConfig.AssetBundleServerUrlFileName);
            return assetBundleServerUrl;
        }

        public static void WriteAssetBundleServerURL()
        {
            // 防护：避免频繁写入导致无限循环
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastUrlWriteTime < MIN_URL_WRITE_INTERVAL)
            {
                return;
            }

            var path = GetStreamingAssetBundleServerUrl();
            string newUrl = GetAssetBundleServerURL();

            // 防护：如果内容没变，不写入
            if (File.Exists(path))
            {
                string existingUrl = File.ReadAllText(path);
                if (existingUrl == newUrl)
                {
                    return;
                }
            }

            GameUtility.SafeWriteAllText(path, newUrl);
            lastUrlWriteTime = currentTime;
            AssetDatabase.Refresh();
        }

        public static void ClearAssetBundleServerURL()
        {
            var path = GetStreamingAssetBundleServerUrl();
            GameUtility.SafeDeleteFile(path);
            AssetDatabase.Refresh();
        }

        public static string GetAssetBundleServerURL()
        {
            string downloadURL = string.Empty;
            // 注意：这里获取所有内网地址后选择一个最小的，因为可能存在虚拟机网卡
            var ips = new List<string>();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips.Add(ip.ToString());
                }
            }
            ips.Sort();
            if (ips.Count <= 0)
            {
                UnityEngine.Debug.LogError("获取内网IP地址失败!");
            }
            else
            {
                downloadURL = "http://" + ips[0] + ":7888/";
                downloadURL = downloadURL + PackageUtils.GetCurPlatformChannelPath() + "/";
            }
            return downloadURL;
        }
    }
}