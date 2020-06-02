using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class LDAHL : Opcode // LD A, (HL-) | LD (HL-), A | LD A, (HL+) | LD (HL+), A
    {
        private bool inc;
        private bool load;

        public LDAHL(Gameboy parent, byte opcode) : base(parent) {
            inc = (opcode & 0b10000) == 0;
            load = (opcode & 0b1000) > 0;

            Cycles = 8;
        }

        public override void Execute()
        {
            byte srcVal = !load ? parent.Registers[Register.A] : parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])];
            if (load)
                parent.Registers[Register.A] = srcVal;
            else
                parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = srcVal;
            parent.WriteRegister16(Register16.HL, (ushort)(parent.ReadRegister16(Register16.HL) + (inc ? 1 : -1)));
        }
    }
}
