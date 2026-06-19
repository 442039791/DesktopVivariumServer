
using SUIFW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceDialogUI : BaseUIForms
{
    private Button ChoiceTrue;
    private Button ChoiceFalse;
    public Text Text;

    public delegate void on_event_handler(int logic_select);
    public on_event_handler on_Event_Handler;
    public override bool Init()
    {
        base.Init();

        // 扫描并注册所有本地化文本（自动处理所有"Id:xxx"格式的文本）
        LocalizationManager.ScanAndRegisterTextsStatic(transform);

        if (ChoiceTrue == null)
        {
            ChoiceTrue = transform.Find("ChoiceTrue").GetComponentInChildren<Button>();
            ChoiceTrue.onClick.AddListener(delegate () {
                if (on_Event_Handler != null)
                {
                    on_Event_Handler(1);
                }
            });
        }
        if (ChoiceFalse == null)
        {
            ChoiceFalse = transform.Find("ChoiceFalse").GetComponentInChildren<Button>();
            ChoiceFalse.onClick.AddListener(delegate ()
            {
                if (on_Event_Handler != null)
                { on_Event_Handler(-1); }
            });
        }
        if(this.Text == null)
        {
            this.Text = GetComponentInChildren<Text>();
        }


        this.Text.text = "ѡ��";
        return true;
    }
    public void CloseUI()
    {
        CloseOrReturnUIForms();
    }



}
