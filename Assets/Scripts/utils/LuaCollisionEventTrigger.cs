using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



public class LuaCollisionEventTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other) {
        OnTriggerEnterFunc(this.gameObject, other);
    }

    void OnTriggerExit(Collider other) {
        OnTriggerExitFunc(this.gameObject, other);
    }

    void OnTriggerStay(Collider other) {
        OnTriggerStayFunc(this.gameObject, other);
    }

    void OnCollisionEnter(Collision collision) {
        OnCollisionEnterFunc(this.gameObject, collision);
    }

    void OnCollisionExit(Collision collision)
    {
        OnCollisionExitFunc(this.gameObject, collision);
    }

    void OnCollisionStay(Collision collision)
    {
        OnCollisionStayFunc(this.gameObject, collision);
    }

    public static void EnableCollisionListener(GameObject obj) {
        obj.AddComponent<LuaCollisionEventTrigger>();
    }

    public static Action<GameObject, Collision> OnCollisionEnterFunc;
    public static Action<GameObject, Collision> OnCollisionStayFunc;
    public static Action<GameObject, Collision> OnCollisionExitFunc;

    public static Action<GameObject, Collider> OnTriggerEnterFunc;
    public static Action<GameObject, Collider> OnTriggerStayFunc;
    public static Action<GameObject, Collider> OnTriggerExitFunc;
    
}
