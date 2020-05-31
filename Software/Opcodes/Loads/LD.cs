using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LD : Opcode // LD r, r
    {
        private Register source;
        private Register target;

        public LD(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister(opcode & 0b111);
            target = OpcodeUtils.BitsToRegister((opcode & 0b111000) >> 3);

            Cycles = target == Register.M ? 8 : 4;
        }

        public override void Execute()
        {
            byte srcVal = (source == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[source];
            if (target == Register.M)
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = srcVal;
            else
                parent.Registers[target] = srcVal;
        }
    }
}
