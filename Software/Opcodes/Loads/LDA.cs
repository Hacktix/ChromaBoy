using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDA : Opcode // LD A, (rr) | LD (rr), A
    {
        private Register16 regpair;
        private bool load;

        public LDA(Gameboy parent, byte opcode) : base(parent) {
            load = (opcode & 0b1000) > 0;
            regpair = (opcode & 0b10000) > 0 ? Register16.DE : Register16.BC;

            Cycles = 8;

            Disassembly = load ? "ld a, [" + OpcodeUtils.Register16ToString(regpair) + "]" : "ld [" + OpcodeUtils.Register16ToString(regpair) + "], a";
        }

        public override void Execute()
        {
            byte srcVal = load ? parent.Memory[parent.ReadRegister16(regpair)] : parent.Registers[Register.A];
            if (load)
                parent.Registers[Register.A] = srcVal;
            else
                parent.Memory[parent.ReadRegister16(regpair)] = srcVal;
        }
    }
}
