using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class CRET : Opcode // RET NZ | RET Z | RET NC | RET C
    {
        private bool cFlagSet;
        private Flag cFlag;

        public CRET(Gameboy parent, byte opcode) : base(parent) {
            cFlagSet = (opcode & 0b1000) > 0;
            cFlag = (opcode & 0b10000) > 0 ? Flag.Carry : Flag.Zero;

            Cycles = 20;

            Disassembly = "ret " + (cFlagSet ? "" : "n") + OpcodeUtils.FlagToString(cFlag);
        }

        public override void Execute()
        {
            if ((cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) == 0) || (!cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) > 0))
            {
                Cycles = 8;
                return;
            }
            parent.PC = (ushort)(parent.Memory[parent.SP] + (parent.Memory[parent.SP + 1] << 8) - 1);
            parent.SP += 2;
        }
    }
}
