using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class RTA : Opcode // RLCA, RLA, RRCA, RRA
    {
        private bool left;
        private bool useCarry;

        public RTA(Gameboy parent, byte opcode) : base(parent) {
            left = (opcode & 0b1000) == 0;
            useCarry = (opcode & 0b10000) > 0;
        }

        public override void Execute()
        {
            if(left)
            {
                if(useCarry)
                {
                    byte carryAdd = (byte)((parent.Registers[Register.F] & 0b10000) >> 4);
                    parent.SetFlag(Flag.Carry, (parent.Registers[Register.A] & 128) > 0);
                    parent.Registers[Register.A] = (byte)((parent.Registers[Register.A] << 1) + carryAdd);
                } else {
                    byte carryAdd = (byte)((parent.Registers[Register.A] & 128) >> 7);
                    parent.SetFlag(Flag.Carry, (parent.Registers[Register.A] & 128) > 0);
                    parent.Registers[Register.A] = (byte)((parent.Registers[Register.A] << 1) + carryAdd);
                }
            } else {
                if (useCarry)
                {
                    byte carryAdd = (byte)((parent.Registers[Register.F] & 0b10000) << 3);
                    parent.SetFlag(Flag.Carry, (parent.Registers[Register.A] & 1) > 0);
                    parent.Registers[Register.A] = (byte)((parent.Registers[Register.A] >> 1) + carryAdd);
                }
                else
                {
                    byte carryAdd = (byte)((parent.Registers[Register.A] & 1) << 7);
                    parent.SetFlag(Flag.Carry, (parent.Registers[Register.A] & 1) > 0);
                    parent.Registers[Register.A] = (byte)((parent.Registers[Register.A] >> 1) + carryAdd);
                }
            }

            // Set Flags
            parent.SetFlag(Flag.Zero, false);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
        }
    }
}
