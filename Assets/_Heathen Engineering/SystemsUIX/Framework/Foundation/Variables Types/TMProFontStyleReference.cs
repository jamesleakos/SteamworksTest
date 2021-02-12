using HeathenEngineering.Scriptable;
using System;
using TMPro;

namespace HeathenEngineering.UIX
{
    [Serializable]
    public class TMProFontStyleReference : VariableReference<FontStyles>
    {
        public TMProFontStylePointerVariable Variable;
        public override IDataVariable<FontStyles> m_variable => Variable;

        public TMProFontStyleReference(FontStyles value) : base(value)
        { }
    }
}
