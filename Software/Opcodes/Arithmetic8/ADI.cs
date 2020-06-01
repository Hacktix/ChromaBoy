using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class ADI : Opcode // ADD A, n
    {
        public ADI(Gameboy parent) : base(parent) {
            Length = 2;
            Cycles = 8;
        }

        public override void Execute()
        {
            byte orgVal = parent.Registers[Register.A];
            byte addVal = parent.Memory[parent.PC + 1];
            parent.Registers[Register.A] += addVal;

            // Set Flags
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal + addVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, (((orgVal & 0xF) + (addVal & 0xF)) & 0x10) == 0x10);
            parent.SetFlag(Flag.Carry, orgVal + addVal > 255);
        }
    }
}
