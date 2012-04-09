using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Entities;
using SkinnedModel;
using AI;

namespace Entities
{
    //Positional Heuristic Types
    public enum EntityPositionState
    {
        KineticArrive,
        KineticFlee,
        SteeringArrive,
        SteeringFlee,
        SteeringWander,
        Attack,
        RangedAttack,
        None
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

    class Zombie : Entity, IObserver
    {
        public const float MAX_DISTANCE = 50;               //Maximum distance at which range will be evaluated
        public const float MAX_PROJECTILE_DISTANCE = 20;    //Maximum distance at which projectile attacks can be made
        public const float MAX_MELEE_DISTANCE = 5;          //Maximum distance at which melee attacks can be made

        public int HealthPoints;
        public int MaxHealth;
        public ZombieType zombieType;

        public bool creep = false;              //Whether or not the Entity should move at half of MaxVelocity
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

        public Weapon MeleeAttack;              //Weapon used for melee attacks
        public Weapon RangedAttack;             //Weapon used for ranged attacks

        public EntityPositionState PosState;    //Movement behavior of the entity
        public EntityOrientationState OrState;  //Orientation behavior of the entity
        public BehaviourState BehaviouralState; //Beahavioural state of the entity

        public AnimationPlayer animationPlayer; // This calculates the Matrices of the animation
        public AnimationClip clip;              // This contains the keyframes of the animation
        public SkinningData skinningData;       // This contains all the skinning data
        public float scale = .1f;               // Scale at which to render the model

        public Action<Entity, Entity> AttackFunction;   // Callback function used when an attack is made

        //flanking data
        public int Targetslot() { return targetslot; }
         
        public int targetslot = -1;
       public Vector3 Position { get; set; }

        public Zombie(int health, int maxHealth, ZombieType type, ref Model model, Action<Entity, Entity> attackFunction)
            : base()
        {
            this.model = model;
            this.HealthPoints = health;
            this.MaxHealth = maxHealth;

            this.MaxVelocity = 0.02f;
            this.MaxAcceleration = 0.2f;
            ArriveRadius = 1.5f;
            FleeRadius = 30;
            TimeToTarget = 0.070f;
            RotationTimeToTarget = 0.00025f;
            InterpolationSpeed = 10;
            TargetRotation = 0.02f;
            SlowRotationThreshold = (float)Math.PI;
            SlowRadiusThreshold = (float)Math.PI * 3;
            MaxRotationSpeed = (float)Math.PI / 12;
            MaxRotationAcceleration = (float)Math.PI;
            
            PosState = EntityPositionState.SteeringWander;
            OrState = EntityOrientationState.Face;
            BehaviouralState = BehaviourState.Wander;

            zombieType = type;
            MeleeAttack = new Weapon(WeaponType.ZombieHands);
            RangedAttack = new Weapon(WeaponType.Vomit);
            this.AttackFunction = attackFunction;

            // Look up our custom skinning information.
            skinningData = (SkinningData)model.Tag;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);
            clip = skinningData.AnimationClips["Take 001"];
            animationPlayer.StartClip(clip);
        }
        
         //Execute entity's action
        public void Update(GameTime gameTime)
        {
            if (animState == AnimationState.Walking)
            {
                animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            }
            else
            {
                animationPlayer.ResetClip();
                animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            }

            PosState = EvaluateBehaviour();
            
            switch (PosState)
            {
                case EntityPositionState.KineticArrive:
                    {
                        KinematicArrive();
                        animState = AnimationState.Walking;
                        Position += Velocity;
                        break;
                    }
                case EntityPositionState.KineticFlee:
                    {
                        if ((Target.Position - this.Position).Length() < FleeRadius)
                        {
                            KinematicFlee();
                            animState = AnimationState.Walking;
                            Position += Velocity;
                        }
                        break;
                    }
                case EntityPositionState.SteeringArrive:
                    {
                        SteeringArrive(creep);
                        animState = AnimationState.Walking;
                        Position += Velocity;
                        break;
                    }
                case EntityPositionState.SteeringFlee:
                    {
                        if ((Target.Position - this.Position).Length() < FleeRadius)
                        {
                            SteeringFlee(creep);
                            animState = AnimationState.Walking;
                            Position += Velocity;
                        }
                        break;
                    }
                case EntityPositionState.SteeringWander:
                    {
                        SteeringWander();
                        animState = AnimationState.Walking;
                        Position += Velocity;
                        break;
                    }
                case EntityPositionState.Attack:
                    {
                        animState = AnimationState.Attacking;
                        SetOrientation(Position - Target.Position);
                        AttackFunction(this, MeleeAttack);
                        break;
                    }
                case EntityPositionState.RangedAttack:
                    {
                        animState = AnimationState.Attacking;
                        SetOrientation(Position - Target.Position);
                        AttackFunction(this, RangedAttack);
                        break;
                    }
                default:
                    {
                        animState = AnimationState.Idle;
                        break;
                    }
            }
        }

        
        // Adjust position state based on current behaviour
        private EntityPositionState EvaluateBehaviour()
        {
            creep = false;
            switch (BehaviouralState)
            {
                case BehaviourState.Wander:
                    {
                        return EntityPositionState.SteeringWander;
                    }
                case BehaviourState.SlowFlee:
                    {
                        creep = true;
                        return EntityPositionState.SteeringFlee;
                    }
                case BehaviourState.RangedPursue:
                    {
                        if ((Position - Target.Position).Length() < MAX_PROJECTILE_DISTANCE)
                        {
                            return EntityPositionState.RangedAttack;
                        }
                        return EntityPositionState.SteeringArrive;
                    }
                case BehaviourState.RangedCreep:
                    {
                        if ((Position - Target.Position).Length() < MAX_PROJECTILE_DISTANCE)
                        {
                            return EntityPositionState.RangedAttack;
                        }
                        creep = true;
                        return EntityPositionState.SteeringArrive;
                    }
                case BehaviourState.MeleePursue:
                    {
                        if ((Position - Target.Position).Length() < MAX_MELEE_DISTANCE)
                        {
                            return EntityPositionState.Attack;
                        }
                        // TODO: check if close enough to reserve a slot and set slot as target
                        return EntityPositionState.SteeringArrive;
                    }
                case BehaviourState.MeleeCreep:
                    {
                        creep = true;
                        return EntityPositionState.SteeringArrive;
                    }
                case BehaviourState.Flee:
                    {
                        return EntityPositionState.SteeringFlee;
                    }
                default:
                    {
                        return EntityPositionState.None;
                    }
            }
        }

        // Set a target for the Zombie if he doesn't already  have one
        public void Alert(Entity target)
        {
            if (Target == null)
            {
                this.Target = target;
                Fuzzifier fuzz = new Fuzzifier(HealthPoints / MaxHealth, (target as Hero).HealthPoints / (target as Hero).MaxHealth,
                    (Position - target.Position).Length());
                BehaviouralState = fuzz.GetBehaviour();
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
        private void SteeringArrive(bool creep = false)
        {
            Vector3 newDirection = Target.Position - this.Position;

            float Distance = newDirection.Length();
            float TargetSpeed = 0;

            if (Distance < ArriveRadius)
            {
                Velocity = new Vector3(0);
                return;
            }

            float adjustedMaxVelocity = (creep) ? MaxVelocity / 2 : MaxVelocity;
            if (Distance > SlowRadiusThreshold)
            {
                TargetSpeed = adjustedMaxVelocity;
            }
            else
            {
                TargetSpeed = adjustedMaxVelocity * Distance / SlowRadiusThreshold;
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
        private void SteeringFlee(bool creep = false)
        {
            Vector3 Direction = this.Position - Target.Position;

            //Make it a unit vector
            Direction.Normalize();

            float adjustedMaxVelocity = (creep) ? MaxVelocity / 2 : MaxVelocity;
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
        //code for flanking
        public void Notify(Vector3 value)
        {
            GroundTarget = value;
        }
    }
}
