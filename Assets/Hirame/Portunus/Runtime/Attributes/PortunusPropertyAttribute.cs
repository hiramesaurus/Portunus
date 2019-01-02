using UnityEditor;

namespace Hirame.Portunus
{
    public abstract class PortunusPropertyAttribute : System.Attribute
    {
        public abstract void ApplyOverrides (SerializedObject obj, SerializedProperty prop);
    }

}