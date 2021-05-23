using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Button), true)]
public class ButtonEditor : InteractableEditor
{
    SerializedObject so;
    SerializedProperty _executionType;
    SerializedProperty _sceneToUnload, _sceneToLoad;
    SerializedProperty _levelToLoad;
    SerializedProperty _upColor, _downColor, _disabledColor;

    private void OnEnable()
    {
        so = serializedObject;
        _executionType = so.FindProperty("executionType");
        _sceneToUnload = so.FindProperty("sceneToUnload");
        _sceneToLoad = so.FindProperty("sceneToLoad");
        _levelToLoad = so.FindProperty("levelToLoad");
        _upColor = so.FindProperty("upColor");
        _downColor = so.FindProperty("downColor");
        _disabledColor = so.FindProperty("disabledColor");
    }

    public override void OnInspectorGUI()
    {
        if (!CheckForCollider2D(target as Interactable))
            EditorGUILayout.HelpBox("The Interactable component cannot function without a Collider2D", MessageType.Error);

        var title = new GUIStyle(GUI.skin.GetStyle("Label"));
        title.alignment = TextAnchor.LowerLeft;
        title.fontStyle = FontStyle.Bold;
        title.fontSize = 12;

        var subtitle = new GUIStyle(GUI.skin.GetStyle("Label"));
        subtitle.alignment = TextAnchor.LowerLeft;
        subtitle.fontStyle = FontStyle.Normal;
        subtitle.fontSize = 12;

        var subtext = new GUIStyle(GUI.skin.GetStyle("Label"));
        subtext.alignment = TextAnchor.LowerLeft;
        subtext.fontStyle = FontStyle.Normal;
        subtext.fontSize = 10;

        so.Update();

        GUILayout.Label("Execution type:", title);
        GuiLine();
                    
        EditorGUILayout.PropertyField(_executionType, GUIContent.none);

        switch (_executionType.enumValueIndex)
        {
            case (int)ButtonExecutionType.LoadScene:
                using(new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Scene =>", subtitle, GUILayout.Width(60));
                    GUILayout.Label("To Unload:", subtitle, GUILayout.Width(75));
                    EditorGUILayout.PropertyField(_sceneToUnload, GUIContent.none, GUILayout.Width(40));
                    GUILayout.Label("To Load:", subtitle, GUILayout.Width(75));
                    EditorGUILayout.PropertyField(_sceneToLoad, GUIContent.none, GUILayout.Width(40));
                }
                break;

            default:
                break;
        }

        GUILayout.Space(5);
        GUILayout.Label("Colors", title);
        GuiLine();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Up:", subtitle, GUILayout.Width(60));
            EditorGUILayout.PropertyField(_upColor, GUIContent.none);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Down:", subtitle, GUILayout.Width(60));
            EditorGUILayout.PropertyField(_downColor, GUIContent.none);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Disabled:", subtitle, GUILayout.Width(60));
            EditorGUILayout.PropertyField(_disabledColor, GUIContent.none);
        }


        so.ApplyModifiedProperties();

        //base.OnInspectorGUI();
    }

    void GuiLine(int i_height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}
