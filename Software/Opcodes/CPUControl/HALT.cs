using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class HALT : Opcode // HALT
    {
        public HALT(Gameboy parent) : base(parent) { }

        public override void Execute()
        {
            parent.Halted = true;
        }
    }
}
