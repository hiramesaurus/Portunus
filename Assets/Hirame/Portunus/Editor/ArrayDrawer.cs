using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus.Editor

{
    public static class ArrayDrawer
    {
        public static bool Draw (SerializedProperty property, GUIContent label)
        {
            if (!property.isArray)
            {
                Debug.Log ("Property is not an array.");
                return false;
            }
            
            var arraySize = property.arraySize;

            using (new GUILayout.VerticalScope (EditorStyles.helpBox))
            {
                var expanded = property.isExpanded;

                using (new EditorGUILayout.HorizontalScope ())
                {
                    if (DrawExpandableHeader (label, arraySize > 0, ref expanded))
                    {
                        property.isExpanded = expanded;
                        return false;
                    }
                    
                    if (GUILayout.Button ("+", Styles.ArrayControl))
                    {
                        property.InsertArrayElementAtIndex (arraySize);
                        property.isExpanded = true;
                        return true;
                    }
                    
                    property.arraySize = Mathf.Max (
                        EditorGUILayout.IntField (arraySize, GUILayout.Width (60)),
                        0);
                }

                property.isExpanded = expanded;
                if (!expanded)
                    return false;

                // Draw Content
                using (new EditorGUILayout.VerticalScope ())
                {
                    for (var i = 0; i < arraySize; i++)
                    {
                        if (DrawRow (property, i))
                            return true;
                    }
                }
            }

            return false;
        }

        private static bool DrawExpandableHeader (GUIContent label, bool hasElements, ref bool isExpanded)
        {
            var originalState = isExpanded;
            EditorGUI.indentLevel++;
            
            if (hasElements)
            {
                isExpanded = EditorGUILayout.Foldout (isExpanded, label, Styles.ArrayFoldout);
            }
            else
            {
                EditorGUILayout.LabelField (label);
            }
            
            EditorGUI.indentLevel--;

            return isExpanded != originalState;
        }
        
        private static bool DrawRow (SerializedProperty property, int index)
        {
            using (new EditorGUILayout.HorizontalScope ())
            {
                EditorGUILayout.LabelField (index.ToString (), GUILayout.Width (20));
                EditorGUILayout.PropertyField (property.GetArrayElementAtIndex (index), GUIContent.none);
                
                if (!GUILayout.Button ("X", Styles.ArrayControl))
                    return false;
                    
                property.DeleteArrayElementAtIndex (index);
                return true;
            }
        }
    }
}