using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public int ui_name;
    public void ShowUI()
    {

    }

    public void HideUI(Action callback=null)
    {
        callback();
    }

    public void DestroyUI()
    {

    }
}
