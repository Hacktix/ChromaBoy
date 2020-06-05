using Chroma;
using Chroma.Graphics;
using ChromaBoy.Hardware;
using System;
using System.Numerics;

namespace ChromaBoy
{
    class Emulator : Game
    {
        public static readonly int SCREEN_WIDTH = 160;
        public static readonly int SCREEN_HEIGHT = 144;
        public static readonly int SCALE_FACTOR = 4;
        public static readonly Vector2 SCALE_VECTOR = new Vector2(SCALE_FACTOR, SCALE_FACTOR);

        public static readonly int CYCLES_PER_UPDATE = 1000000;
        public static readonly int UPDATE_FREQUENCY = 1000 / (4194304 / CYCLES_PER_UPDATE);

        private Gameboy Gameboy;
        private RenderTarget Frame;

        public Emulator()
        {
            Window.GoWindowed((ushort)(SCREEN_WIDTH * SCALE_FACTOR), (ushort)(SCREEN_HEIGHT * SCALE_FACTOR));
            Frame = new RenderTarget((ushort)SCREEN_WIDTH, (ushort)SCREEN_HEIGHT);
            Frame.FilteringMode = TextureFilteringMode.NearestNeighbor;
        }

        public Emulator(byte[] ROM) : this()
        {
            Gameboy = new Gameboy(ROM);
            FixedUpdateFrequency = UPDATE_FREQUENCY;
        }

        protected override void FixedUpdate(float fixedDelta)
        {
            Gameboy.EmulateCycles(CYCLES_PER_UPDATE);
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            if(Gameboy != null)
            {
                Window.Properties.Title = "ChromaBoy - " + Gameboy.Cartridge.Title;
            }
        }

        protected override void Draw(RenderContext context)
        {
            context.RenderTo(Frame, () =>
            {
                for (int x = 0; x < SCREEN_WIDTH; x++)
                {
                    for (int y = 0; y < SCREEN_HEIGHT; y++)
                    {
                        // TODO: Draw correct colors
                        context.Rectangle(ShapeMode.Fill, new Vector2(x, y), 1f, 1f, PPU.Display[x, y] == 0 ? Color.White : Color.Black);
                    }
                }
            });
            
            context.DrawTexture(Frame, Vector2.Zero, SCALE_VECTOR, Vector2.Zero, 0f);
        }
    }
}
