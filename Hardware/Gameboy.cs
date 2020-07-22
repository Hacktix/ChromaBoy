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

    public enum Model { DMG, MGB, SGB, SGB2, CGB, AGB, AGS }
    public enum Mode { DMG, CGB }

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
        { { Register.A, 0 }, { Register.F, 0 }, { Register.B, 0 }, { Register.C, 0 }, { Register.D, 0 }, { Register.E, 0 }, { Register.H, 0 }, { Register.L, 0 } };
        public ushort PC = 0x100;
        public ushort SP = 0xFFFE;

        public bool Halted = false;
        public bool Standby = false;
        public bool InterruptsEnabled = false;
        public bool EINextInstruction = false;

        public bool CallInterruptHandler = true;
        public bool HaltBug = false;

        public ushort DivRegister = 0;
        private bool LastTimerAndResult = false;
        private bool TimerAndResult = false;
        public sbyte TimerReloadCooldown = -1;

        private int CycleCooldown = 0;
        private int cycleSleepInterval = 15000;

        private Opcode ExecOpcode;

        private bool logTrace = false;
        private StreamWriter logWriter;

        public Gameboy(byte[] ROM)
        {
            byte[] bootrom = new byte[0];
            if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "boot.bin"))) bootrom = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "boot.bin"));
            if (bootrom.Length != 0 && bootrom.Length != 256) bootrom = new byte[0];

            Cartridge = new Cartridge(ROM);
            if (bootrom.Length == 0)
            {
                Memory = new Memory(Cartridge.MemoryBankController, ROM, this);
                InitRegisters();
            }
            else
            {
                Memory = new Memory(Cartridge.MemoryBankController, ROM, this, bootrom);
                PC = 0;
            }
            PPU = new PPU(this);
            APU = new APU(this);

            if (logTrace) logWriter = new StreamWriter(File.OpenWrite("log.txt"));

            PerformanceTimer = new Stopwatch();
            CycleTimer = new Stopwatch();
        }

        private void InitRegisters()
        {
            WriteRegister16(Register16.AF, 0x01B0);
            WriteRegister16(Register16.BC, 0x0013);
            WriteRegister16(Register16.DE, 0x00D8);
            WriteRegister16(Register16.HL, 0x014D);
            SP = 0xFFFE;
            DivRegister = 0xABCC;
            Memory.Set(0xFF05, 0x00);
            Memory.Set(0xFF06, 0x00);
            Memory.Set(0xFF07, 0x00);
            Memory.Set(0xFF10, 0x80);
            Memory.Set(0xFF11, 0xBF);
            Memory.Set(0xFF12, 0xF3);
            Memory.Set(0xFF14, 0xBF);
            Memory.Set(0xFF16, 0x3F);
            Memory.Set(0xFF17, 0x00);
            Memory.Set(0xFF19, 0xBF);
            Memory.Set(0xFF1A, 0x7F);
            Memory.Set(0xFF1B, 0xFF);
            Memory.Set(0xFF1C, 0x9F);
            Memory.Set(0xFF1E, 0xBF);
            Memory.Set(0xFF20, 0xFF);
            Memory.Set(0xFF21, 0x00);
            Memory.Set(0xFF22, 0x00);
            Memory.Set(0xFF23, 0xBF);
            Memory.Set(0xFF24, 0x77);
            Memory.Set(0xFF25, 0xF3);
            Memory.Set(0xFF26, 0xF1);
            Memory.Set(0xFF40, 0x91);
            Memory.Set(0xFF42, 0x00);
            Memory.Set(0xFF43, 0x00);
            Memory.Set(0xFF45, 0x00);
            Memory.Set(0xFF47, 0xFC);
            Memory.Set(0xFF48, 0xFF);
            Memory.Set(0xFF49, 0xFF);
            Memory.Set(0xFF4A, 0x00);
            Memory.Set(0xFF4B, 0x00);
            Memory.Set(0xFFFF, 0x00);
        }

        public void EmulateCycles(long cycleLimit)
        {
            long cycleCounter = cycleLimit;
            PerformanceTimer.Restart();

            while (cycleCounter-- > 0)
            {
                CycleTimer.Start();
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
                    WaitForCycleFinish(CycleTimer, cycleCounter);
                    continue;
                }

                if (CheckForInterrupt())
                {
                    HandleInterrupt();
                    WaitForCycleFinish(CycleTimer, cycleCounter);
                    continue;
                }

                if (Halted)
                {
                    if(CycleCooldown == 0) CycleCooldown += 4;
                    if (CycleCooldown > 0)
                    {
                        WaitForCycleFinish(CycleTimer, cycleCounter);
                        continue;
                    }
                }

                if (EINextInstruction)
                {
                    EINextInstruction = false;
                    InterruptsEnabled = true;
                }

                Opcode opcode = Decoder.DecodeOpcode(this, Memory[PC]);
                if (logTrace) LogTrace(opcode);
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

                WaitForCycleFinish(CycleTimer, cycleCounter);
            }

            EndTime = PerformanceTimer.ElapsedTicks;
        }

        private void LogTrace(Opcode opcode)
        {
            string memSection = "";
            for(int i = 0; i < opcode.Length; i++)
            {
                memSection += Memory.Get(PC + i).ToString("X2");
            }
            logWriter.WriteLine("A: " + Registers[Register.A].ToString("X2") + " " +
                "F: " + Registers[Register.F].ToString("X2") + " " +
                "B: " + Registers[Register.B].ToString("X2") + " " +
                "C: " + Registers[Register.C].ToString("X2") + " " +
                "D: " + Registers[Register.D].ToString("X2") + " " +
                "E: " + Registers[Register.E].ToString("X2") + " " +
                "H: " + Registers[Register.H].ToString("X2") + " " +
                "L: " + Registers[Register.L].ToString("X2") + " " +
                "SP: " + SP.ToString("X4") + " " +
                "PC: 00:" + PC.ToString("X4") + " | " +
                memSection + ": " + opcode.Disassembly
            );
            logWriter.Flush();
        }

        private void WaitForCycleFinish(Stopwatch timer, long cycleCounter)
        {
            if (cycleCounter % cycleSleepInterval != 0) return;
            if (!timer.IsRunning) return;
            while (timer.ElapsedTicks < (0.99 / 4194304) * TimeSpan.TicksPerSecond * cycleSleepInterval) { /* Wait... */ }
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
                CycleCooldown = 19;
            } else CallInterruptHandler = true;
        }

        private void HandleTimers()
        {
            DivRegister++;

            int bit = 0;
            switch(Memory.Get(0xFF07) & 0b11)
            {
                case 0b00: bit = 9; break;
                case 0b01: bit = 3; break;
                case 0b10: bit = 5; break;
                case 0b11: bit = 7; break;
            }
            TimerAndResult = ((DivRegister & (1 << bit)) >> bit) != 0 && ((Memory.Get(0xFF07) & 0b100) >> 2) != 0;
            if(TimerAndResult != LastTimerAndResult)
            {
                LastTimerAndResult = TimerAndResult;
                if(!TimerAndResult)
                {
                    if (Memory.Get(0xFF05) == 0xFF)
                    {
                        Memory.Set(0xFF05, 0);
                        TimerReloadCooldown = 3;
                        Memory[0xFF0F] |= 0b100;
                    }
                    else Memory.Set(0xFF05, (byte)(Memory.Get(0xFF05) + 1));
                }
            }

            if (TimerReloadCooldown > -1 && --TimerReloadCooldown == -1) Memory.Set(0xFF05, Memory.Get(0xFF06));
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
