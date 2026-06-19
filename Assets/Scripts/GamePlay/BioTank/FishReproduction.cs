using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 生态缸管理器
/// </summary>
public partial class BioTankManager
{
    /// <summary>
    /// 动物受孕
    /// </summary>
    public void AnimalConception(float IntervalTime)
    {
        if (GetUseVolume() >= Volume)
        {
            return;
        }
        //各种动物的列表
        Dictionary<string, List<Animal>> maleAnimalDic = new();
        //各种动物的累计进度
        Dictionary<string, float> animalReproductionDic = new();
        List<AnimalStateData> _sds = new();
        // 使用快照避免在遍历期间修改集合导致异常
        var animalSnapshot = AnimalCollection.Entities;
        foreach (var item in animalSnapshot)
        {
            if (item.State != AnimalAgeStatus.Adult)
            {
                continue;
            }
            // 健康值不满时暂停繁衍
            if (item.HP < 100)
            {
                continue;
            }
            if (!maleAnimalDic.TryGetValue(item.data.ID, out var ints))
            {
                ints = new();
                maleAnimalDic.Add(item.data.ID, ints);
                animalReproductionDic.Add(item.data.ID, 0);
            }
            ints.Add(item);
            item.ReproductionProgress += IntervalTime;
            animalReproductionDic[item.data.ID] += item.ReproductionProgress;
            //进行产仔
            if (animalReproductionDic[item.data.ID] >= item.data.reproductionTime)
            {

                if (GetUseVolume() >= Volume)
                {
                    return;
                }
                float progress = item.data.reproductionTime;
                //首先扣除受孕进度
                foreach (var item2 in maleAnimalDic[item.data.ID])
                {
                    if (item2.ReproductionProgress >= progress)
                    {
                        item2.ReproductionProgress -= progress;
                        break;
                    }
                    else
                    {
                        progress -= item2.ReproductionProgress;
                        item2.ReproductionProgress = 0;
                    }
                }

                //随机两个动物作为这个动物的父亲跟母亲
                var father = maleAnimalDic[item.data.ID][Random.Range(0, maleAnimalDic[item.data.ID].Count)];
                var mother= maleAnimalDic[item.data.ID][Random.Range(0, maleAnimalDic[item.data.ID].Count)];
                float quality = (father.Quality + mother.Quality) / 2 + Random.Range(-0.2f, 0.2f);
                if (quality < 0)
                {
                    quality = 0;
                }
                else if (quality > 4)
                {
                    quality = 4;
                }

                //整理父母拥有的特质
                List<Feature> featureList = new();
                //foreach (var feature in father.FeatureList)
                //{
                //    featureList.Add(feature);
                //}
                //foreach (var motherFeature in mother.FeatureList)
                //{
                //    bool has=false;
                //    for (int i = 0;i< featureList.Count;i++)
                //    {
                //        if (featureList[i].ID== motherFeature.ID)
                //        {
                //            int strength = (featureList[i].Strength + motherFeature.Strength) / 2 + Random.Range(-20, 20);
                //            if (strength < 0)
                //            {
                //                strength = 0;
                //            }
                //            else if (strength>100)
                //            {
                //                strength = 100;
                //            }
                //            Feature feature = new()
                //            {
                //                ID = featureList[i].ID,
                //                Strength = strength,
                //            };
                //            featureList[i] = feature;
                //            has =true;
                //            break;
                //        }
                //    }
                //    if (!has)
                //    {
                //        featureList.Add(motherFeature);
                //    }
                //}


                AnimalStateData animal = new()
                {
                    ID = stateData.ID, // 设置生态箱ID，确保新生动物属于当前生态箱
                    Type = item.data.ID,
                    Quality = quality,
                    FeatureList = featureList,
                    ReproductionProgress = 0,
                    HP = 100,
                    GrowUpNum = 0,
                    FoodReserve = item.data.foodReserve * 0.2f,
                    ObjID = IDFactory.GetUniqueID(),
                    State= AnimalAgeStatus.Juvenile,
                };
                _sds.Add(animal);
            }
        }
        foreach (var item in _sds)
        {
            PlayerManager.Instance.CreateAnimal(item);
        }
        foreach (var item in maleAnimalDic)
        {
            if (item.Value.Count < 2)
            {
                item.Value[0].ReproductionProgress -= IntervalTime;
            }
        }
    }
}
