using Chroma;
using Chroma.Audio;
using Chroma.Graphics;
using Chroma.Input.EventArgs;
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

        public static readonly int CYCLES_PER_UPDATE = 3500000;
        public static readonly int UPDATE_FREQUENCY = 1000 / (4194304 / CYCLES_PER_UPDATE);

        private static readonly int PERFORMANCE_BUFFER_LENGTH = 100;
        private List<double> PerformanceBuffer = new List<double>();

        public static AudioManager AudioManager;

        public static Dictionary<byte, Color> ShadeColorMap = new Dictionary<byte, Color>()
        {
            { 1, new Color(155, 188, 15) },
            { 2, new Color(139, 172, 15) },
            { 3, new Color(48, 98, 48) },
            { 4, new Color(15, 56, 15) },
            { 0, new Color(202, 220, 159) }
        };

        public static byte InputBits = 0xFF;

        private Gameboy Gameboy;
        private RenderTarget Frame;

        public Emulator()
        {
            Graphics.VSyncEnabled = false;
            Window.GoWindowed((ushort)(SCREEN_WIDTH * SCALE_FACTOR), (ushort)(SCREEN_HEIGHT * SCALE_FACTOR));
            Frame = new RenderTarget((ushort)SCREEN_WIDTH, (ushort)SCREEN_HEIGHT);
            Frame.FilteringMode = TextureFilteringMode.NearestNeighbor;
            AudioManager = Audio;
            Window.QuitRequested += OnQuitRequested;
        }

        private void OnQuitRequested(object sender, Chroma.Windowing.EventArgs.CancelEventArgs e)
        {
            Gameboy.Memory.MBC.SaveExternalRam();
            Environment.Exit(0);
        }

        public Emulator(byte[] ROM) : this()
        {
            Gameboy = new Gameboy(ROM);
            Audio.HookPostMixProcessor<float>((chunk, bytes) =>
            {
                if(Gameboy != null && Gameboy.APU != null)
                {
                    Gameboy.APU.MixAudio(ref chunk);
                    for (int i = 0; i < chunk.Length; i++)
                        chunk[i] = (float)(chunk[i] * 0.1);
                }
            });
            FixedUpdateFrequency = UPDATE_FREQUENCY;
        }

        protected override void FixedUpdate(float fixedDelta)
        {
            if(Gameboy != null)
            {
                Gameboy.EmulateCycles(CYCLES_PER_UPDATE);
                UpdateWindowTitle();
            }
        }

        protected override void KeyPressed(KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Chroma.Input.KeyCode.Right:
                    InputBits &= 0b11111110;
                    if ((Gameboy.Memory.RAM[0xFF00] & 0b10000) == 0) Gameboy.Memory[0xFFFF] |= 0b10000;
                    break;
                case Chroma.Input.KeyCode.Left:
                    InputBits &= 0b11111101;
                    if ((Gameboy.Memory.RAM[0xFF00] & 0b10000) == 0) Gameboy.Memory[0xFFFF] |= 0b10000;
                    break;
                case Chroma.Input.KeyCode.Up:
                    InputBits &= 0b11111011;
                    if ((Gameboy.Memory.RAM[0xFF00] & 0b10000) == 0) Gameboy.Memory[0xFFFF] |= 0b10000;
                    break;
                case Chroma.Input.KeyCode.Down:
                    InputBits &= 0b11110111;
                    if ((Gameboy.Memory.RAM[0xFF00] & 0b10000) == 0) Gameboy.Memory[0xFFFF] |= 0b10000;
                    break;
                case Chroma.Input.KeyCode.C:
                    InputBits &= 0b11101111;
                    if ((Gameboy.Memory.RAM[0xFF00] & 0b100000) == 0) Gameboy.Memory[0xFFFF] |= 0b10000;
                    break;
                case Chroma.Input.KeyCode.X:
                    InputBits &= 0b11011111;
                    if ((Gameboy.Memory.RAM[0xFF00] & 0b100000) == 0) Gameboy.Memory[0xFFFF] |= 0b10000;
                    break;
                case Chroma.Input.KeyCode.RightShift: 
                    InputBits &= 0b10111111;
                    if ((Gameboy.Memory.RAM[0xFF00] & 0b100000) == 0) Gameboy.Memory[0xFFFF] |= 0b10000;
                    break;
                case Chroma.Input.KeyCode.Return:
                    InputBits &= 0b01111111;
                    if ((Gameboy.Memory.RAM[0xFF00] & 0b100000) == 0) Gameboy.Memory[0xFFFF] |= 0b10000;
                    break;
            }
        }

        protected override void KeyReleased(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Chroma.Input.KeyCode.Right:      InputBits |= 0b00000001; break;
                case Chroma.Input.KeyCode.Left:       InputBits |= 0b00000010; break;
                case Chroma.Input.KeyCode.Up:         InputBits |= 0b00000100; break;
                case Chroma.Input.KeyCode.Down:       InputBits |= 0b00001000; break;
                case Chroma.Input.KeyCode.C:          InputBits |= 0b00010000; break;
                case Chroma.Input.KeyCode.X:          InputBits |= 0b00100000; break;
                case Chroma.Input.KeyCode.RightShift: InputBits |= 0b01000000; break;
                case Chroma.Input.KeyCode.Return:     InputBits |= 0b10000000; break;
            }
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
            if (Gameboy != null)
            {
                if (Gameboy.PPU.HasUpdated)
                {
                    Gameboy.PPU.HasUpdated = false;
                    context.RenderTo(Frame, () =>
                    {
                        for (int x = 0; x < SCREEN_WIDTH; x++)
                        {
                            for (int y = 0; y < SCREEN_HEIGHT; y++)
                            {
                                context.Rectangle(ShapeMode.Fill, new Vector2(x, y), 1f, 1f, ShadeColorMap[Gameboy.PPU.LCDBuffer[x, y]]);
                            }
                        }
                    });
                }
            }
            context.DrawTexture(Frame, Vector2.Zero, SCALE_VECTOR, Vector2.Zero, 0f);
        }
    }
}
