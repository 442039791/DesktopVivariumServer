using UnityEngine;
/// <summary>
/// 工具类
/// </summary>
public static partial class Tools
{
    /// <summary>
    /// 钱展示的效果
    /// </summary>
    /// <param name="money">钱的数量</param>
    /// <param name="free">是否展示免费的效果，true为展示免费效果</param>
    /// <returns></returns>
    public static string MoneyString(int money,bool free=true)
    {
        if (money == 0&&!free)
            return LocalizationManager.GetCurrentLanguageByID(SysDefine.Language_Free);

        // 处理不同范围的数字格式
        if (money < 10000)
        {
            return money.ToString("N0");
        }
        else if (money < 10000000) // 1000到999万
        {
            double value = money / 1000.0;
            return value.ToString(value >= 100 ? "0" : "0.0") + "K"; // 智能小数位控制
        }
        else // 1000万以上
        {
            double value = money / 1000000.0;
            return value.ToString(value >= 100 ? "0" : "0.0") + "M"; // 智能小数位控制
        }
    }

    /// <summary>
    /// 时间的展示效果
    /// </summary>
    public static string TimeString(int time)
    {
        // 计算分钟和秒
        int minutes = time / 60;
        int seconds = time % 60;
        //string timeString = minutes + "m" + ":" + seconds + "s";
        //return timeString;
        // 格式化为 mm:ss（自动补零）
        return string.Format("{0}m:{1:00}s", minutes ,seconds);
    }
/// <summary>
/// 得到鱼的icon
/// </summary>
/// <param name="id">鱼的id</param>
/// <param name="quality">鱼的品质，如果鱼没有解锁则为-1</param>
/// <param name="isAdult">成年还是幼年，true为成年</param>
/// <returns></returns>
    public static Sprite GetFishIcon( string id,int quality,bool isAdult)
    {
        Sprite sprite = CoreSetting.Instance.UI_UnKnow_sprite;
        var fishData = GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(id), SysDefine.AnimalConfigData);
        if (fishData != null)
        {
            if (quality >= 0)
            {
                sprite = fishData.GetIconSprite((ObjectQuality)quality, isAdult);
            }
        }
        return sprite;
    }
    /// <summary>
    /// 获取道具icon
    /// </summary>
    /// <param name="type">道具类型</param>
    /// <param name="id"></param>
    /// <param name="isUnlock">是否解锁</param>
    /// <returns></returns>
    public static Sprite GetItemIcon(RewardType type,string id,bool isUnlock)
    {
        Sprite sprite = CoreSetting.Instance.UI_UnKnow_sprite;
        if (isUnlock)
        {
            switch (type)
            {
                case RewardType.Plant:
                    var data = GameConfigDataBase.GetConfigData<PlantData>(int.Parse(id), SysDefine.PlantConfigData);
                    sprite = data.GetIconSprite();
                break;
                case RewardType.Facilities:
                    var facilitiesData = GameConfigDataBase.GetConfigData<FacilitiesData>(int.Parse(id), SysDefine.FacilitiesConfigData);
                    sprite = facilitiesData.GetIconSprite();
                    break;
                case RewardType.Decoration:
                    var decorationData = GameConfigDataBase.GetConfigData<DecorationData>(int.Parse(id), SysDefine.DecorationConfigData);
                    sprite = decorationData.GetIconSprite();
                    break;
                case RewardType.Tank:
                    var tankData = GameConfigDataBase.GetConfigData<TankData>(int.Parse(id), SysDefine.TankConfigData);
                    sprite = tankData.GetIconSprite();
                    break;
                case RewardType.Cube:
                    var cubeData = GameConfigDataBase.GetConfigData<CubeData>(int.Parse(id), SysDefine.CubeConfigData);
                    sprite = cubeData.GetIconSprite();
                    break;
                case RewardType.Admission:
                    var admissionData = GameConfigDataBase.GetConfigData<AdmissionData>(int.Parse(id), SysDefine.AdmissionConfigData);
                    sprite = admissionData.GetIconSprite();
                    break;
            }
        }
        return sprite;
    }

    /// <summary>
    /// 得到边框
    /// </summary>
    /// <param name="id">鱼的id</param>
    /// <param name="quality">鱼的品质，如果鱼没有解锁则为-1</param>
    /// <returns></returns>
    public static Sprite GetFrame(int quality)
    {
        Sprite sprite = CoreSetting.Instance.GetQualityBgSprite(0);
        if (quality >= 0)
        {
            sprite = CoreSetting.Instance.GetQualityBgSprite((ObjectQuality)quality);
        }
        return sprite;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">鱼的id</param>
    /// <param name="quality">鱼的品质</param>
    /// <param name="isAdult">成年还是幼年，true为成年</param>
    /// <returns></returns>
    public static int GetFishPrice(string id,int quality,bool isAdult)
    {
        float Magnification = 1f;
        var fishData = GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(id), SysDefine.AnimalConfigData);
        float price = isAdult ? fishData.adultPurchasePrice : fishData.juvenilePurchasePrice;
        switch (quality)
        {
            case 0:
                Magnification = SysDefine.Price_Magnification_White;
                break;
            case 1:
                Magnification = SysDefine.Price_Magnification_Blue;
                break;
            case 2:
                Magnification = SysDefine.Price_Magnification_Purple;
                break;
            case 3:
                Magnification = SysDefine.Price_Magnification_Gold;
                break;

        }
        return (int)(price * Magnification);
    }
}
