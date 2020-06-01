using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class SCF : Opcode // SCF
    {
        public SCF(Gameboy parent) : base(parent) { }

        public override void Execute()
        {
            parent.SetFlag(Flag.Carry, true);
        }
    }
}
