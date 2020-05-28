using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaBoy.Hardware
{
    public abstract  class MemoryBankController
    {
        public abstract bool IsAddressWritable(int address);
        public abstract bool IsAddressReadable(int address);
        public abstract bool AccessesROM(int address);
        public abstract bool HandleWrite(int address, byte value);
        public abstract int TranslateAddress(int address);
    }
}
