using HeathenEngineering.Scriptable;
using System;

namespace HeathenEngineering.UIX
{
    [Serializable]
    public class TMProFontReference : VariableReference<TMPro.TMP_FontAsset>
    {
        public TMProFontPointerVariable Variable;
        public override IDataVariable<TMPro.TMP_FontAsset> m_variable => Variable;

        public TMProFontReference(TMPro.TMP_FontAsset value) : base(value)
        { }
    }
}
