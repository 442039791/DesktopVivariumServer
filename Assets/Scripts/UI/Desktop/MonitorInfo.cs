using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MonitorInfoManager : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfoEx
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    private class MonitorData
    {
        public string DeviceName;
        public int ResolutionWidth;
        public int ResolutionHeight;
        public int PositionX;
        public int PositionY;
        public bool IsPrimary;

        public override string ToString()
        {
            return $"显示器名称: {DeviceName}, 分辨率: {ResolutionWidth}x{ResolutionHeight}, 位置: ({PositionX}, {PositionY}), 主显示器: {IsPrimary}";
        }
    }

    private List<MonitorData> monitors = new List<MonitorData>();

    void Start()
    {
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnum, IntPtr.Zero);

        // 找到最小的 X 和 Y 值
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var monitor in monitors)
        {
            if (monitor.PositionX < minX) minX = monitor.PositionX;
            if (monitor.PositionY < minY) minY = monitor.PositionY;
        }

        // 调整所有显示器的位置，使其非负
        foreach (var monitor in monitors)
        {
            monitor.PositionX -= minX;
            monitor.PositionY -= minY;
        }

        // 按照调整后的位置 X 进行排序
        monitors.Sort((a, b) => a.PositionX.CompareTo(b.PositionX));

        // 输出调整后所有显示器信息
        foreach (var monitor in monitors)
        {
        }
    }

    private bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
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
}
