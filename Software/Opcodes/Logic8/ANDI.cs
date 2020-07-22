using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class ANDI : Opcode // AND n
    {
        public ANDI(Gameboy parent) : base(parent) {
            Cycles = 8;
            Length = 2;

            Disassembly = "and $" + parent.Memory[parent.PC + 1].ToString("X2");
        }

        public override void Execute()
        {
            parent.Registers[Register.A] &= parent.Memory[parent.PC + 1];

            // Set Flags
            parent.SetFlag(Flag.Zero, parent.Registers[Register.A] == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, true);
            parent.SetFlag(Flag.Carry, false);
        }
    }
}
