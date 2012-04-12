using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Entities;

namespace Entities
{
    public enum ItemType
    {
        Key,
        MedPack,
        Extinguisher
    }

    class Item : Entity
    {
        public ItemType itemType;
        public float SoundRadius;

        public Item(ItemType type, ref Model model) : this(type)
        {
            this.model = model;
        }

        public Item(ItemType type)
        {
            itemType = type;
            switch (type)
            {
                case ItemType.Extinguisher:
                    {
                        SoundRadius = 40;
                        break;
                    }
                case ItemType.Key:
                    {
                        SoundRadius = 15;
                        break;
                    }
                case ItemType.MedPack:
                    {
                        SoundRadius = 10;
                        break;
                    }
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Item))
                return false;
            return itemType.Equals((obj as Item).itemType);
        }
        public override int GetHashCode()
        {
            return (int)itemType;
        }
    }
}
