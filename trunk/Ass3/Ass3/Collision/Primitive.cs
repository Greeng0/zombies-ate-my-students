using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Collisions
{
    enum PrimitiveType
    {
        Line,
        Plane,
        Sphere,
        Box
    }

    //Type of POVRay collision result for a 2x Ray cast
    enum RayCollisionResult
    {
        None,
        Left,
        Right,
        Both
    }

    //Primitive Objects abstract representation for game logic, visual display and collision detection
    class Primitive
    {
        //Ctor
        public Primitive()
        {
            offset = Matrix.Identity;
            mass = 1.0f;
        }

        //Object Mass for Collision Resolution
        protected float mass;

        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        //Matrix used for Rotations, scalings, etc... 
        protected Matrix offset;

        public Matrix Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        protected PrimitiveType type;
        public PrimitiveType Type
        {
            get { return type; }
            set { type = value; }
        }

        //World position of object
        protected Vector3 position;
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set { position = value; }
        }

        //Speed and Direction of object
        protected Vector3 velocity;

        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        //Returns vertices to display for a single contact from the contact point to the deepest penetration point
        public static VertexPositionColor[] GetPenetrationVector(Contact contact)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[2];
            vertices[0] = new VertexPositionColor(contact.DeepestPoint, Color.Yellow);
            vertices[1] = new VertexPositionColor(contact.DeepestPoint + (contact.ContactNormal * contact.PenetrationDepth), Color.Yellow);

            return vertices;
        }

        //Returns vertices to display for multiple contacts from the contact point to the deepest penetration point
        public static VertexPositionColor[] GetPenetrationVector(List<Contact> contacts)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[contacts.Count * 2];

            for (int i = 0; i < contacts.Count; i++)
            {
                vertices[i * 2] = new VertexPositionColor(contacts[i].DeepestPoint, Color.Yellow);
                vertices[(i * 2) + 1] = new VertexPositionColor((contacts[i].ContactNormal * contacts[i].PenetrationDepth) + contacts[i].DeepestPoint, Color.Yellow);
            }

            return vertices;
        }

        #region Sphere --> X Collisions

        //Sphere --> Sphere Collision Detection and Contact Info generation
        //Returns null if no collision, a contact object if a collision occurs
        protected Contact Collides(Sphere sphere1, Sphere sphere2)
        {
            if ((sphere1.Position - sphere2.Position).Length() <= (sphere1.Radius + sphere2.Radius))
            {
                Contact contact = new Contact();
                contact.ContactType = CollisionType.FaceFace;
                contact.ContactNormal = (sphere1.Position - sphere2.Position) / (sphere1.Position - sphere2.Position).Length();
                Vector3 Midline = (sphere1.Position - sphere2.Position);
                contact.ContactPoint = sphere2.Position + (Midline / Midline.Length() * sphere2.Radius);

                contact.DeepestPoint = (sphere1.Position - (Midline / Midline.Length() * sphere1.Radius));
                contact.PenetrationDepth = sphere1.Radius + sphere2.Radius - Midline.Length();

                return contact;
            }

            //No Collision
            return null;
        }

        //Sphere --> Plane Collision Detection and Contact Info generation
        //Returns null if no collision, a contact object if a collision occurs
        protected Contact Collides(Sphere sphere, Plane plane)
        {
            //Remove the absolute value to allow full penetration of the sphere
            //float Distance = Vector3.Dot(plane.Normal, sphere.position - plane.position) / plane.Normal.Length();
            float Distance = Math.Abs(Vector3.Dot(plane.Normal, sphere.Position - plane.Position)) / plane.Normal.Length();

            if (Distance <= sphere.Radius)
            {
                Contact contact = new Contact();
                contact.ContactType = CollisionType.FaceFace;
                contact.ContactNormal = plane.Normal;
                contact.PenetrationDepth = sphere.Radius - Distance;
                contact.ContactPoint = sphere.Position - contact.ContactNormal * Distance;
                contact.DeepestPoint = sphere.Position - contact.ContactNormal * (Distance + contact.PenetrationDepth);

                return contact;
            }

            //No Collision
            return null;
        }

        #region Universal Sphere/Box Collide
        //Detects sphere--> box collision very well but provides no details for contact info generation
        /*protected bool Collides(Sphere sphere, Box box)
        {
            Vector3[] vertices = box.GetVertices();

            //Implementation based on pages 644-645 of Geometric Tools for Computer Graphics [Philip J. Schneider & David H. Eberly, Morgan Kaufmann]
            float[] Distances = new float[8];

            for (int i = 0; i < vertices.Length; i++)
            {
                Distances[i] = (vertices[i] - sphere.Position).Length();
            }

            Vector3 min = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);
            Vector3 max = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);

            for (int i = 0; i < vertices.Length; i++)
            {
                if (min.X > vertices[i].X)
                    min.X = vertices[i].X;

                if (min.Y > vertices[i].Y)
                    min.Y = vertices[i].Y;

                if (min.Z > vertices[i].Z)
                    min.Z = vertices[i].Z;

                if (max.X < vertices[i].X)
                    max.X = vertices[i].X;

                if (max.Y < vertices[i].Y)
                    max.Y = vertices[i].Y;

                if (max.Z < vertices[i].Z)
                    max.Z = vertices[i].Z;
            }

            double dSquared = 0;

            if (sphere.Position.X < min.X)
            {
                dSquared += (float)Math.Pow((sphere.Position.X - min.X), 2);
            }
            else if (sphere.Position.X > max.X)
            {
                dSquared += (float)Math.Pow((sphere.Position.X - max.X), 2);
            }

            if (sphere.Position.Y < min.Y)
            {
                dSquared += (float)Math.Pow((sphere.Position.Y - min.Y), 2);
            }
            else if (sphere.Position.Y > max.Y)
            {
                dSquared += (float)Math.Pow((sphere.Position.Y - max.Y), 2);
            }

            if (sphere.Position.Z < min.Z)
            {
                dSquared += (float)Math.Pow((sphere.Position.Z - min.Z), 2);
            }
            else if (sphere.Position.Z > max.Z)
            {
                float test = sphere.Position.Z - max.Z;
                dSquared += (float)Math.Pow((sphere.Position.Z - max.Z), 2);
            }

            if (dSquared <= Math.Pow(sphere.Radius, 2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }*/

        #endregion

        //Sphere --> Box Collision Detection and Contact Info generation
        //Returns null if no collision, a contact object if a collision occurs
        protected Contact Collides(Sphere sphere, Box box)
        {
            Contact contact = new Contact();
            Vector3[] vertices = box.GetVertices();


            //Implementation based on pages 644-645 of Geometric Tools for Computer Graphics [Philip J. Schneider & David H. Eberly, Morgan Kaufmann]
            //Make sure the sphere is touching the box before continuing
            
            //Distance from sphere to each of the box vertices
            float[] VertexDistances = new float[8];

            //Compute distance
            for (int i = 0; i < vertices.Length; i++)
            {
                VertexDistances[i] = (vertices[i] - sphere.Position).Length();
            }

            //Obtain Minimum and Maximum values for X,Y,Z
            Vector3 min = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);
            Vector3 max = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);


            for (int i = 0; i < vertices.Length; i++)
            {
                if (min.X > vertices[i].X)
                    min.X = vertices[i].X;

                if (min.Y > vertices[i].Y)
                    min.Y = vertices[i].Y;

                if (min.Z > vertices[i].Z)
                    min.Z = vertices[i].Z;

                if (max.X < vertices[i].X)
                    max.X = vertices[i].X;

                if (max.Y < vertices[i].Y)
                    max.Y = vertices[i].Y;

                if (max.Z < vertices[i].Z)
                    max.Z = vertices[i].Z;
            }

            //Test for collision
            double dSquared = 0;

            if (sphere.Position.X < min.X)
            {
                dSquared += (float)Math.Pow((sphere.Position.X - min.X), 2);
            }
            else if (sphere.Position.X > max.X)
            {
                dSquared += (float)Math.Pow((sphere.Position.X - max.X), 2);
            }

            if (sphere.Position.Y < min.Y)
            {
                dSquared += (float)Math.Pow((sphere.Position.Y - min.Y), 2);
            }
            else if (sphere.Position.Y > max.Y)
            {
                dSquared += (float)Math.Pow((sphere.Position.Y - max.Y), 2);
            }

            if (sphere.Position.Z < min.Z)
            {
                dSquared += (float)Math.Pow((sphere.Position.Z - min.Z), 2);
            }
            else if (sphere.Position.Z > max.Z)
            {
                float test = sphere.Position.Z - max.Z;
                dSquared += (float)Math.Pow((sphere.Position.Z - max.Z), 2);
            }

            if (dSquared <= Math.Pow(sphere.Radius, 2))
            {

                #region Vertex-Face

                foreach (Vector3 vertex in vertices)
                {
                    float Distance = (sphere.Position - vertex).Length();

                    if (Distance <= sphere.Radius)
                    {
                        Vector3 midline = vertex - sphere.Position;
                        contact.ContactType = CollisionType.VertexFace;
                        contact.ContactNormal = -midline / midline.Length();
                        contact.ContactPoint = vertex;// sphere.position + (midline * sphere.Radius) / midline.Length();
                        contact.DeepestPoint = sphere.Position + (midline * sphere.Radius) / midline.Length();
                        contact.PenetrationDepth = ((vertex - contact.DeepestPoint)).Length();

                        return contact;
                    }
                }
                #endregion

                #region Face-Face

                Vector3[] normals = box.GetNormals();
                float[] Distances = new float[6];
                for (int i = 0; i < normals.Length; i++)
                {
                    Distances[i] = Vector3.Dot(normals[i], sphere.Position - (box.Position + (normals[i] * box.Size / 2))) / normals[i].Length();
                }

                int index = 0;

                //The number of faces whose normals are not pointing away from the sphere.
                int PositiveCount = 0;

                for (int i = 0; i < Distances.Length; i++)
                {
                    if (Distances[i] > 0)
                    {
                        if (Distances[i] <= sphere.Radius)
                        {
                            PositiveCount++;
                        }

                        if (Distances[index] > Distances[i] || Distances[index] < 0)
                        {
                            index = i;
                        }
                    }
                }



                if (Distances[index] <= sphere.Radius && PositiveCount == 1)
                {
                    contact = new Contact();
                    contact.ContactType = CollisionType.FaceFace;
                    contact.ContactNormal = normals[index];
                    contact.PenetrationDepth = sphere.Radius - Distances[index]; //+

                    contact.ContactPoint = sphere.Position - contact.ContactNormal * VertexDistances[index];
                    contact.DeepestPoint = sphere.Position - contact.ContactNormal * (sphere.Radius);

                    return contact;


                }

                #endregion

                #region Edge-Face

                float[] PointDistances = new float[12];
                Vector3[,] VertexPair = new Vector3[12, 2];

                //Front Vertices
                Vector3 x0 = sphere.Position;
                Vector3 x1 = VertexPair[0, 0] = vertices[0];
                Vector3 x2 = VertexPair[0, 1] = vertices[1];

                PointDistances[0] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[1, 0] = vertices[1];
                x2 = VertexPair[1, 1] = vertices[3];

                PointDistances[1] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[2, 0] = vertices[3];
                x2 = VertexPair[2, 1] = vertices[2];

                PointDistances[2] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[3, 0] = vertices[2];
                x2 = VertexPair[3, 1] = vertices[0];

                PointDistances[3] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                //Back Vertices
                x1 = VertexPair[4, 0] = vertices[4];
                x2 = VertexPair[4, 1] = vertices[5];

                PointDistances[4] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[5, 0] = vertices[5];
                x2 = VertexPair[5, 1] = vertices[7];

                PointDistances[5] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[6, 0] = vertices[7];
                x2 = VertexPair[6, 1] = vertices[6];

                PointDistances[6] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[7, 0] = vertices[6];
                x2 = VertexPair[7, 1] = vertices[4];

                PointDistances[7] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                //Side Vertices
                x1 = VertexPair[8, 0] = vertices[0];
                x2 = VertexPair[8, 1] = vertices[4];

                PointDistances[8] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[9, 0] = vertices[1];
                x2 = VertexPair[9, 1] = vertices[5];

                PointDistances[9] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[10, 0] = vertices[2];
                x2 = VertexPair[10, 1] = vertices[6];

                PointDistances[10] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                x1 = VertexPair[11, 0] = vertices[3];
                x2 = VertexPair[11, 1] = vertices[7];

                PointDistances[11] = Vector3.Cross((x0 - x1), (x0 - x2)).Length() / (x2 - x1).Length();

                index = 0;

                for (int i = 0; i < PointDistances.Length; i++)
                {
                    if (PointDistances[index] > PointDistances[i])
                    {
                        index = i;
                    }
                }

                if (PointDistances[index] < sphere.Radius)
                {
                    x1 = VertexPair[index, 0];
                    x2 = VertexPair[index, 1];

                    //Required to compute point on the line
                    float t = -Vector3.Dot((x1 - x0), (x2 - x1)) / (x2 - x1).LengthSquared();

                    Vector3 LinePoint = new Vector3(x1.X + (x2.X - x1.X) * t, x1.Y + (x2.Y - x1.Y) * t, x1.Z + (x2.Z - x1.Z) * t);
                    contact.ContactType = CollisionType.EdgeFace;
                    contact.ContactNormal = Vector3.Normalize(x0 - LinePoint);
                    contact.ContactPoint = LinePoint;
                    contact.DeepestPoint = x0 - contact.ContactNormal / contact.ContactNormal.Length() * sphere.Radius;
                    contact.PenetrationDepth = (contact.DeepestPoint - contact.ContactPoint).Length();

                    return contact;
                }
                #endregion

            }
            //No Contact found...
            return null;
        }


        #endregion

        #region Box --> X Collisions

        //Box --> Plane Collision Detection and Contact Info generation
        //Returns null if no collision, a contact List object if a collision occurs
        protected List<Contact> Collides(Box box, Plane plane)
        {
            List<Contact> contacts = new List<Contact>();
            Vector3[] Vertices = box.GetVertices();

            foreach (Vector3 vertex in Vertices)
            {
                float Distance = Vector3.Dot(plane.Normal, vertex - plane.Position) / plane.Normal.Length();

                if (Distance <= 0)
                {
                    Contact contact = new Contact();

                    contact.ContactType = CollisionType.VertexFace;
                    contact.ContactNormal = plane.Normal;
                    contact.PenetrationDepth = -Distance;
                    contact.ContactPoint = vertex;
                    contact.DeepestPoint = vertex;

                    contacts.Add(contact);
                }
            }

            return contacts;
        }

        #endregion
    }

    //Sphere Primitive
    class Sphere : Primitive
    {
        //Sphere Radius
        protected float radius;

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        //Ctor
        public Sphere(Vector3 pos, Vector3 velocity, float radius = 1)
        {
            Type = PrimitiveType.Sphere;
            Position = pos;
            this.velocity = velocity;
            this.radius = radius;
        }


        #region Collision functions
        
        //Test for Sphere --> Sphere Collision
        public Contact Collides(Sphere sphere)
        {
            return base.Collides(this, sphere);
        }

        //Test for Plane --> Sphere Collision
        public Contact Collides(Plane plane)
        {
            return base.Collides(this, plane);
        }

        //Test for Box --> Sphere Collision
        public Contact Collides(Box box)
        {
            return base.Collides(this, box);
        }

        #endregion

        //Point-Of-View Ray Casting collision detection for Boxes
        public List<Contact> ProjectPOVRay(Box box, float RayLength)
        {
            List<Contact> Contacts = new List<Contact>();

            Vector3 sideOffset = Vector3.Normalize(Vector3.Cross(this.Velocity, new Vector3(0, 1, 0)));

            Vector3 side1 = this.Position - sideOffset;
            Vector3 side2 = this.Position + sideOffset;

            Vector3[] vertices = box.GetVertices();

            Vector3[] normals = box.GetNormals();

            float[] Distances = new float[6];
            for (int i = 0; i < normals.Length; i++)
            {
                Distances[i] = Vector3.Dot(((box.Position + (normals[i] * box.Size / 2) - side1)), normals[i]) / Vector3.Dot(this.velocity, normals[i]);
            }

            //The number of faces whose normals are not pointing away from the sphere.
            int PositiveCount = 0;

            List<int> indices = new List<int>();

            //Find shortest distance to Box Face
            int shortestIndex = -1;

            //Obtain list of faces whose normals are not pointing away from he sphere and the distance between the sphere and the closest box face
            for (int i = 0; i < Distances.Length; i++)
            {
                if (Distances[i] > 0)
                {
                    if (Distances[i] <= RayLength)
                    {
                        indices.Add(i);
                        PositiveCount++;

                        if (shortestIndex == -1)
                        {
                            shortestIndex = i;
                        }
                        else
                        {
                            if (Distances[shortestIndex] > Distances[i])
                            {
                                shortestIndex = i;
                            }
                        }
                    }
                    else
                    {
                        Distances[i] = -1;
                    }
                }
                else
                {
                    Distances[i] = -1;
                }
            }

            //Test For Left Ray
            //Don't collide if many positives are return, false positive
            if (PositiveCount > 0)
            {
                foreach (int index in indices)
                {
                    //Check for validity with point intersecting the box
                    Vector3 RayPoint = side1 + velocity / velocity.Length() * Distances[index];

                    Vector3 min = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);
                    Vector3 max = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        if (min.X > vertices[i].X)
                            min.X = vertices[i].X;

                        if (min.Y > vertices[i].Y)
                            min.Y = vertices[i].Y;

                        if (min.Z > vertices[i].Z)
                            min.Z = vertices[i].Z;

                        if (max.X < vertices[i].X)
                            max.X = vertices[i].X;

                        if (max.Y < vertices[i].Y)
                            max.Y = vertices[i].Y;

                        if (max.Z < vertices[i].Z)
                            max.Z = vertices[i].Z;
                    }

                    double dSquared = 0;

                    if (RayPoint.X < min.X)
                    {
                        dSquared += (float)Math.Pow((RayPoint.X - min.X), 2);
                    }
                    else if (RayPoint.X > max.X)
                    {
                        dSquared += (float)Math.Pow((RayPoint.X - max.X), 2);
                    }

                    if (RayPoint.Y < min.Y)
                    {
                        dSquared += (float)Math.Pow((RayPoint.Y - min.Y), 2);
                    }
                    else if (RayPoint.Y > max.Y)
                    {
                        dSquared += (float)Math.Pow((RayPoint.Y - max.Y), 2);
                    }

                    if (RayPoint.Z < min.Z)
                    {
                        dSquared += (float)Math.Pow((RayPoint.Z - min.Z), 2);
                    }
                    else if (RayPoint.Z > max.Z)
                    {
                        dSquared += (float)Math.Pow((RayPoint.Z - max.Z), 2);
                    }

                    if (dSquared <= 0.01)
                    {
                        Contact contact = new Contact();
                        contact.ContactType = CollisionType.POVRay;
                        contact.RayCollided = RayCollisionResult.Left;
                        contact.ContactNormal = -velocity / Velocity.Length();
                        contact.ContactPoint = RayPoint;
                        contact.DeepestPoint = side1 + velocity / Velocity.Length() * RayLength;
                        contact.PenetrationDepth = (contact.DeepestPoint - contact.ContactPoint).Length();
                        
                        //Compute contact angle
                        if (PositiveCount == 1)
                        {
                            float scalar = Vector3.Dot(normals[shortestIndex], velocity / velocity.Length());
                            contact.ContactAngle = (float)Math.Acos(scalar);

                        }
                        else
                        {
                            float scalar = Vector3.Dot(normals[shortestIndex], velocity / velocity.Length());
                            contact.ContactAngle = (float)Math.Acos(scalar);

                        }

                        //Angle should never be higher than 90 degrees.
                        if (contact.ContactAngle > Math.PI / 2)
                        {
                            contact.ContactAngle = (float)Math.PI - contact.ContactAngle;
                        }

                        Contacts.Add(contact);
                        break;
                    }
                }
            }

            Distances = new float[6];
            for (int i = 0; i < normals.Length; i++)
            {
                Distances[i] = Vector3.Dot(((box.Position + (normals[i] * box.Size / 2) - side2)), normals[i]) / Vector3.Dot(this.velocity, normals[i]);
            }

            //The number of faces whose normals are not pointing away from the sphere.
            PositiveCount = 0;
            indices = new List<int>();

            //Obtain list of faces whose normals are not pointing away from he sphere and the distance between the sphere and the closest box face
            shortestIndex = 0;
            for (int i = 0; i < Distances.Length; i++)
            {
                if (Distances[i] > 0)
                {
                    if (Distances[i] <= RayLength)
                    {
                        indices.Add(i);
                        PositiveCount++;

                        if (shortestIndex == -1)
                        {
                            shortestIndex = i;
                        }
                        else
                        {
                            if (Distances[shortestIndex] > Distances[i])
                            {
                                shortestIndex = i;
                            }
                        }
                    }
                    else
                    {
                        Distances[i] = -1;
                    }
                }
                else
                {
                    Distances[i] = -1;
                }
            }


            //Test For Right Ray
            //Don't collide if many positives are return, false positive
            if (PositiveCount > 0)
            {
                foreach (int index in indices)
                {
                    //Check for validity with point intersecting the box
                    Vector3 RayPoint = side2 + velocity / velocity.Length() * Distances[index];

                    Vector3 min = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);
                    Vector3 max = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        if (min.X > vertices[i].X)
                            min.X = vertices[i].X;

                        if (min.Y > vertices[i].Y)
                            min.Y = vertices[i].Y;

                        if (min.Z > vertices[i].Z)
                            min.Z = vertices[i].Z;

                        if (max.X < vertices[i].X)
                            max.X = vertices[i].X;

                        if (max.Y < vertices[i].Y)
                            max.Y = vertices[i].Y;

                        if (max.Z < vertices[i].Z)
                            max.Z = vertices[i].Z;
                    }

                    double dSquared = 0;

                    if (RayPoint.X < min.X)
                    {
                        dSquared += (float)Math.Pow((RayPoint.X - min.X), 2);
                    }
                    else if (RayPoint.X > max.X)
                    {
                        dSquared += (float)Math.Pow((RayPoint.X - max.X), 2);
                    }

                    if (RayPoint.Y < min.Y)
                    {
                        dSquared += (float)Math.Pow((RayPoint.Y - min.Y), 2);
                    }
                    else if (RayPoint.Y > max.Y)
                    {
                        dSquared += (float)Math.Pow((RayPoint.Y - max.Y), 2);
                    }

                    if (RayPoint.Z < min.Z)
                    {
                        dSquared += (float)Math.Pow((RayPoint.Z - min.Z), 2);
                    }
                    else if (RayPoint.Z > max.Z)
                    {
                        dSquared += (float)Math.Pow((RayPoint.Z - max.Z), 2);
                    }

                    if (dSquared <= 0.01)
                    {
                        Contact contact = new Contact();
                        contact.ContactType = CollisionType.POVRay;
                        contact.RayCollided = RayCollisionResult.Right;
                        contact.ContactNormal = -velocity / Velocity.Length();
                        contact.ContactPoint = RayPoint;
                        contact.DeepestPoint = side2 + velocity / Velocity.Length() * RayLength;
                        contact.PenetrationDepth = (contact.DeepestPoint - contact.ContactPoint).Length();

                        //Compute contact angle
                        if (PositiveCount == 1)
                        {
                            float scalar = Vector3.Dot(normals[shortestIndex], velocity / velocity.Length());
                            contact.ContactAngle = (float)Math.Acos(scalar);

                        }
                        else
                        {
                            float scalar = Vector3.Dot(normals[shortestIndex], velocity / velocity.Length());
                            contact.ContactAngle = (float)Math.Acos(scalar);
                        }

                        //Angle should never be higher than 90 degrees.
                        if (contact.ContactAngle > Math.PI / 2)
                        {
                            contact.ContactAngle = (float)Math.PI - contact.ContactAngle;
                        }

                        Contacts.Add(contact);
                        break;
                    }
                }
            }

            //If contacts exist, collisions were found else return nothing
            if (Contacts.Count > 0)
            {
                return Contacts;
            }
            else
            {
                return null;
            }
        }
    }

    //Plane
    class Plane : Primitive
    {
        //Bounds of the plane
        protected Vector2 size;

        public Vector2 Size
        {
            get { return size; }
            set { size = value; }
        }

        //Plane Normal
        protected Vector3 normal;

        public Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }

        //Ctor
        public Plane(Vector3 pos, Vector3 velocity, Vector2 size, Vector3 normal)
        {
            Type = PrimitiveType.Plane;
            Position = pos;
            this.velocity = velocity;
            this.size = size;
            this.normal = normal;
        }

        #region Collision Functions
        
        public Contact Collides(Sphere sphere)
        {
            return base.Collides(sphere, this);
        }

        public List<Contact> Collides(Box box)
        {
            return base.Collides(box, this);
        }

        #endregion
    }

    //Box primitive class
    class Box : Primitive
    {
        //Position with returned offset for aligning box to other objects when displaying/Colliding
        private Vector3 position;

        public Vector3 Position
        {
            get { return Vector3.Transform(position, offset); }
            set { position = value; }
        }

        //the Width, Length and Height of the box
        private Vector3 size;

        public Vector3 Size
        {
            get { return size; }
            set { size = value; }
        }

        private string tag;

        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }


        //Vertices and face Normals
        Vector3[] vertices = new Vector3[8];
        Vector3[] normals = new Vector3[6];

        //Return the 8 Box vertices
        public Vector3[] GetVertices()
        {
            Vector3[] vertices = new Vector3[8];

            vertices[0] = Position + Vector3.Transform(new Vector3(-1 * (size.X * 0.5f), -1 * (size.Y * 0.5f), 1 * (size.Z * 0.5f)), Offset);
            vertices[1] = Position + Vector3.Transform(new Vector3(-1 * (size.X * 0.5f), 1 * (size.Y * 0.5f), 1 * (size.Z * 0.5f)), Offset);
            vertices[2] = Position + Vector3.Transform(new Vector3(1 * (size.X * 0.5f), -1 * (size.Y * 0.5f), 1 * (size.Z * 0.5f)), Offset);
            vertices[3] = Position + Vector3.Transform(new Vector3(1 * (size.X * 0.5f), 1 * (size.Y * 0.5f), 1 * (size.Z * 0.5f)), Offset);
            vertices[4] = Position + Vector3.Transform(new Vector3(-1 * (size.X * 0.5f), -1 * (size.Y * 0.5f), -1 * (size.Z * 0.5f)), Offset);
            vertices[5] = Position + Vector3.Transform(new Vector3(-1 * (size.X * 0.5f), 1 * (size.Y * 0.5f), -1 * (size.Z * 0.5f)), Offset);
            vertices[6] = Position + Vector3.Transform(new Vector3(1 * (size.X * 0.5f), -1 * (size.Y * 0.5f), -1 * (size.Z * 0.5f)), Offset);
            vertices[7] = Position + Vector3.Transform(new Vector3(1 * (size.X * 0.5f), 1 * (size.Y * 0.5f), -1 * (size.Z * 0.5f)), Offset);

            return vertices;
        }

        //Return 6 normals. Since each pair is parallel, 3 could be returned instead
        public Vector3[] GetNormals()
        {
            Vector3[] vertices = new Vector3[6];

            vertices[0] = Vector3.Transform(normals[0], offset);
            vertices[1] = Vector3.Transform(normals[1], offset);
            vertices[2] = Vector3.Transform(normals[2], offset);
            vertices[3] = Vector3.Transform(normals[3], offset);
            vertices[4] = Vector3.Transform(normals[4], offset);
            vertices[5] = Vector3.Transform(normals[5], offset);

            return vertices;
        }

        //Retrieve vertices to display vector normals point out of each face
        public VertexPositionColor[] GetNormalVertices()
        {
            Vector3[] normals = GetNormals();

            VertexPositionColor[] vertices = new VertexPositionColor[12];

            vertices[0] = new VertexPositionColor(Vector3.Zero, Color.BurlyWood);
            vertices[1] = new VertexPositionColor(normals[0], Color.BurlyWood);
            vertices[2] = new VertexPositionColor(Vector3.Zero, Color.Chartreuse);
            vertices[3] = new VertexPositionColor(normals[1], Color.Chartreuse);
            vertices[4] = new VertexPositionColor(Vector3.Zero, Color.FloralWhite);
            vertices[5] = new VertexPositionColor(normals[2], Color.FloralWhite);
            vertices[6] = new VertexPositionColor(Vector3.Zero, Color.Magenta);
            vertices[7] = new VertexPositionColor(normals[3], Color.Magenta);
            vertices[8] = new VertexPositionColor(Vector3.Zero, Color.Yellow);
            vertices[9] = new VertexPositionColor(normals[4], Color.Yellow);
            vertices[10] = new VertexPositionColor(Vector3.Zero, Color.Green);
            vertices[11] = new VertexPositionColor(normals[5], Color.Green);

            return vertices;
        }

        //Ctor
        public Box(Vector3 pos, Vector3 velocity)
        {
            Type = PrimitiveType.Box;
            Position = pos;
            size = new Vector3(1);
            this.velocity = velocity;
        }

        //Ctor
        public Box(Vector3 pos, Vector3 velocity, Vector3 size)
        {
            Type = PrimitiveType.Box;
            Position = pos;
            this.size = size;
            this.velocity = velocity;

            //Should only 3 normals as each pair has opposite normals
            normals[0] = new Vector3(1, 0, 0);
            normals[1] = new Vector3(0, 1, 0);
            normals[2] = new Vector3(0, 0, 1);
            normals[3] = new Vector3(-1, 0, 0);
            normals[4] = new Vector3(0, -1, 0);
            normals[5] = new Vector3(0, 0, -1);
        }

        public List<Contact> Collides(Plane plane)
        {
            return base.Collides(this, plane);
        }
    }
}
