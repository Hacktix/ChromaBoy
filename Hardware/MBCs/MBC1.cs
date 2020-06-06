using ChromaBoy.Software.Opcodes;
using System;

namespace ChromaBoy.Hardware.MBCs
{
    class MBC1 : MemoryBankController
    {
        public bool HasBattery = false;

        private byte ROMBankNumber = 1;
        private byte RAMBankNumber = 0;
        private byte Mode = 0;
        private byte HiBank = 0;

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
                ROMBankNumber = (byte)(value & 0x1F);
                if ((ROMBankNumber & 0b11111) == 0) ROMBankNumber++;
                return false;
            } else if (address >= 0x4000 && address <= 0x5FFF) {
                HiBank = (byte)(value & 0b11);
                return false;
            }
            else if (address >= 0x6000 && address <= 0x7FFF) {
                Mode = (byte)(value & 1);
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
            if (address <= 0x7FFF)
            {
                if (address < 0x4000 && Mode == 0) return address;
                else if (address < 0x4000) return (((HiBank << 5) * 0x4000) % ROMSize) + address;
                return (((ROMBankNumber | (HiBank << 5)) * 0x4000) % ROMSize) + (address - 0x4000);
            }
            if (address >= 0xE000 && address <= 0xFDFF)
                return address - 0x2000;
            return address;
        }
    }
}
