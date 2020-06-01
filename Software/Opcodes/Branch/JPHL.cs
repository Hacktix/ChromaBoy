using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class JPHL : Opcode // JP HL
    {
        public JPHL(Gameboy parent) : base(parent) {
            Cycles = 4;
        }

        public override void Execute()
        {
            parent.PC = parent.ReadRegister16(Register16.HL);
        }
    }
}
