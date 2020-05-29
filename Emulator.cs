using Chroma;

namespace ChromaBoy
{
    class Emulator : Game
    {
        public static readonly int SCREEN_WIDTH = 160;
        public static readonly int SCREEN_HEIGHT = 144;
        public static readonly int SCALE_FACTOR = 4;

        public Emulator()
        {
            Window.GoWindowed((ushort)(SCREEN_WIDTH * SCALE_FACTOR), (ushort)(SCREEN_HEIGHT * SCALE_FACTOR));
        }

        public Emulator(byte[] ROM) : this()
        {
            // TODO: Implement automatic ROM Loading
        }
    }
}
