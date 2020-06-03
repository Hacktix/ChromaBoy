using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class ADC : Opcode // ADC A, r
    {
        private Register source;

        public ADC(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Cycles = source == Register.M ? 8 : 4;
        }

        public override void Execute()
        {
            int areg = parent.Registers[Register.A];
            int imm = (source == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[source];
            int carry = parent.GetFlag(Flag.Carry) ? 1 : 0;
            parent.Registers[Register.A] = (byte)(areg + imm + carry);

            // Set Flags
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.Zero, parent.Registers[Register.A] == 0);
            parent.SetFlag(Flag.Carry, (areg + imm + carry) > 0xFF);
            parent.SetFlag(Flag.HalfCarry, ((areg & 0xF) + (imm & 0xF) + carry) > 0xF);
        }
    }
}
