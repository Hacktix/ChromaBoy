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
            byte orgVal = (byte)(parent.Registers[Register.A] + ((parent.Registers[Register.F] & (byte)Flag.Carry) > 0 ? 1 : 0));
            byte addVal = parent.Memory[parent.PC + 1];
            parent.Registers[Register.A] += addVal;

            // Set Flags
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal + addVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, (((orgVal & 0xF) + (addVal & 0xF)) & 0x10) == 0x10);
            parent.SetFlag(Flag.Carry, orgVal + addVal > 0);
        }
    }
}
