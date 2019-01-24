using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus.Editor
{
    public class PropertyDrawer
    {
        public SerializedProperty Property { get; private set; }
        public GUIContent LabelContent { get; private set; }

        public bool HasChanged;
        public bool HideLabel;
        public bool HasChildDrawers;
        public bool ParentIsArray;

        public List<PropertyDrawer> ChildDrawers;

        public int ChildCount => HasChildDrawers ? ChildDrawers.Count : 0;
        
        public PropertyDrawer (SerializedProperty property, bool parentIsArray = false)
        {
            Property = property.Copy ();
            HideLabel = ShouldHideLabel (property);
            LabelContent = new GUIContent (GetName (property));
            HasChildDrawers = UpdateChildDrawers (property);
            ParentIsArray = parentIsArray;
        }

        public bool UpdateChildDrawers (SerializedProperty property)
        {
            if (!property.hasVisibleChildren)
            {
                //Debug.Log ("No child drawers");
                return false;
            }

            if (ChildDrawers == null)
                ChildDrawers = new List<PropertyDrawer> ();
            else
                ChildDrawers.Clear ();

            var next = property.Copy ();
            var depth = next.depth;
            var enter = true;

            // The property is a array of data structures
            if (property.isArray && property.hasVisibleChildren)
            {
                if (property.arraySize == 0 || !property.GetArrayElementAtIndex (0).hasVisibleChildren)
                {
                    return false;
                }

                for (var i = 0; i < property.arraySize; i++)
                {
                    ChildDrawers.Add (new PropertyDrawer (property.GetArrayElementAtIndex (i), true));
                }
            }
            else
            {
                //Debug.Log ($"{property.name} is simple data structure.");
                // The property is a data structure
                while (next.NextVisible (enter) && depth != next.depth)
                {
                    Debug.Log (next.displayName);
                    enter = false;
                    ChildDrawers.Add (new PropertyDrawer (next));
                }
            }

            return true;
        }

        private static bool IsRealArray (SerializedProperty property)
        {
            return property.hasVisibleChildren && property.isArray;
        }

        public bool Draw ()
        {
            HasChanged = false;

            using (var changed = new EditorGUI.ChangeCheckScope ())
            {
                if (HasChildDrawers)
                {
                    if (!Property.isArray)
                    {
                        DrawChildren ();
                    }
                    if (ArrayDrawer.Draw (this))
                    {
                        // TODO:
                        // Add a undo variant that disposes and rebuilds drawers.
                        // Or maybe this is good enough?
                        UpdatePropertyWithUndo ();
                        UpdateChildDrawers (Property);
                    }
                }
                else if (IsRealArray (Property))
                {
                    if (ArrayDrawer.Draw (this))
                    {
                        UpdatePropertyWithUndo ();
                    }
                }
                else
                {
                    DrawSimpleField (Property, LabelContent);
                }


                HasChanged = changed.changed;
            }

            return HasChanged;
        }

        private void DrawChildren ()
        {           
            using (var scope = new GUILayout.VerticalScope (EditorStyles.helpBox))
            {                        
                DrawSimpleField (Property, LabelContent);
                if (!Property.isExpanded)
                    return;
                
                EditorGUI.indentLevel++;

                foreach (var child in ChildDrawers)
                {
                    child.Draw ();
                }

                EditorGUI.indentLevel--;
            }
           
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

        private static void DrawSimpleField (SerializedProperty prop, GUIContent content = null)
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