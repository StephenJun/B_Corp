﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ShowSomething();
        }
    }

    public void ShowSomething()
    {
        Toast.Error("The button is clicked!!!");
    }
}
