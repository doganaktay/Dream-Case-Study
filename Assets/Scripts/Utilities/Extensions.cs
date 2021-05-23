using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class Extensions
{
    public static void ClearChildren(this GameObject parent, int index = 0)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException();

        for(; index < parent.transform.childCount; index++)
        {
            var t = parent.transform.GetChild(index);
            UnityEngine.Object.Destroy(t.gameObject);
        }
    }

    public static void Update(this TextMeshPro slot, string text)
    {
        slot.text = text;
    }
}
