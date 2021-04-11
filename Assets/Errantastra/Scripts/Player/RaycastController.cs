using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Errantastra
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaycastController : NetworkBehaviour
    {
        public LayerMask collisionMask;

        protected const float skinWidth = 0.1f;
        protected int horizontalRayCount = 4;
        protected int verticalRayCount = 4;

        [HideInInspector]
        protected float horizontalRaySpacing;
        [HideInInspector]
        protected float verticalRaySpacing;

        [HideInInspector]
        protected BoxCollider2D collider_jl;
        protected RaycastOrigins raycastOrigins;

        protected virtual void Awake()
        {
            // have put the collider into Awake so that the camera follow scripts 
            collider_jl = GetComponent<BoxCollider2D>();
        }

        protected virtual void Start()
        {
            CalculateRaySpacing();
        }

        public void UpdateRaycastOrigins()
        {
            Bounds bounds = collider_jl.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        public void CalculateRaySpacing()
        {
            Bounds bounds = collider_jl.bounds;
            bounds.Expand(skinWidth * -2);

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

        public struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }
    }
}
