using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Common;

/// <summary>
/// WebSocket连接行为控制器
/// 管理单个客户端连接的生命周期和状态
/// </summary>
public class ControlBehavior : WebSocketBehavior
{
    #region 静态成员

    /// <summary>
    /// 客户端连接字典
    /// 注意：不要直接访问此字典，请使用 WebSocketServerManager.GetClient() 或 GetClients() 接口
    /// </summary>
    internal static ConcurrentDictionary<int, ControlBehavior> clients = new();

    /// <summary>
    /// 连接处理锁，防止并发连接时的竞态条件
    /// </summary>
    private static readonly object _connectionLock = new object();

    #endregion

    #region 客户端状态

    private Vector2Int _position;
    private Vector2Int _size;
    private int clientId;
    private bool IsShowWindow = false;

    #endregion

    #region 状态管理方法

    public bool GetShowWindow()
    {
        return IsShowWindow;
    }

    public void SetShowWindow(bool isShow)
    {
        IsShowWindow = isShow;
    }

    public Vector2Int GetSize()
    {
        return _size;
    }

    public Vector2Int GetPosition()
    {
        return _position;
    }

    public void SetSize(Vector2Int size)
    {
        _size = size;
    }

    public void SetPosition(Vector2Int position)
    {
        _position = position;
    }

    public void SetClientID(int id)
    {
        clientId = id;
    }

    public int GetClientID()
    {
        return clientId;
    }

    #endregion

    #region 生命周期方法

    /// <summary>
    /// 是否为休眠客户端
    /// </summary>
    private bool _isDormantClient = false;

    /// <summary>
    /// 休眠客户端ID（负数）
    /// </summary>
    private int _dormantId = 0;

    /// <summary>
    /// 客户端连接打开时触发
    /// 服务端根据当前状态决定客户端类型并分配ID
    /// 注意：WebSocket回调在工作线程中执行，Unity API调用需要延迟到主线程
    /// </summary>
    protected override void OnOpen()
    {
        Debug.Log("[ControlBehavior.OnOpen] 客户端连接请求，将处理任务加入主线程队列");

        // WebSocket 回调在工作线程中执行，Unity API（如FindObjectsByType）只能在主线程调用
        // 因此需要将整个连接处理逻辑延迟到主线程执行
        var task = new WebSocketServerManager.Task
        {
            Message = "",
            Callback = (message) =>
            {
                // 使用锁防止并发连接时的竞态条件
                // 确保同一时间只有一个客户端连接被处理，避免多个客户端同时被判定为正常客户端
                lock (_connectionLock)
                {
                    try
                    {
                        Debug.Log("[ControlBehavior.OnOpen] 在主线程处理客户端连接");

                        // 检查是否应该作为休眠客户端处理
                        if (ShouldBeDormantClient())
                        {
                            HandleDormantClientConnection();
                            return;
                        }

                        // 正常客户端连接处理
                        HandleNormalClientConnection();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[ControlBehavior] OnOpen 异常: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        };

        WebSocketServerManager.EnqueueTask(task);
    }

    /// <summary>
    /// 处理正常客户端连接
    /// </summary>
    private void HandleNormalClientConnection()
    {
        clientId = GetClientUniqueId();
        Debug.Log($"[ControlBehavior] 服务端分配的clientId: {clientId}");

        if (clientId == -1)
        {
            Debug.LogError("[ControlBehavior] 客户端ID获取失败");
            return;
        }

        clients[clientId] = this;
        Debug.Log($"[ControlBehavior.OnOpen] 客户端已注册, 当前连接数: {clients.Count}");

        // 发送SetBioTankID命令，告诉客户端它的生态箱ID
        SendSetBioTankIDCommand(clientId, WallpaperManager.Instance.GetWorkerWPtr());

        // 同步数据
        SendLoadCommand();

        // 检查并设置活动生物罐
        if (BioTankRegistry.Instance != null)
        {
            if (PlayerManager.Instance.CheckIsActionBiotankID(clientId))
            {
                PlayerManager.Instance.SetActionBioTankManager(clientId);
            }
        }
        else
        {
            Debug.LogWarning($"[ControlBehavior] BioTankRegistry未初始化，跳过活动生物罐设置 (ClientID: {clientId})");
        }
    }

    /// <summary>
    /// 检查是否应该作为休眠客户端处理
    /// </summary>
    private bool ShouldBeDormantClient()
    {
        // 如果预加载池功能未启用，则不作为休眠客户端处理
        if (PreloadClientPoolManager.Instance == null || !PreloadClientPoolManager.Instance.IsEnabled)
        {
            return false;
        }

        if (BioTankRegistry.Instance == null)
        {
            return false;
        }

        var bioTanks = BioTankRegistry.Instance.BioTanks;

        // 如果没有生态箱，则应该是休眠客户端
        if (bioTanks.Count == 0)
        {
            return true;
        }

        // 检查是否所有生态箱都已有客户端连接
        foreach (var bioTankId in bioTanks.Keys)
        {
            if (!clients.ContainsKey(bioTankId))
            {
                // 还有生态箱没有客户端，这是正常客户端
                return false;
            }
        }

        // 所有生态箱都有客户端了，这是休眠客户端
        return true;
    }

    /// <summary>
    /// 处理休眠客户端连接
    /// 服务端生成休眠ID并发送给客户端
    /// </summary>
    private void HandleDormantClientConnection()
    {
        _isDormantClient = true;

        // 生成一个休眠ID（负数）
        if (PreloadClientPoolManager.Instance != null)
        {
            _dormantId = -1000 - PreloadClientPoolManager.Instance.AvailableDormantClients - 1;
        }
        else
        {
            _dormantId = -1000 - clients.Count;
        }
        Debug.Log($"[ControlBehavior] 服务端生成休眠ID: {_dormantId}");

        clientId = _dormantId;

        Debug.Log($"[ControlBehavior] 休眠客户端连接，使用休眠ID: {_dormantId}");

        // 注册到预加载池
        if (PreloadClientPoolManager.Instance != null)
        {
            PreloadClientPoolManager.Instance.RegisterDormantClient(this, _dormantId);
        }

        // 发送进入休眠模式命令，告诉客户端它的休眠ID
        SendEnterDormantModeCommand(_dormantId);

        Debug.Log($"[ControlBehavior] 休眠客户端已注册并进入休眠模式");
    }

    /// <summary>
    /// 客户端断开连接时触发
    /// </summary>
    override protected void OnClose(CloseEventArgs e)
    {
        try
        {
            clients.TryRemove(clientId, out ControlBehavior client);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ControlBehavior] OnClose 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 接收到消息时触发
    /// </summary>
    protected override void OnMessage(MessageEventArgs e)
    {
        // WebSocket 回调在工作线程中执行，需要将处理任务加入主线程队列
        var data = e.Data;
        var currentClientId = clientId;

        var task = new WebSocketServerManager.Task
        {
            Message = data,
            Callback = (message) =>
            {
                try
                {
                    ServerCommandProcessor.Instance.ProcessCommand(message, currentClientId, message);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ControlBehavior] ProcessCommand 异常: {ex.Message}\n{ex.StackTrace}");
                }
            }
        };

        WebSocketServerManager.EnqueueTask(task);
    }

    #endregion

    #region 通用命令发送方法

    /// <summary>
    /// 通用命令发送方法 - 自动处理type、EventID、序列化和发送
    /// </summary>
    /// <typeparam name="T">命令数据类型(必须继承自ReceiveData)</typeparam>
    /// <param name="commandType">命令类型</param>
    /// <param name="builder">用于构建命令对象的回调函数(可选,用于设置额外字段)</param>
    protected void SendCommand<T>(CommandType commandType, System.Action<T> builder = null) where T : ReceiveData, new()
    {
        try
        {
            int eventId = WebSocketServerManager.GetNextActionEventID();
            T commandData = new T
            {
                type = (int)commandType,
                EventID = eventId
            };

            // 使用builder构建命令对象的额外字段
            builder?.Invoke(commandData);

            string json = JsonUtility.ToJson(commandData);
            Send(json);

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ControlBehavior] SendCommand<{typeof(T).Name}> 异常: CommandType={commandType}, {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region 命令发送方法 - 基础命令

    public void SendSaveCommand()
    {
        SendCommand<ReceiveData>(CommandType.Save);
    }

    public void SendLoadCommand()
    {
        SendCommand<ReceiveData>(CommandType.Load);
    }

    public void SendDeleteSaveData()
    {
        SendCommand<ReceiveData>(CommandType.DeleteSaveData);
    }

    public void SendCloseCommand()
    {
        SendCommand<ReceiveData>(CommandType.Close);
    }

    #endregion

    #region 命令发送方法 - 窗口管理

    public void SendShowWindowCommand(bool Show = true)
    {
        SetShowWindow(Show);
        SendCommand<ShowWindowCommandData>(CommandType.ShowWindow, cmd => cmd.isShow = Show);
    }

    public void SendSetAndMoveWindowCommand(Vector2Int position, int width, int height)
    {
        _position = position;
        _size = new Vector2Int(width, height);
        SendCommand<SendSetAndMoveWindowData>(CommandType.SetAndMoveWindow, cmd =>
        {
            cmd.position = position;
            cmd.width = width;
            cmd.height = height;
        });
    }

    public void SetWindowZOrderCommand(WindowZOrder order)
    {
        SendCommand<SetWindowZOrderData>(CommandType.SetWindowZOrder, cmd => cmd.order = order);
    }

    public void SendSetWindowUporDownCommand(bool isUp)
    {
        SendCommand<SetWindowUpOrDownData>(CommandType.SetWindowUporDown, cmd => cmd.isUp = isUp);
    }

    #endregion

    #region 命令发送方法 - 生物罐管理

    public void SendSetBioTankIDCommand(int objID, System.IntPtr ptr)
    {
        SendCommand<SetBioTankIDData>(CommandType.SetBioTankID, cmd =>
        {
            cmd.objID = objID;
            cmd.ptrString = ptr.ToString();
        });
    }

    public void SendActionBioTankCommand(bool isAction)
    {
        SendCommand<ActionBiotankCommandData>(CommandType.ActionBioTank, cmd => cmd.isAction = isAction);
    }

    /// <summary>
    /// 发送绑定生态箱命令（用于休眠客户端激活）
    /// </summary>
    public void SendBindBioTankCommand(int bioTankId, System.IntPtr ptr)
    {
        SendCommand<BindBioTankData>(CommandType.BindBioTank, cmd =>
        {
            cmd.bioTankId = bioTankId;
            cmd.ptrString = ptr.ToString();
        });
    }

    /// <summary>
    /// 发送进入休眠模式命令
    /// </summary>
    public void SendEnterDormantModeCommand(int clientId)
    {
        SendCommand<EnterDormantModeData>(CommandType.EnterDormantMode, cmd =>
        {
            cmd.clientId = clientId;
        });
    }

    /// <summary>
    /// 发送退出休眠模式命令
    /// </summary>
    public void SendExitDormantModeCommand()
    {
        SendCommand<ExitDormantModeData>(CommandType.ExitDormantMode);
    }

    /// <summary>
    /// 发送进入预览模式命令
    /// </summary>
    /// <param name="sizeX">生态箱尺寸X</param>
    /// <param name="sizeY">生态箱尺寸Y</param>
    /// <param name="sizeZ">生态箱尺寸Z</param>
    /// <param name="layoutId">布局ID（可为null）</param>
    /// <param name="terrainData">地形JSON数据</param>
    /// <param name="ptr">桌面窗口句柄</param>
    /// <param name="cameraData">相机参数（可为null使用默认值）</param>
    public void SendEnterPreviewModeCommand(int sizeX, int sizeY, int sizeZ, string layoutId, string terrainData, System.IntPtr ptr, LayoutCameraData cameraData = null)
    {
        // 获取窗口的初始位置和尺寸（使用与新建生态箱相同的方法）
        var windowPosition = WallpaperManager.Instance.GetStartPosition();
        var windowSize = WallpaperManager.Instance.GetStartSize();

        SendCommand<EnterPreviewModeData>(CommandType.EnterPreviewMode, cmd =>
        {
            cmd.sizeX = sizeX;
            cmd.sizeY = sizeY;
            cmd.sizeZ = sizeZ;
            cmd.layoutId = layoutId ?? "";
            cmd.terrainData = terrainData ?? "";
            cmd.ptrString = ptr.ToString();
            cmd.windowPosX = windowPosition.x;
            cmd.windowPosY = windowPosition.y;
            cmd.windowWidth = windowSize.x;
            cmd.windowHeight = windowSize.y;
            // 相机参数
            if (cameraData != null)
            {
                cmd.cameraAngleX = cameraData.angleX;
                cmd.cameraAngleY = cameraData.angleY;
                cmd.cameraRadius = cameraData.radius;
                if (cameraData.targetPos != null)
                {
                    cmd.cameraTargetX = cameraData.targetPos.x;
                    cmd.cameraTargetY = cameraData.targetPos.y;
                    cmd.cameraTargetZ = cameraData.targetPos.z;
                }
            }
        });
    }

    /// <summary>
    /// 发送退出预览模式命令
    /// </summary>
    /// <param name="confirmCreate">是否确认创建</param>
    /// <param name="newBioTankId">新生态箱ID（仅confirmCreate为true时有效）</param>
    public void SendExitPreviewModeCommand(bool confirmCreate, int newBioTankId)
    {
        SendCommand<ExitPreviewModeData>(CommandType.ExitPreviewMode, cmd =>
        {
            cmd.confirmCreate = confirmCreate;
            cmd.newBioTankId = newBioTankId;
        });
    }

    /// <summary>
    /// 发送更新预览布局命令
    /// </summary>
    /// <param name="sizeX">新的生态箱尺寸X</param>
    /// <param name="sizeY">新的生态箱尺寸Y</param>
    /// <param name="sizeZ">新的生态箱尺寸Z</param>
    /// <param name="layoutId">新的布局ID（可为null）</param>
    /// <param name="terrainData">新的地形JSON数据</param>
    /// <param name="cameraData">相机参数（可为null使用默认值）</param>
    public void SendUpdatePreviewLayoutCommand(int sizeX, int sizeY, int sizeZ, string layoutId, string terrainData, LayoutCameraData cameraData = null)
    {
        SendCommand<UpdatePreviewLayoutData>(CommandType.UpdatePreviewLayout, cmd =>
        {
            cmd.sizeX = sizeX;
            cmd.sizeY = sizeY;
            cmd.sizeZ = sizeZ;
            cmd.layoutId = layoutId ?? "";
            cmd.terrainData = terrainData ?? "";
            // 相机参数
            if (cameraData != null)
            {
                cmd.cameraAngleX = cameraData.angleX;
                cmd.cameraAngleY = cameraData.angleY;
                cmd.cameraRadius = cameraData.radius;
                if (cameraData.targetPos != null)
                {
                    cmd.cameraTargetX = cameraData.targetPos.x;
                    cmd.cameraTargetY = cameraData.targetPos.y;
                    cmd.cameraTargetZ = cameraData.targetPos.z;
                }
            }
        });
    }

    #endregion

    #region 命令发送方法 - 植物/方块管理

    /// <summary>
    /// 发送创建方块命令（Cube/Decoration/Plant统一使用此方法）
    /// </summary>
    /// <param name="itemName">物品名称/ID</param>
    /// <param name="objID">对象ID</param>
    /// <param name="growthProgress">成长进度（仅Plant使用）</param>
    /// <param name="blockType">方块类型 (0=Cube, 1=Decoration, 2=Plant)</param>
    public void SendCreateBlockCommand(string itemName, int objID, float growthProgress, int blockType)
    {
        SendCommand<SendPlantCubeData>(CommandType.CreatePlantCube, cmd =>
        {
            cmd.PlantName = itemName;
            cmd.objID = objID;
            cmd.blockType = blockType;
            cmd.growProgress = growthProgress;
        });
    }

    public void SendRemovePlantCubeCommand(int objID, int BlockType)
    {
        SendCommand<SendPlantCubeData>(CommandType.RemovePlantCube, cmd =>
        {
            cmd.objID = objID;
            cmd.blockType = BlockType;
        });
    }

    public void SendPlantGrowthCommand(int objID, int growProgress)
    {
        SendCommand<OnChangePlantGrowData>(CommandType.ChangePlantGrow, cmd =>
        {
            cmd.objID = objID;
            cmd.growProgress = growProgress;
        });
    }

    public void SendPlantDeathCommand(int objID)
    {
        SendCommand<ReceiveID>(CommandType.SendPlantDeath, cmd => cmd.objID = objID);
    }

    public void SendPlantReEditCommand(int objID, string PlantName, float GrowthProgress)
    {
        SendCommand<ReceivePlantCubeData>(CommandType.PlantReEdit, cmd =>
        {
            cmd.PlantName = PlantName;
            cmd.objID = objID;
            cmd.growProgress = GrowthProgress;
        });
    }

    #endregion

    #region 命令发送方法 - 动物管理

    public void SendAddAnimalDisplay(int num, AnimalAgeStatus ageStatus, string fishName, ObjectQuality quality)
    {
        SendCommand<SendFishData>(CommandType.CreateFishDisPlay, cmd =>
        {
            cmd.fishName = fishName;
            cmd.num = num;
            cmd.quality = quality;
            cmd.ageStatus = ageStatus;
        });
    }

    public void SendRemoveAnimalDisplay(int num, AnimalAgeStatus ageStatus, string fishName)
    {
        SendCommand<SendFishData>(CommandType.RemoveFishDisplay, cmd =>
        {
            cmd.fishName = fishName;
            cmd.num = num;
            cmd.ageStatus = ageStatus;
        });
    }

    public void SendAnimalGrowthCommand(int objID, int growProgress)
    {
        SendCommand<OnChangePlantGrowData>(CommandType.ChangeFishGrow, cmd =>
        {
            cmd.objID = objID;
            cmd.growProgress = growProgress;
        });
    }

    public void SendFishDeathCommand(int objID)
    {
        SendCommand<ReceiveID>(CommandType.SendFishDeath, cmd => cmd.objID = objID);
    }

    #endregion

    #region 命令发送方法 - 蓝图管理

    public void SendBluePrintServerCommand(int id, BluePrintCommandType commandType, string IconName = "", string BlockModifyNewDataName = "")
    {
        SendCommand<BluePrintServerCommandData>(CommandType.BluePrintServerCommand, cmd =>
        {
            cmd.IconName = IconName;
            cmd.BlockModifyNewDataName = BlockModifyNewDataName;
            cmd.id = id;
            cmd.commandType = commandType;
        });
    }

    public void SendBluePrintBioTankCommand(BluePrintBioTankCommandType commandType, Vector3 Size, int objID)
    {
        SendCommand<BluePrintBioTankCommand>(CommandType.BluePrintBioTankCommand, cmd =>
        {
            cmd.commandType = commandType;
            cmd.Size = Size;
            cmd.objID = objID;
        });
    }

    #endregion

    #region 命令发送方法 - 相机控制

    public void SendCameraControlMoveCommand(float x, float y)
    {
        SendCommand<CameraControlMoveData>(CommandType.CameraControlMove, cmd =>
        {
            cmd.x = x;
            cmd.y = y;
        });
    }

    public void SendCameraControlPosMoveCommand(float x, float y)
    {
        SendCommand<CameraControlMoveData>(CommandType.CameraControlPosMove, cmd =>
        {
            cmd.x = x;
            cmd.y = y;
        });
    }

    public void SendCameraControlScrollCommand(float scroll)
    {
        SendCommand<CameraControlScrollData>(CommandType.CameraControlScroll, cmd => cmd.scroll = scroll);
    }

    #endregion

    #region 命令发送方法 - 建造控制

    public void SendChangeBuildMode(bool isBuildMode)
    {
        SendCommand<SendChangeBuildModeData>(CommandType.ChangeBuildMode, cmd => cmd.isBuildMode = isBuildMode);
    }

    public void SendBuilControlCommand(BuildControlMode mode, bool RangeSelect)
    {
        SendCommand<BuildControlCommandData>(CommandType.BuildControlCommand, cmd =>
        {
            cmd.mode = mode;
            cmd.RangeSelect = RangeSelect;
        });
    }

    /// <summary>
    /// 发送隐藏光标上方cube命令
    /// </summary>
    /// <param name="enabled">是否启用隐藏</param>
    public void SendHideAboveCursorCommand(bool enabled)
    {
        SendCommand<HideAboveCursorCommandData>(CommandType.HideAboveCursor, cmd =>
        {
            cmd.enabled = enabled;
        });
    }

    #endregion

    #region 命令发送方法 - 其他

    public void SendOutlinableModeCommand(int objID, OutlinableMode mode)
    {
        SendCommand<SendOutlinableModeData>(CommandType.OutlinableMode, cmd =>
        {
            cmd.objID = objID;
            cmd.mode = mode;
        });
    }

    public void SendBatchOutlinableModeCommand(Dictionary<int, OutlinableMode> modeDict)
    {
        SendCommand<SendBatchOutlinableModeData>(CommandType.BatchOutlinableMode, cmd => cmd.modeDict = modeDict);
    }

    public void SendSetCanUseButtonCommand(bool isCanUse)
    {
        SendCommand<SetCanUseButtonData>(CommandType.SetCanUseButton, cmd => cmd.isCanUse = isCanUse);
    }

    #endregion

    #region 客户端ID管理

    /// <summary>
    /// 获取唯一的客户端ID
    /// 从 BioTankRegistry 中获取未被 clients 使用的 ID
    /// </summary>
    private int GetClientUniqueId()
    {
        try
        {
            // 检查 BioTankRegistry 是否已初始化
            if (BioTankRegistry.Instance == null)
            {
                Debug.LogError("[ControlBehavior] BioTankRegistry 未初始化，无法分配客户端ID");
                return -1;
            }

            // 从 BioTankRegistry 中获取所有可用的 BioTank ID
            var bioTanks = BioTankRegistry.Instance.BioTanks;
            Debug.Log($"[ControlBehavior.GetClientUniqueId] BioTanks数量: {bioTanks.Count}, 已连接客户端: {clients.Count}");

            // 【修复】如果 BioTanks 为空（存档还没加载完成），返回默认ID 0
            if (bioTanks.Count == 0)
            {
                Debug.LogWarning("[ControlBehavior] BioTanks为空，存档可能还没加载完成，使用默认ID=0");
                return 0;
            }

            // 查找第一个未被使用的 ID（未在 clients 中的 ID）
            foreach (var bioTankId in bioTanks.Keys)
            {
                if (!clients.ContainsKey(bioTankId))
                {
                    Debug.Log($"[ControlBehavior.GetClientUniqueId] 找到可用ID: {bioTankId}");
                    return bioTankId;
                }
            }

            Debug.LogWarning("[ControlBehavior] 没有可用的客户端ID，所有 BioTank 都已被连接");
            return -1;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ControlBehavior] GetClientUniqueId 异常: {ex.Message}\n{ex.StackTrace}");
            return -1;
        }
    }

    /// <summary>
    /// 发送Cube移动命令给客户端
    /// </summary>
    public void SendCubeMoveCommand(float deltaX, float deltaY, float deltaZ)
    {
        SendCommand<CubeMoveData>(CommandType.CubeMoveCommand, cmd =>
        {
            cmd.deltaX = deltaX;
            cmd.deltaY = deltaY;
            cmd.deltaZ = deltaZ;
        });
    }

    /// <summary>
    /// 发送Cube确认命令给客户端（空格键放置/删除）
    /// </summary>
    /// <remarks>
    /// 简化版：只发送确认信号，物品信息已通过CreatePlantCubeCommand传递给客户端
    /// </remarks>
    public void SendCubeConfirmCommand()
    {
        SendCommand<CubeConfirmData>(CommandType.CubeConfirmCommand, cmd =>
        {
            // 不需要传递物品信息，客户端已通过CreatePlantCubeCommand获取
        });
    }

    #endregion
}
// 注意: CubeMoveData, CubeConfirmData, PlacementResultData 已移至共享定义
// 位置: Assets/Scripts/Shared/Network/NetworkDataModels.cs
