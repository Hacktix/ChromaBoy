using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDAI : Opcode // LD A, (nn) | LD (nn), A
    {
        private bool load;
        private int actionTick;

        public LDAI(Gameboy parent, byte opcode) : base(parent) {
            load = (opcode & 0b10000) > 0;

            Cycles = 16;
            Length = 3;

            TickAccurate = true;
            actionTick = 8;
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if (Tick == actionTick)
            {
                byte srcVal = load ? parent.Memory[parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)] : parent.Registers[Register.A];
                if (load)
                    parent.Registers[Register.A] = srcVal;
                else
                    parent.Memory[parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)] = srcVal;
            }
        }
    }
}
