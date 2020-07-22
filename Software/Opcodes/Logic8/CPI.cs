using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class CPI : Opcode // CP n
    {
        public CPI(Gameboy parent) : base(parent) {
            Cycles = 8;
            Length = 2;

            Disassembly = "cp $" + parent.Memory[parent.PC + 1].ToString("X2");
        }

        public override void Execute()
        {
            byte orgVal = parent.Registers[Register.A];
            byte subVal = parent.Memory[parent.PC + 1];

            // Set Flags
            parent.SetFlag(Flag.AddSub, true);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal - subVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, ((orgVal & 0xF) - (subVal & 0xF)) < 0);
            parent.SetFlag(Flag.Carry, orgVal - subVal < 0);
        }
    }
}
