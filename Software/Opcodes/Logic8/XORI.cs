﻿using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class XORI : Opcode // XOR n
    {
        public XORI(Gameboy parent) : base(parent) {
            Cycles = 8;
            Length = 2;

            Disassembly = "xor $" + parent.Memory[parent.PC + 1].ToString("X2");
        }

        public override void Execute()
        {
            parent.Registers[Register.A] ^= parent.Memory[parent.PC + 1];

            // Set Flags
            parent.SetFlag(Flag.Zero, parent.Registers[Register.A] == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
            parent.SetFlag(Flag.Carry, false);
        }
    }
}
