using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    class BooleanString
    {
        bool? bvalue;
        string svalue;

        public bool? BoolValue
        {
            get { return bvalue; }
            set { bvalue = value; }
        }
        public string StringValue
        {
            get { return svalue; }
            set { svalue = value; }
        }

        public static int? GetNumberInArray(BooleanString[] a, string b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].StringValue == b)
                    return i;
            }
            return null;
        }

        public BooleanString(string stringvalue)
        {
            svalue = stringvalue;
            bvalue = null;
        }
    }
}
