using UnityEngine;

namespace SBR.GAS.AttributeSystem.ScriptableObjects
{
    [CreateAssetMenu(menuName = "SBR/GAS/Attributes/Attribute")]
    public class AttributeSO : ScriptableObject
    {
        #if UNITY_EDITOR
        [field: SerializeField, EditorOnlyMetadata, TextArea, Tooltip("属性描述，仅供查看，不参与游戏逻辑")]
        public string description { get; private set; }
        #endif

        [field: SerializeReference, SubclassSelector, Tooltip("选择合适的初始值计算规则")]
        //SerializeReference-存储序列化字段，但不提供Inspector选中功能
        //SubclassSelector-开源项目Unity SerializeReference Extensions提供
        //为[SerializeReference]字段在Inspector中提供选择下拉框
        public IAttributeValueCalculator initialValueCalculator { get; private set; } = new InitialAttributeValueCalculator();

        [field: SerializeReference, SubclassSelector, Tooltip("选择合适的当前值计算规则")]
        public IAttributeValueCalculator currentValueCalculator { get; private set; } = new CurrentAttributeValueCalculator();
    }
}
