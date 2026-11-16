using System;
using UnityEngine;

namespace SBR.GAS.AttributeSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "InitStats", menuName = "SBR/GAS/Attributes/Initialize Stats")]
    public class InitializeStatsSO : ScriptableObject
    {    //…˘√˜≥ı º Ù–‘

        [field: SerializeField]
        public AttributeWithValue[] Stats { get; private set; }
    }

    [Serializable]
    public struct AttributeWithValue
    {
        public AttributeWithValue(AttributeSO attribute, float value)
        {
            Attribute = attribute;
            Description = attribute.description;
            Value = value;
        }

        public AttributeSO Attribute;
        [SerializeField] private string Description;
        public float Value;
    }
}
