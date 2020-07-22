using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDI : Opcode // LD r, n
    {
        private Register target;
        private int actionTick;

        public LDI(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister((opcode & 0b111000) >> 3);

            Cycles = target == Register.M ? 12 : 8;
            Length = 2;

            TickAccurate = true;
            actionTick = target == Register.M ? 4 : 2;

            Disassembly = "ld " + (target == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(target)) + ", $" + parent.Memory[parent.PC + 1].ToString("X2");
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if(Tick == actionTick)
            {
                byte srcVal = parent.Memory[parent.PC + 1];
                if (target == Register.M)
                    parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = srcVal;
                else
                    parent.Registers[target] = srcVal;
            }
        }
    }
}
