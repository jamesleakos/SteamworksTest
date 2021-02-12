using System;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [Serializable]
    public class SortingLayerValue
    {
        public string name;
        public int id = 0;
        public int value = 0;
        public SortingLayer control
        {
            set
            {
                name = value.name;
                id = value.id;
                this.value = value.value;
            }
            get
            {
                return SortingLayer.IsValid(id) ? SortingLayer.layers.First(l => l.id == id) : SortingLayer.layers[0];
            }
        }

        public SortingLayerValue()
        {

        }

        public SortingLayerValue(SortingLayer layer)
        {
            control = layer;
        }

        public static implicit operator int(SortingLayerValue value)
        {
            return value.id;
        }

        public static implicit operator SortingLayerValue(int id)
        {
            return new SortingLayerValue(SortingLayer.IsValid(id) ? SortingLayer.layers.First(l => l.id == id) : SortingLayer.layers[0]);
        }

        public static int GetLayerValueFromID(int Id)
        {
            return SortingLayer.GetLayerValueFromID(Id);
        }

        public static int GetLayerValueFromName(string name)
        {
            return SortingLayer.GetLayerValueFromName(name);
        }

        public static string IDToName(int Id)
        {
            return SortingLayer.IDToName(Id);
        }
    }
}
