using ChromaBoy.Hardware;
using System;

namespace ChromaBoy.Software.Opcodes
{
    public class LDH : Opcode // LDH A, (C) | LDH (C), A | LDH A, (n) | LDH (n), A
    {
        private bool regC;
        private bool load;
        private int actionTick;

        public LDH(Gameboy parent, byte opcode) : base(parent) {
            load = (opcode & 0b10000) > 0;
            regC = (opcode & 0b10) > 0;

            Cycles = regC ? 8 : 12;
            Length = regC ? 1 : 2;

            TickAccurate = true;
            actionTick = regC ? 2 : 4;

            Disassembly = load ? "ld a, [$" + ((ushort)(0xFF00 + (regC ? parent.Registers[Register.C] : parent.Memory[parent.PC + 1]))).ToString("X4") + "]" : "ld [$" + ((ushort)(0xFF00 + (regC ? parent.Registers[Register.C] : parent.Memory[parent.PC + 1]))).ToString("X4") + "], a";
        }

        public override void ExecuteTick()
        {
            Tick++;
            if (Tick == actionTick)
            {
                int memaddr = 0xFF00 + (regC ? parent.Registers[Register.C] : parent.Memory[parent.PC + 1]);
                byte srcVal = load ? parent.Memory[memaddr] : parent.Registers[Register.A];
                if (load)
                    parent.Registers[Register.A] = srcVal;
                else
                    parent.Memory[memaddr] = srcVal;
            }
        }
    }
}
