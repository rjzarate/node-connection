using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
    where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameObject = new()
                {
                    name = typeof(T).Name,
                    hideFlags = HideFlags.HideAndDontSave,
                };
                _instance = gameObject.AddComponent<T>();
            }
            return _instance;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
