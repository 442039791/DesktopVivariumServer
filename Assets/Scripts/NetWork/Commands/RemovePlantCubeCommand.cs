using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 移除植物方块命令
    /// </summary>
    public class RemovePlantCubeCommand : ServerCommandBase
    {
        public override CommandType Type => CommandType.RemovePlantCube;

        /// <summary>
        /// 需要在主线程执行（访问Unity API）
        /// </summary>
        protected override bool RequiresMainThread => true;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            var data = ParseJson<ReciveCreateBlockData>(jsonData);
            if (data == null)
            {
                return;
            }

            switch (data.BlockType)
            {
                case 1: // 装饰物
                    PlayerManager.Instance.ClientRemoveDecoration(data.objID);
                    break;

                case 2: // 植物
                    PlayerManager.Instance.ClientRemovePlant(data.objID);
                    break;

                default:
                    Debug.LogWarning($"[RemovePlantCube] 未知的方块类型: {data.BlockType}");
                    break;
            }
        }
    }
}
