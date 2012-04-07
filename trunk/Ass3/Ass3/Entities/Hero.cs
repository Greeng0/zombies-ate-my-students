using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;

namespace COMP476Project
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
        public AnimationStance Stance;
        public Dictionary<string, int> ItemsList;
        public Dictionary<string, int> WeaponsList;
        public Item SelectedItem;
        public Weapon EquippedWeapon;
 
        public Hero(int health, int maxHealth)
        {
            this.HealthPoints = health;
            this.MaxHealth = maxHealth;
            this.Stance = AnimationStance.Standing;
        }
    }
}
