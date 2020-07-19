using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class CPL : Opcode // CPL
    {
        public CPL(Gameboy parent) : base(parent) { }

        public override void Execute()
        {
            parent.Registers[Register.A] = (byte)(~parent.Registers[Register.A]);

            // Set Flags
            parent.SetFlag(Flag.AddSub, true);
            parent.SetFlag(Flag.HalfCarry, true);
        }
    }
}
