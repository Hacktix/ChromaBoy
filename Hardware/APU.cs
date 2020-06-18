using ChromaSynth;
using System;

namespace ChromaBoy.Hardware
{
    public class APU
    {
        private Gameboy parent;

        private SquareWave ch1 = new SquareWave(Emulator.AudioManager, 0f, 0f);
        private SquareWave ch2 = new SquareWave(Emulator.AudioManager, 0f, 0f);

        public APU(Gameboy parent)
        {
            this.parent = parent;
        }

        public void ProcessCycle()
        {
            // Update Channels
            UpdateChannel1();
            UpdateChannel2();
        }

        private void UpdateChannel1()
        {
            // Set Volume
            byte nr12 = parent.Memory[0xFF12];
            ch1.Volume = (float)(((nr12 & 0xF0) >> 4) / 16.0);

            // Set Frequency
            ushort nr13 = (ushort)(parent.Memory[0xFF13] | ((parent.Memory[0xFF14] & 0b111) << 8));
            ch1.Frequency = (float)(131072.0 / (2048 - nr13)) * 2.0f;
        }

        private void UpdateChannel2()
        {
            // Set Volume
            byte nr22 = parent.Memory[0xFF17];
            ch2.Volume = (float)(((nr22 & 0xF0) >> 4) / 16.0);

            // Set Frequency
            ushort nr23 = (ushort)(parent.Memory[0xFF18] | ((parent.Memory[0xFF19] & 0b111) << 8));
            ch2.Frequency = (float)(131072.0 / (2048 - nr23)) * 2.0f;
        }

        public void MixAudio(ref Span<float> chunk)
        {
            ch1.GenerateChunk(ref chunk);
            ch2.GenerateChunk(ref chunk);
        }
    }
}
