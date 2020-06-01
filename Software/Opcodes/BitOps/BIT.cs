using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class BIT : Opcode // BIT r
    {
        private Register target;
        private byte bit;

        public BIT(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister(opcode & 0b111);
            bit = (byte)((opcode & 0b111000) >> 3);

            Cycles = target == Register.M ? 12 : 8;
            Length = 2;
        }

        public override void Execute()
        {
            byte tVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
            parent.SetFlag(Flag.Zero, (tVal & (1 << bit)) == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
        }
    }
}
