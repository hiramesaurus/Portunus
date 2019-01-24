using Hirame.Portunus;
using UnityEngine;

public class FooBar : MonoBehaviour
{
    [Label ("Attribute Label")]
    public int IntField;
    public float FloatField;
    
    public string TestString;

    public string[] StringArray;

    public int[] OtherArray;

    public MonoBehaviour[] MonoArray;

    public AClass[] ClassArray;

    [System.Serializable]
    public class AClass
    {
        public int IntField;
        public float FloatField;
    }
}
