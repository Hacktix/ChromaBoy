using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class ADD : Opcode // ADD A, r
    {
        private Register source;

        public ADD(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Cycles = source == Register.M ? 8 : 4;
        }

        public override void Execute()
        {
            byte orgVal = parent.Registers[Register.A];
            byte addVal = (source == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[source];
            parent.Registers[Register.A] += addVal;

            // Set Flags
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal + addVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, (((orgVal & 0xF) + (addVal & 0xF)) & 0x10) == 0x10);
            parent.SetFlag(Flag.Carry, orgVal + addVal > 255);
        }
    }
}
