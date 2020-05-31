using ChromaBoy.Hardware;
using ChromaBoy.Software.Opcodes;
using System;

namespace ChromaBoy.Software
{
    class Decoder
    {
        public static Opcode DecodeOpcode(Gameboy parent, byte code)
        {
            if ((code & 0b11000000) == 0b01000000) return new LD(parent, code);
            throw new NotImplementedException();
        }
    }
}