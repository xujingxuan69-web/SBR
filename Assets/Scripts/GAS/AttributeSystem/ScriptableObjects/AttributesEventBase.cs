using UnityEngine;
using SBR.GAS.AttributeSystem.Components;

namespace SBR.GAS.AttributeSystem.ScriptableObjects
{
    public abstract class AttributesEventBase : ScriptableObject
    {   //抽象类，规定 PreAttributeChange 和 PostAttributeChange 的返回值和参数
        //后续创建子类实现具体逻辑，通过 SO 让 AttributeSystemBehaviour 遍历 _attributeEvents 列表并调用子类的功能

        public abstract void PreAttributeChange(    //属性更新前调用
            AttributeSystemBehaviour attributeSystem,
            ref AttributeValue newAttributeValue
        );


        public abstract void PostAttributeChange(   //属性更新后调用
            AttributeSystemBehaviour attributeSystem,
            ref AttributeValue oldAttributeValue,
            ref AttributeValue newAttributeValue
        );
    }
}