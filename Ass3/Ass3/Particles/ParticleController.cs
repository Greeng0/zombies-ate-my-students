using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Particles
{
    //This class contains all the parameters that can be used to Initialize and Modify a Particle Group
    public class ParticleController
    {
        //Determines The direction in which the particle will travel. This value is offset by directionOffset and then multiplied by Velocity to obtain the new velocity.
        //This value can randomized to allow different directions for a particle to travel. When not randomized, it acts the same as velocity.
        public Vector3 directionRange;

        //Modifier that affects the directionRange. Most often used to emphasize a certain direction over another one given a certain range and velocity. 
        //Setting the value of the offset to negative half of the range will create particles that will go both in the Positive and Negative direction of a given X,Y and Z Axis
        public Vector3 directionOffset;

        //Determines The direction in which the particle will travel. This is a fixed Value
        public Vector3 Velocity;
        
        //Determines how far from the emitter position the particle will be created by a fixed offset
        public Vector3 InitialPositionOffset;

        //Acceleration of the particle *CURRENTLY NOT USED*
        public float acceleration;

        //When Active, Oscillates the direction and its offset to provide variety in the movement of the particles
        public bool RandomizeDirection;
        
        //Determines the speed at which the particle rotations on itself for animation purposes
        public float RotationVelocity;

        //When active, The rotation speed of the particle is chosen to be a value between 0 and RotationVelocity
        public bool RandomizeRotation;

        //When active,  the particle's is determined between the MinimumSize and Size
        public bool RandomizeSize;

        //When Activate, The position of the particles is determined randomly between -50%/+50% of the InitialPositionOffset
        public bool RandomizeInitialPosition;

        public bool IncreaseParticleSize;

        private float sizeIncrease;

        public float SizeIncrease
        {
            get { return sizeIncrease; }
            set { sizeIncrease = value; }
        }
        
        //The minimum size a particle can be if RandomizeSize is active
        private float mininumSize;

        public float MininumSize
        {
            get { return mininumSize; }
            set
            {
                mininumSize = value;

                if (mininumSize < 0)
                    mininumSize = 0;

                if (mininumSize > size)
                    mininumSize = size;
            }
        }

        //Fixed Size of the particle
        private float size;
        public float Size
        {
            get { return size; }
            set
            {
                size = value;

                if (size < 0)
                    size = 0;

                if (size < mininumSize)
                    mininumSize = size * 0.75f;
            }
        }

        //The rate in Milliseconds at which an emitter will emit a particle of this group
        private float emissionRate;
        public float EmissionRate
        {
            get { return emissionRate; }
            set 
            { 
                emissionRate = value;

                if (emissionRate < 0)
                    emissionRate = 0;
            }
        }

        //The number of particles this group can hold
        private int maxParticles;
        public int MaxParticles
        {
            get { return maxParticles; }
            set 
            { 
                maxParticles = value;

                if (maxParticles < 0)
                    maxParticles = 0;
            }
        }

        //Life span of a group of particles
        private long lifeSpan;
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

        //Number of particles created per frame
        private int particlePerEmission;
        public int ParticlePerEmission
        {
            get { return particlePerEmission; }
            set 
            { 
                particlePerEmission = value;

                if (particlePerEmission < 0)
                    particlePerEmission = 0;
            }
        }
        
        //Transparency of particle
        private float alpha;
        public float Alpha
        {
            get { return alpha; }
            set 
            { 
                alpha = value;

                if (alpha < 0)
                    alpha = 0;
            }
        }

        //Ctor
        public ParticleController()
        {
            size = 1f;
            mininumSize = size *0.75f;
            acceleration = 0f;
            directionRange = new Vector3(1);
            directionOffset = new Vector3(0);
            Velocity = new Vector3(0.01f);
            RandomizeDirection = false;
            RandomizeRotation = false;
            RandomizeSize = false;
            ParticlePerEmission = 1;
            alpha = 1;
            lifeSpan = 1000;
            maxParticles = 100;
            emissionRate = 0.1f;
            InitialPositionOffset = new Vector3(0);
            RandomizeInitialPosition = false;
            IncreaseParticleSize = false;
            SizeIncrease = 0.01f;
        }

        //Make a Deep Copy of the ParticleController.
        public ParticleController Clone()
        {
            ParticleController newCtrl = new ParticleController();

            newCtrl = (ParticleController)this.MemberwiseClone();

            newCtrl.lifeSpan = this.lifeSpan;
            newCtrl.directionOffset = new Vector3(this.directionOffset.X, this.directionOffset.Y, this.directionOffset.Z);
            newCtrl.directionRange = new Vector3(this.directionRange.X, this.directionRange.Y, this.directionRange.Z);
            newCtrl.InitialPositionOffset = new Vector3(this.InitialPositionOffset.X, this.InitialPositionOffset.Y, this.InitialPositionOffset.Z);
            newCtrl.Velocity = new Vector3(this.Velocity.X, this.Velocity.Y, this.Velocity.Z);

            return newCtrl;
        }
    }
}
