using ChromaBoy.Hardware;
using ChromaBoy.Software.Opcodes;
using System;

namespace ChromaBoy.Software
{
    class Decoder
    {
        public static Opcode DecodeOpcode(Gameboy parent, byte code)
        {
            // TODO: Add HALT operation before LD in if-chain
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
            else if ((code & 0b11111111) == 0b11001110) return new ADCI(parent);
            else if ((code & 0b11111000) == 0b10010000) return new SUB(parent, code);
            else if ((code & 0b11111111) == 0b11010110) return new SBI(parent);
            else if ((code & 0b11111000) == 0b10011000) return new SBC(parent, code);
            else if ((code & 0b11111111) == 0b11011110) return new SBCI(parent);
            else if ((code & 0b11000111) == 0b00000100) return new INC(parent, code);
            else if ((code & 0b11000111) == 0b00000101) return new DEC(parent, code);
            else if ((code & 0b11111111) == 0b00100111) return new DAA(parent);
            else if ((code & 0b11111111) == 0b00101111) return new CPL(parent);
            else if ((code & 0b11111000) == 0b10100000) return new AND(parent, code);
            else if ((code & 0b11111111) == 0b11100110) return new ANDI(parent);
            else if ((code & 0b11111000) == 0b10101000) return new XOR(parent, code);
            else if ((code & 0b11111111) == 0b11101110) return new XORI(parent);
            else if ((code & 0b11111000) == 0b10110000) return new OR(parent, code);
            else if ((code & 0b11111111) == 0b11110110) return new ORI(parent);
            else if ((code & 0b11111000) == 0b10111000) return new CP(parent, code);
            else if ((code & 0b11111111) == 0b11111110) return new CPI(parent);
            else if ((code & 0b11000111) == 0b00000001) return new ADD16(parent, code);
            else if ((code & 0b11001111) == 0b00000011) return new INC16(parent, code);
            throw new NotImplementedException();
        }
    }
}