using System;
using System.Text;

namespace ChromaBoy.Hardware
{
    public class Memory
    {
        private MemoryBankController MBC;
        public byte[] ROM;
        public byte[] RAM;
        private Gameboy parent;

        public bool UpdatedSTAT = false;

        public Memory(MemoryBankController MBC, byte[] ROM, Gameboy parent)
        {
            this.MBC = MBC;
            this.ROM = ROM;
            this.RAM = new byte[0x10000];
            this.parent = parent;
        }

        public bool DMATransfer = false;
        public ushort DMAAddr = 0;

        public void Set(int addr, byte value)
        {
            if (MBC.AccessesROM(addr))
                ROM[MBC.TranslateAddress(addr)] = value;
            else
                RAM[MBC.TranslateAddress(addr)] = value;
        }

        public byte Get(int addr)
        {
            if (MBC.AccessesROM(addr))
                return ROM[MBC.TranslateAddress(addr)];
            else
                return RAM[MBC.TranslateAddress(addr)];
        }

        public byte this[int i]
        {
            get
            {
                // VRAM & OAM Lock
                if (i >= 0x8000 && i <= 0x9FFF && (RAM[0xFF41] & 3) == 3) return 0xFF;
                if (i >= 0xFE00 && i <= 0xFE9F && (RAM[0xFF41] & 3) > 1) return 0xFF;
                if (i == 0xFF00) return 0xFF;

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
                // VRAM & OAM Lock
                if (i >= 0x8000 && i <= 0x9FFF && (RAM[0xFF41] & 3) == 3) return;
                if (i >= 0xFE00 && i <= 0xFE9F && (RAM[0xFF41] & 3) > 1) return;

                switch (i)
                {
                    case 0xFF04:
                        RAM[0xFF04] = 0;
                        RAM[0xFF05] = RAM[0xFF06];
                        parent.DivRegister = 0;
                        break;
                    case 0xFF05:
                        if (parent.TimerReloadCooldown == -1)  RAM[0xFF05] = value;
                        break;
                    case 0xFF41:
                        RAM[0xFF41] = (byte)((RAM[0xFF41] & 0b111) | (value & 0b1111000));
                        UpdatedSTAT = true;
                        break;
                    case 0xFF46:
                        DMATransfer = true;
                        DMAAddr = (ushort)(100 * value);
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
