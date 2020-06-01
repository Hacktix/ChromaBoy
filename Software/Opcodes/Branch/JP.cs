using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class JP : Opcode // JP nn nn
    {
        public JP(Gameboy parent) : base(parent) {
            Length = 3;
            Cycles = 16;
        }

        public override void Execute()
        {
            parent.PC = (ushort)(parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8));
        }
    }
}
