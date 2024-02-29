using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


// from https://forum.unity.com/threads/editor-tool-better-scriptableobject-inspector-editing.484393/


///// <summary>
///// Draw ScriptableObject in inspector.
///// </summary>
//[CustomPropertyDrawer(typeof(ScriptableObject), true)]
//public class ScriptableObjectDrawer : PropertyDrawer
//{
//     // Cached scriptable object editor
//     Editor editor = null;

//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//          // Draw label
//          EditorGUI.PropertyField(position, property, label, true);

//          // Draw foldout arrow
//          if (property.objectReferenceValue != null)
//          {
//               property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
//          }

//          // Draw foldout properties
//          if (property.isExpanded)
//          {
//               // Make child fields be indented
//               EditorGUI.indentLevel++;

//               // Draw object properties
//               if (!editor)
//                    Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);

//               if (editor) // Catch empty property
//                    editor.OnInspectorGUI();
//               //editor.OnInspectorGUI();

//               // Set indent back to what it was
//               EditorGUI.indentLevel--;
//          }
//     }
//}


[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class ScriptableObjectDrawer : PropertyDrawer
{
     // Static foldout dictionary
     private static Dictionary<System.Type, bool> foldoutByType = new Dictionary<System.Type, bool>();

     // Cached scriptable object editor
     private Editor editor = null;

     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
     {
          // Draw label
          EditorGUI.PropertyField(position, property, label, true);

          // Draw foldout arrow
          bool foldout = false;
          if (property.objectReferenceValue != null)
          {
               // Store foldout values in a dictionary per object type
               bool foldoutExists = foldoutByType.TryGetValue(property.objectReferenceValue.GetType(), out foldout);
               foldout = EditorGUI.Foldout(position, foldout, GUIContent.none);
               if (foldoutExists)
                    foldoutByType[property.objectReferenceValue.GetType()] = foldout;
               else
                    foldoutByType.Add(property.objectReferenceValue.GetType(), foldout);
          }

          // Draw foldout properties
          if (foldout)
          {
               // Make child fields be indented
               EditorGUI.indentLevel++;

               // Draw object properties
               if (!editor)
                    Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
               editor.OnInspectorGUI();

               // Set indent back to what it was
               EditorGUI.indentLevel--;
          }
     }
}


[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class _DummyEditor : Editor
{
     /*
     This one is mandatory since without it, the custom property drawer will throw errors. 

     You need a custom editor class of the component utilising a ScriptableObject. 
     So we just create a dummy editor, that can be used for every MonoBehaviour.
     With this empty implementation it doesn't alter anything, it just removes Unitys property drawing bug.
     */
}









