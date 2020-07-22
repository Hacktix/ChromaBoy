using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class SWAP : Opcode // SWAP r
    {
        private Register target;

        public SWAP(Gameboy parent, byte opcode) : base(parent) {
            target = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Length = 2;
            Cycles = (target == Register.M) ? 16 : 8;

            Disassembly = "swap " + (target == Register.M ? "[hl]" : OpcodeUtils.RegisterToString(target));
        }

        public override void Execute()
        {
            byte sVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
            sVal = (byte)((sVal << 4) | (sVal >> 4));
            if (target == Register.M)
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = sVal;
            else
                parent.Registers[target] = sVal;

            // Set Flags
            parent.SetFlag(Flag.Zero, sVal == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
            parent.SetFlag(Flag.Carry, false);
        }
    }
}
