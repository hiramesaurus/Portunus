using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus.Editor
{
    public class PropertyDrawer
    {
        public SerializedProperty Property { get; private set; }
        public GUIContent Content { get; private set; }

        public bool HasChanged;

        public PropertyDrawer (SerializedProperty property)
        {
            Property = property.Copy ();
            Content = new GUIContent(property.name);
        }

        public bool Draw ()
        {
            HasChanged = false;
            
            using (var changed = new EditorGUI.ChangeCheckScope ())
            {
                if (Property.isArray)
                    DrawArray ();
                else
                    DrawSimpleField (Property, Content);

                HasChanged = changed.changed;
            }

            return HasChanged;
        }

        private void DrawSimpleField (SerializedProperty prop, GUIContent content = null)
        {
            EditorGUILayout.PropertyField (prop, content ?? GUIContent.none);
        }

        private void DrawArray ()
        {
            if (Content.text.Equals ("data"))
                return;

            using (new GUILayout.VerticalScope (EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope ())
                {
                    EditorGUILayout.LabelField (Content, EditorStyles.boldLabel);
                    Property.arraySize = Mathf.Max (
                        EditorGUILayout.IntField (Property.arraySize, GUILayout.Width (60)), 
                        0);                    
                }

                using (new EditorGUILayout.VerticalScope ())
                {
                
                    for (var i = 0; i < Property.arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope ())
                        {
                            EditorGUILayout.LabelField (i.ToString(), GUILayout.Width (20));
                            DrawSimpleField (Property.GetArrayElementAtIndex (i));
                        }
                    }
                }
            }
                
        }
    }

}
