// using UnityEditor;
// using UnityEngine;
// using System.Linq;
// using System.Collections.Generic;

// using Inventory;

// // Can use [MenuItem()] and

// [CustomEditor(typeof(Catalog))]
// public class CatalogEditor : Editor
// {
//     // TODO: Look at AssetDatabase for fetching available items / setting icons

//     Catalog catalog;
//     bool consumableFilter = false;
//     int numColumns = 3;
//     float textureSize = 65f;

//     public void OnEnable()
//     {
//         catalog = (Catalog)target;
//     }

//     public override void OnInspectorGUI()
//     {
//         // serializedObject.Update();
//         // EditorGUILayout.PropertyField(serializedObject.FindProperty("items"));
//         // serializedObject.ApplyModifiedProperties();
//         base.DrawDefaultInspector();

//         numColumns = (int)((Screen.width + 5) / textureSize);

//         consumableFilter = EditorGUILayout.Toggle("Consumable", consumableFilter);

//         // TODO:
//         // * name as tooltip 
//         // * drag-drop space beneath to add items
//         // * Can the ItemData drawer define itself and just be reused here?
//         // * Display as icon
//         int displayIndex = 0;
//         ItemData itemData;
//         EditorGUILayout.BeginHorizontal();
//         for (int i = 0; i < catalog.items.Count; i++)
//         {
//             itemData = catalog.items[i];
//             if (consumableFilter && !itemData.IsConsumable)
//                 continue;
//             catalog.items[i] = (ItemData)EditorGUILayout.ObjectField(
//                 itemData, typeof(ItemData), false, GUILayout.Width(textureSize), GUILayout.Height(textureSize)
//             );
//             displayIndex++;
//             if (displayIndex % numColumns == 0)
//             {
//                 EditorGUILayout.EndHorizontal();
//                 EditorGUILayout.BeginHorizontal();
//             }
//         }
//         EditorGUILayout.EndHorizontal();
//     }


//     // [CustomPropertyDrawer(typeof(...))]
//     // public class XXX : PropertyDrawer

//     // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     // {
//     //     label = EditorGUI.BeginProperty(position, label, property);
//     //     Rect contentPosition = EditorGUI.PrefixLabel(position, label);
//     //     // EditorGUI.indentLevel = 0;
//     //     EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("inventoryID"), GUIContent.none);
//     //     EditorGUI.EndProperty();
//     // }

//     // public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
//     // 	return Screen.width < 333 ? (16f + 18f) : 16f;
//     // }
// }