# Server 工程说明

`server/` 是 DesktopTerrarium 的服务端 Unity 工程，负责生态模拟、存档、Steam、模组和客户端调度。

## 基本信息

- Unity 版本：`6000.0.49f1`
- 渲染管线：URP `17.0.3`
- 入口文件：[Assets/Scripts/Core/GameManager.cs](/C:/1/server/Assets/Scripts/Core/GameManager.cs)
- 主要启动流程：[Assets/Scripts/Core/Initializer/GameInitializer.cs](/C:/1/server/Assets/Scripts/Core/Initializer/GameInitializer.cs)

## 主要职责

- 初始化 Steam、本地化、日志、模组
- 加载和保存主存档
- 创建生态缸、动植物、设施、仓库和引导状态
- 启动 WebSocket 服务
- 拉起客户端进程
- 维护休眠客户端池

## 当前目录重点

```text
server/
├── Assets/Scripts/Core/
│   ├── GameManager.cs
│   ├── Initializer/
│   ├── SaveSystem/
│   ├── DependencyInjection/
│   ├── Timer/
│   ├── Mod/
│   ├── PreloadClientPool/
│   └── TankInitialLayout/
├── Assets/Scripts/GamePlay/
├── Assets/Scripts/Network/
├── Assets/Scripts/Framework/
├── Assets/Scripts/UI/
└── Assets/StreamingAssets/
```

## 启动顺序

1. `GameManager.Init()`
2. 初始化日志系统
3. 初始化 Steam、本地化、Mod、WallpaperManager、DI、SaveSystem、TimerSystem
4. `WebSocketServerManager` 启动监听并写入 `StreamingAssets/port.txt`
5. `GameInitializer.StartGame()` 加载存档并创建运行时对象
6. 根据生态缸数量启动客户端进程
7. 初始化预加载休眠客户端

## 网络模型

### 服务端网络入口

- [Assets/Scripts/Network/WebSocketServerManager.cs](/C:/1/server/Assets/Scripts/Network/WebSocketServerManager.cs)
- [Assets/Scripts/Network/ControlBehavior.cs](/C:/1/server/Assets/Scripts/Network/ControlBehavior.cs)

### 工作方式

- 监听地址：`127.0.0.1`
- 端口：运行时动态分配
- 路径：`/control`
- 端口文件：`Assets/StreamingAssets/port.txt`

### 客户端分配策略

- 普通客户端：绑定一个现有 BioTank ID，随后发送 `SetBioTankID + Load`
- 休眠客户端：发送 `EnterDormantMode`，进入预加载池等待后续复用

## 存档模型

服务端当前使用 `SaveSystem` 管理两层存档：

### 1. 主存档

- 文件结构：`ServerPackage`
- 内容包括：
  - `SaveData`
  - `GatherSaveData`
  - 服务端设置

### 2. 生态缸独立包

- 文件名：`{BioTankId}.biotank.dat`
- 内容包括：
  - 地形数据
  - 摄像机参数
  - 窗口参数

核心实现：

- [Assets/Scripts/Core/SaveSystem/SaveSystem.cs](/C:/1/server/Assets/Scripts/Core/SaveSystem/SaveSystem.cs)

## 关键系统

### SaveSystem

- 负责收集运行时数据并落盘
- 负责旧存档迁移
- Steam 版本下负责云存档同步

### GameInitializer

- 负责从存档恢复生态缸、动物、植物、设施和仓库
- 在首次启动时从模板创建默认数据

### PreloadClientPoolManager

- 负责维护休眠客户端
- 降低新增生态缸或切换展示时的进程启动成本

### TankInitialLayout

- 负责读取初始布局 JSON
- 支持按尺寸匹配默认布局

## 开发注意事项

1. 不要再把服务端理解成“只跑模拟逻辑的无头程序”，它仍然依赖 Unity 场景、资源和部分 UI 初始化。
2. 新增网络命令时，优先修改 `Packages/com.desktopterrarium.network`，不要在 client / server 各写一份。
3. 修改存档格式时，必须同时考虑旧存档迁移和客户端独立包兼容。
4. 涉及客户端启动逻辑时，优先检查 `HiddenProcessLauncher`、`PreloadClientPoolManager`、`ControlBehavior`。

## 相关文档

- 总体架构：[../PROJECT_ARCHITECTURE.md](/C:/1/PROJECT_ARCHITECTURE.md)
- 项目总览：[../README.md](/C:/1/README.md)
- 初始布局说明：[../TankInitialLayout_README.md](/C:/1/TankInitialLayout_README.md)
