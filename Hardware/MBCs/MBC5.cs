using System;
using System.IO;

namespace ChromaBoy.Hardware.MBCs
{
    class MBC5 : MemoryBankController
    {
        public bool HasBattery = false;

        private int ROMBankNumber = 1;
        private int RAMBankNumber = 0;

        private byte[] RAM;
        private bool hasRAM = false;
        private bool enabledRAM = false;

        private bool HasRumble = false;
        public bool RumbleActive = false;

        public MBC5(int RAMSize, int ROMSize, bool Battery, bool Rumble) : base(RAMSize, ROMSize, 0) {
            HasBattery = Battery;
            RAM = new byte[RAMSize];
            hasRAM = RAM.Length > 0;
            if (HasBattery) LoadSave();
            HasRumble = Rumble;
        }

        protected override void LoadSave()
        {
            FileStream saveFile = OpenSave();
            if (saveFile == null) return;
            saveFile.Read(RAM);
            saveFile.Close();
        }

        public override void SaveExternalRam()
        {
            if(HasBattery)
            {
                FileStream savefile = CreateSave();
                savefile.Write(RAM);
                savefile.Close();
            }
        }

        public override bool AccessesROM(int address)
        {
            return address >= 0 && address < 0x8000;
        }

        public override bool HandleRead(int address)
        {
            return address >= 0xA000 && address <= 0xBFFF;
        }

        public override byte MBCRead(int address)
        {
            if (!enabledRAM || !hasRAM) return 0xFF;
            address -= 0xA000;
            address += 0x2000 * RAMBankNumber;
            return RAM[address % RAM.Length];
        }

        public override bool HandleWrite(int address, byte value)
        {
            if(address >= 0x0000 && address <= 0x1FFF)
            {
                enabledRAM = (value & 0xF) == 0xA;
                return false;
            }
            else if(address >= 0x2000 && address <= 0x2FFF)
            {
                ROMBankNumber = (ROMBankNumber & 0x100) | value;
                return false;
            } else if (address >= 0x3000 && address <= 0x3FFF) {
                ROMBankNumber = (ROMBankNumber & 0xFF) | (value & 1);
                return false;
            } else if(address >= 0x4000 && address <= 0x5FFF) {
                if (HasRumble)
                {
                    RAMBankNumber = value & 0b11110111;
                    RumbleActive = (value & 0b1000) != 0;
                    if (RumbleActive) Emulator.VibrateControllers();
                }
                else RAMBankNumber = value;
            } else if(address >= 0xA000 && address <= 0xBFFF) {
                if (enabledRAM && hasRAM) RAM[((address - 0xA000) + (0x2000 * RAMBankNumber)) % RAM.Length] = value;
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
                if (address < 0x4000) return address;
                return ((ROMBankNumber * 0x4000) % ROMSize) + (address - 0x4000);
            }
            if (address >= 0xE000 && address <= 0xFDFF)
                return address - 0x2000;
            return address;
        }
    }
}
