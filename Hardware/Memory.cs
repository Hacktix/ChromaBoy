namespace ChromaBoy.Hardware
{
    class Memory
    {
        private MemoryBankController MBC;
        private byte[] ROM;
        private byte[] RAM;

        public Memory(MemoryBankController MBC, byte[] ROM)
        {
            this.MBC = MBC;
            this.ROM = ROM;
            this.RAM = new byte[0x10000];
        }

        public byte this[int i]
        {
            get
            {
                if (MBC.IsAddressReadable(i))
                {
                    if (MBC.AccessesROM(i))
                        return ROM[MBC.TranslateAddress(i)];
                    else
                        return RAM[MBC.TranslateAddress(i)];
                }
                return 0;
            }
            set
            {
                if(MBC.IsAddressWritable(i))
                {
                    if(MBC.HandleWrite(i, value))
                    {
                        if (MBC.AccessesROM(i))
                            ROM[MBC.TranslateAddress(i)] = value;
                        else
                            RAM[MBC.TranslateAddress(i)] = value;
                    }
                }
            }
        }
    }
}
