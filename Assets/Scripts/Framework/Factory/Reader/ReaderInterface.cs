using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReaderInterface<T> 
{
    public abstract  T GetT();
    public abstract void Init(string name);


}
