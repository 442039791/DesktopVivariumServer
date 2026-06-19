using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestMoney : MonoBehaviour
{
    public TextMeshProUGUI Text;
    private void Awake()
    {
        Text.text = PlayerStateManager.Instance.StateData.Money.ToString();
        event_manager.instance.add_UIevent_listener("Money", (name, money) =>
        {
            string m = (string)money;
            Text.text = m;
        });
    }
}
