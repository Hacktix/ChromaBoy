using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDHLSP : Opcode // LD HL, SP+e
    {
        public LDHLSP(Gameboy parent) : base(parent) {
            Cycles = 12;
            Length = 2;
            TickAccurate = true;

            Disassembly = "ld hl, " + ((ushort)(parent.ReadRegister16(Register16.SP) + (sbyte)parent.Memory[parent.PC + 1])).ToString("X1");
        }

        public override void Execute()
        {
            ushort orgVal = parent.ReadRegister16(Register16.SP);
            sbyte addByte = (sbyte)parent.Memory[parent.PC + 1];
            ushort res = (ushort)(orgVal + addByte);

            parent.WriteRegister16(Register16.HL, (ushort)(orgVal + addByte));

            // Set Flags
            parent.SetFlag(Flag.Zero, false);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, (res & 0xF) < (orgVal & 0xF));
            parent.SetFlag(Flag.Carry, (res & 0xFF) < (orgVal & 0xFF));
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if (Tick == 7) Execute();
        }
    }
}
