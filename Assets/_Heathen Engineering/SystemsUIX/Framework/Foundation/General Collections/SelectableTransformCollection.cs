using UnityEngine;
using HeathenEngineering.Scriptable;
using HeathenEngineering.Events;

namespace HeathenEngineering.UIX
{
    [AddComponentMenu("Heathen/Generic/Select Managed Game Object Collection")]
    public class SelectableTransformCollection : TransformCollection
    {
        [Tooltip(@"If true the index will 'roll' over when it exceeds count otherwise it will clamp between 0 and count.
Curved index cannot select nothing unless the collection is empty.")]
        public BoolReference CurveIndex = new BoolReference(false);
        [SerializeField]
        private IntReference selectedIndex = new IntReference(-1);
        public IntReference SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    ValidateIndex();
                    SelectionChanged.Invoke(SelectedChild);
                }
            }
        }
        public Transform SelectedChild
        {
            get
            {
                ValidateIndex();
                if (selfTransform.childCount > 0)
                    return selfTransform.GetChild(SelectedIndex);
                else
                    return null;
            }
            set
            {
                if (value != null)
                {
                    SelectTransform(value);
                }
            }
        }

        public UnityTransformEvent SelectionChanged;
                
        private void Update()
        {
            ValidateIndex();
        }

        public void ValidateIndex()
        {
            if (selfTransform.childCount == 0)
            {
                SelectedIndex.Value = -1;
                return;
            }

            if (CurveIndex)
            {
                while (SelectedIndex < 0)
                    SelectedIndex.Value += selfTransform.childCount;
                while (SelectedIndex > selfTransform.childCount - 1)
                    SelectedIndex.Value -= selfTransform.childCount;
            }
            else
            {
                if (SelectedIndex != -1)
                    SelectedIndex.Value = Mathf.Clamp(SelectedIndex, 0, selfTransform.childCount - 1);
            }
        }
        
        public void SelectObject(GameObject element)
        {
            if (element != null)
                SelectTransform(element.transform);
        }

        public void SelectTransform(Transform element)
        {
            if (element == null)
                return;

            if (element.parent == this)
                SelectedIndex.Value = element.GetSiblingIndex();
        }
    }
}
