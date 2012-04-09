using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;

namespace Entities
{
    public enum ItemType
    {
        Key,
        MedPack,
        Extinguisher,
        Handgun9mm,
        Magnum
    }

    class Item : Entity
    {
        public ItemType itemType;
        public float SoundRadius;

        public Item(ItemType type)
        {
            itemType = type;
            SoundRadius = 20;
        }

        public override bool Equals(object obj)
        {
            return itemType.Equals((obj as Item).itemType);
        }
        public override int GetHashCode()
        {
            return (int)itemType;
        }
    }
}
