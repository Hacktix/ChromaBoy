using ChromaBoy.Hardware;

namespace ChromaBoy.Software
{
    public abstract class Opcode
    {
        public int Length = 1;
        public int Cycles = 4;
        public bool TickAccurate = false;
        public byte Tick = 0;
        public string Disassembly = "No disassembly available";

        protected Gameboy parent;

        public Opcode(Gameboy parent) { this.parent = parent; }

        public virtual void Execute() { }
        public virtual void ExecuteTick()
        {
            Tick++;
        }
    }
}
