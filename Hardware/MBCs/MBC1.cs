namespace ChromaBoy.Hardware.MBCs
{
    class MBC1 : MemoryBankController
    {
        public bool HasBattery = false;

        private byte ROMBankNumber = 1;
        private byte Mode = 0;

        // TODO: Implement RAM-Related Features

        public MBC1(int RAMSize, int ROMSize, bool Battery) : base(RAMSize, ROMSize, 0) {
            HasBattery = Battery;
        }

        public override bool AccessesROM(int address)
        {
            return address >= 0 && address < 0x8000;
        }

        public override bool HandleRead(int address)
        {
            return false;
        }

        public override bool HandleWrite(int address, byte value)
        {
            if(address >= 0x2000 && address <= 0x3FFF)
            {
                ROMBankNumber = (byte)((ROMBankNumber & 0x60) | (value & 0x1F));
                if (ROMBankNumber == 0) ROMBankNumber++;
                return false;
            }

            return true;
        }

        public override bool IsAddressReadable(int address)
        {
            return true;
        }

        public override bool IsAddressWritable(int address)
        {
            return address >= 0x8000;
        }

        public override int TranslateAddress(int address)
        {
            if (address >= 0x4000 && address <= 0x7FFF)
                return ((ROMBankNumber * 0x4000) % ROMSize) + (address - 0x4000);
            if (address >= 0xE000 && address <= 0xFDFF)
                return address - 0x2000;
            return address;
        }
    }
}
