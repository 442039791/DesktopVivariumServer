
using UnityEngine;
using UnityEngine.UI;


public class ItemIcon : MonoBehaviour
{
    public Image Icon;
    public Image Frame;
    public GameObject Choice;
    public UI_IlluGuide uI_IlluGuide;
    public Button button;
    /// <summary>
    /// 当前选中的物品
    /// </summary>
    public (RewardType rewardType, string id) Selected;
    public void Init(Sprite ICon,Sprite BgSprite)
    {
        Icon.sprite = ICon;
        Frame.sprite = BgSprite;
        button.AddDeskTopListener(() => { Button(); });
        //Choice.SetActive(isEnabled);
    }
    public void Button()
    {
        uI_IlluGuide.SetSelected(Selected.rewardType,Selected.id);
    }
}
