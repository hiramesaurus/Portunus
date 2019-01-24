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
        
        public List<PropertyDrawer> ChildDrawers;

        public PropertyDrawer (SerializedProperty property)
        {
            Property = property.Copy ();
            HideLabel = ShouldHideLabel (property);
            LabelContent = new GUIContent (GetName (property));
            HasChildDrawers = CheckForNesting (property);
        }

        private bool CheckForNesting (SerializedProperty property)
        {
            ChildDrawers = new List<PropertyDrawer> ();

            if (!property.isArray && property.hasVisibleChildren)
            {
                var next = property.Copy ();
                var depth = next.depth;
                var enter = true;
                
                while (next.NextVisible (enter) && depth != next.depth)
                {
                    Debug.Log (next.displayName);
                    enter = false;
                    ChildDrawers.Add (new PropertyDrawer (next));
                }
                return true;
            }

//            for (var i = 0; i < property.arraySize; i++)
//            {
//               ChildDrawers.Add (new PropertyDrawer (property.GetArrayElementAtIndex (i)));
//            }
            return false;
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
                if (IsRealArray (Property))
                {
                    if (ArrayDrawer.Draw (Property, LabelContent))
                    {
                        UpdatePropertyWithUndo ();
                    }
                }
                else if (Property.hasVisibleChildren && Property.isExpanded)
                {
                    DrawSimpleField (Property, LabelContent);
                    DrawChildren ();
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
            EditorGUI.indentLevel++;
            
            foreach (var child in ChildDrawers)
            {
                child.Draw ();
            }

            EditorGUI.indentLevel--;
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