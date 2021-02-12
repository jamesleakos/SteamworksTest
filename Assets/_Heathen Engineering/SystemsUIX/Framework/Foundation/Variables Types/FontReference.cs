using HeathenEngineering.Scriptable;
using System;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [Serializable]
    public class FontReference : VariableReference<Font>
    {
        public FontPointerVariable Variable;
        public override IDataVariable<Font> m_variable => Variable;

        public FontReference(Font value) : base(value)
        { }
    }
}
