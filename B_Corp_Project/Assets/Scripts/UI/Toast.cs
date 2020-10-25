using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LabelPool
{
    private int size;
    private GameObject labelPrefab;
    private Transform parent;
    private List<GameObject> pool;

    public LabelPool()
    {
        this.size = 1;
        this.labelPrefab = Resources.Load("Label") as GameObject;
        this.parent = GameObject.Find("Canvas").transform;
        this.pool = new List<GameObject>(this.size);
        this.AddLabels(this.size);
    }

    public LabelPool(int size)
    {
        this.size = size;
        this.labelPrefab = Resources.Load("Label") as GameObject;
        this.parent = GameObject.Find("Canvas").transform;
        this.pool = new List<GameObject>(this.size);
        this.AddLabels(this.size);
    }

    public GameObject GetLabel(string msg, Color color)
    {
        if (this.pool.Count <= 1)
        {
            this.AddLabels(1);
        }
        GameObject label = this.pool[0];
        Text text = label.GetComponent<Text>();
        text.text = msg;
        text.color = color;
        label.SetActive(true);
        label.GetComponent<MoveUp>().SetMove(true);
        this.pool.RemoveAt(0);
        return label;
    }

    public void ReturnLabel(GameObject label)
    {
        if (this.pool.Count >= this.size)
        {
            UnityEngine.Object.Destroy(label.gameObject);
            return;
        }
        label.SetActive(false);
        label.GetComponent<MoveUp>().SetMove(false);
        Text text = label.GetComponent<Text>();
        text.text = "";
        text.color = Color.white;
        text.transform.position = this.parent.position;
        this.pool.Add(label);
    }

    public void AddLabels(int num)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject label = UnityEngine.Object.Instantiate(this.labelPrefab, this.parent.position, Quaternion.identity);
            label.transform.SetParent(this.parent);
            this.ReturnLabel(label);
        }
    }
}

public class Toast
{
    private static MonoBehaviour dummyInstance = MonoSingleton<MonoBehaviour>.Instance;
    public static float duration = 3;
    private static LabelPool labelPool = new LabelPool(5);

    public static void Show(string msg)
    {
        Toast.dummyInstance.StartCoroutine(Toast.CreateLabel(msg, Color.white));
    }

    public static void Show(string msg, Color color)
    {
        Toast.dummyInstance.StartCoroutine(Toast.CreateLabel(msg, color));
    }

    public static void Warning(string msg) {
        Toast.Show(msg, Color.yellow);
    }

    public static void Error(string msg) {
        Toast.Show(msg, Color.red);
    }

    private static IEnumerator CreateLabel(string msg, Color color)
    {
        GameObject label = Toast.labelPool.GetLabel(msg, color);

        yield return new WaitForSeconds(duration);

        Toast.labelPool.ReturnLabel(label);
    }
}
