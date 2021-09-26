using System;
using UnityEditor;

namespace Inventory
{

    // [UnityEditor.CustomEditor(typeof(CatalogSO))]
    // public class InspectorCustomizer : UnityEditor.Editor
    // {
    //     public void ShowArrayProperty(UnityEditor.SerializedProperty list)
    //     {
    //         UnityEditor.EditorGUI.indentLevel += 1;
    //         for (int i = 0; i < list.arraySize; i++)
    //         {
    //             SerializedProperty item = list.GetArrayElementAtIndex(i);
    //             SerializedProperty itemNameProperty = item.FindPropertyRelative("itemName");
    //             SerializedProperty categoryProperty = item.FindPropertyRelative("category");
    //             UnityEditor.EditorGUILayout.PropertyField(item, new UnityEngine.GUIContent($"{categoryProperty.stringValue}.{itemNameProperty.stringValue}"));
    //         }
    //         UnityEditor.EditorGUI.indentLevel -= 1;
    //     }

    //     public override void OnInspectorGUI()
    //     {
    //         ShowArrayProperty(serializedObject.FindProperty("items"));
    //         // Debug.Log("CUSTOM!");
    //     }
    // }


    // [CustomEditor(typeof(ItemSO))]
    // public class TestObjEditor : Editor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         SerializedProperty nameProperty = serializedObject.FindProperty("m_Name");
    //         // SerializedProperty descriptionProperty = serializedObject.FindProperty("m_Description");

    //         EditorGUILayout.PropertyField(nameProperty);
    //         // EditorGUILayout.PropertyField(descriptionProperty);

    //     }
    // }
}
