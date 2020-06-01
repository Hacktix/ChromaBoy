using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class ORI : Opcode // OR n
    {
        public ORI(Gameboy parent) : base(parent) {
            Cycles = 8;
            Length = 2;
        }

        public override void Execute()
        {
            parent.Registers[Register.A] |= parent.Memory[parent.PC + 1];

            // Set Flags
            parent.SetFlag(Flag.Zero, parent.Registers[Register.A] == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
            parent.SetFlag(Flag.Carry, false);
        }
    }
}
