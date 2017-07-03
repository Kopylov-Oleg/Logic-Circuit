using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    class Scheme
    {
        public static string ConvertToScheme(string schemename, CircuitInput[] Inputs, CircuitOutput[] Outputs, LinkedList<LogicGate> LogicGates, LinkedList<Wire> Wires)
        {
            string scheme = "scheme";// "Максимальный размер объекта String в 2 ГБ памяти, или около 1 миллиард символов." - MSDN

            LogicGate[] logicgates = new LogicGate[LogicGates.Count];
            LogicGates.CopyTo(logicgates, 0);

            scheme += " (";
            for (int i = 0; i < Inputs.Length; i++)
                scheme += " " + Inputs[i].Name;
            scheme += " ) " + schemename + " (";
            for (int i = 0; i < Outputs.Length; i++)
                scheme += " " + Outputs[i].Name;
            scheme += " ) :" + Environment.NewLine;

            scheme += "local";
            for (int i = 0; i < logicgates.Length; i++)
                for (int j = 0; j < logicgates[i].Outputs.Length; j++)
                    if (logicgates[i].Outputs[j].Count != 0)
                    {
                        /*
                        int l = 0;
                        foreach (Wire k in logicgates[i].Outputs[j])
                            if (k.End is LogicGate)
                            {
                                k.Name = i.ToString() + "." + j.ToString() + "." + l.ToString();
                                scheme += " " + k.Name;
                                l++;
                            }
                         */
                        scheme += " " + i.ToString() + "." + j.ToString();
                    }
            scheme += Environment.NewLine;

            bool isdone = false;
            bool smthisdone = false;
            while (!isdone)
            {
                isdone = true;
                for (int i = 0; i < logicgates.Length; i++)
                {
                    if (!logicgates[i].Valueisknown)
                    {
                        isdone = false;
                        bool allinputsareknown = true;
                        for (int j = 0; j < logicgates[i].Inputs.Length; j++)
                        {
                            if (logicgates[i].Inputs[j] == null)
                                return null;
                            if (logicgates[i].Inputs[j].Beginning is LogicGate && !(logicgates[i].Inputs[j].Beginning as LogicGate).Valueisknown)
                            {
                                allinputsareknown = false;
                                break;
                            }
                        }
                        if (allinputsareknown)
                        {
                            smthisdone = true;
                            logicgates[i].Valueisknown = true;
                            if (logicgates[i].Inputs != null)
                            {
                                scheme += "\t(";
                                for (int j = 0; j < logicgates[i].Inputs.Length; j++)
                                {
                                    scheme += " ";
                                    if (logicgates[i].Inputs[j].Beginning is CircuitInput)
                                        scheme += (logicgates[i].Inputs[j].Beginning as CircuitInput).Name;
                                    else
                                        //scheme += logicgates[i].Inputs[j].Name;
                                        scheme += Tools.GetNumberInArray(logicgates, logicgates[i].Inputs[j].Beginning).ToString() + "." + Tools.GetNumberInArray((logicgates[i].Inputs[j].Beginning as LogicGate).Outputs, logicgates[i].Inputs[j]);
                                    /*if (logicgates[(int)Tools.GetNumberInArray(logicgates, logicgates[i].Inputs[j].Beginning)].Outputs[(int)Tools.GetNumberInArray((logicgates[i].Inputs[j].Beginning as LogicGate).Outputs, logicgates[i].Inputs[j])].Count != 0)
                                    {
                                        int l = 0;
                                        foreach (Wire k in logicgates[(int)Tools.GetNumberInArray(logicgates, logicgates[i].Inputs[j].Beginning)].Outputs[(int)Tools.GetNumberInArray((logicgates[i].Inputs[j].Beginning as LogicGate).Outputs, logicgates[i].Inputs[j])])
                                            if (k.End is CircuitOutput)
                                                scheme += " " + (k.End as CircuitOutput).Name;
                                            else
                                            {
                                                scheme += " " + Tools.GetNumberInArray(logicgates, logicgates[i].Inputs[j].Beginning).ToString() + "." + Tools.GetNumberInArray((logicgates[i].Inputs[j].Beginning as LogicGate).Outputs, logicgates[i].Inputs[j]).ToString() + "." + l.ToString();
                                                l++;
                                            }
                                    }  */
                                }
                                scheme += " ) " + logicgates[i].SchemeSymbol.ToString() + " (";
                                for (int j = 0; j < logicgates[i].Outputs.Length; j++)
                                    if (logicgates[i].Outputs[j].Count != 0)
                                    {
                                        /*
                                        foreach (Wire k in logicgates[i].Outputs[j])
                                        {
                                            scheme += " ";
                                            if (k.End is CircuitOutput)
                                                scheme += (k.End as CircuitOutput).Name;
                                            else
                                                scheme += k.Name;
                                        }
                                        */
                                        scheme += " " + i.ToString() + "." + j.ToString();
                                    }
                                scheme += " )" + Environment.NewLine;
                            }
                        }
                    }
                }
                if (!smthisdone)
                    return null;
            }
            for (int i = 0; i < Outputs.Length; i++)
            {
                if (Outputs[i].Output == null)
                    return null;
                scheme += "\t( ";
                if (Outputs[i].Output.Beginning is CircuitInput)
                    scheme += (Outputs[i].Output.Beginning as CircuitInput).Name;
                else
                    scheme += Tools.GetNumberInArray(logicgates, Outputs[i].Output.Beginning).ToString() + "." + Tools.GetNumberInArray((Outputs[i].Output.Beginning as LogicGate).Outputs, Outputs[i].Output);
                scheme += " ) = ( ";
                scheme += Outputs[i].Name;
                scheme += " )" + Environment.NewLine;
            }

            /* for (int i = 0; i < logicgates.Length; i++)
                 logicgates[i].Valueisknown = false;*/
            scheme += "end";

            return scheme;
        }

        public static bool[] RunScheme(string scheme, LinkedList<SchemeContainer> usedschemes, bool[] a)
        {
            char[] array = new char[scheme.Length];
            string schemecopy = "";
            for (int i = 0; i < scheme.Length; i++)
                schemecopy += scheme[scheme.Length - 1 - i];
            //shemecopy = array.ToString();

            if (schemecopy[schemecopy.Length - 1] == 's') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'c') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'h') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'e') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'm') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'e') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == '(') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;

            LinkedList<string> InputsNamesList = new LinkedList<string>();
            while (schemecopy[schemecopy.Length - 1] != ')')
            {
                InputsNamesList.AddLast("");
                while (schemecopy[schemecopy.Length - 1] != ' ')
                {
                    InputsNamesList.Last.Value += schemecopy[schemecopy.Length - 1];
                    schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                }
                schemecopy = schemecopy.Remove(schemecopy.Length - 1);
            }
            schemecopy = schemecopy.Remove(schemecopy.Length - 1);
            if (InputsNamesList.Count == 0 || InputsNamesList.Count != a.Length)
                return null;
            string[] InputsNamesArray = new string[InputsNamesList.Count];
            InputsNamesList.CopyTo(InputsNamesArray, 0);

            if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            while (schemecopy[schemecopy.Length - 1] != ' ')
                schemecopy = schemecopy.Remove(schemecopy.Length - 1);
            schemecopy = schemecopy.Remove(schemecopy.Length - 1);

            if (schemecopy[schemecopy.Length - 1] == '(') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            LinkedList<string> OutputsNamesList = new LinkedList<string>();
            while (schemecopy[schemecopy.Length - 1] != ')')
            {
                OutputsNamesList.AddLast("");
                while (schemecopy[schemecopy.Length - 1] != ' ')
                {
                    OutputsNamesList.Last.Value += schemecopy[schemecopy.Length - 1];
                    schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                }
                schemecopy = schemecopy.Remove(schemecopy.Length - 1);
            }
            schemecopy = schemecopy.Remove(schemecopy.Length - 1);
            if (OutputsNamesList.Count == 0)
                return null;
            string[] OutputsNamesArray = new string[OutputsNamesList.Count];
            OutputsNamesList.CopyTo(OutputsNamesArray, 0);
            bool[] b = new bool[OutputsNamesList.Count];

            if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == ':') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == '\r') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == '\n') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;

            if (schemecopy[schemecopy.Length - 1] == 'l') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'o') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'c') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'a') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'l') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            //if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;

            LinkedList<BooleanString> WiresList = new LinkedList<BooleanString>();
            while (schemecopy[schemecopy.Length - 1] != '\r')
            {
                if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
                WiresList.AddLast(new BooleanString(""));
                while (schemecopy[schemecopy.Length - 1] != '\r' && schemecopy[schemecopy.Length - 1] != ' ')
                {
                    WiresList.Last.Value.StringValue += schemecopy[schemecopy.Length - 1];
                    schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                }
                if (WiresList.Last.Value.StringValue == "")
                    return null;
            }
            schemecopy = schemecopy.Remove(schemecopy.Length - 1);
            BooleanString[] WiresArray = new BooleanString[WiresList.Count];
            WiresList.CopyTo(WiresArray, 0);
            if (schemecopy[schemecopy.Length - 1] == '\n') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;

            string circuitcomponentname;
            LinkedList<bool> inputs;// всё false?
            bool[] outputs = new bool[1];
            int outputnumber;
            while (schemecopy[schemecopy.Length - 1] == '\t')
            {
                inputs = new LinkedList<bool>();
                schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                if (schemecopy[schemecopy.Length - 1] == '(') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
                if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
                while (schemecopy[schemecopy.Length - 1] != ')')
                {
                    circuitcomponentname = "";
                    while (schemecopy[schemecopy.Length - 1] != ' ')
                    {
                        circuitcomponentname += schemecopy[schemecopy.Length - 1];
                        schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                    }
                    if (InputsNamesArray.Contains(circuitcomponentname))
                        inputs.AddLast(a[(int)Tools.GetNumberInArray(InputsNamesArray, circuitcomponentname)]);
                    else if (BooleanString.GetNumberInArray(WiresArray, circuitcomponentname) != null)
                        //if (WiresArray[(int)BooleanString.GetNumberInArray(WiresArray, circuitcomponentname)].StringValue == "0")
                        //inputs.AddLast(false);
                        //else
                        inputs.AddLast((bool)WiresArray[(int)BooleanString.GetNumberInArray(WiresArray, circuitcomponentname)].BoolValue);
                    else return null;
                    schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                }

                schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;

                switch (schemecopy[schemecopy.Length - 1])
                {
                    case '&':
                        if (inputs.Count != 2)
                            return null;
                        outputs = new bool[1];
                        outputs[0] = inputs.First.Value & inputs.First.Next.Value;
                        break;
                    case '1':
                        if (inputs.Count != 2)
                            return null;
                        outputs = new bool[1];
                        outputs[0] = inputs.First.Value | inputs.First.Next.Value;
                        break;
                    case '!':
                        if (inputs.Count != 1)
                            return null;
                        outputs = new bool[1];
                        outputs[0] = !inputs.First.Value;
                        break;
                    /*case '0':
                        if (inputs.Count != 0)
                            return null;
                        outputs = new bool[1];
                        outputs[0] = false;
                        break;*/
                    case '=':
                        if (inputs.Count != 1)
                            return null;
                        outputs = new bool[1];
                        outputs[0] = inputs.First.Value;
                        break;
                    default:
                        SchemeContainer i = SchemeContainer.NecessaryNode(usedschemes, schemecopy[schemecopy.Length - 1]);
                        if (i != null)
                        {
                            bool[] inputsarray = new bool[inputs.Count];
                            inputs.CopyTo(inputsarray, 0);
                            outputs = RunScheme(i.SchemeText, i.UsedSchemes, inputsarray);
                        }
                        break;
                }

                schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;

                if (schemecopy[schemecopy.Length - 1] == '(') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
                if (schemecopy[schemecopy.Length - 1] == ' ') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
                outputnumber = -1;
                while (schemecopy[schemecopy.Length - 1] != ')')
                {
                    outputnumber++;
                    if (outputs.Length == outputnumber)
                        return null;
                    circuitcomponentname = "";
                    while (schemecopy[schemecopy.Length - 1] != ' ')
                    {
                        circuitcomponentname += schemecopy[schemecopy.Length - 1];
                        schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                    }
                    if (OutputsNamesArray.Contains(circuitcomponentname))
                        b[(int)Tools.GetNumberInArray(OutputsNamesArray, circuitcomponentname)] = outputs[outputnumber];
                    else if (BooleanString.GetNumberInArray(WiresArray, circuitcomponentname) != null)
                        WiresArray[(int)BooleanString.GetNumberInArray(WiresArray, circuitcomponentname)].BoolValue = outputs[outputnumber];
                    else return null;
                    schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                    //outputnumber++;
                }
                schemecopy = schemecopy.Remove(schemecopy.Length - 1);
                if (schemecopy[schemecopy.Length - 1] == '\r') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
                if (schemecopy[schemecopy.Length - 1] == '\n') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            }
            if (schemecopy[schemecopy.Length - 1] == 'e') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'n') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;
            if (schemecopy[schemecopy.Length - 1] == 'd') schemecopy = schemecopy.Remove(schemecopy.Length - 1); else return null;

            return b;
        }
    }
}
