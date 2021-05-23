using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Interactable), true)]
public class InteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (!CheckForCollider2D(target as Interactable))
            EditorGUILayout.HelpBox("The Interactable component cannot function without a Collider2D", MessageType.Error);

        base.OnInspectorGUI();
    }

    protected bool CheckForCollider2D(Interactable target)
    {
        if (target.GetComponentInChildren<Collider2D>() != null)
            return true;
        else
            return false;
    }
}
