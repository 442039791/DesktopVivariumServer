/// <summary>
/// 生态箱相关界面的窗口状态管理器
/// 用于协调编辑界面、生态箱列表界面、新建生态箱界面的窗口尺寸
///
/// 需求：
/// 1. 打开任意一个界面时，窗口尺寸变宽
/// 2. 三个界面相互切换时，窗口尺寸不变
/// 3. 三个界面都关闭时，窗口恢复初始尺寸
/// </summary>
public static class BiotankWindowManager
{
    /// <summary>
    /// 当前打开的生态箱相关界面计数
    /// </summary>
    private static int _openWindowCount = 0;

    /// <summary>
    /// 界面打开时调用
    /// </summary>
    public static void OnWindowOpen()
    {
        bool wasZero = _openWindowCount == 0;
        _openWindowCount++;

        // 如果之前没有界面打开，现在打开了第一个，则扩展窗口
        if (wasZero)
        {
            WallpaperManager.Instance.SetWindowPop();
        }
    }

    /// <summary>
    /// 界面关闭时调用
    /// </summary>
    public static void OnWindowClose()
    {
        _openWindowCount--;
        if (_openWindowCount < 0) _openWindowCount = 0;

        // 只有当所有界面都关闭时才恢复窗口
        if (_openWindowCount == 0)
        {
            WallpaperManager.Instance.SetWindowNormal();
        }
    }

    /// <summary>
    /// 重置状态（场景切换时调用）
    /// </summary>
    public static void Reset()
    {
        _openWindowCount = 0;
    }

    /// <summary>
    /// 获取当前打开的窗口数量
    /// </summary>
    public static int OpenWindowCount => _openWindowCount;

    /// <summary>
    /// 是否有任何生态箱相关界面打开
    /// </summary>
    public static bool HasAnyWindowOpen => _openWindowCount > 0;
}
