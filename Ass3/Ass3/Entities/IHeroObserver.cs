using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace Entities
{
    interface IHeroObserver
    {
        void Notify(Vector3 value);
        int Targetslot();
    }
}
