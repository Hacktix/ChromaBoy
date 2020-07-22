using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDMSP : Opcode // LD (nn), SP
    {
        public LDMSP(Gameboy parent) : base(parent) {
            Cycles = 20;
            Length = 3;

            Disassembly = "ld [$" + (parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8)).ToString("X4") + "]";
        }

        public override void Execute()
        {
            ushort addr = (ushort)(parent.Memory[parent.PC + 1] + (parent.Memory[parent.PC + 2] << 8));
            parent.Memory[addr] = (byte)(parent.SP & 0xFF);
            parent.Memory[addr + 1] = (byte)(parent.SP >> 8);
        }
    }
}
