using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ConfigurationReaderFactory<T, Q> where Q:ReaderInterface<T>,new()
    {

        private static Dictionary<string, object> pairs;
        private static object  BuildMap(string name)
        {
            if (pairs.ContainsKey(name)) return pairs[name];
            pairs.Add(name, new Dictionary<string, ReaderInterface<T>>());
            return pairs[name];
        }

        public static T GetMap(string fileName)
        {
            var map = (Dictionary<string, ReaderInterface<T>>)BuildMap(fileName);
            if (!map.ContainsKey(fileName))
            {
                Q statesEffect = new Q();
                statesEffect.Init(fileName);
                
                map.Add(fileName, statesEffect);
            }
            return map[fileName].GetT();
        }
        static ConfigurationReaderFactory()
        {
            pairs = new Dictionary<string, object>();
            
        }
        //public static Dictionary<string, Dictionary<string, string>> GetMap(string fileName)
        //{
        //    if (!cache.ContainsKey(fileName))
        //    {
        //        cache.Add(fileName, new AIConfigurationReader(fileName));
        //    }
        //    return cache[fileName].Map;
        //}
    }

