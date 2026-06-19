/*****************************************************************
 * --@ FileName: FacilityDefine
 * --@ Description: 设备定义
 * --@ Date: 2025/08/22 10:25
 * --@ Author: MagicianJoker, fengliudianshao@gmail.com
 * --@ Copyright:  Copyright (c) 2025, MagicianJoker
 ******************************************************************/

public class FacilityDefine
{
        
}

public enum FacilityState
{
    /// <summary>
    /// 未解锁
    /// </summary>
    Unlock,

    /// <summary>
    /// 已解锁未购买
    /// </summary>
    NoBuy,

    /// <summary>
    /// 已购买未激活
    /// </summary>
    NoActivation,

    /// <summary>
    /// 已激活
    /// </summary>
    Activation
}