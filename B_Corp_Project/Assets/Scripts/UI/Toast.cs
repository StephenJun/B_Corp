using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabelPool
{
    private Text labelPrefab;
    private Transform parent;
    private List<Text> pool;

    public LabelPool(Text labelPrefab, Transform parent)
    {
        this.labelPrefab = labelPrefab;
        this.parent = parent;
        this.pool = new List<Text>(5);
        this.AddLabels(5);
    }

    public LabelPool(Text labelPrefab, Transform parent, int size)
    {
        this.labelPrefab = labelPrefab;
        this.parent = parent;
        this.pool = new List<Text>(size);
        this.AddLabels(size);
    }

    public Text GetLabel(string msg, Color color)
    {
        if (this.pool.Count <= 1)
        {
            this.AddLabels(1);
        }
        Text label = this.pool[0];
        label.text = msg;
        label.color = color;
        label.enabled = true;
        this.pool.RemoveAt(0);
        return label;
    }

    public void ReturnLabel(Text label)
    {
        label.enabled = false;
        label.text = "";
        label.color = Color.white;
        label.transform.position = this.parent.position;
        this.pool.Add(label);
    }

    public void AddLabels(int num)
    {
        for (int i = 0; i < num; i++)
        {
            Text label = Object.Instantiate(this.labelPrefab, this.parent.position, Quaternion.identity);
            label.transform.SetParent(this.parent);
            label.enabled = false;
            this.pool.Add(label);
        }
    }
}

public class Toast : MonoBehaviour
{
    public float duration;
    public Text labelPrefab;
    private LabelPool labelPool;

    // Start is called before the first frame update
    void Start()
    {
        this.labelPool = new LabelPool(this.labelPrefab, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Show(string msg)
    {
        this.StartCoroutine(this.CreateLabel(msg, Color.white));
    }

    public void Show(string msg, Color color)
    {
        this.StartCoroutine(this.CreateLabel(msg, color));
    }

    public void Warning(string msg) {
        this.Show(msg, Color.yellow);
    }

    public void Error(string msg) {
        this.Show(msg, Color.red);
    }

    private IEnumerator CreateLabel(string msg, Color color)
    {
        Text label = this.labelPool.GetLabel(msg, color);
        label.GetComponent<MoveUp>().SetMove(true);
        print(label.GetComponent<MoveUp>().GetMove());

        yield return new WaitForSeconds(3);

        label.GetComponent<MoveUp>().SetMove(false);
        this.labelPool.ReturnLabel(label);
    }
}
