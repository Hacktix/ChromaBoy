﻿using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class DI : Opcode // DI
    {
        public DI(Gameboy parent) : base(parent) {
            Disassembly = "di";
        }

        public override void Execute()
        {
            parent.InterruptsEnabled = false;
        }
    }
}
