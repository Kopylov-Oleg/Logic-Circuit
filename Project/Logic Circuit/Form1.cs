using System;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using System.Reflection;
using System.Security.Permissions;


namespace Logic_Circuit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;

            version = "1.01";
            showinfoStripLabel.Text = "v. " + version;

            schemesymbol = 's';
            schemename = "newscheme";
            this.Text = schemename;

            GV.radius = 20; // кратно 10
            GV.jackdiameter = 2 * GV.radius / 5;
            GV.jackradius = GV.jackdiameter / 2;
            //GV.firstgap = GV.radius / 2 - GV.jackradius;
            PanelX = 236;
            PanelY = 52;
            PanelWidth = 341;
            PanelHeight = 489;
            BigPanelX = PanelX - GV.jackradius;
            BigPanelY = PanelY - GV.jackradius;
            BigPanelWidth = PanelWidth + GV.jackdiameter;
            BigPanelHeight = PanelHeight + GV.jackdiameter;

            MouseX = 0;
            MouseY = 0;
            FirstPoint = new Point();

            newInputs(2);
            newOutputs(1);
            maxjacksnumber = PanelHeight / GV.jackdiameter;

            LogicGates = new LinkedList<LogicGate>();
            Wires = new LinkedList<Wire>();

            ChosenGate = null;
            ChosenWire = null;

            OperatorsMovement = false;
            WiringFromBeginning = false;
            WiringFromEnd = false;
            TryingToCut = false;

            GV.SpecialNotOutput = false;
            GV.ShowingResults = false;
            GV.FreeDevelopment = true;

            this.Cursor = new Cursor(new Bitmap(Properties.Resources.ClosedHand).GetHicon());
            this.Cursor = new Cursor(new Bitmap(Properties.Resources.OpenHand).GetHicon());
            this.Cursor = new Cursor(new Bitmap(Properties.Resources.Pencil).GetHicon());
            this.Cursor = new Cursor(new Bitmap(Properties.Resources.Scissors).GetHicon());
            this.Cursor = DefaultCursor;

            UsedSchemes = new LinkedList<SchemeContainer>();

            openSchemeDialog.FileName = "newscheme";
            openSchemeDialog.Filter = "Big Scheme Files (*.bsch)|*.bsch|All Files (*.*)|*.*";
            addSchemeDialog.FileName = "newscheme";
            addSchemeDialog.Filter = "Scheme Files (*.bsch, *.ssch)|*.bsch;*.ssch|All Files (*.*)|*.*";
            openTaskDialog.FileName = "newtask";
            openTaskDialog.Filter = "Task Scheme Files (*.tsch)|*.tsch|All Files (*.*)|*.*";

            saveAllFileDialog.Filter = "Big Scheme Files (*.bsch)|*.bsch|All Files (*.*)|*.*";
            saveLogicFileDialog.Filter = "Small Scheme Files (*.ssch)|*.ssch|All Files (*.*)|*.*";
            saveTaskFileDialog.Filter = "Task Scheme Files (*.tsch)|*.tsch|All Files (*.*)|*.*";

            StartOrStopFreeDevelopment();
            свободнаяРазработкаToolStripMenuItem.CheckState = CheckState.Checked;
            Basictasks();

            Refresh();
        }

        string version;

        char schemesymbol;
        string schemename;

        int PanelX;
        int PanelY;
        int PanelWidth;
        int PanelHeight;
        int BigPanelX;
        int BigPanelY;
        int BigPanelWidth;
        int BigPanelHeight;

        int MouseX;
        int MouseY;
        Point FirstPoint;

        CircuitInput[] Inputs;
        CircuitOutput[] Outputs;
        int maxjacksnumber;

        LinkedList<LogicGate> LogicGates;
        LinkedList<Wire> Wires;

        CircuitComponent ChosenGate;
        Wire ChosenWire;

        bool OperatorsMovement;
        bool WiringFromBeginning;
        bool WiringFromEnd;
        bool TryingToCut;

        LinkedList<SchemeContainer> UsedSchemes;

        bool lastsavewasfull;

        Task ActiveTask;
        Task[] BasicTasks;

        #region Отрисовка

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphReload = e.Graphics;
            graphReload.Clear(BackColor);
            DrawEverything(graphReload);
        }
        void DrawEverything(Graphics g)
        {
            g.DrawRectangle(new Pen(Color.Black), PanelX, PanelY, PanelWidth, PanelHeight);

            if (LogicGates.Count != 0)
                for (LinkedListNode<LogicGate> i = LogicGates.Last; ; i = i.Previous)
                {
                    i.Value.Draw(g);
                    if (i.Previous == null)
                        break;
                }
            if (Wires.Count != 0)
                foreach (Wire i in Wires)
                {
                    if (i == Wires.Last.Value && (i.Beginning == null || i.End == null))
                        i.Draw(g, MouseX, MouseY);
                    else
                        i.Draw(g);
                }

            for (int i = 0; i < Inputs.Length; i++)
                Inputs[i].Draw(g);
            for (int i = 0; i < Outputs.Length; i++)
                Outputs[i].Draw(g);

            if (TryingToCut)
                g.DrawLine(new Pen(Color.DarkRed), FirstPoint.X, FirstPoint.Y, MouseX, MouseY);
        }

        #endregion

        #region Меню

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewScheme();
        }
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenScheme();
        }
        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddScheme();
        }
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveScheme();
        }
        private void всюСхемуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAll();
        }
        private void толькоЛогическуюСоставляющеюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveLogic();
        }
        private void сохранитьКакЗадачуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveTask();
        }
        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void переименоватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameScheme();
        }

        private void инверсныйВыходВСхемеНЕToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = this.инверсныйВыходВСхемеНЕToolStripMenuItem;
            if (tsmi.CheckState == CheckState.Unchecked)
            {
                tsmi.CheckState = CheckState.Checked;
                GV.SpecialNotOutput = true;
            }
            else
            {
                tsmi.CheckState = CheckState.Unchecked;
                GV.SpecialNotOutput = false;
            }
            Refresh();
        }

        private void свободнаяРазработкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = true;
            ClearCheckStates();
            свободнаяРазработкаToolStripMenuItem.CheckState = CheckState.Checked;
            NewScheme();
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void иЛИНаБазеИИНЕToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            иЛИНаБазеИИНЕToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[0];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void иЛИ3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            иЛИ3ToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[1];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void иЛИ4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            иЛИ4ToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[2];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void равенствоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            равенствоToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[3];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void исключающееИЛИToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            исключающееИЛИToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[4];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void мультиплексорToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            мультиплексорToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[5];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void большинствоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            большинствоToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[6];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void разрядСуммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            разрядСуммыToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[7];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void одноразрядныйСумматорToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            одноразрядныйСумматорToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[8];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void одноразрядныйСумматорСПереносомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            одноразрядныйСумматорСПереносомToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[9];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void двухразрядныйСумматорСПереносомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            двухразрядныйСумматорСПереносомToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[10];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void четырехразрядныйСумматорСПереносомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            четырехразрядныйСумматорСПереносомToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[11];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }
        private void многоразрядныйУмножительToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            многоразрядныйУмножительToolStripMenuItem.CheckState = CheckState.Checked;
            ActiveTask = BasicTasks[12];
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }

        private void решитьСозданнуюРанееЗадачуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            решитьСозданнуюРанееЗадачуToolStripMenuItem.CheckState = CheckState.Checked;
            OpenTask();
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }

        private void заданиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowGoal();
        }
        private void результатToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowResult();
        }

        private void содержаниеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowInfo();
        }

        #endregion

        #region Полоса инструментов

        private void newschemeStripButton_Click(object sender, EventArgs e)
        {
            NewScheme();
        }
        private void openschemeStripButton_Click(object sender, EventArgs e)
        {
            OpenScheme();
        }
        private void addschemeStripButton_Click(object sender, EventArgs e)
        {
            AddScheme();
        }
        private void savedinstructionStripButton_Click(object sender, EventArgs e)
        {
            GV.FreeDevelopment = false;
            ClearCheckStates();
            решитьСозданнуюРанееЗадачуToolStripMenuItem.CheckState = CheckState.Checked;
            OpenTask();
            Starttask(ActiveTask);
            StartOrStopFreeDevelopment();
            Refresh();
        }

        private void saveschemeStripButton_Click(object sender, EventArgs e)
        {
            SaveScheme();
        }
        private void saveallStripButton_Click(object sender, EventArgs e)
        {
            SaveAll();
        }
        private void savelogicStripButton_Click(object sender, EventArgs e)
        {
            SaveLogic();
        }
        private void saveastaskButton_Click(object sender, EventArgs e)
        {
            SaveTask();
        }

        private void renameStripButton_Click(object sender, EventArgs e)
        {
            RenameScheme();
        }

        private void instructionStripButton_Click(object sender, EventArgs e)
        {
            ShowGoal();
        }
        private void showresultStripButton_Click(object sender, EventArgs e)
        {
            ShowResult();
        }

        private void showhelpStripButton_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }
        private void showinfoStripLabel_Click(object sender, EventArgs e)
        {
            ShowInfo();
        }

        #endregion

        #region Подменю

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogicGateDelete(sender, e);
            Refresh();
        }

        private void инвертироватьЗначениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (ChosenGate as CircuitInput).JackValue = !(ChosenGate as CircuitInput).JackValue;
            Refresh();
        }
        private void изменитьНазваниеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeNameForm ChangeName = new ChangeNameForm((ChosenGate as CircuitJack).Name);
            ChangeName.ShowDialog();
            (ChosenGate as CircuitJack).Name = ChangeName.name;
            Refresh();
        }
        private void переместитьВышеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Outputs.Contains(ChosenGate))
            {
                MoveLeftInMassive(Outputs, ChosenGate as CircuitOutput);
                newOutputs(Outputs.Length);
            }
            else
            {
                MoveLeftInMassive(Inputs, ChosenGate as CircuitInput);
                newInputs(Inputs.Length);
            }
            Refresh();
        }
        private void переместитьНижеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Outputs.Contains(ChosenGate))
            {
                MoveRightInMassive(Outputs, ChosenGate as CircuitOutput);
                newOutputs(Outputs.Length);
            }
            else
            {
                MoveRightInMassive(Inputs, ChosenGate as CircuitInput);
                newInputs(Inputs.Length);
            }
            Refresh();
        }
        private void удалитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            JackDelete(sender, e);
            Refresh();
        }

        #endregion

        #region Добавление элементов схемы

        private void addinputbutton_Click(object sender, EventArgs e)
        {
            if (Inputs.Length <= maxjacksnumber)
                newInputs(Inputs.Length + 1);
            Refresh();
        }
        private void addoutputbutton_Click(object sender, EventArgs e)
        {
            if (Outputs.Length <= maxjacksnumber)
                newOutputs(Outputs.Length + 1);
            Refresh();
        }

        private void addandbutton_Click(object sender, EventArgs e)
        {
            LogicGates.AddFirst(new LogicGate(PanelX + 2 * GV.radius, PanelY + GV.radius, new string[2] { "a0", "a1" }, new string[1] { "b0" }, '&', "И"));
            Refresh();
        }
        private void addorbutton_Click(object sender, EventArgs e)
        {
            LogicGates.AddFirst(new LogicGate(PanelX + 2 * GV.radius, PanelY + GV.radius, new string[2] { "a0", "a1" }, new string[1] { "b0" }, '1', "ИЛИ"));
            Refresh();
        }
        private void addnotbutton_Click(object sender, EventArgs e)
        {
            LogicGates.AddFirst(new LogicGate(PanelX + 2 * GV.radius, PanelY + GV.radius, new string[1] { "a0" }, new string[1] { "b0" }, '!', "НЕ"));
            Refresh();
        }
        private void addzerobutton_Click(object sender, EventArgs e)
        {
            LogicGates.AddFirst(new LogicGate(PanelX + 2 * GV.radius, PanelY + GV.radius, new string[0] { }, new string[1] { "b0" }, '0', "Сигнал \"0\""));
            Refresh();
        }

        #endregion

        #region Работа мышью

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) && BigPanelHit(e.X, e.Y) && !OperatorsMovement && !WiringFromBeginning && !WiringFromEnd && !TryingToCut && !GV.ShowingResults)
            {
                bool jackhit = false;
                for (int i = 0; i < Inputs.Length; i++)
                    if (Inputs[i].HitCheck(e.X, e.Y))
                    {
                        ChosenGate = Inputs[i];
                        if (e.Button == MouseButtons.Left)
                        {
                            ChosenWire = new Wire(ChosenGate, true);
                            Wires.AddLast(ChosenWire);
                            Inputs[i].Input.AddLast(ChosenWire);
                            MouseX = e.X;
                            MouseY = e.Y;
                            WiringFromBeginning = true;
                            this.Cursor = new Cursor(new Bitmap(Properties.Resources.Pencil).GetHicon());
                        }
                        else
                        {
                            инвертироватьЗначениеToolStripMenuItem.Enabled = true;
                            изменитьНазваниеToolStripMenuItem.Enabled = GV.FreeDevelopment;
                            переместитьВышеToolStripMenuItem.Enabled = GV.FreeDevelopment && !(Tools.GetNumberInArray(Inputs, ChosenGate as CircuitInput) == 0);
                            переместитьНижеToolStripMenuItem.Enabled = GV.FreeDevelopment && !(Tools.GetNumberInArray(Inputs, ChosenGate as CircuitInput) == Inputs.Length - 1);
                            удалитьToolStripMenuItem1.Enabled = GV.FreeDevelopment && (Inputs.Length > 1);
                            ContextMenuStripJack.Show(Cursor.Position.X, Cursor.Position.Y);
                        }
                        jackhit = true;
                        break;
                    }

                if (!jackhit)
                    for (int i = 0; i < Outputs.Length; i++)
                        if (Outputs[i].HitCheck(e.X, e.Y))
                        {
                            ChosenGate = Outputs[i];
                            if (e.Button == MouseButtons.Left)
                            {
                                if (Outputs[i].Output == null)
                                {
                                    ChosenWire = new Wire(ChosenGate, false);
                                    Wires.AddLast(ChosenWire);
                                    Outputs[i].Output = ChosenWire;
                                    MouseX = e.X;
                                    MouseY = e.Y;
                                    WiringFromEnd = true;
                                    this.Cursor = new Cursor(new Bitmap(Properties.Resources.Pencil).GetHicon());
                                }
                            }
                            else
                            {
                                инвертироватьЗначениеToolStripMenuItem.Enabled = false;
                                изменитьНазваниеToolStripMenuItem.Enabled = GV.FreeDevelopment;
                                переместитьВышеToolStripMenuItem.Enabled = GV.FreeDevelopment && !(Tools.GetNumberInArray(Outputs, ChosenGate as CircuitOutput) == 0);
                                переместитьНижеToolStripMenuItem.Enabled = GV.FreeDevelopment && !(Tools.GetNumberInArray(Outputs, ChosenGate as CircuitOutput) == Outputs.Length - 1);
                                удалитьToolStripMenuItem1.Enabled = GV.FreeDevelopment && (Outputs.Length > 1);
                                ContextMenuStripJack.Show(Cursor.Position.X, Cursor.Position.Y);
                            }
                            jackhit = true;
                            break;
                        }

                if (!jackhit)
                {
                    bool remove = false;
                    foreach (LogicGate i in LogicGates)
                    {
                        ChosenGate = i;
                        if (i.OutputHitNumber(e.X, e.Y) != i.Outputs.Length)
                        {
                            WiringFromBeginning = true;
                            this.Cursor = new Cursor(new Bitmap(Properties.Resources.Pencil).GetHicon());
                            break;
                        }
                        else if (i.InputHitNumber(e.X, e.Y) != i.Inputs.Length)
                        {
                            WiringFromEnd = true;
                            this.Cursor = new Cursor(new Bitmap(Properties.Resources.Pencil).GetHicon());
                            break;
                        }

                        else if (i.HitCheck(e.X, e.Y))
                        {
                            if (e.Button == MouseButtons.Right)
                                remove = true;
                            else
                            {
                                OperatorsMovement = true;
                                this.Cursor = new Cursor(new Bitmap(Properties.Resources.ClosedHand).GetHicon());
                            }
                            break;
                        }
                    }
                    if (e.Button == MouseButtons.Left)
                    {
                        if (WiringFromBeginning)
                        {
                            ChosenWire = new Wire(ChosenGate, true);
                            Wires.AddLast(ChosenWire);
                            (ChosenGate as LogicGate).Outputs[(ChosenGate as LogicGate).OutputHitNumber(e.X, e.Y)].AddLast(ChosenWire);
                            FirstPoint.X = e.X;
                            FirstPoint.Y = e.Y;
                            MouseX = e.X;
                            MouseY = e.Y;
                        }
                        else if (WiringFromEnd)
                        {
                            ChosenWire = new Wire(ChosenGate, false);
                            Wires.AddLast(ChosenWire);
                            (ChosenGate as LogicGate).Inputs[(ChosenGate as LogicGate).InputHitNumber(e.X, e.Y)] = ChosenWire;
                            FirstPoint.X = e.X;
                            FirstPoint.Y = e.Y;
                            MouseX = e.X;
                            MouseY = e.Y;
                        }
                        else if (OperatorsMovement)
                        {
                            LogicGates.Remove(LogicGates.Find(ChosenGate as LogicGate));
                            LogicGates.AddFirst(ChosenGate as LogicGate);
                            MouseX = e.X - ChosenGate.X;
                            MouseY = e.Y - ChosenGate.Y;
                        }
                        else
                        {
                            TryingToCut = true;
                            FirstPoint.X = e.X;
                            FirstPoint.Y = e.Y;
                            MouseX = e.X;
                            MouseY = e.Y;
                            this.Cursor = new Cursor(new Bitmap(Properties.Resources.Scissors).GetHicon());
                        }
                    }
                    else
                    {
                        WiringFromEnd = false;
                        WiringFromBeginning = false;
                        OperatorsMovement = false;
                        if (remove)
                        {
                            logicGateToolStripMenuItem.Text = (ChosenGate as LogicGate).SchemeName;
                            ContextMenuStripLogicGateDelete.Show(Cursor.Position.X, Cursor.Position.Y);
                        }
                        else
                            ChosenGate = null;
                    }
                }
                Refresh();
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (OperatorsMovement)
            {
                if (e.X < PanelX + MouseX)
                    ChosenGate.X = PanelX;
                else if (e.X > PanelX + PanelWidth - (GV.radius - MouseX))
                    ChosenGate.X = PanelX + PanelWidth - GV.radius;
                else ChosenGate.X = e.X - MouseX;

                if (e.Y < PanelY + MouseY)
                    ChosenGate.Y = PanelY;
                else if (e.Y > PanelY + PanelHeight - (Math.Max((ChosenGate as LogicGate).Inputs.Length, (ChosenGate as LogicGate).Outputs.Length) * GV.radius - MouseY))
                    ChosenGate.Y = PanelY + PanelHeight - Math.Max((ChosenGate as LogicGate).Inputs.Length, (ChosenGate as LogicGate).Outputs.Length) * GV.radius;
                else ChosenGate.Y = e.Y - MouseY;

                Refresh();
            }
            else if (WiringFromBeginning || WiringFromEnd || TryingToCut)
            {
                if (e.X < PanelX)
                {
                    MouseX = PanelX;
                    //MouseY = (MouseX * (e.Y - FirstPoint.Y) + (FirstPoint.Y * e.X - e.Y * FirstPoint.X)) / (e.X - FirstPoint.X);
                }
                else if (e.X > PanelX + PanelWidth)
                {
                    MouseX = PanelX + PanelWidth;
                    //MouseY = (MouseX * (e.Y - FirstPoint.Y) + (FirstPoint.Y * e.X - e.Y * FirstPoint.X)) / (e.X - FirstPoint.X);
                }
                else MouseX = e.X;

                if (e.Y < PanelY)
                {
                    MouseY = PanelY;
                    //MouseX = (MouseY * (e.X - FirstPoint.X) + e.Y * FirstPoint.X - FirstPoint.Y * e.X) / (e.Y - FirstPoint.Y);
                }
                else if (e.Y > PanelY + PanelHeight)
                {
                    MouseY = PanelY + PanelHeight;
                    //MouseX = (MouseY * (e.X - FirstPoint.X) + e.Y * FirstPoint.X - FirstPoint.Y * e.X) / (e.Y - FirstPoint.Y);
                }
                else MouseY = e.Y;

                Refresh();
            }
            else if (!GV.ShowingResults)
                MakeRightCursor(e.X, e.Y);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !GV.ShowingResults)
            {
                if (WiringFromBeginning)
                {
                    foreach (CircuitOutput i in Outputs)
                    {
                        if (i != ChosenGate && i.HitCheck(MouseX, MouseY) && i.Output == null)
                        {
                            i.Output = ChosenWire;
                            ChosenWire.End = i;
                            ChosenWire = null;
                            break;
                        }
                    }
                    if (ChosenWire != null)
                        foreach (LogicGate i in LogicGates)
                        {
                            if (i != ChosenGate && i.InputHitNumber(MouseX, MouseY) != i.Inputs.Length)
                            {
                                i.Inputs[i.InputHitNumber(MouseX, MouseY)] = ChosenWire;
                                ChosenWire.End = i;
                                ChosenWire = null;
                                break;
                            }
                        }
                    if (ChosenWire != null)
                    {
                        Wires.RemoveLast();
                        if (ChosenGate is CircuitInput)
                            (ChosenGate as CircuitInput).Input.RemoveLast();
                        else
                            (ChosenGate as LogicGate).Outputs[(ChosenGate as LogicGate).OutputHitNumber(ChosenWire)].RemoveLast();
                        ChosenWire = null;
                    }
                    WiringFromBeginning = false;
                    MakeRightCursor(e.X, e.Y);
                }
                else if (WiringFromEnd)
                {
                    foreach (CircuitInput i in Inputs)
                    {
                        if (i != ChosenGate && i.HitCheck(MouseX, MouseY))
                        {
                            i.Input.AddLast(ChosenWire);
                            ChosenWire.Beginning = i;
                            ChosenWire = null;
                            break;
                        }
                    }
                    if (ChosenWire != null)
                        foreach (LogicGate i in LogicGates)
                        {
                            if (i != ChosenGate && i.OutputHitNumber(MouseX, MouseY) != i.Outputs.Length)
                            {
                                i.Outputs[i.OutputHitNumber(MouseX, MouseY)].AddLast(ChosenWire);
                                ChosenWire.Beginning = i;
                                ChosenWire = null;
                                break;
                            }
                        }
                    if (ChosenWire != null)
                    {
                        Wires.RemoveLast();
                        if (ChosenGate is CircuitOutput)
                            (ChosenGate as CircuitOutput).Output = null;
                        else
                            (ChosenGate as LogicGate).Inputs[(ChosenGate as LogicGate).InputHitNumber(ChosenWire)] = null;
                        ChosenWire = null;
                    }
                    WiringFromEnd = false;
                    MakeRightCursor(e.X, e.Y);
                }
                else if (TryingToCut)
                {
                    if (Wires.Count != 0)
                    {
                        LinkedListNode<Wire> i = Wires.Last;
                        while (true)
                        {
                            if (CrossingCheck(FirstPoint.X, FirstPoint.Y, MouseX, MouseY, i.Value.Beginning.X + ((i.Value.Beginning is LogicGate) ? GV.radius : 0), i.Value.Beginning.GetOutputCentralYCoordinate(i.Value), i.Value.End.X, i.Value.End.GetInputCentralYCoordinate(i.Value)))
                            {
                                if (i.Value.Beginning is CircuitInput)
                                    (i.Value.Beginning as CircuitInput).Input.Remove(i.Value);
                                else
                                    (i.Value.Beginning as LogicGate).Outputs[(i.Value.Beginning as LogicGate).OutputHitNumber(i.Value)].Remove(i.Value);

                                if (i.Value.End is CircuitOutput)
                                    (i.Value.End as CircuitOutput).Output = null;
                                else
                                    (i.Value.End as LogicGate).Inputs[(i.Value.End as LogicGate).InputHitNumber(i.Value)] = null;

                                Wires.Remove(i.Value);
                                if (Wires.Count != 0)
                                {
                                    i = Wires.Last;
                                    continue;
                                }
                            }
                            if (i.Previous == null)
                                break;
                            i = i.Previous;
                        }
                    }
                    MouseX = 0;
                    MouseY = 0;
                    TryingToCut = false;
                    MakeRightCursor(e.X, e.Y);
                }
                else if (OperatorsMovement)
                {
                    /*
                    if (e.X < PanelX + MouseX)
                        ChosenGate.X = PanelX;
                    else if (e.X > PanelX + PanelWidth - (GV.radius - MouseX))
                        ChosenGate.X = PanelX + PanelWidth - GV.radius;
                    else ChosenGate.X = e.X - MouseX;

                    if (e.Y < PanelY + MouseY)
                        ChosenGate.Y = PanelY;
                    else if (e.Y > PanelY + PanelHeight - (2 * GV.radius - MouseY))
                        ChosenGate.Y = PanelY + PanelHeight - 2 * GV.radius;
                    else ChosenGate.Y = e.Y - MouseY;
                    */
                    MouseX = 0;
                    MouseY = 0;
                    OperatorsMovement = false;
                    MakeRightCursor(e.X, e.Y);
                }
                ChosenGate = null;

                Refresh();
                Refresh();
            }
        }

        #endregion

        #region Работа схемы

        private void runschemebutton_Click(object sender, EventArgs e)
        {
            if (!GV.ShowingResults)
            {
                string scheme = Scheme.ConvertToScheme(schemename, Inputs, Outputs, LogicGates, Wires);
                foreach (LogicGate i in LogicGates)
                {
                    i.Valueisknown = false;
                }
                if (scheme == null)
                    MessageBox.Show("Ошибка при обработке схемы!");
                else
                {
                    richTextBox1.Text = scheme;
                    bool[] b = Scheme.RunScheme(scheme, UsedSchemes, CircuitInput.GetBoolArray(Inputs));
                    if (b == null || b.Length != Outputs.Length)
                        MessageBox.Show("Ошибка при попытке запустить схему!");
                    else
                        for (int i = 0; i < Outputs.Length; i++)
                            Outputs[i].JackValue = b[i];
                }
                GV.ShowingResults = true;
            }
            else
            {
                richTextBox1.Text = "";
                GV.ShowingResults = false;
            }
            StartOrStopShowingResults();
            Refresh();
        }

        #endregion

        #region Инструменты

        void ClearCheckStates()
        {
            свободнаяРазработкаToolStripMenuItem.CheckState = CheckState.Unchecked;

            иЛИНаБазеИИНЕToolStripMenuItem.CheckState = CheckState.Unchecked;

            иЛИ3ToolStripMenuItem.CheckState = CheckState.Unchecked;

            иЛИ4ToolStripMenuItem.CheckState = CheckState.Unchecked;

            равенствоToolStripMenuItem.CheckState = CheckState.Unchecked;

            исключающееИЛИToolStripMenuItem.CheckState = CheckState.Unchecked;

            мультиплексорToolStripMenuItem.CheckState = CheckState.Unchecked;

            большинствоToolStripMenuItem.CheckState = CheckState.Unchecked;

            разрядСуммыToolStripMenuItem.CheckState = CheckState.Unchecked;

            одноразрядныйСумматорToolStripMenuItem.CheckState = CheckState.Unchecked;

            одноразрядныйСумматорСПереносомToolStripMenuItem.CheckState = CheckState.Unchecked;

            двухразрядныйСумматорСПереносомToolStripMenuItem.CheckState = CheckState.Unchecked;

            четырехразрядныйСумматорСПереносомToolStripMenuItem.CheckState = CheckState.Unchecked;

            многоразрядныйУмножительToolStripMenuItem.CheckState = CheckState.Unchecked;

            решитьСозданнуюРанееЗадачуToolStripMenuItem.CheckState = CheckState.Unchecked;
        }

        void LogicGateDelete(object sender, EventArgs e)
        {
            for (int i = 0; i < (ChosenGate as LogicGate).Outputs.Length; i++)
            {
                foreach (Wire j in (ChosenGate as LogicGate).Outputs[i])
                {
                    if (j.End is CircuitOutput)
                        (j.End as CircuitOutput).Output = null;
                    else if (j.End is CircuitInput)
                        (j.End as CircuitInput).Input.Remove(j);
                    else
                        (j.End as LogicGate).Inputs[(j.End as LogicGate).InputHitNumber(j)] = null;
                    Wires.Remove(j);
                }
            }
            for (int i = 0; i < (ChosenGate as LogicGate).Inputs.Length; i++)
            {
                if ((ChosenGate as LogicGate).Inputs[i] != null)
                {
                    if ((ChosenGate as LogicGate).Inputs[i].Beginning is CircuitOutput)
                        (ChosenGate as LogicGate).Inputs[i].Beginning = null;
                    else if ((ChosenGate as LogicGate).Inputs[i].Beginning is CircuitInput)
                        ((ChosenGate as LogicGate).Inputs[i].Beginning as CircuitInput).Input.Remove((ChosenGate as LogicGate).Inputs[i]);
                    else
                        ((ChosenGate as LogicGate).Inputs[i].Beginning as LogicGate).Outputs[((ChosenGate as LogicGate).Inputs[i].Beginning as LogicGate).OutputHitNumber((ChosenGate as LogicGate).Inputs[i])].Remove((ChosenGate as LogicGate).Inputs[i]);
                    Wires.Remove((ChosenGate as LogicGate).Inputs[i]);
                }
            }
            //LogicalOperators.Remove(LogicalOperators.Find(ChosenOperator));
            LogicGates.Remove(ChosenGate as LogicGate);
            if (UsedSchemes.Count != 0)
                for (LinkedListNode<SchemeContainer> i = UsedSchemes.Last; ; i = i.Previous)
                {
                    if (i.Value.SchemeName == (ChosenGate as LogicGate).SchemeName)
                    {
                        UsedSchemes.Remove(i);
                        break;
                    }
                    if (i.Previous == null)
                        break;
                }
            ChosenGate = null;
            Refresh();
        }
        void JackDelete(object sender, EventArgs e)
        {
            if (ChosenGate is CircuitInput)
            {
                foreach (Wire i in (ChosenGate as CircuitInput).Input)
                {
                    if (i.End is CircuitOutput)
                        (i.End as CircuitOutput).Output = null;
                    else if (i.End is CircuitInput)
                        (i.End as CircuitInput).Input.Remove(i);
                    else
                        (i.End as LogicGate).Inputs[(i.End as LogicGate).InputHitNumber(i)] = null;
                    Wires.Remove(i);
                }
                //Inputs = DeleteFromMassive(Inputs, (ChosenGate as CircuitJack));
                CircuitInput[] c = new CircuitInput[Inputs.Length - 1];
                byte isalreadydeleted = 0;
                for (int i = 0; i < Inputs.Length; i++)
                {
                    if (Inputs[i] == ChosenGate)
                        isalreadydeleted = 1;
                    else
                        c[i - isalreadydeleted] = Inputs[i];
                }
                Inputs = c;
                newInputs(Inputs.Length);
            }
            else
            {
                if ((ChosenGate as CircuitOutput).Output != null)
                {
                    Wire i = (ChosenGate as CircuitOutput).Output;
                    if (i.Beginning is CircuitOutput)
                        (i.Beginning as CircuitOutput).Output = null;
                    else if (i.Beginning is CircuitInput)
                        (i.Beginning as CircuitInput).Input.Remove(i);
                    else
                        (i.Beginning as LogicGate).Outputs[(i.Beginning as LogicGate).OutputHitNumber(i)].Remove(i);
                    Wires.Remove(i);
                }
                //Outputs = DeleteFromMassive(Outputs, (ChosenGate));
                CircuitOutput[] c = new CircuitOutput[Outputs.Length - 1];
                byte isalreadydeleted = 0;
                for (int i = 0; i < Outputs.Length; i++)
                {
                    if (Outputs[i] == ChosenGate)
                        isalreadydeleted = 1;
                    else
                        c[i - isalreadydeleted] = Outputs[i];
                }
                Outputs = c;
                newOutputs(Outputs.Length);
            }
            ChosenGate = null;
            Refresh();
        }

        bool PanelHit(int x, int y)
        {
            if (PanelX <= x && x <= PanelX + PanelWidth && PanelY <= y && y <= PanelY + PanelHeight)
                return true;
            return false;
        }
        bool BigPanelHit(int x, int y)
        {
            if (BigPanelX <= x && x <= BigPanelX + BigPanelWidth && BigPanelY <= y && y <= BigPanelY + BigPanelHeight)
                return true;
            return false;
        }
        bool CrossingCheck(int l2x1, int l2y1, int l2x2, int l2y2, int l1x1, int l1y1, int l1x2, int l1y2)
        {
            if (Math.Min(l2x1, l2x2) > Math.Max(l1x1, l1x2) || Math.Min(l1x1, l1x2) > Math.Max(l2x1, l2x2) || Math.Min(l2y1, l2y2) > Math.Max(l1y1, l1y2) || Math.Min(l1y1, l1y2) > Math.Max(l2y1, l2y2))
                return false;
            if (l1x1 == l1x2 && l2x1 == l2x2)
                return false;
            else if (l1x1 == l1x2)
            {
                double k2 = (l2y1 - l2y2) / (double)(l2x1 - l2x2);
                double b2 = (l2x1 * l2y2 - l2y1 * l2x2) / (double)(l2x1 - l2x2);
                double y0 = l1x1 * k2 + b2;
                if (Math.Min(l1y1, l1y2) > y0 || Math.Max(l1y1, l1y2) < y0 || Math.Min(l2y1, l2y2) > y0 || Math.Max(l2y1, l2y2) < y0)
                    return false;
            }
            else if (l2x1 == l2x2)
            {
                double k1 = (l1y1 - l1y2) / (double)(l1x1 - l1x2);
                double b1 = (l1x1 * l1y2 - l1y1 * l1x2) / (double)(l1x1 - l1x2);
                double y0 = l2x1 * k1 + b1;
                if (Math.Min(l1y1, l1y2) > y0 || Math.Max(l1y1, l1y2) < y0 || Math.Min(l2y1, l2y2) > y0 || Math.Max(l2y1, l2y2) < y0)
                    return false;
            }
            else
            {
                double k1 = (l1y1 - l1y2) / (double)(l1x1 - l1x2);
                double b1 = (l1x1 * l1y2 - l1y1 * l1x2) / (double)(l1x1 - l1x2);

                double k2 = (l2y1 - l2y2) / (double)(l2x1 - l2x2);
                double b2 = (l2x1 * l2y2 - l2y1 * l2x2) / (double)(l2x1 - l2x2);

                double x0 = (b2 - b1) / (k1 - k2);
                if (Math.Min(l1x1, l1x2) > x0 || Math.Max(l1x1, l1x2) < x0 || Math.Min(l2x1, l2x2) > x0 || Math.Max(l2x1, l2x2) < x0)
                    return false;


                /*if ((l2y1 >= l2x1 * k + b) == (l2y2 >= l2x2 * k + b))
                    return false;*/
            }
            return true;
        }
        //Point 

        void newInputs(int inputsnumber)
        {
            CircuitInput[] a = Inputs;
            Inputs = new CircuitInput[inputsnumber];
            int gapbetweeninputs = (PanelHeight - GV.jackradius * inputsnumber) / (inputsnumber + 1);
            for (int i = 0; i < inputsnumber; i++)
            {
                if (a == null || i >= a.Length)
                {
                    int name = 0;
                    for (int j = 0; j < Inputs.Length; j++)
                        if (Inputs[j] != null && Inputs[j].Name == "a" + name.ToString())
                        {
                            name++;
                            j = -1;
                        }
                    Inputs[i] = new CircuitInput(PanelX, PanelY + (i + 1) * gapbetweeninputs + i * GV.jackradius, "a" + name.ToString());
                }
                else
                {
                    Inputs[i] = a[i];
                    Inputs[i].Y = PanelY + (i + 1) * gapbetweeninputs + i * GV.jackradius;
                }
            }
        }
        void newInputs(string[] inputs)
        {
            Inputs = new CircuitInput[inputs.Length];
            int gapbetweeninputs = (PanelHeight - GV.jackradius * inputs.Length) / (inputs.Length + 1);
            for (int i = 0; i < inputs.Length; i++)
                Inputs[i] = new CircuitInput(PanelX, PanelY + (i + 1) * gapbetweeninputs + i * GV.jackradius, inputs[i]);

        }
        void newOutputs(int outputsnumber)
        {
            CircuitOutput[] b = Outputs;
            Outputs = new CircuitOutput[outputsnumber];
            int gapbetweenoutputs = (PanelHeight - GV.jackradius * outputsnumber) / (outputsnumber + 1);
            for (int i = 0; i < outputsnumber; i++)
            {
                if (b == null || i >= b.Length)
                {
                    int name = 0;
                    for (int j = 0; j < Outputs.Length; j++)
                        if (Outputs[j] != null && Outputs[j].Name == "b" + name.ToString())
                        {
                            name++;
                            j = -1;
                        }
                    Outputs[i] = new CircuitOutput(PanelX + PanelWidth, PanelY + (i + 1) * gapbetweenoutputs + i * GV.jackradius, "b" + name.ToString());
                }
                else
                {
                    Outputs[i] = b[i];
                    Outputs[i].Y = PanelY + (i + 1) * gapbetweenoutputs + i * GV.jackradius;
                }
            }
        }
        void newOutputs(string[] outputs)
        {
            CircuitOutput[] b = Outputs;
            Outputs = new CircuitOutput[outputs.Length];
            int gapbetweenoutputs = (PanelHeight - GV.jackradius * outputs.Length) / (outputs.Length + 1);
            for (int i = 0; i < outputs.Length; i++)
                Outputs[i] = new CircuitOutput(PanelX + PanelWidth, PanelY + (i + 1) * gapbetweenoutputs + i * GV.jackradius, outputs[i]);
        }

        CircuitJack[] MoveLeftInMassive(CircuitJack[] a, CircuitJack b)
        {
            int movedfromnumber = (int)Tools.GetNumberInArray(a, b);
            CircuitJack c = a[movedfromnumber - 1];
            a[movedfromnumber - 1] = a[movedfromnumber];
            a[movedfromnumber] = c;
            return a;
        }
        CircuitJack[] MoveRightInMassive(CircuitJack[] a, CircuitJack b)
        {
            int movedfromnumber = (int)Tools.GetNumberInArray(a, b);
            CircuitJack c = a[movedfromnumber + 1];
            a[movedfromnumber + 1] = a[movedfromnumber];
            a[movedfromnumber] = c;
            return a;
        }
        /* CircuitJack[] DeleteFromMassive(CircuitJack[] a, CircuitJack b)
         {
             CircuitJack[] c = new CircuitJack[a.Length - 1];
             byte isalreadydeleted = 0;
             for (int i = 0; i < a.Length; i++)
             {
                 if (a[i] == b)
                     isalreadydeleted = 1;
                 else
                     c[i - isalreadydeleted] = a[i];
             }
             return c;
         }*/

        void MakeRightCursor(int x, int y)
        {
            bool abletomove = false;
            if (PanelHit(x, y))
            {
                foreach (LogicGate i in LogicGates)
                {
                    if (i.HitCheck(x, y) && i.InputHitNumber(x, y) == i.Inputs.Length && i.OutputHitNumber(x, y) == i.Outputs.Length)
                    {
                        this.Cursor = new Cursor(new Bitmap(Properties.Resources.OpenHand).GetHicon());
                        abletomove = true;
                        break;
                    }
                }
            }
            if (this.Cursor != DefaultCursor && !abletomove)
                this.Cursor = DefaultCursor;
        }

        BigSchemeContainer BigDataSave()
        {
            return new BigSchemeContainer(schemesymbol, schemename, Inputs, Outputs, LogicGates, Wires, GV.SpecialNotOutput, UsedSchemes);
        }
        SchemeContainer DataSave()
        {
            return new SchemeContainer(schemesymbol, schemename, Scheme.ConvertToScheme(schemename, Inputs, Outputs, LogicGates, Wires), CircuitInput.GetStringArray(Inputs), CircuitOutput.GetStringArray(Outputs), UsedSchemes);
        }
        void BigDataOpen(BigSchemeContainer Data)
        {
            schemesymbol = Data.SchemeSymbol;
            schemename = Data.SchemeName;
            this.Text = schemename;
            Inputs = Data.Inputs;
            Outputs = Data.Outputs;
            LogicGates = Data.LogicGates;
            Wires = Data.Wires;
            GV.SpecialNotOutput = Data.SpecialNotOutput;
            UsedSchemes = Data.UsedSchemes;
            if (!GV.FreeDevelopment)
            {
                for (int i = 0; i < Inputs.Length; i++)
                    Inputs[i].Name = ActiveTask.Inputs[i];
                for (int i = 0; i < Outputs.Length; i++)
                    Outputs[i].Name = ActiveTask.Outputs[i];
            }
            Refresh();
        }

        void NewScheme()
        {
            schemesymbol = 's';
            schemename = "newscheme";
            SchemeNameForm SchemeName = new SchemeNameForm(schemesymbol, schemename);
            SchemeName.ShowDialog();
            schemesymbol = SchemeName.formschemesymbol;
            schemename = SchemeName.formschemename;
            this.Text = schemename;

            FirstPoint = new Point();

            Inputs = null;
            Outputs = null;
            newInputs(2);
            newOutputs(1);

            LogicGates = new LinkedList<LogicGate>();
            Wires = new LinkedList<Wire>();

            ChosenGate = null;
            ChosenWire = null;

            OperatorsMovement = false;
            WiringFromBeginning = false;
            WiringFromEnd = false;
            TryingToCut = false;

            richTextBox1.Text = "";
            GV.ShowingResults = false;
            StartOrStopShowingResults();

            UsedSchemes = new LinkedList<SchemeContainer>();

            Refresh();
        }
        void OpenScheme()
        {
            openSchemeDialog.ShowDialog();
        }
        private void openSchemeDialog_FileOk(object sender, CancelEventArgs e)
        {
            OpenSchemeOk();
        }
        void OpenSchemeOk()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(openSchemeDialog.FileName, FileMode.Open, FileAccess.Read);
            object ob = bf.Deserialize(fs);
            if (ob is BigSchemeContainer)
            {
                BigSchemeContainer Data = (BigSchemeContainer)ob;
                fs.Close();
                BigDataOpen(Data);
            }
            else
                MessageBox.Show("Выбран файл неправильного типа!");
        }
        void AddScheme()
        {
            addSchemeDialog.ShowDialog();
        }
        private void addSchemeDialog_FileOk(object sender, CancelEventArgs e)
        {
            AddSchemeOk();
        }
        void AddSchemeOk()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(addSchemeDialog.FileName, FileMode.Open, FileAccess.Read);
            object ob = bf.Deserialize(fs);
            if (ob is BigSchemeContainer)
            {
                BigSchemeContainer Data = (BigSchemeContainer)ob;
                fs.Close();
                UsedSchemes.AddLast(new SchemeContainer(Data.SchemeSymbol, Data.SchemeName, Scheme.ConvertToScheme(Data.SchemeName, Data.Inputs, Data.Outputs, Data.LogicGates, Data.Wires), CircuitInput.GetStringArray(Data.Inputs), CircuitOutput.GetStringArray(Data.Outputs), Data.UsedSchemes));
                LogicGates.AddFirst(new LogicGate(PanelX + 2 * GV.radius, PanelY + GV.radius, CircuitInput.GetStringArray(Data.Inputs), CircuitOutput.GetStringArray(Data.Outputs), Data.SchemeSymbol, Data.SchemeName));
            }
            else if (ob is SchemeContainer)
            {

                SchemeContainer Data = (SchemeContainer)ob;
                fs.Close();
                UsedSchemes.AddLast(Data);
                LogicGates.AddFirst(new LogicGate(PanelX + 2 * GV.radius, PanelY + GV.radius, Data.Inputs, Data.Outputs, Data.SchemeSymbol, Data.SchemeName));
            }
            else
            {
                MessageBox.Show("Выбран файл неправильного типа!");
            }
        }
        void OpenTask()
        {
            openTaskDialog.ShowDialog();
        }
        private void openTaskDialog_FileOk(object sender, CancelEventArgs e)
        {
            OpenTaskOk();
        }
        void OpenTaskOk()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(openTaskDialog.FileName, FileMode.Open, FileAccess.Read);
            object ob = bf.Deserialize(fs);
            if (ob is Task)
            {
                Task Data = (Task)ob;
                fs.Close();
                ActiveTask = Data;
            }
            else
                MessageBox.Show("Выбран файл неправильного типа!");
        }

        void SaveScheme()
        {
            if (lastsavewasfull && saveAllFileDialog.FileName != "")
                SaveAllOk();
            else if (!lastsavewasfull && saveLogicFileDialog.FileName != "")
                SaveLogicOk();
            else
                SaveAll();
        }
        void SaveAll()
        {
            saveAllFileDialog.FileName = schemename;
            saveAllFileDialog.ShowDialog();
        }
        private void saveAllFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            SaveAllOk();
        }
        void SaveAllOk()
        {
            BigSchemeContainer Data = new BigSchemeContainer(BigDataSave());
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(saveAllFileDialog.FileName, FileMode.Create, FileAccess.Write);
            bf.Serialize(fs, Data);
            fs.Close();
            lastsavewasfull = true;
        }
        void SaveLogic()
        {
            saveLogicFileDialog.FileName = schemename;
            saveLogicFileDialog.ShowDialog();
        }
        private void saveLogicFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            SaveLogicOk();
        }
        void SaveLogicOk()
        {
            SchemeContainer Data = new SchemeContainer(DataSave());
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(saveLogicFileDialog.FileName, FileMode.Create, FileAccess.Write);
            bf.Serialize(fs, Data);
            fs.Close();
            lastsavewasfull = false;
        }

        void SaveTask()
        {
            string scheme = Scheme.ConvertToScheme(schemename, Inputs, Outputs, LogicGates, Wires);
            foreach (LogicGate i in LogicGates)
            {
                i.Valueisknown = false;
            }
            if (scheme == null)
                MessageBox.Show("Ошибка при обработке схемы!");
            else
            {
                bool everythingisawesome = true;
                bool[] a = new bool[Inputs.Length];
                int numberofvariants = (int)Math.Pow(2, Inputs.Length);
                bool[,] b = new bool[numberofvariants, Outputs.Length];
                bool[] storage;
                int i, i2, j;
                for (i = 0; i < numberofvariants; i++)
                {
                    i2 = i;
                    for (j = 0; j < a.Length; j++)
                    {
                        a[j] = i2 % 2 == 1;
                        i2 >>= 1;
                    }
                    storage = Scheme.RunScheme(scheme, UsedSchemes, a);
                    if (storage == null || storage.Length != Outputs.Length)
                    {
                        everythingisawesome = false;
                        MessageBox.Show("Ошибка при попытке запустить схему!");
                        break;
                    }
                    for (j = 0; j < storage.Length; j++)
                        b[i, j] = storage[j];
                }
                if (everythingisawesome)
                {
                    TaskForm taskform = new TaskForm();
                    taskform.ShowDialog();

                    ActiveTask = new Task(taskform.stask, CircuitInput.GetStringArray(Inputs), CircuitOutput.GetStringArray(Outputs), b, taskform.andisallowed, taskform.orisallowed, taskform.notisallowed, taskform.zeroisallowed, taskform.otherschemesareallowed);

                    saveTaskFileDialog.FileName = schemename;
                    saveTaskFileDialog.ShowDialog();
                }
            }

        }
        private void saveTaskFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            SaveTaskOk();
        }
        void SaveTaskOk()
        {
            Task Data = ActiveTask;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(saveTaskFileDialog.FileName, FileMode.Create, FileAccess.Write);
            bf.Serialize(fs, Data);
            fs.Close();

        }

        void RenameScheme()
        {
            SchemeNameForm SchemeName = new SchemeNameForm(schemesymbol, schemename);
            SchemeName.ShowDialog();
            schemesymbol = SchemeName.formschemesymbol;
            schemename = SchemeName.formschemename;
            this.Text = schemename;
        }

        void StartOrStopShowingResults()
        {
            addinputbutton.Enabled = !GV.ShowingResults && GV.FreeDevelopment;
            addoutputbutton.Enabled = !GV.ShowingResults && GV.FreeDevelopment;
            addandbutton.Enabled = !GV.ShowingResults && (GV.FreeDevelopment || ActiveTask.AndIsAllowed);
            addorbutton.Enabled = !GV.ShowingResults && (GV.FreeDevelopment || ActiveTask.OrIsAllowed);
            addnotbutton.Enabled = !GV.ShowingResults && (GV.FreeDevelopment || ActiveTask.NotIsAllowed);
            addzerobutton.Enabled = !GV.ShowingResults && (GV.FreeDevelopment || ActiveTask.ZeroIsAllowed);
            runschemebutton.Text = (GV.ShowingResults) ? "Изменить схему" : "Запустить схему";
        }
        void StartOrStopFreeDevelopment()
        {
            показатьToolStripMenuItem.Enabled = !GV.FreeDevelopment;
            instructionStripButton.Enabled = !GV.FreeDevelopment;
            showresultStripButton.Enabled = !GV.FreeDevelopment;

            добавитьToolStripMenuItem.Enabled = GV.FreeDevelopment || ActiveTask.OtherSchemesAreAllowed;
            addschemeStripButton.Enabled = GV.FreeDevelopment || ActiveTask.OtherSchemesAreAllowed;
            открытьToolStripMenuItem.Enabled = GV.FreeDevelopment;
            openschemeStripButton.Enabled = GV.FreeDevelopment;

            addinputbutton.Enabled = GV.FreeDevelopment;
            addoutputbutton.Enabled = GV.FreeDevelopment;
            addandbutton.Enabled = GV.FreeDevelopment || ActiveTask.AndIsAllowed;
            addorbutton.Enabled = GV.FreeDevelopment || ActiveTask.OrIsAllowed;
            addnotbutton.Enabled = GV.FreeDevelopment || ActiveTask.NotIsAllowed;
            addzerobutton.Enabled = GV.FreeDevelopment || ActiveTask.ZeroIsAllowed;
        }

        void Basictasks()
        {
            BasicTasks = new Task[13];
            bool[,] truthtable;

            truthtable = new bool[4, 1];
            truthtable[0, 0] = false;
            truthtable[1, 0] = true;
            truthtable[2, 0] = true;
            truthtable[3, 0] = true;
            BasicTasks[0] = new Task(@"
Спроектировать схему ИЛИ, пользуясь только схемами И и НЕ.
            ", new string[2] { "a0", "a1" }, new string[1] { "b0" }, truthtable, true, false, true, false, false);

            truthtable = new bool[8, 1];
            truthtable[0, 0] = false;
            for (int i = 1; i < 8; i++)
                truthtable[i, 0] = true;
            BasicTasks[1] = new Task(@"
Составить схему, вычисляющую ИЛИ от трех входов (на выходе 1, если хотя бы на одном из входов 1).    
            ", new string[3] { "a0", "a1", "a2" }, new string[1] { "b0" }, truthtable, true, true, true, true, false);

            truthtable = new bool[16, 1];
            truthtable[0, 0] = false;
            for (int i = 1; i < 16; i++)
                truthtable[i, 0] = true;
            BasicTasks[2] = new Task(@"
Составить схему, вычисляющую ИЛИ от четырех входов (на выходе 1, если хотя бы на одном из входов 1).
            ", new string[4] { "a0", "a1", "a2", "a3" }, new string[1] { "b0" }, truthtable, true, true, true, true, false);

            truthtable = new bool[4, 1];
            truthtable[0, 0] = true;
            truthtable[1, 0] = false;
            truthtable[2, 0] = false;
            truthtable[3, 0] = true;
            BasicTasks[3] = new Task(@"
Составить схему РАВЕНСТВО с двумя входами и одним выходом. На выходе 1, если входы равны, и 0, если входы разные.
            ", new string[2] { "a0", "a1" }, new string[1] { "b0" }, truthtable, true, true, true, true, false);

            truthtable = new bool[4, 1];
            truthtable[0, 0] = false;
            truthtable[1, 0] = true;
            truthtable[2, 0] = true;
            truthtable[3, 0] = false;
            BasicTasks[4] = new Task(@"
Составить схему ИСКЛЮЧАЮЩЕЕ ИЛИ с двумя входами и одним выходом. На выходе 0, если входы равны, и 1, если входы разные.
            ", new string[2] { "a0", "a1" }, new string[1] { "b0" }, truthtable, true, true, true, true, false);

            truthtable = new bool[8, 1];
            truthtable[0, 0] = false;
            truthtable[1, 0] = false;
            truthtable[2, 0] = false;
            truthtable[3, 0] = true;
            truthtable[4, 0] = true;
            truthtable[5, 0] = false;
            truthtable[6, 0] = true;
            truthtable[7, 0] = true;
            BasicTasks[5] = new Task(@"
Составить схему с тремя входами (a0, a1, select), на выходе которой a0 если select=0 и a1 если select=1.
            ", new string[3] { "a0", "a1", "select" }, new string[1] { "b0" }, truthtable, true, true, true, true, false);

            truthtable = new bool[8, 1];
            truthtable[0, 0] = false;
            truthtable[1, 0] = false;
            truthtable[2, 0] = false;
            truthtable[3, 0] = true;
            truthtable[4, 0] = false;
            truthtable[5, 0] = true;
            truthtable[6, 0] = true;
            truthtable[7, 0] = true;
            BasicTasks[6] = new Task(@"
Составить схему с тремя входами, у которой выход равен 1 или 0 в зависимости от того, каких входов больше (то есть на выходе 1, если на входах хотя бы две 1, и 0, если на входах хотя бы два 0).
            ", new string[3] { "a0", "a1", "a2" }, new string[1] { "b0" }, truthtable, true, true, true, true, false);

            truthtable = new bool[4, 1];
            truthtable[0, 0] = false;
            truthtable[1, 0] = true;
            truthtable[2, 0] = true;
            truthtable[3, 0] = false;
            BasicTasks[7] = new Task(@"
Составить схему, на выходе которой сумма двух двоичных разрядов (перенос в старший разряд отбрасывается; то есть, на выходе 1, если ровно на одном входе 1).
            ", new string[2] { "a0", "a1" }, new string[1] { "b0" }, truthtable, true, true, true, true, false);

            truthtable = new bool[4, 2];
            truthtable[0, 0] = false;
            truthtable[1, 0] = true;
            truthtable[2, 0] = true;
            truthtable[3, 0] = false;

            truthtable[0, 1] = false;
            truthtable[1, 1] = false;
            truthtable[2, 1] = false;
            truthtable[3, 1] = true;
            BasicTasks[8] = new Task(@"
Составить схему с двумя входами и двумя выходами. На выходах схемы - сумма двух входов (оба разряда).
            ", new string[2] { "a0", "b0" }, new string[2] { "s0", "p1" }, truthtable, true, true, true, true, false);

            truthtable = new bool[8, 2];
            truthtable[0, 0] = false;
            truthtable[1, 0] = true;
            truthtable[2, 0] = true;
            truthtable[3, 0] = false;
            truthtable[4, 0] = true;
            truthtable[5, 0] = false;
            truthtable[6, 0] = false;
            truthtable[7, 0] = true;

            truthtable[0, 1] = false;
            truthtable[1, 1] = false;
            truthtable[2, 1] = false;
            truthtable[3, 1] = true;
            truthtable[4, 1] = false;
            truthtable[5, 1] = true;
            truthtable[6, 1] = true;
            truthtable[7, 1] = true;
            BasicTasks[9] = new Task(@"
Составить схему с тремя входами и двумя выходами. На выходах схемы - сумма трех входов (оба разряда).
            ", new string[3] { "a0", "b0", "p0" }, new string[2] { "s0", "p1" }, truthtable, true, true, true, true, true);

            truthtable = new bool[32, 3];
            int j = 0;
            int storage = 0;
            for (byte p0 = 0; p0 < 2; p0++)
                for (byte b1 = 0; b1 < 2; b1++)
                    for (byte b0 = 0; b0 < 2; b0++)
                        for (byte a1 = 0; a1 < 2; a1++)
                            for (byte a0 = 0; a0 < 2; a0++)
                            {
                                storage = a0 + b0 + p0;
                                truthtable[j, 0] = storage % 2 == 1;
                                storage = a1 + b1 + ((storage > 1) ? 1 : 0);
                                truthtable[j, 1] = storage % 2 == 1;
                                truthtable[j, 2] = storage > 1;
                                j++;
                            }
            BasicTasks[10] = new Task(@"
Составить схему, на входах которой два 2-битовых числа и еще один бит переноса, а на выходах - сумма этих трех слагаемых (3 бита).
            ", new string[5] { "a0", "a1", "b0", "b1", "p0" }, new string[3] { "s0", "s1", "p2" }, truthtable, true, true, true, true, true);

            truthtable = new bool[512, 5];
            j = 0;
            storage = 0;
            for (byte p0 = 0; p0 < 2; p0++)
                for (byte b3 = 0; b3 < 2; b3++)
                    for (byte b2 = 0; b2 < 2; b2++)
                        for (byte b1 = 0; b1 < 2; b1++)
                            for (byte b0 = 0; b0 < 2; b0++)
                                for (byte a3 = 0; a3 < 2; a3++)
                                    for (byte a2 = 0; a2 < 2; a2++)
                                        for (byte a1 = 0; a1 < 2; a1++)
                                            for (byte a0 = 0; a0 < 2; a0++)
                                            {
                                                storage = a0 + b0 + p0;
                                                truthtable[j, 0] = storage % 2 == 1;
                                                storage = a1 + b1 + ((storage > 1) ? 1 : 0);
                                                truthtable[j, 1] = storage % 2 == 1;
                                                storage = a2 + b2 + ((storage > 1) ? 1 : 0);
                                                truthtable[j, 2] = storage % 2 == 1;
                                                storage = a3 + b3 + ((storage > 1) ? 1 : 0);
                                                truthtable[j, 3] = storage % 2 == 1;
                                                truthtable[j, 4] = storage > 1;
                                                j++;
                                            }
            BasicTasks[11] = new Task(@"
Составить схему, на входах которой два 4-битовых числа и еще один бит переноса, а на выходах - сумма этих трех слагаемых (5 битов).
            ", new string[9] { "a0", "a1", "a2", "a3", "b0", "b1", "b2", "b3", "p0" }, new string[5] { "s0", "s1", "s2", "s3", "p4" }, truthtable, true, true, true, true, true);

            truthtable = new bool[256, 8];
            j = 0;
            storage = 0;
            for (byte b3 = 0; b3 < 2; b3++)
                for (byte b2 = 0; b2 < 2; b2++)
                    for (byte b1 = 0; b1 < 2; b1++)
                        for (byte b0 = 0; b0 < 2; b0++)
                            for (byte a3 = 0; a3 < 2; a3++)
                                for (byte a2 = 0; a2 < 2; a2++)
                                    for (byte a1 = 0; a1 < 2; a1++)
                                        for (byte a0 = 0; a0 < 2; a0++)
                                        {
                                            storage = a0 * b0;
                                            truthtable[j, 0] = storage % 2 == 1;
                                            storage = a1 * b0 + a0 * b1 + ((storage > 1) ? 1 : 0);
                                            truthtable[j, 1] = storage % 2 == 1;
                                            storage = a2 * b0 + a1 * b1 + a0 * b2 + ((storage > 1) ? 1 : 0);
                                            truthtable[j, 2] = storage % 2 == 1;
                                            storage = a3 * b0 + a2 * b1 + a1 * b2 + a0 * b3 + ((storage > 1) ? 1 : 0);
                                            truthtable[j, 3] = storage % 2 == 1;
                                            storage = a3 * b1 + a2 * b2 + a1 * b3 + ((storage > 1) ? 1 : 0);
                                            truthtable[j, 4] = storage % 2 == 1;
                                            storage = a3 * b2 + a2 * b3 + ((storage > 1) ? 1 : 0);
                                            truthtable[j, 5] = storage % 2 == 1;
                                            storage = a3 * b3 + ((storage > 1) ? 1 : 0);
                                            truthtable[j, 6] = storage % 2 == 1;
                                            truthtable[j, 7] = storage > 1;
                                            j++;
                                        }
            BasicTasks[12] = new Task(@"
Построить схему, умножающую два 4-разрядных числа.
            ", new string[8] { "a0", "a1", "a2", "a3", "b0", "b1", "b2", "b3" }, new string[8] { "m0", "m1", "m2", "m3", "m4", "m5", "m6", "m7" }, truthtable, true, true, true, true, true);

        }
        void Starttask(Task a)
        {
            NewScheme();
            newInputs(a.Inputs);
            newOutputs(a.Outputs);
            ShowGoal();
        }
        void ShowGoal()
        {
            string goal = "Задача:\n";
            goal += ActiveTask.STask;
            goal += "\nИспользовать элемент И ";
            if (!ActiveTask.AndIsAllowed) goal += "не ";
            goal += "разрешено.";
            goal += "\nИспользовать элемент ИЛИ ";
            if (!ActiveTask.OrIsAllowed) goal += "не ";
            goal += "разрешено.";
            goal += "\nИспользовать элемент НЕ ";
            if (!ActiveTask.NotIsAllowed) goal += "не ";
            goal += "разрешено.";
            goal += "\nИспользовать сигнал \"0\" ";
            if (!ActiveTask.ZeroIsAllowed) goal += "не ";
            goal += "разрешено.";
            goal += "\nИспользовать другие схемы ";
            if (!ActiveTask.OtherSchemesAreAllowed) goal += "не ";
            goal += "разрешено.";
            MessageBox.Show(goal);
        }
        void ShowResult()
        {
            string scheme = Scheme.ConvertToScheme(schemename, Inputs, Outputs, LogicGates, Wires);
            foreach (LogicGate i in LogicGates)
            {
                i.Valueisknown = false;
            }
            if (scheme == null)
                MessageBox.Show("Ошибка при обработке схемы!");
            else
            {
                bool everythingisawesome = true;
                bool[] a = new bool[Inputs.Length];
                int numberofvariants = (int)Math.Pow(2, Inputs.Length);
                bool[][] b = new bool[numberofvariants][];
                int i, i2, j;
                for (i = 0; i < numberofvariants; i++)
                {
                    i2 = i;
                    for (j = 0; j < a.Length; j++)
                    {
                        a[j] = i2 % 2 == 1;
                        i2 >>= 1;
                    }
                    b[i] = Scheme.RunScheme(scheme, UsedSchemes, a);
                    if (b[i] == null || b[i].Length != Outputs.Length)
                    {
                        everythingisawesome = false;
                        MessageBox.Show("Ошибка при попытке запустить схему!");
                    }
                    else
                    {
                        for (j = 0; j < Outputs.Length; j++)
                        {
                            if (b[i][j] != ActiveTask.Truthtable[i, j])
                            {
                                everythingisawesome = false;
                                break;
                            }
                        }
                    }
                    if (!everythingisawesome)
                        break;
                }
                if (everythingisawesome)
                    MessageBox.Show("Задача решена верно!");
                else
                    MessageBox.Show("Задача не решена!");
            }
        }

        #endregion

        #region Справка

        void ShowHelp()
        {
            //Logic Circuit Help.hnd
            //Help.ShowHelp(this, @"./Resources/efts.chm");
            //Properties.Resources.Логические_схемы
            Help.ShowHelp(this, @"./Resources/LCHelp.chm");
        }

        void ShowInfo()
        {
            string message =
                "Данная программа - обучающая программа для создания логических схем с использованием графического редактора для персональных компьютеров и планшетов на базе операционных систем семейства Microsoft Windows.\n\n" +
                "Автор: Копылов Олег (2014)\n\n" +
                //"Application path: " + Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + ".exe") + Environment.NewLine +
                "Базовая директория приложения: " + Directory.GetCurrentDirectory();
            MessageBox.Show(message, "Версия программы v. " + version, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

    }
}
