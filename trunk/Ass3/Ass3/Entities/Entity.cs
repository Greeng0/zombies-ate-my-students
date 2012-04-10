using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

/*
 * Claude Gagné 5474019
 * COMP 476 Entity Class
 * 
 */

namespace Entities
{
    public class Entity
    {
        public enum AnimationState
        {
            Idle,
            Walking,
            Attacking, //Zombie Specific animation
            Shooting,  //Hero Specific animation
            Dying,
            Hurt,
            StanceChange, //Hero Specific animation
            UseItem //Hero Specific animation
        }

        public static Random rand = new Random((int)DateTime.Now.Ticks);
        public Model model;
        public float modelRadius = 1.5f;
        public Vector3 Position;                //Position on the entity on screen.
        public Vector3 Velocity;                //Velocity Vector containing both the direction and the current velocity of the entity
        public double Rotation;                 //Current orientation Angle 
        
        public AnimationState animState;

        public Entity()
        {
            Position = new Vector3(0, 0, 0);
            Velocity = new Vector3(0f, 0, 0.0f);

            animState = AnimationState.Idle;
            Rotation = 0;
        }
   }
}
