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

        public PropertyDrawer (SerializedProperty property)
        {
            Property = property.Copy ();
            Content = new GUIContent(property.name);
        }

        public bool Draw ()
        {
            using (var changed = new EditorGUI.ChangeCheckScope ())
            {
                EditorGUILayout.PropertyField (Property, Content);
                return changed.changed;
            }
        }
    }

}
