using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    [Serializable]
    abstract class CircuitComponent
    {
        public abstract int X { get; set; }
        public abstract int Y { get; set; }
        public abstract int GetInputCentralYCoordinate(Wire thisWire);
        public abstract int GetOutputCentralYCoordinate(Wire thisWire);
        public abstract void Draw(Graphics graph);
        public abstract bool HitCheck(int mouseX, int mouseY);
    }
}
