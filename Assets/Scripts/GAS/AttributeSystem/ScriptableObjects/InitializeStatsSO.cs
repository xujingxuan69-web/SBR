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
            Value = value;
        }

        [ShowEditorOnlyMetadata] public AttributeSO Attribute;
        public float Value;
    }
}
