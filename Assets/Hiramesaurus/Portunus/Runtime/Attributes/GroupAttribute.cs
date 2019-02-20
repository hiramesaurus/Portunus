using System;

namespace Hiramesaurus.Portunus
{
    [AttributeUsage (AttributeTargets.Field)]
    public class GroupAttribute : System.Attribute
    {
        public string Name;

        public GroupAttribute (string name)
        {
            Name = name;
        }
        
    }

}