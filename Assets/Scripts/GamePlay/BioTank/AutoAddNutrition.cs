using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    /// <summary>
    /// 自动添加营养
    /// </summary>
    public void AutoAddNutrition()
    {
        if (NutrientProportion() < 0.5)
        {
            int num = (int)(GetNutrientMax() * 0.5 - Nutrient)+1;
            //记得扣费
            if (PlayerManager.Instance.OnMoneyChanged(-num))
            {
                OnNutrientChanged(num);
            }
        }
    }
}
