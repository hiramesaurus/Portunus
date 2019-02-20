
using UnityEditor;

namespace Hiramesaurus.Portunus
{
    public abstract class DrawerAttribute : System.Attribute
    {
        public int DrawOrder { get; private set; } = 0;
        
        public virtual bool IsVisible { get; private set; } = true;

        protected SerializedProperty targetProperty;
        
        public virtual void Initialize (SerializedProperty property)
        {
            targetProperty = property;
        }
        
        public virtual string CustomLabel (string label)
        {
            return label;
        }
    }

}