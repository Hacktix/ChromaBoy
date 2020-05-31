using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDI : Opcode // LD r, n
    {
        private Register target;

        public LDI(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister((opcode & 0b111000) >> 3);

            Cycles = target == Register.M ? 12 : 8;
            Length = 2;
        }

        public override void Execute()
        {
            byte srcVal = parent.Memory[parent.PC + 1];
            if (target == Register.M)
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = srcVal;
            else
                parent.Registers[target] = srcVal;
        }
    }
}
