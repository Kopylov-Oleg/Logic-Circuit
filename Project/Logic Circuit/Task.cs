using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic_Circuit
{
    [Serializable]
    class Task
    {
        string stask;
        string[] inputs;
        string[] outputs;
        bool[,] truthtable;
        /*
         * 1 вход:
         * 0ой вариант - 0
         * 1ый вариант - 1
         * 2 входа:
         * 0ой вариант - 00
         * 1ый вариант - 01
         * 2ой вариант - 10
         * 3ий вариант - 11
         * 1 вход:
         * 0ой вариант - 000
         * 1ый вариант - 001
         * 2ой вариант - 010
         * 3ий вариант - 011
         * 4ый вариант - 100
         * 5ый вариант - 101
         * 6ой вариант - 110
         * 7ой вариант - 111
         * и т.д.
        */
        bool andisallowed;
        bool orisallowed;
        bool notisallowed;
        bool zeroisallowed;
        bool otherschemesareallowed;

        public string STask
        {
            get { return stask; }
        }
        public string[] Inputs
        {
            get { return inputs; }
        }
        public string[] Outputs
        {
            get { return outputs; }
        }
        public bool[,] Truthtable
        {
            get { return truthtable; }
        }
        public bool AndIsAllowed
        {
            get { return andisallowed; }
        }
        public bool OrIsAllowed
        {
            get { return orisallowed; }
        }
        public bool NotIsAllowed
        {
            get { return notisallowed; }
        }
        public bool ZeroIsAllowed
        {
            get { return zeroisallowed; }
        }
        public bool OtherSchemesAreAllowed
        {
            get { return otherschemesareallowed; }
        }

        public Task(string stask, string[] inputs, string[] outputs, bool[,] truthtable, bool andisallowed, bool orisallowed, bool notisallowed, bool zeroisallowed, bool otherschemesareallowed)
        {
            this.inputs = inputs;
            this.outputs = outputs;
            this.truthtable = truthtable;
            this.stask = stask;
            this.andisallowed = andisallowed;
            this.orisallowed = orisallowed;
            this.notisallowed = notisallowed;
            this.zeroisallowed = zeroisallowed;
            this.otherschemesareallowed = otherschemesareallowed;
        }
    }
}
