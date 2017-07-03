using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    [Serializable]
    abstract class CircuitJack : CircuitComponent
    {
        public abstract bool JackValue { get; set; }
        public abstract string Name { get; set; }
    }
}
