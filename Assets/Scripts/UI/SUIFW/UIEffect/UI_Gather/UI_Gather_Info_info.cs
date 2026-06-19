using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Gather_Info_info : MonoBehaviour
{
    public TextMeshProUGUI GatherName_name;
    public TextMeshProUGUI GatherName;
    public TextMeshProUGUI GatherPrice;
    public TextMeshProUGUI GatherPrice_name;
    public Button DoGather;
    public void Init(string _GatherName_name,string _GatherPrice_name)
    {
        GatherName_name.text = _GatherName_name;
        GatherPrice_name.text = _GatherPrice_name;
        DoGather.AddDeskTopListener(() => {
            PlayerManager.Instance.GatherToWareHouse();
        });
        
    }
}
