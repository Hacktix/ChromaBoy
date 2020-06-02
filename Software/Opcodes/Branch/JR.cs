using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class JR : Opcode // JR e
    {
        public JR(Gameboy parent) : base(parent) {
            Length = 2;
            Cycles = 12;
        }

        public override void Execute()
        {
            byte param = parent.Memory[parent.PC + 1];
            int addval = (param & 128) > 0 ? -(param & 0x7F) : param & 0x7F;
            parent.PC = (ushort)(parent.PC + addval);
        }
    }
}
