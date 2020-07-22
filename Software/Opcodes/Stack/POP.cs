using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class POP : Opcode // POP rr
    {
        private Register16 target;

        public POP(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister16((opcode & 0b110000) >> 4);
            if (target == Register16.SP) target = Register16.AF;

            Cycles = 12;

            Disassembly = "pop " + OpcodeUtils.Register16ToString(target);
        }

        public override void Execute()
        {
            ushort srcVal = (ushort)(parent.Memory[parent.SP++] + (parent.Memory[parent.SP++] << 8));
            parent.WriteRegister16(target, srcVal);
        }
    }
}
