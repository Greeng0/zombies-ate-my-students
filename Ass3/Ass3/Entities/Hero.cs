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
        public float moveSpeed = 0.1f;
        public float rotationSpeed = 0.1f;
        public AnimationStance Stance;

        public AnimationPlayer animationPlayer;     // This calculates the Matrices of the animation
        public AnimationClip clip;                  // This contains the keyframes of the animation
        public SkinningData skinningData;           // This contains all the skinning data
        public float scale = .1f;

        public List<string> PowerupsList;
        public Dictionary<Item, int> ItemsList;
        public Dictionary<Weapon, int> WeaponsList;
        public Item SelectedItem;
        public Weapon EquippedWeapon;
 
        public Hero(int health, int maxHealth, ref Model model) : base()
        {
            this.model = model;
            this.HealthPoints = health;
            this.MaxHealth = maxHealth;
            this.Stance = AnimationStance.Standing;

            PowerupsList = new List<string>();
            ItemsList = new Dictionary<Item, int>();
            WeaponsList = new Dictionary<Weapon, int>();
            AddWeapon(new Weapon(WeaponType.BareHands));

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

        public float DoAction()
        {
            if (Stance == AnimationStance.Standing)
                return UseItem();
            else
                return FireWeapon();
        }

        private float UseItem()
        {
            float radius = 0;
            if (SelectedItem != null)
            {
                radius = 20;
                // TODO: use the selected item
            }
            return radius;
        }

        private float FireWeapon()
        {
            float radius = 0;
            if (EquippedWeapon != null)
            {
                radius = EquippedWeapon.SoundRadius;
                if (EquippedWeapon.weaponType == WeaponType.Handgun9mm && PowerupsList.Contains("silencer"))
                {
                    radius /= 2;
                }
                // TODO: fire the equipped weapon
            }
            return radius;
        }

        public void AddWeapon(Weapon weapon)
        {
            WeaponsList.Add(weapon, 1);
            EquippedWeapon = weapon;
        }
        public void AddItem(Item item)
        {
            if (ItemsList.Count < 1)
            {
                SelectedItem = item;
            }
            if (ItemsList.ContainsKey(item))
            {
                ItemsList[item]++;
            }
            else
            {
                ItemsList.Add(item, 1);
            }
        }

        public void SwitchNextWeapon()
        {
            // TODO
        }
        public void SwitchNextItem()
        {
            // TODO
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
            Rotation %= Math.PI * 2;
        }
        public void TurnRight()
        {
            Rotation -= rotationSpeed;
            Rotation %= Math.PI * 2;
        }
    }
}
