﻿using System;
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
        ItemType itemType;

        public Item(ItemType type)
        {
            itemType = type;
        }
    }
}