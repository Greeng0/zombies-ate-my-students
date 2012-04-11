﻿using System;
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
        public int slot;
    }

    public struct Node
    {
        public Vector3 po;
        public bool occ;
    }

    class Hero : Entity
    {
        public int HealthPoints;
        public int MaxHealth;
        public float moveSpeed = 0.8f;
        public float rotationSpeed = 0.1f;
        public AnimationStance Stance;
        public bool Dead = false;

        //animations
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
        public const int DEATH_ANIM_LENGTH = 2000;
        public int ElapsedDeathTime = 0;
        public AnimationPlayer animationPlayerdie; // This calculates the Matrices of the animation
        public AnimationClip clipdie;              // This contains the keyframes of the animation
        public SkinningData skinningDatadie;       // This contains all the skinning data
        
        public float scale = .1f;                   // Scale at which to render the model

        public List<Powerup> PowerupsList;           // List of powerups obtained by the Hero
        public Dictionary<Item, int> ItemsList;     // List of items obtained by the Hero
        public Dictionary<Weapon, int> WeaponsList; // List of weapons obtained by the Hero
        public Item SelectedItem;                   // Item which will be used when UseItem is called
        public Weapon EquippedWeapon;
        public int TimeSinceLastFire = 0;
        public int TimeSinceLastUse = 0;
        public const int ITEM_USE_INTERVAL = 500;


        Item fireext;
        


        public Action<Entity, Entity> ActionFunction;   // Callback function used when an attack is made

        //adding flanking var
        private float attackdist = 3;
        private List<IHeroObserver> observer = new List<IHeroObserver>();

        //add nodes
        private Node[] nodes = new Node[6];

        //adding line to show where aiming info
        public VertexPositionColor[] ray = new VertexPositionColor[2];
        private Vector3 ray1 = new Vector3(10,20,0);
        private Vector3 ray2 = new Vector3(0,10,10);
        private Vector3 rayHeight = new Vector3(0, 5.5f, 0);
        public float raydist = 50;


        //items

        public int extinguishers = 0;
        public int meds = 0;
        public int keys = 0;

        public int current = 0;


        public Hero(int health, int maxHealth, ref Model modelwalk, ref Model  modeldie, ref Model modelhurt, Action<Entity, Entity> actionFunction)
            : base()
        {
            this.model = modelwalk;
            this.HealthPoints = health;
            this.MaxHealth = maxHealth;
            this.Stance = AnimationStance.Standing;
         

            PowerupsList = new List<Powerup>();
            ItemsList = new Dictionary<Item, int>();
            WeaponsList = new Dictionary<Weapon, int>();
         //   AddWeapon(new Weapon(WeaponType.Handgun9mm));

            this.ActionFunction = actionFunction;

            //get animations
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

            // Look up our custom skinning information. for hurting
            skinningDatahurt = (SkinningData)modelhurt.Tag;

            if (skinningDatahurt == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayerhurt = new AnimationPlayer(skinningDatahurt);
            cliphurt = skinningDatahurt.AnimationClips["Take 001"];
            animationPlayerhurt.StartClip(cliphurt);

            //adding flanking info
            for (int i = 0; i < 6; i++)
            {
                nodes[i] = new Node();
            }
        }

        public void Update(GameTime gameTime)
        {
            TimeSinceLastFire += gameTime.ElapsedGameTime.Milliseconds;
            TimeSinceLastUse += gameTime.ElapsedGameTime.Milliseconds;

            if (animState == AnimationState.Idle)
            {
                animationPlayer = animationPlayerwalk;
                animationPlayer.ResetClip();
            }
            else if (animState == AnimationState.Walking)
            {
                animationPlayer = animationPlayerwalk;
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


            //update ray positions

            //ray[0] = new VertexPositionColor(Position + rayHeight, Color.GreenYellow);
            //ray[1] = new VertexPositionColor(Position + rayHeight + new Vector3((float)Math.Sin(Rotation), 0, (float)Math.Cos(Rotation)) * EquippedWeapon.Range, Color.GreenYellow);
        
            ray[0] = new VertexPositionColor(Position + rayHeight, Color.GreenYellow);
            ray[1] = new VertexPositionColor(Position + rayHeight + raydist * new Vector3((float)Math.Sin(Rotation), 0, (float)Math.Cos(Rotation)) * 2f, Color.GreenYellow);          
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
            if (SelectedItem != null && TimeSinceLastUse > ITEM_USE_INTERVAL)
            {
                ActionFunction(this, SelectedItem);
                TimeSinceLastUse = 0;
            }
        }

        private void FireWeapon()
        {
            if (EquippedWeapon != null && TimeSinceLastFire > EquippedWeapon.Speed)
            {
                ActionFunction(this, EquippedWeapon);
                TimeSinceLastFire = 0;
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
            if (EquippedWeapon != null)
            {
                if (EquippedWeapon == WeaponsList.First().Key)
                {
                    EquippedWeapon = WeaponsList.Last().Key;
                }
                else
                {
                    EquippedWeapon = WeaponsList.First().Key;
                }
            }
        }
        public void SwitchNextItem()
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.itemType == ItemType.Extinguisher)
                {
                    SelectedItem.itemType = ItemType.Key;
                    current = 1;
                }
                else if (SelectedItem.itemType == ItemType.Key)
                {
                    SelectedItem.itemType = ItemType.MedPack;
                    current = 2;
                }

                else if (SelectedItem.itemType == ItemType.MedPack )
                {
                    SelectedItem.itemType = ItemType.Extinguisher;
                    current = 0;
                }
                else
                {
                    current = 0; 
                }
            }
        }

        public void MoveForward()
        {
            Position += (PowerupsList.Contains(new Powerup(PowerupType.Sneakers))) ? 2 * Velocity : Velocity;
            notifyObservers();
        }
        public void MoveBackward()
        {
            Position -= Velocity;
            notifyObservers();
        }
        public void TurnLeft()
        {
            if (this.Stance == AnimationStance.Shooting)
            {
                Rotation += rotationSpeed/3;
            }
            else
            {
                Rotation += rotationSpeed;
            }
            Rotation %= Math.PI * 2;
            Velocity = moveSpeed * new Vector3((float)Math.Sin(Rotation), 0, (float)Math.Cos(Rotation));
        }
        public void TurnRight()
        {
            if (this.Stance == AnimationStance.Shooting)
            {
                Rotation -= rotationSpeed / 3;
            }
            else
            {
                Rotation -= rotationSpeed;
            }
            Rotation %= Math.PI * 2;
            Velocity = moveSpeed * new Vector3((float)Math.Sin(Rotation), 0, (float)Math.Cos(Rotation));
        }

        public void Heal(int health)
        {
            HealthPoints += health;
            if (HealthPoints > MaxHealth)
                HealthPoints = MaxHealth;
        }

        public void TakeDamage(int damage)
        {
            animState = AnimationState.Hurt;
            HealthPoints -= damage;
            if (HealthPoints <= 0)
            {
                HealthPoints = 0;
                Die();
            }
        }

        private void Die()
        {
            animState = AnimationState.Dying;
        }

        //adding flanking data
        private void notifyObservers()
        {
            foreach (IHeroObserver i in observer)
            {
                i.Notify(nodes[i.Targetslot()].po + Position);
            }
        }

        private moveme calculateSlotPositiion(Entity ent)
        {
            //finding slot positions.
            moveme decision = new moveme();

            if (observer.Count == 0)//if first in list, give him ideal position
            {
                //give new slot at shortest distace
                decision.move = true;
                decision.pos = Vector3.Normalize(ent.Position - Position) * attackdist;

                //reset nodes
                float angle = (float)Math.Atan((ent.Position - Position).Z / (ent.Position - Position).X);

                nodes[0].po = new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle)) * attackdist;
                nodes[1].po = new Vector3((float)Math.Sin(angle + MathHelper.ToRadians(60)), 0, (float)Math.Cos(angle + MathHelper.ToRadians(60))) * attackdist;
                nodes[2].po = new Vector3((float)Math.Sin(angle + MathHelper.ToRadians(120)), 0, (float)Math.Cos(angle + MathHelper.ToRadians(120))) * attackdist;
                nodes[3].po = -new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle)) * attackdist;
                nodes[4].po = -new Vector3((float)Math.Sin(angle + MathHelper.ToRadians(60)), 0, (float)Math.Cos(angle + MathHelper.ToRadians(60))) * attackdist;
                nodes[5].po = -new Vector3((float)Math.Sin(angle + MathHelper.ToRadians(120)), 0, (float)Math.Cos(angle + MathHelper.ToRadians(120))) * attackdist;

                //set node to right state
                nodes[0].occ = true;
                decision.slot = 0;
            }
            else if (observer.Count > 6)//too many zombies, refuse
            {
                decision.move = false;
            }
            else
            {
                int start = observer.Last().Targetslot();

                if (!nodes[((start + 2)) % 6].occ)
                {
                    nodes[((start + 2) % 6)].occ = true;
                    decision.slot = ((start + 2) % 6);

                    decision.move = true;
                    decision.pos = nodes[((start + 2) % 6)].po;
                }
                else if (!nodes[((start + 4)) % 6].occ)//first try not open go on
                {
                    nodes[((start + 4) % 6)].occ = true;
                    decision.slot = ((start + 4) % 6);

                    decision.move = true;
                    decision.pos = nodes[((start + 4) % 6)].po;
                }
                else if (!nodes[((start + 3)) % 6].occ)
                {
                    nodes[((start + 3) % 6)].occ = true;
                    decision.slot = ((start + 3) % 6);

                    decision.move = true;
                    decision.pos = nodes[((start + 3) % 6)].po;
                }
                else if (!nodes[((start + 5)) % 6].occ)
                {
                    nodes[((start + 5) % 6)].occ = true;
                    decision.slot = ((start + 5) % 6);

                    decision.move = true;
                    decision.pos = nodes[((start + 5) % 6)].po;
                }
                else if (!nodes[((start + 1)) % 6].occ)
                {
                    nodes[((start + 1) % 6)].occ = true;
                    decision.slot = ((start + 1) % 6);

                    decision.move = true;
                    decision.pos = nodes[((start + 1) % 6)].po;
                }
                else
                {
                    //absolutely no solution get ouyt.
                    decision.move = false;
                }
            }

            return decision;
        }

        public int reserveSlot(Zombie z)
        {
            moveme decision = calculateSlotPositiion(z);

            if (decision.move)//if good value returned
            {
                subscribe(z);
                z.Notify(decision.pos + Position);
                return decision.slot;
            }
            return -1;
        }

        public void releaseSlot(IHeroObserver obs, int slot)
        {
            nodes[slot].occ = false;
            unsubscribe(obs);
        }

        private void subscribe(IHeroObserver obs)
        {
            observer.Add(obs);
        }
        private void unsubscribe(IHeroObserver obs)
        {
            observer.Remove(obs);
        }
    }
}
