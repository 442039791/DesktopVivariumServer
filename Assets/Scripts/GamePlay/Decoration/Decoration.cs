using System.Collections.Generic;
using Unity.VisualScripting;

using UnityEngine;

public class Decoration : MonoBehaviour
{
    public DecorationData data;
    public DecorationStateData stateData;
    public SpriteRenderer foregroundSprite;
    public SpriteRenderer midgroundSprite;
    public SpriteRenderer backgroundSprite;
    //public BioTankManager Parent
    //{
    //    get { return PlayerManager.Instance.BioTanks[stateData.ID]; }

    //}
    public int ID;
    public IDHolder idHolder;
    public void Init(DecorationStateData stateData)
    {
        ID = stateData.ObjID;
        this.stateData = stateData;
        data = GameConfigDataBase.GetConfigData<DecorationData>(int.Parse(stateData.Type), SysDefine.DecorationConfigData);

        BioTankRegistry.Instance.GetBioTank(stateData.ID)?.AddDecoration(this);
        //idHolder ??= transform.AddComponent<IDHolder>();
        //ID = IDFactory.GetUniqueID();
        //this.stateData = stateData;
        //data = GameConfigDataBase.GetConfigData<DecorationData>(int.Parse(stateData.Type), SysDefine.DecorationConfigData);


    }
    public void Destroy()
    {
        BioTankRegistry.Instance.GetBioTank(stateData.ID)?.RemoveDecoration(this);
        // 使用装饰物所属的生态箱ID（stateData.ID）而不是活动生态箱
        // 修复：多生态箱时消息错误发送给活动生态箱导致第一个客户端卡顿的问题
        WebSocketServerManager.GetClient(stateData.ID)?.SendRemovePlantCubeCommand(stateData.ObjID,(int)BlockType.Decoration);
        //Parent.RemoveDecoration(this);
        //PlayerManager.Instance.RemoveCube(stateData.Position);
    }

}
