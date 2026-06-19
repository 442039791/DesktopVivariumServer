using Common;
using System;
using System.Collections.Generic;

public enum ButtonEvent
{
    SellAdult,
    SellChild,
    SaveChild,
    SaveAdult,
    ChangePlantORAnimal
}

public class MyButtonEvent:MonoSingleton<MyButtonEvent>
{
    public Dictionary<ButtonEvent, Action<object>> EventList=new();
    public bool InitCompelete;
    public override void Init()
    {
        if(!InitCompelete)
        {

        }
    }
}
