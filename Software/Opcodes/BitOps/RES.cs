using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class RES : Opcode // RES r
    {
        private Register target;
        private byte bit;

        private byte modValue;
        private ushort modAddress;

        public RES(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister(opcode & 0b111);
            bit = (byte)((opcode & 0b111000) >> 3);

            Cycles = target == Register.M ? 16 : 8;
            Length = 2;
            TickAccurate = true;

            Disassembly = "res $" + bit.ToString("X2") + ", " + (target == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(target));
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if(target == Register.M && Tick == 3) {
                modAddress = (ushort)((parent.Registers[Register.H] << 8) | (parent.Registers[Register.L]));
            } else if ((target == Register.M && Tick == 7) || (target != Register.M && Tick == 3)) {
                modValue = target == Register.M ? parent.Memory[modAddress] : parent.Registers[target];
            } else if((target == Register.M && Tick == 11) || (target != Register.M && Tick == 7)) {
                if (target == Register.M)
                    parent.Memory[modAddress] = (byte)(modValue & (byte)~(1 << bit));
                else
                    parent.Registers[target] = (byte)(modValue & (byte)~(1 << bit));
            }
        }
    }
}
