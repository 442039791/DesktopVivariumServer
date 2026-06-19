using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatusConfigurationReader<T> : ReaderInterface<T> where T: Dictionary<string, string>
{
    private Dictionary<string, string> Map;
    public override T GetT()
    {
        return (T)Map;
    }

    public override void Init(string name)
    {
        Map = new Dictionary<string, string>();
        string configFile = ConfigurationReader.GetconfigFile(name);
        ConfigurationReader.Reader(configFile, BuildMap);
    }
    private void BuildMap(string line)
    {
        string[] keyValue = line.Split('=');
        Map.Add(keyValue[0], keyValue[1]);
    }
}
