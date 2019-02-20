using System.Runtime.InteropServices.ComTypes;
using Hiramesaurus.Portunus;
using UnityEngine;

public class FooBar : MonoBehaviour
{
    public enum SuperEnum { Asd, Wasd, Kay }
    
    [Label ("Attribute Label")]
    public int IntField;
    public float FloatField;
    
    public string TestString;
    
    public bool ShowSecret;
    
    [ShowIf (nameof (ShowSecret), true)]
    public string Secret = "Boo!";

    public SuperEnum EnumValue;

    [ShowIf (nameof (EnumValue), SuperEnum.Kay)]
    public string ShowIfKay = "Kay";
    
    public AClass SimpleClass;
    
    public string[] StringArray;

    public int[] OtherArray;

    public MonoBehaviour[] MonoArray;


    
    public AClass[] ClassArray;

    [System.Serializable]
    public class AClass
    {
        public int AIntField;
        public float AFloatField;
    }
}
