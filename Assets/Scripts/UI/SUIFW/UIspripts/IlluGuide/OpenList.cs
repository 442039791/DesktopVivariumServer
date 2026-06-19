
using Common;
using UnityEngine;
using UnityEngine.UI;


public class OpenList : MonoBehaviour
{
    public RewardType rewardType;
    UI_IlluGuide uI_IlluGuide;

    public void OnEnable()
    {
        uI_IlluGuide= MonoSingleton< UI_IlluGuide >.Instance;
        GetComponent<Button>().AddDeskTopListener(() => { Button(); });
        //Choice.SetActive(isEnabled);
    }
    public void Button()
    {

        uI_IlluGuide.OpenList(rewardType);
    }
}
