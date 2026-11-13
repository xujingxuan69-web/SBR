using SBR.GAS.AttributeSystem.Components;

namespace SBR.GAS.AttributeSystem
{
    public interface IAttributeValueCalculator  //定义计算接口
    {
        AttributeValue Calculate(
            AttributeValue baseValue,
            AttributeSystemBehaviour atrributeSystem);
    }

    public class InitialAttributeValueCalculator : IAttributeValueCalculator
    {
        public AttributeValue Calculate(AttributeValue baseValue,
            AttributeSystemBehaviour atrributeSystem) => baseValue;
    }

    public class CurrentAttributeValueCalculator : IAttributeValueCalculator
    {
        public AttributeValue Calculate(AttributeValue baseValue,
            AttributeSystemBehaviour atrributeSystem) => CalaculateCurrentAttributeValue(baseValue);

        public static AttributeValue CalaculateCurrentAttributeValue(AttributeValue attributeValue)
        {
            return attributeValue;
        }
    }
}
