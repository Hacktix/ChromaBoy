using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class ADD16 : Opcode // ADD HL, rr
    {
        private Register16 source;

        public ADD16(Gameboy parent, byte opcode) : base(parent) {
            source = OpcodeUtils.BitsToRegister16((opcode & 0b110000) >> 4);

            Disassembly = "add hl, " + OpcodeUtils.Register16ToString(source);
            Cycles = 8;
        }

        public override void Execute()
        {
            ushort orgVal = parent.ReadRegister16(Register16.HL);
            ushort addVal = parent.ReadRegister16(source);
            parent.WriteRegister16(Register16.HL, (ushort)(orgVal + addVal));

            // Set Flags
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, (((orgVal & 0xFFF) + (addVal & 0xFFF)) & 0x1000) == 0x1000);
            parent.SetFlag(Flag.Carry, orgVal + addVal > 0xFFFF);
        }
    }
}
