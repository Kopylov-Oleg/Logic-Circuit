using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    class Tools
    {
        public static int? GetNumberInArray(CircuitComponent[] a, CircuitComponent b)
        {
            for (int i = 0; i < a.Length; i++)
                if (a[i] == b)
                    return i;
            return null;
        }

        public static int? GetNumberInArray(LinkedList<Wire>[] a, Wire b)
        {
            for (int i = 0; i < a.Length; i++)
                if (a[i].Contains(b))
                    return i;
            return null;
        }
        public static int? GetNumberInArray(string[] a, string b)
        {
            for (int i = 0; i < a.Length; i++)
                if (a[i].Contains(b))
                    return i;
            return null;
        }

        /*public static int? GetNumberInArray(LinkedList<Wire>[] a, Wire b)
        {
            for (int i = 0; i < a.Length; i++)
                if (a[i].Contains(b))
                    return i;
            return null;
        }*/
    }
}
