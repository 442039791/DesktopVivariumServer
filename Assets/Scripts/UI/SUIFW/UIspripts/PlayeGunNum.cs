using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayeGunNum : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    void Start()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }
        event_manager.instance.add_event_listener("PlayerGunNum", on_event_handler);
    }
    void on_event_handler(string name, object udata)
    {
        var num = "X" + (string)udata;
        textMesh.text = num;
    }

}
