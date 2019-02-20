using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hiramesaurus.Portunus.Editor
{
    public delegate void ChangeHandler (SerializedProperty property);
   
    [CustomEditor (typeof (Object), true)]
    public class EditorBase : UnityEditor.Editor
    {        
        
        private readonly HashSet<string> ignoredProperties = new HashSet<string>
        {
            "Base" , "m_Script"
        }; 
        
        private readonly Dictionary<string, ChangeHandler> changedCallbacks = 
            new Dictionary<string, ChangeHandler> ();
        
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

        public void AddChangeListener (
            SerializedProperty property, ChangeHandler callback)
        {
            if (changedCallbacks.ContainsKey (property.name))
            {
                changedCallbacks[property.name] += callback;
                return;
            }
            changedCallbacks.Add (property.name, callback);
        }

        public void RemoveChangeListener (
            SerializedProperty property, ChangeHandler callback)
        {
            if (changedCallbacks.ContainsKey (property.name))
            {
                // ReSharper disable once DelegateSubtraction
                changedCallbacks[property.name] -= callback;  
            }        
        }

        private void CreateDrawers ()
        {            
            drawerGroups.Clear ();
            drawerGroups.Add (new PropertyGroup (string.Empty));

            using (var iterator = serializedObject.GetIterator ())
            {
                if (!iterator.NextVisible (true))
                    return;
                
                do
                {
                    if (IsHiddenProperty (iterator))
                        continue;               
                    
                    drawerGroups[0].Add (new PropertyDrawer (iterator));

                } while (iterator.NextVisible (false));
            }
            
        }

        private bool IsHiddenProperty (SerializedProperty property)
        {
            return ignoredProperties.Contains (property.name);
        }
      
        public static bool DrawPropertyWithChangeCheck (SerializedProperty property)
        {
            using (var changeScope = new EditorGUI.ChangeCheckScope ())
            {
                EditorGUILayout.PropertyField (property);
                return changeScope.changed;
            }
        }

        private void NotifyHasChanged (SerializedProperty property)
        {
            if (changedCallbacks.TryGetValue (property.name, out var callbacks))
                callbacks.Invoke (property);           
        }
        
    }

}