using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LD16 : Opcode // LD rr, nn
    {
        private Register16 target;

        public LD16(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister16((opcode & 0b110000) >> 4);

            Cycles = 12;
            Length = 2;
        }

        public override void Execute()
        {
            ushort srcVal = (ushort)(parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8));
            parent.WriteRegister16(target, srcVal);
        }
    }
}
