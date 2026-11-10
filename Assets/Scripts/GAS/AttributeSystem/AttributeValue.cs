using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.GAS
{
    [System.Serializable]   //显式结构体
    public struct AttributeValue
    {
        public float BaseValue { get; set; }

        public float CurrentValue { get; set; }
    }
}