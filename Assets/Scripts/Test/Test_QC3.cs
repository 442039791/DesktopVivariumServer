using SUIFW;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Test_QC3 : MonoBehaviour
{
    public Button button;
    public Button button2;
    public Button button3;
    public Button Gather;
    public Button DeleteSaveData;
    [Header("�鿨��������")]
    public string GatherName;
    public Transform testplantTransform;
    public ListController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Screen.SetResolution(1280, 720, false);


        Gather.AddDeskTopListener(() =>
        {
            UIManager.Instance.ShowUIForms("UI_Gather");
        });
        DeleteSaveData.AddDeskTopListener(() =>
        {
            GameManager.Instance.DoDeleteCommand();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
