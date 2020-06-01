using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class RET : Opcode // RET
    {
        private bool enableInt;

        public RET(Gameboy parent, byte opcode) : base(parent) {
            enableInt = (opcode & 0b10000) > 0;

            Cycles = 16;
        }

        public override void Execute()
        {
            if (enableInt) parent.InterruptsEnabled = true;
            parent.PC = (ushort)(parent.Memory[parent.SP] + (parent.Memory[parent.SP + 1] << 8) - 1);
            parent.SP += 2;
        }
    }
}
