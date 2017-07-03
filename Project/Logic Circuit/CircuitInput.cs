using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    [Serializable]
    class CircuitInput : CircuitJack
    {
        bool jackvalue;
        int x;
        int y;
        string name;
        LinkedList<Wire> input;


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

        public LinkedList<Wire> Input
        {
            get { return input; }
        }

        public override void Draw(Graphics graph)
        {
            graph.FillRectangle(new SolidBrush(Color.Black), x - GV.jackradius, y - GV.jackradius, GV.jackdiameter, GV.jackdiameter);
            graph.DrawString((jackvalue) ? "1" : "0", new Font("Arial", 2 * GV.radius / 5), new SolidBrush(Color.Black), x - 3 * GV.jackradius, y - 3 * GV.jackradius);
            graph.DrawString(name, new Font("Arial", 2 * GV.radius / 5), new SolidBrush(Color.Black), x - (2 + 3 * name.Length / 2) * GV.jackradius, y + GV.jackradius);
        }

        public override bool HitCheck(int mouseX, int mouseY)
        {
            if ((x - GV.jackradius <= mouseX && mouseX <= x + GV.jackradius) && (y - GV.jackradius <= mouseY && mouseY <= y + GV.jackradius))
                return true;
            return false;
        }

        public static bool[] GetBoolArray(CircuitInput[] Inputs)
        {
            bool[] a = new bool[Inputs.Length];
            for (int i = 0; i < a.Length; i++)
                a[i] = Inputs[i].JackValue;
            return a;
        }
        public static string[] GetStringArray(CircuitInput[] Inputs)
        {
            string[] a = new string[Inputs.Length];
            for (int i = 0; i < a.Length; i++)
                a[i] = Inputs[i].Name;
            return a;
        }

        public CircuitInput(int x, int y, string name)
        {
            jackvalue = false;
            this.x = x;
            this.y = y;
            this.name = name;
            input = new LinkedList<Wire>();
        }
    }
}
