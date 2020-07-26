using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class PRT : Opcode // RLC r | RRC r | RL r | RR r
    {
        private bool left;
        private bool useCarry;
        private Register target;

        private byte readValue;

        public PRT(Gameboy parent, byte opcode) : base(parent) {
            left = (opcode & 0b1000) == 0;
            useCarry = (opcode & 0b10000) > 0;
            target = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Length = 2;
            Cycles = (target == Register.M) ? 16 : 8;
            TickAccurate = target == Register.M;

            Disassembly = (left ? "rl" : "rr") + (useCarry ? "c" : "") + " " + OpcodeUtils.RegisterToString(target);
        }

        public override void Execute()
        {
            if(left)
            {
                if(useCarry)
                {
                    byte rVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
                    byte carryAdd = (byte)((parent.Registers[Register.F] & 0b10000) >> 4);
                    parent.SetFlag(Flag.Carry, (rVal & 128) > 0);
                    if(target == Register.M)
                        parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((rVal << 1) + carryAdd);
                    else
                        parent.Registers[target] = (byte)((rVal << 1) + carryAdd);
                } else {
                    byte rVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
                    byte carryAdd = (byte)((rVal & 128) >> 7);
                    parent.SetFlag(Flag.Carry, (rVal & 128) > 0);
                    if(target == Register.M)
                        parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((rVal << 1) + carryAdd);
                    else
                        parent.Registers[target] = (byte)((rVal << 1) + carryAdd);
                }
            } else {
                if (useCarry)
                {
                    byte rVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
                    byte carryAdd = (byte)((parent.Registers[Register.F] & 0b10000) << 3);
                    parent.SetFlag(Flag.Carry, (rVal & 1) > 0);
                    if (target == Register.M)
                        parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((rVal >> 1) + carryAdd);
                    else
                        parent.Registers[target] = (byte)((rVal >> 1) + carryAdd);
                }
                else
                {
                    byte rVal = (target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target];
                    byte carryAdd = (byte)((rVal & 1) << 7);
                    parent.SetFlag(Flag.Carry, (rVal & 1) > 0);
                    if (target == Register.M)
                        parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((rVal >> 1) + carryAdd);
                    else
                        parent.Registers[target] = (byte)((rVal >> 1) + carryAdd);
                }
            }

            // Set Flags
            parent.SetFlag(Flag.Zero, ((target == Register.M) ? parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] : parent.Registers[target]) == 0);
            parent.SetFlag(Flag.AddSub, false);
            parent.SetFlag(Flag.HalfCarry, false);
        }

        public override void ExecuteTick()
        {
            base.ExecuteTick();
            if(Tick == 7)
            {
                readValue = parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])];
            } else if(Tick == 11)
            {
                if (left)
                {
                    if (useCarry)
                    {
                        byte carryAdd = (byte)((parent.Registers[Register.F] & 0b10000) >> 4);
                        parent.SetFlag(Flag.Carry, (readValue & 128) > 0);
                        if (target == Register.M)
                            parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((readValue << 1) + carryAdd);
                        else
                            parent.Registers[target] = (byte)((readValue << 1) + carryAdd);
                    }
                    else
                    {
                        byte carryAdd = (byte)((readValue & 128) >> 7);
                        parent.SetFlag(Flag.Carry, (readValue & 128) > 0);
                        if (target == Register.M)
                            parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((readValue << 1) + carryAdd);
                        else
                            parent.Registers[target] = (byte)((readValue << 1) + carryAdd);
                    }
                }
                else
                {
                    if (useCarry)
                    {
                        byte carryAdd = (byte)((parent.Registers[Register.F] & 0b10000) << 3);
                        parent.SetFlag(Flag.Carry, (readValue & 1) > 0);
                        if (target == Register.M)
                            parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((readValue >> 1) + carryAdd);
                        else
                            parent.Registers[target] = (byte)((readValue >> 1) + carryAdd);
                    }
                    else
                    {
                        byte carryAdd = (byte)((readValue & 1) << 7);
                        parent.SetFlag(Flag.Carry, (readValue & 1) > 0);
                        if (target == Register.M)
                            parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] = (byte)((readValue >> 1) + carryAdd);
                        else
                            parent.Registers[target] = (byte)((readValue >> 1) + carryAdd);
                    }
                }

                // Set Flags
                parent.SetFlag(Flag.Zero, parent.Memory[(parent.Registers[Register.H] << 8) | (parent.Registers[Register.L])] == 0);
                parent.SetFlag(Flag.AddSub, false);
                parent.SetFlag(Flag.HalfCarry, false);
            }
        }
    }
}
