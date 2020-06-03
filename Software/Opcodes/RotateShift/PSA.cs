using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class PSA : Opcode // SLA, r | SRA, r
    {
        private bool left;
        private Register target;

        public PSA(Gameboy parent, byte opcode) : base(parent) {
            left = (opcode & 0b1000) == 0;
            target = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Length = 2;
            Cycles = 8;
        }

        public override void Execute()
        {
            byte rVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
            if (left)
            {
                parent.SetFlag(Flag.Carry, (rVal & 128) > 0);
                if (target == Register.M)
                    parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)(rVal << 1);
                else
                    parent.Registers[target] = (byte)(rVal << 1);
            } else {
                parent.SetFlag(Flag.Carry, (rVal & 1) > 0);
                byte msb = (byte)(rVal & 128);
                if (target == Register.M)
                    parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((rVal >> 1) | msb);
                else
                    parent.Registers[target] = (byte)((rVal >> 1) | msb);
            }

            // Set Flags
            parent.SetFlag(Flag.Zero, ((target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target]) == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
        }
    }
}
