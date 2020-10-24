using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUp : MonoBehaviour
{
    private bool move;

    // Start is called before the first frame update
    void Start()
    {
        this.move = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.move)
        {
            this.transform.Translate(Vector3.up * 0.2f);
        }
    }

    public bool GetMove()
    {
        return this.move;
    }

    public void SetMove(bool move)
    {
        this.move = move;
    }
}
