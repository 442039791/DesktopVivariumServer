
public class HelpData : GameConfigDataBase
{
    /// <summary>
    /// id
    /// </summary>
    public int ID;
    /// <summary>
    /// 名称
    /// </summary>
    public string Title;
    /// <summary>
    /// 描述
    /// </summary>
    public string Describe;
    /// <summary>
    /// 排序优先级
    /// </summary>
    public int Sort;
    /// <summary>
    /// 是否在图鉴中展示：0不展示、1展示
    /// </summary>
    public int Display;

    protected override string getFilePath()
    {
        return "Help.json";
    }
}
