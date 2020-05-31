using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDMSP : Opcode // LD (nn), SP
    {
        public LDMSP(Gameboy parent) : base(parent) {
            Cycles = 20;
            Length = 3;
        }

        public override void Execute()
        {
            ushort srcVal = (ushort)(parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8));
            parent.WriteRegister16(Register16.SP, srcVal);
        }
    }
}
