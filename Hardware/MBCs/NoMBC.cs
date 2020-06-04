namespace ChromaBoy.Hardware.MBCs
{
    class NoMBC : MemoryBankController
    {
        public NoMBC(int ROMSize) : base(0, ROMSize, 0) { }

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
            if (address >= 0xE000 && address <= 0xFDFF)
                return address - 0x2000;
            return address;
        }
    }
}
