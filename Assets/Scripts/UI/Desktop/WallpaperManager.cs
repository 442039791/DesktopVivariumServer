using System;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.Threading;
using Common;
using System.Collections;
using SUIFW;
using Unity.Mathematics;
using System.Diagnostics;
// 使用共享的 Windows API 包装
using static WindowsAPI;

/// <summary>
/// 服务器端窗口管理类
/// </summary>
public class WallpaperManager : MonoSingleton<WallpaperManager>
{
    // 结构体和 API 声明已移至共享的 WindowsAPI 类
    private List<MonitorData> monitors = new List<MonitorData>();

    private bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref WindowsAPI.Rect lprcMonitor, IntPtr dwData)
    {
        MonitorInfoEx mi = new MonitorInfoEx();
        mi.Size = Marshal.SizeOf(typeof(MonitorInfoEx));
        if (GetMonitorInfo(hMonitor, ref mi))
        {
            var monitorData = new MonitorData
            {
                DeviceName = mi.DeviceName,
                ResolutionWidth = mi.Monitor.Right - mi.Monitor.Left,
                ResolutionHeight = mi.Monitor.Bottom - mi.Monitor.Top,
                PositionX = mi.Monitor.Left,
                PositionY = mi.Monitor.Top,
                IsPrimary = mi.Flags == 1
            };

            monitors.Add(monitorData);
        }
        return true; // 继续枚举下一个显示器
    }

    public void StartWindowDrag()
    {
        if (unityWnd != IntPtr.Zero)
        {
            ReleaseCapture();
            WindowsAPI.SendMessage(unityWnd, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }
        else
        {
            Debug.LogWarning("窗口句柄未初始化，无法拖拽");
        }
    }
    // Windows API 声明已移至共享的 WindowsAPI 类

    // GetWindowRect API 用于获取窗口位置
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // 类的字段
    public IntPtr unityWnd = IntPtr.Zero;
    private IntPtr workerw = IntPtr.Zero;

    // 【重要】这些字段必须是私有的，避免被 Unity Inspector 覆盖
    private int displayLeft = 0;
    private int displayTop = 0;
    private float displayScale = 1.0f;
    private int _displayWidth = 650;
    private int _displayHeight =800;
    public void SetDisplayScale(float scale)
    {
        displayScale = scale;
    }
    private int displayWidth
    {
        get { return (int)(_displayWidth * displayScale); }
        set { _displayWidth = value; }

    }
    private int displayHeight
    {
        get { return (int)(_displayHeight * displayScale); }
        set { _displayHeight = value; }
    }
    // 常量已移至 WindowsAPI
    public bool InitWorkerW = false;
    public int testimes = 0;
    public void MinimizeWindow()
    {
#if !UNITY_EDITOR // 只在非编辑器环境下生效
        if (unityWnd != IntPtr.Zero)
        {
            ShowWindow(unityWnd, SW_MINIMIZE);
        }
        else
        {
        }
#endif
    }
    public IntPtr GetWorkerWPtr()
    {
        return workerw;
    }
    private IntPtr GetWorkerW()
    {
        // 获取 WorkerW 窗口
        IntPtr progman = FindWindow("Progman", null);
        IntPtr WorkerW = IntPtr.Zero;
        IntPtr result = IntPtr.Zero;
        SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, 0x0, 1000, out result);
        Thread.Sleep(1000);
        EnumWindows((topHandle, topParamHandle) =>
        {
            IntPtr p = FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (p != IntPtr.Zero)
            {
                WorkerW = FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", null);
            }
            return true;
        }, IntPtr.Zero);

        bool foundWorkerW = false;
        if (WorkerW == IntPtr.Zero)
        {
            IntPtr child = IntPtr.Zero;
            while ((child = FindWindowEx(progman, child, "WorkerW", null)) != IntPtr.Zero)
            {
                // 检查此 WorkerW 是否包含 SHELLDLL_DefView
                IntPtr shellView = FindWindowEx(child, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shellView == IntPtr.Zero)
                {
                    // 找到不包含 SHELLDLL_DefView 的 WorkerW，目标窗口
                    WorkerW = child;
                    foundWorkerW = true;
                    break; // 找到后停止查找
                }
                else
                {
                }
            }
            if (!foundWorkerW)
            {
                Debug.LogError("未能找到不包含 SHELLDLL_DefView 的 WorkerW 窗口。");
                InitWorkerW = true;
                return WorkerW;
            }
        }
        InitWorkerW = true;
        return WorkerW;
    }
    // DWM API 已移至 WindowsAPI
    public Vector2Int GetStartPosition()
    {
        if(!minPositionXInited)
            Test();
        return new Vector2Int(minPositionX, minPositionY);
    }

    /// <summary>
    /// 获取主显示器的位置（用于窗口重置）
    /// </summary>
    public Vector2Int GetPrimaryMonitorPosition()
    {
        if(!minPositionXInited)
        {
            Debug.LogWarning("[GetPrimaryMonitorPosition] 显示器信息未初始化，调用 Test() 初始化");
            Test();
        }
        return new Vector2Int(displayLeft, displayTop);
    }

    public Vector2Int GetStartSize()
    {
        if (!minPositionXInited)
            Test();
        return FullWindowSize;
    }

    /// <summary>
    /// 获取主显示器的完整尺寸（用于窗口重置）
    /// </summary>
    public Vector2Int GetPrimaryMonitorSize()
    {
        if (!minPositionXInited)
            Test();

        // 找到主显示器并返回原始分辨率（不受 displayScale 影响）
        foreach (var monitor in monitors)
        {
            if (monitor.IsPrimary)
            {
                return new Vector2Int(monitor.ResolutionWidth, monitor.ResolutionHeight);
            }
        }

        // Fallback: 如果没找到主显示器，使用第一个显示器
        if (monitors.Count > 0)
        {
            Debug.LogWarning($"[GetPrimaryMonitorSize] 未找到主显示器，使用第一个显示器: {monitors[0].ResolutionWidth}x{monitors[0].ResolutionHeight}");
            return new Vector2Int(monitors[0].ResolutionWidth, monitors[0].ResolutionHeight);
        }

        // 最后的 fallback
        Debug.LogError("[GetPrimaryMonitorSize] 没有检测到任何显示器！");
        return new Vector2Int(1920, 1080); // 默认分辨率
    }
    private float _initialSize;

    void Start()
    {
        Test();
        //Screen.SetResolution(displayWidth, displayHeight, false);
        workerw = GetWorkerW();
        _initialSize = CoreSetting.Instance.UICamera.orthographicSize;

#if !UNITY_EDITOR
        StartCoroutine(ApplyWindowStyle());
#endif

    }

    /// <summary>
    /// 检查是否为单屏幕
    /// </summary>
    public bool IsSingleMonitor()
    {
        if (!minPositionXInited)
            Test();
        return monitors.Count == 1;
    }
    // GetUnityWindowHandle 已移至 WindowsAPI.GetUnityWindowHandle()
    private IEnumerator ApplyWindowStyle()
    {
        // 等待窗口完全初始化
        yield return new WaitForSeconds(0.5f);

        try
        {
            // 更可靠的窗口句柄获取方式
            unityWnd = WindowsAPI.GetUnityWindowHandle();
            if (unityWnd == IntPtr.Zero) unityWnd = FindWindow(null, Application.productName);

            // 首先确保窗口可见
            ShowWindow(unityWnd, SW_SHOW);

            // 移除边框和标题
            long style = WindowsAPI.GetWindowStyle(unityWnd);
            long newStyle = style & ~(WS_CAPTION | WS_BORDER | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
            // 确保保留WS_VISIBLE标志
            newStyle |= WS_VISIBLE;
            WindowsAPI.SetWindowStyle(unityWnd, newStyle);

            // 强制刷新窗口并确保显示
            SetWindowPos(unityWnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);

            // 应用DWM效果
            ApplyDwmEffects(unityWnd);

            // 再次确保窗口显示
            ShowWindow(unityWnd, SW_SHOW);

            windowStyleApplied = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"应用窗口样式失败: {ex.Message}");
        }
    }


    private void ApplyDwmEffects(IntPtr hWnd)
    {
        if (Environment.OSVersion.Version.Major >= 6)
        {
            MARGINS margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(hWnd, ref margins);

            DWM_BLURBEHIND blurBehind = new DWM_BLURBEHIND
            {
                dwFlags = DWM_BB_ENABLE,
                fEnable = true,
                hRgnBlur = IntPtr.Zero
            };
            DwmEnableBlurBehindWindow(hWnd,ref  blurBehind);
        }
    }
    // Windows API 已移至 WindowsAPI

    private bool windowStyleApplied = false;

    /// <summary>
    /// 窗口是否处于扩展状态
    /// </summary>
    public bool IsWindowExpanded { get; private set; } = false;

    /// <summary>
    /// 扩展区域的X偏移量（新界面应该放在这个位置）
    /// 窗口从650扩展到1172，扩展了522像素，新界面放在原界面右侧
    /// </summary>
    public float ExpandedAreaOffsetX => 650f / 2f + (1172f - 650f) / 2f; // = 325 + 261 = 586

    public void SetWindowPop()
    {
        _displayWidth = 1172;
        _displayHeight = 800;
        IsWindowExpanded = true;

        // 计算新的正交相机参数，使视野只向右扩展
        var uiCamera = CoreSetting.Instance.UICamera;
        float orthoSize = uiCamera.orthographicSize; // 保持高度不变
        float oldAspect = 650f / 800f;
        float newAspect = 1172f / 800f;

        // 设置相机aspect
        uiCamera.aspect = newAspect;

        // 计算偏移量：新宽度比旧宽度多出的一半
        // 旧宽度 = orthoSize * 2 * oldAspect
        // 新宽度 = orthoSize * 2 * newAspect
        // 偏移 = (新宽度 - 旧宽度) / 2
        float oldWidth = orthoSize * 2f * oldAspect;
        float newWidth = orthoSize * 2f * newAspect;
        float offset = (newWidth - oldWidth) / 2f;

        // 使用自定义投影矩阵，将视野向右偏移
        Matrix4x4 matrix = Matrix4x4.Ortho(
            -orthoSize * oldAspect,           // left: 保持原来的左边界
            orthoSize * oldAspect + offset * 2f, // right: 向右扩展
            -orthoSize,                        // bottom
            orthoSize,                         // top
            uiCamera.nearClipPlane,
            uiCamera.farClipPlane
        );
        uiCamera.projectionMatrix = matrix;

        // 更新Canvas Scaler
        UpdateCanvasScalerResolution(1172f, 800f);
        SetWindowSize();
    }
    public void SetWindowNormal()
    {
        _displayWidth = 650;
        _displayHeight = 800;
        IsWindowExpanded = false;

        // 恢复相机默认投影矩阵
        var uiCamera = CoreSetting.Instance.UICamera;
        uiCamera.aspect = 650f / 800f;
        uiCamera.ResetProjectionMatrix();

        // 恢复Canvas Scaler
        UpdateCanvasScalerResolution(650f, 800f);
        SetWindowSize();
    }

    /// <summary>
    /// 更新Canvas Scaler的参考分辨率
    /// </summary>
    private void UpdateCanvasScalerResolution(float width, float height)
    {
        var canvas = GameObject.FindGameObjectWithTag("_TagCanvas");
        if (canvas != null)
        {
            var canvasScaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.referenceResolution = new Vector2(width, height);
            }
        }
    }

    /// <summary>
    /// 递归查找子物体
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child != null) return child;

        for (int i = 0; i < parent.childCount; i++)
        {
            child = FindChildRecursive(parent.GetChild(i), childName);
            if (child != null) return child;
        }
        return null;
    }
    public IEnumerator SetWindowSizeCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        SetWindowNormal();
    }
    private int _displayLeft = 0;
    private int _displayTop = 0;
    public void SetWindowSize()
    {
        //CoreSetting.Instance.UICamera.aspect = 1120f/800f;
#if !UNITY_EDITOR // 只在非编辑器环境下生效
        // 获取当前窗口位置，保持左上角不变，只向右扩展
        RECT currentRect;
        if (GetWindowRect(unityWnd, out currentRect))
        {
            // 使用当前窗口的左上角位置，只改变宽高
            SetWindowPos(unityWnd, IntPtr.Zero, currentRect.Left, currentRect.Top, displayWidth, displayHeight,
                    SWP_FRAMECHANGED | SWP_NOZORDER | SWP_SHOWWINDOW);
        }
        else
        {
            // 如果获取位置失败，使用原来的方式
            SetWindowPos(unityWnd, IntPtr.Zero, 0, 0, displayWidth, displayHeight,
                    SWP_FRAMECHANGED | SWP_NOZORDER | SWP_NOMOVE | SWP_SHOWWINDOW);
        }
#endif

        //Screen.SetResolution(displayWidth, displayHeight, false);
    }

    public void SetDisplayPosition(int left, int top)
    {
        _displayLeft = left;
        _displayTop = top;
    }
    public float GetWindowScale()
    {
        return displayScale;
    }
    void Update()
    {

        // 持续刷新确保样式不被覆盖,保持左上角位置不变
        if (!windowStyleApplied && unityWnd != IntPtr.Zero)
        {
#if !UNITY_EDITOR
            RECT currentRect;
            if (GetWindowRect(unityWnd, out currentRect))
            {
                SetWindowPos(unityWnd, IntPtr.Zero, currentRect.Left, currentRect.Top, displayWidth, displayHeight,
                    SWP_FRAMECHANGED | SWP_NOZORDER | SWP_SHOWWINDOW);
            }
#endif
        }


    }
    public void TestMoveWindow()
    {
        MoveWindow(unityWnd, displayLeft + 1080, displayTop, displayHeight, displayWidth, true);
    }
    public void MoveWindowToPosition(Vector2Int Positon, int width, int height)
    {

        MoveWindow(unityWnd, Positon.x, Positon.y, width, height, true);
    }

    void OnApplicationQuit()
    {

        // 恢复 Unity 窗口的父窗口为默认桌面
        if (unityWnd != IntPtr.Zero)
        {
            SetParent(unityWnd, IntPtr.Zero);
        }

        // 销毁嵌入的窗口，防止残留
        if (unityWnd != IntPtr.Zero)
        {
            DestroyWindow(unityWnd);
        }
    }
    public bool minPositionXInited = false;
    int minPositionX;
    int minPositionY;
    Vector2Int FullWindowSize;
    public void Test()
    {
        if (minPositionXInited)
        {
            return;
        }

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnum, IntPtr.Zero);

        minPositionX = Math.Abs(monitors.Min(monitor => monitor.PositionX));
        minPositionY = Math.Abs(monitors.Min(monitor => monitor.PositionY));

        // 找到主显示器
        foreach (var monitor in monitors)
        {
            if (monitor.IsPrimary)
            {
                displayLeft = monitor.PositionX;
                displayTop = monitor.PositionY;
                displayHeight = monitor.ResolutionHeight;
                displayWidth = monitor.ResolutionWidth;
                break;
            }
        }

        FullWindowSize = new Vector2Int(displayWidth, displayHeight);
        minPositionXInited = true;
    }
}