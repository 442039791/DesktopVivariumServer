using Common;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class DirectWindowDrag : MonoSingleton<DirectWindowDrag>
{
    // 窗口操作API
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    // 结构体
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        // 便捷属性方便访问
        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public Vector2Int Position => new Vector2Int(Left, Top);
        public Vector2Int Size => new Vector2Int(Width, Height);
    }

    // 起始位置保存
    private POINT dragStartPosition;
    private POINT windowStartPosition;

    // 拖拽状态标志
    private bool isDragging = false;
    private IntPtr windowHandle;

    // 移动窗口标志位
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_SHOWWINDOW = 0x0040;

    // 当前窗口位置缓存
    private RECT currentWindowRect;
    private DateTime lastUpdateTime;
    private const float CACHE_VALID_TIME = 0.1f; // 缓存有效期（秒）

    void Start()
    {
        if (WallpaperManager.Instance.unityWnd == IntPtr.Zero)
        {
            windowHandle = WindowsAPI.GetUnityWindowHandle();
        }
        else
        {
            windowHandle = WallpaperManager.Instance.unityWnd;
        }
        UpdateWindowRectCache();
    }

    void Update()
    {
        if (isDragging)
        {
            // 实时获取鼠标位置
            POINT currentMousePos;
            GetCursorPos(out currentMousePos);

            // 计算窗口新位置
            int newX = windowStartPosition.X + (currentMousePos.X - dragStartPosition.X);
            int newY = windowStartPosition.Y + (currentMousePos.Y - dragStartPosition.Y);

            // 更新窗口位置
            SetWindowPos(windowHandle, 0, newX, newY, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);

            // 更新缓存
            UpdateWindowRectCache();
        }
        else if ((DateTime.Now - lastUpdateTime).TotalSeconds > CACHE_VALID_TIME)
        {
            // 定期更新窗口位置缓存
            UpdateWindowRectCache();
        }
    }

    /// <summary>
    /// 设置窗口位置
    /// </summary>
    /// <param name="position">屏幕坐标系中的窗口位置（左上角）</param>
    public void SetWindowPosition(Vector2Int position)
    {
        if (windowHandle != IntPtr.Zero)
        {
            SetWindowPos(windowHandle, 0, position.x, position.y, 0, 0,
                         SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            UpdateWindowRectCache();
        }
    }

    /// <summary>
    /// 获取当前窗口位置（左上角坐标）
    /// </summary>
    public Vector2Int GetWindowPosition()
    {
        UpdateWindowRectCache(); // 确保数据最新
        return new Vector2Int(currentWindowRect.Left, currentWindowRect.Top);
    }

    /// <summary>
    /// 获取完整的窗口矩形信息
    /// </summary>
    public RECT GetWindowRect()
    {
        UpdateWindowRectCache(); // 确保数据最新
        return currentWindowRect;
    }

    /// <summary>
    /// 获取窗口中心位置
    /// </summary>
    public Vector2Int GetWindowCenter()
    {
        UpdateWindowRectCache(); // 确保数据最新
        return new Vector2Int(
            (currentWindowRect.Left + currentWindowRect.Right) / 2,
            (currentWindowRect.Top + currentWindowRect.Bottom) / 2
        );
    }

    /// <summary>
    /// 获取窗口尺寸
    /// </summary>
    public Vector2Int GetWindowSize()
    {
        UpdateWindowRectCache(); // 确保数据最新
        return new Vector2Int(
            currentWindowRect.Right - currentWindowRect.Left,
            currentWindowRect.Bottom - currentWindowRect.Top
        );
    }

    // 开始拖拽
    public void StartDrag()
    {
        // 1. 获取起始鼠标位置
        GetCursorPos(out dragStartPosition);

        // 2. 获取当前窗口位置
        UpdateWindowRectCache();
        windowStartPosition.X = currentWindowRect.Left;
        windowStartPosition.Y = currentWindowRect.Top;

        // 3. 标记拖拽中
        isDragging = true;

    }

    // 结束拖拽
    public void EndDrag()
    {
        isDragging = false;
    }

    /// <summary>
    /// 更新窗口句柄位置缓存
    /// </summary>
    private void UpdateWindowRectCache()
    {
        if (windowHandle != IntPtr.Zero)
        {
            if (GetWindowRect(windowHandle, out currentWindowRect))
            {
                lastUpdateTime = DateTime.Now;
            }
            else
            {
#if !UNITY_EDITOR
                Debug.LogError("获取窗口矩形失败");
#endif
            }
        }
    }

    /// <summary>
    /// 将屏幕坐标转换为Unity窗口坐标
    /// </summary>
    public Vector2 ScreenToWindowPosition(Vector2 screenPosition)
    {
        var pos = GetWindowPosition();
        var size = GetWindowSize();

        return new Vector2(
            screenPosition.x - pos.x,
            size.y - (screenPosition.y - pos.y) // 翻转Y轴
        );
    }

    /// <summary>
    /// 将Unity窗口坐标转换为屏幕坐标
    /// </summary>
    public Vector2 WindowToScreenPosition(Vector2 windowPosition)
    {
        var pos = GetWindowPosition();
        var size = GetWindowSize();

        return new Vector2(
            pos.x + windowPosition.x,
            pos.y + (size.y - windowPosition.y) // 翻转Y轴
        );
    }
}