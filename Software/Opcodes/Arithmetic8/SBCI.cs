using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class SBCI : Opcode // SBC A, n
    {
        public SBCI(Gameboy parent) : base(parent) {
            Cycles = 8;
            Length = 2;
        }

        public override void Execute()
        {
            byte orgVal = (byte)(parent.Registers[Register.A] - ((parent.Registers[Register.F] & (byte)Flag.Carry) > 0 ? 1 : 0));
            byte subVal = parent.Memory[parent.PC + 1];
            parent.Registers[Register.A] -= subVal;

            // Set Flags
            parent.SetFlag(Flag.AddSub, true);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal - subVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, ((orgVal & 0xF) - (subVal & 0xF)) < 0);
            parent.SetFlag(Flag.Carry, orgVal - subVal < 0);
        }
    }
}
