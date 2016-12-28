using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _me;

    public static T Me
    {
        get
        {
            return _me;
        }
    }

    public virtual void Awake()
    {
        if (_me != null && _me != this)
        {
            Destroy(gameObject);
        }
        _me = (T) this;
    }
}
