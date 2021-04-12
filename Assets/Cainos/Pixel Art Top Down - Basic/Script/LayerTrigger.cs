using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Errantastra;

namespace Cainos.PixelArtTopDown_Basic
{
    //when object exit the trigger, put it to the assigned layer and sorting layers
    //used in the stair objects for player to travel between layers
    public class LayerTrigger : MonoBehaviour
    {
        public string layer;
        public string sortingLayer;

        private void OnTriggerExit2D(Collider2D other)
        {
            other.gameObject.layer = LayerMask.NameToLayer("Player " + layer);

            SpriteRenderer[] srs = other.gameObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach ( SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }

            RaycastController rc = other.gameObject.GetComponent<RaycastController>();
            if (rc != null)
            {
                rc.collisionMask = LayerMask.NameToLayer(layer);
                Debug.Log(rc.collisionMask.value);
            }
            else Debug.Log("No raycast controller on object with name " + other.transform.name);
        }

    }
}
