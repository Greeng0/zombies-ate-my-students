using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace Entities
{
    interface IObserver
    {

        void Notify(Vector3 value);
    }
}
