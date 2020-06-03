using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class ADCI : Opcode // ADC A, n
    {
        public ADCI(Gameboy parent) : base(parent) {
            Length = 2;
            Cycles = 8;
        }

        public override void Execute()
        {
            int areg = parent.Registers[Register.A];
            int imm = parent.Memory[parent.PC + 1];
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
