using System.Collections.Generic;

public  class CustomClassSorter
{

    public CustomClassSorter()
    {
        if (Order == null)
            Order = new Dictionary<AnimalAgeStatus, int>()
            {
                { AnimalAgeStatus.Adult, 3 },
                {AnimalAgeStatus.AdultDeath,1 },
                { AnimalAgeStatus.Juvenile, 4 },
                { AnimalAgeStatus.JuvenileDeath, 2 },
            };
    }
    public static Dictionary<AnimalAgeStatus, int> Order;

    public int Compare(AnimalAgeStatus a, AnimalAgeStatus b)
    {
        // 如果字典中没有对应的值，给一个默认低优先级
        int orderA = Order.TryGetValue(a, out int valA) ? valA : int.MaxValue;
        int orderB = Order.TryGetValue(b, out int valB) ? valB : int.MaxValue;

        return orderA.CompareTo(orderB);
    }
    public int Compare(AnimalData a, AnimalData b)
    {
        return a.Sort.CompareTo(b.Sort);
    }
}
public enum AnimalSortType
{
    Default,
}
public enum PlantSortType
{
    Default,
}