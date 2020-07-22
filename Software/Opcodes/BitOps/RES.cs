using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class RES : Opcode // RES r
    {
        private Register target;
        private byte bit;

        public RES(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister(opcode & 0b111);
            bit = (byte)((opcode & 0b111000) >> 3);

            Cycles = target == Register.M ? 16 : 8;
            Length = 2;

            Disassembly = "RES " + bit + ", " + (target == Register.M ? "$" + ((parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])).ToString("X4") : OpcodeUtils.RegisterToString(target));
        }

        public override void Execute()
        {
            if (target == Register.M)
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] &= (byte)~(1 << bit);
            else
                parent.Registers[target] &= (byte)~(1 << bit);
        }
    }
}
