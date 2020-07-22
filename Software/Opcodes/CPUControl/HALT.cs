using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class HALT : Opcode // HALT
    {
        public HALT(Gameboy parent) : base(parent) {
            Disassembly = "halt";
        }

        public override void Execute()
        {
            if(parent.InterruptsEnabled)
                parent.Halted = true;
            else
            {
                if ((parent.Memory[0xFF0F] & parent.Memory[0xFFFF]) == 0)
                {
                    parent.Halted = true;
                    parent.CallInterruptHandler = false;
                }
                else
                {
                    parent.Halted = true;
                    parent.HaltBug = true;
                }
            }
        }
    }
}
