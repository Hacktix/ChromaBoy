using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDHLSP : Opcode // LD HL, SP+e
    {
        public LDHLSP(Gameboy parent) : base(parent) {
            Cycles = 12;
            Length = 2;
        }

        public override void Execute()
        {
            ushort orgVal = parent.ReadRegister16(Register16.SP);
            byte addByte = parent.Memory[parent.PC + 1];
            int addVal;

            if ((addByte & 128) > 0) addVal = ~(addByte & 0x7F);
            else addVal = addByte & 0x7F;
            parent.WriteRegister16(Register16.HL, (ushort)(orgVal + addVal));

            // Set Flags
            parent.SetFlag(Flag.Zero, false);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, (((orgVal & 0xF) + (addVal & 0xF)) & 0x10) == 0x10);
            parent.SetFlag(Flag.Carry, (orgVal & 0xFF) + addVal > 0xFF);
        }
    }
}
