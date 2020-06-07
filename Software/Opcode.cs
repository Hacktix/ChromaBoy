using ChromaBoy.Hardware;

namespace ChromaBoy.Software
{
    public abstract class Opcode
    {
        public int Length = 1;
        public int Cycles = 4;
        public bool TickAccurate = false;
        public byte Tick = 0;

        protected Gameboy parent;

        public Opcode(Gameboy parent) { this.parent = parent; }

        public abstract void Execute();
        public virtual void ExecuteTick()
        {
            Tick++;
        }
    }
}
