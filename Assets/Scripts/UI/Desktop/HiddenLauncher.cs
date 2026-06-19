using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Common;
public class HiddenProcessLauncher : MonoSingleton<HiddenProcessLauncher>
{
    // 定义 STARTUPINFO 结构体
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public ushort wShowWindow;
        public ushort cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    // 定义 PROCESS_INFORMATION 结构体
    [StructLayout(LayoutKind.Sequential)]
    struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    // 导入 CreateProcess 函数
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation
    );

    // 导入 CloseHandle 函数
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);

    // 常量定义
    const uint CREATE_NO_WINDOW = 0x08000000;
    const ushort SW_HIDE = 0;
    const ushort SW_SHOW = 5;
    const uint STARTF_USESHOWWINDOW = 0x00000001;

    /// <summary>
    /// 启动被隐藏的 Unity 应用程序
    /// </summary>
    /// <param name="exePath">被启动的 Unity 程序的完整路径</param>
    /// <param name="arguments">命令行参数（可选）</param>
    public void LaunchHiddenProcess(string exePath, string arguments = "")
    {
        STARTUPINFO si = new STARTUPINFO();
        si.cb = Marshal.SizeOf(si);
        si.dwFlags = STARTF_USESHOWWINDOW;
        si.wShowWindow = SW_HIDE;

        PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

        // 构建命令行（包含参数）
        string commandLine = string.IsNullOrEmpty(arguments)
            ? $"\"{exePath}\""
            : $"\"{exePath}\" {arguments}";

        bool result = CreateProcess(
            null,                            // lpApplicationName
            commandLine,                     // lpCommandLine
            IntPtr.Zero,                     // lpProcessAttributes
            IntPtr.Zero,                     // lpThreadAttributes
            false,                           // bInheritHandles
            CREATE_NO_WINDOW,                // dwCreationFlags
            IntPtr.Zero,                     // lpEnvironment
            null,                            // lpCurrentDirectory
            ref si,                          // lpStartupInfo
            out pi                           // lpProcessInformation
        );

        if (result)
        {
            // 进程启动成功，关闭句柄以避免泄漏
            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);
            Debug.Log($"[HiddenProcessLauncher] 启动成功: {commandLine}");
        }
        else
        {
            // 进程启动失败，获取错误码
            int error = Marshal.GetLastWin32Error();
            Debug.LogError($"启动程序失败：{exePath}。错误码：{error}");
        }
    }

    /// <summary>
    /// 启动客户端进程（用于生态箱或休眠池）
    /// 服务端会在客户端连接后分配ID
    /// </summary>
    public void LaunchHiddenProcess()
    {
        LaunchHiddenProcess(GlobalSetting.Instance.GetSecondUnityExePath());
    }

    public string secondUnityExePath = @"D:\unityapp\DesktopTerrarium-f - 副本\Build\DesktopTerrarium.exe";
    // 示例调用

}
