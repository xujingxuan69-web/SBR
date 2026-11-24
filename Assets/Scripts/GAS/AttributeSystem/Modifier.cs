using System;

namespace SBR.GAS.AttributeSystem
{
    [Serializable]
    public struct Modifier
    {
        public float Additive;
        public float Multiplicative;

        public bool IsOverrideZero; 
        public float Overriding;
        //Overriding在不为0时会覆盖CurrentValue
        //IsOverrideZero用于处理Overriding为0且需要覆盖的情况

        public static Modifier operator +(Modifier a, Modifier b)
        {   //定义Modifier对于操作符"+"的行为
            return new Modifier()
            {
                Additive = a.Additive + b.Additive,
                Multiplicative = a.Multiplicative + b.Multiplicative,
                IsOverrideZero = b.IsOverrideZero,
                Overriding = b.Overriding,
            };
        }

        public override readonly string ToString()
            => $"Additive: {Additive}\nMultiplicative：{Multiplicative}\nIsOverride：{IsOverrideZero}\nOverriding：{Overriding}";
        //ToString()时显示值，readonly使修改不会影响到原Modifier
    }
}
