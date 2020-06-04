using System;
using System.Text;

namespace ChromaBoy.Hardware
{
    public class Memory
    {
        private MemoryBankController MBC;
        public byte[] ROM;
        public byte[] RAM;

        public Memory(MemoryBankController MBC, byte[] ROM)
        {
            this.MBC = MBC;
            this.ROM = ROM;
            this.RAM = new byte[0x10000];
        }

        public void Set(int addr, byte value)
        {
            if (MBC.AccessesROM(addr))
                ROM[MBC.TranslateAddress(addr)] = value;
            else
                RAM[MBC.TranslateAddress(addr)] = value;
        }

        public byte this[int i]
        {
            get
            {
                if (MBC.HandleRead(i)) return MBC.MBCRead(i);
                if (MBC.IsAddressReadable(i))
                {
                    if (MBC.AccessesROM(i))
                        return ROM[MBC.TranslateAddress(i)];                        
                    else
                        return RAM[MBC.TranslateAddress(i)];
                }
                return 0xFF;
            }
            set
            {
                switch(i)
                {
                    case 0xFF01:
                        Console.Write(Encoding.ASCII.GetString(new byte[] { value }));
                        break;
                    case 0xFF04:
                        RAM[0xFF04] = 0;
                        break;
                    default:
                        if (MBC.HandleWrite(i, value))
                        {
                            if (MBC.IsAddressWritable(i))
                            {
                                if (MBC.AccessesROM(i))
                                    ROM[MBC.TranslateAddress(i)] = value;
                                else
                                    RAM[MBC.TranslateAddress(i)] = value;
                            }
                        }
                        break;
                }
            }
        }
    }
}
