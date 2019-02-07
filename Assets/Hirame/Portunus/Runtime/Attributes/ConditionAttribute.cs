using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hirame.Portunus
{
    public class ConditionAttribute : DynamicDrawerAttribute
    {
        protected System.Func<SerializedProperty, bool> Comparison;
        protected string MemberName;
        protected int IntValue;
        protected bool BoolValue;

        public ConditionAttribute (string memberName, int value)
        {
            MemberName = memberName;
            IntValue = value;
            Comparison = CompareInts;
        }

        public ConditionAttribute (string memberName, bool value)
        {
            MemberName = memberName;
            BoolValue = value;          
            Comparison = CompareBooleans;
        }      

        public override bool IsVisible => Comparison.Invoke (targetProperty);
    
        private bool CompareInts (SerializedProperty property)
        {
            var prop = GetMemberProperty (property);
            return prop.intValue == IntValue;
        }
    
        private bool CompareBooleans (SerializedProperty property)
        {
            var prop = GetMemberProperty (property);
            return prop.boolValue == BoolValue;
        }
    
        private SerializedProperty GetMemberProperty (SerializedProperty property)
        {
            SerializedProperty prop;
            if (property.depth == 0)
            {
                prop = property.serializedObject.FindProperty (MemberName);
            }
            else
            {
                Debug.Log (property.propertyPath);
                prop = property;
            }
            return prop;
        }
    }

}
