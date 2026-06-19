using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Common;

public class GlobalKeyboardHook : MonoSingleton<GlobalKeyboardHook>
{
    private static IntPtr hookId = IntPtr.Zero;
    private static LowLevelKeyboardProc proc;
    private static ConcurrentQueue<KeyEvent> keyQueue = new ConcurrentQueue<KeyEvent>();

    // 定义委托类型
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // 定义事件常量
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    // 声明所需要的 Windows API 函数
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    // 声明获取窗口信息所需要的 Windows API 函数
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

    // 维护按键状态
    private static Dictionary<KeyCode, bool> keyStates = new Dictionary<KeyCode, bool>();
    private static Dictionary<KeyCode, bool> prevKeyStates = new Dictionary<KeyCode, bool>();

    void Start()
    {
#if !UNITY_EDITOR && !MY_CUSTOM_MACRO
        // 安装全局键盘钩子
        proc = HookCallback;
        hookId = SetHook(proc);
#endif
    }

    void OnDestroy()
    {
#if !UNITY_EDITOR && !MY_CUSTOM_MACRO
        // 卸载全局键盘钩子
        UnhookWindowsHookEx(hookId);
#endif
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private struct KeyEvent
    {
        public KeyCode keyCode;
        public bool isKeyDown;
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                bool isKeyDown = false;

                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    isKeyDown = true;
                }
                else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                {
                    isKeyDown = false;
                }

                if (isKeyDown || !isKeyDown)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    KeyCode key = KeyCodeMapper.GetUnityKeyCode(vkCode);

                    // 检查当前窗口是否为桌面
                    if (IsDesktopFocused())
                    {
                        keyQueue.Enqueue(new KeyEvent { keyCode = key, isKeyDown = isKeyDown });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"钩子回调异常：{ex.Message}");
        }

        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }

    void Update()
    {
#if !UNITY_EDITOR && !MY_CUSTOM_MACRO
        // 保存前一帧的按键状态
        foreach (var key in keyStates.Keys)
        {
            if (!prevKeyStates.ContainsKey(key))
            {
                prevKeyStates[key] = false;
            }
            prevKeyStates[key] = keyStates[key];
        }

        // 处理钩子捕获的按键事件
        while (keyQueue.TryDequeue(out KeyEvent keyEvent))
        {
            // 更新按键状态
            if (!keyStates.ContainsKey(keyEvent.keyCode))
            {
                keyStates[keyEvent.keyCode] = false;
            }

            keyStates[keyEvent.keyCode] = keyEvent.isKeyDown;

            // 示例：检测 Esc 键并退出程序
            if (keyEvent.keyCode == KeyCode.Escape && keyEvent.isKeyDown)
            {
                Application.Quit();
            }
            else
            {
                // 可以在这里添加其他按键事件
                // 例如，可以触发某些动作或自定义逻辑
                // 例如：
                // if (keyEvent.keyCode == KeyCode.A && keyEvent.isKeyDown)
                // {
                //     // 执行某个操作
                // }
            }
        }
#endif
    }

    /// <summary>
    /// 判断当前焦点是否在桌面窗口
    /// </summary>
    /// <returns>如果在桌面窗口，返回 true；否则，返回 false</returns>
    private static bool IsDesktopFocused()
    {
        IntPtr hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return false;

        StringBuilder className = new StringBuilder(256);
        GetClassName(hwnd, className, className.Capacity);
        string clsName = className.ToString();

        // 检查是否是桌面窗口类名
        if (clsName == "Progman" || clsName == "WorkerW")
            return true;

        // 检查是否是当前Unity应用的窗口
        uint windowProcessId;
        GetWindowThreadProcessId(hwnd, out windowProcessId);
        uint currentProcessId = (uint)Process.GetCurrentProcess().Id;

        // 如果前台窗口是当前进程,也认为是桌面
        return windowProcessId == currentProcessId;
    }

    // 提供公共方法来获取按键状态
    public static bool GetKey(KeyCode key)
    {
        if (keyStates.ContainsKey(key))
        {
            return keyStates[key];
        }
        return false;
    }

    public static bool GetKeyDown(KeyCode key)
    {
        if (keyStates.ContainsKey(key) && prevKeyStates.ContainsKey(key))
        {
            return keyStates[key] && !prevKeyStates[key];
        }
        return false;
    }

    public static bool GetKeyUp(KeyCode key)
    {
        if (keyStates.ContainsKey(key) && prevKeyStates.ContainsKey(key))
        {
            return !keyStates[key] && prevKeyStates[key];
        }
        return false;
    }
}

public static class CustomInput
{
    /// <summary>
    /// 检测指定按键是否被按下。
    /// </summary>
    public static bool GetKey(KeyCode key)
    {
#if UNITY_EDITOR || MY_CUSTOM_MACRO
        return Input.GetKey(key);
#else
        return GlobalKeyboardHook.GetKey(key);
#endif
    }

    /// <summary>
    /// 检测指定按键是否在当前帧被按下。
    /// </summary>
    public static bool GetKeyDown(KeyCode key)
    {
#if UNITY_EDITOR || MY_CUSTOM_MACRO
        return Input.GetKeyDown(key);
#else
        return GlobalKeyboardHook.GetKeyDown(key);
#endif
    }

    /// <summary>
    /// 检测指定按键是否在当前帧被释放。
    /// </summary>
    public static bool GetKeyUp(KeyCode key)
    {
#if UNITY_EDITOR || MY_CUSTOM_MACRO
        return Input.GetKeyUp(key);
#else
        return GlobalKeyboardHook.GetKeyUp(key);
#endif
    }

    // 鼠标滚轮增量
    private static float scrollDelta = 0f;

    /// <summary>
    /// 设置当前帧鼠标滚轮增量。
    /// </summary>
    public static void SetScrollDelta(float delta)
    {
        scrollDelta += delta;
    }

    /// <summary>
    /// 获取当前帧鼠标滚轮增量。
    /// </summary>
    /// <returns>滚轮轴的滚动增量</returns>
    public static float GetAxis(string axisName)
    {
        if (axisName == "Mouse ScrollWheel")
        {
#if UNITY_EDITOR || MY_CUSTOM_MACRO
            return Input.GetAxis("Mouse ScrollWheel");
#else
        return scrollDelta;
#endif

        }

        // 对于其他轴的请求，可以根据需要扩展
        return 0f;
    }

    /// <summary>
    /// 重置滚轮增量（通常在每帧更新时调用）
    /// </summary>
    public static void ResetScrollDelta()
    {
        scrollDelta = 0f;
    }
}

public static class KeyCodeMapper
{
    private static readonly Dictionary<int, KeyCode> vkCodeToKeyCode = new Dictionary<int, KeyCode>()
    {
        // 字母键
        { 65, KeyCode.A },
        { 66, KeyCode.B },
        { 67, KeyCode.C },
        { 68, KeyCode.D },
        { 69, KeyCode.E },
        { 70, KeyCode.F },
        { 71, KeyCode.G },
        { 72, KeyCode.H },
        { 73, KeyCode.I },
        { 74, KeyCode.J },
        { 75, KeyCode.K },
        { 76, KeyCode.L },
        { 77, KeyCode.M },
        { 78, KeyCode.N },
        { 79, KeyCode.O },
        { 80, KeyCode.P },
        { 81, KeyCode.Q },
        { 82, KeyCode.R },
        { 83, KeyCode.S },
        { 84, KeyCode.T },
        { 85, KeyCode.U },
        { 86, KeyCode.V },
        { 87, KeyCode.W },
        { 88, KeyCode.X },
        { 89, KeyCode.Y },
        { 90, KeyCode.Z },

        // 数字键
        { 48, KeyCode.Alpha0 },
        { 49, KeyCode.Alpha1 },
        { 50, KeyCode.Alpha2 },
        { 51, KeyCode.Alpha3 },
        { 52, KeyCode.Alpha4 },
        { 53, KeyCode.Alpha5 },
        { 54, KeyCode.Alpha6 },
        { 55, KeyCode.Alpha7 },
        { 56, KeyCode.Alpha8 },
        { 57, KeyCode.Alpha9 },

        // 功能键
        { 112, KeyCode.F1 },
        { 113, KeyCode.F2 },
        { 114, KeyCode.F3 },
        { 115, KeyCode.F4 },
        { 116, KeyCode.F5 },
        { 117, KeyCode.F6 },
        { 118, KeyCode.F7 },
        { 119, KeyCode.F8 },
        { 120, KeyCode.F9 },
        { 121, KeyCode.F10 },
        { 122, KeyCode.F11 },
        { 123, KeyCode.F12 },

        // 方向键
        { 37, KeyCode.LeftArrow },
        { 38, KeyCode.UpArrow },
        { 39, KeyCode.RightArrow },
        { 40, KeyCode.DownArrow },

        // 控制键
        { 27, KeyCode.Escape },
        { 32, KeyCode.Space },
        { 13, KeyCode.Return },
        { 9, KeyCode.Tab },
        { 16, KeyCode.LeftShift },
        { 17, KeyCode.LeftControl },
        { 18, KeyCode.LeftAlt },

        // 其他符号键
        { 186, KeyCode.Semicolon },
        { 187, KeyCode.Equals },
        { 188, KeyCode.Comma },
        { 189, KeyCode.Minus },
        { 190, KeyCode.Period },
        { 191, KeyCode.Slash },
        { 192, KeyCode.BackQuote },
        { 219, KeyCode.LeftBracket },
        { 220, KeyCode.Backslash },
        { 221, KeyCode.RightBracket },
        { 222, KeyCode.Quote }

        // 如果需要可以添加更多映射
    };

    /// <summary>
    /// 将虚拟键码转换为 Unity 的 KeyCode。
    /// </summary>
    /// <param name="vkCode">虚拟键码 (Virtual-Key Code)</param>
    /// <returns>对应的 Unity KeyCode，如果未找到映射，则返回 KeyCode.None</returns>
    public static KeyCode GetUnityKeyCode(int vkCode)
    {
        if (vkCodeToKeyCode.TryGetValue(vkCode, out KeyCode keyCode))
        {
            return keyCode;
        }
        return KeyCode.None;
    }
}
