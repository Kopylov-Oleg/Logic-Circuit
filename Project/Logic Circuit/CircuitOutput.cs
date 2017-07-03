using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    [Serializable]
    class CircuitOutput : CircuitJack
    {
        bool jackvalue;
        int x;
        int y;
        string name;
        Wire output;

        public override bool JackValue
        {
            get { return jackvalue; }
            set { jackvalue = value; }
        }
        public override int X
        {
            get { return x; }
            set { x = value; }
        }
        public override int Y
        {
            get { return y; }
            set { y = value; }
        }

        public override int GetInputCentralYCoordinate(Wire thisWire)
        {
            return y;
        }
        public override int GetOutputCentralYCoordinate(Wire thisWire)
        {
            return y;
        }

        public override string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Wire Output
        {
            get { return output; }
            set { output = value; }
        }

        public override void Draw(Graphics graph)
        {
            graph.FillRectangle(new SolidBrush(Color.Black), x - GV.jackradius, y - GV.jackradius, GV.jackdiameter, GV.jackdiameter);
            if (GV.ShowingResults)
                graph.DrawString((jackvalue) ? "1" : "0", new Font("Arial", 2 * GV.radius / 5), new SolidBrush(Color.Black), x + GV.jackradius, y - 3 * GV.jackradius);
            graph.DrawString(name, new Font("Arial", 2 * GV.radius / 5), new SolidBrush(Color.Black), x + GV.jackradius, y);
        }

        public override bool HitCheck(int mouseX, int mouseY)
        {
            if ((x - GV.jackradius <= mouseX && mouseX <= x + GV.jackradius) && (y - GV.jackradius <= mouseY && mouseY <= y + GV.jackradius))
                return true;
            return false;
        }

        public static string[] GetStringArray(CircuitOutput[] Outputs)
        {
            string[] a = new string[Outputs.Length];
            for (int i = 0; i < a.Length; i++)
                a[i] = Outputs[i].Name;
            return a;
        }

        public CircuitOutput(int x, int y, string name)
        {
            jackvalue = false;
            this.x = x;
            this.y = y;
            this.name = name;
            name = "";
        }
    }
}
