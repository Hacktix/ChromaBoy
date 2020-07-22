using ChromaBoy.Hardware;
using System;

namespace ChromaBoy.Software.Opcodes
{
    public class ADDSP : Opcode // ADD SP, e
    {
        public ADDSP(Gameboy parent) : base(parent) {
            Cycles = 16;
            Length = 2;
            Disassembly = "ADD SP, $" + parent.Memory[parent.PC + 1].ToString("X2");
        }

        public override void Execute()
        {
            ushort orgVal = parent.ReadRegister16(Register16.SP);
            sbyte addByte = (sbyte)parent.Memory[parent.PC + 1];
            ushort res = (ushort)(orgVal + addByte);

            parent.WriteRegister16(Register16.SP, res);

            // Set Flags
            parent.SetFlag(Flag.Zero, false);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, (res & 0xF) < (orgVal & 0xF));
            parent.SetFlag(Flag.Carry, (res & 0xFF) < (orgVal & 0xFF));
        }
    }
}
