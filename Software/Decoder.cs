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
            else if ((code & 0b11100111) == 0b00100010) return new LDAHL(parent, code);
            else if ((code & 0b11001111) == 0b00000001) return new PUSH(parent, code);
            else if ((code & 0b11111111) == 0b00001000) return new LDMSP(parent);
            else if ((code & 0b11111111) == 0b11111001) return new LDSPHL(parent);
            else if ((code & 0b11001111) == 0b11000101) return new PUSH(parent, code);
            else if ((code & 0b11001111) == 0b11000001) return new POP(parent, code);
            else if ((code & 0b11111000) == 0b10000000) return new ADD(parent, code);
            else if ((code & 0b11111111) == 0b11000110) return new ADI(parent);
            else if ((code & 0b11111000) == 0b10001000) return new ADC(parent, code);
            throw new NotImplementedException();
        }
    }
}