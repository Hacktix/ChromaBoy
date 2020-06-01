using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class SBI : Opcode // SUB A, n
    {
        public SBI(Gameboy parent) : base(parent) {
            Cycles = 8;
        }

        public override void Execute()
        {
            byte orgVal = parent.Registers[Register.A];
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
