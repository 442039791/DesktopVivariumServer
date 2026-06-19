
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Common
{
    public interface IResetable
    {
        void OnReset();
    }
public class GameObjectPool : MonoSingleton<GameObjectPool>
{
    private Dictionary<string, List<GameObject>> cache;
    public override void Init()
    {
        base.Init();
        cache = new Dictionary<string, List<GameObject>>();
    }
    public GameObject CreateObject(string key, Vector3 pos = default, Vector3 rotate = default, Transform parent = default)
    {
        return CreateObject(key, ResourceManager.Load<GameObject>(key), pos, rotate, parent);
    }
    public GameObject CreateObject(string key,GameObject prefab,Vector3 pos=default,Vector3 rotate=default,Transform parent=default)
    {

        GameObject go = FindUsableObject(key);
        if (cache.ContainsKey(key))
            go = cache[key].Find(g => !g.activeInHierarchy);
        if (go == null)
        {
            go = Instantiate(prefab);
            if (!cache.ContainsKey(key))
                cache.Add(key, new List<GameObject>());
            cache[key].Add(go);
        }
            //var scale = go.transform.localScale;
        //TODO:
            go.transform.SetParent(null, false);
        //go.transform.SetToParentWithSameGlobalScale(GlobalSetting.Instance.BattleSence);
        //go.transform.SetGlobalScale(scale);
        if (parent!=default)
                go.transform.SetToParentWithSameGlobalScale(parent);
        if(pos ==default&&rotate==default)
        {
                go.SetActive(true);
                go.transform.localScale = Vector3.one;
                return go;
        }
        else
        {
           UseObject(pos, rotate, go);
        }
            go.transform.localScale = Vector3.one;
            return go;
    }

    private static void UseObject(Vector3 pos, Vector3 rotate, GameObject go)
    {
        
        go.transform.position = new Vector3(pos.x, pos.y, 0);
        go.transform.eulerAngles = rotate;
        go.SetActive(true);
        foreach (var item in go.GetComponents<IResetable>())
        {
                item.OnReset();
        }
    }

    private GameObject FindUsableObject(string key)
    {
        if (cache.ContainsKey(key))
            return  cache[key].Find(g => !g.activeInHierarchy);
        return null;
         
    }
    public void CollectObject(GameObject go,float delay=0)
    {
        if (delay!=0)
        {
            StartCoroutine(CollectObjectDelay(go, delay));
        }
        else
        {
            go.SetActive(false);
        }
        
    }
    private IEnumerator CollectObjectDelay(GameObject go,float delay)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(false);
    }
    public void Clear(string key)
    {
        foreach (var item in cache[key])
        {
            Destroy(item);
        }
        //for (int i=0;i<cache[key].Count;i++)
        //{
        //    Destroy(cache[key][i]);
        //}
        cache.Remove(key);
    }
    public void ClearAll()
    {
        List<string> keyList = new List<string>(cache.Keys);
        foreach (var key in keyList)
        {
            Clear(key);
        }
    }
}
}

