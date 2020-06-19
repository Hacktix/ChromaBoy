using ChromaSynth;
using System;

namespace ChromaBoy.Hardware
{
    public class APU
    {
        private Gameboy parent;

        private PulseWave ch1 = new PulseWave(Emulator.AudioManager, 0f, 0f);
        private PulseWave ch2 = new PulseWave(Emulator.AudioManager, 0f, 0f);

        private byte ch1Len = 0;
        private byte ch2Len = 0;

        private long apuCycles = 0;

        public APU(Gameboy parent)
        {
            this.parent = parent;
        }

        public void ProcessCycle()
        {
            // Update Channels
            if(parent.Memory.UpdateAudioChannel1) UpdateChannel1();
            if(parent.Memory.UpdateAudioChannel2) UpdateChannel2();

            if (++apuCycles == 65536) apuCycles = 0;
        }

        private void UpdateChannel1()
        {
            // Reset update flag
            parent.Memory.UpdateAudioChannel1 = false;

            // Set Volume
            byte nr12 = parent.Memory[0xFF12];
            ch1.Volume = (float)(((nr12 & 0xF0) >> 4) / 16.0);

            // Set Duty
            ch1.Duty = 0.125f;
            switch((parent.Memory[0xFF11] & 0b11000000) >> 6)
            {
                case 0b01: ch1.Duty = 0.25f; break;
                case 0b10: ch1.Duty = 0.5f; break;
                case 0b11: ch1.Duty = 0.75f; break;
            }

            // Set Frequency
            ushort nr13 = (ushort)(parent.Memory[0xFF13] | ((parent.Memory[0xFF14] & 0b111) << 8));
            ch1.Frequency = (float)(131072.0 / (2048 - nr13)) * 2.0f;

            // Handle Triggers
            if (parent.Memory.TriggerAudio1)
            {
                parent.Memory.TriggerAudio1 = false;
                if (ch1Len == 0) ch1Len = 64;
                ch1.Volume = (float)(((parent.Memory[0xFF12] & 0xF0) >> 4) / 16.0);
            }

            // Update Sound Length
            if (parent.Memory.UpdateAudioLength1)
            {
                parent.Memory.UpdateAudioLength1 = false;
                ch1Len = (byte)(64 - (parent.Memory[0xFF11] & 0x3F));
            }
            if ((parent.Memory.Get(0xFF14) & 0b1000000) != 0)
            {
                if (apuCycles % 16384 == 0 && ch1Len > 0)
                {
                    ch1Len--;
                }
                if (ch1.Volume != 0 && ch1Len == 0) ch1.Volume = 0;
            }
        }

        private void UpdateChannel2()
        {
            // Reset update flag
            parent.Memory.UpdateAudioChannel2 = false;

            // Set Volume
            byte nr22 = parent.Memory[0xFF17];
            ch2.Volume = (float)(((nr22 & 0xF0) >> 4) / 16.0);

            // Set Duty
            ch2.Duty = 0.125f;
            switch ((parent.Memory[0xFF16] & 0b11000000) >> 6)
            {
                case 0b01: ch2.Duty = 0.25f; break;
                case 0b10: ch2.Duty = 0.5f; break;
                case 0b11: ch2.Duty = 0.75f; break;
            }

            // Set Frequency
            ushort nr23 = (ushort)(parent.Memory[0xFF18] | ((parent.Memory[0xFF19] & 0b111) << 8));
            ch2.Frequency = (float)(131072.0 / (2048 - nr23)) * 2.0f;

            // Handle Triggers
            if (parent.Memory.TriggerAudio2)
            {
                parent.Memory.TriggerAudio2 = false;
                if (ch2Len == 0) ch2Len = 64;
                ch2.Volume = (float)(((parent.Memory[0xFF17] & 0xF0) >> 4) / 16.0);
            }

            // Update Sound Length
            if (parent.Memory.UpdateAudioLength1)
            {
                parent.Memory.UpdateAudioLength2 = false;
                ch2Len = (byte)(64 - (parent.Memory[0xFF16] & 0x3F));
            }
            if ((parent.Memory.Get(0xFF19) & 0b1000000) != 0)
            {
                if (apuCycles % 16384 == 0 && ch2Len > 0)
                {
                    ch2Len--;
                }
                if (ch2.Volume != 0 && ch2Len == 0) ch2.Volume = 0;
            }
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
