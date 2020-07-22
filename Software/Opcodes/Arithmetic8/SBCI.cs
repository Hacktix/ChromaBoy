using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class SBCI : Opcode // SBC A, n
    {
        public SBCI(Gameboy parent) : base(parent) {
            Cycles = 8;
            Length = 2;

            Disassembly = "sbc $" + parent.Memory[parent.PC + 1].ToString("X2");
        }

        public override void Execute()
        {
            int areg = parent.Registers[Register.A];
            int imm = parent.Memory[parent.PC + 1];
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
