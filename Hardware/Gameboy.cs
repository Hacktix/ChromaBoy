﻿using ChromaBoy.Software;
using System.Collections.Generic;
using Decoder = ChromaBoy.Software.Decoder;

namespace ChromaBoy.Hardware
{
    public enum Register { A, F, B, C, D, E, H, L, M }
    public enum Register16 { AF, BC, DE, HL, SP, PC }
    public enum Flag { Zero = 0b10000000, AddSub = 0b1000000, HalfCarry = 0b100000, Carry = 0b10000 }

    public class Gameboy
    {
        public Memory Memory;
        public Cartridge Cartridge;
        public PPU PPU;

        public Dictionary<Register, byte> Registers = new Dictionary<Register, byte>()
        { { Register.A, 0 }, { Register.F, 0 }, { Register.B, 0 }, { Register.C, 0 }, { Register.D, 0 }, { Register.E, 0 }, { Register.H, 0 }, { Register.L, 0 } };
        public ushort PC = 0x100;
        public ushort SP = 0;

        public bool Halted = false;
        public bool Standby = false;
        public bool InterruptsEnabled = false;
        public bool EINextInstruction = false;
        public long CycleCount = 0;

        public bool CallInterruptHandler = true;
        public bool HaltBug = false;

        private int CycleCooldown = 0;

        public Gameboy(byte[] ROM)
        {
            Cartridge = new Cartridge(ROM);
            Memory = new Memory(Cartridge.MemoryBankController, ROM);
            PPU = new PPU(this);
        }

        public void EmulateCycles(long cycleLimit)
        {
            long cycleCounter = cycleLimit;

            while (cycleCounter-- > 0)
            {
                CycleCount++;
                HandleTimers();
                PPU.ProcessCycle();

                if (CycleCooldown > 0)
                {
                    CycleCooldown--;
                    continue;
                }

                if (CheckForInterrupt())
                {
                    HandleInterrupt();
                    continue;
                }

                if (Halted)
                {
                    CycleCooldown += 4;
                    if(CycleCooldown > 0) continue;
                }

                if (EINextInstruction)
                {
                    EINextInstruction = false;
                    InterruptsEnabled = true;
                }

                Opcode opcode = Decoder.DecodeOpcode(this, Memory[PC]);
                opcode.Execute();
                if (!HaltBug) PC += (ushort)opcode.Length;
                else HaltBug = false;
                CycleCooldown = opcode.Cycles - 1;
            }
        }

        private bool CheckForInterrupt()
        {
            if (Halted || InterruptsEnabled) return (Memory[0xFFFF] & Memory[0xFF0F]) != 0;
            return false;
        }

        private void HandleInterrupt()
        {
            byte intVec = 0;
            byte maskedInt = (byte)(Memory[0xFFFF] & Memory[0xFF0F]);
            byte bit = 1;
            while ((maskedInt & bit) == 0) bit <<= 1;
            switch (bit)
            {
                case 0b00001: intVec = 0x40; break;
                case 0b00010: intVec = 0x48; break;
                case 0b00100: intVec = 0x50; break;
                case 0b01000: intVec = 0x58; break;
                case 0b10000: intVec = 0x60; break;
            }
            InterruptsEnabled = false;
            Halted = false;
            Standby = false;

            // Call Interrupt Vector
            if (CallInterruptHandler)
            {
                Memory[0xFF0F] &= (byte)~bit;

                SP -= 2;
                Memory[SP + 1] = (byte)((PC & 0xFF00) >> 8);
                Memory[SP] = (byte)(PC & 0xFF);
                PC = intVec;
                CycleCooldown = 8;
            } else CallInterruptHandler = true;
        }

        private void HandleTimers()
        {
            // DIV Register
            if(CycleCount % 256 == 0) Memory.Set(0xFF04, (byte)(Memory[0xFF04] + 1));

            // TIMA Register
            if((Memory[0xFF07] & 0b100) > 0) // If "Timer Enable" bit is set
            {
                byte clockSetting = (byte)(Memory[0xFF07] & 0b11);
                int modValue = 0;
                switch(clockSetting)
                {
                    case 0b00: modValue = 1024; break;
                    case 0b01: modValue = 16; break;
                    case 0b10: modValue = 64; break;
                    case 0b11: modValue = 256; break;
                }

                if(CycleCount % modValue == 0)
                {
                    if(Memory[0xFF05] == 0xFF)
                    {
                        Memory[0xFF05] = Memory[0xFF06];
                        Memory[0xFF0F] |= 0b100;
                    } else Memory[0xFF05]++;
                }
            }
        }

        public void WriteRegister16(Register16 regpair, ushort value)
        {
            switch (regpair)
            {
                case Register16.BC:
                    Registers[Register.B] = (byte)((value & 0xFF00) >> 8);
                    Registers[Register.C] = (byte)(value & 0xFF);
                    break;
                case Register16.DE:
                    Registers[Register.D] = (byte)((value & 0xFF00) >> 8);
                    Registers[Register.E] = (byte)(value & 0xFF);
                    break;
                case Register16.HL:
                    Registers[Register.H] = (byte)((value & 0xFF00) >> 8);
                    Registers[Register.L] = (byte)(value & 0xFF);
                    break;
                case Register16.AF:
                    Registers[Register.A] = (byte)((value & 0xFF00) >> 8);
                    Registers[Register.F] = (byte)(value & 0xF0);
                    break;
                case Register16.SP:
                    SP = value;
                    break;
                case Register16.PC:
                    PC = value;
                    break;
            }
        }

        public ushort ReadRegister16(Register16 regpair)
        {
            switch (regpair)
            {
                case Register16.BC: return (ushort)((Registers[Register.B] << 8) + Registers[Register.C]);
                case Register16.DE: return (ushort)((Registers[Register.D] << 8) + Registers[Register.E]);
                case Register16.HL: return (ushort)((Registers[Register.H] << 8) + Registers[Register.L]);
                case Register16.AF: return (ushort)((Registers[Register.A] << 8) + Registers[Register.F]);
                case Register16.SP: return SP;
                case Register16.PC: return PC;
            }
            return 0;
        }

        public void SetFlag(Flag flag, bool set)
        {
            if (set)
                Registers[Register.F] |= (byte)flag;
            else
                Registers[Register.F] &= (byte)~flag;
        }

        public bool GetFlag(Flag flag)
        {
            return (Registers[Register.F] & ((byte)flag)) > 0;
        }
    }
}
