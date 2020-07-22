using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class DEC : Opcode // DEC r
    {
        private Register target;

        public DEC(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister((opcode & 0b111000) >> 3);

            Cycles = target == Register.M ? 12 : 4;
            Disassembly = "dec " + (target == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(target));
        }

        public override void Execute()
        {
            byte orgVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
            byte subVal = 1;
            if (target == Register.M)
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] -= subVal;
            else
                parent.Registers[target] -= subVal;

            // Set Flags
            parent.SetFlag(Flag.AddSub, true);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal - subVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, ((orgVal & 0xF) - (subVal & 0xF)) < 0);
        }
    }
}
