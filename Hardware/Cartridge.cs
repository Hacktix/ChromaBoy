using ChromaBoy.Hardware.MBCs;
using System;
using System.Text;

namespace ChromaBoy.Hardware
{
    public enum ColorSupportMode { NoColor, SupportsColor, ColorOnly }

    public class Cartridge
    {
        public string Title = "";
        public string ManufacturerCode = "";
        public ColorSupportMode ColorSupport = ColorSupportMode.NoColor;
        public string LicenseeCode = "00";
        public bool SupportsSGB = false;
        public byte CartridgeType = 0;
        public byte ROMBanks = 0;
        public int ExternalRAMSize = 0;
        public bool Japanese = false;
        public byte Version = 0;
        public byte Checksum = 0;
        public ushort GlobalChecksum = 0;

        public byte[] ROM;
        public MemoryBankController MemoryBankController;

        public Cartridge(byte[] romfile)
        {
            ROM = romfile;

            // CGB Support
            if (ROM[0x0143] == 0x80) ColorSupport = ColorSupportMode.SupportsColor;
            else if (ROM[0x0143] == 0xC0) ColorSupport = ColorSupportMode.ColorOnly;

            // SGB Flag
            if (ROM[0x0146] == 0x03) SupportsSGB = true;

            // ROM Size / ROM Banks Count
            if (ROM[0x0148] == 0) ROMBanks = 0;
            else if (ROM[0x0148] > 0 && ROM[0x148] < 9) ROMBanks = (byte)Math.Pow(2, ROM[0x0148]);
            else
            {
                switch(ROM[0x0148])
                {
                    case 0x52: ROMBanks = 72; break;
                    case 0x53: ROMBanks = 80; break;
                    case 0x54: ROMBanks = 96; break;
                }
            }

            // External RAM Size
            switch(ROM[0x0149])
            {
                case 0x01: ExternalRAMSize = 2048; break;
                case 0x00:
                case 0x02: ExternalRAMSize = 8192; break;
                case 0x03: ExternalRAMSize = 32768; break;
                case 0x04: ExternalRAMSize = 131072; break;
                case 0x05: ExternalRAMSize = 65536; break;
            }

            // Destination Code
            if (ROM[0x14A] == 0) Japanese = true;

            // Licensee Code
            if (ROM[0x014B] == 0x33) LicenseeCode = Encoding.ASCII.GetString(new byte[] { ROM[0x0144], ROM[0x0145] });
            else LicenseeCode = ROM[0x014B].ToString("X2");

            // Version Number & Checksums
            Version = ROM[0x014C];
            Checksum = ROM[0x014D];
            GlobalChecksum = (ushort)((ROM[0x014E] << 8) + ROM[0x014F]);

            // Cartridge Type + MBC
            CartridgeType = ROM[0x0147];
            MemoryBankController = InitializeMBC();

            // Title + Manufacturer Code
            for(int i = 0x0134; i <= 0x0143; i++)
            {
                if (i == 0x0143 && ColorSupport != ColorSupportMode.NoColor) break;
                string c = Encoding.ASCII.GetString(new byte[] { ROM[i] });
                if (i >= 0x013F) ManufacturerCode += c;
                Title += c;
            }
        }

        private MemoryBankController InitializeMBC()
        {
            switch(CartridgeType)
            {
                case 0: return new NoMBC(ROM.Length);
                case 1: return new MBC1(0, ROM.Length, false) ;
                case 2: return new MBC1(ExternalRAMSize, ROM.Length, false);
                case 3: return new MBC1(ExternalRAMSize, ROM.Length, true);
                default: throw new NotImplementedException();
            }
        }
    }
}
