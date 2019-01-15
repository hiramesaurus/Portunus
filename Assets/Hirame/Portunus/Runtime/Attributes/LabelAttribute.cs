using UnityEditor;

namespace Hirame.Portunus
{
    public class LabelAttribute : PortunusPropertyAttribute
    {
        public string Label { get; private set; }

        public LabelAttribute (string label)
        {
            Label = label;
        }

        public override void ApplyOverrides ()
        {
            
        }
    }

}