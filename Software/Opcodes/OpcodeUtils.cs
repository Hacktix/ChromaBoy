using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public static class OpcodeUtils
    {
        public static Register BitsToRegister(int bits)
        {
            switch (bits)
            {
                case 0b000: return Register.B;
                case 0b001: return Register.C;
                case 0b010: return Register.D;
                case 0b011: return Register.E;
                case 0b100: return Register.H;
                case 0b101: return Register.L;
                case 0b110: return Register.M;
                case 0b111: return Register.A;
                default: return Register.A;
            }
        }

        public static Register16 BitsToRegister16(int bits)
        {
            switch (bits)
            {
                case 0b00: return Register16.BC;
                case 0b01: return Register16.DE;
                case 0b10: return Register16.HL;
                case 0b11: return Register16.SP;
                default: return Register16.SP;
            }
        }
    }
}
