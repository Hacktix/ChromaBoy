﻿using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class STOP : Opcode // STOP
    {
        public STOP(Gameboy parent) : base(parent) {
            Length = 2;
            Disassembly = "stop";
        }

        public override void Execute()
        {
            parent.Standby = true;
        }
    }
}
