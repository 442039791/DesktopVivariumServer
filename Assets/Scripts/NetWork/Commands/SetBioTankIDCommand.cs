using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 设置生物罐ID命令
    /// </summary>
    public class SetBioTankIDCommand : ServerCommandBase
    {
        public override CommandType Type => CommandType.SetBioTankID;

        /// <summary>
        /// 需要在主线程执行（访问Unity API）
        /// </summary>
        protected override bool RequiresMainThread => true;

        protected override void ExecuteInternal(string jsonData, int clientId)
        {
            var data = ParseJson<ReceiveID>(jsonData);
            if (data == null)
            {
                return;
            }

            if (data.objID != -1)
            {
                if (BioTankRegistry.Instance.TryGetBioTank(data.objID, out var bioTank))
                {
                    // 设置活动生物罐，但不改变窗口层级（未来会有专门的功能控制窗口层级）
                    BioTankRegistry.Instance.SetActiveBioTank(bioTank);

                    BioTankRegistry.Instance.IsInTank = true;
                }
                else
                {
                    BioTankRegistry.Instance.IsInTank = false;
                    Debug.LogWarning($"[SetBioTankID] 未找到生物罐: {data.objID}");
                }
            }
            else
            {
                BioTankRegistry.Instance.IsInTank = false;
            }
        }
    }
}
