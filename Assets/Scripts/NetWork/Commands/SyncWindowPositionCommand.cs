using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 同步窗口位置命令 - 处理客户端发送的窗口位置同步请求
    /// 客户端自主定位窗口后，通过此命令通知服务器当前窗口位置
    /// </summary>
    public class SyncWindowPositionCommand : ServerCommandBase
    {
        public override CommandType Type => CommandType.SyncWindowPosition;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            var data = ParseJson<SyncWindowPositionData>(jsonData);
            if (data == null)
            {
                Debug.LogError($"[SyncWindowPositionCommand] 解析数据失败，clientId: {clientId}");
                return;
            }

            var client = GetClient(clientId);
            if (client == null)
            {
                Debug.LogWarning($"[SyncWindowPositionCommand] 找不到客户端: {clientId}");
                return;
            }

            // 更新服务器端记录的客户端窗口位置和尺寸
            client.SetPosition(data.position);
            client.SetSize(new Vector2Int(data.width, data.height));

            Debug.Log($"[SyncWindowPositionCommand] 已同步客户端{clientId}窗口位置: ({data.position.x}, {data.position.y}), 尺寸: {data.width}x{data.height}");
        }
    }
}
