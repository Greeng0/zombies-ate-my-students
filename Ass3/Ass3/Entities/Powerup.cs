using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Entities
{
    public enum PowerupType
    {
        Sneakers,
        Silencer
    }
    class Powerup : Entity
    {
        public PowerupType Type;

        public Powerup(PowerupType type)
        {
            this.Type = type;
        }
        public Powerup(PowerupType type, ref Model model)
        {
            this.model = model;
            this.Type = type;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Powerup))
                return false;
            return Type.Equals((obj as Powerup).Type);
        }
        public override int GetHashCode()
        {
            return (int)Type;
        }
    }
}
