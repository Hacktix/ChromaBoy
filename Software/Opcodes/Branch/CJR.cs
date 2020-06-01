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
        }

        public override void Execute()
        {
            if((cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) == 0) || (!cFlagSet && (parent.Registers[Register.F] & (byte)cFlag) > 0))
            {
                Cycles = 8;
                return;
            }
            byte param = parent.Memory[parent.PC + 1];
            int addval = (param & 128) > 0 ? -(param & 0x7F) : param & 0x7F;
            parent.PC = (ushort)(parent.PC + addval);
        }
    }
}
