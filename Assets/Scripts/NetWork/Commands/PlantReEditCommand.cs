using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 植物重新编辑命令
    /// </summary>
    public class PlantReEditCommand : ServerCommandBase
    {
        public override CommandType Type => CommandType.PlantReEdit;

        /// <summary>
        /// 需要在主线程执行（访问Unity API和事件系统）
        /// </summary>
        protected override bool RequiresMainThread => true;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            var data = ParseJson<ReciveCreateBlockData>(jsonData);
            if (data == null)
            {
                return;
            }

            // 安全地获取生物罐
            var bioTank = PlayerManager.Instance?.GetBioTank(PlayerManager.ActiveBioTanksID);
            if (bioTank == null)
            {
                Debug.LogError($"[PlantReEdit] 未找到生物罐: {PlayerManager.ActiveBioTanksID}");
                return;
            }

            // 安全地获取植物
            if (!bioTank.PlantCollection.TryGetById(data.objID, out var plant))
            {
                Debug.LogError($"[PlantReEdit] 未找到植物: {data.objID}");
                return;
            }

            // 更新植物位置
            plant.stateData.Pos = data.position;
            UI_BioTankInfo_CreatureInfo_PlantInfo.ResetEditPlantDic();
            event_manager.instance.dispatch_event("PlantReEdit", -99);

        }
    }
}
