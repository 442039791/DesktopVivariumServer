# UnlockableItemManager 使用说明

## 概述

`UnlockableItemManager` 是一个专门管理Cube和Decoration等解锁型物品的管理器。

### 解锁型物品 vs 消耗型物品

- **解锁型物品** (Cube, Decoration)：解锁后永久可用，可以无限次放置（每次放置消耗金币，但不消耗物品本身）
- **消耗型物品** (鱼, 植物)：每次使用会从仓库中移除

## 主要功能

### 1. 解锁物品

```csharp
// 解锁单个Cube
UnlockableItemManager.Instance.UnlockCube("cube_001");

// 解锁单个Decoration
UnlockableItemManager.Instance.UnlockDecoration("deco_001");

// 批量解锁
UnlockableItemManager.Instance.UnlockCubes(new[] { "cube_001", "cube_002" });

// 解锁所有配置的Cube
UnlockableItemManager.Instance.UnlockAllCubes();

// 解锁所有配置的Decoration
UnlockableItemManager.Instance.UnlockAllDecorations();
```

### 2. 查询解锁状态

```csharp
// 检查Cube是否已解锁
bool isUnlocked = UnlockableItemManager.Instance.IsCubeUnlocked("cube_001");

// 检查Decoration是否已解锁
bool isUnlocked = UnlockableItemManager.Instance.IsDecorationUnlocked("deco_001");

// 获取所有已解锁的Cube ID列表
List<string> cubeIds = UnlockableItemManager.Instance.GetUnlockedCubeIds();

// 获取已解锁的Cube数量
int count = UnlockableItemManager.Instance.GetUnlockedCubeCount();
```

### 3. 同步到仓库

```csharp
// 从图鉴系统同步已解锁状态
UnlockableItemManager.Instance.SyncFromIllustratedGuide();

// 同步所有已解锁物品到仓库
UnlockableItemManager.Instance.SyncToWarehouse();
```

### 4. 调试工具

```csharp
// 打印所有已解锁物品信息
UnlockableItemManager.Instance.PrintUnlockedItems();
```

## 与其他系统的集成

### 图鉴系统 (IlluGuideMgr)

当通过图鉴系统解锁Cube或Decoration时，会自动通知`UnlockableItemManager`：

```csharp
// 在IlluGuideMgr.UnlockEntry中
if (category == E_IllustratedGuideCategory.Cube)
{
    UnlockableItemManager.Instance.UnlockCube(itemId, true);
}
```

### 仓库系统 (WarehouseManager)

`UnlockableItemManager`会自动将已解锁的物品同步到仓库系统，确保UI能正确显示：

```csharp
// 解锁时自动同步到仓库
UnlockableItemManager.Instance.UnlockCube("cube_001", syncToWarehouse: true);
```

### UI_BioTank_Edit界面

在打开编辑界面时，会自动同步已解锁物品：

```csharp
// 在UI_BioTank_Edit.Init中
UnlockableItemManager.Instance.SyncFromIllustratedGuide();
UnlockableItemManager.Instance.SyncToWarehouse();
```

## Unity编辑器工具

选中`UI_BioTank_Edit`组件后，在Inspector面板中会显示调试工具按钮：

- **解锁所有Cube**: 快速解锁所有配置的Cube
- **解锁所有Decoration**: 快速解锁所有配置的Decoration
- **强制同步已解锁物品到仓库**: 修复同步问题
- **刷新UI**: 刷新编辑界面

## 常见问题

### Q: UI_BioTank_Edit界面中看不到Cube？

A: 按照以下步骤排查：

1. 确认Cube已在图鉴中解锁：
   ```csharp
   IlluGuideMgr.Instance.cubeEntries[cubeId].isUnlocked
   ```

2. 使用编辑器工具"解锁所有Cube"按钮

3. 强制同步：
   ```csharp
   UnlockableItemManager.Instance.SyncFromIllustratedGuide();
   UnlockableItemManager.Instance.SyncToWarehouse();
   ```

4. 刷新UI：
   ```csharp
   UI_BioTank_Edit.CurrentInstance.TestRefresh();
   ```

### Q: 如何为新的Cube/Decoration配置解锁？

A: 新的Cube/Decoration会通过以下方式解锁：

1. **通过游戏事件自动解锁**：
   ```csharp
   event_manager.instance.dispatch_event("Craft_OnCubeObtained", cubeId);
   ```

2. **通过图鉴系统解锁**：
   ```csharp
   IlluGuideMgr.Instance.UnlockEntry(cubeId, E_IllustratedGuideCategory.Cube);
   ```

3. **直接通过管理器解锁**：
   ```csharp
   UnlockableItemManager.Instance.UnlockCube(cubeId);
   ```

### Q: 数据如何持久化？

A: 使用以下方法保存和加载：

```csharp
// 获取保存数据
var saveData = UnlockableItemManager.Instance.GetSaveData();

// 加载保存数据
UnlockableItemManager.Instance.LoadSaveData(saveData);
```

## 事件系统

`UnlockableItemManager`会触发以下事件：

- `UnlockableItem_CubeUnlocked`: Cube解锁时触发，参数为cubeId
- `UnlockableItem_DecorationUnlocked`: Decoration解锁时触发，参数为decorationId
- `UnlockableItem_SyncComplete`: 同步完成时触发

监听事件：

```csharp
event_manager.instance.add_event_listener("UnlockableItem_CubeUnlocked", (name, data) =>
{
    string cubeId = (string)data;
    Debug.Log($"Cube解锁: {cubeId}");
});
```

## 架构设计

```
[游戏逻辑] --> [IlluGuideMgr] --> [UnlockableItemManager] --> [WarehouseManager] --> [UI显示]
                    ↓                       ↓
                图鉴展示              解锁状态管理 + 仓库同步
```

这种架构确保了：
1. **单一职责**：每个管理器只负责自己的功能
2. **数据一致性**：通过UnlockableItemManager统一管理解锁状态
3. **灵活扩展**：可以轻松添加新的解锁型物品类型

## 最佳实践

1. **优先使用UnlockableItemManager解锁物品**：而不是直接操作仓库
2. **在游戏初始化时调用SyncFromIllustratedGuide()**：确保数据一致性
3. **使用事件系统监听解锁**：而不是轮询查询
4. **定期保存解锁数据**：防止玩家进度丢失
