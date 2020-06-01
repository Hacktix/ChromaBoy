using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class STOP : Opcode // STOP
    {
        public STOP(Gameboy parent) : base(parent) { }

        public override void Execute()
        {
            parent.Standby = true;
        }
    }
}
