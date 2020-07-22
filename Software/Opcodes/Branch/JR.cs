using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class JR : Opcode // JR e
    {
        public JR(Gameboy parent) : base(parent) {
            Length = 2;
            Cycles = 12;
            Disassembly = "jr $" + (parent.PC + (sbyte)parent.Memory[parent.PC + 1] + 2).ToString("X4");
        }

        public override void Execute()
        {
            sbyte addval = (sbyte)parent.Memory[parent.PC + 1];
            parent.PC = (ushort)(parent.PC + addval);
        }
    }
}
