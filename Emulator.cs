using Chroma;
using ChromaBoy.Hardware;

namespace ChromaBoy
{
    class Emulator : Game
    {
        public static readonly int SCREEN_WIDTH = 160;
        public static readonly int SCREEN_HEIGHT = 144;
        public static readonly int SCALE_FACTOR = 4;

        public static readonly int CYCLES_PER_UPDATE = 1000000;
        public static readonly int UPDATE_FREQUENCY = 1000 / (4194304 / CYCLES_PER_UPDATE);

        private Gameboy Gameboy;

        public Emulator()
        {
            Window.GoWindowed((ushort)(SCREEN_WIDTH * SCALE_FACTOR), (ushort)(SCREEN_HEIGHT * SCALE_FACTOR));
        }

        public Emulator(byte[] ROM) : this()
        {
            Gameboy = new Gameboy(ROM);
            FixedUpdateFrequency = UPDATE_FREQUENCY;
        }

        protected override void FixedUpdate(float fixedDelta)
        {
            Gameboy.EmulateCycles(CYCLES_PER_UPDATE);
        }
    }
}
