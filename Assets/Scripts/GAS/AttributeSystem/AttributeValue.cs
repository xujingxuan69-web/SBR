using System;
using UnityEngine;
using SBR.GAS.AttributeSystem.Components;
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
        [field:SerializeField, Tooltip("常值-基础值")]
        public float BaseValue { get; set; }
        [field: SerializeField, Tooltip("临时值-受效果影响")] 
        public float CurrentValue { get; set; }

        //核心属性修饰器-包括永久增益、被动效果等加成
        public Modifier CoreModifier;

        //外部效果修饰器-包括临时的Buff、技能效果等加成
        public Modifier ExternalModifier;

        public AttributeValue(AttributeSO attribute)
        {
            Attribute = attribute;
            BaseValue = CurrentValue = 0f;
            CoreModifier = new Modifier();
            ExternalModifier = new Modifier();
        }

        public AttributeValue Clone()
        {
            return new AttributeValue()
            {
                Attribute = Attribute,
                BaseValue = BaseValue,
                CurrentValue = CurrentValue,
                CoreModifier = CoreModifier,
                ExternalModifier = ExternalModifier
            };
        }

        public readonly AttributeValue CalculateInitialValue(AttributeSystemBehaviour attributeSystem)
            => Attribute.initialValueCalculator.Calculate(this, attributeSystem);

        public readonly AttributeValue CalculateCurrentValue(AttributeSystemBehaviour attributeSystem)
            => Attribute.currentValueCalculator.Calculate(this, attributeSystem);
    }
}