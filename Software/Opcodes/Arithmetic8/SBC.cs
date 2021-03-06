﻿using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class SBC : Opcode // SBC A, r
    {
        private Register source;

        public SBC(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Cycles = source == Register.M ? 8 : 4;
            Disassembly = "sbc " + (source == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(source));
        }

        public override void Execute()
        {
            int areg = parent.Registers[Register.A];
            int imm = (source == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[source];
            int carry = parent.GetFlag(Flag.Carry) ? 1 : 0;
            parent.Registers[Register.A] = (byte)(areg - imm - carry);

            // Set Flags
            parent.SetFlag(Flag.AddSub, true);
            parent.SetFlag(Flag.Zero, parent.Registers[Register.A] == 0);
            parent.SetFlag(Flag.Carry, (areg - imm - carry) < 0);
            parent.SetFlag(Flag.HalfCarry, ((areg & 0xF) - (imm & 0xF) - carry) < 0);
        }
    }
}
