using System;

namespace Hirame.Portunus
{
    [AttributeUsage (AttributeTargets.Field)]
    public class ShowIfAttribute : ConditionAttribute
    {
        public ShowIfAttribute (string memberName, int value) : base (memberName, value)
        {
        }

        public ShowIfAttribute (string memberName, bool value) : base (memberName, value)
        {
        }
    }

}
