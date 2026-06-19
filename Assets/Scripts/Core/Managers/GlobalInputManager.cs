using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using UnityEngine;
using Common;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 全局输入管理器 - 独立于UI系统的输入监听
/// 用途:在窗口失去焦点时仍能监听键盘和鼠标输入
/// 限制:仅监听,不拦截(CallNextHookEx确保事件继续传递)
/// </summary>
public class GlobalInputManager : MonoSingleton<GlobalInputManager>
{
    #region Windows API - 键盘

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelKeyboardProc _keyboardProc;
    private static IntPtr _keyboardHookId = IntPtr.Zero;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    // 键盘钩子常量
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    #endregion

    #region Windows API - 鼠标

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelMouseProc _mouseProc;
    private static IntPtr _mouseHookId = IntPtr.Zero;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    // 鼠标钩子常量(仅定义需要的)
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    // ⚠️ 故意不定义 WM_MOUSEMOVE (0x0200) 和 WM_MOUSEWHEEL (0x020A) 等

    #endregion

    #region Windows API - 窗口焦点检测

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    #endregion

    #region 键盘状态管理

    private struct KeyEvent
    {
        public KeyCode keyCode;
        public bool isKeyDown;
    }

    private static ConcurrentQueue<KeyEvent> _keyQueue = new ConcurrentQueue<KeyEvent>();
    private static Dictionary<KeyCode, bool> _keyStates = new Dictionary<KeyCode, bool>();
    private static Dictionary<KeyCode, bool> _prevKeyStates = new Dictionary<KeyCode, bool>();

    // 需要监控的常用按键列表
    private static readonly KeyCode[] _commonKeys = new KeyCode[]
    {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.X, KeyCode.Z,  // 上下移动按键
        KeyCode.Space, KeyCode.LeftShift, KeyCode.LeftControl,
        KeyCode.Escape, KeyCode.Q, KeyCode.E, KeyCode.R, KeyCode.F,
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Tab, KeyCode.LeftAlt,
        KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow  // 方向键
    };

    #endregion

    #region 鼠标状态管理

    // 鼠标按下标志(单帧有效,读取后自动清零)
    private static bool _leftButtonPressed = false;
    private static bool _rightButtonPressed = false;

    #endregion

    #region Unity生命周期

    void Start()
    {
#if !UNITY_EDITOR
        // 安装键盘钩子
        _keyboardProc = KeyboardHookCallback;
        _keyboardHookId = SetKeyboardHook(_keyboardProc);

        if (_keyboardHookId == IntPtr.Zero)
        {
            Debug.LogError("[GlobalInputManager] 键盘钩子安装失败!错误代码: " + Marshal.GetLastWin32Error());
        }
        else
        {
        }

        // 注意: 鼠标钩子已禁用，因为会导致鼠标移动延迟
#else
#endif
    }

    void OnDestroy()
    {
#if !UNITY_EDITOR
        // 卸载键盘钩子
        if (_keyboardHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHookId);
        }
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        // 编辑器模式使用Unity Input
        UpdateKeyStatesFromUnityInput();
        UpdateMouseFromUnityInput();
#else
        // 打包模式：检测当前窗口焦点
        // 如果是本进程窗口在前台，使用Unity Input（因为钩子在自己窗口前台时不工作）
        // 如果是其他窗口在前台，使用钩子
        if (IsCurrentProcessFocused())
        {
            // 本进程窗口在前台，钩子不会触发，使用Unity Input
            UpdateKeyStatesFromUnityInput();
            UpdateMouseFromUnityInput();
            // 清空钩子队列，避免切换窗口时的残留事件
            while (_keyQueue.TryDequeue(out _)) { }
        }
        else
        {
            // 其他窗口在前台，使用钩子
            UpdateKeyStatesFromHook();
        }
#endif
    }

    /// <summary>
    /// 检测当前进程的窗口是否在前台
    /// </summary>
    private bool IsCurrentProcessFocused()
    {
        IntPtr hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return false;

        uint windowProcessId;
        GetWindowThreadProcessId(hwnd, out windowProcessId);
        uint currentProcessId = (uint)Process.GetCurrentProcess().Id;

        return windowProcessId == currentProcessId;
    }

    #endregion

    #region 钩子安装

    private IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private IntPtr SetMouseHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    #endregion

    #region 钩子回调

    /// <summary>
    /// 键盘钩子回调 - 用于捕获其他窗口前台时的键盘输入
    /// </summary>
    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                bool isKeyDown = (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN);

                int vkCode = Marshal.ReadInt32(lParam);
                KeyCode key = KeyCodeMapper.GetUnityKeyCode(vkCode);

                // 无论窗口是否有焦点,都捕获按键（但实际上本进程窗口前台时钩子不会触发）
                _keyQueue.Enqueue(new KeyEvent { keyCode = key, isKeyDown = isKeyDown });
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GlobalInputManager] 键盘钩子回调异常: {ex.Message}");
        }

        return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// 鼠标钩子回调 - 只检测按下事件,忽略所有其他事件
    /// </summary>
    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();

                // ✅ 只处理按下事件
                if (msg == WM_LBUTTONDOWN)
                {
                    _leftButtonPressed = true;
                }
                else if (msg == WM_RBUTTONDOWN)
                {
                    _rightButtonPressed = true;
                }
                // ⚠️ 其他所有消息(UP, MOVE, WHEEL等)都忽略
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GlobalInputManager] 鼠标钩子回调异常: {ex.Message}");
        }

        return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
    }

    #endregion

    #region 状态更新

    /// <summary>
    /// 从钩子队列更新键盘状态
    /// </summary>
    private void UpdateKeyStatesFromHook()
    {
        // 保存前一帧状态
        foreach (var key in _keyStates.Keys)
        {
            if (!_prevKeyStates.ContainsKey(key))
                _prevKeyStates[key] = false;
            _prevKeyStates[key] = _keyStates[key];
        }

        // 处理钩子捕获的事件
        while (_keyQueue.TryDequeue(out KeyEvent keyEvent))
        {
            if (!_keyStates.ContainsKey(keyEvent.keyCode))
            {
                _keyStates[keyEvent.keyCode] = false;
                _prevKeyStates[keyEvent.keyCode] = false;
            }

            _keyStates[keyEvent.keyCode] = keyEvent.isKeyDown;
        }
    }

    /// <summary>
    /// 使用Unity Input更新键盘状态（用于编辑器模式或本进程窗口前台时）
    /// </summary>
    private void UpdateKeyStatesFromUnityInput()
    {
        foreach (KeyCode key in _commonKeys)
        {
            if (!_prevKeyStates.ContainsKey(key))
                _prevKeyStates[key] = false;
            else
                _prevKeyStates[key] = _keyStates.ContainsKey(key) && _keyStates[key];

            _keyStates[key] = Input.GetKey(key);
        }
    }

    /// <summary>
    /// 使用Unity Input更新鼠标状态
    /// </summary>
    private void UpdateMouseFromUnityInput()
    {
        if (Input.GetMouseButtonDown(0))
            _leftButtonPressed = true;
        if (Input.GetMouseButtonDown(1))
            _rightButtonPressed = true;
    }

    #endregion

    #region 公共API - 键盘

    /// <summary>
    /// 检测按键是否持续按住
    /// </summary>
    public static bool GetKey(KeyCode key)
    {
        return _keyStates.ContainsKey(key) && _keyStates[key];
    }

    /// <summary>
    /// 检测按键在本帧是否按下
    /// </summary>
    public static bool GetKeyDown(KeyCode key)
    {
        if (_keyStates.ContainsKey(key) && _prevKeyStates.ContainsKey(key))
            return _keyStates[key] && !_prevKeyStates[key];
        return false;
    }

    /// <summary>
    /// 检测按键在本帧是否释放
    /// </summary>
    public static bool GetKeyUp(KeyCode key)
    {
        if (_keyStates.ContainsKey(key) && _prevKeyStates.ContainsKey(key))
            return !_keyStates[key] && _prevKeyStates[key];
        return false;
    }

    #endregion

    #region 公共API - 鼠标

    /// <summary>
    /// 检测鼠标按键在本帧是否按下(单次触发)
    /// ⚠️ 读取后立即清零,保证只在按下的那一帧返回true
    /// </summary>
    /// <param name="button">0=左键, 1=右键</param>
    /// <returns>按下的那一帧返回true,之后返回false</returns>
    public static bool GetMouseButtonDown(int button)
    {
        bool result = false;

        if (button == 0)
        {
            result = _leftButtonPressed;
            _leftButtonPressed = false; // ⚠️ 读取后立即清零
        }
        else if (button == 1)
        {
            result = _rightButtonPressed;
            _rightButtonPressed = false; // ⚠️ 读取后立即清零
        }

        return result;
    }

    #endregion
}
