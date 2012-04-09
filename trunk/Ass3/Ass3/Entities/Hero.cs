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
        public struct moveme
    {
        public bool move;
        public Vector3 pos;
        
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
        public float scale = .1f;                   // Scale at which to render the model

        public List<string> PowerupsList;           // List of powerups obtained by the Hero
        public Dictionary<Item, int> ItemsList;     // List of items obtained by the Hero
        public Dictionary<Weapon, int> WeaponsList; // List of weapons obtained by the Hero
        public Item SelectedItem;                   // Item which will be used when UseItem is called
        public Weapon EquippedWeapon;

        public Action<Entity, Entity> ActionFunction;   // Callback function used when an attack is made

        //adding flanking var

        private List<Zombie> Observers;
        private float attackdist = 5;
        private List<IObserver> observer = new List<IObserver>();

        public Hero(int health, int maxHealth, ref Model model, Action<Entity, Entity> actionFunction)
            : base()
        {
            this.model = model;
            this.HealthPoints = health;
            this.MaxHealth = maxHealth;
            this.Stance = AnimationStance.Standing;

            PowerupsList = new List<string>();
            ItemsList = new Dictionary<Item, int>();
            WeaponsList = new Dictionary<Weapon, int>();
            AddWeapon(new Weapon(WeaponType.BareHands));

            this.ActionFunction = actionFunction;

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

        public void DoAction()
        {
            if (Stance == AnimationStance.Standing)
                UseItem();
            else
                FireWeapon();
        }

        private void UseItem()
        {
            if (SelectedItem != null)
            {
                ActionFunction(this, SelectedItem);
            }
        }

        private void FireWeapon()
        {
            if (EquippedWeapon != null)
            {
                ActionFunction(this, EquippedWeapon);
            }
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


          //adding flanking data


        private void notifyObservers()
        {
           /* foreach (Zombie obs in Observers)
            {
                
            }
            */

            foreach (IObserver i in observer)
            {
                i.Notify(Position);
            }
        }
        private moveme calculateSlotPositiion(Vector3 position)
        {
            //finding slot positions.
            moveme decision = new moveme();
        
            if (Observers.Count == 0)//if first in list, give him ideal position
            {
          
                decision.move = true;
                //give new slot at shortest distace
                decision.pos = Vector3.Normalize(Position - position) * attackdist;

            }
            else if (Observers.Count > 6)//too many zombies, refuse
            {
                decision.move = false;

            }
            else
            {
                decision.move = true;
                //give new slot at shortest distace
                decision.pos = Vector3.Normalize(Position - position) * attackdist;
            }
            return decision;
        }

        public void reserveSlot(Zombie z)
        {
             moveme decision = calculateSlotPositiion(z.Position);

             if (decision.move)//if good value returned
             {      
                 observer.Add(z);    
             }
             else//no slots available, deny move
             {
              
             }
           
        }

        public void releaseSlot(int slot)
        {
            unsubscrive(slot);
        }

        private void subscrive(int slot, Zombie zombie)//
        {
            observer.Insert(slot, zombie);
        }
        private void unsubscrive(int slot)
        {
            observer.RemoveAt(slot);
        }
    }
    
}
