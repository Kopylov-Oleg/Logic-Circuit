using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    [Serializable]
    class LogicGate : CircuitComponent
    {
        int x;
        int y;

        char schemesymbol;
        string schemename;

        Wire[] inputs;
        string[] inputsnames;
        LinkedList<Wire>[] outputs;
        string[] outputsnames;

        bool valueisknown;

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
            int maxnumber = Math.Max(inputs.Length, outputs.Length);
            int i;
            for (i = 0; i < inputs.Length; i++)
                if (inputs[i] == thisWire)
                    break;
            return y + i * maxnumber * GV.radius / inputs.Length + ((maxnumber * GV.radius / inputs.Length) / 2 - GV.jackradius) + GV.jackradius;
        }
        public override int GetOutputCentralYCoordinate(Wire thisWire)
        {
            int maxnumber = Math.Max(inputs.Length, outputs.Length);
            int i;
            for (i = 0; i < outputs.Length; i++)
                if (outputs[i].Contains(thisWire))
                    break;
            return y + i * maxnumber * GV.radius / outputs.Length + ((maxnumber * GV.radius / outputs.Length) / 2 - GV.jackradius) + GV.jackradius;
        }

        public char SchemeSymbol
        {
            get { return schemesymbol; }
            set { schemesymbol = value; }
        }
        public string SchemeName
        {
            get { return schemename; }
            set { schemename = value; }
        }

        public Wire[] Inputs
        {
            get { return inputs; }
        }
        public LinkedList<Wire>[] Outputs
        {
            get { return outputs; }
        }

        public bool Valueisknown
        {
            get { return valueisknown; }
            set { valueisknown = value; }
        }

        public override void Draw(Graphics graph)
        {
            int maxnumber = Math.Max(inputs.Length, outputs.Length);
            graph.FillRectangle(new SolidBrush(Color.FromKnownColor(KnownColor.Control)), x, y, GV.radius, maxnumber * GV.radius);
            graph.DrawRectangle(new Pen(Color.Black), x, y, GV.radius, maxnumber * GV.radius);
            for (int i = 0; i < inputs.Length; i++)
            {
                graph.FillRectangle(new SolidBrush(Color.Black), x - GV.jackradius, y + i * maxnumber * GV.radius / inputs.Length + ((maxnumber * GV.radius / inputs.Length) / 2 - GV.jackradius), GV.jackdiameter, GV.jackdiameter);
                graph.DrawString(inputsnames[i], new Font("Arial", 2 * GV.radius / 5), new SolidBrush(Color.Black), x - (2 + 3 * inputsnames[i].Length / 2) * GV.jackradius, y + i * maxnumber * GV.radius / inputsnames.Length + ((maxnumber * GV.radius / inputsnames.Length) / 2 - GV.jackradius) - 3 * GV.jackradius);
            }
            for (int i = 0; i < outputs.Length; i++)
            {
                if (schemesymbol == '!' && GV.SpecialNotOutput)
                {
                    graph.FillRectangle(new SolidBrush(Color.FromKnownColor(KnownColor.Control)), x + GV.radius - GV.jackradius, y + i * maxnumber * GV.radius / outputs.Length + ((maxnumber * GV.radius / outputs.Length) / 2 - GV.jackradius), GV.jackdiameter, GV.jackdiameter);
                    graph.DrawRectangle(new Pen(Color.Black), x + GV.radius - GV.jackradius, y + i * maxnumber * GV.radius / outputs.Length + ((maxnumber * GV.radius / outputs.Length) / 2 - GV.jackradius), GV.jackdiameter, GV.jackdiameter);
                }
                else
                    graph.FillRectangle(new SolidBrush(Color.Black), x + GV.radius - GV.jackradius, y + i * maxnumber * GV.radius / outputs.Length + ((maxnumber * GV.radius / outputs.Length) / 2 - GV.jackradius), GV.jackdiameter, GV.jackdiameter);
                graph.DrawString(outputsnames[i], new Font("Arial", 2 * GV.radius / 5), new SolidBrush(Color.Black), x + GV.radius, y + i * maxnumber * GV.radius / outputsnames.Length + ((maxnumber * GV.radius / outputsnames.Length) / 2 - GV.jackradius) - 3 * GV.jackradius);
            }
            graph.DrawString(schemesymbol.ToString(), new Font("Arial", 2 * GV.radius / 5), new SolidBrush(Color.Black), x + 3 * GV.radius / 10, y + GV.radius / 10);
        }

        public override bool HitCheck(int mouseX, int mouseY)
        {
            if (x <= mouseX && mouseX <= x + GV.radius && y <= mouseY && mouseY <= y + Math.Max(inputs.Length, outputs.Length) * GV.radius)
                return true;
            return false;
        }
        public int InputHitNumber(int mouseX, int mouseY)
        {
            int inputnumber = 0;
            int maxnumber = Math.Max(inputs.Length, outputs.Length);
            if (x - GV.jackradius <= mouseX && mouseX <= x + GV.jackradius)
                for (; inputnumber < inputs.Length; inputnumber++)
                    if (inputs[inputnumber] == null && y + inputnumber * maxnumber * GV.radius / inputs.Length + ((maxnumber * GV.radius / inputs.Length) / 2 - GV.jackradius) <= mouseY && mouseY <= y + inputnumber * maxnumber * GV.radius / inputs.Length + ((maxnumber * GV.radius / inputs.Length) / 2 - GV.jackradius) + GV.jackdiameter)
                        return inputnumber;
            return inputs.Length;
        }
        public int InputHitNumber(Wire thisWire)
        {
            int inputnumber = 0;
            for (; inputnumber < inputs.Length; inputnumber++)
                if (inputs[inputnumber] == thisWire)
                    return inputnumber;
            return inputs.Length;
        }
        public int OutputHitNumber(int mouseX, int mouseY)
        {
            int outputnumber = 0;
            int maxnumber = Math.Max(inputs.Length, outputs.Length);
            if (x + GV.radius - GV.jackradius <= mouseX && mouseX <= x + GV.radius + GV.jackradius)
                for (; outputnumber < outputs.Length; outputnumber++)
                    if (y + outputnumber * maxnumber * GV.radius / outputs.Length + ((maxnumber * GV.radius / outputs.Length) / 2 - GV.jackradius) <= mouseY && mouseY <= y + outputnumber * maxnumber * GV.radius / outputs.Length + ((maxnumber * GV.radius / outputs.Length) / 2 - GV.jackradius) + GV.jackdiameter)
                        return outputnumber;
            return outputs.Length;
        }
        public int OutputHitNumber(Wire thisWire)
        {
            int outputnumber = 0;
            for (; outputnumber < outputs.Length; outputnumber++)
                if (outputs[outputnumber].Contains(thisWire))
                    return outputnumber;
            return outputs.Length;
        }

        public LogicGate(int x, int y, string[] inputsnames, string[] outputsnames, char schemesymbol, string schemename)
        {
            this.x = x;
            this.y = y;
            this.schemesymbol = schemesymbol;
            this.schemename = schemename;
            valueisknown = false;

            inputs = new Wire[inputsnames.Length];
            this.inputsnames = inputsnames;
            outputs = new LinkedList<Wire>[outputsnames.Length];
            for (int i = 0; i < outputs.Length; i++)
                outputs[i] = new LinkedList<Wire>();
            this.outputsnames = outputsnames;
        }
    }
}
