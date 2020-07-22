using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDSPHL : Opcode // LD SP, HL
    {
        public LDSPHL(Gameboy parent) : base(parent) {
            Cycles = 8;

            Disassembly = "ld sp, hl";
        }

        public override void Execute()
        {
            parent.WriteRegister16(Register16.SP, parent.ReadRegister16(Register16.HL));
        }
    }
}
