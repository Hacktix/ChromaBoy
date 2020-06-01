using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class DAA : Opcode // DAA
    {
        public DAA(Gameboy parent) : base(parent) {
            Cycles = 4;
        }

        public override void Execute()
        {
            if((parent.Registers[Register.F] & (byte)Flag.AddSub) == 0)
            {
                if((parent.Registers[Register.F] & (byte)Flag.Carry) > 0 || parent.Registers[Register.A] > 0x99)
                {
                    parent.Registers[Register.A] += 0x60;
                    parent.SetFlag(Flag.Carry, true);
                }
                if ((parent.Registers[Register.F] & (byte)Flag.HalfCarry) > 0 || (parent.Registers[Register.A] & 0xF) > 0x09)
                    parent.Registers[Register.A] += 0x6;
            } else {
                if ((parent.Registers[Register.F] & (byte)Flag.Carry) > 0)
                    parent.Registers[Register.A] += 0x60;
                if ((parent.Registers[Register.F] & (byte)Flag.HalfCarry) > 0)
                    parent.Registers[Register.A] += 0x6;
            }

            // Set Flags
            parent.SetFlag(Flag.HalfCarry, false);
            parent.SetFlag(Flag.Zero, parent.Registers[Register.A] == 0);
        }
    }
}
