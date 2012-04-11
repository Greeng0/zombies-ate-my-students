using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Particles
{
    //Determines whether the particles will appear flat or 3D 
    public enum ParticleAppearance
    {
        Flat,
        ThreeDimensional
    }

    //Group of particles and main control structure for animating particles.
    //Responsible for creating new particles to be emitted by the Emitter
    public class ParticleGroup
    {
        public static Random rand = new Random((int)DateTime.Now.Ticks);
        
        public string name; //Group Name
        public ParticleController controller; //Controller structure
        public List<Particle> particles; //List of particles
        private BasicEffect _effect; //XNA's rendering effect
        public BlendState blendState; 
        public DepthStencilState depthStencil;
        public VertexPositionNormalTexture[] vertices; //Vertex array used to send to the vertex buffer

        public float EmitTimer; //Timer used to regulate particle emission rate

        public BasicEffect effect
        {
            get { return _effect; }
            set { _effect = value; }
        }
        
        //Return group particle count
        public int GetTotalGroupParticleCount()
        {
            return particles.Count;
        }

        //Return a Deep Copy of the Particle Group
        public ParticleGroup Clone()
        {
            ParticleGroup newPG = new ParticleGroup();

            newPG.name = this.name;
            newPG.controller = this.controller;
            newPG.particles = new List<Particle>();
            newPG.effect = (BasicEffect)this.effect.Clone();
            newPG.blendState = this.blendState;
            newPG.depthStencil = this.depthStencil;
            newPG.vertices = null;
            newPG.EmitTimer = 0;

            return newPG;
        }
        
        //Ctor
        private ParticleGroup()
        {
        
        }

        //Ctor
        public ParticleGroup(string name,BlendState blend, DepthStencilState depthStencil, BasicEffect basicEffect)
        {
            this.name = name;
            particles = new List<Particle>();
            effect = basicEffect;
            blendState = blend;
            this.depthStencil = depthStencil;
            EmitTimer = 0;
            controller = new ParticleController();

            effect.TextureEnabled = true;
        }

        //Animate particles in the group, create new ones based on the controller parameters and delete old particles
        public void UpdateGroup(GameTime gameTime,bool Emitting,Vector3 EmitterPosition)
        {
            //Set transparency for the group
            effect.Alpha = controller.Alpha;

            List<Particle> toDelete = new List<Particle>();
            
            //Update Particle animation and get rid of old particles
            foreach(Particle particle in particles)
            {
                particle.Update(gameTime);
                if (particle.age >= particle.lifeSpan && particle.lifeSpan != 0)
                {
                    toDelete.Add(particle);
                }
            }

            //Delete old particles
            foreach(Particle particle in toDelete)
            {
                particles.Remove(particle);
            }

            //Create new particles if emitter is Active
            if (Emitting)
            {
                //Update Timer
                EmitTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                //Add a new particle in the group if timer is up
                if (EmitTimer <= 0)
                {
                    //Create a number of particles based on the controller's particles to be created per frame/emission
                    for (int i = 0; i < controller.ParticlePerEmission; i++)
                    {
                        //Create particles only if max particles is not reached
                        if (particles.Count < controller.MaxParticles)
                        {
                            Particle newPart = new Particle();

                            //Assign new particle position
                            if (controller.RandomizeInitialPosition)
                            {
                                Vector3 Offset = new Vector3();
                                Offset.X = ((float)rand.NextDouble() * controller.InitialPositionOffset.X) - controller.InitialPositionOffset.X;
                                Offset.Y = ((float)rand.NextDouble() * controller.InitialPositionOffset.Y) - controller.InitialPositionOffset.Y;
                                Offset.Z = ((float)rand.NextDouble() * controller.InitialPositionOffset.Z) - controller.InitialPositionOffset.Z;

                                newPart.position = EmitterPosition + Offset;
                            }
                            else
                            {
                                newPart.position = EmitterPosition + controller.InitialPositionOffset;
                            }
                            
                            //Assign new particle direction and speed
                            if (controller.RandomizeDirection)
                            {
                                Vector3 Direction = new Vector3(
                                        (float)((rand.NextDouble() * controller.directionRange.X) + controller.directionOffset.X), 
                                        (float)((rand.NextDouble() * controller.directionRange.Y) + controller.directionOffset.Y),
                                        (float)((rand.NextDouble() * controller.directionRange.Z) + controller.directionOffset.Z));

                                //Direction.Normalize();

                                newPart.velocity = new Vector3(Direction.X * controller.Velocity.X,Direction.Y * controller.Velocity.Y,Direction.Z * controller.Velocity.Z);
                            }
                            else
                            {

                                Vector3 Direction = new Vector3(
                                    (float)(( controller.directionRange.X) + controller.directionOffset.X),
                                    (float)((controller.directionRange.Y) + controller.directionOffset.Y),
                                    (float)(( controller.directionRange.Z) + controller.directionOffset.Z));

                                //Direction.Normalize();

                                newPart.velocity = new Vector3(Direction.X * controller.Velocity.X,Direction.Y * controller.Velocity.Y,Direction.Z * controller.Velocity.Z);
                            }


                            if (controller.IncreaseParticleSize)
                            {
                                newPart.SizeIncrease = controller.SizeIncrease;
                            }
                            else
                            {
                                newPart.SizeIncrease = 0;
                            }

                            newPart.acceleration = controller.acceleration;

                            //Assign new particle rotation speed
                            if (controller.RandomizeRotation)
                            {
                                newPart.rotationVelocity = (float)rand.NextDouble() * controller.RotationVelocity;
                            }
                            else
                            {
                                newPart.rotationVelocity = 1 * controller.RotationVelocity;
                            }

                            //Assign new particle size
                            if (controller.RandomizeSize)
                            {
                                newPart.size = (float)rand.NextDouble() * controller.Size;
                                if (newPart.size < controller.MininumSize)
                                    newPart.size = controller.MininumSize;
                            }
                            else
                            {
                                newPart.size = controller.Size;
                            }

                            //Assign new particle lifespan
                            newPart.lifeSpan = controller.LifeSpan;

                            //Add new particle to the list
                            particles.Add(newPart);
                        }
                    }

                    //Reset timer to specified emission rate
                    EmitTimer = controller.EmissionRate;
                }
            }
            else
            {
                EmitTimer = 0;
            }
        }

        //Load the vertices array to be used through the Vertex Buffer
        public void LoadVertexArray(ParticleAppearance appearance)
        {
            //Each Particle requires 6 vertices
            vertices = new VertexPositionNormalTexture[6 * particles.Count];

            for (int i = 0; i < particles.Count; i++)
            {
                Matrix rotMat = Matrix.Identity;

                //if Particles must keep a 3D look, reverse the current rotation of the world so particles face the camera
                if (appearance == ParticleAppearance.Flat)
                {
                    rotMat = Matrix.CreateRotationZ(particles[i].rotationAngle);
                }
                else
                {
                    rotMat = Matrix.CreateRotationZ(particles[i].rotationAngle) * Matrix.Invert(effect.World);
                }
                
                vertices[i * 6].Position = Vector3.Transform(new Vector3(-1f, 1f, 0) * particles[i].size, rotMat) + new Vector3(particles[i].position.X, particles[i].position.Y, particles[i].position.Z);
                vertices[i * 6].TextureCoordinate.X = 0;
                vertices[i * 6].TextureCoordinate.Y = 0;

                vertices[(i * 6) + 1].Position = Vector3.Transform(new Vector3(1f, -1f, 0) * particles[i].size, rotMat) + new Vector3(particles[i].position.X, particles[i].position.Y, particles[i].position.Z);
                vertices[(i * 6) + 1].TextureCoordinate.X = 1;
                vertices[(i * 6) + 1].TextureCoordinate.Y = 1;

                vertices[(i * 6) + 2].Position = Vector3.Transform(new Vector3(-1f, -1f, 0) * particles[i].size, rotMat) + new Vector3(particles[i].position.X, particles[i].position.Y, particles[i].position.Z);
                vertices[(i * 6) + 2].TextureCoordinate.X = 0;
                vertices[(i * 6) + 2].TextureCoordinate.Y = 1;

                vertices[(i * 6) + 3].Position = Vector3.Transform(new Vector3(-1f, 1f, 0) * particles[i].size, rotMat) + new Vector3(particles[i].position.X, particles[i].position.Y, particles[i].position.Z);
                vertices[(i * 6) + 3].TextureCoordinate.X = 0;
                vertices[(i * 6) + 3].TextureCoordinate.Y = 0;

                vertices[(i * 6) + 4].Position = Vector3.Transform(new Vector3(1f, 1f, 0) * particles[i].size, rotMat) + new Vector3(particles[i].position.X, particles[i].position.Y, particles[i].position.Z);
                vertices[(i * 6) + 4].TextureCoordinate.X = 1;
                vertices[(i * 6) + 4].TextureCoordinate.Y = 0;

                vertices[(i * 6) + 5].Position = Vector3.Transform(new Vector3(1f, -1f, 0) * particles[i].size, rotMat) + new Vector3(particles[i].position.X, particles[i].position.Y, particles[i].position.Z);
                vertices[(i * 6) + 5].TextureCoordinate.X = 1;
                vertices[(i * 6) + 5].TextureCoordinate.Y = 1;
            }
        }
    }
}
