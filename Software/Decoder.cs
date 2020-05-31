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
            else if ((code & 0b11000111) == 0b00000110) return new LDI(parent, code);
            else if ((code & 0b11100111) == 0b00000010) return new LDA(parent, code);
            else if ((code & 0b11101111) == 0b11101010) return new LDAI(parent, code);
            else if ((code & 0b11101101) == 0b11100000) return new LDH(parent, code);
            throw new NotImplementedException();
        }
    }
}