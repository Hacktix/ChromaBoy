using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class SUB : Opcode // SUB A, r
    {
        private Register source;

        public SUB(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Cycles = source == Register.M ? 8 : 4;
            Disassembly = "SUB A, " + (source == Register.M ? "$" + ((parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])).ToString("X4") : OpcodeUtils.RegisterToString(source));
        }

        public override void Execute()
        {
            byte orgVal = parent.Registers[Register.A];
            byte subVal = (source == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[source];
            parent.Registers[Register.A] -= subVal;

            // Set Flags
            parent.SetFlag(Flag.AddSub, true);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal - subVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, ((orgVal & 0xF) - (subVal & 0xF)) < 0);
            parent.SetFlag(Flag.Carry, orgVal - subVal < 0);
        }
    }
}
