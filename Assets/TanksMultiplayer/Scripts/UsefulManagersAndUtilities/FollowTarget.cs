﻿/*  This file is part of the "Errantastra" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;

namespace Errantastra
{
    /// <summary>
    /// Camera script for following the player or a different target transform.
    /// Extended with ability to hide certain layers (e.g. UI) while in "follow mode".
    /// </summary>
    public class FollowTarget : MonoBehaviour
    {
        /// <summary>
        /// The camera target to follow.
        /// Automatically picked up in LateUpdate().
        /// </summary>
        public Transform target;
        
        /// <summary>
        /// Layers to hide after calling HideMask().
        /// </summary>
        public LayerMask respawnMask;

        /// <summary>
        /// Reference to the Camera component.
        /// </summary>
        [HideInInspector]
        public Camera cam;
        
        /// <summary>
        /// Reference to the camera Transform.
        /// </summary>
        [HideInInspector]
        public Transform camTransform;
        
        
        //initialize variables
        void Start()
        {
            cam = GetComponent<Camera>();
            camTransform = transform;
        }


        //position the camera in every frame
        void LateUpdate()
        {
            //cancel if we don't have a target
            if (!target)
                return;

            transform.position = new Vector3(target.transform.position.x, target.transform.position.y, camTransform.position.z);

        }


        /// <summary>
        /// Culls the specified layers of 'respawnMask' by the camera.
        /// </summary>
        public void HideMask(bool shouldHide)
        {
            if(shouldHide) cam.cullingMask &= ~respawnMask;
            else cam.cullingMask |= respawnMask;
        }
    }
}