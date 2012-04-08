﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entities
{
    public enum WeaponType
    {
        BareHands,
        Handgun9mm,
        Magnum
    }
    
    class Weapon
    {
        public WeaponType weaponType;
        public float Speed;            // time interval, in seconds, at which the weapon can be used
        public int FirePower;          // damage done by the weapon
        public float SoundRadius;      // distance from the wielder at which zombies may be alerted

        public Weapon(WeaponType type)
        {
            weaponType = type;
            switch (type)
            {
                case WeaponType.BareHands:
                    {
                        FirePower = 10;
                        Speed = 2;
                        SoundRadius = 20;
                        break;
                    }
                case WeaponType.Handgun9mm:
                    {
                        FirePower = 200;
                        Speed = 0.5f;
                        SoundRadius = 40;
                        break;
                    }
                case WeaponType.Magnum:
                    {
                        FirePower = 1000;
                        Speed = 2;
                        SoundRadius = 50;
                        break;
                    }
            }
        }

        public override bool Equals(object obj)
        {
            return weaponType.Equals((obj as Weapon).weaponType);
        }
        public override int GetHashCode()
        {
            return (int)weaponType;
        }
    }
}
