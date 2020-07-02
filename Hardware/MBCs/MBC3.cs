using System;
using System.Collections.Generic;
using System.IO;

namespace ChromaBoy.Hardware.MBCs
{
    public enum RTCRegister { RTC_S = 0x08, RTC_M = 0x09, RTC_H = 0x0A, RTC_DL = 0x0B, RTC_DH = 0x0C }

    class MBC3 : MemoryBankController
    {
        public bool HasBattery = false;
        public bool HasRTC = false;

        private byte romBank = 1;
        private byte ramBank = 0;
        private bool ramEnabled = false;
        private byte lastLatchValue = 0;

        private Dictionary<RTCRegister, byte> rtcRegs = new Dictionary<RTCRegister, byte>() { { RTCRegister.RTC_S, 0 }, { RTCRegister.RTC_M, 0 }, { RTCRegister.RTC_H, 0 }, { RTCRegister.RTC_DL, 0 }, { RTCRegister.RTC_DH, 0 } };
        private Dictionary<RTCRegister, byte> rtcRegsLatched = new Dictionary<RTCRegister, byte>() { { RTCRegister.RTC_S, 0 }, { RTCRegister.RTC_M, 0 }, { RTCRegister.RTC_H, 0 }, { RTCRegister.RTC_DL, 0 }, { RTCRegister.RTC_DH, 0 } };
        private long lastUpdated = -1;
        private long lastStopped = -1;

        private byte[] RAM;
        private bool enabledRAM = false;

        public MBC3(int RAMSize, int ROMSize, bool Battery, bool Timer) : base(RAMSize, ROMSize, 0) {
            HasBattery = Battery;
            HasRTC = Timer;
            RAM = new byte[RAMSize];
            if (HasBattery) LoadSave();
            if (HasRTC) UpdateClockRegisters();
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
            return address <= 0x7FFF;
        }

        public override bool HandleRead(int address)
        {
            return address >= 0xA000 && address <= 0xBFFF;
        }

        public override byte MBCRead(int address)
        {
            if(HasRTC && ramBank >= 0x08 && ramBank <= 0x0C)
            {
                switch (ramBank)
                {
                    case 0x08: return rtcRegsLatched[RTCRegister.RTC_S];
                    case 0x09: return rtcRegsLatched[RTCRegister.RTC_M];
                    case 0x0A: return rtcRegsLatched[RTCRegister.RTC_H];
                    case 0x0B: return rtcRegsLatched[RTCRegister.RTC_DL];
                    case 0x0C: return rtcRegsLatched[RTCRegister.RTC_DH];
                }
            }
            if(!enabledRAM)
                return 0xFF;
            return RAM[((address - 0xA000) + ramBank * 0x4000) % RAM.Length];
        }

        private void UpdateClockRegisters()
        {
            DateTime data = DateTime.Now;

            if(lastUpdated != -1)
            {
                long ticks = data.Ticks - lastUpdated;
                long seconds = (ticks / TimeSpan.TicksPerSecond) + rtcRegs[RTCRegister.RTC_S];
                long minutes = (seconds / 60) + rtcRegs[RTCRegister.RTC_M];
                seconds %= 60;
                long hours = (minutes / 60) + rtcRegs[RTCRegister.RTC_H];
                minutes %= 60;
                long days = (hours / 24) + rtcRegs[RTCRegister.RTC_DL] + ((rtcRegs[RTCRegister.RTC_DH] & 1) << 8);
                hours %= 24;

                bool dayCarry = (rtcRegs[RTCRegister.RTC_DH] & 0b10000000) != 0;
                if(days > 0x1FF)
                {
                    days = 0;
                    dayCarry = true;
                }

                rtcRegs[RTCRegister.RTC_S] = (byte)seconds;
                rtcRegs[RTCRegister.RTC_M] = (byte)minutes;
                rtcRegs[RTCRegister.RTC_H] = (byte)hours;
                rtcRegs[RTCRegister.RTC_DL] = (byte)(days & 0xFF);
                rtcRegs[RTCRegister.RTC_DH] = (byte)((byte)((days & 0x100) >> 8) | (byte)(rtcRegs[RTCRegister.RTC_DH] & 0b1000000) | (byte)(dayCarry ? 0b10000000 : 0));
            }

            lastUpdated = data.Ticks;
        }

        private void LatchClockData()
        {
            UpdateClockRegisters();
            foreach(KeyValuePair<RTCRegister,byte> reg in rtcRegs)
                rtcRegsLatched[reg.Key] = reg.Value;
        }

        public override bool HandleWrite(int address, byte value)
        {
            if(address <= 0x1FFF)
            {
                ramEnabled = value == 0x0A;
                return false;
            }
            else if(address <= 0x3FFF)
            {
                romBank = (byte)(value & 0x7F);
                if (romBank == 0) romBank++;
                return false;
            }
            else if(address <= 0x5FFF)
            {
                ramBank = value;
                return false;
            }
            else if(address <= 0x7FFF)
            {
                if(lastLatchValue == 0 && value == 1)
                    LatchClockData();
                lastLatchValue = value;
                return false;
            }
            else if(address >= 0xA000 && address <= 0xBFFF)
            {
                if(HasRTC && ramBank >= 0x08 && ramBank <= 0x0C)
                {
                    switch (ramBank)
                    {
                        case 0x08: rtcRegs[RTCRegister.RTC_S] = value; return false;
                        case 0x09: rtcRegs[RTCRegister.RTC_M] = value; return false;
                        case 0x0A: rtcRegs[RTCRegister.RTC_H] = value; return false;
                        case 0x0B: rtcRegs[RTCRegister.RTC_DL] = value; return false;
                        case 0x0C: rtcRegs[RTCRegister.RTC_DH] = value; return false;
                    }
                }
                if(RAM.Length > 0 && ramBank <= 3)
                    RAM[((address - 0xA000) + ramBank * 0x4000) % RAM.Length] = value;
            }
            return true;
        }

        public override bool IsAddressReadable(int address)
        {
            return true;
        }

        public override bool IsAddressWritable(int address)
        {
            return true;
        }

        public override int TranslateAddress(int address)
        {
            if (address >= 0x4000 && address <= 0x7FFF)
                return ((romBank - 1) * 0x4000 + address) % ROMSize;
            if (address >= 0xE000 && address <= 0xFDFF)
                return address - 0x2000;
            return address;
        }
    }
}
