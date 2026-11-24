using System;
using SBR.GAS.AttributeSystem.Components;

namespace SBR.GAS.AttributeSystem
{
    public interface IAttributeValueCalculator  //定义计算接口
    {
        AttributeValue Calculate(
            AttributeValue baseValue,
            AttributeSystemBehaviour attributeSystem);
    }

    [Serializable]  
    //允许Unity序列化当前类的实例，使通过SerializeReference和SubclassSelector选中当前类为计算规则
    public class InitialAttributeValueCalculator : IAttributeValueCalculator
    {   //当前InitialCalculator和CurrentCalculator是默认计算规则，允许自行拓展
        public AttributeValue Calculate(AttributeValue baseValue,
            AttributeSystemBehaviour attributeSystem) => baseValue;
    }

    [Serializable]
    public class CurrentAttributeValueCalculator : IAttributeValueCalculator
    {
        public AttributeValue Calculate(AttributeValue baseValue,
            AttributeSystemBehaviour attributeSystem) => CalaculateCurrentAttributeValue(baseValue);

        public static AttributeValue CalaculateCurrentAttributeValue(AttributeValue attributeValue)
        {
            if (attributeValue.ExternalModifier.Overriding != 0)
            {   //Overriding-如装备的效果为让生命值翻倍，但攻击力为-1，就要使用该效果
                attributeValue.CurrentValue = attributeValue.ExternalModifier.Overriding;
                return attributeValue;
            }
            else if (attributeValue.ExternalModifier.IsOverrideZero)
            {
                attributeValue.CurrentValue = 0;
                return attributeValue;
            }

            if (attributeValue.CoreModifier.Overriding != 0)
            {
                attributeValue.CurrentValue = attributeValue.CoreModifier.Overriding;
                return attributeValue;
            }
            else if (attributeValue.CoreModifier.IsOverrideZero)
            {
                attributeValue.CurrentValue = 0;
                return attributeValue;
            }

            attributeValue.CurrentValue = (attributeValue.GetCoreValue() + attributeValue.ExternalModifier.Additive)
                * (attributeValue.ExternalModifier.Multiplicative + 1);
            return attributeValue;
        }
    }

    public static class AttributeValueExtensions
    { 
        public static float GetCoreValue(this AttributeValue attributeValue)
        {
            if (attributeValue.CoreModifier.Overriding != 0)
                return attributeValue.CoreModifier.Overriding;
            else if (attributeValue.CoreModifier.IsOverrideZero)
                return 0;

            return (attributeValue.BaseValue + attributeValue.CoreModifier.Additive)
                * (attributeValue.CoreModifier.Multiplicative + 1);
        }
    }
}
