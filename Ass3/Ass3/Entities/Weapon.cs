using System;
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
        WeaponType weaponType;
        float Speed;
        int FirePower;

        public Weapon(WeaponType type)
        {
            weaponType = type;
        }
    }
}
