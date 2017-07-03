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
    class SchemeContainer
    {
        char schemesymbol;
        string schemename;
        string schemetext;
        string[] inputs;
        string[] outputs;
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
        public string SchemeText
        {
            get { return schemetext; }
            set { schemetext = value; }
        }
        public string[] Inputs
        {
            get { return inputs; }
        }
        public string[] Outputs
        {
            get { return outputs; }
        }
        public LinkedList<SchemeContainer> UsedSchemes
        {
            get { return usedschemes; }
            set { usedschemes = value; }
        }

        public static SchemeContainer NecessaryNode(LinkedList<SchemeContainer> l, char c)
        {
            foreach (SchemeContainer i in l)
                if (i.schemesymbol == c)
                    return i;
            return null;
        }

        public SchemeContainer(char SchemeSymbol, string SchemeName, string SchemeText, string[] Inputs, string[] Outputs, LinkedList<SchemeContainer> UsedSchemes)
        {
            schemesymbol = SchemeSymbol;
            schemename = SchemeName;
            schemetext = SchemeText;
            inputs = Inputs;
            outputs = Outputs;
            usedschemes = UsedSchemes;
        }
        public SchemeContainer(SchemeContainer OldData)
        {
            schemesymbol = OldData.SchemeSymbol;
            schemename = OldData.SchemeName;
            schemetext = OldData.SchemeText;
            inputs = OldData.Inputs;
            outputs = OldData.Outputs;
            usedschemes = OldData.UsedSchemes;
        }
    }
}
