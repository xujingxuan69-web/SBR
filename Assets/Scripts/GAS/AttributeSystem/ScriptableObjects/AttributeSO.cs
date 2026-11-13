using UnityEngine;

namespace SBR.GAS.AttributeSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "SBR/GAS/Attribute")]
    public class AttributeSO : ScriptableObject
    {   //定义属性，以及声明初始化与当前值的计算规则

        [SerializeField, TextArea, Tooltip("属性描述")]
        private string description; //属性的描述，方便后续看


        [field: SerializeReference] //用于指定对应接口
        private IAttributeValueCalculator initialValueCalculator;

        [field: SerializeReference]
        private IAttributeValueCalculator currentValueCalculator;
    }
}
