using ChromaBoy.Software;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Decoder = ChromaBoy.Software.Decoder;

namespace ChromaBoy.Hardware
{
    public enum Register { A, F, B, C, D, E, H, L, M }
    public enum Register16 { AF, BC, DE, HL, SP, PC }
    public enum Flag { Zero = 0b10000000, AddSub = 0b1000000, HalfCarry = 0b100000, Carry = 0b10000 }

    public class Gameboy
    {
        public long EndTime = 0;
        private Stopwatch PerformanceTimer;
        private Stopwatch CycleTimer;

        public Memory Memory;
        public Cartridge Cartridge;
        public PPU PPU;
        public APU APU;

        public Dictionary<Register, byte> Registers = new Dictionary<Register, byte>()
        { { Register.A, 0x01 }, { Register.F, 0xB0 }, { Register.B, 0x00 }, { Register.C, 0x13 }, { Register.D, 0x00 }, { Register.E, 0xD8 }, { Register.H, 0x01 }, { Register.L, 0x4D } };
        public ushort PC = 0x100;
        public ushort SP = 0;

        public bool Halted = false;
        public bool Standby = false;
        public bool InterruptsEnabled = false;
        public bool EINextInstruction = false;
        public long CycleCount = 0;

        public bool CallInterruptHandler = true;
        public bool HaltBug = false;

        public ushort DivRegister = 0xABCC;
        private bool LastTimerAndResult = false;
        private bool TimerAndResult = false;
        public sbyte TimerReloadCooldown = -1;

        private int CycleCooldown = 0;

        private Opcode ExecOpcode;

        public Gameboy(byte[] ROM)
        {
            byte[] bootrom = new byte[0];
            if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "boot.bin"))) bootrom = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "boot.bin"));
            if (bootrom.Length != 0 && bootrom.Length != 256) bootrom = new byte[0];

            Cartridge = new Cartridge(ROM);
            if (bootrom.Length == 0) Memory = new Memory(Cartridge.MemoryBankController, ROM, this);
            else
            {
                Memory = new Memory(Cartridge.MemoryBankController, ROM, this, bootrom);
                PC = 0;
            }
            PPU = new PPU(this);
            APU = new APU(this);

            PerformanceTimer = new Stopwatch();
            CycleTimer = new Stopwatch();
        }

        public void EmulateCycles(long cycleLimit)
        {
            long cycleCounter = cycleLimit;
            PerformanceTimer.Restart();

            while (cycleCounter-- > 0)
            {
                CycleTimer.Start();
                CycleCount++;
                HandleTimers();
                PPU.ProcessCycle();
                APU.ProcessCycle();

                if (CycleCooldown > 0)
                {
                    if (ExecOpcode != null) ExecOpcode.ExecuteTick();
                    CycleCooldown--;
                    if (CycleCooldown == 0 && ExecOpcode != null)
                    {
                        if (!HaltBug) PC += (ushort)ExecOpcode.Length;
                        else HaltBug = false;
                        ExecOpcode = null;
                    }
                    WaitForCycleFinish(CycleTimer);
                    continue;
                }

                if (CheckForInterrupt())
                {
                    HandleInterrupt();
                    WaitForCycleFinish(CycleTimer);
                    continue;
                }

                if (Halted)
                {
                    if(CycleCooldown == 0) CycleCooldown += 4;
                    if (CycleCooldown > 0)
                    {
                        WaitForCycleFinish(CycleTimer);
                        continue;
                    }
                }

                if (EINextInstruction)
                {
                    EINextInstruction = false;
                    InterruptsEnabled = true;
                }

                Opcode opcode = Decoder.DecodeOpcode(this, Memory[PC]);
                if (opcode.TickAccurate) ExecOpcode = opcode;
                else ExecOpcode = null;

                if (!opcode.TickAccurate)
                {
                    opcode.Execute();
                    if (!HaltBug) PC += (ushort)opcode.Length;
                    else HaltBug = false;
                }
                else ExecOpcode.ExecuteTick();

                CycleCooldown = opcode.Cycles - 1;

                WaitForCycleFinish(CycleTimer);
            }

            EndTime = PerformanceTimer.ElapsedTicks;
        }

        private void WaitForCycleFinish(Stopwatch timer)
        {
            while (timer.ElapsedTicks < (1.0 / (4194304 * 1.5)) * TimeSpan.TicksPerSecond) { /* Wait... */ }
            timer.Reset();
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
            DivRegister++;
            Memory.Set(0xFF04, (byte)((DivRegister & 0xFF00) >> 8));

            int shiftCnt = (Memory[0xFF07] & 0b11) == 0 ? 6 : (Memory[0xFF07] & 0b11) == 1 ? 0 : (Memory[0xFF07] & 0b11) == 2 ? 2 : 4;
            ushort dbmp = (ushort)(0b1000 << shiftCnt);

            TimerAndResult = ((DivRegister & dbmp) & ((Memory[0xFF07] & 0b100) << (shiftCnt+1))) > 0;
            if(TimerAndResult != LastTimerAndResult)
            {
                LastTimerAndResult = TimerAndResult;
                if(!TimerAndResult)
                {
                    if (Memory[0xFF05] == 0xFF)
                    {
                        Memory[0xFF05] = 0;
                        TimerReloadCooldown = 4;
                        Memory[0xFF0F] |= 0b100;
                    }
                    else Memory[0xFF05]++;
                }
            }

            if (TimerReloadCooldown > -1 && --TimerReloadCooldown == -1) Memory[0xFF05] = Memory[0xFF06];
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
