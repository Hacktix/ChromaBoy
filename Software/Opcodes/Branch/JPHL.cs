﻿using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class JPHL : Opcode // JP HL
    {
        public JPHL(Gameboy parent) : base(parent) {
            Disassembly = "jp hl";
        }

        public override void Execute()
        {
            parent.PC = (ushort)(parent.ReadRegister16(Register16.HL) - 1);
        }
    }
}
