using UnityEngine;
using System.Collections;

/// <summary>
/// 生态箱数量费用数据 - 对应 GridData.csv
/// 根据新建的是第几个生态箱，读取对应的价格
/// </summary>
public partial class GridData : GameConfigDataBase
{
    /// <summary>
    /// ID - 对应第几个生态箱（从1开始）
    /// </summary>
    public int ID;

    /// <summary>
    /// 购买价格 - 新建第ID个生态箱时需要支付的数量费用
    /// </summary>
    public int Price;

    protected override string getFilePath()
    {
        return "GridData.json";
    }
}
