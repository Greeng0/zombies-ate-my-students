using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Collisions
{
    //Identifies the type of Contact
    enum CollisionType
    {
        VertexFace,
        FaceFace,
        EdgeFace,
        POVRay
    }

    class Contact
    {
        //Normal given by the collision of two objects
        protected Vector3 contactNormal;

        public Vector3 ContactNormal
        {
            get { return contactNormal; }
            set { contactNormal = value; }
        }

        //Point Of Impact
        protected Vector3 contactPoint;

        public Vector3 ContactPoint
        {
            get { return contactPoint; }
            set { contactPoint = value; }
        }

        //Depth of interpenetration
        protected float penetrationDepth;

        public float PenetrationDepth
        {
            get { return penetrationDepth; }
            set { penetrationDepth = value; }
        }

        //Point at the deepest interpenetration point
        protected Vector3 deepestPoint;

        public Vector3 DeepestPoint
        {
            get { return deepestPoint; }
            set { deepestPoint = value; }
        }

        private CollisionType contactType;

        public CollisionType ContactType
        {
            get { return contactType; }
            set { contactType = value; }
        }

        //Returns whether 1,2 or no rays collided in a POVray test
        protected RayCollisionResult rayCollided;

        internal RayCollisionResult RayCollided
        {
            get { return rayCollided; }
            set { rayCollided = value; }
        }

        //POVRay angle between Ray and surface normal
        protected float contactAngle;

        public float ContactAngle
        {
            get { return contactAngle; }
            set { contactAngle = value; }
        }

        public Contact()
        {
            ContactNormal = Vector3.Zero;
            ContactPoint = Vector3.Zero;
            PenetrationDepth = 0;
            rayCollided = RayCollisionResult.None;
        }
    }
}
