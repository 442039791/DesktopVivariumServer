using System.Linq;
using UnityEngine;

public class UI_Gather_Info : MonoBehaviour
{
    public ListController listController;
    public void Init()
    {
            listController.Init(SysDefine.PrefabAssetbundle+"UI_Gather_Info_info.prefab",
        Gather.Instance.gatherAddressData.Values.Count,
        InitGameobject
        );
    }
    public void InitGameobject(int index, GameObject @object)
    {
        var list = Gather.Instance.gatherAddressData.ToList();
        var info = @object.GetComponent<UI_Gather_Info_info>();
        info.Init(list[index].Key, list[index].Value.Price.ToString());
    }
}
