using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class PSL : Opcode // SRL r
    {
        private Register target;

        private byte readValue;

        public PSL(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Length = 2;
            Cycles = target == Register.M ? 16 : 8;
            TickAccurate = target == Register.M;

            Disassembly = "srl " + OpcodeUtils.RegisterToString(target);
        }

        public override void Execute()
        {
            byte rVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
            parent.SetFlag(Flag.Carry, (rVal & 1) > 0);
            if (target == Register.M)
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)(rVal >> 1);
            else
                parent.Registers[target] = (byte)(rVal >> 1);

            // Set Flags
            parent.SetFlag(Flag.Zero, ((target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target]) == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if(Tick == 7)
            {
                readValue = parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])];
            } else if(Tick == 11)
            {
                parent.SetFlag(Flag.Carry, (readValue & 1) > 0);
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)(readValue >> 1);

                // Set Flags
                parent.SetFlag(Flag.Zero, parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] == 0);
                parent.SetFlag(Flag.AddSub, false);
                parent.SetFlag(Flag.HalfCarry, false);
            }
        }
    }
}
