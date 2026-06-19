using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UI_Wildpicking : UIBase
{
    public Button TestGather;
    public override bool Init()
    {
        

        TestGather.AddDeskTopListener(() =>
        {
            var list = Gather.Instance.gatherAddressData.ToList();
            PlayerManager.Instance.GatherToWareHouse();
        });
        return base.Init();
    }
}
