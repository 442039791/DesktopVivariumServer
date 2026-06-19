using UnityEngine;
using UnityEngine.UI;
using Common;
using TMPro;
using System.Collections.Generic;
using TMPro.Examples;


public class UI_Log : MonoSingleton<UI_Log>
{
    public GameObject _LogList;
    public GameObject _LogGo;
    public Button _LogBtn;
    List<TextMeshProUGUI> logGoList;
    bool isInit = false;
    public void OnEnable()
    {
        if (!isInit)
        {
            logGoList = new();
            for (int i = 0; i < 100; i++)
            {
                GameObject instance = Instantiate(_LogGo, _LogList.transform);
                instance.SetActive(true);
                instance.transform.Find("describe").GetComponent<TextMeshProUGUI>().text = "";
                logGoList.Add(instance.transform.Find("describe").GetComponent<TextMeshProUGUI>());
            }
            _LogBtn.AddDeskTopListener(() => { Button(); });
            isInit = true;
        }
    }

    public void AddLog(string describe)
    {

        foreach (var go in logGoList)
        {
            if (go.text=="")
            {
                go.text= describe;
                break;
            }
        }
    }

    public void Button()
    {
        foreach (var go in logGoList)
        {
            go.text="";
        }
    }
}
