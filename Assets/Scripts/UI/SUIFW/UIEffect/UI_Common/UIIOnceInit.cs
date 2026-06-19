using UnityEngine;

public class UIIOnceInit : MonoBehaviour
{
    public bool Inited = false;
    public virtual void Init()
    {
        if (Inited) return;
        Inited = true;
    }
}
