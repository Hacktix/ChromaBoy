using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class CALL : Opcode // CALL nn nn
    {
        public CALL(Gameboy parent) : base(parent) {
            Length = 3;
            Cycles = 24;

            Disassembly = "call $" + (parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)).ToString("X1");
        }

        public override void Execute()
        {
            parent.SP -= 2;
            ushort stackval = (ushort)(parent.PC + 3);
            parent.Memory[parent.SP + 1] = (byte)((stackval & 0xFF00) >> 8);
            parent.Memory[parent.SP] = (byte)(stackval & 0xFF);
            parent.PC = (ushort)(parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8) - 3);
        }
    }
}
