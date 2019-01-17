using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus.Editor
{
    internal static class Styles
    {
        internal static readonly GUIStyle ArrayControl = new GUIStyle (EditorStyles.miniButton)
        {
            fixedWidth = 15,
            fixedHeight = 14,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(3, 0, 0, 0)
        };
        
        internal static readonly GUIStyle ArrayFoldout = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        }; 
    }
    
    public class PropertyDrawer
    {
        public SerializedProperty Property { get; private set; }
        public GUIContent LabelContent { get; private set; }

        public bool HasChanged;
        public bool HideLabel;

        public PropertyDrawer (SerializedProperty property)
        {
            Property = property.Copy ();
            HideLabel = ShouldHideLabel (property);
            LabelContent = new GUIContent (GetName (property));
        }

        public bool Draw ()
        {
            HasChanged = false;

            using (var changed = new EditorGUI.ChangeCheckScope ())
            {
                if (Property.isArray)
                {
                    if (ArrayDrawer.Draw (Property, LabelContent))
                    {
                        UpdatePropertyWithUndo ();
                    }
                }
                else
                    DrawSimpleField (Property, LabelContent);

                HasChanged = changed.changed;
            }

            return HasChanged;
        }

        /// <summary>
        /// THIS IS ACTUALLY NOT ONLY GETTING NAME
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private string GetName (SerializedProperty property)
        {
            // TODO:
            // Figure out some super cool way of having those properties be
            // generically applicable.
            var attributes = GetDrawerAttributes (property);

            var customLabel = attributes.FirstOrDefault (a => a is LabelAttribute);
            
            // TODO:
            // For now this is just a super stupid way of doing a custom label
            if (customLabel != null)
            {
                return (customLabel as LabelAttribute).Label;
            }
            
            
            return ObjectNames.NicifyVariableName (property.name);
        }

        private static IEnumerable<PortunusPropertyAttribute> GetDrawerAttributes (SerializedProperty property)
        {
            var fieldInfo = property.serializedObject.targetObject
                .GetType ().GetField (property.name);

            var list = new List<PortunusPropertyAttribute> ();
            if (fieldInfo == null)
                return list;

            foreach (var attr in fieldInfo.GetCustomAttributes ())
            {
                if (attr is PortunusPropertyAttribute item)
                    list.Add (item);
            }

            return list;
        }

        private static bool ShouldHideLabel (SerializedProperty property)
        {
            return string.IsNullOrWhiteSpace (property.name) || property.isArray;
        }

        private void DrawSimpleField (SerializedProperty prop, GUIContent content = null)
        {
            EditorGUILayout.PropertyField (prop, content ?? GUIContent.none);
        }    

        private void UpdatePropertyWithUndo ()
        {
            var data = Property.serializedObject;
            Undo.RecordObject (data.targetObject, "Undo list modification");

            data.ApplyModifiedProperties ();
            data.Update ();
            Property = data.FindProperty (Property.name);
        }
    }
}