using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SBR.GAS
{
    [CreateAssetMenu(menuName = "SBR/GAS/Attribute")]
    public class AttributeSO : ScriptableObject  //定义属性
    {
        [SerializeField, TextArea] private string description;  //属性的描述，方便后续看


        [field: SerializeReference]    //用于指定对应接口
        private IAttributeValueCalculator initialValueCalculator;

        [field: SerializeReference]
        private IAttributeValueCalculator currentValueCalculator;
    }
}
