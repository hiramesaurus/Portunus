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
        internal static GUIStyle ControlButton = new GUIStyle (EditorStyles.miniButton)
        {
            fixedWidth = 15,
            fixedHeight = 14,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(3, 0, 0, 0)
        };
    }
    
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
            Content = new GUIContent (GetName (property));
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

        private void DrawArray ()
        {
            using (new GUILayout.VerticalScope (EditorStyles.helpBox))
            {
                var expanded = Property.isExpanded;
                // Draw Header
                using (new EditorGUILayout.HorizontalScope ())
                {
                    EditorGUI.indentLevel++;
                    expanded = EditorGUILayout.Foldout (expanded, Content, true);
                    EditorGUI.indentLevel--;

                    if (GUILayout.Button ("+", Styles.ControlButton))
                    {
                        Property.InsertArrayElementAtIndex (Property.arraySize);
                        UpdatePropertyWithUndo ();
                        return;
                    }
                    
                    Property.arraySize = Mathf.Max (
                        EditorGUILayout.IntField (Property.arraySize, GUILayout.Width (60)),
                        0);
                }

                Property.isExpanded = expanded;
                if (!expanded)
                    return;

                // Draw Content
                using (new EditorGUILayout.VerticalScope ())
                {
                    for (var i = 0; i < Property.arraySize; i++)
                    {
                        if (DrawDelete (i))
                            return;
                    }
                }
            }

            bool DrawDelete (int index)
            {
                using (new EditorGUILayout.HorizontalScope ())
                {
                    EditorGUILayout.LabelField (index.ToString (), GUILayout.Width (20));
                    DrawSimpleField (Property.GetArrayElementAtIndex (index));

                    if (!GUILayout.Button ("X", Styles.ControlButton))
                        return false;
                    
                    Property.DeleteArrayElementAtIndex (index);
                    UpdatePropertyWithUndo ();
                    return true;
                }
            }
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