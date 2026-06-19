using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameConfigDataBase
{
    protected virtual string getFilePath()
    {
        return "";
    }

    static Dictionary<string, Dictionary<string, GameConfigDataBase>> dataDic = new Dictionary<string, Dictionary<string, GameConfigDataBase>>();

    public static T GetConfigData<T>(int key, string fileName = null) where T : GameConfigDataBase
    {
        Type setT = typeof(T);
        if (fileName == null)
        {
            T obj = Activator.CreateInstance<T>();
            fileName = obj.getFilePath();
        }
        string _key = key.ToString();
        if (!dataDic.ContainsKey(fileName))
        {
            ReadConfigData<T>(fileName);
        }
        Dictionary<string, GameConfigDataBase> objDic = dataDic[fileName];
        if (!objDic.ContainsKey(_key))
        {
            Debug.LogError("Can not find config data with key: " + _key);
            return null;
        }
        return (T)(objDic[_key]);
    }

    /// <summary>
    /// 检查指定key的配置是否存在（不输出错误日志）
    /// 用于在加载存档时检测孤儿实体（模组卸载后遗留的动植物）
    /// </summary>
    public static bool HasConfigData<T>(int key, string fileName = null) where T : GameConfigDataBase
    {
        if (fileName == null)
        {
            T obj = Activator.CreateInstance<T>();
            fileName = obj.getFilePath();
        }
        string _key = key.ToString();
        if (!dataDic.ContainsKey(fileName))
        {
            ReadConfigData<T>(fileName);
        }
        if (!dataDic.TryGetValue(fileName, out var objDic))
        {
            return false;
        }
        return objDic.ContainsKey(_key);
    }

    /// <summary>
    /// 检查指定key的配置是否存在（字符串key版本）
    /// </summary>
    public static bool HasConfigData<T>(string key, string fileName = null) where T : GameConfigDataBase
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }
        if (!int.TryParse(key, out int intKey))
        {
            return false;
        }
        return HasConfigData<T>(intKey, fileName);
    }

    public static List<T> GetConfigDatas<T>(string fileName = null) where T : GameConfigDataBase
    {
        List<T> returnList = new List<T>();
        Type setT = typeof(T);
        if (fileName == null)
        {
            fileName = setT.Name;
        }

        if (!dataDic.ContainsKey(fileName))
        {
            ReadConfigData<T>(fileName);
        }
        Dictionary<string, GameConfigDataBase> objDic = dataDic[fileName];
        foreach (KeyValuePair<string, GameConfigDataBase> kvp in objDic)
        {
            returnList.Add((T)(kvp.Value));
        }
        return returnList;
    }

    public static Dictionary<string, T> GetConfigDatasByDic<T>(string fileName = null) where T : GameConfigDataBase
    {
        Dictionary<string, T> returnList = new Dictionary<string, T>();
        Type setT = typeof(T);
        if (fileName == null)
        {
            fileName = setT.Name + ".json";
        }

        if (!dataDic.ContainsKey(fileName))
        {
            ReadConfigData<T>(fileName);
        }
        Dictionary<string, GameConfigDataBase> objDic = dataDic[fileName];
        foreach (KeyValuePair<string, GameConfigDataBase> kvp in objDic)
        {
            returnList.Add(kvp.Key, (T)(kvp.Value));
        }
        return returnList;
    }

    public static void ReadConfigData<T>(string fileName = null) where T : GameConfigDataBase
    {
        T obj = Activator.CreateInstance<T>();
        if (fileName == null || fileName == "null")
        {
            fileName = obj.getFilePath();
        }

        string jsonString = ResMgr.Instance.GetConfigFile(fileName);

        if (string.IsNullOrEmpty(jsonString))
        {
            Debug.LogError($"[GameConfigDataBase] 配置文件读取失败或为空: {fileName}");
            dataDic.Add(fileName, new Dictionary<string, GameConfigDataBase>());
            return;
        }

        Dictionary<string, GameConfigDataBase> objDic = new Dictionary<string, GameConfigDataBase>();

        try
        {
            JObject jsonData = JObject.Parse(jsonString);
            JArray fieldsArray = (JArray)jsonData["fields"];
            JArray dataArray = (JArray)jsonData["data"];

            if (fieldsArray == null || dataArray == null)
            {
                Debug.LogError($"[GameConfigDataBase] JSON格式错误，缺少fields或data字段: {fileName}");
                dataDic.Add(fileName, new Dictionary<string, GameConfigDataBase>());
                return;
            }

            List<string> fieldNames = new List<string>();
            foreach (var field in fieldsArray)
            {
                fieldNames.Add(field.ToString());
            }

            FieldInfo[] fis = new FieldInfo[fieldNames.Count];
            for (int i = 0; i < fieldNames.Count; i++)
            {
                string fieldName = fieldNames[i];
                if (!string.IsNullOrEmpty(fieldName))
                {
                    fis[i] = typeof(T).GetField(fieldName);
                }
            }

            foreach (JObject rowData in dataArray)
            {
                T configObj = Activator.CreateInstance<T>();

                for (int i = 0; i < fieldNames.Count; i++)
                {
                    string fieldName = fieldNames[i];
                    if (string.IsNullOrEmpty(fieldName) || fis[i] == null)
                    {
                        continue;
                    }

                    JToken valueToken = rowData[fieldName];
                    if (valueToken == null)
                    {
                        continue;
                    }

                    object setValue = null;
                    string fieldTypeName = fis[i].FieldType.ToString();

                    try
                    {
                        switch (fieldTypeName)
                        {
                            case "System.Int32":
                                setValue = valueToken.Type == JTokenType.Null ? 0 : valueToken.ToObject<int>();
                                break;
                            case "System.Int64":
                                setValue = valueToken.Type == JTokenType.Null ? 0L : valueToken.ToObject<long>();
                                break;
                            case "System.String":
                                setValue = valueToken.Type == JTokenType.Null ? "" : valueToken.ToString();
                                break;
                            case "System.Single":
                                setValue = valueToken.Type == JTokenType.Null ? 0f : valueToken.ToObject<float>();
                                break;
                            case "System.Double":
                                setValue = valueToken.Type == JTokenType.Null ? 0d : valueToken.ToObject<double>();
                                break;
                            case "System.Boolean":
                                setValue = valueToken.Type == JTokenType.Null ? false : valueToken.ToObject<bool>();
                                break;
                            case "System.Collections.Generic.List`1[System.String]":
                                if (valueToken.Type == JTokenType.Array)
                                {
                                    setValue = valueToken.ToObject<List<string>>();
                                }
                                else if (valueToken.Type == JTokenType.String)
                                {
                                    string strValue = valueToken.ToString();
                                    if (string.IsNullOrEmpty(strValue) || strValue == "[]")
                                    {
                                        setValue = new List<string>();
                                    }
                                    else
                                    {
                                        if (strValue.StartsWith("[") && strValue.EndsWith("]"))
                                        {
                                            strValue = strValue.Substring(1, strValue.Length - 2);
                                        }
                                        if (string.IsNullOrEmpty(strValue))
                                        {
                                            setValue = new List<string>();
                                        }
                                        else
                                        {
                                            setValue = new List<string>(strValue.Split(','));
                                        }
                                    }
                                }
                                else
                                {
                                    setValue = new List<string>();
                                }
                                break;
                            default:
                                if (fis[i].FieldType.IsEnum)
                                {
                                    try
                                    {
                                        int enumValue = valueToken.Type == JTokenType.Null ? 0 : valueToken.ToObject<int>();
                                        setValue = Enum.ToObject(fis[i].FieldType, enumValue);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError($"Error parsing enum for field {fis[i].Name} with value {valueToken}: {e.Message}");
                                        setValue = Enum.ToObject(fis[i].FieldType, 0);
                                    }
                                }
                                break;
                        }

                        if (setValue != null)
                        {
                            fis[i].SetValue(configObj, setValue);
                        }

                        if (fis[i].Name == "key" || fis[i].Name == "ID")
                        {
                            objDic.Add(setValue.ToString(), configObj);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[GameConfigDataBase] 解析字段失败 - 文件: {fileName}, 字段: {fieldName}, 值: {valueToken}, 类型: {fieldTypeName}, 错误: {e.Message}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameConfigDataBase] JSON解析失败: {fileName}, 错误: {e.Message}");
            dataDic.Add(fileName, new Dictionary<string, GameConfigDataBase>());
            return;
        }

        dataDic.Add(fileName, objDic);
    }
}
