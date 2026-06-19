using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IDFactory  {
	public static int Count=0;
	public static int GetUniqueID()
    {
        Count++;
		return Count;
    }
	public static void ResetIDS()
    {
		Count = 0;
    }
}
