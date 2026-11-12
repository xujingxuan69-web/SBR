using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.GAS
{
    public class AttributeSystemBehaviour : MonoBehaviour
    {
        public delegate void PreAttributeChangeDelegate(AttributeSO attribute, AttributeValue newValue);
        //delegate-声明固定回调方法格式(参数、返回类型)的委托，该委托只能订阅该固定格式的方法
        public delegate void PostAttributeChangeDelegate(AttributeSO attribute,
            AttributeValue oldValue, AttributeValue newValue);

        public event PreAttributeChangeDelegate PreAttributeChange;
        //事件的订阅方法的格式要与声明委托的订阅方法的格式相同
        //实际上这里的声明与直接用using System + Action/System.Action<...>等价
        public event PostAttributeChangeDelegate PostAttributeChange;

        [SerializeField] private bool _initOnAwake = true;

    }
}
