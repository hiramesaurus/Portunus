using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus.Editor
{
    
    [CustomEditor (typeof (MonoBehaviour), true)]
    public class EditorBase : UnityEditor.Editor
    {        
        private readonly HashSet<string> ignoredProperties = new HashSet<string> { "Base" , "m_Script"}; 
        private readonly Dictionary<string, Action<SerializedProperty>> changedCallbacks = 
            new Dictionary<string, Action<SerializedProperty>> ();
        
        private readonly List<PropertyGroup> drawerGroups = new List<PropertyGroup> ();

        protected virtual void OnEnable ()
        {
            CreateDrawers ();
        }

        public override void OnInspectorGUI ()
        {
            EditorGUILayout.Space ();
            
            var requiresSaving = false;

            foreach (var group in drawerGroups)
            {
                if (!group.DrawWithChangeCheck ())
                    continue;

                requiresSaving = true;
                
                foreach (var drawer in group.Drawers)
                {
                    NotifyHasChanged (drawer.Property);
                }
            }
            
            if (requiresSaving)
                serializedObject.ApplyModifiedProperties ();
        }

        public void AddChangeListener (SerializedProperty property, Action<SerializedProperty> callback)
        {
            if (changedCallbacks.ContainsKey (property.name))
            {
                changedCallbacks[property.name] += callback;
                return;
            }
            changedCallbacks.Add (property.name, callback);
        }

        public void RemoveChangeListener (SerializedProperty property, Action<SerializedProperty> callback)
        {
            if (!changedCallbacks.ContainsKey (property.name))
                return;
            // ReSharper disable once DelegateSubtraction
            changedCallbacks[property.name] -= callback;
        }

        private void CreateDrawers ()
        {            
            drawerGroups.Clear ();
            drawerGroups.Add (new PropertyGroup (null));

            using (var iterator = serializedObject.GetIterator ())
            {
                do
                {
                    if (ignoredProperties.Contains (iterator.name))
                        continue;

                    drawerGroups[0].Add (new PropertyDrawer (iterator));

                    // This will ensure that we skip drawing the size of a array.
                    if (iterator.isArray)
                        iterator.NextVisible (true);

                } while (iterator.NextVisible (true));
            }
            
        }
      
      
        public static bool DrawPropertyWithChangeCheck (SerializedProperty property)
        {
            using (var changed = new EditorGUI.ChangeCheckScope ())
            {
                EditorGUILayout.PropertyField (property);
                return changed.changed;
            }
        }

        private void NotifyHasChanged (SerializedProperty property)
        {
            if (!changedCallbacks.TryGetValue (property.name, out var callbacks))
                return;
            callbacks.Invoke (property);
        }
        
    }

}