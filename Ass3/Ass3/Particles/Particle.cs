using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Particles
{
    public class Particle
    {
        public static Random rand = new Random((int)DateTime.Now.Ticks);
        public Vector3 position;
        public Vector3 velocity;
        public float rotationVelocity;
        private float _rotationAngle;
        public float SizeIncrease;

        public float rotationAngle
        {
            get { return _rotationAngle; }
            set 
            {
                _rotationAngle = value % (2 * (float)Math.PI);
            }
        }

        public float acceleration;
        public long lifeSpan;
        public float age;
        public float size;

        public Particle()
        {
            position = new Vector3(0, 0, 0);
            velocity = new Vector3((float)((Particle.rand.NextDouble()) - 0.5f) / 100, (float)Particle.rand.NextDouble() / 25, 0);
            acceleration = 0f;
            lifeSpan = 200;
            age = 0f;
            size = 1f;
            rotationVelocity = (float)Particle.rand.NextDouble() / 15;
            rotationAngle = 0;
            SizeIncrease = 0;
        }

        public Particle(Particle particle)
        {
            position = new Vector3(particle.position.X, particle.position.Y, particle.position.Z);
            velocity = new Vector3(particle.velocity.X, particle.velocity.Y, particle.velocity.Z);
            acceleration = particle.acceleration;
            lifeSpan = particle.lifeSpan;
            age = 0f;
            size = particle.size;
            rotationVelocity = particle.rotationVelocity;
            rotationAngle = 0;
            SizeIncrease = particle.SizeIncrease;
        }

        public Particle(Vector3 pos, Vector3 vel, float acc, float rotVel, long life, float size,float szeInc)
        {
            position = pos;
            velocity = vel;
            acceleration = acc;
            lifeSpan = life;
            age = 0f;
            this.size = size;
            rotationVelocity = rotVel;
            rotationAngle = 0;
            SizeIncrease = szeInc;
        }

        public void Update(GameTime gameTime)
        {
            rotationAngle += rotationVelocity;
            position += velocity;
            velocity += ((velocity/velocity.Length()) * acceleration);
            age += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            size += SizeIncrease;
        }
    }
}
