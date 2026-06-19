using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    /// <summary>
    /// 素食动物吃饭
    /// </summary>
    public void WhiteEat(float IntervalTime)
    {
        //素食动物群落数量
        int whiteNum = 0;
        //素食供给
        float whiteProvide = 0;
        //素食需求
        float whiteDemand = 0;
        //指定素食供给
        Dictionary<string, float> specificWhiteProvideDIC = new();
        //指定素食拥有的动物群落数量
        Dictionary<string, int> specificWhiteNumDIC = new();
        //指定素食需求
        Dictionary<string, float> specificWhiteDemandDIC = new();
        //哪些动物要吃饭
        List<Animal> FeedingAnimalList = new();
        //统计食物需求
        foreach (var animal in AnimalCollection.Entities)
        {
            float foodReserveMax = animal.data.foodReserve;
            if (animal.FoodReserve >= animal.data.foodReserve)
            {
                continue;
            }
            if (animal.data.foodType == FoodType.Meat)
            {
                continue;
            }
            if (animal.data.foodType == FoodType.SpecificMeat)
            {
                continue;
            }
            FeedingAnimalList.Add(animal);
            if (animal.data.foodType == FoodType.White)
            {
                whiteNum++;
                whiteDemand += foodReserveMax - animal.FoodReserve;
            }

            else if (animal.data.foodType == FoodType.SpecificWhite)
            {
                whiteNum++;
                if (!specificWhiteDemandDIC.ContainsKey(animal.data.foodParameter))
                {
                    specificWhiteDemandDIC.Add(animal.data.foodParameter, 0);
                    specificWhiteNumDIC.Add(animal.data.foodParameter, 0);
                }
                specificWhiteDemandDIC[animal.data.foodParameter] += foodReserveMax - animal.FoodReserve;
                specificWhiteNumDIC[animal.data.foodParameter]++;
            }
        }
        //统计素食供给
        if (IsNutrientConsume())
        {
            foreach (var plant in PlantCollection.Entities)
            {
                whiteProvide += plant.data.FoodNum * plant.GrowthProgress * IntervalTime;
                //看看有没有对这个植物有特性需求的
                foreach (var item in specificWhiteDemandDIC)
                {
                    if (item.Key == plant.data.ID)
                    {
                        if (!specificWhiteProvideDIC.ContainsKey(item.Key))
                        {
                            specificWhiteProvideDIC.Add(item.Key, 0);
                        }
                        specificWhiteProvideDIC[item.Key] += plant.data.FoodNum * plant.GrowthProgress * IntervalTime;
                        break;
                    }
                }
            }
        }
        //族群增加食物
        foreach (var animal in FeedingAnimalList)
        {
            float foodReserveMax = animal.data.foodReserve;
            if (animal.data.foodType == FoodType.White && whiteProvide > 0 && whiteDemand > 0)
            {
                float num = whiteProvide * (foodReserveMax - animal.FoodReserve) / whiteDemand;
                if (num > foodReserveMax - animal.FoodReserve)
                {
                    num = foodReserveMax - animal.FoodReserve;
                }
                animal.FoodReserve += num;

            }
            else if (animal.data.foodType == FoodType.SpecificWhite && specificWhiteProvideDIC[animal.data.foodParameter] > 0)
            {
                float num = whiteProvide * (foodReserveMax - animal.FoodReserve) / whiteDemand;
                if (num > specificWhiteProvideDIC[animal.data.foodParameter] * (foodReserveMax - animal.FoodReserve) / specificWhiteDemandDIC[animal.data.foodParameter])
                {
                    num = specificWhiteProvideDIC[animal.data.foodParameter] * (foodReserveMax - animal.FoodReserve) / specificWhiteDemandDIC[animal.data.foodParameter];
                }
                if (num > foodReserveMax - animal.FoodReserve)
                {
                    num = foodReserveMax - animal.FoodReserve;
                }
                animal.FoodReserve += num;
            }
        }
    }
    /// <summary>
    /// 肉食动物吃饭
    /// </summary>
    public void MeatEat()
    {
        //肉食动物群落数量
        int meatNum = 0;
        //肉食供给
        float meatProvide = 0;
        //肉食需求
        float meatDemand = 0;
        //肉食实际消耗了多少
        float newMeatDemand = 0;
        //指定肉食供给
        Dictionary<string, float> specificMeatProvideDIC = new();
        //指定肉食拥有的动物群落数量
        Dictionary<string, int> specificMeatNumDIC = new();
        //指定肉食需求
        Dictionary<string, float> specificMeatDemandDIC = new();
        //指定肉食真实消耗
        Dictionary<string, float> newSpecificMeatDemandDIC = new();
        //哪些动物要吃饭
        List<Animal> FeedingAnimalList = new();
        //统计食物需求
        foreach (var animal in AnimalCollection.Entities)
        {
            float foodReserveMax = animal.data.foodReserve;
            if (animal.FoodReserve >= animal.data.foodReserve)
            {
                continue;
            }
            if (animal.data.foodType == FoodType.White)
            {
                continue;
            }
            if (animal.data.foodType == FoodType.SpecificWhite)
            {
                continue;
            }

            FeedingAnimalList.Add(animal);
            if (animal.data.foodType == FoodType.Meat)
            {
                meatNum++;
                meatDemand += foodReserveMax - animal.FoodReserve;
            }
            else if (animal.data.foodType == FoodType.SpecificMeat)
            {
                meatNum++;
                if (!specificMeatDemandDIC.ContainsKey(animal.data.foodParameter))
                {
                    specificMeatDemandDIC.Add(animal.data.foodParameter, 0);
                    specificMeatNumDIC.Add(animal.data.foodParameter, 0);
                }
                specificMeatDemandDIC[animal.data.foodParameter] += foodReserveMax - animal.FoodReserve;
                specificMeatNumDIC[animal.data.foodParameter]++;
            }
        }
        //统计肉食供给
        foreach (var animal in AnimalCollection.Entities)
        {
            meatProvide += animal.data.FoodNum;
            //看看有没有对这个动物有特殊需求的
            foreach (var item in specificMeatDemandDIC)
            {
                if (item.Key == animal.data.ID)
                {
                    if (!specificMeatProvideDIC.ContainsKey(item.Key))
                    {
                        specificMeatProvideDIC.Add(item.Key, 0);
                    }
                    specificMeatProvideDIC[item.Key] += animal.data.FoodNum;
                    break;
                }
            }
        }
        //族群增加食物
        foreach (var animal in FeedingAnimalList)
        {
            float foodReserveMax = animal.data.foodReserve;
            if (animal.data.foodType == FoodType.Meat && meatProvide > 0 && meatDemand > 0)
            {
                float num = meatProvide * (foodReserveMax - animal.FoodReserve) / meatDemand;
                if (num > foodReserveMax - animal.FoodReserve)
                {
                    num = foodReserveMax - animal.FoodReserve;
                }
                animal.FoodReserve += num;
                newMeatDemand += num;
            }
            else if (animal.data.foodType == FoodType.SpecificMeat && specificMeatProvideDIC[animal.data.foodParameter] > 0)
            {
                float num = meatProvide * (foodReserveMax - animal.FoodReserve) / meatDemand;
                if (num > specificMeatProvideDIC[animal.data.foodParameter] * (foodReserveMax - animal.FoodReserve) / specificMeatDemandDIC[animal.data.foodParameter])
                {
                    num = specificMeatProvideDIC[animal.data.foodParameter] * (foodReserveMax - animal.FoodReserve) / specificMeatDemandDIC[animal.data.foodParameter];
                }
                if (num > foodReserveMax - animal.FoodReserve)
                {
                    num = foodReserveMax - animal.FoodReserve;
                }
                animal.FoodReserve += num;
                if (!newSpecificMeatDemandDIC.ContainsKey(animal.data.foodParameter))
                {
                    newSpecificMeatDemandDIC.Add(animal.data.foodParameter, 0);
                }
                newSpecificMeatDemandDIC[animal.data.foodParameter] += num;
            }
        }
    }
    /// <summary>
    /// 投喂素食的行为
    /// </summary>
    /// <param name="whiteProvide">投喂素食的数量。</param>
    public void FeedingWhiteBehavior(float whiteProvide)
    {
        //素食需求
        float whiteDemand = 0;
        //哪些动物要吃饭
        List<Animal> FeedingAnimalList = new();
        //统计食物需求
        foreach (var animal in AnimalCollection.Entities)
        {
            if (animal.State== AnimalAgeStatus.AdultDeath|| animal.State == AnimalAgeStatus.JuvenileDeath)
            {
                continue;
            }
            if (animal.data.foodType == FoodType.White)
            {
                if (animal.FoodReserve < animal.data.foodReserve)
                {
                    whiteDemand += animal.data.foodReserve - animal.FoodReserve;
                    FeedingAnimalList.Add(animal);
                }
            }
        }
        //族群增加食物
        foreach (var animal in FeedingAnimalList)
        {
            float num = whiteProvide * (animal.data.foodReserve - animal.FoodReserve) / whiteDemand;
            if (num > animal.data.foodReserve - animal.FoodReserve)
            {
                num = animal.data.foodReserve - animal.FoodReserve;
            }
            animal.FoodReserve += num;
        }
    }
    /// <summary>
    /// 投喂肉食的行为
    /// </summary>
    /// <param name="meatProvide">投喂肉食的数量。</param>
    public void FeedingMeatBehavior(float meatProvide)
    {
        //素食需求
        float whiteDemand = 0;
        //哪些动物要吃饭
        List<Animal> FeedingAnimalList = new();
        //统计食物需求
        foreach (var animal in AnimalCollection.Entities)
        {
            if (animal.State == AnimalAgeStatus.AdultDeath || animal.State == AnimalAgeStatus.JuvenileDeath)
            {
                continue;
            }
            if (animal.data.foodType == FoodType.Meat)
            {
                if (animal.FoodReserve < animal.data.foodReserve)
                {
                    whiteDemand += animal.data.foodReserve - animal.FoodReserve;
                    FeedingAnimalList.Add(animal);
                }
            }
        }
        //族群增加食物
        foreach (var animal in FeedingAnimalList)
        {
            float num = meatProvide * (animal.data.foodReserve - animal.FoodReserve) / whiteDemand;
            if (num > animal.data.foodReserve - animal.FoodReserve)
            {
                num = animal.data.foodReserve - animal.FoodReserve;
            }
            animal.FoodReserve += num;
        }
    }
    /// <summary>
    /// 获取食物1小时数据的变化量
    /// </summary>
    /// <param name="whiteNum">素食数量。</param>
    /// <param name="meatNum">肉食数量。</param>
    /// <param name="specificWhiteDIC">指定素食数量。</param>
    /// <param name="specificMeatDIC">指定肉食数量。</param>
    public void GetFoodHourChange(out float whiteNum, out float meatNum, out Dictionary<string, float> specificWhiteDIC, out Dictionary<string, float> specificMeatDIC)
    {
        whiteNum = 0f;
        meatNum = 0f;
        specificWhiteDIC = new();
        specificMeatDIC = new();
        //先统计需求
        foreach (var item in AnimalCollection.Entities)
        {
            if (item.State == AnimalAgeStatus.AdultDeath || item.State == AnimalAgeStatus.JuvenileDeath)
            {
                continue;
            }
            if (item.data.foodType == FoodType.White)
            {
                whiteNum -= item.GetFoodHourChange();
            }
            else if (item.data.foodType == FoodType.Meat)
            {
                meatNum -= item.GetFoodHourChange() * 100;
            }
            else if (item.data.foodType == FoodType.SpecificWhite)
            {
                if (!specificWhiteDIC.ContainsKey(item.data.foodParameter))
                {
                    specificWhiteDIC.Add(item.data.foodParameter, 0);
                }
                specificWhiteDIC[item.data.foodParameter] -= item.GetFoodHourChange();
            }
            else if (item.data.foodType == FoodType.SpecificMeat)
            {
                if (!specificMeatDIC.ContainsKey(item.data.foodParameter))
                {
                    specificMeatDIC.Add(item.data.foodParameter, 0);
                }
                specificMeatDIC[item.data.foodParameter] -= item.GetFoodHourChange();
            }
        }
        //计算植物产出素食
        if (IsNutrientConsume())
        {

            foreach (var item in PlantCollection.Entities)
            {
                whiteNum += item.data.FoodNum * item.GrowthProgress * 60;
                if (specificWhiteDIC.ContainsKey(item.data.ID))
                {
                    specificWhiteDIC[item.data.ID] += item.data.FoodNum * item.GrowthProgress * 60;
                }
            }
        }

        //计算动物产出的肉食
        foreach (var item in AnimalCollection.Entities)
        {
            meatNum += item.data.FoodNum;
            if (specificMeatDIC.ContainsKey(item.data.ID))
            {
                specificMeatDIC[item.data.ID] += item.data.FoodNum;
            }
        }
    }
    /// <summary>
    /// 获取食物的相关数据
    /// </summary>
    /// <param name="whiteNum">素食数量。</param>
    /// <param name="meatNum">肉食数量。</param>
    /// <param name="specificWhiteDIC">指定素食数量。</param>
    /// <param name="specificMeatDIC">指定肉食数量。</param>
    public void GetFoodData(out float whiteNum, out float whiteMaxNum, out float meatNum, out float meatMaxNum,
        out Dictionary<string, float> specificWhiteDIC,
        out Dictionary<string, float> specificWhiteMaxDIC,
        out Dictionary<string, float> specificMeatDIC,
        out Dictionary<string, float> specificMeatMaxDIC)
    {
        whiteNum = 0f;
        whiteMaxNum = 0f;
        meatNum = 0f;
        meatMaxNum = 0f;
        specificWhiteDIC = new();
        specificWhiteMaxDIC = new();
        specificMeatDIC = new();
        specificMeatMaxDIC = new();
        //先统计指定食物的需求
        foreach (var item in AnimalCollection.Entities)
        {
            if (item.State == AnimalAgeStatus.AdultDeath || item.State == AnimalAgeStatus.JuvenileDeath)
            {
                continue;
            }
            if (item.data.foodType == FoodType.White)
            {
                whiteNum += item.FoodReserve;
                whiteMaxNum += item.data.foodReserve;
            }
            else if (item.data.foodType == FoodType.Meat)
            {
                meatNum += item.FoodReserve;
                meatMaxNum += item.data.foodReserve;
            }
            else if (item.data.foodType == FoodType.SpecificWhite)
            {
                whiteNum += item.FoodReserve;
                whiteMaxNum += item.data.foodReserve;
                if (!specificWhiteDIC.ContainsKey(item.data.foodParameter))
                {
                    specificWhiteDIC.Add(item.data.foodParameter, 0);
                    specificWhiteMaxDIC.Add(item.data.foodParameter, 0);
                }
                specificWhiteDIC[item.data.foodParameter] += item.FoodReserve;
                specificWhiteMaxDIC[item.data.foodParameter] += item.data.foodReserve;
            }
            else if (item.data.foodType == FoodType.SpecificMeat)
            {
                meatNum += item.FoodReserve;
                meatMaxNum += item.data.foodReserve;
                if (!specificMeatDIC.ContainsKey(item.data.foodParameter))
                {
                    specificMeatDIC.Add(item.data.foodParameter, 0);
                    specificMeatMaxDIC.Add(item.data.foodParameter, 0);
                }
                specificMeatDIC[item.data.foodParameter] += item.FoodReserve;
                specificMeatMaxDIC[item.data.foodParameter] += item.data.foodReserve;
            }
        }
    }
}
