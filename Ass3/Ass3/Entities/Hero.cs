using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Entities;
using SkinnedModel;

namespace Entities
{
    public enum AnimationStance
    {
        Standing,
        Shooting
    }

    class Hero : Entity
    {
        public int HealthPoints;
        public int MaxHealth;
        public bool walking = false;
        public float moveSpeed = 0.1f;
        public float rotationSpeed = 0.1f;
        public AnimationStance Stance;

        public AnimationPlayer animationPlayer;     // This calculates the Matrices of the animation
        public AnimationClip clip;                  // This contains the keyframes of the animation
        public SkinningData skinningData;           // This contains all the skinning data
        public float scale = .1f;

        public Dictionary<string, int> ItemsList;
        public Dictionary<string, int> WeaponsList;
        public Item SelectedItem;
        public Weapon EquippedWeapon;
 
        public Hero(int health, int maxHealth, ref Model model)
        {
            this.model = model;
            this.HealthPoints = health;
            this.MaxHealth = maxHealth;
            this.Stance = AnimationStance.Standing;

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
        }

        public void SwitchNextWeapon()
        {
            //TODO
        }
        public void SwitchNextItem()
        {
            //TODO
        }
        public void MoveForward()
        {
            Position -= moveSpeed * new Vector3((float)Math.Sin(Rotation), 0, (float)Math.Cos(Rotation));
        }
        public void MoveBackward()
        {
            Position += moveSpeed * new Vector3((float)Math.Sin(Rotation), 0, (float)Math.Cos(Rotation));
        }
        public void TurnLeft()
        {
            Rotation += rotationSpeed;
        }
        public void TurnRight()
        {
            Rotation -= rotationSpeed;
        }
    }
}
