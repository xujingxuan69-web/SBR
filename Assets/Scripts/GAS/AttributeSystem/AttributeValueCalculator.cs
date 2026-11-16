using SBR.GAS.AttributeSystem.Components;
using System;

namespace SBR.GAS.AttributeSystem
{
    public interface IAttributeValueCalculator  //定义计算接口
    {
        AttributeValue Calculate(
            AttributeValue baseValue,
            AttributeSystemBehaviour atrributeSystem);
    }

    [Serializable]  
    //允许Unity序列化当前类的实例，使通过SerializeReference和SubclassSelector选中当前类为计算规则
    public class InitialAttributeValueCalculator : IAttributeValueCalculator
    {   //当前InitialCalculator和CurrentCalculator是默认计算规则，允许自行拓展
        public AttributeValue Calculate(AttributeValue baseValue,
            AttributeSystemBehaviour atrributeSystem) => baseValue;
    }

    [Serializable]
    public class CurrentAttributeValueCalculator : IAttributeValueCalculator
    {
        public AttributeValue Calculate(AttributeValue baseValue,
            AttributeSystemBehaviour atrributeSystem) => CalaculateCurrentAttributeValue(baseValue);

        public static AttributeValue CalaculateCurrentAttributeValue(AttributeValue attributeValue)
        {
            return attributeValue;  //待续
        }
    }
}
