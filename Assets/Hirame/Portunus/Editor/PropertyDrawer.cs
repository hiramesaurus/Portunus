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
        public bool HideLabel;

        public PropertyDrawer (SerializedProperty property)
        {
            Property = property.Copy ();
            HideLabel = ShouldHideLabel (property);
            Content = new GUIContent(ObjectNames.NicifyVariableName (property.name));
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

        private static bool ShouldHideLabel (SerializedProperty property)
        {
            if (string.IsNullOrWhiteSpace (property.name))
                return true;
            if (property.isArray && property.name.Equals ("data"))
                return true;
            return false;
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
