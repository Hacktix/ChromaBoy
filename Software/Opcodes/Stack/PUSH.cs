using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class PUSH : Opcode // PUSH rr
    {
        private Register16 source;

        public PUSH(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister16((opcode & 0b110000) >> 4);
            if (source == Register16.SP) source = Register16.AF;

            Cycles = 16;

            Disassembly = "push " + OpcodeUtils.Register16ToString(source);
        }

        public override void Execute()
        {
            ushort srcVal = parent.ReadRegister16(source);
            parent.Memory[--parent.SP] = (byte)((srcVal & 0xFF00) >> 8);
            parent.Memory[--parent.SP] = (byte)(srcVal & 0xFF);
        }
    }
}
