using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class JR : Opcode // JR e
    {
        public JR(Gameboy parent) : base(parent) {
            Length = 2;
            Cycles = 12;
            Disassembly = "JR $" + parent.Memory[parent.PC + 1].ToString("X2");
        }

        public override void Execute()
        {
            sbyte addval = (sbyte)parent.Memory[parent.PC + 1];
            parent.PC = (ushort)(parent.PC + addval);
        }
    }
}
