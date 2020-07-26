using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDMSP : Opcode // LD (nn), SP
    {
        private ushort writeAddr;

        public LDMSP(Gameboy parent) : base(parent) {
            Cycles = 20;
            Length = 3;
            TickAccurate = true;

            Disassembly = "ld [$" + (parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)).ToString("X4") + "]";
        }

        public override void Execute()
        {
            ushort addr = (ushort)(parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8));
            parent.Memory[addr] = (byte)(parent.SP & 0xFF);
            parent.Memory[addr + 1] = (byte)(parent.SP >> 8);
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if (Tick == 3) writeAddr = parent.Memory[parent.PC + 1];
            else if (Tick == 7) writeAddr = (ushort)(writeAddr + (parent.Memory[parent.PC + 2] << 8));
            else if (Tick == 11) parent.Memory[writeAddr] = (byte)(parent.SP & 0xFF);
            else if (Tick == 15) parent.Memory[writeAddr + 1] = (byte)((parent.SP & 0xFF00) >> 8);
        }
    }
}
