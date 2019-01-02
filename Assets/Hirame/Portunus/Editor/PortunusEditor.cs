using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus.Editor
{
    
    [CustomEditor (typeof (MonoBehaviour), true)]
    public class PortunusEditor : UnityEditor.Editor
    {        
        private readonly HashSet<string> ignoredProperties = new HashSet<string> { "Base" , "m_Script"}; 
        private readonly Dictionary<string, Action<SerializedProperty>> changedCallbacks = 
            new Dictionary<string, Action<SerializedProperty>> ();
        
        private readonly List<PropertyDrawer> drawers = new List<PropertyDrawer> ();

        protected virtual void OnEnable ()
        {
            CreateDrawers ();
        }

        public override void OnInspectorGUI ()
        {
            var requiresSaving = false;
            foreach (var drawer in drawers)
            {
                if (!drawer.Draw ())
                    continue;
                
                requiresSaving = true;
                NotifyHasChanged (drawer.Property);
            }
            if (requiresSaving)
                serializedObject.ApplyModifiedProperties ();
        }

        public void AddChangeListener (SerializedProperty property, Action<SerializedProperty> callback)
        {
            if (changedCallbacks.TryGetValue (property.name, out var callbacks))
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
            drawers.Clear ();

            var iterator = serializedObject.GetIterator ();
            do
            {
                if (ignoredProperties.Contains (iterator.name))
                    continue;
                
               drawers.Add (new PropertyDrawer (iterator));

            } while (iterator.NextVisible (true));
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