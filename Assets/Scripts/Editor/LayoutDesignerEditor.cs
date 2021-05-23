using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayoutDesigner))]
public class LayoutDesignerEditor : Editor
{
    SerializedObject so;
    SerializedProperty _exposedComponents;
    SerializedProperty _horizontalAlignment, _verticalAlignment;
    SerializedProperty _widthRatio, _heightRatio;
    SerializedProperty _hasBackground, _backgroundColor, _backgroundMaterial;
    SerializedProperty _spacing, _scaling;
    SerializedProperty _overrideVerticalAlignment;
    SerializedProperty _childVerticalDisplacement;
    SerializedProperty _childAlignPercent, _childScaling;

    int childCount = 0;
    GameObject objectToExpose;
    Component componentToExpose;
    int selectedComponentIndex;
    LayoutDesigner designer;

    private void OnEnable()
    {
        so = serializedObject;

        _exposedComponents = so.FindProperty("exposedComponents");
        _horizontalAlignment = so.FindProperty("horizontalAlignment");
        _verticalAlignment = so.FindProperty("verticalAlignment");
        _widthRatio = so.FindProperty("widthRatio");
        _heightRatio = so.FindProperty("heightRatio");
        _hasBackground = so.FindProperty("hasBackground");
        _backgroundColor = so.FindProperty("backgroundColor");
        _backgroundMaterial = so.FindProperty("backgroundMaterial");
        _scaling = so.FindProperty("scaling");
        _spacing = so.FindProperty("spacing");
        _childAlignPercent = so.FindProperty("childAlignPercent");
        _childVerticalDisplacement = so.FindProperty("childVerticalDisplacement");
        _overrideVerticalAlignment = so.FindProperty("overrideVerticalAlignment");
        _childScaling = so.FindProperty("childScaling");

        designer = target as LayoutDesigner;
        childCount = Application.isPlaying && _hasBackground.boolValue ? designer.ChildCount - 1: designer.ChildCount;
        if(_childAlignPercent == null || _childAlignPercent.arraySize != childCount)
            designer.InitChildAlignmentArray(childCount);
        if (_childScaling == null || _childScaling.arraySize != childCount)
            designer.InitChildScalingArray(childCount);


        // due to not having written custom serialization for the exposed components class
        // if a child of the object, which currently has an exposed component in the list is removed
        // the inspector for the object will crash
        //if (_exposedComponents != null)
        //{
        //    int size = _exposedComponents.arraySize;

        //    if (size > 0)
        //    {
        //        for (int i = 0; i < size; i++)
        //        {
        //            var element = _exposedComponents.GetArrayElementAtIndex(i);

        //            if (element.FindPropertyRelative("gameObject").objectReferenceValue == null
        //                && element.FindPropertyRelative("component").objectReferenceValue == null)
        //            {
        //                element.DeleteArrayElementAtIndex(i);
        //            }

        //            so.ApplyModifiedProperties();
        //        }
        //    }
        //}
    }

    public override void OnInspectorGUI()
    {
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

        GUILayout.Label("Expose Components on Children", title);
        GuiLine();

        using(new GUILayout.HorizontalScope())
        {
            objectToExpose = EditorGUILayout.ObjectField(objectToExpose, typeof(GameObject), true, GUILayout.Width(120)) as GameObject;

            if(objectToExpose != null)
            {
                var components = GetAllComponentsOnGameObject(objectToExpose);
                var componentNames = components.Select(x => x.GetType().Name).ToArray();
                selectedComponentIndex = EditorGUILayout.Popup(selectedComponentIndex, componentNames);
                componentToExpose = components[selectedComponentIndex];
            }
        }
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add") && objectToExpose != null)
            {
                _exposedComponents.arraySize++;
                var element = _exposedComponents.GetArrayElementAtIndex(_exposedComponents.arraySize - 1);
                element.FindPropertyRelative("gameObject").objectReferenceValue = objectToExpose;
                element.FindPropertyRelative("component").objectReferenceValue = componentToExpose;

                selectedComponentIndex = 0;
            }
        }

        if (designer.exposedComponents != null && designer.exposedComponents.Count > 0)
        {
            GUILayout.Label("Exposed GO-Component Pairs", subtitle);
            GuiLine();

            var temp = new List<ExposedGOComponentPair>();

            foreach (var pair in designer.exposedComponents)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label($"{pair.gameObject.name} => {pair.component.GetType().Name}", subtext, GUILayout.Width(200));
                    if (GUILayout.Button("-"))
                    {
                        temp.Add(pair);
                    }
                }
            }

            foreach (var pair in temp)
            {
                var index = designer.exposedComponents.IndexOf(pair);

                // the deleted element will be completely removed from list
                // when modified properties are applied and the SO updates
                _exposedComponents.DeleteArrayElementAtIndex(index);
            }
        }

        GUILayout.Space(5);

        GUILayout.Label("Alignment", title);
        GuiLine();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Override Vertical:", subtitle, GUILayout.Width(150));
            EditorGUILayout.PropertyField(_overrideVerticalAlignment, GUIContent.none, GUILayout.Width(80));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Horizontal:", subtitle, GUILayout.Width(65));
            EditorGUILayout.PropertyField(_horizontalAlignment, GUIContent.none, GUILayout.Width(80));

            if (!_overrideVerticalAlignment.boolValue)
            {
                GUILayout.Label("Vertical:", subtitle, GUILayout.Width(50));
                EditorGUILayout.PropertyField(_verticalAlignment, GUIContent.none, GUILayout.Width(80));
            }
        }

        GUILayout.Space(5);

        GUILayout.Label("Ratios", title);
        GuiLine();

        EditorGUILayout.HelpBox("Ratios are calculated against screen dimensions", MessageType.Info);
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Width ratio:", subtitle, GUILayout.Width(100));
            EditorGUILayout.Slider(_widthRatio, 0f, 1f, GUIContent.none);
        }
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Height ratio:", subtitle, GUILayout.Width(100));
            EditorGUILayout.Slider(_heightRatio, 0f, 1f, GUIContent.none);
        }

        GUILayout.Space(5);

        GUILayout.Label("Background", title);
        GuiLine();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Create Background:", subtitle, GUILayout.Width(150));
            EditorGUILayout.PropertyField(_hasBackground, GUIContent.none, GUILayout.Width(80));
        }

        if (_hasBackground.boolValue)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Background Color:", subtitle, GUILayout.Width(150));
                EditorGUILayout.PropertyField(_backgroundColor, GUIContent.none, GUILayout.Width(80));
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Background Material:", subtitle, GUILayout.Width(150));
                EditorGUILayout.PropertyField(_backgroundMaterial, GUIContent.none, GUILayout.Width(80));
            }
        }

        GUILayout.Space(5);

        GUILayout.Label("Child Vertical Displacement", title);
        GuiLine();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Displacement Ratio:", subtitle, GUILayout.Width(150));
            EditorGUILayout.Slider(_childVerticalDisplacement, -1f, 1f, GUIContent.none);
        }

        GUILayout.Space(5);

        if (childCount == 0)
            EditorGUILayout.HelpBox("Designer has no children to align or scale", MessageType.Warning);

        GUILayout.Label("Scaling", title);
        GuiLine();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Scaling:", subtitle, GUILayout.Width(65));
            EditorGUILayout.PropertyField(_scaling, GUIContent.none, GUILayout.Width(80));
        }

        bool isCustomScaling = _scaling.enumValueIndex == (int)LayoutScaling.Custom;

        if(isCustomScaling && childCount > 0)
        {
            for(int i = 0; i < childCount; i++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label($"{i + 1}:", subtitle, GUILayout.Width(10));
                    EditorGUILayout.PropertyField(_childScaling.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.Width(10));
                }
            }
        }

        GUILayout.Space(5);

        GUILayout.Label("Spacing", title);
        GuiLine();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Spacing:", subtitle, GUILayout.Width(65));
            EditorGUILayout.PropertyField(_spacing, GUIContent.none, GUILayout.Width(80));
        }

        bool isCustomSpacing = _spacing.enumValueIndex == (int)LayoutSpacing.Custom;

        if (isCustomSpacing && childCount > 0)
        {
            int i = 0;

            for (; i < childCount; i++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label($"Child {i + 1}:", subtitle, GUILayout.Width(100));
                    EditorGUILayout.Slider(_childAlignPercent.GetArrayElementAtIndex(i), 0f, 1f, GUIContent.none);
                }
            }

            i = 0;
            float total = 0;
            for (; i < _childAlignPercent.arraySize; i++)
                total += _childAlignPercent.GetArrayElementAtIndex(i).floatValue;

            if ( total > 1f)
                EditorGUILayout.HelpBox("Percentages add up to more than 1", MessageType.Warning);
        }

        so.ApplyModifiedProperties();
    }

    Component[] GetAllComponentsOnGameObject(GameObject go)
    {
        return go.gameObject.GetComponents(typeof(Component));
    }

    void GuiLine(int i_height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}
