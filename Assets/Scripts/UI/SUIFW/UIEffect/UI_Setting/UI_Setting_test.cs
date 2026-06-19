using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_Setting_test : MonoBehaviour
{
    public Slider slider;
    private float value;
    private float MaxValue = 1f;
    private float MinValue = 0f;
    public Button UpButton;
    public Button DownButton;
    public Button PosUpButton;
    public Button PosDownButton;
    public Button PosLeftButton;
    public Button PosRightButton;
    private List<TriggerConfig> triggerConfigs = new List<TriggerConfig>();
    private List<PosTriggerConfig> posTriggerConfigs = new List<PosTriggerConfig>();
    private class TriggerConfig
    {
        public MyButtonTriggerEvent trigger;
        public float moveDirection;
        public float activationDelay = 0.5f;
        public float repeatInterval = 0.1f;

        [HideInInspector] public float holdDuration;
        [HideInInspector] public float lastTriggerTime;
    }
    private class PosTriggerConfig
    {
        public MyButtonTriggerEvent trigger;
        public Vector2 moveDirection;
        public float activationDelay = 0.5f;
        public float repeatInterval = 0.1f;

        [HideInInspector] public float holdDuration;
        [HideInInspector] public float lastTriggerTime;
    }
    public void TriggerConfigInit(Button button, float moveDirection)
    {
        //var trigger = button.AddDeskTopStillClipListener();
        //triggerConfigs.Add( new TriggerConfig() { trigger = trigger, moveDirection = moveDirection });
    }
    public void PosTriggerConfigInit(Button button, Vector2 moveDirection)
    {
        //var trigger = button.AddDeskTopStillClipListener();
        //posTriggerConfigs.Add(new PosTriggerConfig() { trigger = trigger, moveDirection = moveDirection });
    }
    void Start()
    {
        UpButton.AddDeskTopListener(() => { value = Mathf.Clamp(value + 0.01f, MinValue, MaxValue); });
        DownButton.AddDeskTopListener(() => { value = Mathf.Clamp(value - 0.01f, MinValue, MaxValue); });
        PosUpButton.AddDeskTopListener(() => { WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(0,  3); });
        PosDownButton.AddDeskTopListener(() => { WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(0, -3); });
        PosLeftButton.AddDeskTopListener(() => { WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(-3, 0); });
        PosRightButton.AddDeskTopListener(() => { WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(3, 0); });
        PosTriggerConfigInit(PosUpButton, new Vector2(0, 3));
        PosTriggerConfigInit(PosDownButton, new Vector2(0, -3));
        PosTriggerConfigInit(PosLeftButton, new Vector2(-3, 0));
        PosTriggerConfigInit(PosRightButton, new Vector2(3, 0));
        TriggerConfigInit(UpButton, 0.01f);
        TriggerConfigInit(DownButton, -0.01f);
        slider.onValueChanged.AddListener((float value) => { this.value = value; });
    }
    
    // Update is called once per frame
    void Update()
    {
        foreach (var posTriggerConfig in posTriggerConfigs)
        {
            if (posTriggerConfig.trigger._OnPointerClick)
            {
                if (posTriggerConfig.holdDuration >= posTriggerConfig.activationDelay)
                {
                    if (posTriggerConfig.lastTriggerTime >= posTriggerConfig.repeatInterval)
                    {
                        WebSocketServerManager.GetActiveClient()?.SendCameraControlMoveCommand(posTriggerConfig.moveDirection.x, posTriggerConfig.moveDirection.y);
                        posTriggerConfig.lastTriggerTime = 0;
                    }
                    posTriggerConfig.lastTriggerTime += Time.deltaTime;
                }
                posTriggerConfig.holdDuration += Time.deltaTime;
            }
            else
            {
                posTriggerConfig.holdDuration = 0;
                posTriggerConfig.lastTriggerTime = 0;
            }
        }
        foreach (var triggerConfig in triggerConfigs)
        {
            if (triggerConfig.trigger._OnPointerClick)
            {
                if (triggerConfig.holdDuration >= triggerConfig.activationDelay)
                {
                    if (triggerConfig.lastTriggerTime >= triggerConfig.repeatInterval)
                    {

                        value = Mathf.Clamp(value + triggerConfig.moveDirection, MinValue, MaxValue);
                        triggerConfig.lastTriggerTime = 0;
                    }
                    triggerConfig.lastTriggerTime += Time.deltaTime;
                }
                triggerConfig.holdDuration += Time.deltaTime;
            }
            else
            {
                triggerConfig.holdDuration = 0;
                triggerConfig.lastTriggerTime = 0;
            }
        }
        //if (Onfist)
        //{
        //    Onfist = false;
        //    slider.value = 0.5f;
        //}
        //else
        //{
        //    if (value != LastValue)
        //    {
        //        LastValue = value;
        //        WebSocketServerManager.GetAcitveClient()?.SendCameraControlScrollCommand(value);
        //    }
        //}
        
    }
    public void Reset()
    {
        slider.value = 0.5f;
        value = 0.5f;
    }
}
