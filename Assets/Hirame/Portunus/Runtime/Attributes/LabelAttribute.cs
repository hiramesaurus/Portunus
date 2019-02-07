using UnityEditor;

namespace Hirame.Portunus
{
    public class LabelAttribute : DrawerAttribute
    {
        public string Label { get; private set; }

        public LabelAttribute (string label)
        {
            Label = label;
        }

        public override string CustomLabel (string label)
        {
            return Label;
        }
    }

}