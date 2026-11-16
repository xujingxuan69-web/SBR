using System;
using UnityEngine;
using SBR.GAS.AttributeSystem.ScriptableObjects;

namespace SBR.GAS.AttributeSystem
{
    public enum EModifierType
    { 
        External,
        Core,
    }

    [Serializable]
    public struct AttributeValue
    {
        [field: SerializeField] public AttributeSO Attribute { get; set; }
        public float BaseValue { get; set; }

        public float CurrentValue { get; set; }



    }
}