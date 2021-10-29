using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using Inventory;


// [CustomPropertyDrawer(typeof(ItemData))]
public class ItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // label = EditorGUI.BeginProperty(position, label, property);
        // Rect contentPosition = EditorGUI.PrefixLabel(position, label);
        // EditorGUI.indentLevel = 0;
        // Weird hack to stop null error for serialized property
        SerializedObject obj = new SerializedObject(property.objectReferenceValue);
        EditorGUI.ObjectField(position, obj.FindProperty("sprite"), GUIContent.none);
        // EditorGUI.PropertyField(position, obj.FindProperty("texture"), GUIContent.none);
        // EditorGUI.EndProperty();

    }
    // public override VisualElement CreatePropertyGUI(SerializedProperty property)
    // {
    //     // Create property container element.
    //     var container = new VisualElement();

    //     // Create property fields.
    //     var textureField = new PropertyField(property.FindPropertyRelative("texture"));

    //     // Add fields to the container.
    //     container.Add(textureField);
    //     return container;
    // }
}

// [CustomEditor(typeof(ItemData))]
// public class ItemDataEditor : Editor
// {
//     ItemData itemData;

//     public void OnEnable()
//     {
//         itemData = (ItemData)target;
//     }

//     public override void OnInspectorGUI()
//     {
//         itemData.texture = (Texture2D)EditorGUILayout.ObjectField(itemData.texture, typeof(Texture2D), false, GUILayout.Width(65f), GUILayout.Height(65f));
//         base.DrawDefaultInspector();
//     }
// }

// [CustomPropertyDrawer(typeof(ItemData))]
// public class ItemDataPropertyDrawer : PropertyDrawer
// {
//     ItemData itemData;

//     public void OnEnable()
//     {
//         itemData = (ItemData)target;
//     }

//     public override void OnGUI()
//     {
//         itemData.texture = (Texture2D)EditorGUILayout.ObjectField(itemData.texture, typeof(Texture2D), false, GUILayout.Width(65f), GUILayout.Height(65f));
//         base.DrawDefaultInspector();
//     }
// }