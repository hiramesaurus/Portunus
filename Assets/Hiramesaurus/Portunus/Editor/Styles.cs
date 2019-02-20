using UnityEditor;
using UnityEngine;

namespace Hiramesaurus.Portunus.Editor
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
}
