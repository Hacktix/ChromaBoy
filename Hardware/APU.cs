using ChromaSynth;
using System;

namespace ChromaBoy.Hardware
{
    public class APU
    {
        private Gameboy parent;

        private SquareWave ch1 = new SquareWave(Emulator.AudioManager, 0f, 0f);
        private SquareWave ch2 = new SquareWave(Emulator.AudioManager, 0f, 0f);

        private int ch1Len = 16384;
        private int ch2Len = 16384;

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

            // Take care of sound length
            if((parent.Memory.Get(0xFF14) & 0b1000000) != 0)
            {
                byte nr11 = parent.Memory.Get(0xFF11);
                if ((nr11 & 0x3F) == 0) ch1.Volume = 0;
                else if (ch1Len > 0) ch1Len--;
                else
                {
                    parent.Memory[0xFF11] = (byte)(nr11 - 1);
                    ch1Len = 16384;
                }
            }

            // Set Frequency
            ushort nr13 = (ushort)(parent.Memory[0xFF13] | ((parent.Memory[0xFF14] & 0b111) << 8));
            ch1.Frequency = (float)(131072.0 / (2048 - nr13)) * 2.0f;
        }

        private void UpdateChannel2()
        {
            // Set Volume
            byte nr22 = parent.Memory[0xFF17];
            ch2.Volume = (float)(((nr22 & 0xF0) >> 4) / 16.0);

            // Take care of sound length
            if ((parent.Memory.Get(0xFF19) & 0b1000000) != 0)
            {
                byte nr21 = parent.Memory.Get(0xFF16);
                if ((nr21 & 0x3F) == 0) ch2.Volume = 0;
                else if (ch2Len > 0) ch2Len--;
                else
                {
                    parent.Memory[0xFF16] = (byte)(nr21 - 1);
                    ch2Len = 16384;
                }
            }

            // Set Frequency
            ushort nr23 = (ushort)(parent.Memory[0xFF18] | ((parent.Memory[0xFF19] & 0b111) << 8));
            ch2.Frequency = (float)(131072.0 / (2048 - nr23)) * 2.0f;
        }

        public void MixAudio(ref Span<float> chunk)
        {
            if(ch1.Volume > 0)
                ch1.GenerateChunk(ref chunk);
            if(ch2.Volume > 0)
                ch2.GenerateChunk(ref chunk);
        }
    }
}
