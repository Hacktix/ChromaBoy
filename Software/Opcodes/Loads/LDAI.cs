using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDAI : Opcode // LD A, (nn) | LD (nn), A
    {
        private bool load;

        private ushort readAddress;

        public LDAI(Gameboy parent, byte opcode) : base(parent) {
            load = (opcode & 0b10000) > 0;

            Cycles = 16;
            Length = 3;

            TickAccurate = true;

            Disassembly = load ? "ld a, [$" + (parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)).ToString("X4") + "]" : "ld [$" + (parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)).ToString("X4") + "], a";
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if(Tick == 3)
            {
                readAddress = parent.Memory[parent.PC + 1];
            } else if(Tick == 7)
            {
                readAddress = (ushort)(readAddress + (parent.Memory[parent.PC + 2] << 8));
            } else if(Tick == 11)
            {
                byte srcVal = load ? parent.Memory[readAddress] : parent.Registers[Register.A];
                if (load)
                    parent.Registers[Register.A] = srcVal;
                else
                    parent.Memory[readAddress] = srcVal;
            }
        }
    }
}
