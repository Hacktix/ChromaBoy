using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class CCF : Opcode // CCF
    {
        public CCF(Gameboy parent) : base(parent) {
            Disassembly = "ccf";        
        }

        public override void Execute()
        {
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
            parent.SetFlag(Flag.Carry, (parent.Registers[Register.F] & (byte)Flag.Carry) == 0);
        }
    }
}
