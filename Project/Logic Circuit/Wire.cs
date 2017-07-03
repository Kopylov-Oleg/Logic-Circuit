using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    [Serializable]
    class Wire
    {
        CircuitComponent beginning;
        CircuitComponent end;

        public CircuitComponent Beginning
        {
            get { return beginning; }
            set { beginning = value; }
        }
        public CircuitComponent End
        {
            get { return end; }
            set { end = value; }
        }

        public void Draw(Graphics graph)
        {
            if (beginning is CircuitJack)
                graph.DrawLine(new Pen(Color.Black), beginning.X, beginning.GetOutputCentralYCoordinate(this), end.X, end.GetInputCentralYCoordinate(this));
            else
                graph.DrawLine(new Pen(Color.Black), beginning.X + GV.radius, beginning.GetOutputCentralYCoordinate(this), end.X, end.GetInputCentralYCoordinate(this));
        }
        public void Draw(Graphics graph, int mouseX, int mouseY)
        {
            if (beginning == null)
                graph.DrawLine(new Pen(Color.Black), mouseX, mouseY, end.X, end.GetInputCentralYCoordinate(this));
            else
            {
                if (beginning is CircuitJack)
                    graph.DrawLine(new Pen(Color.Black), beginning.X, beginning.Y, mouseX, mouseY);
                else
                    graph.DrawLine(new Pen(Color.Black), beginning.X + GV.radius, beginning.GetOutputCentralYCoordinate(this), mouseX, mouseY);
            }
        }

        public Wire(CircuitComponent Point, bool PointIsBeginning)
        {
            if (PointIsBeginning)
                beginning = Point;
            else
                end = Point;
        }
        public Wire(CircuitComponent beginning, CircuitComponent end)
        {
            this.beginning = beginning;
            this.end = end;
        }
    }
}
