using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class CJR : Opcode // JR NZ e | JR Z e | JR NC e | JR C e
    {
        private bool cFlagSet;
        private Flag cFlag;

        public CJR(Gameboy parent, byte opcode) : base(parent) {
            cFlagSet = (opcode & 0b1000) > 0;
            cFlag = (opcode & 0b10000) > 0 ? Flag.Carry : Flag.Zero;

            Length = 2;
            Cycles = 12;

            Disassembly = "jr " + (cFlagSet ? "" : "n") + OpcodeUtils.FlagToString(cFlag) + ", $" + (parent.PC + (sbyte)parent.Memory[parent.PC + 1] + 2).ToString("X4");
        }

        public override void Execute()
        {
            if((cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) == 0) || (!cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) > 0))
            {
                Cycles = 8;
                return;
            }
            sbyte addval = (sbyte)parent.Memory[parent.PC + 1];
            parent.PC = (ushort)(parent.PC + addval);
        }
    }
}
