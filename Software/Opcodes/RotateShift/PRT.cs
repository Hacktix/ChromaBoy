using ChromaBoy.Hardware;

namespace ChromaBoy.Software.Opcodes
{
    public class PRT : Opcode // RLC r | RRC r | RL r | RR r
    {
        private bool left;
        private bool useCarry;
        private Register target;

        public PRT(Gameboy parent, byte opcode) : base(parent) {
            left = (opcode & 0b1000) == 0;
            useCarry = (opcode & 0b10000) > 0;
            target = OpcodeUtils.BitsToRegister(opcode & 0b111);

            Length = 2;
            Cycles = (target == Register.M) ? 16 : 8;
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
    }
}
