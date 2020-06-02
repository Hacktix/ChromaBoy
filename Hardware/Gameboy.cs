using ChromaBoy.Software;
using System;
using System.Collections.Generic;
using System.Text;
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

        public Dictionary<Register, byte> Registers = new Dictionary<Register, byte>()
        { { Register.A, 0 }, { Register.F, 2 }, { Register.B, 0 }, { Register.C, 0 }, { Register.D, 0 }, { Register.E, 0 }, { Register.H, 0 }, { Register.L, 0 }, };
        public ushort PC = 0x100;
        public ushort SP = 0;

        public bool Halted = false;
        public bool Standby = false;
        public bool InterruptsEnabled = false;
        public long CycleCount = 0;

        private int CycleCooldown = 0;

        public Gameboy(byte[] ROM)
        {
            Cartridge = new Cartridge(ROM);
            Memory = new Memory(Cartridge.MemoryBankController, ROM);

            DebugInit();
        }

        private void DebugInit()
        {
            WriteRegister16(Register16.AF, 0x01B0);
            WriteRegister16(Register16.BC, 0x0013);
            WriteRegister16(Register16.DE, 0x00D8);
            WriteRegister16(Register16.HL, 0x014D);
            SP = 0xFFFE;
        }

        public void EmulateCycles(long cycleLimit)
        {
            long cycleCounter = cycleLimit;

            while (cycleCounter-- > 0)
            {
                if(CycleCooldown > 0)
                {
                    CycleCooldown--;
                    continue;
                }

                if (Halted)
                {
                    CycleCooldown += 4;
                    continue;
                }

                Opcode opcode = Decoder.DecodeOpcode(this, Memory[PC]);
                Console.WriteLine("AF: " + ReadRegister16(Register16.AF).ToString("X4") + ", BC: " + ReadRegister16(Register16.BC).ToString("X4") + ", DE: " + ReadRegister16(Register16.DE).ToString("X4") + ", HL: " + ReadRegister16(Register16.DE).ToString("X4") + ", SP: " + SP.ToString("X4") + ", PC: " + PC.ToString("X4") + " (" + Memory[PC].ToString("X2") + " " + Memory[PC + 1].ToString("X2") + " " + Memory[PC + 2].ToString("X2") + " " + Memory[PC + 3].ToString("X2") + ") " + opcode);
                opcode.Execute();
                PC += (ushort)opcode.Length;
                CycleCooldown = opcode.Cycles - 1;

                CycleCount++;
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
                    Registers[Register.F] = (byte)(value & 0xFF);
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
    }
}
