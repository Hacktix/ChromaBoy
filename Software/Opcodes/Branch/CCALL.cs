using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class CCALL : Opcode // CALL NZ nn nn | CALL Z nn nn | CALL NC nn nn | CALL C nn nn
    {
        private bool cFlagSet;
        private Flag cFlag;

        public CCALL(Gameboy parent, byte opcode) : base(parent) {
            cFlagSet = (opcode & 0b1000) > 0;
            cFlag = (opcode & 0b10000) > 0 ? Flag.Carry : Flag.Zero;

            Length = 3;
            Cycles = 24;

            Disassembly = "CALL " + (cFlagSet ? "" : "N") + OpcodeUtils.FlagToString(cFlag) + " $" + (parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)).ToString("X4");
        }

        public override void Execute()
        {
            if((cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) == 0) || (!cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) > 0))
            {
                Cycles = 12;
                return;
            }
            parent.SP -= 2;
            ushort stackval = (ushort)(parent.PC + 3);
            parent.Memory[parent.SP + 1] = (byte)((stackval & 0xFF00) >> 8);
            parent.Memory[parent.SP] = (byte)(stackval & 0xFF);
            parent.PC = (ushort)(parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8) - 3);
        }
    }
}
