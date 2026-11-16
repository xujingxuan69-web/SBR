using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.GAS.AttributeSystem.Components
{
    public class StatsInitializer : MonoBehaviour
    {
        [SerializeField] private AttributeSystemBehaviour _attributeSystem;
        [SerializeField] private bool _initOnStart = false;
        //[SerializeReference] private IStatsProvider _statsProvider;
    }
}
