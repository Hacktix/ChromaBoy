using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class CJP : Opcode // JP NZ nn nn | JP Z nn nn | JP NC nn nn | JP C nn nn
    {
        private bool cFlagSet;
        private Flag cFlag;

        public CJP(Gameboy parent, byte opcode) : base(parent) {
            cFlagSet = (opcode & 0b1000) > 0;
            cFlag = (opcode & 0b10000) > 0 ? Flag.Carry : Flag.Zero;

            Length = 3;
            Cycles = 16;

            Disassembly = "jp " + (cFlagSet ? "" : "n") + OpcodeUtils.FlagToString(cFlag) + ", $" + (parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)).ToString("X1");
        }

        public override void Execute()
        {
            if((cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) == 0) || (!cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) > 0))
            {
                Cycles = 12;
                return;
            }
            parent.PC = (ushort)(parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8) - 3);
        }
    }
}
