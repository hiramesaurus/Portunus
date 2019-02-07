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

        public bool IsVisible;
        public bool HasChanged;
        public bool HideLabel;
        public bool HasChildDrawers;
        public bool ParentIsArray;

        public List<PropertyDrawer> ChildDrawers;

        public List<DynamicDrawerAttribute> CustomDrawers = new List<DynamicDrawerAttribute> ();
        
        public int ChildCount => HasChildDrawers ? ChildDrawers.Count : 0;
        
        public PropertyDrawer (SerializedProperty property, bool parentIsArray = false)
        {
            Property = property.Copy ();
            var attributes = GetDrawerAttributes (property);
            LabelContent = ApplyCustomDrawers (property, attributes);
                        
            HideLabel = ShouldHideLabel (property);
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
                    //Debug.Log (next.displayName);
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

        private void CheckCustomDrawers ()
        {
            IsVisible = true;
            foreach (var drawer in CustomDrawers)
            {
                drawer.Initialize (Property);
                IsVisible = IsVisible & drawer.IsVisible;
            }
        }

        public bool Draw ()
        {
            CheckCustomDrawers ();
            
            if (!IsVisible)
                return false;
            
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
            using (new GUILayout.VerticalScope (EditorStyles.helpBox))
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

        private GUIContent ApplyCustomDrawers (SerializedProperty property, IEnumerable<DrawerAttribute> attr)
        {
            var content = new GUIContent(property.displayName);
            foreach (var drawer in attr)
            {
                drawer.Initialize (property);
                content.text = drawer.CustomLabel (content.text);
                IsVisible = IsVisible & drawer.IsVisible;
                
                if (drawer is DynamicDrawerAttribute item)
                    CustomDrawers.Add (item);
            }
           
            return content;
        }

        private static IEnumerable<DrawerAttribute> GetDrawerAttributes (SerializedProperty property)
        {
            var fieldInfo = property.serializedObject.targetObject
                .GetType ().GetField (property.name);

            var list = new List<DrawerAttribute> ();
            if (fieldInfo == null)
                return list;

            foreach (var attr in fieldInfo.GetCustomAttributes ())
            {
                if (attr is DrawerAttribute item)
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