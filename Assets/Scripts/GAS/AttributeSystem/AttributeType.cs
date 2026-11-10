using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SBR.GAS
{
    [CreateAssetMenu(menuName = "SBR/GAS/Attribute")]
    public class AttributeType : MonoBehaviour
    {
        [SerializeField] private float initialValue;
        public float InitialValue => initialValue;
    }
}
