using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 创建植物方块命令
    /// </summary>
    public class CreatePlantCubeCommand : ServerCommandBase
    {
        public override CommandType Type => CommandType.CreatePlantCube;

        /// <summary>
        /// 需要在主线程执行（访问Unity API）
        /// </summary>
        protected override bool RequiresMainThread => true;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            var data = ParseJson<ReciveCreateBlockData>(jsonData);
            if (data == null)
            {
                Debug.LogError($"[CreatePlantCubeCommand] ParseJson失败");
                return;
            }

            // 现在只处理植物（blockType == 2）
            // Cube和Decoration已改为解锁型物品，不需要通过此命令处理
            if (data.BlockType == 2)
            {
                PlayerManager.Instance.CreateSavePlant(clientId, data.objID, data.position);
            }
            else
            {
                Debug.LogWarning($"[CreatePlantCube] 收到非植物类型的请求: {data.BlockType}（Cube和Decoration现在应使用CreateCubeCommand）");
            }
        }
    }
}
