using UnityEngine;

/// <summary>
/// 语言配置数据类 - 对应 languageData.csv
/// 每条记录定义一种语言及其对应的字体资源
/// </summary>
public partial class LanguageConfigData : GameConfigDataBase
{
    public int ID; // 语言ID
    public string Lauguage; // 语言文件名（如 CN, English）
    public string Fonts; // 对应的SDF字体资源名（不含路径和扩展名）

    protected override string getFilePath()
    {
        return "languageData.json";
    }
}
