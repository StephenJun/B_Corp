using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    public static Transform GetChildByName(this Transform trans, string name)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            if (trans.GetChild(i).name.Equals(name))
            {
                return trans.GetChild(i);
            }
        }
        return null;
    }
}
