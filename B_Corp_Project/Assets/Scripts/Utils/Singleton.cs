using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _Instance; //实例  单例模式：单个实例
    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<T>(); //在当前场景中找到T类型的对象，并返回第一个
                if (_Instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name, typeof(T));
                    _Instance = go.GetComponent<T>();
                }
            }
            DontDestroyOnLoad(_Instance.transform.root.gameObject);
            return _Instance;
        }
    }

    protected virtual void Awake()
    {
        if (_Instance != null)
        {
            Destroy(this.transform.root.gameObject);
        }
    }

}

public class Singleton<T> where T : new()
{
    private static T _Instance; //实例  单例模式：单个实例
    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new T(); //调用构造函数
            }
            return _Instance;
        }
    }
}
