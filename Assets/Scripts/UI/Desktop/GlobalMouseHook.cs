using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class GlobalMouseHook : MonoBehaviour
{
    private static IntPtr _hookID = IntPtr.Zero;

    // 低级鼠标委托类型
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelMouseProc _proc;

    // 定义我们需要的 Windows API 函数
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelMouseProc lpfn,
        IntPtr hMod,
        uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("USER32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    // 鼠标消息常量
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_MOUSEWHEEL = 0x020A;
    private const int WM_MOUSEHWHEEL = 0x020E;

    // 定义我们需要的 Windows API 函数
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(
        IntPtr hWnd,
        out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetClassName(
        IntPtr hWnd,
        StringBuilder lpClassName,
        int nMaxCount);

    // 用于存储鼠标事件的结构
    private struct MouseEvent
    {
        public string eventType;
        public int delta; // 滚轮增量
        public bool isHorizontal; // 是否为水平滚轮
    }

    // 定义 MSLLHOOKSTRUCT 结构体
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }


    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);
    // 线程安全的鼠标事件队列
    private static ConcurrentQueue<MouseEvent> mouseQueue = new ConcurrentQueue<MouseEvent>();

    void Start()
    {
#if UNITY_EDITOR || MY_CUSTOM_MACRO
        return;
#endif

    }

    void OnDestroy()
    {
#if UNITY_EDITOR || MY_CUSTOM_MACRO
        return;
#endif
        // 卸载全局鼠标钩子

    }
    public static Vector2Int GetMousePoint()
    {
        POINT p;
        GetCursorPos(out p);
        return new Vector2Int(p.x, p.y);
    }
    private IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            IntPtr moduleHandle = GetModuleHandle(curModule.ModuleName);
            return SetWindowsHookEx(WH_MOUSE_LL, proc, moduleHandle, 0);
        }
    }
    const int VK_LBUTTON = 0x01;  // 左键
    const int VK_RBUTTON = 0x02;  // 右键
    const int VK_MBUTTON = 0x04;  // 中键

    void Update()
    {
        // 已注销所有鼠标监听操作,避免窗口被意外移动
        return;

//#if UNITY_EDITOR || MY_CUSTOM_MACRO
//        return;
//#endif

            //short stateLeft = GetAsyncKeyState(VK_LBUTTON);
            //bool isLeftButtonDown = (stateLeft & 0x8000) != 0;  // 高位为 0x8000 表示按下

            //// 只在按键从未按下变为按下时触发（边缘检测）
            //if (isLeftButtonDown && !wasLeftButtonDown)
            //{
            //    // 获取点击时的窗口句柄
            //    IntPtr hwnd = GetForegroundWindow();
            //    uint processId;
            //    uint threadId = GetWindowThreadProcessId(hwnd, out processId);

            //    // 获取窗口类名
            //    StringBuilder className = new StringBuilder(256);
            //    GetClassName(hwnd, className, className.Capacity);

            //    string clsName = className.ToString();

            //    // 判断是否为桌面窗口
            //    if (IsDesktopFocused(clsName))
            //    {
            //        // 触发按钮点击事件
            //        DesktopButtonManager.Button?.OnClick();

            //        if(BioTankRegistry.Instance.IsInTank)
            //        {
            //            var point = GetMousePoint();
            //            WindowManager.Instance.MouseMoveWindow(point);
            //        }
            //    }


            //}

            //// 更新按键状态
            //wasLeftButtonDown = isLeftButtonDown;
    }

    /// <summary>
    /// 判断当前焦点是否在桌面窗口
    /// </summary>
    /// <param name="className">当前窗口的类名</param>
    /// <returns>如果焦点在桌面窗口，则返回 true；否则，返回 false</returns>
    private bool IsDesktopFocused(string className)
    {
        // 桌面窗口的类名列表，同时允许 Unity 窗口（UnityWndClass）和UWP窗口（Windows.UI.Core.CoreWindow）
        return className == "Progman"
            || className == "WorkerW"
            || className == "UnityWndClass"
            || className == "Windows.UI.Core.CoreWindow";
    }
}
