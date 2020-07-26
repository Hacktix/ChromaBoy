using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LD16 : Opcode // LD rr, nn
    {
        private Register16 target;
        private Register targetLow;
        private Register targetHigh;

        public LD16(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister16((opcode & 0b110000) >> 4);
            targetLow = target == Register16.BC ? Register.C : target == Register16.DE ? Register.E : Register.L;
            targetHigh = target == Register16.BC ? Register.B : target == Register16.DE ? Register.D : Register.H;

            Cycles = 12;
            Length = 3;
            TickAccurate = true;

            Disassembly = "ld " + OpcodeUtils.Register16ToString(target) + ", $" + (parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)).ToString("X2");
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if(Tick == 3)
            {
                if (target == Register16.SP)
                    parent.SP = parent.Memory[parent.PC + 1];
                else
                    parent.Registers[targetLow] = parent.Memory[parent.PC + 1];
            } else if(Tick == 7)
            {
                if (target == Register16.SP)
                    parent.SP = (ushort)(parent.SP + (parent.Memory[parent.PC + 2] << 8));
                else
                    parent.Registers[targetHigh] = parent.Memory[parent.PC + 2];
            }
        }
    }
}
