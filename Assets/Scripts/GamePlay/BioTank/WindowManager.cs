using UnityEngine;
using Common;
using System.Drawing;
using static WebSocketServerManager;
using System.Collections.Generic;
using System.Linq;

public class WindowManager : MonoSingleton<WindowManager>
{
    public Dictionary<int, BioTankSaveData> saveData = new Dictionary<int, BioTankSaveData>();

    public override void Init()
    {
        //BioTankSaveData sd = new BioTankSaveData();
        //sd.Id = 0;
        //sd.Position = new Vector2Int(WallpaperManager.Instance.displayLeft, WallpaperManager.Instance.displayTop);
        //sd.Size = SysDefine.DefaultWindowSize;
        //WindowManager.Instance.saveData.TryAdd(0, sd);
    }
    public void SetSaveData(BioTankSaveData[] datas)
    {
        if (datas == null) return;
        saveData.Clear();
        foreach (var data in datas)
        {
            saveData[data.Id] = data;
        }
        if (saveData.Count == 0)
        {
            Init();
        }
    }
    public BioTankSaveData[] GetSaveData()
    {
        List<BioTankSaveData> dataList = new List<BioTankSaveData>();
        var clients = WebSocketServerManager.GetClients();
        foreach (var item in clients)
        {
            BioTankSaveData data = new BioTankSaveData();
            data.Id = item.Key;
            data.Position = item.Value.GetPosition();
            data.Size = item.Value.GetSize();
            dataList.Add(data);
        }
        return dataList.ToArray();
    }
    //public Vector2Int windowSize = new Vector2Int(1280, 720);
    //public Vector2Int SavePoint = new Vector2Int(0, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ResetWindow()
    {

        var client = WebSocketServerManager.GetActiveClient();
        if (client == null)
        {
            Debug.LogError("[WindowManager.ResetWindow] 未找到活动客户端");
            return;
        }

        // 【修复】使用首次启动时相同的方法获取窗口位置和尺寸
        var windowSize = WallpaperManager.Instance.GetStartSize();
        var windowPosition = WallpaperManager.Instance.GetStartPosition();

        client.SendSetAndMoveWindowCommand(windowPosition, windowSize.x, windowSize.y);
    }
    public void ResetWindowSize()
    {
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null)
        {
            return;
        }
        // 重置窗口大小为主显示器的完整尺寸
        var windowSize = WallpaperManager.Instance.GetPrimaryMonitorSize();
        client.SendSetAndMoveWindowCommand(client.GetPosition(), windowSize.x, windowSize.y);
    }
    public void ResetWindowPosition()
    {
        // 调用 ResetWindow() 来完全复用首次启动时的逻辑
        ResetWindow();
    }
    public void SetWindowSize(Vector2Int size)
    {
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null) return;
        WebSocketServerManager.GetClient(PlayerManager.ActiveBioTanksID)?.SendSetAndMoveWindowCommand(client.GetPosition(), size.x, size.y);
    }
    public void SetIsVisibleWindow(bool isVisible)
    {
        var client = WebSocketServerManager.GetActiveClient();
        client?.SendShowWindowCommand(isVisible);
    }
    public void TopMostWindow()
    {
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null)
        {
            Debug.LogWarning("[WindowManager.TopMostWindow] 未找到活动客户端");
            return;
        }
        Debug.Log($"[WindowManager.TopMostWindow] 发送置顶命令到客户端 {PlayerManager.ActiveBioTanksID}");
        client.SetWindowZOrderCommand(WindowZOrder.Topmost);
    }

    /// <summary>
    /// 隐藏/显示窗口（不暂停生态箱运算）
    /// 当窗口隐藏时，生态箱的资源消耗、动植物成长繁衍等运算继续进行
    /// </summary>
    /// <param name="hide">是否隐藏窗口</param>
    public void SetPausedAndHideWindow(bool hide)
    {
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null) return;

        // 只隐藏/显示窗口，不暂停生态箱运算
        client.SendShowWindowCommand(!hide);
    }

    /// <summary>
    /// 获取当前活动生态箱的暂停状态
    /// </summary>
    public bool GetCurrentBioTankPausedState()
    {
        var bioTank = PlayerManager.Instance.GetBioTank(PlayerManager.ActiveBioTanksID);
        return bioTank?.IsPaused ?? false;
    }

    public void SetWindowSizeScaleProportionally(float scale)
    {
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null) return;
        var windowSize = client.GetSize();
        var newSize = new Vector2Int((int)(windowSize.x * scale), (int)(windowSize.y * scale));
        WebSocketServerManager.GetClient(PlayerManager.ActiveBioTanksID)?.SendSetAndMoveWindowCommand(client.GetPosition(), newSize.x, newSize.y);
    }
    public void MoveAddWindow(Vector2Int point)
    {
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null) return;
        WebSocketServerManager.GetClient(PlayerManager.ActiveBioTanksID)?.SendSetAndMoveWindowCommand(client.GetPosition()+point, client.GetSize().x, client.GetSize().y);
    }
    public void MoveWindow(ControlBehavior control, Vector2Int point, Vector2Int size)
    {
        
        control.SendSetAndMoveWindowCommand(point, size.x, size.y);
    }
    public void SetWindowSize(ControlBehavior control, Vector2Int size)
    {
        control.SendSetAndMoveWindowCommand(control.GetPosition(), size.x, size.y);
    }
    public void MoveWindow(Vector2Int point)
    {
        //SavePoint = point;
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null) return;
        client.SendSetAndMoveWindowCommand(point, client.GetSize().x, client.GetSize().y);
    }
    public void MouseMoveWindow(Vector2Int point)
    {
        var client = WebSocketServerManager.GetActiveClient();
        if (client == null) return;

        var windowSize = client.GetSize();

        // 使用主显示器信息，将点击位置作为窗口中心，避免硬编码分辨率
        Vector2Int monitorPos = WallpaperManager.Instance.GetPrimaryMonitorPosition();
        Vector2Int monitorSize = WallpaperManager.Instance.GetPrimaryMonitorSize();

        // 将全局屏幕坐标转换为相对于主显示器的坐标
        Vector2Int local = point - monitorPos;

        // 希望点击位置落在窗口中心
        int targetX = local.x - windowSize.x / 2 + monitorPos.x;
        int targetY = local.y - windowSize.y / 2 + monitorPos.y;

        // 确保窗口不会移出主显示器边界
        int minX = monitorPos.x;
        int maxX = monitorPos.x + monitorSize.x - windowSize.x;
        int minY = monitorPos.y;
        int maxY = monitorPos.y + monitorSize.y - windowSize.y;

        // 如果窗口比屏幕大，直接落在左上角/顶部，避免 Clamp 反转
        if (minX > maxX) { minX = maxX = monitorPos.x; }
        if (minY > maxY) { minY = maxY = monitorPos.y; }

        targetX = Mathf.Clamp(targetX, minX, maxX);
        targetY = Mathf.Clamp(targetY, minY, maxY);

        var newPos = new Vector2Int(targetX, targetY);

        client.SendSetAndMoveWindowCommand(newPos, windowSize.x, windowSize.y);
    }
    //public void ReduceorenlargeWindow(float scale)
    //{
    //    windowSize = new Vector2Int((int)(windowSize.x * scale), (int)(windowSize.y * scale));
    //    GetClient(PlayerManager.ActiveBioTanksID).SendSetAndMoveWindowCommand(SavePoint, windowSize.x, windowSize.y);
    //}
    // Update is called once per frame

}
