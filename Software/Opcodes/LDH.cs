using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDH : Opcode // LDH A, (C) | LDH (C), A | LDH A, (n) | LDH (n), A
    {
        private bool regC;
        private bool load;

        public LDH(Gameboy parent, byte opcode) : base(parent) {
            load = (opcode & 0b10000) > 0;
            regC = (opcode & 0b10) > 0;

            Cycles = regC ? 8 : 12;
            Length = regC ? 1 : 2;
        }

        public override void Execute()
        {
            int memaddr = 0xFF00 + (regC ? parent.Registers[Register.C] : parent.Memory[parent.PC + 1]);
            byte srcVal = load ? parent.Memory[memaddr] : parent.Registers[Register.A];
            if (load)
                parent.Registers[Register.A] = srcVal;
            else
                parent.Memory[memaddr] = srcVal;
        }
    }
}
