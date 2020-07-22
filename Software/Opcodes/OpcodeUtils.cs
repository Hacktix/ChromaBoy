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

        public static string RegisterToString(Register reg)
        {
            switch(reg)
            {
                case Register.A: return "a";
                case Register.B: return "b";
                case Register.C: return "c";
                case Register.D: return "d";
                case Register.E: return "e";
                case Register.H: return "h";
                case Register.L: return "l";
                default: return "Unknown register";
            }
        }

        public static string Register16ToString(Register16 reg)
        {
            switch(reg)
            {
                case Register16.AF: return "af";
                case Register16.BC: return "bc";
                case Register16.DE: return "de";
                case Register16.HL: return "hl";
                case Register16.PC: return "pc";
                case Register16.SP: return "sp";
                default: return "Unknown register pair";
            }
        }

        public static string FlagToString(Flag flag)
        {
            switch(flag)
            {
                case Flag.AddSub: return "n";
                case Flag.Carry: return "c";
                case Flag.HalfCarry: return "hc";
                case Flag.Zero: return "z";
                default: return "Unknown flag";
            }
        }
    }
}
