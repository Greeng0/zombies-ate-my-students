using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Entities;


namespace COMP476Project
{
    //Positional Heuristic Types
    public enum EntityPositionState
    {
        KineticArrive,
        KineticFlee,
        SteeringArrive,
        SteeringFlee,
        SteeringWander
    }

    public enum ZombieType
    {
        Adult,
        Kid,
        Boss
    }

    //Orientation Heuristic Types
    public enum EntityOrientationState
    {
        None,
        Interpolated,
        Face
    }

    class Zombie : Entity
    {
        public int HealthPoints;
        public int MaxHealth;
        public ZombieType zombieType;

        public float MaxVelocity;               //Maximum Entity Velocity
        public float MaxAcceleration;           //Maximum Entity Acceleration
        public float ArriveRadius;              //Radius used in Steering arrive. Determine when to stop slowing down 
        public float FleeRadius;                //Radius used in the Flee behaviors. Determines the Maximum flee distance from a target. 
        public float TimeToTarget;              //Determines the movement "time" to target for many behaviors
        public float RotationTimeToTarget;      //Determines the rotation "time" to target for orienting the character smoothly
        public float TargetRotation;            //The target angle at which the entity should rotate
        public float SlowRotationThreshold;     //The angle difference from the target orientation angle for which the entity should slow down.
        public float SlowRadiusThreshold;       //The distance from target for which the entity should start to slow down for the arrive behavior
        public float RotationSpeed;             //Current rotation velocity for the align behavior
        public float MaxRotationSpeed;          //Max rotation velocity for the align behavior
        public float RotationAcceleration;      //Current rotation acceleration for the align behavior
        public float MaxRotationAcceleration;   //Maximum rotation acceleration for the Align behavior
        public float InterpolationSpeed;        //Speed at which the orientation occurs
        
        public Entity Target;                   //Entity Target used for seeking, arriving, pursuing and such behaviors  
        public Vector3 GroundTarget;            //Target used for wandering behavior.

        public EntityPositionState PosState;    //Movement behavior of the entity
        public EntityOrientationState OrState;  //Orientation behavior of the entity

        public Zombie(int health, int maxHealth, ZombieType type)
        {
            this.HealthPoints = health;
            this.MaxHealth = maxHealth;

            this.MaxVelocity = 20f;
            this.MaxAcceleration = 20f;
            ArriveRadius = 1.5f;
            FleeRadius = 15f;
            TimeToTarget = 0.070f;
            RotationTimeToTarget = 0.00025f;
            InterpolationSpeed = 10;
            TargetRotation = 0.02f;
            SlowRotationThreshold = (float)Math.PI;
            SlowRadiusThreshold = (float)Math.PI * 3;
            MaxRotationSpeed = (float)Math.PI / 12;
            MaxRotationAcceleration = (float)Math.PI;
            
            PosState = EntityPositionState.SteeringArrive;
            OrState = EntityOrientationState.Face;

            zombieType = type;
        }

         //Execute entity's action
        public void Update()
        {
            switch (PosState)
            {
                case EntityPositionState.KineticArrive:
                    {
                        KinematicArrive();

                        break;
                    }
                case EntityPositionState.KineticFlee:
                    {
                        if ((Target.Position - this.Position).Length() < FleeRadius)
                        {
                            KinematicFlee();
                        }

                        break;
                    }
                case EntityPositionState.SteeringArrive:
                    {
                        SteeringArrive();
                        break;
                    }
                case EntityPositionState.SteeringFlee:
                    {
                        if ((Target.Position - this.Position).Length() < FleeRadius)
                        {
                            SteeringFlee();
                        }

                        break;
                    }
            }
        }

        //Kinematic Arrive. Player follows target and stops when it touches it
        //Mostly based on Artificial Intelligence For Games 2nd Edition by Millington & Funge
        private void KinematicArrive()
        {
            Vector3 newDirection = Target.Position - this.Position;

            float Distance = newDirection.Length();

            if (Distance < ArriveRadius)
            {
                Velocity = new Vector3(0);

                return;
            }

            newDirection /= TimeToTarget;

            if (newDirection.Length() > MaxVelocity)
            {
                newDirection.Normalize();
                newDirection *= MaxVelocity;
            }

            this.Velocity = newDirection;

            SetOrientation(this.Velocity);
        }

        //Steering Arrive. Player follows target and swill slow down near its target until it touches it
        //Mostly based on Artificial Intelligence For Games 2nd Edition by Millington & Funge
        private void SteeringArrive()
        {
            Vector3 newDirection = Target.Position - this.Position;

            float Distance = newDirection.Length();
            float TargetSpeed = 0;

            if (Distance < ArriveRadius)
            {
                Velocity = new Vector3(0);
                return;
            }

            if (Distance > SlowRadiusThreshold)
            {
                TargetSpeed = MaxVelocity;
            }
            else
            {
                TargetSpeed = MaxVelocity * Distance / SlowRadiusThreshold;
            }

            newDirection.Normalize();
            newDirection *= TargetSpeed;

            newDirection /= TimeToTarget;

            if (newDirection.Length() > MaxAcceleration)
            {
                newDirection.Normalize();
                newDirection *= MaxAcceleration;
            }

            this.Velocity = newDirection;

            SetOrientation(this.Velocity);
        }

        private void SteeringWander()
        {
            //this.Position + this.Direction / this.Direction.Length() * 4;
            Vector3 newDirection = GroundTarget - this.Position;

            float Distance = newDirection.Length();
            float TargetSpeed = 0;

            //Determine if character has reached its target
            if (Distance < ArriveRadius)
            {
                //Assign new random target
                this.GroundTarget = this.Position + (this.Velocity / this.Velocity.Length() * 2.5f) + new Vector3((float)(rand.NextDouble() - rand.NextDouble()), 0, (float)(rand.NextDouble() - rand.NextDouble()));
            }

            //Increase speed if from target and reduce it slowly when near target
            if (Distance > SlowRadiusThreshold)
            {
                TargetSpeed = MaxVelocity;
            }
            else
            {
                TargetSpeed = MaxVelocity * Distance / SlowRadiusThreshold;
            }

            newDirection.Normalize();
            newDirection *= TargetSpeed;

            newDirection /= TimeToTarget;

            //Throttle Acceleration is over maximum allowed
            if (newDirection.Length() > MaxAcceleration)
            {
                newDirection.Normalize();
                newDirection *= MaxAcceleration;
            }

            //Set newly computed direction
            this.Velocity = newDirection;

            //Orient Character based on new direction
            SetOrientation(this.Velocity);
        }

        //Kinematic Flee. Player follows target and stops when it touches it
        //Mostly based on Artificial Intelligence For Games 2nd Edition by Millington & Funge
        private void KinematicFlee()
        {
            Vector3 Direction = this.Position - Target.Position;

            //Make it a unit vector
            Direction.Normalize();

            Direction *= MaxVelocity;

            SetOrientation(Direction);

            this.Velocity = Direction;
        }

        //Same as Kinematic Flee, thus redundant but there only for "distinguishing" between behaviors
        //Steering Flee. Player follows target and stops when it touches it
        //Mostly based on Artificial Intelligence For Games 2nd Edition by Millington & Funge
        private void SteeringFlee()
        {
            Vector3 Direction = this.Position - Target.Position;

            //Make it a unit vector
            Direction.Normalize();

            Direction *= MaxVelocity * 0.75f;

            SetOrientation(Direction);

            this.Velocity = Direction;
        }

        //Align. Player aligns itself with the orientation of the target
        //Mostly based on Artificial Intelligence For Games 2nd Edition by Millington & Funge
        private void Align(double TargetDirectionAngle)
        {
            double newRotation = (TargetDirectionAngle - this.Rotation);

            if ((newRotation > Math.PI || newRotation < -Math.PI))
            {
                newRotation += (newRotation / Math.Abs(newRotation)) * (-2 * Math.PI);
            }

            double RotationSize = Math.Abs(newRotation);

            double TargetRot;

            if (RotationSize < TargetRotation || RotationSize == 0.0)
            {
                return;
            }

            if (RotationSize > SlowRotationThreshold)
            {
                TargetRot = newRotation / Math.Abs(newRotation) * MaxRotationSpeed;
            }
            else
            {
                TargetRot = MaxRotationSpeed * RotationSize / SlowRotationThreshold;

                //Get Proper rotation direction
                TargetRot *= newRotation / RotationSize;

                RotationSpeed = (float)(TargetRot - Rotation) / RotationTimeToTarget;

                RotationAcceleration = Math.Abs(RotationSpeed);
                if (RotationAcceleration > MaxRotationAcceleration)
                {
                    RotationSpeed /= RotationAcceleration;
                    RotationSpeed *= MaxRotationAcceleration;
                }
            }

            Rotation += TargetRot;

            Rotation %= Math.PI * 2;
        }

        //Assign proper orientation to players
        private void SetOrientation(Vector3 newDirection)
        {
            switch (OrState)
            {
                case EntityOrientationState.Face:
                    {
                        Align(Math.Atan2(newDirection.X, newDirection.Z));
                        break;
                    }
                case EntityOrientationState.Interpolated:
                    {
                        double newRotation = Math.Atan2(newDirection.X, newDirection.Z);

                        Rotation += (newRotation - Rotation) / InterpolationSpeed;

                        Rotation %= Math.PI * 2;
                        break;
                    }
                case EntityOrientationState.None:
                default:
                    {
                        Rotation = Math.Atan2(newDirection.X, newDirection.Z);

                        break;
                    }
            }
        }
    }
}
