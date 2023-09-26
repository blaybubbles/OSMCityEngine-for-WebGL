using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    /// <summary>
    /// The static reference to the instance
    /// </summary>
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>(true);
            }

            if (instance == null && typeof(T).GetInterface(nameof(ICreateMe)) != null)
            {
                var go = new GameObject(typeof(T).Name, typeof(T));
                instance = go.GetComponent<T>();
            }
            return instance;
        }
    }

    /// <summary>
    /// Gets whether an instance of this singleton exists
    /// </summary>
    public static bool instanceExists
    {
        get { return instance != null; }
    }

    /// <summary>
    /// Awake method to associate singleton with instance
    /// </summary>
    protected virtual void Awake()
    {
        if (instanceExists && instance != this)
        {
            Debug.Log("Delete " + gameObject.name);
            Destroy(gameObject);
        }
        else
        {
            instance = (T)this;
            if (typeof(T).GetInterface(nameof(IDontDestroyed)) != null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    /// <summary>
    /// OnDestroy method to clear singleton association
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

}

internal interface ICreateMe
{
}

public interface IDontDestroyed
{

}