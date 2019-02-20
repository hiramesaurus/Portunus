using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hiramesaurus.Portunus
{
    public class HideIfAttribute : ConditionAttribute
    {
        public HideIfAttribute (string memberName, int value) : base (memberName, value)
        {
        }

        public HideIfAttribute (string memberName, bool value) : base (memberName, value)
        {
        }

        public HideIfAttribute (string memberName, object value) : base (memberName, value)
        {       
        }
        
        public override bool IsVisible => !base.IsVisible;
    }

}
