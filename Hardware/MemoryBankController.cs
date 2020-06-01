namespace ChromaBoy.Hardware
{
    public abstract class MemoryBankController
    {
        public int RAMSize;
        public int MemoryBanks;

        public MemoryBankController(int RAMSize, int MemoryBanks)
        {
            this.RAMSize = RAMSize;
            this.MemoryBanks = MemoryBanks;
        }

        public abstract bool IsAddressWritable(int address);
        public abstract bool IsAddressReadable(int address);
        public abstract bool AccessesROM(int address);
        public abstract bool HandleWrite(int address, byte value);
        public abstract int TranslateAddress(int address);
    }
}
