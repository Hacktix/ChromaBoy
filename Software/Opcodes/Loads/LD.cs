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

            Cycles = source == Register.M || target == Register.M ? 8 : 4;

            Disassembly = "ld " + (target == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(target)) + ", " + (source == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(source));
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
