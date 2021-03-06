﻿using ChromaBoy.Hardware;
using ChromaBoy.Software.Opcodes;
using System;

namespace ChromaBoy.Software
{
    class Decoder
    {
        private static Opcode DecodePrefixOpcode(Gameboy parent, byte code)
        {
            if ((code & 0b11100000) == 0b00000000) return new PRT(parent, code);
            else if ((code & 0b11110000) == 0b00100000) return new PSA(parent, code);
            else if ((code & 0b11111000) == 0b00110000) return new SWAP(parent, code);
            else if ((code & 0b11111000) == 0b00111000) return new PSL(parent, code);
            else if ((code & 0b11000000) == 0b01000000) return new BIT(parent, code);
            else if ((code & 0b11000000) == 0b10000000) return new RES(parent, code);
            else if ((code & 0b11000000) == 0b11000000) return new SET(parent, code);
            throw new NotImplementedException("Unknown opcode 0xCB" + code.ToString("X2") + " at $" + parent.PC);
        }

        public static Opcode DecodeOpcode(Gameboy parent, byte code)
        {
            // Hande 0xCB Prefixed Opcodes
            if (code == 0xCB) return DecodePrefixOpcode(parent, parent.Memory[parent.PC + 1]);

            if ((code & 0b11111111) == 0b01110110) return new HALT(parent);
            else if ((code & 0b11111111) == 0b00010000) return new STOP(parent);
            else if ((code & 0b11000000) == 0b01000000) return new LD(parent, code);
            else if ((code & 0b11000111) == 0b00000110) return new LDI(parent, code);
            else if ((code & 0b11100111) == 0b00000010) return new LDA(parent, code);
            else if ((code & 0b11101111) == 0b11101010) return new LDAI(parent, code);
            else if ((code & 0b11101101) == 0b11100000) return new LDH(parent, code);
            else if ((code & 0b11100111) == 0b00100010) return new LDAHL(parent, code);
            else if ((code & 0b11001111) == 0b00000001) return new LD16(parent, code);
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
            else if ((code & 0b11001111) == 0b00001011) return new DEC16(parent, code);
            else if ((code & 0b11111111) == 0b11101000) return new ADDSP(parent);
            else if ((code & 0b11111111) == 0b11111000) return new LDHLSP(parent);
            else if ((code & 0b11100111) == 0b00000111) return new RTA(parent, code);
            else if ((code & 0b11111111) == 0b00111111) return new CCF(parent);
            else if ((code & 0b11111111) == 0b00110111) return new SCF(parent);
            else if ((code & 0b11111111) == 0b00000000) return new NOP();
            else if ((code & 0b11111111) == 0b11110011) return new DI(parent);
            else if ((code & 0b11111111) == 0b11111011) return new EI(parent);
            else if ((code & 0b11111111) == 0b11000011) return new JP(parent);
            else if ((code & 0b11111111) == 0b11101001) return new JPHL(parent);
            else if ((code & 0b11100111) == 0b11000010) return new CJP(parent, code);
            else if ((code & 0b11111111) == 0b00011000) return new JR(parent);
            else if ((code & 0b11100111) == 0b00100000) return new CJR(parent, code);
            else if ((code & 0b11111111) == 0b11001101) return new CALL(parent);
            else if ((code & 0b11100111) == 0b11000100) return new CCALL(parent, code);
            else if ((code & 0b11101111) == 0b11001001) return new RET(parent, code);
            else if ((code & 0b11100111) == 0b11000000) return new CRET(parent, code);
            else if ((code & 0b11000111) == 0b11000111) return new RST(parent, code);
            throw new NotImplementedException("Unknown opcode 0x" + code.ToString("X2") + " at $" + parent.PC);
        }
    }
}