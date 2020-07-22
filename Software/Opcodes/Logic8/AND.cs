using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class AND : Opcode // AND r
    {
        private Register source;

        public AND(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Cycles = source == Register.M ? 8 : 4;

            Disassembly = "and " + (source == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(source));
        }

        public override void Execute()
        {
            parent.Registers[Register.A] &= (source == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[source];

            // Set Flags
            parent.SetFlag(Flag.Zero, parent.Registers[Register.A] == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, true);
            parent.SetFlag(Flag.Carry, false);
        }
    }
}
