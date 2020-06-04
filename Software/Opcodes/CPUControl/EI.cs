using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class EI : Opcode // EI
    {
        public EI(Gameboy parent) : base(parent) { }

        public override void Execute()
        {
            parent.EINextInstruction = true;
        }
    }
}
