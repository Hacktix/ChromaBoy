using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class RST : Opcode // RST
    {
        private ushort addr;

        public RST(Gameboy parent, byte opcode) : base(parent) {
            addr = (ushort)(opcode & 0b00111000);

            Cycles = 16;
            Disassembly = "RST $" + (addr & 0xFF).ToString("X2");
        }

        public override void Execute()
        {
            parent.SP -= 2;
            ushort stackval = (ushort)(parent.PC + 1);
            parent.Memory[parent.SP + 1] = (byte)((stackval & 0xFF00) >> 8);
            parent.Memory[parent.SP] = (byte)(stackval & 0xFF);
            parent.PC = (ushort)(addr - 1);
        }
    }
}
