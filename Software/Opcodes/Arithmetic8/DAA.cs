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
            byte aval = parent.Registers[Register.A];
            byte corr = 0;
            bool setc = false;

            if(parent.GetFlag(Flag.HalfCarry) || (!parent.GetFlag(Flag.AddSub) && (aval & 0xF) > 9))
                corr |= 0x6;

            if(parent.GetFlag(Flag.Carry) || (!parent.GetFlag(Flag.AddSub) && aval > 0x99))
            {
                corr |= 0x60;
                setc = true;
            }

            if (parent.GetFlag(Flag.AddSub)) parent.Registers[Register.A] -= corr;
            else parent.Registers[Register.A] += corr;

            // Set Flags
            parent.SetFlag(Flag.Carry, setc);
            parent.SetFlag(Flag.HalfCarry, false);
            parent.SetFlag(Flag.Zero, parent.Registers[Register.A] == 0);
        }
    }
}
