using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus.Editor
{
    public class PropertyGroup
    {
        public string Label = "_Default";

        public List<PropertyDrawer> Drawers = new List<PropertyDrawer> ();

        public PropertyDrawer this [int i] => Drawers[i];

        public bool DrawBox;

        public PropertyGroup (string label)
        {
            if (string.IsNullOrWhiteSpace (label) || label[0] == '_')
                return;
            
            Label = label;
            DrawBox = true;
        }

        public void Add (PropertyDrawer drawer)
        {
            Drawers.Add (drawer);
        }

        public bool DrawWithChangeCheck ()
        {
            var requiresSaving = false;
            var style = DrawBox ? EditorStyles.helpBox : GUIStyle.none;
            
            using (new GUILayout.VerticalScope (style))
            {
                foreach (var drawer in Drawers)
                {
                    if (!drawer.Draw ())
                        continue;

                    requiresSaving = true;
                }
            }

            return requiresSaving;
        }
    }
}