using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hiramesaurus.Portunus
{
    public class ConditionAttribute : DynamicDrawerAttribute
    {
        protected System.Func<SerializedProperty, bool> Comparison;
        protected string MemberName;
        protected long LongValue;
        protected bool BoolValue;
        protected object ObjectValue;

        public ConditionAttribute (string memberName, int value)
        {
            MemberName = memberName;
            LongValue = value;
            Comparison = CompareInts;
        }

        public ConditionAttribute (string memberName, bool value)
        {
            MemberName = memberName;
            BoolValue = value;          
            Comparison = CompareBooleans;
        }

        public ConditionAttribute (string memberName, object value)
        {
            MemberName = memberName;
            if (value is ValueType)
            {
                LongValue = Convert.ToInt64 (value);
                Comparison = CompareEnums;
            }
            else
            {
                ObjectValue = value;
                Comparison = CompareObjects;
            }
        }

        public override bool IsVisible => Comparison.Invoke (targetProperty);
    
        private bool CompareInts (SerializedProperty property)
        {
            var prop = GetMemberProperty (property);
            return prop.intValue == LongValue;
        }
    
        private bool CompareBooleans (SerializedProperty property)
        {
            var prop = GetMemberProperty (property);
            return prop.boolValue == BoolValue;
        }

        private bool CompareEnums (SerializedProperty property)
        {
            var prop = GetMemberProperty (property);
            return prop.enumValueIndex == LongValue;
        }

        private bool CompareObjects (SerializedProperty property)
        {
            var prop = GetMemberProperty (property);
            Debug.Log (prop.objectReferenceValue.name);
            return false;
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
