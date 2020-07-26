using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class INC : Opcode // INC r
    {
        private Register target;

        private byte readValue;

        public INC(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister((opcode & 0b111000) >> 3);

            Cycles = target == Register.M ? 12 : 4;
            TickAccurate = target == Register.M;
            Disassembly = "inc " + (target == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(target));
        }

        public override void Execute()
        {
            byte orgVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
            byte addVal = 1;
            if (target == Register.M)
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] += addVal;
            else
                parent.Registers[target] += addVal;

            // Set Flags
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.Zero, ((byte)(orgVal + addVal)) == 0);
            parent.SetFlag(Flag.HalfCarry, (((orgVal & 0xF) + (addVal & 0xF)) & 0x10) == 0x10);
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if(Tick == 3)
            {
                readValue = parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])];
            } else if(Tick == 7)
            {
                byte orgVal = readValue;
                byte addVal = 1;
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)(readValue + 1);

                // Set Flags
                parent.SetFlag(Flag.AddSub, false);
                parent.SetFlag(Flag.Zero, ((byte)(orgVal + addVal)) == 0);
                parent.SetFlag(Flag.HalfCarry, (((orgVal & 0xF) + (addVal & 0xF)) & 0x10) == 0x10);
            }
        }
    }
}
