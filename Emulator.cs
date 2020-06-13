using Chroma;
using Chroma.Graphics;
using ChromaBoy.Hardware;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChromaBoy
{
    class Emulator : Game
    {
        public static readonly int SCREEN_WIDTH = 160;
        public static readonly int SCREEN_HEIGHT = 144;
        public static readonly int SCALE_FACTOR = 4;
        public static readonly Vector2 SCALE_VECTOR = new Vector2(SCALE_FACTOR, SCALE_FACTOR);

        public static readonly int CYCLES_PER_UPDATE = 3000000;
        public static readonly int UPDATE_FREQUENCY = 1000 / (4194304 / CYCLES_PER_UPDATE);

        private static readonly int PERFORMANCE_BUFFER_LENGTH = 100;
        private List<double> PerformanceBuffer = new List<double>();

        public static Dictionary<byte, Color> ShadeColorMap = new Dictionary<byte, Color>()
        {
            { 1, new Color(155, 188, 15) },
            { 2, new Color(139, 172, 15) },
            { 3, new Color(48, 98, 48) },
            { 4, new Color(15, 56, 15) },
            { 0, new Color(202, 220, 159) }
        };

        private Gameboy Gameboy;
        private RenderTarget Frame;

        public Emulator()
        {
            Graphics.VSyncEnabled = false;
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
                // Performance Calculation
                double percent = (CYCLES_PER_UPDATE * (1.0 / 4194304.0) * TimeSpan.TicksPerSecond) / Gameboy.EndTime;
                PerformanceBuffer.Add(percent);
                if (PerformanceBuffer.Count > PERFORMANCE_BUFFER_LENGTH) PerformanceBuffer.RemoveAt(0);

                percent = 0;
                foreach (double value in PerformanceBuffer) percent += value;
                percent /= PerformanceBuffer.Count;
                percent = ((int)(percent * 10000)) / 100.0;
                Window.Properties.Title = "ChromaBoy (" + Window.FPS + " FPS) [" + percent + "%] : " + Gameboy.Cartridge.Title;
            }
        }

        protected override void Draw(RenderContext context)
        {
            if(PPU.CanDraw && PPU.HasUpdated)
            {
                PPU.HasUpdated = false;
                context.RenderTo(Frame, () =>
                {
                    for (int x = 0; x < SCREEN_WIDTH; x++)
                    {
                        for (int y = 0; y < SCREEN_HEIGHT; y++)
                        {
                            context.Rectangle(ShapeMode.Fill, new Vector2(x, y), 1f, 1f, ShadeColorMap[PPU.Display[x, y]]);
                        }
                    }
                });
            }
            
            context.DrawTexture(Frame, Vector2.Zero, SCALE_VECTOR, Vector2.Zero, 0f);
        }
    }
}
