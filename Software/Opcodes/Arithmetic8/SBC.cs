using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class SBC : Opcode // SBC A, r
    {
        private Register source;

        public SBC(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Cycles = source == Register.M ? 8 : 4;
        }

        public override void Execute()
        {
            byte orgVal = (byte)(parent.Registers[Register.A]);
            int subVal = ((source == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[source]) + ((parent.Registers[Register.F] & (byte)Flag.Carry) > 0 ? 1 : 0);
            parent.Registers[Register.A] -= (byte)subVal;

            // Set Flags
            parent.SetFlag(Flag.AddSub, true);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal - subVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, ((orgVal & 0xF) - (subVal & 0xF)) < 0);
            parent.SetFlag(Flag.Carry, orgVal - subVal < 0);
        }
    }
}
