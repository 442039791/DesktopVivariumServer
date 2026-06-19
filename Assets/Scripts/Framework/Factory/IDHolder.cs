using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDHolder : MonoBehaviour {
	public int UniqueID=-1;
	private static List<IDHolder> allIDHolders = new List<IDHolder>();
	void Awake()
    {
		allIDHolders.Add(this);
    }
	public static GameObject GetGameObjectWithID(int ID)
    {
		foreach(IDHolder i in allIDHolders)
        {
			if (i.UniqueID == ID)
				return i.gameObject;
        }
		return null;
    }
	public static void ClearIDHoldersList()
    {
		allIDHolders.Clear();
    }

}
