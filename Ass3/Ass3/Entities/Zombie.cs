using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Entities;
using SkinnedModel;
using AI;
using PathFinding;

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

    class Zombie : Entity, IHeroObserver
    {
        public const float MAX_DISTANCE = 50;               //Maximum distance at which range will be evaluated
        public const float MAX_PROJECTILE_DISTANCE = 15;    //Maximum distance at which projectile attacks can be made
        public const float MAX_MELEE_DISTANCE = 5;          //Maximum distance at which melee attacks can be made

        public int HealthPoints;
        public int MaxHealth;
        public ZombieType zombieType;
        public bool Dead = false;

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
        
        public Hero Target;                     //Entity Target used for seeking, arriving, pursuing and such behaviors  
        public Vector3 SlotTarget;              //Target used for slot assignment
        public Vector3 GroundTarget;            //Target used for wandering behavior and pathfinding.

        public Weapon MeleeAttack;              //Weapon used for melee attacks
        public Weapon RangedAttack;             //Weapon used for ranged attacks
        public float lastAttackTime;

        public EntityPositionState PosState;    //Movement behavior of the entity
        public EntityOrientationState OrState;  //Orientation behavior of the entity
        public BehaviourState BehaviouralState; //Beahavioural state of the entity


        //basic state
        public AnimationPlayer animationPlayer; // main animation player

        //walking state
        public AnimationPlayer animationPlayerwalk; // This calculates the Matrices of the animation
        public AnimationClip clipwalk;              // This contains the keyframes of the animation
        public SkinningData skinningDatawalk;       // This contains all the skinning data

        //hurt state
        public const int DAMAGE_ANIM_LENGTH = 1000;
        public int ElapsedDamagedTime = 0;
        public AnimationPlayer animationPlayerhurt; // This calculates the Matrices of the animation
        public AnimationClip cliphurt;              // This contains the keyframes of the animation
        public SkinningData skinningDatahurt;       // This contains all the skinning data

        //die
        public const int DEATH_ANIM_LENGTH = 700;
        public int ElapsedDeathTime = 0;
        public AnimationPlayer animationPlayerdie; // This calculates the Matrices of the animation
        public AnimationClip clipdie;              // This contains the keyframes of the animation
        public SkinningData skinningDatadie;       // This contains all the skinning data

        //attack
        public AnimationPlayer animationPlayerattack; // This calculates the Matrices of the animation
        public AnimationClip clipattack;              // This contains the keyframes of the animation
        public SkinningData skinningDataattack;       // This contains all the skinning data

        public float scale = .1f;               // Scale at which to render the model

        public Action<Entity, Entity> AttackFunction;   // Callback function used when an attack is made

        //flanking data
        public int Targetslot() { return targetslot; }
        public int targetslot = -1;

        //pathfinding
        public List<PathFinding.Node> path = new List<PathFinding.Node>();
        public bool onPath = false;
        public Func<Vector3, Vector3, PathFinding.Node> astarGetter;
        public AStar astar = new AStar();
        public const int MIN_PATHFINDING_IDLE = 3000;
        public int timeSinceAstar = 3000;

        public Zombie(int health, int maxHealth, ZombieType type, ref Model modelwalk, ref Model modelatt, ref Model modelhurt, ref Model modeldie, Action<Entity, Entity> attackFunction, Func<Vector3, Vector3, PathFinding.Node> astarGetter)
            : base()
        {
            this.astarGetter = astarGetter;
            this.model = modelwalk;
            this.HealthPoints = health;
            this.MaxHealth = maxHealth;

            this.MaxVelocity = 0.04f;
            this.MaxAcceleration = 0.04f;
            if (type == ZombieType.Boss)
            {
                this.MaxVelocity *= 2;
                this.MaxAcceleration *= 2;
                this.modelRadius *= 2;
            }
            ArriveRadius = 1;
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
            if (type == ZombieType.Boss)
            {
                MeleeAttack.FirePower *= 2;
                RangedAttack.FirePower *= 2;
            }
            this.AttackFunction = attackFunction;
            lastAttackTime = 0;

            // Look up our custom skinning information. for walking
            skinningDatawalk = (SkinningData)modelwalk.Tag;

            if (skinningDatawalk == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayerwalk = new AnimationPlayer(skinningDatawalk);
            clipwalk = skinningDatawalk.AnimationClips["Take 001"];
            animationPlayerwalk.StartClip(clipwalk);


            // Look up our custom skinning information. for dying
            skinningDatadie = (SkinningData)modeldie.Tag;

            if (skinningDatadie == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayerdie = new AnimationPlayer(skinningDatadie);
            clipdie = skinningDatadie.AnimationClips["Take 001"];
            animationPlayerdie.StartClip(clipdie);


            // Look up our custom skinning information. for attacking
            skinningDataattack = (SkinningData)modelatt.Tag;

            if (skinningDataattack == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayerattack = new AnimationPlayer(skinningDataattack);
            clipattack = skinningDataattack.AnimationClips["Take 001"];
            animationPlayerattack.StartClip(clipattack);

            // Look up our custom skinning information. for hurting
            skinningDatahurt = (SkinningData)modelhurt.Tag;

            if (skinningDatahurt == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayerhurt = new AnimationPlayer(skinningDatahurt);
            cliphurt = skinningDatahurt.AnimationClips["Take 001"];
            animationPlayerhurt.StartClip(cliphurt);

        }
        
         //Execute entity's action
        public void Update(GameTime gameTime)
        {
            lastAttackTime += gameTime.ElapsedGameTime.Milliseconds;
            timeSinceAstar += gameTime.ElapsedGameTime.Milliseconds;

            #region Animations
            if (animState == AnimationState.Idle)
            {
                animationPlayer = animationPlayerwalk;
                animationPlayer.ResetClip();
            }
            else if (animState == AnimationState.Walking)
            {
                animationPlayer = animationPlayerwalk;
            }
            else if(animState == AnimationState.Attacking)//animation for attacking
            {
                animationPlayer = animationPlayerattack;
            }
            else if (animState == AnimationState.Hurt)//animation when hurt
            {
                animationPlayer = animationPlayerhurt;

                ElapsedDamagedTime += gameTime.ElapsedGameTime.Milliseconds;
                if (ElapsedDamagedTime < DAMAGE_ANIM_LENGTH)
                {
                    if (animationPlayer.currentKeyframe < animationPlayer.CurrentClip.Keyframes.Count() - 1)
                    {
                        animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
                    }
                    else
                        animationPlayer.ResetClip();
                    return;
                }
                ElapsedDamagedTime = 0;
                animState = AnimationState.Idle;
            }
            else if (animState == AnimationState.Dying)//animation for dying
            {
                animationPlayer = animationPlayerdie;
                ElapsedDeathTime += gameTime.ElapsedGameTime.Milliseconds;
                if (ElapsedDeathTime < DEATH_ANIM_LENGTH)
                {
                    if (animationPlayer.currentKeyframe < animationPlayer.CurrentClip.Keyframes.Count() - 1)
                    {
                        animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
                    }
                    return;
                }
                else
                {
                    Dead = true;
                    return;
                }
            }
            else//if just standing
            {
                animationPlayer = animationPlayerwalk;
                animationPlayer.ResetClip();
            }
            animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            #endregion

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
                        //if (onPath)
                        //{
                        //    // see if we can get to second node in path
                        //    if (path.Count > 1 && astarGetter(Position, path[1].position).Equals(Vector3.Zero))
                        //    {
                        //        GroundTarget = path[1].position;
                        //        path.RemoveAt(0);
                        //    }
                        //    else if (path.Count > 0)
                        //    {
                        //        if (!astarGetter(Position, GroundTarget).Equals(Vector3.Zero))
                        //            GroundTarget = path[0].position;
                        //        else
                        //            onPath = false;
                        //    }
                        //}
                        //else if (!onPath) // try to get path
                        //{
                        //    if (timeSinceAstar > MIN_PATHFINDING_IDLE)
                        //    {
                        //        path = GetAStarPath(GroundTarget);
                        //        if (path != null)
                        //            onPath = true;
                        //        timeSinceAstar = 0;
                        //    }
                        //}
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
                            onPath = false;
                        }
                        break;
                    }
                case EntityPositionState.SteeringWander:
                    {
                        SteeringWander();
                        animState = AnimationState.Walking;
                        Position += Velocity;
                        onPath = false;
                        break;
                    }
                case EntityPositionState.Attack:
                    {
                        animState = AnimationState.Attacking;
                        if (lastAttackTime > MeleeAttack.Speed)
                        {
                            Attack(MeleeAttack);
                            lastAttackTime = 0;
                        }
                        onPath = false;
                        break;
                    }
                case EntityPositionState.RangedAttack:
                    {
                        animState = AnimationState.Attacking;
                        if (lastAttackTime > RangedAttack.Speed)
                        {
                            Attack(RangedAttack);
                            lastAttackTime = 0;
                        }
                        onPath = false;
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
                        GroundTarget = Target.Position;
                        return EntityPositionState.SteeringArrive;
                    }
                case BehaviourState.RangedCreep:
                    {
                        if ((Position - Target.Position).Length() < MAX_PROJECTILE_DISTANCE)
                        {
                            return EntityPositionState.RangedAttack;
                        }
                        creep = true;
                        GroundTarget = Target.Position;
                        return EntityPositionState.SteeringArrive;
                    }
                case BehaviourState.MeleePursue:
                    {
                        // if zombie has reserved a slot, check if close enough to attack, or far enough to release
                        if (targetslot >= 0)
                        {
                            // check if close enough to slot to start attacking
                            if ((Position - SlotTarget).Length() < ArriveRadius)
                            {
                                return EntityPositionState.Attack;
                            }
                            // check if far enough to release slot
                            if ((Position - Target.Position).Length() > 2 * MAX_PROJECTILE_DISTANCE)
                            {
                                Target.releaseSlot(this, targetslot);
                                targetslot = -1;
                            }
                            GroundTarget = SlotTarget;
                            return EntityPositionState.SteeringArrive;
                        }
                        // zombie has no slot, try to attack or reserve a slot
                        if ((Position - Target.Position).Length() < MAX_MELEE_DISTANCE)
                        {
                            return EntityPositionState.Attack;
                        }
                        // check if close enough to reserve a slot and set slot as target
                        if ((Position - Target.Position).Length() < MAX_PROJECTILE_DISTANCE)
                        {
                            targetslot = Target.reserveSlot(this);
                            GroundTarget = Target.Position;
                            return EntityPositionState.SteeringArrive;
                        }
                        
                        GroundTarget = Target.Position;
                        return EntityPositionState.SteeringArrive;
                    }
                case BehaviourState.MeleeCreep:
                    {
                        // if zombie has reserved a slot, check if close enough to attack, or far enough to release
                        if (targetslot >= 0)
                        {
                            // check if close enough to slot to start attacking
                            if ((Position - SlotTarget).Length() < ArriveRadius)
                            {
                                return EntityPositionState.Attack;
                            }
                            // check if far enough to release slot
                            if ((Position - Target.Position).Length() > 2 * MAX_PROJECTILE_DISTANCE)
                            {
                                Target.releaseSlot(this, targetslot);
                                targetslot = -1;
                            }
                            GroundTarget = SlotTarget;
                            creep = true;
                            return EntityPositionState.SteeringArrive;
                        }
                        // zombie has no slot, try to attack or reserve a slot
                        if ((Position - Target.Position).Length() < MAX_MELEE_DISTANCE)
                        {
                            return EntityPositionState.Attack;
                        }
                        // check if close enough to reserve a slot and set slot as target
                        if ((Position - Target.Position).Length() < MAX_PROJECTILE_DISTANCE)
                        {
                            targetslot = Target.reserveSlot(this);
                            GroundTarget = Target.Position;
                            return EntityPositionState.SteeringArrive;
                        }

                        creep = true;
                        GroundTarget = Target.Position;
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

        // Set a target for the Zombie if he doesn't already have one
        public void Alert(Hero target)
        {
            if (Target == null)
            {
                this.Target = target;
                DoFuzzyLogic();
            }
        }

        public void TakeDamage(int damage)
        {
            animState = AnimationState.Hurt;
            HealthPoints -= damage;

            if (HealthPoints <= 0)
                Die();

            DoFuzzyLogic();
        }

        private void Die()
        {
            animState = AnimationState.Dying;
            if (Target != null && targetslot >= 0)
                Target.releaseSlot(this, targetslot);
        }

        #region Movement Heuristics
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
            Vector3 newDirection = GroundTarget - this.Position;

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
                TargetSpeed = MaxVelocity / 2;
            }
            else
            {
                TargetSpeed = MaxVelocity / 2 * Distance / SlowRadiusThreshold;
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
        #endregion

        // Executes procedure for performing an attack
        private void Attack(Weapon weapon)
        {
            // Set velocity only as a direction vector
            Velocity = Target.Position - Position;
            Velocity.Normalize();
            // Face towards target with no heuristic
            EntityOrientationState originalState = OrState;
            OrState = EntityOrientationState.None;
            SetOrientation(Target.Position - Position);
            OrState = originalState;

            if (lastAttackTime > weapon.Speed)
            {
                AttackFunction(this, weapon);
                DoFuzzyLogic();
                lastAttackTime = 0;
            }
        }

        // Retrieves next behaviour from fuzzy logic module
        private void DoFuzzyLogic()
        {
            if (Target != null)
            {
                Fuzzifier fuzz = new Fuzzifier(HealthPoints / MaxHealth, (Target as Hero).HealthPoints / (Target as Hero).MaxHealth,
                        (Position - Target.Position).Length() / MAX_DISTANCE);
                BehaviouralState = fuzz.GetBehaviour();

                // release any reserved slot if new behaviour does not require one
                if (BehaviouralState != BehaviourState.MeleeCreep && BehaviouralState != BehaviourState.MeleePursue)
                {
                    if (targetslot >= 0)
                    {
                        Target.releaseSlot(this, targetslot);
                    }
                    GroundTarget = new Vector3();
                }
                // forget the target if the bew behaviour does not require one
                if (BehaviouralState == BehaviourState.Wander)
                {
                    Target = null;
                }
            }
        }

        // Get A* path. If anything at all is wrong, ABORT
        private List<PathFinding.Node> GetAStarPath(Vector3 destination)
        {
            if (astarGetter(Position, destination).Equals(Vector3.Zero))
            {
                return null;
            }
            
            astar.Destination = astarGetter(destination, Vector3.Up);
            if (astar.Destination == null)
                return null;
            PathFinding.Node o = astarGetter(Position, Vector3.Up);
            if (o == null)
                return null;
            astar.Origin = o;
            return astar.GetShortestPath();
        }

        //code for flanking
        public void Notify(Vector3 value)
        {
            SlotTarget = value;
        }
    }
}
