using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Particles
{
    //This class is a Container for groups of particles. It can be controlled independently to provide group wide movement of particles being created
    public class ParticleEmitter
    {
        public List<ParticleGroup> particleGroups; //Particle Groups
        public Vector3 position; //Positon of Emitter
        public Vector3 velocity; // Speed and direction of Emitter
        public float acceleration; //Acceleration of Emitter *CURRENT NOT USED*
        private long lifeSpan; // Time before Emitter stops itself in milliseconds

        public long LifeSpan
        {
            get { return lifeSpan; }
            set 
            { 
                lifeSpan = value;

                if (lifeSpan < 0)
                    lifeSpan = 0;
            }
        }

        public float age; //Current age of emitter
        public float size; //Size of Emitter *CURRENT NOT USED*
        private bool Emitting;//Indicates whether the Emitter is active or not

        //Retrieve particle count for all the groups
        public long GetTotalParticleCount()
        {
            int count = 0;

            foreach (ParticleGroup group in particleGroups)
            {
                count += group.GetTotalGroupParticleCount();
            }

            return count;
        }

        //Special variable that propagates the world matrix to the groups for their BasicEffect to update.
        public Matrix World
        {
            set 
            {
                foreach (ParticleGroup group in particleGroups)
                {
                    group.effect.World = value;
                }
            }
        }

        private ParticleEmitter()
        {
        }

        //Ctor
        public ParticleEmitter(Vector3 Pos, Vector3 Vel, float Acc, long Life, float Size)
        {
            position = Pos;
            velocity = Vel;
            acceleration = Acc;
            lifeSpan = Life;
            age = 0;
            this.size = Size;
            particleGroups = new List<ParticleGroup>();
            Emitting = false;
        }

        //Makes a Deep Copy of the Emitter
        public ParticleEmitter Clone()
        {
            ParticleEmitter newPE = new ParticleEmitter();

            newPE.position = new Vector3(this.position.X, this.position.Y, this.position.Z);
            newPE.velocity = new Vector3(this.velocity.X, this.velocity.Y, this.velocity.Z);
            newPE.acceleration = this.acceleration;
            newPE.lifeSpan =  this.lifeSpan;
            newPE.size = this.size;
            newPE.Emitting = this.Emitting;
            newPE.particleGroups = new List<ParticleGroup>();

            foreach (ParticleGroup group in this.particleGroups)
            {
                newPE.particleGroups.Add(group.Clone());
            }

            return newPE;
        }

        //Start Emitting Particles
        public void Start()
        {
            Emitting = true;
        }

        //Stop Emitting Particles
        public void Stop(bool Smooth = true)
        {
            //Kill off all particles abruptly if Smooth if false
            if (!Smooth)
            {
                foreach (ParticleGroup group in particleGroups)
                {
                    group.particles.Clear();
                }
            }
            Emitting = false;
        }

        //Retrieve Emission status of emitter
        public bool isEmitting()
        {
            return Emitting;
        }

        //Update emitter and the groups it contains
        public void UpdateEmitter(GameTime gameTime)
        {
            //Move emitter
            position += velocity;

            //Stop emitter if life is over
            if (Emitting)
            {
                //lifeSpan = 0 means infinite life
                if (lifeSpan != 0)
                {
                    age += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (age >= lifeSpan)
                    {
                        age = 0;
                        Emitting = false;
                    }
                }
            }
           
            //Update all groups
            foreach (ParticleGroup group in particleGroups)
            {
                group.UpdateGroup(gameTime, Emitting, position);
            }
        }
    }
}
