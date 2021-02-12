using HeathenEngineering.Scriptable;
using System;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [Serializable]
    public class UguiFontStyleReference : VariableReference<FontStyle>
    {
        public UguiFontStylePointerVariable Variable;
        public override IDataVariable<FontStyle> m_variable => Variable;

        public UguiFontStyleReference(FontStyle value) : base(value)
        { }
    }
}
