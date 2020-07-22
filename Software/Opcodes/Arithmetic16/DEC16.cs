using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class DEC16 : Opcode // DEC rr
    {
        private Register16 target;

        public DEC16(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister16((opcode & 0b1110000) >> 4);

            Cycles = 8;
            Disassembly = "dec " + OpcodeUtils.Register16ToString(target);
        }

        public override void Execute()
        {
            parent.WriteRegister16(target, (ushort)(parent.ReadRegister16(target) - 1));
        }
    }
}
