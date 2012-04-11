using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Entities
{
    public enum WeaponType
    {
        BareHands,
        Handgun9mm,
        Magnum,
        ZombieHands,
        Vomit
    }
    
    class Weapon : Entity
    {
        public WeaponType weaponType;
        public float Speed;            // time interval, in milliseconds, at which the weapon can be used
        public int FirePower;          // damage done by the weapon
        public float SoundRadius;      // distance from the wielder at which zombies may be alerted
        public float Range;            // distance at which the weapon can do damage
        public Vector3 offset = new Vector3(0);

        public Weapon(WeaponType type, ref Model model)
            : this(type)
        {
            this.model = model;
        }

        public Weapon(WeaponType type)
        {
            weaponType = type;
            switch (type)
            {
                case WeaponType.BareHands:
                    {
                        FirePower = 10;
                        Speed = 2000;
                        SoundRadius = 20;
                        Range = 5;
                        break;
                    }
                case WeaponType.Handgun9mm:
                    {
                        FirePower = 200;
                        Speed = 500;
                        SoundRadius = 60;
                        Range = 40;
                        offset = new Vector3(-2.3f, 4.5f, -1f);
                        break;
                    }
                case WeaponType.Magnum:
                    {
                        FirePower = 2000;
                        Speed = 2000;
                        SoundRadius = 80;
                        Range = 40;
                        offset = new Vector3(-2.3f, 4.5f, -1f);
                        
                        break;
                    }
                case WeaponType.Vomit:
                    {
                        FirePower = 100;
                        Speed = 2000;
                        SoundRadius = 20;
                        Range = 15;
                        break;
                    }
                case WeaponType.ZombieHands:
                    {
                        FirePower = 100;
                        Speed = 2000;
                        SoundRadius = 20;
                        Range = 5;
                        break;
                    }
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Weapon))
                return false;
            return weaponType.Equals((obj as Weapon).weaponType);
        }
        public override int GetHashCode()
        {
            return (int)weaponType;
        }
    }
}
