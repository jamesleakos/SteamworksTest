using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using HeathenEngineering.Tools;

namespace HeathenEngineering.UIX
{
    [AddComponentMenu("Heathen/Generic/Game Object Collection")]
    public class TransformCollection : HeathenBehaviour, ICollection<Transform>
    {
        public int Count { get { return selfTransform.childCount; } }

        public bool IsReadOnly { get { return false; } }

        public Transform this[int index]
        {
            get { return selfTransform.GetChild(index); }
        }
                
        public int IndexOf(GameObject element)
        {
            return IndexOf(element.transform);
        }

        public int IndexOf(Transform element)
        {
            if (element.parent != this)
                return -1;
            else
                return element.GetSiblingIndex();
        }
        
        public void Add(Transform item)
        {
            item.SetParent(selfTransform);
        }

        /// <summary>
        /// [warning] this will destroy all game objects in the collection.
        /// If you wish to remove all child objects without destroying them, then you must reparent each manually.
        /// </summary>
        public void Clear()
        {
            foreach (Transform t in selfTransform)
                Destroy(t.gameObject);
        }

        public bool Contains(Transform item)
        {
            if (item.parent == selfTransform)
                return true;
            else
                return false;
        }

        public void CopyTo(Transform[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array is null");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex is less than 0");
            if (array.Length - arrayIndex < selfTransform.childCount)
                throw new ArgumentException("The number of elements in the collection is greater than the available space from " + arrayIndex.ToString() + " to the end of the destination array");

            for (int i = 0; i < selfTransform.childCount; i++)
                array[arrayIndex + i] = selfTransform.GetChild(i);
        }

        /// <summary>
        /// [warning] this will destroy the game object for item.
        /// If you with to remove it from the collection without destroying it then you need to reparent it.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(Transform item)
        {
            if (Contains(item))
            {
                Destroy(item.gameObject);
                return true;
            }
            else
                return false;
        }

        IEnumerator<Transform> IEnumerable<Transform>.GetEnumerator()
        {
            return selfTransform.GetEnumerator() as IEnumerator<Transform>;
        }

        public IEnumerator GetEnumerator()
        {
            return selfTransform.GetEnumerator();
        }
    }
}
