using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class INC16 : Opcode // INC rr
    {
        public INC16(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister16((opcode & 0b1110000) >> 4);

            Cycles = 8;
        }

        public override void Execute()
        {
            parent.WriteRegister16(Register16.HL, (ushort)(parent.ReadRegister16(Register16.HL) + 1));
        }
    }
}
