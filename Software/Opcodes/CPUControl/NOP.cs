using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class NOP : Opcode // NOP
    {
        public NOP() : base(null) {
            Disassembly = "nop";
        }

        public override void Execute() { }
    }
}
