using System;
using System.Runtime;

namespace ChromaBoy.Hardware
{
    public class Memory
    {
        public MemoryBankController MBC;
        public byte[] ROM;
        public byte[] RAM;
        public byte[] BOOTROM;
        private Gameboy parent;

        public bool UpdatedSTAT = false;
        public bool LockVRAM = false;
        public bool LockOAM = false;
        public bool LockDMA = false;

        public bool UpdateAudioChannel1 = false;
        public bool UpdateAudioChannel2 = false;
        public bool UpdateAudioLength1 = false;
        public bool UpdateAudioLength2 = false;
        public bool TriggerAudio1 = false;
        public bool TriggerAudio2 = false;

        public Memory(MemoryBankController MBC, byte[] ROM, Gameboy parent, byte[] bootrom = null)
        {
            this.MBC = MBC;
            this.ROM = ROM;
            this.RAM = new byte[0x10000];
            this.BOOTROM = bootrom;
            this.parent = parent;

            RAM[0xFF00] = 0xCF;
        }

        public bool DMATransfer = false;
        public ushort DMAAddr = 0;
        public byte LastDMAValue = 0xFF;

        public void Set(int i, byte value, bool accessOverride = false)
        {
            if (MBC.HandleWrite(i, value))
            {
                if (MBC.IsAddressWritable(i) || accessOverride)
                {
                    if (MBC.AccessesROM(i))
                        ROM[MBC.TranslateAddress(i)] = value;
                    else
                        RAM[MBC.TranslateAddress(i)] = value;
                }
            }
        }

        public byte Get(int i, bool accessOverride = false)
        {
            if (MBC.HandleRead(i)) return MBC.MBCRead(i);
            if (MBC.IsAddressReadable(i) || accessOverride)
            {
                if (MBC.AccessesROM(i))
                    return ROM[MBC.TranslateAddress(i)];
                else
                    return RAM[MBC.TranslateAddress(i)];
            }
            return 0xFF;
        }

        public byte this[int i]
        {
            get
            {
                // VRAM & OAM Lock
                if (i >= 0x8000 && i <= 0x9FFF && LockVRAM) return 0xFF;
                if (i >= 0xFE00 && i <= 0xFE9F && LockOAM) return 0xFF;

                // OAM DMA Lock
                // if (LockDMA && (i < 0xFF80 || i > 0xFFFE)) return LastDMAValue;

                // Bootrom Translation
                if (i <= 0xFF && BOOTROM != null && RAM[0xFF50] == 0) return BOOTROM[i];

                // Inputs
                if (i == 0xFF00)
                {
                    if ((RAM[0xFF00] & 0b10000) == 0) return (byte)((RAM[0xFF00] & 0xF0) | (Emulator.InputBits & 0x0F));
                    else if ((RAM[0xFF00] & 0b100000) == 0) return (byte)((RAM[0xFF00] & 0xF0) | ((Emulator.InputBits & 0xF0) >> 4));
                }

                switch (i)
                {
                    case 0xFF02: return (byte)(RAM[0xFF02] | 0b1111110);
                    case 0xFF03: return 0xFF;
                    case 0xFF04: return (byte)((parent.DivRegister & 0xFF00) >> 8);
                    case 0xFF07: return (byte)(RAM[0xFF07] | 0b11111000);
                    case 0xFF08:
                    case 0xFF09:
                    case 0xFF0A:
                    case 0xFF0B:
                    case 0xFF0C:
                    case 0xFF0D:
                    case 0xFF0E: return 0xFF;
                    case 0xFF0F: return (byte)(RAM[0xFF0F] | 0b11100000);
                    case 0xFF10: return (byte)(RAM[0xFF10] | 0b10000000);
                    case 0xFF15: return 0xFF;
                    case 0xFF1A: return (byte)(RAM[0xFF1A] | 0b01111111);
                    case 0xFF1C: return (byte)(RAM[0xFF1C] | 0b10011111);
                    case 0xFF1F: return 0xFF;
                    case 0xFF20: return (byte)(RAM[0xFF20] | 0b11000000);
                    case 0xFF23: return (byte)(RAM[0xFF23] | 0b00111111);
                    case 0xFF26: return (byte)(RAM[0xFF26] | 0b01110000);
                    case 0xFF27:
                    case 0xFF28:
                    case 0xFF29: return 0xFF;
                    case 0xFF41: return (byte)(RAM[0xFF41] | 0b10000000);
                }

                if (i >= 0xFF4C && i <= 0xFF7F)
                    return 0xFF;

                return Get(i);
            }
            set
            {
                // VRAM & OAM Lock
                if (i >= 0x8000 && i <= 0x9FFF && LockVRAM) return;
                if (i >= 0xFE00 && i <= 0xFE9F && LockOAM) return;

                // OAM DMA Lock
                // if (LockDMA && (i < 0xFF80 || i > 0xFFFE)) return;

                // Audio Channel Updates
                if (i >= 0xFF10 && i <= 0xFF14) UpdateAudioChannel1 = true;
                else if (i >= 0xFF16 && i <= 0xFF19) UpdateAudioChannel2 = true;

                if (i == 0xFF11 && (value & 0x3F) != (RAM[0xFF11] & 0x3F)) UpdateAudioLength1 = true;
                else if (i == 0xFF16 && (value & 0x3F) != (RAM[0xFF16] & 0x3F)) UpdateAudioLength2 = true;

                if (i == 0xFF14 && (value & 128) != 0) TriggerAudio1 = true;
                else if (i == 0xFF19 && (value & 128) != 0) TriggerAudio2 = true;

                switch (i)
                {
                    case 0xFF00:
                        RAM[0xFF00] = (byte)((RAM[0xFF00] & 0b11001111) | (value & 0b110000));
                        return;
                    case 0xFF04:
                        RAM[0xFF04] = 0;
                        RAM[0xFF05] = RAM[0xFF06];
                        parent.DivRegister = 0;
                        return;
                    case 0xFF05:
                        if (parent.TimerReloadCooldown == -1) RAM[0xFF05] = value;
                        return;
                    case 0xFF41:
                        RAM[0xFF41] = (byte)((RAM[0xFF41] & 0b111) | (value & 0b11111000));
                        UpdatedSTAT = true;
                        return;
                    case 0xFF46:
                        RAM[0xFF46] = value;
                        if (!DMATransfer)
                        {
                            DMATransfer = true;
                            DMAAddr = (ushort)(0x100 * value);
                        }
                        return;
                    case 0xFF0F:
                        RAM[0xFF0F] = (byte)(value & 0b11111);
                        return;
                }

                Set(i, value);
            }
        }
    }
}
