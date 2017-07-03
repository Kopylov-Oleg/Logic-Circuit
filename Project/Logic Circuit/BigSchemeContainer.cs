using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.IO;

namespace Logic_Circuit
{
    [Serializable]
    class BigSchemeContainer
    {
        char schemesymbol;
        string schemename;
        CircuitInput[] inputs;
        CircuitOutput[] outputs;
        LinkedList<LogicGate> logicgates;
        LinkedList<Wire> wires;
        bool specialnotoutput;
        LinkedList<SchemeContainer> usedschemes;

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
        public CircuitInput[] Inputs
        {
            get { return inputs; }
        }
        public CircuitOutput[] Outputs
        {
            get { return outputs; }
        }
        public LinkedList<LogicGate> LogicGates
        {
            get { return logicgates; }
        }
        public LinkedList<Wire> Wires
        {
            get { return wires; }
        }
        public bool SpecialNotOutput
        {
            get { return specialnotoutput; }
        }
        public LinkedList<SchemeContainer> UsedSchemes
        {
            get { return usedschemes; }
        }

        public BigSchemeContainer(char SchemeSymbol, string SchemeName, CircuitInput[] Inputs, CircuitOutput[] Outputs, LinkedList<LogicGate> LogicGates, LinkedList<Wire> Wires, bool SpecialNotOutput, LinkedList<SchemeContainer> UsedSchemes)
        {
            schemesymbol = SchemeSymbol;
            schemename = SchemeName;
            inputs = Inputs;
            outputs = Outputs;
            logicgates = LogicGates;
            wires = Wires;
            specialnotoutput = SpecialNotOutput;
            usedschemes = UsedSchemes;
        }
        public BigSchemeContainer(BigSchemeContainer OldData)
        {
            schemesymbol = OldData.SchemeSymbol;
            schemename = OldData.SchemeName;
            inputs = OldData.Inputs;
            outputs = OldData.Outputs;
            logicgates = OldData.LogicGates;
            wires = OldData.Wires;
            specialnotoutput = OldData.SpecialNotOutput;
            usedschemes = OldData.UsedSchemes;
        }
    }
}
