using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.GAS
{
    public class AttributeSystemBehaviour : MonoBehaviour
    {
        public delegate void PreAttributeChangeDelegate(AttributeSO attribute, AttributeValue newValue);
        //delegate-定义回调方法的格式(参数、返回类型)，挂在委托/事件上，回调时触发

    }
}
