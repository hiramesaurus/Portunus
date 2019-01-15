using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus
{
    public class GroupAttribute : PortunusPropertyAttribute
    {
        public string Name;

        public GroupAttribute (string name)
        {
            Name = name;
        }
        
        public override void ApplyOverrides ()
        {
            
        }
    }

}