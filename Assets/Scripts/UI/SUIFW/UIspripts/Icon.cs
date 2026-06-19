
using Common;
using UnityEngine;
using UnityEngine.UI;


public class Icon : MonoBehaviour
{
    /// <summary>
    /// 详情按钮
    /// </summary>
    public GameObject _Exclaim;
    /// <summary>
    /// 物品图标
    /// </summary>
    public Image _Icon;
    /// <summary>
    /// 品质边框
    /// </summary>
    public Image _Frame;
    /// <summary>
    /// 当前选中的物品
    /// </summary>
    public (RewardType rewardType, string id) Selected;
    public void Init(RewardType rewardType, string id)
    {
        Selected.rewardType = rewardType;
        Selected.id = id;
        var exclaimButton=_Exclaim.GetComponent<Button>() ;
        exclaimButton.AddDeskTopListener(() => { Button(); });
        _Exclaim.SetActive(true);
    }

    public void Button()
    {
        event_manager.instance.dispatch_UIevent(SysDefine.UI_LllustratedGuideEvent, Selected);
    }
}
